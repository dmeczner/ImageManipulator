using System.Net;
using System.Text;
using static AgeDetectorApp.GoogleDrive.FileDownloader;

namespace AgeDetectorApp.GoogleDrive
{
    // Web client that preserves cookies (needed for Google Drive)
    internal class CookieAwareWebClient : WebClient
    {

        private readonly CookieContainer cookies = new();
        public DownloadProgress ContentRangeTarget;

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest httpRequest)
            {
                string cookie = cookies[address];
                if (cookie != null)
                    httpRequest.Headers.Set("cookie", cookie);

                if (ContentRangeTarget != null)
                    httpRequest.AddRange(0);
            }

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            return ProcessResponse(base.GetWebResponse(request, result));
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            return ProcessResponse(base.GetWebResponse(request));
        }

        private WebResponse ProcessResponse(WebResponse response)
        {
            string[] cookies = response.Headers.GetValues("Set-Cookie");
            if (cookies != null && cookies.Length > 0)
            {
                int length = 0;
                for (int i = 0; i < cookies.Length; i++)
                    length += cookies[i].Length;

                StringBuilder cookie = new(length);
                for (int i = 0; i < cookies.Length; i++)
                    cookie.Append(cookies[i]);

                this.cookies[response.ResponseUri] = cookie.ToString();
            }

            if (ContentRangeTarget != null)
            {
                string[] rangeLengthHeader = response.Headers.GetValues("Content-Range");
                if (rangeLengthHeader != null && rangeLengthHeader.Length > 0)
                {
                    int splitIndex = rangeLengthHeader[0].LastIndexOf('/');
                    if (splitIndex >= 0 && splitIndex < rangeLengthHeader[0].Length - 1)
                    {
                        long length;
                        if (long.TryParse(rangeLengthHeader[0].AsSpan(splitIndex + 1), out length))
                            ContentRangeTarget.TotalBytesToReceive = length;
                    }
                }
            }

            return response;
        }
    }
}
