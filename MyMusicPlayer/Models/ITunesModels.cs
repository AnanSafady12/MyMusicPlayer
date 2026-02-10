using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyMusicPlayer.Models
{
    public class ITunesSearchResponse
    {
        [JsonPropertyName("resultCount")]
        public int ResultCount { get; set; }

        [JsonPropertyName("results")]
        public List<ITunesTrack> Results { get; set; } = new();
    }

    public class ITunesTrack
    {
        [JsonPropertyName("trackName")]
        public string? TrackName { get; set; }

        [JsonPropertyName("artistName")]
        public string? ArtistName { get; set; }

        [JsonPropertyName("collectionName")]
        public string? CollectionName { get; set; }

        // artworkUrl100 exists almost always; we can convert to higher res later
        [JsonPropertyName("artworkUrl100")]
        public string? ArtworkUrl100 { get; set; }
    }
}
