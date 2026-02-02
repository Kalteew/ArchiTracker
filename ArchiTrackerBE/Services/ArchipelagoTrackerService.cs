using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ArchiTrackerBE.Dtos;
using ArchiTrackerBE.Models;

namespace ArchiTrackerBE.Services;

public class ArchipelagoTrackerService
{
    private static readonly string[] AllowedHosts = ["archipelago.gg", "www.archipelago.gg"];
    private static readonly Uri ApiBaseUri = new("https://archipelago.gg/api/");
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly HtmlParser _htmlParser = new();

    public ArchipelagoTrackerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ArchipelagoRoomVisualizationResponse> GetRoomDetailsAsync(
        ArchipelagoRoom room,
        CancellationToken cancellationToken)
    {
        var trackerUri = EnsureAllowedUri(room.Url);
        var html = await FetchHtmlAsync(trackerUri, cancellationToken);
        var document = await _htmlParser.ParseDocumentAsync(html, cancellationToken);

        var players = ParsePlayers(document);
        var hints = ParseHints(document);
        var status = await FetchRoomStatusAsync(room.Link, cancellationToken);

        return new ArchipelagoRoomVisualizationResponse
        {
            RoomCode = room.Link,
            TrackerUrl = trackerUri.ToString(),
            RoomStatus = status,
            Players = players,
            Hints = hints,
        };
    }

    private static Uri EnsureAllowedUri(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Invalid tracker URL.");
        }

        if (!AllowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Tracker host is not allowed.");
        }

        return uri;
    }

    private async Task<string> FetchHtmlAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<RoomStatusDto?> FetchRoomStatusAsync(string roomCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            return null;
        }

        try
        {
            var statusUri = new Uri(ApiBaseUri, $"room_status/{roomCode}");
            using var response = await _httpClient.GetAsync(statusUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<RoomStatusApiResponse>(options: SerializerOptions, cancellationToken: cancellationToken);
            if (payload is null)
            {
                return null;
            }

            return new RoomStatusDto
            {
                LastActivity = ParseLastActivity(payload.LastActivity),
                LastPort = payload.LastPort,
                TimeoutSeconds = payload.Timeout,
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    private static DateTimeOffset? ParseLastActivity(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static List<TrackerPlayerDto> ParsePlayers(IHtmlDocument document)
    {
        var rows = document.QuerySelectorAll("#checks-table tbody tr");
        var players = new List<TrackerPlayerDto>();

        foreach (var row in rows)
        {
            var cells = row.QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToList();
            if (cells.Count < 5)
            {
                continue;
            }

            players.Add(new TrackerPlayerDto
            {
                Slot = cells[0],
                Player = cells[1],
                State = cells[2],
                Checks = cells[3],
                LastActivity = cells.ElementAtOrDefault(4) ?? string.Empty,
            });
        }

        return players;
    }

    private static List<TrackerHintDto> ParseHints(IHtmlDocument document)
    {
        var rows = document.QuerySelectorAll("#hints-table tbody tr");
        var hints = new List<TrackerHintDto>();

        foreach (var row in rows)
        {
            var cells = row.QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToList();
            if (cells.Count < 4)
            {
                continue;
            }

            hints.Add(new TrackerHintDto
            {
                Sender = cells[0],
                Receiver = cells[1],
                Item = cells[2],
                Location = cells[3],
            });
        }

        return hints;
    }

    private sealed record RoomStatusApiResponse
    {
        [JsonPropertyName("last_activity")]
        public string? LastActivity { get; init; }

        [JsonPropertyName("last_port")]
        public int? LastPort { get; init; }

        [JsonPropertyName("timeout")]
        public int? Timeout { get; init; }
    }
}
