namespace ArchiTrackerBE.Dtos;

public class TrackerPlayerDto
{
    public string Slot { get; set; } = string.Empty;
    public string Player { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Checks { get; set; } = string.Empty;
    public string LastActivity { get; set; } = string.Empty;
}

public class TrackerHintDto
{
    public string Sender { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class RoomStatusDto
{
    public DateTimeOffset? LastActivity { get; set; }
    public int? LastPort { get; set; }
    public int? TimeoutSeconds { get; set; }
}

public class ArchipelagoRoomVisualizationResponse
{
    public string RoomCode { get; set; } = string.Empty;
    public string TrackerUrl { get; set; } = string.Empty;
    public RoomStatusDto? RoomStatus { get; set; }
    public IReadOnlyCollection<TrackerPlayerDto> Players { get; set; } = Array.Empty<TrackerPlayerDto>();
    public IReadOnlyCollection<TrackerHintDto> Hints { get; set; } = Array.Empty<TrackerHintDto>();
}
