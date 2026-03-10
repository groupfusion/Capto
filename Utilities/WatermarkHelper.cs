using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Capto.Utilities
{
    public static class WatermarkHelper
    {
        public static void AddWatermark(Bitmap bitmap, string? watermarkText = null)
        {
            string text = watermarkText ?? "Capto 截图";
            
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            
            float fontSize = Math.Max(12, Math.Min(bitmap.Width, bitmap.Height) / 40f);
            using var font = new Font("Microsoft YaHei UI", fontSize, FontStyle.Regular);
            
            var textSize = g.MeasureString(text, font);
            
            float margin = textSize.Width * 0.1f;
            float x = bitmap.Width - textSize.Width - margin;
            float y = bitmap.Height - textSize.Height - margin;
            
            using var shadowBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
            g.DrawString(text, font, shadowBrush, x + 2, y + 2);
            
            using var textBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            g.DrawString(text, font, textBrush, x, y);
        }
    }
}
