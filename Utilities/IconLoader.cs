using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using SkiaSharp;
using Svg.Skia;

namespace Capto.Utilities
{
    /// <summary>
    /// 图标加载器类
    /// 负责加载和处理SVG图标
    /// </summary>
    public class IconLoader
    {
        /// <summary>
        /// 加载SVG图标
        /// </summary>
        /// <param name="fileName">图标文件名</param>
        /// <param name="targetColor">目标颜色（SKColor）</param>
        /// <param name="size">输出图片尺寸（正方形）</param>
        /// <returns>加载的图像对象，失败返回兜底图标</returns>
        public static Image LoadSvgIcon(string fileName, SKColor targetColor, int size = 48)
        {
            // 兜底返回：默认纯色占位图（避免返回 null 导致空引用）
            Image fallbackImage = CreateFallbackIcon(size, size);

            try
            {
                // 1. 定义候选路径（修复 null 拼接问题）
                string?[] possiblePaths = GetPossibleIconPaths(fileName);
                
                string filePath = null;
                foreach (var path in possiblePaths)
                {
                    // 跳过 null/空路径，避免 Path.Combine 异常
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    
                    if (File.Exists(path))
                    {
                        filePath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine($"SVG icon {fileName} not found in all candidate paths.");
                    return fallbackImage; // 找不到文件返回兜底图
                }

                // 2. 加载 SVG（使用 SKSvg）
                using var svg = new SKSvg();
                var picture = svg.Load(filePath);
                if (picture == null)
                {
                    Console.WriteLine($"SVG icon {fileName} is empty or corrupted.");
                    return fallbackImage;
                }

                // 3. 计算目标尺寸（严谨的缩放逻辑）
                float scaleX = (float)size / picture.CullRect.Width;
                float scaleY = (float)size / picture.CullRect.Height;
                float scale = Math.Min(scaleX, scaleY); // 等比缩放，避免变形

                // 4. 绘制 SVG（高画质，释放资源）
                using var skBitmap = new SKBitmap(size, size);
                using var skCanvas = new SKCanvas(skBitmap);
                skCanvas.Clear(SKColors.Transparent); // 透明背景
                
                // 平移+缩放，让 SVG 居中
                skCanvas.Translate(size / 2f, size / 2f);
                skCanvas.Scale(scale);
                skCanvas.Translate(-picture.CullRect.Width / 2f, -picture.CullRect.Height / 2f);
                
                // 抗锯齿绘制，使用目标颜色
                using var paint = new SKPaint { IsAntialias = true, Color = targetColor };
                skCanvas.DrawPicture(picture, paint);

                // 5. 转换为 Image（自动释放 SkiaSharp 资源）
                using var stream = new MemoryStream();
                skBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                stream.Seek(0, SeekOrigin.Begin);
                Image finalImage = Image.FromStream(stream);
                return finalImage;
            }
            catch (FileNotFoundException ex)
            {
                // 精准捕获文件不存在（已提前判断，此处为兜底）
                Console.WriteLine($"Icon file not found: {ex.FileName}");
            }
            catch (IOException ex)
            {
                // 捕获 IO 异常（权限不足、文件被占用）
                Console.WriteLine($"IO error loading icon {fileName}: {ex.Message}\nStack: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                // 其他异常（SVG 解析失败、内存不足等）
                Console.WriteLine($"Unexpected error loading icon {fileName}: {ex.Message}\nStack: {ex.StackTrace}");
            }

            // 所有异常场景返回兜底图
            return fallbackImage;
        }

        /// <summary>
        /// 创建兜底图标（避免返回 null）
        /// </summary>
        private static Image CreateFallbackIcon(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using var g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            // 绘制一个简单的圆形占位图
            using var brush = new SolidBrush(Color.Gray);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillEllipse(brush, 2, 2, width - 4, height - 4);
            return bitmap;
        }

        /// <summary>
        /// 生成候选路径（修复 null 拼接问题）
        /// </summary>
        private static string?[] GetPossibleIconPaths(string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string parentDir = Directory.GetParent(baseDir)?.Parent?.FullName ?? string.Empty;
            string currentDir = Directory.GetCurrentDirectory();

            return new string?[]
            {
                Path.Combine(baseDir, "resource", fileName),
                string.IsNullOrEmpty(parentDir) ? null : Path.Combine(parentDir, "resource", fileName),
                Path.Combine(currentDir, "resource", fileName)
            };
        }
    }
}