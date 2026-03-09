using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Capto.Utilities
{
    public static class IconUtils
    {
        private static readonly Dictionary<string, Bitmap> _iconCache = new();

        public static Bitmap CreateColorBlock(Color color, int width = 24, int height = 16)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                using var brush = new SolidBrush(color);
                using var pen = new Pen(Color.LightGray, 1);
                g.FillRectangle(brush, 0, 0, width, height);
                g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
            }
            return bitmap;
        }

        public static string GetColorName(Color color)
        {
            var colorNames = typeof(Color).GetProperties(
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(Color))
                .Select(p => new { Name = p.Name, Color = (Color)p.GetValue(null) })
                .GroupBy(c => c.Color.ToArgb())
                .ToDictionary(g => g.Key, g => g.First().Name);

            if (colorNames.TryGetValue(color.ToArgb(), out var name))
            {
                var nameMap = new Dictionary<string, string>
                {
                    {"Black", "黑色"}, {"DarkGray", "深灰"}, {"Gray", "灰色"},
                    {"LightGray", "浅灰"}, {"White", "白色"}, {"DarkRed", "深红"},
                    {"Red", "红色"}, {"Orange", "橙色"}, {"Yellow", "黄色"},
                    {"Lime", "青绿"}, {"Green", "绿色"}, {"Cyan", "青色"},
                    {"Blue", "蓝色"}, {"DarkBlue", "深蓝"}, {"Purple", "紫色"},
                    {"Pink", "粉色"}, {"Brown", "棕色"}, {"Maroon", "栗色"},
                    {"Olive", "橄榄"}, {"Teal", "蓝绿"}
                };
                return nameMap.TryGetValue(name, out var chineseName) ? chineseName : name;
            }
            return name ?? "";
        }

        public static Icon LoadIconFromBase64(string base64Icon)
        {
            try
            {
                byte[] iconBytes = Convert.FromBase64String(base64Icon);
                using var stream = new MemoryStream(iconBytes);
                using var bmp = new Bitmap(stream);
                return Icon.FromHandle(bmp.GetHicon());
            }
            catch
            {
                return SystemIcons.Error;
            }
        }
    }

    public static class DrawUtils
    {
        public static Point GetOffsetPoint(Point point, Rectangle rect)
        {
            return new Point(point.X - rect.X, point.Y - rect.Y);
        }

        public static Rectangle NormalizeRect(Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);
            return new Rectangle(x, y, width, height);
        }
    }
}
