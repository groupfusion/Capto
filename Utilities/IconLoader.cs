using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace Capto.Utilities
{
    public class IconLoader
    {
        private static readonly Assembly _assembly = typeof(IconLoader).Assembly;
        
        public static Image LoadIcon(string fileName, Color targetColor, int size = 48)
        {
            Image fallbackImage = CreateFallbackIcon(size, size);

            try
            {
                string resourceName = $"Capto.resource.{fileName}.png";
                
                using var stream = _assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine($"Embedded resource {resourceName} not found.");
                    return fallbackImage;
                }

                var originalImage = Image.FromStream(stream);
                
                if (originalImage.Width == size && originalImage.Height == size)
                {
                    if (targetColor == Color.Black || targetColor.A == 0)
                    {
                        return originalImage;
                    }
                    
                    return RecolorImage(originalImage, targetColor);
                }
                
                var resizedImage = ResizeImage(originalImage, size, size);
                originalImage.Dispose();
                
                if (targetColor == Color.Black || targetColor.A == 0)
                {
                    return resizedImage;
                }
                
                return RecolorImage(resizedImage, targetColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading icon {fileName}: {ex.Message}");
            }

            return fallbackImage;
        }

        private static Image ResizeImage(Image originalImage, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(originalImage, destRect, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }

        private static Image RecolorImage(Image originalImage, Color targetColor)
        {
            var bitmap = new Bitmap(originalImage.Width, originalImage.Height);
            bitmap.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                var colorMatrix = new ColorMatrix();
                colorMatrix.Matrix00 = targetColor.R / 255f;
                colorMatrix.Matrix11 = targetColor.G / 255f;
                colorMatrix.Matrix22 = targetColor.B / 255f;
                colorMatrix.Matrix33 = targetColor.A / 255f;

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                graphics.DrawImage(originalImage,
                    new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                    0, 0, originalImage.Width, originalImage.Height,
                    GraphicsUnit.Pixel, attributes);
            }

            return bitmap;
        }

        private static Image CreateFallbackIcon(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using var g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.Gray);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillEllipse(brush, 2, 2, width - 4, height - 4);
            return bitmap;
        }
    }
}
