using System;
using System.Collections.Generic;
using System.Text;

namespace ZMuse.Model
{
    public class AudioFile
    {
        // Variable/Property //////////////////////////////////////////////////////////////////////

        public String FileName      { get; set; }
        public String FileNameImage { get; set; }
        public String NameAlbum     { get; set; }
        public String NameArtist    { get; set; }
        public String NameSong      { get; set; }
        public Int32  Track         { get; set; }
        public String NameTrack     { get { return Track.ToString("D" + 2); } }

        // Function ///////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName"></param>
        public AudioFile(String fileName)
        {
            String[] fileNameParts;
            Int32      itemp;

            this.FileName = fileName;

            fileNameParts = this.FileName.Split("_");
                                           this.NameArtist = fileNameParts[0];
            this.NameAlbum = "-";
            if (fileNameParts.Length >= 2) this.NameAlbum  = fileNameParts[1];
            this.NameSong  = "-";
            if (fileNameParts.Length >= 4) this.NameSong   = fileNameParts[3];

            this.Track = 0;
            if (fileNameParts.Length >= 3 &&
                Int32.TryParse(fileNameParts[2], out itemp))
            {
                this.Track = itemp;
            }

            // Figure out the image file.
            this.FileNameImage = this.NameArtist + "_" + this.NameAlbum + ".jpg";

            // Trim off the path from the Artist name.
            itemp           = this.NameArtist.LastIndexOf('\\');
            this.NameArtist = this.NameArtist.Substring(itemp + 1);

            // Trim the extension from the song name.
            itemp           = this.NameSong.LastIndexOf('.');
            if (itemp > 0)
            { 
                this.NameSong = this.NameSong.Substring(0, itemp);
            }
        }
    }
}
