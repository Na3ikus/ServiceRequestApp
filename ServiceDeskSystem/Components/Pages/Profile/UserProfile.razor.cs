using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.WorkLogs;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Components.UI.Elements;
using ServiceDeskSystem.Components.UI.Primitives;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Profile;

/// <summary>
/// User profile page: identity card, figures, activity map, recent tickets and the edit form.
/// </summary>
public partial class UserProfile : BaseComponent
{
    private static readonly IReadOnlyList<PathSegment> Segments = [new ("profile")];

    private bool isLoading = true;
    private bool isSaving;
    private bool isSaved;
    private string? loadError;
    private string? errorMessage;
    private string? warningMessage;
    private UpdateProfileRequest? model;
    private List<ContactTypeDto> contactTypes = new List<ContactTypeDto>();
    private User? user;
    private IReadOnlyList<(string LabelKey, string Value)> numbers = [];
    private IReadOnlyList<DailyActivityDto>? activity;
    private List<Ticket>? recentTickets;
    private DateOnly today = DateOnly.FromDateTime(DateTime.Now);

    [Inject]
    public IProfileService ProfileService { get; set; } = null!;

    [Inject]
    public IUserActivityService ActivityService { get; set; } = null!;

    [Inject]
    public ITicketService TicketService { get; set; } = null!;

    [Inject]
    public ITicketStatisticsService TicketStatisticsService { get; set; } = null!;

    [Inject]
    public IWorkLogService WorkLogService { get; set; } = null!;

    private bool IsStaff => this.user?.Role is UserRole.Admin or UserRole.Developer;

    private string FullName => this.model is null
        ? this.user?.Login ?? string.Empty
        : string.Join(' ', new[] { this.model.FirstName, this.model.MiddleName, this.model.LastName }.Where(p => !string.IsNullOrWhiteSpace(p)));

    private UpdateContactRequest? PrimaryContact =>
        this.model?.Contacts.FirstOrDefault(c => c.IsPrimary && !string.IsNullOrWhiteSpace(c.Value))
        ?? this.model?.Contacts.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Value));

    protected override async Task OnInitializedAsync()
    {
        try
        {
            this.user = this.AuthService.CurrentUser;
            if (this.user == null)
            {
                this.loadError = "profile.error.unauthorized";
                return;
            }

            this.contactTypes = await this.ProfileService.GetContactTypesAsync();
            var profileDto = await this.ProfileService.GetProfileAsync(this.user.Id);

            if (profileDto != null)
            {
                this.model = new UpdateProfileRequest
                {
                    FirstName = profileDto.FirstName,
                    LastName = profileDto.LastName,
                    MiddleName = profileDto.MiddleName,
                    Bio = profileDto.Bio,
                    Contacts = profileDto.Contacts.Select(c => new UpdateContactRequest
                    {
                        Id = c.Id,
                        Value = c.Value,
                        ContactTypeId = c.ContactTypeId,
                        IsPrimary = c.IsPrimary,
                    }).ToList(),
                };
            }
            else
            {
                this.loadError = "profile.error.notFound";
                return;
            }
        }
        catch (Exception)
        {
            this.loadError = "profile.error.load";
            return;
        }
        finally
        {
            this.isLoading = false;
        }

        if (this.user is not null)
        {
            await this.LoadStatsAsync(this.user);
        }
    }

    private async Task LoadStatsAsync(User current)
    {
        try
        {
            await this.LoadNumbersAndTicketsAsync(current);
            this.activity = await this.ActivityService.GetDailyActivityAsync(
                current.Id, ActivityHeatmap.FirstMonday(this.today), this.today);
        }
        catch (Exception)
        {
            // The figures are secondary: leave the form usable and drop the skeletons.
            this.recentTickets ??= [];
            this.activity ??= [];
        }
    }

    private async Task LoadNumbersAndTicketsAsync(User current)
    {
        if (current.Role == UserRole.User)
        {
            var authored = await this.TicketService.GetUserTicketsAsync(current.Id);
            var resolved = authored.Count(t => t.Status is TicketStatus.Resolved or TicketStatus.Done or TicketStatus.Closed);
            this.numbers =
            [
                ("profile.stat.created", this.Number(authored.Count)),
                ("profile.stat.open", this.Number(authored.Count - resolved)),
                ("profile.stat.resolved", this.Number(resolved)),
            ];
            this.recentTickets = authored.OrderByDescending(t => t.CreatedAt).Take(5).ToList();
        }
        else
        {
            var stats = await this.TicketStatisticsService.GetDeveloperDashboardStatsAsync(current.Id);
            var minutes = await this.WorkLogService.GetTotalTimeSpentForUserAsync(current.Id);
            var assigned = await this.TicketService.GetDeveloperTicketsAsync(current.Id);
            this.numbers =
            [
                ("profile.stat.assigned", this.Number(stats.AssignedCount)),
                ("profile.stat.inProgress", this.Number(stats.InProgressCount)),
                ("profile.stat.completed", this.Number(stats.CompletedCount)),
                ("profile.stat.hours", (minutes / 60.0).ToString("0.#", this.UiCulture)),
            ];
            this.recentTickets = assigned.OrderByDescending(t => t.CreatedAt).Take(5).ToList();
        }
    }

    private string Number(int value) => value.ToString(this.UiCulture);

    private string ContactTypeName(int typeId) => this.contactTypes.FirstOrDefault(t => t.Id == typeId)?.Name ?? string.Empty;

    private void AddContact()
    {
        this.model?.Contacts.Add(new UpdateContactRequest
        {
            ContactTypeId = this.contactTypes.FirstOrDefault()?.Id ?? 0,
        });
    }

    private void RemoveContact(UpdateContactRequest contact)
    {
        this.model?.Contacts.Remove(contact);
    }

    private async Task HandleValidSubmit()
    {
        this.warningMessage = null;
        this.errorMessage = null;
        this.isSaved = false;

        if (this.model is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(this.model.FirstName))
        {
            this.warningMessage = "profile.validation.firstNameRequired";
            return;
        }

        if (this.model.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Value)))
        {
            this.warningMessage = "profile.validation.contactValueRequired";
            return;
        }

        var current = this.AuthService.CurrentUser;
        if (current is null)
        {
            this.errorMessage = "profile.error.unauthorized";
            return;
        }

        this.isSaving = true;
        try
        {
            var result = await this.ProfileService.UpdateProfileAsync(current.Id, this.model);
            if (result.Success)
            {
                this.isSaved = true;
            }
            else
            {
                this.errorMessage = "profile.error.save";
            }
        }
        catch (Exception)
        {
            this.errorMessage = "profile.error.save";
        }
        finally
        {
            this.isSaving = false;
        }
    }
}
