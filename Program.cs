using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Security.Permissions;

namespace SeekAndArchive
{
    class Program
    {
        private static List<FileInfo> _foundFiles;
        private static List<FileSystemWatcher> _watchers;
        private static List<DirectoryInfo> _archiveDirs;

        static void Main(string[] args)
        {
            string searchedFileName = args[0];
        
            //string directoryPath = @"C:\Users\doram\OneDrive\Desktop\külsőMerevlemezről\gabortol";
            DirectoryInfo currentDirectory = new DirectoryInfo(args[1]);
            _foundFiles = new List<FileInfo>();
            _watchers = new List<FileSystemWatcher>();

            try
            {
                if (!currentDirectory.Exists)
                {
                    Console.WriteLine("The specified directory does not exist.");
                    Console.ReadKey();
                    return;
                }
                RecursiveSearch(currentDirectory, _foundFiles, searchedFileName);

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            foreach (FileInfo file in _foundFiles)
            {
                Console.WriteLine(file.ToString());
            }
            
            if (_foundFiles.Count > 0)
            {
                AddWatcher();
                ArchiveFoundFiles();
            }
            Console.Read();
        }

        private static void RecursiveSearch(DirectoryInfo currentDirectory, List<FileInfo> foundFiles, string searchedFileName)
        {
            foreach (FileInfo file in currentDirectory.GetFiles())
            {
                if (searchedFileName == file.Name)
                    foundFiles.Add(file);
            }
            foreach (DirectoryInfo dir in currentDirectory.GetDirectories())
            {
                RecursiveSearch(dir, foundFiles, searchedFileName);
            }
        }


        private static void AddWatcher()
        {
            foreach (FileInfo file in _foundFiles)
            {
                FileSystemWatcher newWatcher = new FileSystemWatcher(file.DirectoryName, file.Name);
                //WatcherChanged() method is the param of FileSystemEventHandler !
                newWatcher.Changed += new FileSystemEventHandler(WatcherChanged);

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                newWatcher.NotifyFilter = NotifyFilters.LastAccess
                     | NotifyFilters.LastWrite
                     | NotifyFilters.FileName
                     | NotifyFilters.DirectoryName;

                // Add event handlers.
                newWatcher.Changed += OnChanged;
                newWatcher.Created += OnChanged;
                newWatcher.Deleted += OnChanged;
                newWatcher.Renamed += OnRenamed;

                // Begin watching.
                newWatcher.EnableRaisingEvents = true;
                _watchers.Add(newWatcher);

                Console.WriteLine($"{file.FullName} file is watched.");

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit the sample.");
                while (Console.Read() != 'q') ;
            }
        }

        private static void ArchiveFoundFiles()
        {
            _archiveDirs = new List<DirectoryInfo>();

            //create archive directories 
            for (int i = 0; i < _foundFiles.Count; i++)
            {
                _archiveDirs.Add(Directory.CreateDirectory($"archive {i}"));
                Console.WriteLine("Archive directory created");
            }
        }

        static void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"{e.FullPath} has been changed!");

            //find the the index of the changed file 
            FileSystemWatcher senderWatcher = (FileSystemWatcher)sender;
            int index = _watchers.IndexOf(senderWatcher, 0);

            //now that we have the index, we can archive the file 
            ArchiveFile(_archiveDirs[index], _foundFiles[index]);
        }

        static void ArchiveFile(DirectoryInfo archiveDir, FileInfo fileToArchive)
        {
            FileStream input = fileToArchive.OpenRead();
            FileStream output = File.Create(archiveDir.FullName + @"\" + fileToArchive.Name + ".gz");

            //Compressing files
            GZipStream Compressor = new GZipStream(output, CompressionMode.Compress);

            int currentByte = input.ReadByte();

            while (currentByte != -1)
            {
                Compressor.WriteByte((byte)currentByte);
                currentByte = input.ReadByte();
            }
            Console.WriteLine("File archived!");
            Compressor.Close();
            input.Close();
            output.Close();
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e) =>
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");

    }

}
