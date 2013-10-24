using System;
using System.IO;
using System.Linq;

/**
 * Class used to store cache of stats objects before we process them.
 */

namespace Admo.classes.stats
{
    public class DataCache
    {
        private readonly String _folder;

        private const String CacheExtention = ".analytics";

        public DataCache(String folder)
        {
            _folder = folder;
        }

        public Boolean Exists()
        {
            return Directory.Exists(_folder);
        }


        public void InsertData(String data)
        {
            var filename = Utils.AsEpocTime(DateTime.UtcNow);
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            var file = Path.Combine(_folder, filename + CacheExtention);
            //If 2 processes are called with in the same time timeframe handle 
            //duplicate files
            var counter = 1;
            while (File.Exists(file))
            {
                file = Path.Combine(_folder, filename + '-' + counter + CacheExtention);
                counter++;
            }
            File.WriteAllText(file, data);
        }

        public int GetRowCount()
        {
            if (!Directory.Exists(_folder)) return 0;
            var files = Directory.GetFiles(_folder, "*", SearchOption.TopDirectoryOnly);
            return files.Length;
        }

        public String PopData()
        {
            var directory = new DirectoryInfo(_folder);
            if (directory.Exists)
            {
                var file = directory.GetFiles().OrderBy(f => f.CreationTime).First();

                //There is no values left return empty array
                if (!file.Exists) return String.Empty;

                var contents = File.ReadAllText(file.FullName);
                file.Delete();
                return contents;
            }
            return String.Empty;
        }
    }
}