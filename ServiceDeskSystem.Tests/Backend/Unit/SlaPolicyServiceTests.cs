using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Sla;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class SlaPolicyServiceTests
{
    private Mock<IRepositoryFacadeFactory> _mockFactory;
    private Mock<IRepositoryFacade> _mockRepo;
    private Mock<ISlaPolicyRepository> _mockPolicyRepo;
    private Mock<ITicketRepository> _mockTicketRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IRealtimeNotifier> _mockRealtimeNotifier;
    private Mock<IAuditService> _mockAuditService;
    private SlaPolicyService _service;

    [SetUp]
    public void SetUp()
    {
        _mockFactory = new Mock<IRepositoryFacadeFactory>();
        _mockRepo = new Mock<IRepositoryFacade>();
        _mockPolicyRepo = new Mock<ISlaPolicyRepository>();
        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRealtimeNotifier = new Mock<IRealtimeNotifier>();
        _mockAuditService = new Mock<IAuditService>();

        _mockRepo.Setup(r => r.SlaPolicies).Returns(_mockPolicyRepo.Object);
        _mockRepo.Setup(r => r.Tickets).Returns(_mockTicketRepo.Object);
        _mockRepo.Setup(r => r.UnitOfWork).Returns(_mockUnitOfWork.Object);
        _mockFactory.Setup(f => f.Create()).Returns(_mockRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockRealtimeNotifier.Setup(n => n.NotifyTicketsChangedAsync()).Returns(Task.CompletedTask);

        _service = new SlaPolicyService(_mockFactory.Object, _mockRealtimeNotifier.Object, _mockAuditService.Object);
    }

    [Test]
    public async Task ResolvePolicyAsync_PrefersProductSpecificOverGlobal()
    {
        var global = new SlaPolicy { Id = 1, Priority = TicketPriority.High, ProductId = null, IsActive = true, ResolutionMinutes = 2880, ResponseMinutes = 240 };
        var productSpecific = new SlaPolicy { Id = 2, Priority = TicketPriority.High, ProductId = 7, IsActive = true, ResolutionMinutes = 480, ResponseMinutes = 60 };

        _mockPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(TicketPriority.High))
            .ReturnsAsync(new List<SlaPolicy> { global, productSpecific });

        var result = await _service.ResolvePolicyAsync(TicketPriority.High, productId: 7);

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    [Test]
    public async Task ResolvePolicyAsync_FallsBackToGlobalWhenProductDoesNotMatch()
    {
        var global = new SlaPolicy { Id = 1, Priority = TicketPriority.High, ProductId = null, IsActive = true, ResolutionMinutes = 2880, ResponseMinutes = 240 };
        var otherProduct = new SlaPolicy { Id = 2, Priority = TicketPriority.High, ProductId = 7, IsActive = true, ResolutionMinutes = 480, ResponseMinutes = 60 };

        _mockPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(TicketPriority.High))
            .ReturnsAsync(new List<SlaPolicy> { global, otherProduct });

        var result = await _service.ResolvePolicyAsync(TicketPriority.High, productId: 99);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Test]
    public async Task ResolvePolicyAsync_IgnoresInactivePolicies()
    {
        var inactive = new SlaPolicy { Id = 1, Priority = TicketPriority.Low, ProductId = null, IsActive = false, ResolutionMinutes = 100, ResponseMinutes = 10 };

        _mockPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(TicketPriority.Low))
            .ReturnsAsync(new List<SlaPolicy> { inactive });

        var result = await _service.ResolvePolicyAsync(TicketPriority.Low, productId: null);

        result.Should().BeNull();
    }

    [Test]
    public async Task ApplyPolicyAsync_SetsDeadlinesFromPolicy()
    {
        var createdAt = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
        var policy = new SlaPolicy { Id = 3, Priority = TicketPriority.Critical, IsActive = true, ResponseMinutes = 60, ResolutionMinutes = 240 };

        _mockPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(TicketPriority.Critical))
            .ReturnsAsync(new List<SlaPolicy> { policy });

        var ticket = new Ticket { Id = 5, Priority = TicketPriority.Critical, CreatedAt = createdAt };

        var applied = await _service.ApplyPolicyAsync(ticket);

        applied.Should().BeTrue();
        ticket.SlaPolicyId.Should().Be(3);
        ticket.ResponseDueAt.Should().Be(createdAt.AddMinutes(60));
        ticket.DueDate.Should().Be(createdAt.AddMinutes(240));
    }

    [Test]
    public async Task ApplyPolicyAsync_KeepsManualDueDateUnlessOverrideRequested()
    {
        var createdAt = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
        var manualDueDate = createdAt.AddDays(10);
        var policy = new SlaPolicy { Id = 3, Priority = TicketPriority.Medium, IsActive = true, ResponseMinutes = 60, ResolutionMinutes = 240 };

        _mockPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(TicketPriority.Medium))
            .ReturnsAsync(new List<SlaPolicy> { policy });

        var ticket = new Ticket { Id = 5, Priority = TicketPriority.Medium, CreatedAt = createdAt, DueDate = manualDueDate };

        await _service.ApplyPolicyAsync(ticket);

        ticket.DueDate.Should().Be(manualDueDate);

        await _service.ApplyPolicyAsync(ticket, overrideExistingDueDate: true);

        ticket.DueDate.Should().Be(createdAt.AddMinutes(240));
    }

    [Test]
    public async Task ApplyPolicyAsync_ReturnsFalseWhenNoPolicyMatches()
    {
        _mockPolicyRepo.Setup(r => r.GetActiveForPriorityAsync(It.IsAny<TicketPriority>()))
            .ReturnsAsync(new List<SlaPolicy>());

        var ticket = new Ticket { Id = 5, Priority = TicketPriority.Low, CreatedAt = DateTime.UtcNow };

        var applied = await _service.ApplyPolicyAsync(ticket);

        applied.Should().BeFalse();
        ticket.SlaPolicyId.Should().BeNull();
        ticket.DueDate.Should().BeNull();
    }

    [Test]
    public void CreatePolicyAsync_RejectsResponseLongerThanResolution()
    {
        var policy = new SlaPolicy { Name = "Bad", Priority = TicketPriority.High, ResponseMinutes = 500, ResolutionMinutes = 100 };

        var act = async () => await _service.CreatePolicyAsync(policy);

        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task CreatePolicyAsync_RejectsDuplicateScope()
    {
        _mockPolicyRepo.Setup(r => r.ExistsForScopeAsync(TicketPriority.High, null, null)).ReturnsAsync(true);

        var policy = new SlaPolicy { Name = "Duplicate", Priority = TicketPriority.High, ResponseMinutes = 60, ResolutionMinutes = 240 };

        await FluentActions.Awaiting(() => _service.CreatePolicyAsync(policy))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task CreatePolicyAsync_ClampsWarningThreshold()
    {
        _mockPolicyRepo.Setup(r => r.ExistsForScopeAsync(It.IsAny<TicketPriority>(), It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(false);
        _mockPolicyRepo.Setup(r => r.CreateAsync(It.IsAny<SlaPolicy>())).Returns(Task.CompletedTask);

        var policy = new SlaPolicy { Name = "Clamped", Priority = TicketPriority.Low, ResponseMinutes = 60, ResolutionMinutes = 240, WarningThresholdPercent = 250 };

        var created = await _service.CreatePolicyAsync(policy);

        created.WarningThresholdPercent.Should().Be(99);
    }

    [Test]
    public async Task ReapplyToOpenTicketsAsync_UpdatesMatchingTicketsOnly()
    {
        var policy = new SlaPolicy { Id = 1, Priority = TicketPriority.High, ProductId = null, IsActive = true, ResponseMinutes = 60, ResolutionMinutes = 240 };
        _mockPolicyRepo.Setup(r => r.GetAllWithProductAsync()).ReturnsAsync(new List<SlaPolicy> { policy });

        var matching = new Ticket { Id = 1, Priority = TicketPriority.High, CreatedAt = DateTime.UtcNow.AddHours(-1) };
        var unmatched = new Ticket { Id = 2, Priority = TicketPriority.Low, CreatedAt = DateTime.UtcNow.AddHours(-1) };

        _mockTicketRepo.Setup(r => r.GetActiveTicketsForSlaAsync())
            .ReturnsAsync(new List<Ticket> { matching, unmatched });

        var updated = await _service.ReapplyToOpenTicketsAsync();

        updated.Should().Be(1);
        matching.SlaPolicyId.Should().Be(1);
        unmatched.SlaPolicyId.Should().BeNull();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    [TestCase(0, "0m")]
    [TestCase(45, "45m")]
    [TestCase(240, "4h")]
    [TestCase(1440, "1d")]
    [TestCase(2880, "2d")]
    [TestCase(1500, "1d 1h")]
    public void FormatMinutes_RendersCompactDuration(int minutes, string expected)
    {
        SlaPolicy.FormatMinutes(minutes).Should().Be(expected);
    }
}
