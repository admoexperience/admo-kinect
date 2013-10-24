using System;
using System.IO;

/**
 * Class used to store cache of stats objects before we process them.
 */

namespace Admo.classes.stats
{
    public class DataCache
    {
        private readonly String _cacheFile;

        public DataCache(String file)
        {
            _cacheFile = file;
        }

        public Boolean Exists()
        {
            return File.Exists(_cacheFile);
        }

        public void CreateDataCache()
        {
            
        }

        public void InsertData(String data)
        {
            
        }

        public int GetRowCount()
        {
            return 0;
        }

        public String PopData()
        {
            return "";
        }

    }
}