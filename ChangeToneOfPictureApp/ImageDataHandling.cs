using ImageManipulatorSharedCommon;
using System.Drawing;
using System.Drawing.Imaging;

namespace ChangeToneOfPictureApp
{
    public static class ImageDataHandling
    {
        public static List<string> ProcessImages(List<FileHandlingHelper> imageBytes)
        {
            List<string> processedFiles = [];
            for (int i = 0; i < imageBytes.Count; i++)
            {
                if (!imageBytes[i].FileContentBytes.Any()) { continue; }

                using var memoryStream = new MemoryStream(imageBytes[i].FileContentBytes);
                using var bitmap = new Bitmap(memoryStream);

                var newBitmap =ImageHandling.MakeOrangeGrayscale(bitmap);

                var fileFullPath = imageBytes[i].CompleteFilePath.AsSpan();
                var outputFullPath = FileHandling.GetNextFileName(fileFullPath);

                newBitmap.Save(outputFullPath, ImageFormat.Jpeg);

                processedFiles.Add(outputFullPath);
            }
            return processedFiles;
        }
    }
}
