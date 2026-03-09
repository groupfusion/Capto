using System;
using System.Drawing;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 模糊绘制对象
    /// 用于对指定区域进行模糊处理
    /// </summary>
    public class BlurDrawObject : DrawObject
    {
        private Rectangle _rectangle;
        private Bitmap? _blurredBitmap;

        /// <summary>
        /// 矩形
        /// </summary>
        public Rectangle Rectangle
        {
            get { return _rectangle; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="originalBitmap">原始位图</param>
        public BlurDrawObject(Rectangle rect, Bitmap? originalBitmap) : base(new Pen(Color.Black))
        {
            _rectangle = rect;
            // 截取指定区域的图像并进行模糊处理
            if (originalBitmap != null && rect.Width > 0 && rect.Height > 0)
            {
                // 确保矩形在图像范围内
                rect.Intersect(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height));
                if (rect.Width > 0 && rect.Height > 0)
                {
                    var croppedBitmap = originalBitmap.Clone(rect, originalBitmap.PixelFormat);
                    _blurredBitmap = ApplyBlur(croppedBitmap, 10);
                }
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="g">绘图对象</param>
        public override void Draw(Graphics g)
        {
            if (g == null || _blurredBitmap == null) return;
            
            try
            {
                g.DrawImage(_blurredBitmap, _rectangle);
            }
            catch (Exception)
            {
                // 忽略绘制异常，避免程序崩溃
            }
        }

        /// <summary>
        /// 绘制（带偏移量）
        /// </summary>
        /// <param name="g">绘图对象</param>
        /// <param name="offset">偏移量</param>
        public override void Draw(Graphics g, Point offset)
        {
            if (g == null || _blurredBitmap == null) return;
            
            try
            {
                var rect = new Rectangle(
                    _rectangle.X - offset.X,
                    _rectangle.Y - offset.Y,
                    _rectangle.Width,
                    _rectangle.Height
                );
                g.DrawImage(_blurredBitmap, rect);
            }
            catch (Exception)
            {
                // 忽略绘制异常，避免程序崩溃
            }
        }

        /// <summary>
        /// 应用模糊效果
        /// </summary>
        /// <param name="bitmap">位图</param>
        /// <param name="blurRadius">模糊半径</param>
        /// <returns>模糊后的位图</returns>
        private Bitmap ApplyBlur(Bitmap bitmap, int blurRadius)
        {
            var blurred = new Bitmap(bitmap.Width, bitmap.Height);
            using (var graphics = Graphics.FromImage(blurred))
            {
                // 使用简单的模糊效果
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                
                // 缩小再放大来实现模糊效果
                int scale = 4;
                int smallWidth = System.Math.Max(1, bitmap.Width / scale);
                int smallHeight = System.Math.Max(1, bitmap.Height / scale);
                
                var smallBitmap = new Bitmap(smallWidth, smallHeight);
                using (var smallGraphics = Graphics.FromImage(smallBitmap))
                {
                    smallGraphics.DrawImage(bitmap, 0, 0, smallBitmap.Width, smallBitmap.Height);
                }
                
                graphics.DrawImage(smallBitmap, 0, 0, bitmap.Width, bitmap.Height);
                smallBitmap.Dispose();
            }
            return blurred;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _blurredBitmap?.Dispose();
        }

        /// <summary>
        /// 添加点
        /// 模糊工具不需要添加点，因为它是通过起点和终点确定的
        /// </summary>
        /// <param name="point">点</param>
        public override void AddPoint(Point point)
        {
            // 模糊工具不需要添加点，因为它是通过起点和终点确定的
        }
    }
}