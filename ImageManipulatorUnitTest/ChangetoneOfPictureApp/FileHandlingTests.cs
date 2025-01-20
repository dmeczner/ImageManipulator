using ChangeToneOfPictureApp;

namespace ImageManipulatorUnitTest.ChangetoneOfPictureApp
{
    public class FileHandlingTests
    {
        [Fact]
        public void ReadAllImagesFromArguments_ValidImages_ReturnsListOfFileHandlingHelpers()
        {
            // Arrange
            string[] args = { "test1.jpg", "test2.jpeg" };
            foreach (var arg in args)
            {
                File.WriteAllBytes(arg, [0x1, 0x2, 0x3]);
            }

            // Act
            var result = FileHandling.ReadAllImagesFromArguments(args);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, item => Assert.Contains(Path.GetExtension(item.CompleteFilePath), new[] { ".jpg", ".jpeg" }));

            // Cleanup
            foreach (var arg in args)
            {
                File.Delete(arg);
            }
        }

        [Fact]
        public void ReadAllImagesFromArguments_InvalidImages_ReturnsEmptyList()
        {
            // Arrange
            string[] args = { "test1.png", "test2.bmp" };

            // Act
            var result = FileHandling.ReadAllImagesFromArguments(args);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetNextFileName_ValidPath_ReturnsNewFileName()
        {
            // Arrange
            string fullPath = @"C:\Images\test.jpg";

            // Act
            var result = FileHandling.GetNextFileName(fullPath);

            // Assert
            Assert.Equal(@"C:\Images\test_grayscaleOrange.jpg", result);
        }
    }
}
