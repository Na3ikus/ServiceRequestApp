namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding.Demo;

/// <summary>
/// Ids of the users and products seeded by the migrations (<see cref="UserConfiguration"/>, <see cref="ProductConfiguration"/>).
/// </summary>
internal static class DemoIds
{
    public const int Admin = 1;
    public const int Kovalenko = 2;
    public const int Shevchenko = 3;
    public const int Smith = 4;
    public const int Bondarenko = 5;
    public const int Melnyk = 6;
    public const int Tkachenko = 7;
    public const int Lysenko = 8;
    public const int Petrenko = 9;
    public const int Wilson = 10;
    public const int Hrytsenko = 11;
    public const int Savchuk = 12;

    /// <summary>The QA engineer moves tickets into testing.</summary>
    public const int Qa = Bondarenko;

    public const int Accounting = 1;
    public const int Warehouse = 2;
    public const int HrPortal = 3;
    public const int Shop = 4;
    public const int Crm = 5;
    public const int Pos = 6;
    public const int SmartLock = 7;
    public const int Router = 8;
}
