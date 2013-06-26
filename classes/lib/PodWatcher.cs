using System;
using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Timers;
using NLog;
using System.IO;
using System.IO.Compression;

/**
 * Class used to watch for changes to the pod file, on change will unzip the pod into the required location
 * it will also fire events to let others know the data has changed
 */
namespace Admo.classes.lib
{
    internal class PodWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly String _podFile;
        private readonly String _podDestFolder;

        private Timer _fileChangedTimer;
        private Timer _podChangedTimer;


        public event PodDataCahnged Changed;
        public delegate void PodDataCahnged(string podFolder);

        public PodWatcher(String podFile, String podDestFolder)
        {
            _podFile = podFile;
            _podDestFolder = podDestFolder;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void StartWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            var podWatcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(_podFile), 
                    Filter = Path.GetFileName(_podFile),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

            // Add event handlers.
            podWatcher.Changed += OnChanged;
            podWatcher.Created += OnChanged;
            // Begin watching.
            podWatcher.EnableRaisingEvents = true;
            Logger.Debug("Watching for changes to " + _podFile);
            
            //It should try unzip all the pod files in the directory. (theortically there should only be one)
            TryUnzipPodInto(_podFile, _podDestFolder + "current");

            // Create a new FileSystemWatcher and set its properties.
            var destFolderWatcher = new FileSystemWatcher
            {
                Path = _podDestFolder,
                Filter = "*",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            Logger.Debug("Watching for changes to " + _podDestFolder);

            // Add event handlers.
            destFolderWatcher.Changed += OnWebSiteContentChanged;
            destFolderWatcher.Created += OnWebSiteContentChanged;
            destFolderWatcher.EnableRaisingEvents = true;

            _fileChangedTimer  = new Timer {AutoReset = false, Interval = 2000};
            _fileChangedTimer.Elapsed += FileTimerElapsed;

            _podChangedTimer = new Timer { AutoReset = false, Interval = 2000 };
            _podChangedTimer.Elapsed += PodTimerElapsed;
        }

        private void FileTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Changed != null) Changed(_podDestFolder);
        }

        private void PodTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TryUnzipPodInto(_podFile, _podDestFolder + "current");
        }

        // Define the event handlers. 
        private void OnWebSiteContentChanged(object source, FileSystemEventArgs e)
        {
            //Basically forces the "onChange" Event to only happen once.
            _fileChangedTimer.Stop();
            _fileChangedTimer.Start();
        }

        // Define the event handlers. 
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Logger.Debug("File: " + e.FullPath + " " + e.ChangeType);
            //Basically forces the "onChange" Event to only happen once.
            _fileChangedTimer.Stop();
            _fileChangedTimer.Start();
        }


        private static void ClearDirectory(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles()) file.Delete();
            foreach (var subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }


        //Tries
        private void TryUnzipPodInto(String file, String destFolder)
        {
            try
            {
                Logger.Debug("Clearning the contents out of ["+destFolder+"]");
                var dir = new DirectoryInfo(destFolder);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                //For now we are not cleaning out the older content.
                ClearDirectory(dir);
                //We need a way of deleting diffs
                
                Logger.Debug("Extracting ["+file+"] to ["+destFolder+"]");
                ZipFile.ExtractToDirectory(file, destFolder);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to unzip "+file+" "+e.ToString());
            }

        }
    }
}