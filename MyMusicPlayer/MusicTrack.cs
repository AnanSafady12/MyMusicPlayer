using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMusicPlayer
{
    public class MusicTrack
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? ApiCoverUrl { get; set; }   // iTunes cover URL (cached)

        // User-managed images for this song (paths on disk)
        public List<string> ImagePaths { get; set; } = new List<string>();

        // Optional: user override for title (if you want to keep original filename too)
        public string? EditedTitle { get; set; }


        // This makes sure the ListBox shows the Title instead of object name
        public override string ToString()
        {
            return Title;
        }
    }
}