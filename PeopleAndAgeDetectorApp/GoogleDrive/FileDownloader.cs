using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;

namespace AgeDetectorApp.GoogleDrive
{
    public class FileDownloader : IDisposable
    {
        private const string GOOGLE_DRIVE_DOMAIN = "drive.google.com";
        private const string GOOGLE_DRIVE_DOMAIN2 = "https://drive.google.com";

        // In the worst case, it is necessary to send 3 download requests to the Drive address
        //   1. an NID cookie is returned instead of a download_warning cookie
        //   2. download_warning cookie returned
        //   3. the actual file is downloaded
        private const int GOOGLE_DRIVE_MAX_DOWNLOAD_ATTEMPT = 3;

        public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgress progress);

        private readonly CookieAwareWebClient webClient;
        private readonly DownloadProgress downloadProgress;

        private Uri downloadAddress;
        private string downloadPath;

        private bool asyncDownload;
        private object userToken;

        private bool downloadingDriveFile;
        private int driveDownloadAttempt;

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        public FileDownloader()
        {
            webClient = new CookieAwareWebClient();
            webClient.DownloadProgressChanged += DownloadProgressChangedCallback;
            webClient.DownloadFileCompleted += DownloadFileCompletedCallback;

            downloadProgress = new DownloadProgress();
        }

        public void DownloadFile(string address, string fileName)
        {
            DownloadFile(address, fileName, false, null);
        }

        public void DownloadFileAsync(string address, string fileName, object userToken = null)
        {
            DownloadFile(address, fileName, true, userToken);
        }

        private void DownloadFile(string address, string fileName, bool asyncDownload, object userToken)
        {
            downloadingDriveFile = address.StartsWith(GOOGLE_DRIVE_DOMAIN) || address.StartsWith(GOOGLE_DRIVE_DOMAIN2);
            if (downloadingDriveFile)
            {
                address = GetGoogleDriveDownloadAddress(address);
                driveDownloadAttempt = 1;

                webClient.ContentRangeTarget = downloadProgress;
            }
            else
                webClient.ContentRangeTarget = null;

            downloadAddress = new Uri(address);
            downloadPath = fileName;

            downloadProgress.TotalBytesToReceive = -1L;
            downloadProgress.UserState = userToken;

            this.asyncDownload = asyncDownload;
            this.userToken = userToken;

            DownloadFileInternal();
        }

        private void DownloadFileInternal()
        {
            if (!asyncDownload)
            {
                webClient.DownloadFile(downloadAddress, downloadPath);

                // This callback isn't triggered for synchronous downloads, manually trigger it
                DownloadFileCompletedCallback(webClient, new AsyncCompletedEventArgs(null, false, null));
            }
            else if (userToken == null)
                webClient.DownloadFileAsync(downloadAddress, downloadPath);
            else
                webClient.DownloadFileAsync(downloadAddress, downloadPath, userToken);
        }

        private void DownloadProgressChangedCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
            {
                downloadProgress.BytesReceived = e.BytesReceived;
                if (e.TotalBytesToReceive > 0L)
                    downloadProgress.TotalBytesToReceive = e.TotalBytesToReceive;

                DownloadProgressChanged(this, downloadProgress);
            }
        }

        private void DownloadFileCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (!downloadingDriveFile)
            {
                DownloadFileCompleted?.Invoke(this, e);
            }
            else
            {
                if (driveDownloadAttempt < GOOGLE_DRIVE_MAX_DOWNLOAD_ATTEMPT && !ProcessDriveDownload())
                {
                    // Try downloading the Drive file again
                    driveDownloadAttempt++;
                    DownloadFileInternal();
                }
                else DownloadFileCompleted?.Invoke(this, e);
            }
        }

        // Downloading large files from Google Drive prompts a warning screen and requires manual confirmation
        // Consider that case and try to confirm the download automatically if warning prompt occurs
        // Returns true, if no more download requests are necessary
        private bool ProcessDriveDownload()
        {
            FileInfo downloadedFile = new(downloadPath);
            if (downloadedFile == null)
                return true;

            // Confirmation page is around 50KB, shouldn't be larger than 60KB
            if (downloadedFile.Length > 60000L)
                return true;

            // Downloaded file might be the confirmation page, check it
            string content;
            using (var reader = downloadedFile.OpenText())
            {
                // Confirmation page starts with <!DOCTYPE html>, which can be preceeded by a newline
                char[] header = new char[20];
                int readCount = reader.ReadBlock(header, 0, 20);
                if (readCount < 20 || !new string(header).Contains("<!DOCTYPE html>"))
                    return true;

                content = reader.ReadToEnd();
            }

            int linkIndex = content.LastIndexOf("href=\"/uc?");
            if (linkIndex >= 0)
            {
                linkIndex += 6;
                int linkEnd = content.IndexOf('"', linkIndex);
                if (linkEnd >= 0)
                {
                    downloadAddress = new Uri("https://drive.google.com" + content[linkIndex..linkEnd].Replace("&amp;", "&"));
                    return false;
                }
            }

            int formIndex = content.LastIndexOf("<form id=\"download-form\"");
            if (formIndex >= 0)
            {
                int formEndIndex = content.IndexOf("</form>", formIndex + 10);
                int inputIndex = formIndex;
                StringBuilder sb = new StringBuilder().Append("https://drive.usercontent.google.com/download");
                bool isFirstArgument = true;
                while ((inputIndex = content.IndexOf("<input type=\"hidden\"", inputIndex + 10)) >= 0 && inputIndex < formEndIndex)
                {
                    linkIndex = content.IndexOf("name=", inputIndex + 10) + 6;
                    sb.Append(isFirstArgument ? '?' : '&').Append(content, linkIndex, content.IndexOf('"', linkIndex) - linkIndex).Append('=');

                    linkIndex = content.IndexOf("value=", linkIndex) + 7;
                    sb.Append(content, linkIndex, content.IndexOf('"', linkIndex) - linkIndex);

                    isFirstArgument = false;
                }

                downloadAddress = new Uri(sb.ToString());
                return false;
            }

            return true;
        }

        // Handles the following formats (links can be preceeded by https://):
        // - drive.google.com/open?id=FILEID&resourcekey=RESOURCEKEY
        // - drive.google.com/file/d/FILEID/view?usp=sharing&resourcekey=RESOURCEKEY
        // - drive.google.com/uc?id=FILEID&export=download&resourcekey=RESOURCEKEY
        private static string GetGoogleDriveDownloadAddress(string address)
        {
            int index = address.IndexOf("id=");
            int closingIndex;
            if (index > 0)
            {
                index += 3;
                closingIndex = address.IndexOf('&', index);
                if (closingIndex < 0)
                    closingIndex = address.Length;
            }
            else
            {
                index = address.IndexOf("file/d/");
                if (index < 0) // address is not in any of the supported forms
                    return string.Empty;

                index += 7;

                closingIndex = address.IndexOf('/', index);
                if (closingIndex < 0)
                {
                    closingIndex = address.IndexOf('?', index);
                    if (closingIndex < 0)
                        closingIndex = address.Length;
                }
            }

            string fileID = address[index..closingIndex];

            index = address.IndexOf("resourcekey=");
            if (index > 0)
            {
                index += 12;
                closingIndex = address.IndexOf('&', index);
                if (closingIndex < 0)
                    closingIndex = address.Length;

                string resourceKey = address[index..closingIndex];
                return string.Concat("https://drive.google.com/uc?id=", fileID, "&export=download&resourcekey=", resourceKey, "&confirm=t");
            }
            else
                return string.Concat("https://drive.google.com/uc?id=", fileID, "&export=download&confirm=t");
        }

        public void Dispose()
        {
            webClient.Dispose();
        }
    }
}
