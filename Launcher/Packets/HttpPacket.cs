using System;
using System.Text;

namespace Launcher
{
    class HttpPacket
    {
        private static string _emptyHtmlPage = "<html><head></head><body>[content]</body></html>";
        private readonly string CRLF = "\r\n";

        #region Common headers
        private string Response { get; set; }
        public string HttpResponse { get; set; }
        private readonly string DateAndTime = DateTime.Now.ToString("r");
        private readonly string Server = HttpServer.HTTP_SERVER_VERSION;
        private string LastModified { get; set; }       
        public string AcceptRanges { get; set; }
        public string CacheControl { get; set; } = "no-cache, no-store, must-revalidate";
        #endregion

        #region Content headers
        public string Content { get; set; }
        public string ContentType { get; set; }
        public string ContentLocation { get; set; }
        public string ContentLength { get; set; }
        #endregion

        #region File headers
        public string XRepository { get; set; }
        public string XPatchModule { get; set; }
        public string XProtocol { get; set; }
        public string XInfoUrl { get; set; }
        public string XLatestVersion { get; set; }
        #endregion

        #region Connection headers
        public string KeepAlive { get; set; }
        public string Connection { get; set; }
        #endregion

        public HttpPacket(string file)
        {
            Content = GetFile(file);

            if (!Content.Equals("")) //if page was found and has content
            {
                HttpResponse = "200 OK";
                LastModified = GetLastModified(file);
                AcceptRanges = "bytes";
                ContentType = "text";
            }
            else
            {
                HttpResponse = "200 OK";
                LastModified = DateTime.Now.ToString("r");
                //Content = ErrorPage("Page not found!");
            }            
        }

        public HttpPacket() { }

        public byte[] ToBytes()
        {           
            Response = "HTTP/1.1 " + HttpResponse + CRLF;
            CheckHeader("Date", DateAndTime);
            CheckHeader("Server", Server);
            CheckHeader("Last-Modified", LastModified);            
            CheckHeader("Cache-Control", CacheControl);            
            CheckHeader("Accept-Ranges", AcceptRanges);                  
            CheckHeader("Content-Type", ContentType);
            CheckHeader("Content-Location", ContentLocation);
            CheckHeader("Content-Length", ContentLength);
            CheckHeader("X-Repository", XRepository);
            CheckHeader("X-Patch-Module", XPatchModule);
            CheckHeader("X-Protocol", XProtocol);
            CheckHeader("X-Info-Url", XInfoUrl);
            CheckHeader("X-Latest-Version", XLatestVersion);
            CheckHeader("Keep-Alive", KeepAlive);
            CheckHeader("Connection", Connection);
            Response += CRLF; //html header finalizer
            Response += Content;

            return Encoding.ASCII.GetBytes(Response);
        }

        private string CheckHeader(string label, string value)
            => (value != null) ? Response += label + ": " + value + CRLF : Response;

        private string GetFile(string file)
        {
            string page = "";
            try { page = System.IO.File.ReadAllText(file); }
            catch (Exception) { /*page = ErrorPage("Page not found. Please, reinstall FFXIV1.0 Primal Launcher.");*/ }
            return page;
        }

        private static string GetLastModified(string file)
           => System.IO.File.GetLastWriteTime(file).ToString("r");

        #region Default pages
        /* We need this internal error page in case something goes wrong with the html outside the app */
        public static byte[] ErrorPage(string msg) => DefaultPage(BuildPage("<h1>Error!</h1><h4>" + msg + "<h4>"));                   

        /* Auth page is internally generated here as we don't want the user changing the page contents and causing unexpected errors :) */
        public static byte[] AuthPage(int userId)
            => DefaultPage(BuildPage("<x-sqexauth sid=\"" + userId + "\" lang=\"en-us\" region=\"2\" utc=\"11231131\"/>"));
        
        private static string BuildPage(string html) => _emptyHtmlPage.Replace("[content]", html);

        private static byte[] DefaultPage(string page)
        {
            HttpPacket response = new HttpPacket()
            {
                HttpResponse = "200 OK",
                ContentType = "text/html",
                LastModified = DateTime.Now.ToString("r"),
                AcceptRanges = "bytes",
                ContentLength = page.Length.ToString(),
                Content = page
            };
            return response.ToBytes();
        }
        #endregion
    }
}
