namespace AgeDetectorApp.GoogleDrive
{
    // Custom download progress reporting (needed for Google Drive)
    public class DownloadProgress
    {
        public long BytesReceived, TotalBytesToReceive;
        public object UserState;

        public int ProgressPercentage
        {
            get
            {
                if (TotalBytesToReceive > 0L)
                    return (int)((double)BytesReceived / TotalBytesToReceive * 100);

                return 0;
            }
        }
    }
}
