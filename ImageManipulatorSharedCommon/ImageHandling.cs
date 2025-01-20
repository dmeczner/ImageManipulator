using System.Drawing;
using System.Drawing.Imaging;

namespace ImageManipulatorSharedCommon
{
    public static class ImageHandling
    {
        //https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale i used this solution becase i think this is the fastest
        public static Bitmap MakeOrangeGrayscale(Bitmap original)
        {
            Bitmap newBitmap = new(original.Width, original.Height);

            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {

                //create the orange grayscale ColorMatrix
                ColorMatrix colorMatrix = new(
                [
                    [-1.0f, -1.0f, -1.0f, 0, 0],
                    [-0.3f, -0.3f, -0.3f, 0, 0],
                    [-0.05f, -0.05f, -0.05f, 0, 0],
                    [0, 0, 0, 1, 0],
                    [1.5f, 1.0f, 0.5f, 0, 1]
                ]);

                using ImageAttributes attributes = new();

                attributes.SetColorMatrix(colorMatrix);

                //draw the original image on the new image
                //using the grayscale color matrix
                graphics.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }
            return newBitmap;
        }
    }
}
