using System;
using System.Windows;
using MyMusicPlayer.ViewModels;

namespace MyMusicPlayer
{
    public partial class EditSongWindow : Window
    {
        public EditSongWindow(MusicTrack track, Action saveLibraryCallback)
        {
            InitializeComponent();
            DataContext = new EditSongViewModel(track, saveLibraryCallback);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
