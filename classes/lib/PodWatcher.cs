using System;
using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
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
        private readonly String _podSrcFolder;
        private readonly String _podDestFolder;

        public event PodDataCahnged Changed;

        public PodWatcher(String podSrcFolder, String podDestFolder)
        {
            _podSrcFolder = podSrcFolder;
            _podDestFolder = podDestFolder;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void StartWatcher()
        {
            var podExt = ".zip";
            // Create a new FileSystemWatcher and set its properties.
            var watcher = new FileSystemWatcher
                {
                    Path = _podSrcFolder,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Filter = "*" + podExt
                };

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            Logger.Debug("Watching for changes to " + _podSrcFolder);
            
            //It should try unzip all the pod files in the directory. (theortically there should only be one)
            var files = Directory.GetFiles(_podSrcFolder, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(podExt));
            foreach (var file in files)
            {
                TryUnzipPodInto(file,_podDestFolder);   
            }
        }

        // Define the event handlers. 
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Logger.Debug("File: " + e.FullPath + " " + e.ChangeType);
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                TryUnzipPodInto(e.FullPath, this._podDestFolder);
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Logger.Debug("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        private static void Empty(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles()) file.Delete();
            foreach (var subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }


        public delegate void PodDataCahnged(string podFolder);

        private void TryUnzipPodInto(String file, String destFolder)
        {
            try
            {
                Logger.Info("Clearning the contents out of ["+destFolder+"]");
                Empty(new DirectoryInfo(destFolder));
                
                Logger.Debug("Extracting ["+file+"] to ["+destFolder+"]");
                ZipFile.ExtractToDirectory(file, destFolder);
                if (Changed != null) Changed(destFolder);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to unzip "+file, e);
            }

        }
    }
}