using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Controls;

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

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            LoadLibrary(); // Load playlist when app starts
        }


        // ------------------------------------
        // BASIC PLAYBACK (Step 4)
        // ------------------------------------

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            //if(sender is Button btn)
            //{
            //    btn.Background=Brushes.LightGreen;
            //}



            mediaPlayer.Play();
            timer.Start();
            txtStatus.Text = "playing";
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
        // EMPTY METHODS FOR NEXT STEPS
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
                        Title = System.IO.Path.GetFileNameWithoutExtension(file),
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
                txtCurrentSong.Text = track.Title;
                txtStatus.Text = "Playing";
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
            string json = JsonSerializer.Serialize(library,options);
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //1 create the settings window instance
            Settings settingsWin = new Settings();

            // 2 subscribe/register to the OnScanCompleted event
            settingsWin.OnScanCompleted += SettingsWin_OnScanCompleted;

            settingsWin.ShowDialog();
        }


        private void SettingsWin_OnScanCompleted(List<MusicTrack> eventDateList)
        {
            foreach (var track in eventDateList)
            {
                // Prevent duplicates based on FilePath
                if (!library.Any(x => x.FilePath == track.FilePath))
                {
                    library.Add(track);
                }
            }

            UpdateLibraryUI();
            SaveLibrary();
        }
    }
}
