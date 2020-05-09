using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using ZMuse.Command;
using ZMuse.Model;
using DataGrid = System.Windows.Controls.DataGrid;

namespace ZMuse.ViewModel
{
    internal class MainWindowVM : INotifyPropertyChanged
    {
        // Variable/Property //////////////////////////////////////////////////////////////////////
        // Public /////////////////////////////////////////////////////////////////////////////////

        public ICommand                         CmdNext                 { get; private set; }
        public ICommand                         CmdPlayPause            { get; private set; }
        public ICommand                         CmdPrev                 { get; private set; }
        public ICommand                         CmdStop                 { get; private set; }
        public ICommand                         CmdArtistAdd            { get; private set; }
        public ICommand                         CmdAlbumAdd             { get; private set; }
        public ICommand                         CmdSongAdd              { get; private set; }
        public ICommand                         CmdSongRemove           { get; private set; }
        public ICommand                         CmdSongShuffle          { get; private set; }
        public ICommand                         CmdLibraryFolder        { get; private set; }
        
        public Boolean                          IsPaused                { get; set;         } = false;
        public String                           NameAlbum               => _songAudioFile?.NameAlbum;
        public String                           NameArtist              => _songAudioFile?.NameArtist;
        public String                           NameFile                => _songAudioFile?.FileName;
        public String                           NameFileImage           => _songAudioFile?.FileNameImage;
        public String                           NameSong                => _songAudioFile?.NameSong;
        public String                           NameTrack               => _songAudioFile?.Track.ToString();
        public String                           LibraryFolder           { get; private set; } = "";
        public ObservableCollection<AudioFile>  LibraryList             { get; private set; } = null;
        public ObservableCollection<AudioFile>  PlayList                { get; private set; } = null;

        public String LibrarySearch  
        {
            get
            {
                return _librarySearch;
            }
            set
            {
                AudioFile file;
                Int32     index,
                          count;

                // Get the new value.
                this._librarySearch = value;

                // Clean out the old.
                this.LibraryList.Clear();

                // Repopulate.
                count = this._audioFileList.FileList.Count;
                for (index = 0; index < count; index++)
                {
                    file = this._audioFileList.FileList[index];

                    // If the value appears anywhere in the filename
                    value = value.ToLower();
                    if (file.NameArtist.ToLower().Contains(value) ||
                        file.NameAlbum.ToLower().Contains( value) ||
                        file.NameSong.ToLower().Contains(  value))
                    {
                        this.LibraryList.Add(file);
                    }
                }

                // Update the grid.
                this.OnPropertyChanged("LibraryList");
            }
        }

        public String NameLength
        {
            get => this._nameLength;
            set {  this._nameLength = value; this.OnPropertyChanged("NameLength"); }
        }

        public Boolean IsPlaying
        {
            get => this._isPlaying;
            set {  this._isPlaying = value; this.OnPropertyChanged("IsPlaying"); }
        }

        public Int32 SongPosition
        {
            get => this._songPosition;
            set {  this._songPosition = value; this.OnPropertyChanged("SongPosition"); }
        }

        public Int32 SongIndex
        {
            get => this._songIndex;
            set => this._SetSong(value);
        }

        // private variables.

        private AudioFileList                   _audioFileList          = null;
        private Boolean                         _hasSongEnded           = false;
        private Boolean                         _isPlaying              = false;
        private List<AudioFile>                 _libraryListSelected    = null;
        private String                          _librarySearch          = "";
        private String                          _nameLength             = null;
        private List<AudioFile>                 _playListSelected       = null;
        private AudioFile                       _songAudioFile          = null;
        private Int32                           _songIndex              = 0;
        private Int32                           _songPosition           = 0;
        private readonly Timer                  _timer                  = null;
        private readonly Window                 _view                   = null;
        private WMPLib.WindowsMediaPlayer       _wmPlayer               = null;

        // Function ///////////////////////////////////////////////////////////////////////////////
        // Public /////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowVM(Window view)
        {
            _SettingLoad();

            this.CmdNext            = new CommandHandler(null,                this._ExeNext);
            this.CmdPlayPause       = new CommandHandler(null,                this._ExePlayPause);
            this.CmdPrev            = new CommandHandler(null,                this._ExePrev);
            this.CmdStop            = new CommandHandler(null,                this._ExeStop);
            this.CmdArtistAdd       = new CommandHandler(this._CanLibSel1,     this._ExeAddArtist);
            this.CmdAlbumAdd        = new CommandHandler(this._CanLibSel1,     this._ExeAddAlbum);
            this.CmdSongAdd         = new CommandHandler(this._CanLibSel1OrN,  this._ExeAddSong);
            this.CmdSongRemove      = new CommandHandler(this._CanPlaySel1OrN, this._ExeRemoveSong);
            this.CmdSongShuffle     = new CommandHandler(this._CanPlay2OrN,    this._ExeShuffleSong);
            this.CmdLibraryFolder   = new CommandHandler(null,                this._ExePickLibraryFolder);

            this._view              = view;

            this._wmPlayer          = new WMPLib.WindowsMediaPlayer();
            this._wmPlayer.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(_SetHasSongEnded);

            this._timer             = new Timer();
            this._timer.Tick       += new EventHandler(_TimerTick);
            this._timer.Interval    = 100;
            this._timer.Start();
        }

        /// <summary>
        /// Selection changed in the library list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LibraryListSelChange(Object sender, EventArgs e)
        {
            DataGrid dgrid;
            Int32    count;
            Int32    index;
            
            dgrid = sender as DataGrid;
            count = dgrid.SelectedItems.Count;

            this._libraryListSelected = new List<AudioFile>();
            for (index = 0; index < count; index++)
            {
                this._libraryListSelected.Add(dgrid.SelectedItems[index] as AudioFile);
            }
        }

        /// <summary>
        /// Selection changed in the play list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PlayListSelChange(Object sender, EventArgs e)
        {
            DataGrid dgrid;
            Int32    count;
            Int32    index;
            
            dgrid = sender as DataGrid;
            count = dgrid.SelectedItems.Count;

            this._playListSelected = new List<AudioFile>();
            for (index = 0; index < count; index++)
            {
                this._playListSelected.Add(dgrid.SelectedItems[index] as AudioFile);
            }
        }

        // Private ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Test for single library list selection.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private Boolean _CanLibSel1(Object parameter)    => (this._libraryListSelected?.Count == 1);

        /// <summary>
        /// Test for multiple library list selection.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private Boolean _CanLibSel1OrN(Object parameter)  => (this._libraryListSelected?.Count >= 1);

        /// <summary>
        /// Test for multiple play list selection.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private Boolean _CanPlaySel1OrN(Object parameter) => (this._playListSelected?.Count >= 1);

        /// <summary>
        /// Test for play list size > 1.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private Boolean _CanPlay2OrN(Object parameter)    => (this.PlayList?.Count >= 2);

        /// <summary>
        /// Command to add all songes of an artist.
        /// </summary>
        /// <param name="parameter"></param>
        private void _ExeAddArtist(Object parameter)
        {
            AudioFile selected;
            Int32     index;
            Int32     count;
            Boolean   IsAddingToEmpty;

            IsAddingToEmpty = (this.PlayList.Count == 0);

            selected = this._libraryListSelected[0];

            count = this.LibraryList.Count;
            for (index = 0; index < count; index++)
            {
                if (selected.NameArtist.Equals(this.LibraryList[index].NameArtist))
                {
                    PlayList.Add(this.LibraryList[index]);
                }
            }

            this.OnPropertyChanged("PlayList");

            if (IsAddingToEmpty)
            {
                this._SetSong(0);
            }
        }

        /// <summary>
        /// Command to add all the songs of an album
        /// </summary>
        /// <param name="parameter"></param>
        private void _ExeAddAlbum(Object parameter)
        {
            AudioFile selected;
            Int32     index;
            Int32     count;
            Boolean   isAddingToEmpty;

            isAddingToEmpty = (this.PlayList.Count == 0);

            selected = this._libraryListSelected[0];

            count = this.LibraryList.Count;
            for (index = 0; index < count; index++)
            {
                if (selected.NameArtist.Equals(this.LibraryList[index].NameArtist) &&
                    selected.NameAlbum.Equals( this.LibraryList[index].NameAlbum))
                {
                    this.PlayList.Add(LibraryList[index]);
                }
            }

            if (isAddingToEmpty)
            {
                this._SetSong(0);
            }
        }

        /// <summary>
        /// Command to add all the selected songs.
        /// </summary>
        /// <param name="parameter"></param>
        private void _ExeAddSong(Object parameter)
        {
            Int32   index;
            Int32   count;
            Boolean isAddingToEmpty;

            isAddingToEmpty = (this.PlayList.Count == 0);

            count = this._libraryListSelected.Count;
            for (index = 0; index < count; index++)
            {
                this.PlayList.Add(this._libraryListSelected[index]);
            }

            this.OnPropertyChanged("PlayList");

            if (isAddingToEmpty)
            {
                this._SetSong(0);
            }
        }

        /// <summary>
        /// Command to move to the next song
        /// </summary>
        /// <param name="param"></param>
        private void _ExeNext(Object param)
        {
            Boolean playTemp;

            playTemp = this.IsPlaying;

            // Stop playing
            if (this.IsPlaying || this.IsPaused)
            {
                this._ExeStop(null);
            }

            // Change the song.
            this.SongIndex = this.SongIndex + 1;

            // Resume playing
            if (playTemp)
            {
                this._ExePlayPause(null);
            }
        }

        /// <summary>
        /// Command to select the library folder.
        /// </summary>
        /// <param name="parameter"></param>
        private void _ExePickLibraryFolder(Object parameter)
        {
            VistaFolderBrowserDialog folderBrowser;

            folderBrowser = new VistaFolderBrowserDialog();

            // Get the folder.
            if (folderBrowser.ShowDialog(this._view) == true)
            {
                // Set the folder path.  
                this._SetLibraryFolder(folderBrowser.SelectedPath);

                // Update the ui.
                this.OnPropertyChanged("LibraryList");

                this.OnPropertyChanged("PlayList");

                this.OnPropertyChanged("NameAlbum"); 
                this.OnPropertyChanged("NameArtist");
                this.OnPropertyChanged("NameFile");
                this.OnPropertyChanged("NameFileImage");
                this.OnPropertyChanged("NameSong");
                this.OnPropertyChanged("NameTrack");
                this.OnPropertyChanged("NameLength");

                this.OnPropertyChanged("LibraryFolder");

                // Save the change for next time in.
                this._SettingSave();
            }
        }

        /// <summary>
        /// Command to play / pause the current song.
        /// </summary>
        /// <param name="param"></param>
        private void _ExePlayPause(Object param)
        {
            // No song to play.
            if (this._songAudioFile == null)
            {
                return;
            }

            // We are playing
            if (!this.IsPlaying && this.IsPaused)
            {
                this.IsPaused  = false;
                this.IsPlaying = true;

                this._wmPlayer.controls.play();
            }
            else if (!this.IsPlaying && !this.IsPaused)
            {
                this.IsPaused     = false;
                this.IsPlaying    = true;
                this.SongPosition = 0;

                this._wmPlayer.URL = this.NameFile;
                this._wmPlayer.controls.play();
            }
            else if (this.IsPlaying)
            {
                this.IsPlaying = false;
                this.IsPaused  = true;

                this._wmPlayer.controls.pause();
            }
        }

        /// <summary>
        /// Command to move to the previous song.
        /// </summary>
        /// <param name="param"></param>
        private void _ExePrev(Object param)
        {
            Boolean playTemp;

            playTemp = this.IsPlaying;

            // Stop playing
            if (this.IsPlaying || this.IsPaused)
            {
                this._ExeStop(null);
            }

            // Change the song.
            this.SongIndex = this.SongIndex - 1;

            // Resume playing
            if (playTemp)
            {
                this._ExePlayPause(null);
            }
        }

        /// <summary>
        /// Command to remove songs from the play list.
        /// </summary>
        /// <param name="parameter"></param>
        private void _ExeRemoveSong(Object parameter)
        {
            Int32           index;
            Int32           count;
            List<AudioFile> ltemp;

            // When we modify the _playList, _playListSelected is volatile.
            // We need to make a copy.
            ltemp = new List<AudioFile>();

            count = this._playListSelected.Count;
            for (index = 0; index < count; index++)
            {
                ltemp.Add(this._playListSelected[index]);
            }

            for (index = 0; index < count; index++)
            {
                this.PlayList.Remove(ltemp[index]);
            }

            this.OnPropertyChanged("PlayList");

            // Stop playing when things are out of sync or empty.
            if (this.SongIndex >= this.PlayList.Count ||
                (this._songAudioFile != null &&
                 !this._songAudioFile.FileName.Equals(this.PlayList[SongIndex].FileName)))
            {
                this._ExeStop(null);
                this._SetSong(0);
            }
        }

        /// <summary>
        /// Command to randomize the play list.
        /// </summary>
        /// <param name="parameter"></param>
        private void _ExeShuffleSong(Object parameter)
        {
            List<AudioFile> playList;
            Int32           index;
            Int32           count;
            Int32           songIndex;
            Random          random;

            this._ExeStop(null);
            
            // Get the current play list.
            playList = new List<AudioFile>();
            count    = this.PlayList.Count;
            for (index = 0; index < count; index++)
            {
                playList.Add(this.PlayList[index]);
            }

            // Clear the play list.
            this.PlayList.Clear();

            // Randomly repopulate the play list.
            random = new Random((Int32) DateTime.Now.Ticks & 0xFFFF);
            for (index = 0; index < count; index++)
            {
                songIndex = random.Next(playList.Count);
                
                this.PlayList.Add(playList[songIndex]);

                playList.RemoveAt(songIndex);
            }

            this.OnPropertyChanged("PlayList");

            this._SetSong(0);
        }

        /// <summary>
        /// Command to stop playing the current song.
        /// </summary>
        /// <param name="param"></param>
        private void _ExeStop(Object param)
        {
            this.IsPlaying = false;
            this.IsPaused  = false;

            this._wmPlayer.controls.stop();
        }

        /// <summary>
        /// Callback to determine that the song has ended.
        /// </summary>
        /// <param name="NewState"></param>
        private void _SetHasSongEnded(Int32 NewState)
        {
            if (NewState == (Int32) WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                this._hasSongEnded = true;
            }
        }

        /// <summary>
        /// Set the library folder.
        /// </summary>
        /// <param name="folder"></param>
        private void _SetLibraryFolder(String folder)
        { 
            Int32 index;

            this.LibraryFolder = folder;

            this._audioFileList = new AudioFileList(this.LibraryFolder);

            this.LibraryList = new ObservableCollection<AudioFile>();
            for (index = 0; index < this._audioFileList.FileList.Count; index++)
            { 
                this.LibraryList.Add(this._audioFileList.FileList[index]);
            }

            this.PlayList = new ObservableCollection<AudioFile>();
        }

        /// <summary>
        /// Set the song to play.
        /// </summary>
        /// <param name="value"></param>
        private void _SetSong(Int32 value)
        {
            // Set the song index;
            this._songIndex = Math.Min(this.PlayList.Count - 1, Math.Max(value, 0));

            // Set the song audio file.
            this._songAudioFile = null;
            if (this.PlayList.Count > 0)
            {
                this._songAudioFile = this.PlayList[this.SongIndex];
            }

            // Update the ui.
            this.OnPropertyChanged("NameAlbum"); 
            this.OnPropertyChanged("NameArtist");
            this.OnPropertyChanged("NameFile");
            this.OnPropertyChanged("NameFileImage");
            this.OnPropertyChanged("NameSong");
            this.OnPropertyChanged("NameTrack");
            this.OnPropertyChanged("NameLength");
        }

        /// <summary>
        /// Program settings load
        /// </summary>
        private void _SettingLoad()
        {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\zmuse.dat";

            if (File.Exists(path))
            { 
                String folder;

                folder = File.ReadAllText(path);

                this._SetLibraryFolder(folder);
            }
        }

        /// <summary>
        /// Program settings save
        /// </summary>
        private void _SettingSave()
        {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\zmuse.dat";

            File.WriteAllText(path, this.LibraryFolder);
        }

        /// <summary>
        /// Update the ui.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _TimerTick(object sender, EventArgs e)
        {
            Int32 itemp;

            this.SongPosition = 0;
            if (this._wmPlayer?.currentMedia != null)
            {
                this.SongPosition = (Int32) ((this._wmPlayer.controls.currentPosition /
                    this._wmPlayer.currentMedia.duration) * 1000.0);

                if (!this._wmPlayer.currentMedia.durationString.Equals(this.NameLength))
                {
                    this.NameLength = this._wmPlayer.currentMedia.durationString;
                }

                // We have come to the end.
                if (this._hasSongEnded)
                {
                    this.IsPlaying     = false;
                    this._hasSongEnded = false;

                    // For checking if there are any more songs.
                    itemp = this.SongIndex;

                    // Move to the next song.
                    this._SetSong(this.SongIndex + 1);

                    // SongIndex changed so there are more songs in the list.
                    if (itemp != this.SongIndex)
                    {
                        // Play the new song.
                        this._ExePlayPause(null);
                    }
                }
            }
        }

        // INotifyPropertyChanged /////////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
