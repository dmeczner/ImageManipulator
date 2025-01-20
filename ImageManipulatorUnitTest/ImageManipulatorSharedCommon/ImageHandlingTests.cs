using ImageManipulatorSharedCommon;
using System.Drawing;

namespace ImageManipulatorUnitTest.ImageManipulatorSharedCommon
{
    public class ImageHandlingTests
    {
        [Fact]
        public void MakeOrangeGrayscale_ValidImage_ReturnsGrayscaleImage()
        {
            // Arrange
            Bitmap original = new(10, 10);
            using (Graphics g = Graphics.FromImage(original))
            {
                g.Clear(Color.Blue);
            }

            // Act
            Bitmap result = ImageHandling.MakeOrangeGrayscale(original);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Width, result.Width);
            Assert.Equal(original.Height, result.Height);

            // Check a few pixels to ensure they are not the original color
            Color originalColor = original.GetPixel(0, 0);
            Color newColor = result.GetPixel(0, 0);
            Assert.NotEqual(originalColor, newColor);
        }
    }
}
