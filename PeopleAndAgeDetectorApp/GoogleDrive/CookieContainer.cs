namespace AgeDetectorApp.GoogleDrive
{
    internal class CookieContainer
    {
        private readonly Dictionary<string, string> cookies = [];

        public string this[Uri address]
        {
            get
            {
                string cookie;
                if (cookies.TryGetValue(address.Host, out cookie))
                    return cookie;

                return null;
            }
            set
            {
                cookies[address.Host] = value;
            }
        }
    }
}
