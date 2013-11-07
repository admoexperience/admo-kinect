using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Admo.Api.Dto;
using Admo.Utilities;
using NLog;

namespace Admo.Api
{
    public class PodDownloader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public readonly String Location;

        public PodDownloader(string location)
        {
            Location = location;
        }

        public async Task<String> Download(List<PodApp> pods)
        {
            Logger.Debug("Found ["+pods.Count+"] pod files");
            foreach (var podApp in pods)
            {
                Logger.Debug("Proccessing ["+ podApp.PodUrl+" with ["+podApp.PodChecksum+"]");
                var url = podApp.PodUrl;
                //Figure out the files name from the url. 
                // for http://hostname.com/path/foo/filename.pod.zip
                var uri = new Uri(url);
                var segment = uri.Segments.Last();
                var fileName = Path.Combine(Location, segment);
                var checkSum = podApp.PodChecksum;
                //Basically if the file is already there
                //And the checksums already match do nothing.
                if (File.Exists(fileName) && checkSum.Equals(Utils.Sha256(fileName)))
                {
                    Logger.Debug("Pod file is there and checksum matches");
                    continue;
                }
                Logger.Debug("Proccessing  [" +fileName + "]");
                Logger.Debug("Proccessing  [" + podApp.PodChecksum + "]");

                Logger.Debug("Downloading");
                //TODO: Be able to test this.
                var httpClient = new HttpClient();

                var responseMessage = await httpClient.GetAsync(
                           url,
                           HttpCompletionOption.ResponseHeadersRead);  // the essential magic
                Logger.Debug("Http result ");
                using (var fileStream = File.Create(fileName))
                {
                    using (var httpStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        Logger.Debug("Streaming to disk");
                        httpStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
                Logger.Debug("Finished downloading filename [" +fileName+"] ["+Utils.Sha256(fileName)+"]");
            }
            return "asf";
        }
    }
}
