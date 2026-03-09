using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using SkiaSharp;
using Svg.Skia;

namespace Capto.Utilities
{
    public class IconLoader
    {
        private static readonly Assembly _assembly = typeof(IconLoader).Assembly;
        
        public static Image LoadSvgIcon(string fileName, SKColor targetColor, int size = 48)
        {
            Image fallbackImage = CreateFallbackIcon(size, size);

            try
            {
                string resourceName = $"Capto.resource.{fileName}";
                
                using var stream = _assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine($"Embedded resource {resourceName} not found.");
                    return fallbackImage;
                }

                using var svg = new SKSvg();
                var picture = svg.Load(stream);
                if (picture == null)
                {
                    Console.WriteLine($"SVG icon {fileName} is empty or corrupted.");
                    return fallbackImage;
                }

                float scaleX = (float)size / picture.CullRect.Width;
                float scaleY = (float)size / picture.CullRect.Height;
                float scale = Math.Min(scaleX, scaleY);

                using var skBitmap = new SKBitmap(size, size);
                using var skCanvas = new SKCanvas(skBitmap);
                skCanvas.Clear(SKColors.Transparent);
                
                skCanvas.Translate(size / 2f, size / 2f);
                skCanvas.Scale(scale);
                skCanvas.Translate(-picture.CullRect.Width / 2f, -picture.CullRect.Height / 2f);
                
                using var paint = new SKPaint { IsAntialias = true, Color = targetColor };
                skCanvas.DrawPicture(picture, paint);

                using var memoryStream = new MemoryStream();
                skBitmap.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
                memoryStream.Seek(0, SeekOrigin.Begin);
                Image finalImage = Image.FromStream(memoryStream);
                return finalImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading icon {fileName}: {ex.Message}");
            }

            return fallbackImage;
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
