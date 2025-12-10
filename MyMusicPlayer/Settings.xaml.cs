using Microsoft.Win32;
using System.IO;
using System.Windows;



namespace MyMusicPlayer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private AppSettings currentSettings;
        public event Action<List<MusicTrack>>? OnScanCompleted;

        public Settings()
        {
            InitializeComponent();
            currentSettings = AppSettings.Load();
            RefreshFolderList();
        }
        private void RefreshFolderList()
        {
            lstFolders.ItemsSource = null;
            lstFolders.ItemsSource = currentSettings.MusicFolders;
        }

        private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.FolderName;
                if (!currentSettings.MusicFolders.Contains(folder))
                {
                    currentSettings.MusicFolders.Add(folder);
                    AppSettings.Save(currentSettings);
                    RefreshFolderList();
                }
            }

        }

        private void BtnRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (lstFolders.SelectedItem is string folder)
            {
                currentSettings.MusicFolders.Remove(folder);
                AppSettings.Save(currentSettings);
                RefreshFolderList();
            }

        }



        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            //create a list to hold found tracks(Main window model)
            List<MusicTrack> foundTracks = new List<MusicTrack>();

            foreach (string folderPath in currentSettings.MusicFolders)
            {
                if (Directory.Exists(folderPath))
                {
                    // SearchOption.AllDirectories makes it scan sub-folders
                    string[] files = Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        foundTracks.Add(new MusicTrack
                        {
                            Title = Path.GetFileNameWithoutExtension(file),
                            FilePath = file
                        });
                    }
                }
            }

            // Send data back to MainWindow
            // event invocation to notify scan completion
            OnScanCompleted?.Invoke(foundTracks);
            if (OnScanCompleted!= null)
            {
                //at least one subscriber 
                OnScanCompleted(foundTracks);
            }
            MessageBox.Show($"Scan Complete! Found {foundTracks.Count} songs.");
            this.Close();

        }
    }
}
