using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ByrneLabs.Commons.Images.AForgePort.Images;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Images
{
    [PublicAPI]
    public static class ImageComparer
    {
        public static float ImageCompare(byte[] imageBytes1, byte[] imageBytes2, float similarityThreshold)
        {
            float score;
            if (imageBytes1 == null || imageBytes1.Length == 0 || imageBytes2 == null || imageBytes2.Length == 0)
            {
                score = 0;
            }
            else
            {
                using var stream1 = new MemoryStream(imageBytes1);
                using var stream2 = new MemoryStream(imageBytes2);
                using var image1 = new Bitmap(stream1);
                using var image2 = new Bitmap(stream2);
                var minWidth = Math.Min(image1.Width, image2.Width);
                var minHeight = Math.Min(image1.Height, image2.Height);
                if (minWidth > 2000 && minWidth > minHeight)
                {
                    minHeight = (int) (minHeight * (2000.0 / minWidth));
                    minWidth = 2000;
                }
                else if (minHeight > 2000 && minHeight > minWidth)
                {
                    minWidth = (int) (minWidth * (2000.0 / minHeight));
                    minHeight = 2000;
                }

                using var thumbnail1 = new Bitmap(image1.GetThumbnailImage(minWidth, minHeight, null, IntPtr.Zero));
                using var thumbnail2 = new Bitmap(image2.GetThumbnailImage(minWidth, minHeight, null, IntPtr.Zero));
                using var newBitmap1 = thumbnail1.Clone(new Rectangle(0, 0, minWidth, minHeight), PixelFormat.Format24bppRgb);
                using var newBitmap2 = thumbnail2.Clone(new Rectangle(0, 0, minWidth, minHeight), PixelFormat.Format24bppRgb);
                var templateMatching = new ExhaustiveTemplateMatching(similarityThreshold);

                var results = templateMatching.ProcessImage(newBitmap1, newBitmap2);

                score = results.Length == 0 ? 0 : results[0].Similarity;
            }

            return score;
        }
    }
}
