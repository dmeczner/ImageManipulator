using ChangeToneOfPictureApp;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageManipulatorUnitTest.ChangetoneOfPictureApp
{
    public class ImageDataHandlingTests
    {
        private const string TEST_IMAGE_FILEPATH = @".\PictureForTest\originalTestPicture.jpg";
        private const string EXPECTED_FILEPATH = @".\PictureForTest\expectedResult.jpg";

        [Fact]
        public void ProcessImages_WithValidImages_ReturnsProcessedFiles()
        {
            // Arrange
            var imageBytes = new List<FileHandlingHelper>
            {
                new() {
                    CompleteFilePath = TEST_IMAGE_FILEPATH,
                    FileContentBytes = GetTestImageBytes()
                },
                new() {
                    CompleteFilePath = EXPECTED_FILEPATH,
                    FileContentBytes = GetTestImageBytes()
                }
            };

            // Act
            var result = ImageDataHandling.ProcessImages(imageBytes);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, path => Assert.EndsWith(".jpg", path));
        }

        [Fact]
        public void ProcessImages_WithEmptyFileContentBytes_SkipsProcessing()
        {
            // Arrange
            var imageBytes = new List<FileHandlingHelper>
            {
                new FileHandlingHelper
                {
                    CompleteFilePath = "test1.jpg",
                    FileContentBytes = Array.Empty<byte>()
                }
            };

            // Act
            var result = ImageDataHandling.ProcessImages(imageBytes);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ProcessImages_WithOrigninalTestPictureAndExpectedResult_Equal()
        {
            // Arrange
            string[] args = [TEST_IMAGE_FILEPATH];
            var imagesWithData = FileHandling.ReadAllImagesFromArguments(args);
            byte[] expectedBytes = File.ReadAllBytes(EXPECTED_FILEPATH);

            // Act
            var result = ImageDataHandling.ProcessImages(imagesWithData).First();
            var resultBytes = File.ReadAllBytes(result);

            // Assert
            Assert.Equal(expectedBytes, resultBytes);

            // Cleanup
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        private byte[] GetTestImageBytes()
        {
            using var bitmap = new Bitmap(10, 10);
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            return memoryStream.ToArray();
        }
    }
}
