namespace ChangeToneOfPictureApp
{
    public static class FileHandling
    {
        private static readonly List<string> ALLOWED_EXTENSITONS = [".jpg", ".jpeg"];

        public static List<FileHandlingHelper> ReadAllImagesFromArguments(string[] args)
        {
            List<FileHandlingHelper> images = [];

            for (int i = 0; i < args.Length; i++)
            {
                string imageFullPath = args[i];
                try
                {
                    string currentFileExtension = Path.GetExtension(imageFullPath);

                    // check file extension is allowed
                    if (ALLOWED_EXTENSITONS.Any(allowedExtension => string.Equals(allowedExtension, currentFileExtension, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        byte[] bytes = File.ReadAllBytes(imageFullPath);
                        images.Add(new FileHandlingHelper { CompleteFilePath = imageFullPath, FileContentBytes = bytes });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while try to read Image ({imageFullPath})", ex.ToString());
                }
            }

            return images;
        }

        /// <summary>
        /// try to calculate the next file name i keep the original one.
        /// </summary>
        /// <param name="fullPath"></param>
        public static string GetNextFileName(ReadOnlySpan<char> fullPath)
        {
            var directory = Path.GetDirectoryName(fullPath);
            var getCurrentFile = Path.GetFileNameWithoutExtension(fullPath);
            var fileSuffix = $"_grayscaleOrange.jpg".AsSpan();
            var newFileName = string.Concat(getCurrentFile, fileSuffix).AsSpan();
            return Path.Combine(directory.ToString(), newFileName.ToString());
        }
    }
}
