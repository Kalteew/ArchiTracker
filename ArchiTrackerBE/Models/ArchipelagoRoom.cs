namespace ArchiTrackerBE.Models;

public class ArchipelagoRoom
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? IpAdded { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
