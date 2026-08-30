using FluentAssertions;
using Moq;
using ServiceDeskSystemApp.Models.Common;
using ServiceDeskSystemApp.Models.Tickets;
using ServiceDeskSystemApp.Services;
using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Tests.ViewModels;

public class TicketsViewModelTests
{
    private readonly Mock<ITicketService> _ticketServiceMock;
    private readonly TicketsViewModel _sut;

    public TicketsViewModelTests()
    {
        _ticketServiceMock = new Mock<ITicketService>();
        _sut = new TicketsViewModel(_ticketServiceMock.Object);
    }

    [Fact]
    public async Task LoadTicketsCommand_FetchesTicketsAndStats()
    {
        // Arrange
        var stats = new TicketStatsDto { Total = 10, Open = 5, Critical = 2 };
        var ticketsPage = new PagedResult<TicketDto> 
        { 
            Items = new List<TicketDto> { new TicketDto { Id = 1, Title = "Test Ticket" } },
            TotalCount = 1
        };

        _ticketServiceMock.Setup(s => s.GetStatsAsync()).ReturnsAsync(stats);
        _ticketServiceMock.Setup(s => s.GetTicketsAsync(1, 10)).ReturnsAsync(ticketsPage);

        // Act
        await _sut.LoadTicketsCommand.ExecuteAsync(null);

        // Assert
        _sut.Tickets.Should().HaveCount(1);
        _sut.Tickets[0].Title.Should().Be("Test Ticket");
        _sut.StatsTotal.Should().Be(10);
        _sut.StatsOpen.Should().Be(5);
        _sut.StatsCritical.Should().Be(2);
        _sut.IsRefreshing.Should().BeFalse();
    }
}
