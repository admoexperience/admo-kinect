using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NLog;

namespace Admo.classes
{
    public class WebServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static string Port = "5001";
        private readonly Thread _listenThread;
        private readonly HttpListener _listener;

        private readonly string _overridePath;
        private readonly string _currentPath;

        public WebServer(string baseLocation)
        {
            //Folders need to exsist before
            _overridePath = Path.Combine(baseLocation, "override");
            _currentPath = Path.Combine(baseLocation, "current");
            var address = "https://+:" + Port + "/";
            // setup thread
            _listenThread = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Normal };

            // setup listener
            _listener = new HttpListener();
            _listener.Prefixes.Add(address);
        }

        public void Start()
        {
            _listener.Start();
            _listenThread.Start();
        }
           public void Run()
        {
            Logger.Info("Starting webserver");
           
           // http://www.codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server.aspx
               while (_listener.IsListening)
               {
                   try
                   {
                       var context = _listener.GetContext();
                       ThreadPool.QueueUserWorkItem((c) => ProcessRequest(context.Request, context));
                   }
                   catch (Exception)
                   {
                     Logger.Error("Unable to process request");
                   }
                      
               }
              
        }

        private void ProcessRequest(HttpListenerRequest request, HttpListenerContext context)
        {
            /* respond to the request.
            * in this case it'll show "Server appears to be working".
            * regardless of what file/path was requested.
            */

            var myRequest = request.Url.AbsolutePath;

            if (myRequest.StartsWith("/"))
            {
                myRequest = myRequest.Remove(0, 1);
            }

            if (myRequest.Contains("favicon.ico"))
            {
              FileNotFound(context);
               
                return; 
            }
            //First try find it in the overide folder
            var myPath = Path.Combine(_overridePath, myRequest);

            //If the file was not overriden

            if (!File.Exists(myPath))
            {
                myPath = Path.Combine(_currentPath, myRequest);
            }

            if (request.RawUrl.EndsWith("/") || Directory.Exists(myPath))
            {
                myPath += "/index.html";
            }

            var mimeExtension = GetMimeType(myPath);
            var response = context.Response;

            //try again
            if (!File.Exists(myPath))
            {
                FileNotFound(context);

                Logger.Debug("unable to load file file does not exist " + myPath + " ");
                return;
            }

            try
            {

                var fi = new FileInfo(myPath);

                var size = (int)fi.Length;
                //fi.Delete();

                using (var fs = File.OpenRead(myPath))
                {
                    response.ContentType = mimeExtension;

                    //Section required for streaming content
                    var range = context.Request.Headers["Range"];
                    var rangeBegin = 0;
                    var rangeEnd = size;
                    if (range != null)
                    {
                        var byteRange = range.Replace("bytes=", "").Split('-');
                        Int32.TryParse(byteRange[0], out rangeBegin);
                        //byte range can contain an empty which means to the end
                        if (byteRange.Length > 1 && !string.IsNullOrEmpty(byteRange[1]))
                        {
                            Int32.TryParse(byteRange[1], out rangeEnd);
                        }
                        context.Response.AddHeader("Connection", "keep-alive");
                        context.Response.StatusCode = (int) HttpStatusCode.PartialContent;
                        context.Response.AddHeader("Accept-Ranges", "bytes");
                    }


                    int read;
                    var totalRead = 0;

                    var lenghtToRead = 64 * 1024;
                    response.ContentLength64 = rangeEnd - rangeBegin;
                    if ((totalRead + 64 * 1024) > rangeEnd - rangeBegin)
                    {
                        lenghtToRead = rangeEnd - rangeBegin - totalRead;
                    }
                    var buffer = new byte[lenghtToRead];
                    while ( ( read = fs.Read(buffer, 0, lenghtToRead)) > 0)
                    {
                
                       response.AddHeader("Content-Range",
                            "bytes " + (rangeBegin + totalRead) + "-" + (rangeBegin + totalRead+ read - 1) + "/" + size);
                       totalRead += read;

                        if ((totalRead + buffer.Length) > rangeEnd - rangeBegin)
                        {
                            lenghtToRead = rangeEnd - rangeBegin - totalRead;
                        }

                        try
                        {
                            response.OutputStream.Write(buffer, 0, read);
                            response.OutputStream.Flush(); 
                        }
                        catch (Exception e)
                        {

                        }

                    }
                fs.Close();
                }
                
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                Logger.Debug("Unable to server file" + myPath + e);
            }

            
        }

        private void FileNotFound( HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            string html = "<b>File not found Error 404!</b>";
            byte[] data = Encoding.UTF8.GetBytes(html);

            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = data.Length;

            using (Stream output = context.Response.OutputStream)
            {
                output.Write(data, 0, data.Length);
            }
        }


        public void Close()
        {
       
            _listener.Stop();

            _listener.Abort();
            _listener.Close();
        }

        private static string GetMimeType(string fileName)
        {
            //get file extension
            var s = Path.GetExtension(fileName);
            if (s != null)
            {
                var extension = s.ToLowerInvariant().Remove(0, 1);

                if (MimeTypesDictionary.ContainsKey(extension))
                {
                    return MimeTypesDictionary[extension];
                }
            }
            return "unknown/unknown";
        }

        private static readonly Dictionary<string, string> MimeTypesDictionary = new Dictionary<string, string>
            {
                {"ai", "application/postscript"},
                {"aif", "audio/x-aiff"},
                {"aifc", "audio/x-aiff"},
                {"aiff", "audio/x-aiff"},
                {"asc", "text/plain"},
                {"atom", "application/atom+xml"},
                {"au", "audio/basic"},
                {"avi", "video/x-msvideo"},
                {"bcpio", "application/x-bcpio"},
                {"bin", "application/octet-stream"},
                {"bmp", "image/bmp"},
                {"cdf", "application/x-netcdf"},
                {"cgm", "image/cgm"},
                {"class", "application/octet-stream"},
                {"cpio", "application/x-cpio"},
                {"cpt", "application/mac-compactpro"},
                {"csh", "application/x-csh"},
                {"css", "text/css"},
                {"dcr", "application/x-director"},
                {"dif", "video/x-dv"},
                {"dir", "application/x-director"},
                {"djv", "image/vnd.djvu"},
                {"djvu", "image/vnd.djvu"},
                {"dll", "application/octet-stream"},
                {"dmg", "application/octet-stream"},
                {"dms", "application/octet-stream"},
                {"doc", "application/msword"},
                {"docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
                {"docm", "application/vnd.ms-word.document.macroEnabled.12"},
                {"dotm", "application/vnd.ms-word.template.macroEnabled.12"},
                {"dtd", "application/xml-dtd"},
                {"dv", "video/x-dv"},
                {"dvi", "application/x-dvi"},
                {"dxr", "application/x-director"},
                {"eps", "application/postscript"},
                {"etx", "text/x-setext"},
                {"exe", "application/octet-stream"},
                {"ez", "application/andrew-inset"},
                {"gif", "image/gif"},
                {"gram", "application/srgs"},
                {"grxml", "application/srgs+xml"},
                {"gtar", "application/x-gtar"},
                {"hdf", "application/x-hdf"},
                {"hqx", "application/mac-binhex40"},
                {"htm", "text/html"},
                {"html", "text/html"},
                {"ice", "x-conference/x-cooltalk"},
                {"ico", "image/x-icon"},
                {"ics", "text/calendar"},
                {"ief", "image/ief"},
                {"ifb", "text/calendar"},
                {"iges", "model/iges"},
                {"igs", "model/iges"},
                {"jnlp", "application/x-java-jnlp-file"},
                {"jp2", "image/jp2"},
                {"jpe", "image/jpeg"},
                {"jpeg", "image/jpeg"},
                {"jpg", "image/jpeg"},
                {"js", "application/x-javascript"},
                {"kar", "audio/midi"},
                {"latex", "application/x-latex"},
                {"lha", "application/octet-stream"},
                {"lzh", "application/octet-stream"},
                {"m3u", "audio/x-mpegurl"},
                {"m4a", "audio/mp4a-latm"},
                {"m4b", "audio/mp4a-latm"},
                {"m4p", "audio/mp4a-latm"},
                {"m4u", "video/vnd.mpegurl"},
                {"m4v", "video/x-m4v"},
                {"mac", "image/x-macpaint"},
                {"man", "application/x-troff-man"},
                {"mathml", "application/mathml+xml"},
                {"me", "application/x-troff-me"},
                {"mesh", "model/mesh"},
                {"mid", "audio/midi"},
                {"midi", "audio/midi"},
                {"mif", "application/vnd.mif"},
                {"mov", "video/quicktime"},
                {"movie", "video/x-sgi-movie"},
                {"mp2", "audio/mpeg"},
                {"mp3", "audio/mpeg"},
                {"mp4", "video/mp4"},
                {"mpe", "video/mpeg"},
                {"mpeg", "video/mpeg"},
                {"mpg", "video/mpeg"},
                {"mpga", "audio/mpeg"},
                {"ms", "application/x-troff-ms"},
                {"msh", "model/mesh"},
                {"mxu", "video/vnd.mpegurl"},
                {"nc", "application/x-netcdf"},
                {"oda", "application/oda"},
                {"ogg", "application/ogg"},
                {"pbm", "image/x-portable-bitmap"},
                {"pct", "image/pict"},
                {"pdb", "chemical/x-pdb"},
                {"pdf", "application/pdf"},
                {"pgm", "image/x-portable-graymap"},
                {"pgn", "application/x-chess-pgn"},
                {"pic", "image/pict"},
                {"pict", "image/pict"},
                {"png", "image/png"},
                {"pnm", "image/x-portable-anymap"},
                {"pnt", "image/x-macpaint"},
                {"pntg", "image/x-macpaint"},
                {"ppm", "image/x-portable-pixmap"},
                {"ppt", "application/vnd.ms-powerpoint"},
                {"pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {"potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
                {"ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
                {"ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
                {"pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
                {"potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
                {"ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
                {"ps", "application/postscript"},
                {"qt", "video/quicktime"},
                {"qti", "image/x-quicktime"},
                {"qtif", "image/x-quicktime"},
                {"ra", "audio/x-pn-realaudio"},
                {"ram", "audio/x-pn-realaudio"},
                {"ras", "image/x-cmu-raster"},
                {"rdf", "application/rdf+xml"},
                {"rgb", "image/x-rgb"},
                {"rm", "application/vnd.rn-realmedia"},
                {"roff", "application/x-troff"},
                {"rtf", "text/rtf"},
                {"rtx", "text/richtext"},
                {"sgm", "text/sgml"},
                {"sgml", "text/sgml"},
                {"sh", "application/x-sh"},
                {"shar", "application/x-shar"},
                {"silo", "model/mesh"},
                {"sit", "application/x-stuffit"},
                {"skd", "application/x-koan"},
                {"skm", "application/x-koan"},
                {"skp", "application/x-koan"},
                {"skt", "application/x-koan"},
                {"smi", "application/smil"},
                {"smil", "application/smil"},
                {"snd", "audio/basic"},
                {"so", "application/octet-stream"},
                {"spl", "application/x-futuresplash"},
                {"src", "application/x-wais-source"},
                {"sv4cpio", "application/x-sv4cpio"},
                {"sv4crc", "application/x-sv4crc"},
                {"svg", "image/svg+xml"},
                {"swf", "application/x-shockwave-flash"},
                {"t", "application/x-troff"},
                {"tar", "application/x-tar"},
                {"tcl", "application/x-tcl"},
                {"tex", "application/x-tex"},
                {"texi", "application/x-texinfo"},
                {"texinfo", "application/x-texinfo"},
                {"tif", "image/tiff"},
                {"tiff", "image/tiff"},
                {"tr", "application/x-troff"},
                {"tsv", "text/tab-separated-values"},
                {"txt", "text/plain"},
                {"ustar", "application/x-ustar"},
                {"vcd", "application/x-cdlink"},
                {"vrml", "model/vrml"},
                {"vxml", "application/voicexml+xml"},
                {"wav", "audio/x-wav"},
                {"wbmp", "image/vnd.wap.wbmp"},
                {"webm","video/webm"},
                {"wbmxl", "application/vnd.wap.wbxml"},
                {"wml", "text/vnd.wap.wml"},
                {"wmlc", "application/vnd.wap.wmlc"},
                {"wmls", "text/vnd.wap.wmlscript"},
                {"wmlsc", "application/vnd.wap.wmlscriptc"},
                {"woff", "application/font-woff"},
                {"wrl", "model/vrml"},
                {"xbm", "image/x-xbitmap"},
                {"xht", "application/xhtml+xml"},
                {"xhtml", "application/xhtml+xml"},
                {"xls", "application/vnd.ms-excel"},
                {"xml", "application/xml"},
                {"xpm", "image/x-xpixmap"},
                {"xsl", "application/xml"},
                {"xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {"xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
                {"xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
                {"xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
                {"xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
                {"xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
                {"xslt", "application/xslt+xml"},
                {"xul", "application/vnd.mozilla.xul+xml"},
                {"xwd", "image/x-xwindowdump"},
                {"xyz", "chemical/x-xyz"},
                {"zip", "application/zip"}
            };
    }
}