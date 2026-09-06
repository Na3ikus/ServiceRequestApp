using System.Globalization;
using System.Resources;

namespace ServiceDeskSystemApp.Resources.Localization;

public class AppResources
{
    private static readonly ResourceManager ResourceManager =
        new ResourceManager("ServiceDeskSystemApp.Resources.Localization.AppResources", typeof(AppResources).Assembly);

    public static CultureInfo? Culture { get; set; }

    public static string GetString(string name)
    {
        return ResourceManager.GetString(name, Culture ?? CultureInfo.CurrentUICulture) ?? name;
    }

    // General
    public static string AppTitle => GetString(nameof(AppTitle));
    public static string Loading => GetString(nameof(Loading));
    public static string Error => GetString(nameof(Error));
    public static string OK => GetString(nameof(OK));
    public static string Cancel => GetString(nameof(Cancel));
    public static string Retry => GetString(nameof(Retry));

    // Login
    public static string LoginTitle => GetString(nameof(LoginTitle));
    public static string LoginSubtitle => GetString(nameof(LoginSubtitle));
    public static string Username => GetString(nameof(Username));
    public static string UsernamePlaceholder => GetString(nameof(UsernamePlaceholder));
    public static string Password => GetString(nameof(Password));
    public static string PasswordPlaceholder => GetString(nameof(PasswordPlaceholder));
    public static string SignIn => GetString(nameof(SignIn));
    public static string SigningIn => GetString(nameof(SigningIn));

    // Dashboard
    public static string Dashboard => GetString(nameof(Dashboard));
    public static string WelcomeBack => GetString(nameof(WelcomeBack));
    public static string DashboardSubtitle => GetString(nameof(DashboardSubtitle));
    public static string TotalTickets => GetString(nameof(TotalTickets));
    public static string OpenTickets => GetString(nameof(OpenTickets));
    public static string CriticalPriority => GetString(nameof(CriticalPriority));
    public static string MyTickets => GetString(nameof(MyTickets));
    public static string RecentActivity => GetString(nameof(RecentActivity));
    public static string ViewAll => GetString(nameof(ViewAll));

    // Tickets
    public static string Tickets => GetString(nameof(Tickets));
    public static string TicketsSubtitle => GetString(nameof(TicketsSubtitle));
    public static string SearchTickets => GetString(nameof(SearchTickets));
    public static string NoTicketsFound => GetString(nameof(NoTicketsFound));
    public static string NoTicketsSubtext => GetString(nameof(NoTicketsSubtext));
    public static string PullToRefresh => GetString(nameof(PullToRefresh));

    // Ticket Detail
    public static string TicketDetails => GetString(nameof(TicketDetails));
    public static string Description => GetString(nameof(Description));
    public static string StepsToReproduce => GetString(nameof(StepsToReproduce));
    public static string Details => GetString(nameof(Details));
    public static string Comments => GetString(nameof(Comments));
    public static string NoComments => GetString(nameof(NoComments));
    public static string Product => GetString(nameof(Product));
    public static string Priority => GetString(nameof(Priority));
    public static string Status => GetString(nameof(Status));
    public static string Type => GetString(nameof(Type));
    public static string AssignedTo => GetString(nameof(AssignedTo));
    public static string CreatedBy => GetString(nameof(CreatedBy));
    public static string CreatedAt => GetString(nameof(CreatedAt));
    public static string DueDate => GetString(nameof(DueDate));
    public static string Environment => GetString(nameof(Environment));
    public static string NotAssigned => GetString(nameof(NotAssigned));

    // Profile
    public static string Profile => GetString(nameof(Profile));
    public static string ProfileTitle => GetString(nameof(ProfileTitle));
    public static string AccountInfo => GetString(nameof(AccountInfo));
    public static string ContactInfo => GetString(nameof(ContactInfo));
    public static string Login => GetString(nameof(Login));
    public static string Email => GetString(nameof(Email));
    public static string Bio => GetString(nameof(Bio));
    public static string Role => GetString(nameof(Role));
    public static string Logout => GetString(nameof(Logout));
    public static string LogoutConfirm => GetString(nameof(LogoutConfirm));
}
