using System.Drawing;
using System.IO;

namespace Web.Common
{
    public static class ImageUtil
    {
        public static byte[] ResizeImage(int max, byte[] originBytes)
        {
            byte[] resizedBytes;
            using (var originStream = new MemoryStream(originBytes))
            {
                var originImage = Image.FromStream(originStream);

                int width, height;
                float ratio = (float)originImage.Width / originImage.Height;
                if (ratio > 1)
                {
                    width = max;
                    height = (int)(width / ratio);
                }
                else
                {
                    height = max;
                    width = (int)(height * ratio);
                }

                Image resizedImage = new Bitmap(originImage, width, height);
                var resizedStream = new MemoryStream();
                resizedImage.Save(resizedStream, System.Drawing.Imaging.ImageFormat.Bmp);
                resizedBytes = resizedStream.ToArray();
            }

            return resizedBytes;
        }
    }
}