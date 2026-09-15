namespace ServiceDeskSystem.Domain.Entities;

using ServiceDeskSystem.Domain.Common;
using ServiceDeskSystem.Domain.Enums;

public class SlaPolicy : Entity
{
    public const int DefaultWarningThresholdPercent = 80;

    private const int MinutesPerDay = 24 * 60;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TicketPriority Priority { get; set; }

    public int? ProductId { get; set; }

    public Product? Product { get; set; }

    public int ResponseMinutes { get; set; }

    public int ResolutionMinutes { get; set; }

    public int WarningThresholdPercent { get; set; } = DefaultWarningThresholdPercent;

    public bool EscalateOnBreach { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public int Specificity => this.ProductId.HasValue ? 1 : 0;

    public static SlaPolicy Create(
        string name,
        TicketPriority priority,
        int responseMinutes,
        int resolutionMinutes,
        int? productId = null,
        int warningThresholdPercent = DefaultWarningThresholdPercent)
    {
        return new SlaPolicy
        {
            Name = name,
            Priority = priority,
            ResponseMinutes = responseMinutes,
            ResolutionMinutes = resolutionMinutes,
            ProductId = productId,
            WarningThresholdPercent = NormalizeThreshold(warningThresholdPercent),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public static int NormalizeThreshold(int percent) => Math.Clamp(percent, 1, 99);

    public static string FormatMinutes(int minutes)
    {
        if (minutes <= 0)
        {
            return "0m";
        }

        var days = minutes / MinutesPerDay;
        var hours = minutes % MinutesPerDay / 60;
        var remainingMinutes = minutes % 60;

        var parts = new List<string>(3);
        if (days > 0)
        {
            parts.Add($"{days}d");
        }

        if (hours > 0)
        {
            parts.Add($"{hours}h");
        }

        if (remainingMinutes > 0 && days == 0)
        {
            parts.Add($"{remainingMinutes}m");
        }

        return string.Join(" ", parts);
    }

    public DateTime GetResponseDueAt(DateTime createdAt) => createdAt.AddMinutes(this.ResponseMinutes);

    public DateTime GetResolutionDueAt(DateTime createdAt) => createdAt.AddMinutes(this.ResolutionMinutes);

    public DateTime GetWarningAt(DateTime createdAt)
    {
        var elapsed = this.ResolutionMinutes * (double)NormalizeThreshold(this.WarningThresholdPercent) / 100d;
        return createdAt.AddMinutes(elapsed);
    }
}
