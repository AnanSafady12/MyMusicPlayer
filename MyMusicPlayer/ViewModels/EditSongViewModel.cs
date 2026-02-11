using Microsoft.Win32;
using MyMusicPlayer.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MyMusicPlayer.ViewModels
{
    public class EditSongViewModel : INotifyPropertyChanged
    {
        private readonly MusicTrack _track;
        private readonly Action _saveLibraryCallback;

        private string _title;
        private string _artist;
        private string _album;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditSongViewModel(MusicTrack track, Action saveLibraryCallback)
        {
            _track = track;
            _saveLibraryCallback = saveLibraryCallback;

            _title = track.EditedTitle ?? track.Title;
            _artist = track.Artist ?? "-";
            _album = track.Album ?? "-";

            Images = new ObservableCollection<string>(track.ImagePaths ?? new());

            SaveCommand = new RelayCommand(_ => Save());
            AddImageCommand = new RelayCommand(_ => AddImage());
            RemoveImageCommand = new RelayCommand(p => RemoveImage(p as string));
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        public string Album
        {
            get => _album;
            set { _album = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Images { get; }

        public ICommand SaveCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }

        private void AddImage()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.png;*.jpeg",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                foreach (var file in ofd.FileNames)
                    Images.Add(file);
            }
        }

        private void RemoveImage(string? path)
        {
            if (path != null)
                Images.Remove(path);
        }

        private void Save()
        {
            _track.EditedTitle = Title;
            _track.Artist = Artist;
            _track.Album = Album;
            _track.ImagePaths = Images.ToList();

            _saveLibraryCallback.Invoke();

            MessageBox.Show("Song saved successfully");
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
