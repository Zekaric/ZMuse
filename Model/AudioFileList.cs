using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ZMuse.Model
{
    class AudioFileList
    {
        // Variable/Property //////////////////////////////////////////////////////////////////////
        private List<AudioFile> _FileList  = new List<AudioFile>();

        public List<AudioFile> FileList
        {
            get { return _FileList; }
        }

        // Function ///////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor
        /// </summary>
        public AudioFileList(String AudioDir)
        {
            // Get the song files.
            try
            { 
                _ProcessDir(AudioDir);
            }
            catch
            {
                #if DEBUG
                Debug.WriteLine("Failure to read directory.");
                #endif
            }

            // Sort the list.
            FileList.Sort();
        }

        /// <summary>
        /// Add a new file to the list.
        /// </summary>
        /// <param name="file"></param>
        public void Add(AudioFile file)
        {
            this.FileList.Add(file);
        }

        /// <summary>
        /// Process a directory of files and sub directories.
        /// </summary>
        /// <param name="dir"></param>
        private void _ProcessDir(String dir)
        {
            String   dirName;
            String[] dirList;
            Int32    dirListCount;
            Int32    dirIndex;

            String   fileName;
            String[] fileList;
            Int32    fileListCount;
            Int32    fileIndex;

            // Build up the file list from the directory.
            dirList = Directory.GetDirectories(dir);
            dirListCount = dirList.Length;

            // For all directories...
            for (dirIndex = 0; dirIndex < dirListCount; dirIndex++)
            {
                dirName = dirList[dirIndex];

                // Recurse to add to the list.
                _ProcessDir(dirName);
            }

            // Get the MP3 files in the directory.
            fileList = Directory.GetFiles(dir, "*.mp3");
            fileListCount = fileList.Length;

            // For all files...
            for (fileIndex = 0; fileIndex < fileListCount; fileIndex++)
            {
                fileName = fileList[fileIndex];

                _FileList.Add(new AudioFile(fileName));
            }

            // Get the WAV files in the directory.
            fileList = Directory.GetFiles(dir, "*.wav");
            fileListCount = fileList.Length;

            // For all files...
            for (fileIndex = 0; fileIndex < fileListCount; fileIndex++)
            {
                fileName = fileList[fileIndex];

                _FileList.Add(new AudioFile(fileName));
            }
        }
    }
}
