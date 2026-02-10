using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MyMusicPlayer.Models;

namespace MyMusicPlayer.Services
{
    public class ITunesService
    {
        private readonly HttpClient _http;

        public ITunesService()
        {
            _http = new HttpClient();
        }

        // Returns the best matching track (or null if none)
        public async Task<ITunesTrack?> SearchBestMatchAsync(string query, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            // iTunes search endpoint
            // media=music limits to music results
            // entity=song returns songs
            var url =
                $"https://itunes.apple.com/search?term={Uri.EscapeDataString(query)}&media=music&entity=song&limit=10";

            using var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);

            var data = JsonSerializer.Deserialize<ITunesSearchResponse>(json);
            if (data == null || data.Results == null || data.Results.Count == 0)
                return null;

            // Simple best match: first result
            // (Later we can improve matching logic)
            return data.Results.FirstOrDefault();
        }
    }
}
