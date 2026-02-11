using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using MyMusicPlayer.Services;
using MyMusicPlayer.Models;
using System.Windows.Media.Imaging;
using System.Net.Http;

namespace MyMusicPlayer
{
    public partial class MainWindow : Window
    {
        // Global Variables
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;
        private const string FILE_NAME = "library.json";

        private readonly ITunesService _itunesService = new ITunesService();
        private CancellationTokenSource? _itunesCts;
        private static readonly HttpClient _http = new HttpClient();

        // Cover slideshow (custom images)
        private DispatcherTimer coverTimer = new DispatcherTimer();
        private int coverIndex = 0;
        private MusicTrack? currentTrackForCovers = null;

        public MainWindow()
        {
            InitializeComponent();

            SetDefaultCover();
            txtArtist.Text = "-";
            txtAlbum.Text = "-";
            txtStatus.Text = imgCover.Source == null ? "Cover NOT loaded" : "Cover loaded";

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            coverTimer.Interval = TimeSpan.FromSeconds(3);
            coverTimer.Tick += CoverTimer_Tick;

            LoadLibrary(); // Load playlist when app starts
        }

        // ------------------------------------
        // COVER SLIDESHOW (custom images)
        // ------------------------------------

        private void CoverTimer_Tick(object? sender, EventArgs e)
        {
            if (currentTrackForCovers == null) return;

            var images = currentTrackForCovers.ImagePaths;
            if (images == null || images.Count == 0)
            {
                coverTimer.Stop();
                return;
            }

            // Skip missing files
            int tries = 0;
            while (tries < images.Count && !File.Exists(images[coverIndex]))
            {
                coverIndex = (coverIndex + 1) % images.Count;
                tries++;
            }

            if (!File.Exists(images[coverIndex]))
                return;

            imgCover.Source = new BitmapImage(new Uri(images[coverIndex], UriKind.Absolute));
            coverIndex = (coverIndex + 1) % images.Count;
        }

        private void StartCoverSlideshowIfNeeded(MusicTrack track)
        {
            if (track.ImagePaths != null && track.ImagePaths.Count > 0)
            {
                currentTrackForCovers = track;
                coverIndex = 0;

                // show first custom image immediately
                if (File.Exists(track.ImagePaths[0]))
                {
                    imgCover.Source = new BitmapImage(new Uri(track.ImagePaths[0], UriKind.Absolute));
                }

                coverTimer.Start();
                txtStatus.Text = "Cover slideshow (custom images)";
            }
            else
            {
                coverTimer.Stop();
                currentTrackForCovers = null;
                // cover will be handled by cached API/default logic
            }
        }

        private void StopCoverSlideshow()
        {
            coverTimer.Stop();
            currentTrackForCovers = null;
            coverIndex = 0;
        }

        // ------------------------------------
        // BASIC PLAYBACK (Demand 2 + Demand 3 slideshow)
        // ------------------------------------

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                mediaPlayer.Open(new Uri(track.FilePath));
                mediaPlayer.Play();
                timer.Start();

                txtCurrentSong.Text = track.EditedTitle ?? track.Title;
                txtStatus.Text = "Playing";

                StartCoverSlideshowIfNeeded(track);

                // Start iTunes/cache load in parallel (non-blocking)
                _ = FetchAndShowITunesMetadataAsync(track);
                return;
            }

            // If nothing is selected, just resume
            mediaPlayer.Play();
            timer.Start();
            txtStatus.Text = "Playing";
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0;

            StopCoverSlideshow();
            _itunesCts?.Cancel();

            txtStatus.Text = "Stopped";
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        // ------------------------------------
        // TIMER + PROGRESS BAR (Step 5)
        // ------------------------------------

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        // ------------------------------------
        // LIBRARY CRUD
        // ------------------------------------

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    MusicTrack track = new MusicTrack
                    {
                        Title = Path.GetFileNameWithoutExtension(file),
                        FilePath = file
                    };
                    library.Add(track);
                }
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                mediaPlayer.Open(new Uri(track.FilePath));
                mediaPlayer.Play();
                timer.Start();

                txtCurrentSong.Text = track.EditedTitle ?? track.Title;
                txtStatus.Text = "Playing";

                StartCoverSlideshowIfNeeded(track);

                _ = FetchAndShowITunesMetadataAsync(track);
            }
        }

        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }

        private void SaveLibrary()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(library, options);
            File.WriteAllText(FILE_NAME, json);
        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                UpdateLibraryUI();
            }
        }

        // ------------------------------------
        // Settings + Edit window
        // ------------------------------------

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWin = new Settings();
            settingsWin.OnScanCompleted += SettingsWin_OnScanCompleted;
            settingsWin.ShowDialog();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is not MusicTrack track)
            {
                MessageBox.Show("Please select a song first.");
                return;
            }

            var win = new EditSongWindow(track, SaveLibrary);
            win.Owner = this;
            win.ShowDialog();

            UpdateLibraryUI();
            if (lstLibrary.SelectedItem is MusicTrack t)
                txtCurrentSong.Text = t.EditedTitle ?? t.Title;
        }

        private void SettingsWin_OnScanCompleted(List<MusicTrack> eventDateList)
        {
            foreach (var track in eventDateList)
            {
                if (!library.Any(x => x.FilePath == track.FilePath))
                    library.Add(track);
            }

            UpdateLibraryUI();
            SaveLibrary();
        }

        // ------------------------------------
        // SINGLE CLICK (Demand 2): show name + full path + cached data only (NO API CALL)
        // ------------------------------------

        private void LstLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is not MusicTrack track)
                return;

            // Selection is not playback: stop slideshow
            StopCoverSlideshow();

            txtCurrentSong.Text = track.EditedTitle ?? Path.GetFileNameWithoutExtension(track.FilePath);
            txtFilePath.Text = track.FilePath;

            txtArtist.Text = track.Artist ?? "-";
            txtAlbum.Text = track.Album ?? "-";

            // Cover from JSON cache only
            if (track.ImagePaths != null && track.ImagePaths.Count > 0 && File.Exists(track.ImagePaths[0]))
            {
                imgCover.Source = new BitmapImage(new Uri(track.ImagePaths[0], UriKind.Absolute));
                txtStatus.Text = "Cover loaded (custom)";
            }
            else if (!string.IsNullOrWhiteSpace(track.ApiCoverUrl))
            {
                _ = LoadCoverFromUrlAsync(track.ApiCoverUrl);
            }
            else
            {
                SetDefaultCover();
                txtStatus.Text = "Default cover";
            }
        }

        private async Task LoadCoverFromUrlAsync(string url)
        {
            try
            {
                byte[] bytes = await _http.GetByteArrayAsync(url);
                var bmp = new BitmapImage();
                using (var ms = new MemoryStream(bytes))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                imgCover.Source = bmp;
                txtStatus.Text = "Cover loaded (cached API)";
            }
            catch
            {
                SetDefaultCover();
                txtStatus.Text = "Default cover (fallback)";
            }
        }

        // ------------------------------------
        // Default cover
        // ------------------------------------

        private void SetDefaultCover()
        {
            try
            {
                imgCover.Source = new BitmapImage(
                    new Uri("pack://application:,,,/assets/default_cover.jpg", UriKind.Absolute)
                );
                txtStatus.Text = "Cover loaded (resource)";
            }
            catch (Exception ex)
            {
                imgCover.Source = null;
                txtStatus.Text = "Cover load failed: " + ex.Message;
            }
        }

        // ------------------------------------
        // iTunes query + cache (Demand 2 + Demand 3.1)
        // ------------------------------------

        private string BuildSearchQueryFromFile(MusicTrack track)
        {
            string name = Path.GetFileNameWithoutExtension(track.FilePath);
            name = name.Replace("-", " ").Trim();
            return name;
        }

        private async Task FetchAndShowITunesMetadataAsync(MusicTrack track)
        {
            _itunesCts?.Cancel();
            _itunesCts = new CancellationTokenSource();
            var ct = _itunesCts.Token;

            string query = BuildSearchQueryFromFile(track);

            try
            {
                // 1) CACHE FIRST (NO API CALL)
                bool hasCachedMetadata =
                    !string.IsNullOrWhiteSpace(track.Artist) ||
                    !string.IsNullOrWhiteSpace(track.Album) ||
                    !string.IsNullOrWhiteSpace(track.ApiCoverUrl) ||
                    (track.ImagePaths != null && track.ImagePaths.Count > 0);

                if (hasCachedMetadata)
                {
                    txtCurrentSong.Text = track.EditedTitle ?? track.Title;
                    txtArtist.Text = track.Artist ?? "-";
                    txtAlbum.Text = track.Album ?? "-";
                    txtFilePath.Text = track.FilePath;

                    // If custom images exist: slideshow owns the image
                    if (track.ImagePaths != null && track.ImagePaths.Count > 0)
                    {
                        txtStatus.Text = "Cover slideshow (custom images)";
                        return;
                    }

                    // Otherwise show cached API cover or default
                    if (!string.IsNullOrWhiteSpace(track.ApiCoverUrl))
                    {
                        await LoadCoverFromUrlAsync(track.ApiCoverUrl);
                    }
                    else
                    {
                        SetDefaultCover();
                        txtStatus.Text = "Default cover (cached)";
                    }

                    return;
                }

                // 2) NO CACHE -> CALL ITUNES (async, non-blocking)
                txtStatus.Text = "Searching iTunes...";

                ITunesTrack? result = await _itunesService.SearchBestMatchAsync(query, ct);
                ct.ThrowIfCancellationRequested();

                if (result == null)
                {
                    // fallback on no match
                    txtArtist.Text = "-";
                    txtAlbum.Text = "-";
                    txtCurrentSong.Text = Path.GetFileNameWithoutExtension(track.FilePath);
                    txtFilePath.Text = track.FilePath;
                    SetDefaultCover();
                    txtStatus.Text = "No iTunes match (showing local info)";
                    return;
                }

                // 3) SAVE (CACHE) INTO TRACK + JSON
                track.Artist = result.ArtistName ?? "-";
                track.Album = result.CollectionName ?? "-";

                if (string.IsNullOrWhiteSpace(track.EditedTitle))
                    track.EditedTitle = result.TrackName;

                if (!string.IsNullOrWhiteSpace(result.ArtworkUrl100))
                    track.ApiCoverUrl = result.ArtworkUrl100.Replace("100x100", "600x600");

                SaveLibrary();

                // 4) UPDATE UI
                txtCurrentSong.Text = track.EditedTitle ?? track.Title;
                txtArtist.Text = track.Artist ?? "-";
                txtAlbum.Text = track.Album ?? "-";
                txtFilePath.Text = track.FilePath;

                if (!string.IsNullOrWhiteSpace(track.ApiCoverUrl))
                {
                    await LoadCoverFromUrlAsync(track.ApiCoverUrl);
                    txtStatus.Text = "Cover loaded (iTunes, cached)";
                }
                else
                {
                    SetDefaultCover();
                    txtStatus.Text = "No cover in iTunes (default used)";
                }
            }
            catch (OperationCanceledException)
            {
                // switching songs quickly - ok
            }
            catch (Exception ex)
            {
                // fallback on error
                txtCurrentSong.Text = Path.GetFileNameWithoutExtension(track.FilePath);
                txtFilePath.Text = track.FilePath;
                txtArtist.Text = "-";
                txtAlbum.Text = "-";
                SetDefaultCover();
                txtStatus.Text = "iTunes error: " + ex.Message;
            }
        }
    }
}
