using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ZMuse.Command;
using ZMuse.Model;

namespace ZMuse.ViewModel
{
    internal class MainWindowVM : INotifyPropertyChanged
    {
        // Variable/Property //////////////////////////////////////////////////////////////////////
        
        // private variables.

        private AudioFile                       _songAudioFile          = null;
        private AudioFileList                   _audioFileList          = null;
        private ObservableCollection<AudioFile> _libraryList            = null;
        private List<AudioFile>                 _libraryListSelected    = null;
        private ObservableCollection<AudioFile> _playList               = null;
        private List<AudioFile>                 _playListSelected       = null;
        private Boolean                         _isPlaying              = false;
        private String                          _nameLength             = null;
        private Int32                           _songPosition           = 0;
        private Timer                           _timer                  = null;
        private WMPLib.WindowsMediaPlayer       _wmPlayer               = null;
        private Int32                           _songIndex              = 0;

        // auto-implemented properties.

        public Boolean                          IsPaused                { get; set;                                                     } = false;
        public String                           NameAlbum               { get { return _songAudioFile?.NameAlbum;        }   }
        public String                           NameArtist              { get { return _songAudioFile?.NameArtist;       }   }
        public String                           NameFile                { get { return _songAudioFile?.FileName;         }   }
        public String                           NameFileImage           { get { return _songAudioFile?.FileNameImage;    }   }
        public String                           NameSong                { get { return _songAudioFile?.NameSong;         }   }
        public String                           NameTrack               { get { return _songAudioFile?.Track.ToString(); }   }

        public ICommand                         CmdNext                 { get; private set; }
        public ICommand                         CmdPlayPause            { get; private set; }
        public ICommand                         CmdPrev                 { get; private set; }
        public ICommand                         CmdStop                 { get; private set; }
        public ICommand                         CmdArtistAdd            { get; private set; }
        public ICommand                         CmdAlbumAdd             { get; private set; }
        public ICommand                         CmdSongAdd              { get; private set; }
        public ICommand                         CmdSongRemove           { get; private set; }
        public ICommand                         CmdSongShuffle          { get; private set; }

        // More complicate properties that can't be auto-implemented.

        public String NameLength
        {
            get { return this._nameLength; }
            set { this._nameLength = value; OnPropertyChanged("NameLength"); }
        }

        public Boolean IsPlaying
        {
            get { return this._isPlaying; } 
            set { this._isPlaying = value; OnPropertyChanged("IsPlaying"); } 
        }

        public Int32 SongPosition 
        {
            get { return this._songPosition; }
            set { this._songPosition = value; OnPropertyChanged("SongPosition"); }
        }

        public Int32 SongIndex
        {
            get { return this._songIndex; }
            set 
            {
                SetSong(value);
            }
        }

        public ObservableCollection<AudioFile> LibraryList
        {
            get { return this._libraryList; }
        }

        public ObservableCollection<AudioFile> PlayList
        {
            get { return this._playList; }
        }

        // Function ///////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowVM()
        {
            Int32 index;

            CmdNext        = new CommandHandler(null,                this.ExeNext);
            CmdPlayPause   = new CommandHandler(null,                this.ExePlayPause);
            CmdPrev        = new CommandHandler(null,                this.ExePrev);
            CmdStop        = new CommandHandler(null,                this.ExeStop);
            CmdArtistAdd   = new CommandHandler(this.CanLibSel1,     this.ExeArtistAdd);
            CmdAlbumAdd    = new CommandHandler(this.CanLibSel1,     this.ExeAlbumAdd);
            CmdSongAdd     = new CommandHandler(this.CanLibSel1OrN,  this.ExeSongAdd);
            CmdSongRemove  = new CommandHandler(this.CanPlaySel1OrN, this.ExeSongRemove);
            CmdSongShuffle = new CommandHandler(this.CanPlay2OrN,    this.ExeSongShuffle);

            this._audioFileList = new AudioFileList("\\\\nastasha\\public\\devnetD\\pmusic");

            this._libraryList = new ObservableCollection<AudioFile>();
            for (index = 0; index < this._audioFileList.FileList.Count; index++)
            { 
                this._libraryList.Add(this._audioFileList.FileList[index]);
            }

            this._playList = new ObservableCollection<AudioFile>();

            this._wmPlayer       = new WMPLib.WindowsMediaPlayer();

            this._timer          = new Timer();
            this._timer.Tick    += new EventHandler(TimerTick);
            this._timer.Interval = 100;
            this._timer.Start();
        }

        /// <summary>
        /// Command execution functions.
        /// </summary>
        /// <param name="param"></param>
        private void ExeNext(Object param)
        {
            Boolean playTemp;

            playTemp = IsPlaying;

            // Stop playing
            if (IsPlaying || IsPaused)
            {
                ExeStop(null);
            }

            // Change the song.
            SongIndex = SongIndex + 1;

            // Resume playing
            if (playTemp)
            {
                ExePlayPause(null);
            }
        }

        private void ExePlayPause(Object param)
        {
            // No song to play.
            if (_songAudioFile == null)
            {
                return;
            }

            // We are playing
            if (!IsPlaying && IsPaused)
            {
                IsPaused  = false;
                IsPlaying = true;

                _wmPlayer.controls.play();
            }
            else if (!IsPlaying && !IsPaused)
            {
                IsPaused     = false;
                IsPlaying    = true;
                SongPosition = 0;

                _wmPlayer.URL = this.NameFile;
                _wmPlayer.controls.play();
            }
            else if (IsPlaying)
            {
                IsPlaying = false;
                IsPaused  = true;

                _wmPlayer.controls.pause();
            }
        }

        private void ExePrev(Object param)
        {
            Boolean playTemp;

            playTemp = IsPlaying;

            // Stop playing
            if (IsPlaying || IsPaused)
            {
                ExeStop(null);
            }

            // Change the song.
            SongIndex = SongIndex - 1;

            // Resume playing
            if (playTemp)
            {
                ExePlayPause(null);
            }
        }

        private void ExeStop(Object param)
        {
            IsPlaying = false;
            IsPaused  = false;

            _wmPlayer.controls.stop();
        }

        private Boolean CanLibSel1(Object parameter)
        {
            if (_libraryListSelected == null)
            {
                return false;
            }
            return _libraryListSelected.Count == 1;
        }

        private Boolean CanLibSel1OrN(Object parameter)
        {
            if (_libraryListSelected == null)
            {
                return false;
            }
            return _libraryListSelected.Count >= 1;
        }

        private Boolean CanPlaySel1OrN(Object parameter)
        {
            if (_playListSelected == null)
            {
                return false;
            }
            return _playListSelected.Count >= 1;
        }

        private Boolean CanPlay2OrN(Object parameter)
        {
            if (_playList == null)
            {
                return false;
            }
            return _playList.Count >= 2;
        }

        private void ExeArtistAdd(Object parameter)
        {
            AudioFile selected;
            Int32     index;
            Int32     count;
            Boolean   IsAddingToEmpty;

            IsAddingToEmpty = (_playList.Count == 0);

            selected = _libraryListSelected[0];

            count = _libraryList.Count;
            for (index = 0; index < count; index++)
            {
                if (selected.NameArtist.Equals(_libraryList[index].NameArtist))
                {
                    _playList.Add(_libraryList[index]);
                }
            }

            OnPropertyChanged("PlayList");

            if (IsAddingToEmpty)
            {
                SetSong(0);
            }
        }

        private void ExeAlbumAdd(Object parameter)
        {
            AudioFile selected;
            Int32     index;
            Int32     count;
            Boolean   IsAddingToEmpty;

            IsAddingToEmpty = (_playList.Count == 0);

            selected = _libraryListSelected[0];

            count = _libraryList.Count;
            for (index = 0; index < count; index++)
            {
                if (selected.NameArtist.Equals(_libraryList[index].NameArtist) &&
                    selected.NameAlbum.Equals( _libraryList[index].NameAlbum))
                {
                    _playList.Add(_libraryList[index]);
                }
            }

            if (IsAddingToEmpty)
            {
                SetSong(0);
            }
        }

        private void ExeSongAdd(Object parameter)
        {
            Int32 index;
            Int32 count;
            Boolean   IsAddingToEmpty;

            IsAddingToEmpty = (_playList.Count == 0);

            count = _libraryListSelected.Count;
            for (index = 0; index < count; index++)
            {
                _playList.Add(_libraryListSelected[index]);
            }

            OnPropertyChanged("PlayList");

            if (IsAddingToEmpty)
            {
                SetSong(0);
            }
        }

        private void ExeSongRemove(Object parameter)
        {
            Int32 index;
            Int32 count;
            List<AudioFile> ltemp;

            // When we modify the _playList, _playListSelected is volatile.
            // We need to make a copy.
            ltemp = new List<AudioFile>();

            count = _playListSelected.Count;
            for (index = 0; index < count; index++)
            {
                ltemp.Add(_playListSelected[index]);
            }

            for (index = 0; index < count; index++)
            {
                _playList.Remove(ltemp[index]);
            }

            OnPropertyChanged("PlayList");

            // Stop playing when things are out of sync or empty.
            if (SongIndex >= _playList.Count ||
                (_songAudioFile != null &&
                 !_songAudioFile.FileName.Equals(_playList[SongIndex].FileName)))
            {
                ExeStop(null);
                SetSong(0);
            }

        }

        private void ExeSongShuffle(Object parameter)
        {
            List<AudioFile> playList;
            Int32           index;
            Int32           count;
            Int32           songIndex;
            Random          random;

            ExeStop(null);
            
            // Get the current play list.
            playList = new List<AudioFile>();
            count    = _playList.Count;
            for (index = 0; index < count; index++)
            {
                playList.Add(_playList[index]);
            }

            // Clear the play list.
            _playList.Clear();

            // Randomly repopulate the play list.
            random = new Random((Int32) DateTime.Now.Ticks & 0xFFFF);
            for (index = 0; index < count; index++)
            {
                songIndex = random.Next(playList.Count);
                
                _playList.Add(playList[songIndex]);

                playList.RemoveAt(songIndex);
            }

            OnPropertyChanged("PlayList");

            SetSong(0);
        }

        /// <summary>
        /// Update the ui.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            Int32 itemp;

            SongPosition = 0;
            if (_wmPlayer              != null &&
                _wmPlayer.currentMedia != null)
            { 
                SongPosition = (Int32) ((_wmPlayer.controls.currentPosition / _wmPlayer.currentMedia.duration) * 1000.0);

                if (!_wmPlayer.currentMedia.durationString.Equals(NameLength))
                { 
                    NameLength = _wmPlayer.currentMedia.durationString;
                }

                // We have come to the end.
                if (_wmPlayer.currentMedia.duration != 0 &&
                    _wmPlayer.controls.currentPosition == _wmPlayer.currentMedia.duration)
                {
                    // Move to the next song in the play list.
                    if (IsPlaying)
                    { 
                        // For checking if there are any more songs.
                        itemp = SongIndex;

                        // Move to the next song.
                        SetSong(SongIndex + 1);

                        // SongIndex changed so there are more songs in the list.
                        if (itemp != SongIndex)
                        {
                            // Play the new song.
                            ExePlayPause(null);
                        }
                    }
                }
            }
        }

        private void SetSong(Int32 value)
        {
            // Set the song index;
            _songIndex = Math.Min(_playList.Count - 1, Math.Max(value, 0));

            // Set the song audio file.
            _songAudioFile = null;
            if (_playList.Count > 0)
            {
                _songAudioFile = _playList[SongIndex];
            }

            // Update the ui.
            OnPropertyChanged("NameAlbum"); 
            OnPropertyChanged("NameArtist");
            OnPropertyChanged("NameFile");
            OnPropertyChanged("NameFileImage");
            OnPropertyChanged("NameSong");
            OnPropertyChanged("NameTrack");
            OnPropertyChanged("NameLength");
        }

        public void LibraryListSelChange(Object sender, EventArgs e)
        {
            DataGrid dgrid;
            Int32    count;
            Int32    index;
            
            dgrid = sender as DataGrid;
            count = dgrid.SelectedItems.Count;

            _libraryListSelected = new List<AudioFile>();
            for (index = 0; index < count; index++)
            {
                _libraryListSelected.Add(dgrid.SelectedItems[index] as AudioFile);
            }
        }

        public void PlayListSelChange(Object sender, EventArgs e)
        {
            DataGrid dgrid;
            Int32    count;
            Int32    index;
            
            dgrid = sender as DataGrid;
            count = dgrid.SelectedItems.Count;

            _playListSelected = new List<AudioFile>();
            for (index = 0; index < count; index++)
            {
                _playListSelected.Add(dgrid.SelectedItems[index] as AudioFile);
            }
        }

        // INotifyPropertyChanged /////////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
