using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Cape_Changer.Utils
{
    public class ImageUtils
    {
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static BitmapImage ImageToBitmapImage(Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static Image CropImage(Image image, Rectangle cropRectangle)
        {
            Bitmap bitmap = new Bitmap(image);

            return bitmap.Clone(cropRectangle, bitmap.PixelFormat);
        }

        public static Image ReziseImage(Image image, int multiplierX, int multiplierY)
        {
            //multiplierX = image.Width * multiplierX;
            //multiplierY = image.Height * multiplierY;

            //TODO: adapter cette image pour qu'elle fonctionne avec tout multiple.

            int newWidth = image.Width * multiplierX - multiplierX;
            int newHeight = image.Height * multiplierY - multiplierY;

            Bitmap imageInBitmap = (Bitmap)image;
            Bitmap newImage = new Bitmap(newWidth, newHeight);

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    newImage.SetPixel(x, y, imageInBitmap.GetPixel(x >> (int)Math.Sqrt(multiplierX), y >> (int)Math.Sqrt(multiplierY)));
                }
            }

            return newImage;
        }
    }
}
