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
using NLog;

namespace Admo.Api
{
    public class PodDownloader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public String Location { set; get; }
        public List<PodApp> Pods {set; get; }

        public async Task<String> Download()
        {
            foreach (var podApp in Pods)
            {
                
                var url = podApp.PodUrl;
                Logger.Debug("Downloading ..  " + url);
                var uri = new Uri(url);
                var segment = uri.Segments.Last();
                var fileName = Path.Combine(Location, segment);
                var httpClient = new HttpClient();
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);


                using (
                        Stream contentStream = await (await httpClient.SendAsync(requestMessage)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 2
                , true))
                {
                    await contentStream.CopyToAsync(stream);
                }
                Logger.Debug("Finished writing filename " +fileName);
                Logger.Debug(new FileInfo(fileName).Length);
                using (var stream = File.OpenRead(fileName))
                {
                    var sha = new SHA256Managed();
                    byte[] hash = sha.ComputeHash(stream);
                    
                    var shaHash = BitConverter.ToString(hash).Replace("-", String.Empty);
                    Logger.Debug(shaHash);
                }
            }
            return "asf";
        }
    }
}
