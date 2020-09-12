using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Images
{
    [PublicAPI]
    public static class ImageStandardizer
    {
        private const byte _colorMatchThreshold = 5;

        public static bool AllImageFilesExist(string filename)
        {
            var fullSizeFilename = filename + ".jpeg";
            var thumbnail50Filename = filename + "-thumbnail-50.jpeg";
            var thumbnail150Filename = filename + "-thumbnail-150.jpeg";
            var thumbnail250Filename = filename + "-thumbnail-250.jpeg";

            return File.Exists(fullSizeFilename) && File.Exists(thumbnail50Filename) && File.Exists(thumbnail150Filename) && File.Exists(thumbnail250Filename);
        }

        public static Bitmap ConvertToStandard(byte[] imageBytes)
        {
            using var stream = new MemoryStream(imageBytes);
            return ConvertToStandard(stream);
        }

        public static Bitmap ConvertToStandard(Stream stream)
        {
            Bitmap bitmap;
            try
            {
                bitmap = (Bitmap) Image.FromStream(stream);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidImageException(exception);
            }

            using (bitmap)
            {
                return ConvertToStandard(bitmap);
            }
        }

        public static Bitmap ConvertToStandard(Bitmap bitmap)
        {
            Bitmap standardBitmap;
            if (bitmap.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                standardBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(standardBitmap))
                {
                    graphics.DrawImage(bitmap, new Rectangle(0, 0, standardBitmap.Width, standardBitmap.Height));
                }

                bitmap.Dispose();
            }
            else
            {
                standardBitmap = bitmap;
            }

            return standardBitmap;
        }

        public static byte[] RemoveBorder(byte[] imageBytes)
        {
            using var standardBitmap = ConvertToStandard(imageBytes);
            using var trimmedBitmap = RemoveBorder0(standardBitmap);
            var converter = new ImageConverter();
            return (byte[]) converter.ConvertTo(trimmedBitmap, typeof(byte[]));
        }

        public static Bitmap RemoveBorder(Bitmap bitmap)
        {
            var standardBitmap = ConvertToStandard(bitmap);
            return RemoveBorder0(standardBitmap);
        }

        public static void SaveToStandardFile(byte[] imageBytes, string filename)
        {
            using var image = ConvertToStandard(imageBytes);
            SaveToStandardFile0(image, filename);
        }

        public static void SaveToStandardFile(Stream stream, string filename)
        {
            using var image = ConvertToStandard(stream);
            SaveToStandardFile0(image, filename);
        }

        public static void SaveToStandardFile(Bitmap bitmap, string filename)
        {
            using var image = ConvertToStandard(bitmap);
            SaveToStandardFile0(image, filename);
        }

        private static Bitmap RemoveBorder0(Bitmap bitmap)
        {
            var firstRow = 0;
            var firstColumn = 0;
            var lastRow = bitmap.Height - 1;
            var lastColumn = bitmap.Width - 1;

            var firstPixel = bitmap.GetPixel(0, 0);
            var firstPixelR = firstPixel.R;
            var firstPixelG = firstPixel.G;
            var firstPixelB = firstPixel.B;

            for (var y = 0; y < bitmap.Height; y++)
            {
                var noMatch = false;
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if
                    (
                        pixel.R > firstPixelR + _colorMatchThreshold || pixel.R < firstPixelR - _colorMatchThreshold ||
                        pixel.G > firstPixelG + _colorMatchThreshold || pixel.G < firstPixelG - _colorMatchThreshold ||
                        pixel.B > firstPixelB + _colorMatchThreshold || pixel.B < firstPixelB - _colorMatchThreshold
                    )
                    {
                        noMatch = true;
                        break;
                    }
                }

                if (noMatch)
                {
                    firstRow = y;
                }
            }

            for (var y = bitmap.Height - 1; y >= 0; y--)
            {
                var noMatch = false;
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if
                    (
                        pixel.R > firstPixelR + _colorMatchThreshold || pixel.R < firstPixelR - _colorMatchThreshold ||
                        pixel.G > firstPixelG + _colorMatchThreshold || pixel.G < firstPixelG - _colorMatchThreshold ||
                        pixel.B > firstPixelB + _colorMatchThreshold || pixel.B < firstPixelB - _colorMatchThreshold
                    )
                    {
                        noMatch = true;
                        break;
                    }
                }

                if (noMatch)
                {
                    lastRow = y;
                }
            }

            for (var x = 0; x < bitmap.Width; x++)
            {
                var noMatch = false;
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if
                    (
                        pixel.R > firstPixelR + _colorMatchThreshold || pixel.R < firstPixelR - _colorMatchThreshold ||
                        pixel.G > firstPixelG + _colorMatchThreshold || pixel.G < firstPixelG - _colorMatchThreshold ||
                        pixel.B > firstPixelB + _colorMatchThreshold || pixel.B < firstPixelB - _colorMatchThreshold
                    )
                    {
                        noMatch = true;
                        break;
                    }
                }

                if (noMatch)
                {
                    firstColumn = x;
                }
            }

            for (var x = bitmap.Width - 1; x >= 0; x--)
            {
                var noMatch = false;
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if
                    (
                        pixel.R > firstPixelR + _colorMatchThreshold || pixel.R < firstPixelR - _colorMatchThreshold ||
                        pixel.G > firstPixelG + _colorMatchThreshold || pixel.G < firstPixelG - _colorMatchThreshold ||
                        pixel.B > firstPixelB + _colorMatchThreshold || pixel.B < firstPixelB - _colorMatchThreshold
                    )
                    {
                        noMatch = true;
                        break;
                    }
                }

                if (noMatch)
                {
                    lastColumn = x;
                }
            }

            Bitmap trimmedBitmap;
            if (firstColumn < lastColumn && firstRow < lastRow && (firstColumn > 0 || firstRow > 0 || lastColumn < bitmap.Width - 1 || lastRow < bitmap.Height - 1))
            {
                trimmedBitmap = new Bitmap(lastColumn - firstColumn + 1, lastRow - firstRow + 1, PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawImage(trimmedBitmap, new Rectangle(firstColumn, firstRow, trimmedBitmap.Width, trimmedBitmap.Height));
                }

                bitmap.Dispose();
            }
            else
            {
                trimmedBitmap = bitmap;
            }

            return trimmedBitmap;
        }

        private static void SaveThumbnail(Bitmap bitmap, int size, string filename, ImageCodecInfo jpegEncoder, EncoderParameters encoderParameters)
        {
            var thumbnailScale = Math.Max(bitmap.Height, bitmap.Width) / (decimal) size;
            if (Convert.ToInt32(bitmap.Width / thumbnailScale) > 10 && Convert.ToInt32(bitmap.Height / thumbnailScale) > 10)
            {
                using var thumbnail = new Bitmap(Convert.ToInt32(bitmap.Width / thumbnailScale), Convert.ToInt32(bitmap.Height / thumbnailScale), bitmap.PixelFormat);
                using var graphics = Graphics.FromImage(thumbnail);
                graphics.DrawImage(bitmap, new Rectangle(0, 0, thumbnail.Width, thumbnail.Height));
                thumbnail.Save(filename, jpegEncoder, encoderParameters);
            }
        }

        private static void SaveToStandardFile0(Bitmap bitmap, string filename)
        {
            var fullSizeFilename = filename + ".jpeg";
            var thumbnail50Filename = filename + "-thumbnail-50.jpeg";
            var thumbnail150Filename = filename + "-thumbnail-150.jpeg";
            var thumbnail250Filename = filename + "-thumbnail-250.jpeg";
            var jpegEncoder = ImageCodecInfo.GetImageDecoders().Single(decoder => decoder.FormatID == ImageFormat.Jpeg.Guid);
            using var encoderParameters = new EncoderParameters(2) { Param = { [0] = new EncoderParameter(Encoder.Quality, 100L), [1] = new EncoderParameter(Encoder.Compression, 0L) } };

            if (!File.Exists(fullSizeFilename))
            {
                bitmap.Save(fullSizeFilename, jpegEncoder, encoderParameters);
            }

            if (!File.Exists(thumbnail50Filename))
            {
                SaveThumbnail(bitmap, 50, thumbnail50Filename, jpegEncoder, encoderParameters);
            }

            if (!File.Exists(thumbnail150Filename))
            {
                SaveThumbnail(bitmap, 150, thumbnail150Filename, jpegEncoder, encoderParameters);
            }

            if (!File.Exists(thumbnail250Filename))
            {
                SaveThumbnail(bitmap, 250, thumbnail250Filename, jpegEncoder, encoderParameters);
            }
        }
    }
}
