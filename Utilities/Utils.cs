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
        private static readonly string _base64Icon = "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAM9UlEQVR4AezdP4xcVxXH8XvH8s5U/JGIEBIEgaiwC2ooYoSSoEi7diQMPQ3dTiKI6LC3QgoJ2aGiQ6IKuICdBUWCZl2QggKKJO6goTMUoYg0tuK53Am7zni1s/vezP1zzj1f6z3N7Mx7957zOc8/JY6lDBy/EEDArAABYHb0NI6AcwQATwEChgUIAMPDp3XbAovuCYCFAicCRgUIAKODp20EFgIEwEKBEwGjAgSA0cHTtm2Bk+4JgBMJXhEwKEAAGBw6LSNwIkAAnEjwioBBAQLA4NBp2bbAcvcEwLIG7xEwJkAAGBs47SKwLEAALGvwHgFjAgSAsYHTrm2B090TAKdF+BkBQwIEgKFh0yoCpwUIgNMi/IyAIQECwNCwadW2wFndEwBnqfAZAkYECAAjg6ZNBM4SIADOUuEzBIwIEABGBk2btgVWdU8ArJLhcwQMCBAABoZMiwisEiAAVsnwOQIGBAgAA0OmRdsC53VPAJynw3cINC5AADQ+YNpD4DwBAuA8Hb5DoHEBAqDxAdOebYGLuhcbAJff+OPXtvanPxnuT/80nEz/OZpMQ+XzNxdhrvN95Z76mDbZf3y2/hvPd0b70zfjs/YD99rhZ9aZo9Z75AXA67//wnBycOfS4NHfB97tee+e9c59SSswdcsWiM/WJ+J51Xn3vfis/XJ4OfwjBsKPZFedrjpRARB/4397eMm/453/TroWE60Ugk+0ks5ljPQfh7wIhJ+NJgd/cfsHn9U5rO5ViwmA4S8On/POv+W9/2T38gte6X0ouJu8rcz1778+dP5t98bvPiVvGN0q6nKVjADYnz7tQ3izS8Fcg0ApAe/dl4eDS78utV+NfUQEwNCH12Pzn46n3MPIPwKvHIDR/r1z21uTwxdXuij/onoALP6030v8d37lg6X8dAI+zF9Nt5qslaoHwCX/6KYsEqpB4EkB7/1Xtn4+vfLkp7J/6lpd9QCIhT4bTw4ERAv4gfuW6ALXLE5AAITPr1k7tyFQTsCHL5bbrNxO9QPA+8+Va3eDncz9Z7BTVsb7j39O9dQpkSZ+rB8AWhgDfxFIy6iy1BnCKMu6GRbtsyQB0EeLaxFoTIAAaGygtINAHwECoI8W1yLQmAAB0NhAace2QN/uCYC+YlyPQEMCBEBDw6QVBPoKEAB9xbgegYYECICGhkkrtgXW6Z4AWEeNexBoRKDRAPC33dx/M+U5H4TbWWaeuM6UPS+vpar/4I5c6l+N/lXoJgNgHub3Zi9vH6U8H+7euJf6mVqsl7LGnGtp6j/48J+FbdKz0b8K3mQAJB08iyGgQGDdEgmAdeW4D4EGBAiABoZICwisK0AArCvHfQg0IEAANDBEWrAtsEn3BMAmetyLgHIBAkD5ACkfgU0ECIBN9LgXAeUCBIDyAVK+bYFNuycANhXkfgQUCxAAiodH6QhsKkAAbCrI/QgoFiAAFA+P0m0LpOieAEihyBoIKBUgAJQOjrIRSCFAAKRQZA0ElAoQAEoHR9m2BVJ1TwCkkmQdBBQKEAAKh0bJCKQSkBAAd2MzSc/BwN+Pa3IYFfBu8K5zLukz5fzgvbhmc0f1AJiNd64lP3d3FsNvblg01E1gNt7eS/5MxTW77Z7/qpQ7VA+AlM2wFgII9BMgAPp5cTUCTQkQAE2Nk2YQ6CdAAPTz4moEqgqk3pwASC3KeggoEiAAFA2LUhFILUAApBZlPQQUCRAAioZFqbYFcnRPAORQZU0ElAgQAEoGRZkI5BAgAHKosiYCSgQIACWDokzbArm6JwByybIuAgoECAAFQ6JEBHIJEAC5ZFkXAQUCBICCIVGibYGc3RMAOXVZGwHhAgSA8AFRHgI5BQiAnLqsjYBwAQJA+IAoz7ZA7u6rB8BoMg2pz639g5u54VhfrsBwcnAn9TM12j/4rdyO16+segCsXzp3IoDApgIEwKaC3I+AYgECQPHwKL1tgRLdEQAllNkDAaECBIDQwVAWAiUECIASyuyBgFABAkDoYCjLtkCp7gmAUtLsg4BAAQJA4FAoCYFSAgRAKWn2QUCgAAEgcCiUZFugZPcEQElt9kJAmAABIGwglINASQECoKQ2eyEgTIAAEDYQyrEtULp7AqC0OPshIEiAABA0DEpBoLQAAVBanP0QECRAAAgaBqXYFqjRPQFQQ509ERAiQAAIGQRlIFBDgACooc6eCAgRIACEDIIybAvU6p4AqCXPvggIECAABAyBEhCoJUAA1JJnXwQECBAAAoZACbYFanZPANTUZ28EKgsQAJUHwPYI1BQgAGrqszcClQUIgMoDYHvbArW7JwBqT4D9kws8GF+/ORvv+KTnS9e/m7xQAQsSAAKGQAkI1BIgAGrJsy8CAgQIAAFDoASbAhK6JgAkTIEaEKgkQABUgmdbBCQIEAASpkANCFQSIAAqwbOtbQEp3RMAUiZBHQhUECAAKqCzJQJSBCQEwN2IkfQcDPz9uCYHAghcIFA9AGbjnWvJz92dRaBc0DpfI1BHQNKu1QNAEga1IGBNgACwNnH6RWBJgABYwuAtAtYECABrE6ffqgLSNicApE2EehAoKEAAFMRmKwSkCRAA0iZCPQgUFCAACmKzlW0Bid0TABKnQk0IFBIgAApBs005gdHk8NZoMj1Kex7eKtdBuZ0IgHLW7FRIILj51bjVM0nPML8S12vuIACaGykNSRSQWhMBIHUy1IVAAQECoAAyWyAgVYAAkDoZ6kKggAABUACZLWwLSO6eAJA8HWpDILMAAZAZmOURkCxAAEieDrUhkFmAAMgMzPK2BaR3Xz0ARpNpSH1u7R/clA5PfQhIEKgeABIQqAEBqwIEgNXJ0zcCUYAAiAgcCOQQ0LAmAaBhStSIQCYBAiATLMsioEGAANAwJWpEIJMAAZAJlmVtC2jpngDQMinqRCCDAAGQAZUlEdAiQABomRR1IpBBgADIgMqStgU0dU8AaJoWtSKQWIAASAzKcghoEiAANE2LWhFILEAAJAZlOdsC2ronALRNjHoRSChAACTEZCkEtAkQANomRr0IJBQgABJispRtAY3dEwAap0bNCCQSaDIAvHe3R5PpUdrz8FYi8yeWSVtj/56fKCbRD6PJ4a1Rcv/uvXnnn0nUysfLeH+tZk+jaPpxMenetRkAzn81Ei0egnRnmF+Ja+Y40tXo3DprJe8puPnVuOg6taS656m4f+pjsWaq+vqvk+n5azIAUk+e9RC4SEDr9wSA1slRNwIJBAiABIgsgYBWAQJA6+SoG4EEAgRAAkSWsC2guXsCQPP0qB2BDQUIgA0BuR0BzQIEgObpUTsCGwoQABsCcrttAe3dVw+AENwH2hGpHwGtAtUDwHl3XysedSOgXaB+AAT3N+2I1I+AVoHqAeCD+4NWPOq2LdBC99UDYPbh8E4I4f0WMOkBAW0C1QPAvfL8B967iTY46kWgBYH6ARAVZ/7yT+PLX+PJgQACBQVEBIDbfeHBbP7oeefC2wV7Z6tMAvHPdUKmpcUs20ohMgJgofnyi+/Pxte/EZ+c1xY/ijt9fKzFFSW0IKzSDyaTqZwAOCZ7MN55Ze7nV0Jwbx1/xAsCCGQSEBcAiz4f7t649+ClnRdmH86fnrvw47lzv4qB8Of43d16p78X985xVOzJLfbO0NNHVou1a53/ztDUYs1a/cR9PzJN3pbIAHjc5Q9v/Ovh+PqrD8c734+B8NxsvHOt3rm997iuhG/q9fN/y4StPF5qNt7eq9lXcCH+hnlcTpo3IRyd9FTnNc/zN0ijwyoIIKBRgADQODVqRiCRAAGQCJJlENAoQABonBo1VxNobWMCoLWJ0g8CPQQIgB5YXIpAawIEQGsTpR8EeggQAD2wuNS2QIvdEwAtTpWeEOgoQAB0hOIyBFoUIABanCo9IdBRgADoCMVltgVa7Z4AaHWy9IVABwECoAMSlyDQqgAB0Opk6QuBDgIEQAckLrEt0HL3BEDL06U3BC4QIAAuAOJrBFoWIABani69IXCBAAFwARBf2xZovXsCoPUJ0x8C5wgQAOfg8BUCrQsQAK1PmP4QOEeAADgHh69sC1jongCwMGV6RGCFAAGwAoaPEbAgQABYmDI9IrBCgABYAcPHtgWsdE8AWJk0fSJwhgABcAYKHyFgRYAAsDJpQ316N3jXOXc36ekH78X1mjsIgOZGSkOz8fbeJv8P/7Pv3d5rUZYAaHGq9IRARwECoCMUlyHQogAB0OJU6QmBjgIEQEcoLrMhYK1LAsDaxOkXgSUBAmAJg7cIWBMgAKxNnH4RWBIgAJYweGtbwGL3BIDFqdMzAscCBMAxBC8IWBQgACxOnZ4ROBYgAI4heLEtYLV7AsDq5OkbgShAAEQEDgSsChAAVidP3whEAQIgInDYFrDcPQFgefr0bl6AADD/CABgWYAAsDx9ejcvQACYfwRsA1jv/n8AAAD//zGJ3X4AAAAGSURBVAMAeI0naoamUe0AAAAASUVORK5CYII=";

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
                return  System.Drawing.SystemIcons.Information;;
            }
        }

        public static Icon GetAppIcon()
        {
            return LoadIconFromBase64(_base64Icon);
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
