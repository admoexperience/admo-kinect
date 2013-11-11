using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Timers;
using NLog;
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
        private String _podFile;
        private readonly String _podDestFolder;

        private Timer _fileChangedTimer;
        private Timer _podChangedTimer;

        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();


        public event PodDataCahnged Changed;
        public delegate void PodDataCahnged(string podFolder);

        public PodWatcher(String podFile, String podDestFolder)
        {
            _podFile = podFile;
            _podDestFolder = podDestFolder;
        }


        //Called when config is changed
        public void OnConfigChange()
        {
            if (_podFile == Config.GetPodFile()) return;
            Logger.Debug("Pod file changed from ["+_podFile+"] to ["+Config.GetPodFile()+"]");
            _podFile = Config.GetPodFile();
            //Restart the watchers and reload the content
            StopWatchers();
            StartWatcher();
            SocketServer.SendReloadEvent();
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
            _watchers.Add(podWatcher);
            // Add event handlers.
            podWatcher.Changed += OnPodChanged;
            podWatcher.Created += OnPodChanged;
            // Begin watching.
            podWatcher.EnableRaisingEvents = true;
            Logger.Debug("Watching for changes to " + _podFile);
            
            //It should try unzip all the pod files in the directory. (theortically there should only be one)
            TryUnzipPodInto(_podFile, Path.Combine(_podDestFolder, "current"));

            // Create a new FileSystemWatcher for pods dir
            AddDestWatcher(_podDestFolder);
            AddFileWatchersIncludingSymlinks(_podDestFolder);
           

            _fileChangedTimer  = new Timer {AutoReset = false, Interval = 2000};
            _fileChangedTimer.Elapsed += FileTimerElapsed;

            _podChangedTimer = new Timer { AutoReset = false, Interval = 2000 };
            _podChangedTimer.Elapsed += PodTimerElapsed;
        }

        private void AddFileWatchersIncludingSymlinks(string path)
        {
            var directory = new DirectoryInfo(path);
            foreach (var file in directory.GetDirectories())
            {
                var checkPath = file.FullName;
                var attributes = File.GetAttributes(checkPath);
                // Now, check whether directory is Reparse point or symbolic link
                if (attributes == (FileAttributes.Directory | FileAttributes.ReparsePoint))
                {
                    //Update the checkpath to the reference for the symbolic link
                    var newLink = JunctionPoint.GetTarget(checkPath);
                    Logger.Debug("Found symbolic link " + checkPath + "==> " + newLink);
                    if (Directory.Exists(newLink))
                    {
                        AddDestWatcher(newLink);
                    }
                    else
                    {
                        Logger.Warn("["+newLink+"] Doesn't exsist not adding it to the watch list");
                    }
                }
                else
                {
                    AddFileWatchersIncludingSymlinks(checkPath);
                }
            }
        }

        private void AddDestWatcher(string path)
        {
            //Add a watcher for each sub folder or the source of the link
            var destFolderWatcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            Logger.Debug("Adding file watcher for " + path);
            // Add event handlers.
            destFolderWatcher.Changed += OnWebSiteContentChanged;
            destFolderWatcher.Created += OnWebSiteContentChanged;
            destFolderWatcher.EnableRaisingEvents = true;
            _watchers.Add(destFolderWatcher);
        }

        //Stops and removes all the current watchers
        public void StopWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();
        }


        private void FileTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Changed != null) Changed(_podDestFolder);
        }

        private void PodTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TryUnzipPodInto(_podFile, Path.Combine(_podDestFolder, "current"));
        }

        // Define the event handlers. 
        private void OnWebSiteContentChanged(object source, FileSystemEventArgs e)
        {
            //Ignore tvr files
            if (e.FullPath.EndsWith(".tvrdat")) return;
            //Basically forces the "onChange" Event to only happen once.
            if (_fileChangedTimer == null) return;
            _fileChangedTimer.Stop();
            _fileChangedTimer.Start();
        }

        // Define the event handlers. 
        private void OnPodChanged(object source, FileSystemEventArgs e)
        {
            //Ignore tvr files
            if (e.FullPath.EndsWith(".tvrdat")) return;
            _podChangedTimer.Stop();
            _podChangedTimer.Start();
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
                using (var archive = ZipFile.Open(file, ZipArchiveMode.Read))
                {
                    //Try read the entries, should fail if the zip isn't fully completed copying
                    var x = archive.Entries;
                    Logger.Debug("Clearning the contents out of [" + destFolder + "]");
                    var dir = new DirectoryInfo(destFolder);
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }
                    ClearDirectory(dir);
                    Logger.Debug("Extracting [" + file + "] to [" + destFolder + "]");
                    archive.ExtractToDirectory(destFolder);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to unzip " + file + " " + e);
            }
        }
    }
}