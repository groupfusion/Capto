using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 箭头样式枚举
    /// </summary>
    public enum ArrowStyle
    {
        /// <summary>
        /// 普通箭头（仅边框）
        /// </summary>
        Normal,
        /// <summary>
        /// 实心箭头
        /// </summary>
        Solid,
        /// <summary>
        /// 双线箭头
        /// </summary>
        DoubleLine
    }

    /// <summary>
    /// 箭头绘制对象（适配 .NET 9）
    /// </summary>
    public class ArrowDrawObject : DrawObject, IDisposable
    {
        private Point _start;
        private Point _end;
        private ArrowStyle _arrowStyle;
        
        // 可配置参数（暴露为属性，方便外部调整）
        public int ArrowLength { get; set; } = 15;
        public double ArrowAngle { get; set; } = Math.PI / 6;
        public bool IsDoubleArrow { get; set; } = false; // 是否双向箭头
        public float DoubleLineOffset { get; set; } = 2f; // 双线箭头偏移量

        /// <summary>
        /// 起始点（支持修改）
        /// </summary>
        public Point StartPoint
        {
            get => _start;
            set => _start = value;
        }

        /// <summary>
        /// 结束点（支持修改）
        /// </summary>
        public Point EndPoint
        {
            get => _end;
            set => _end = value;
        }

        /// <summary>
        /// 箭头样式
        /// </summary>
        public ArrowStyle Style
        {
            get => _arrowStyle;
            set => _arrowStyle = value;
        }

        // 缓存画笔（避免重复创建）
        private Pen _cachedPen;
        private SolidBrush _cachedBrush;

        public ArrowDrawObject(Point start, Point end, Pen pen) : base(pen)
        {
            _start = start;
            _end = end;
            _arrowStyle = ArrowStyle.Normal;
            InitCachedObjects();
        }

        public ArrowDrawObject(Point start, Point end, Pen pen, ArrowStyle style) : base(pen)
        {
            _start = start;
            _end = end;
            _arrowStyle = style;
            InitCachedObjects();
        }

        /// <summary>
        /// 初始化缓存的绘图对象（减少GC）
        /// </summary>
        private void InitCachedObjects()
        {
            if (Pen == null) return;
            _cachedPen = new Pen(Pen.Color, Pen.Width)
            {
                LineJoin = LineJoin.Round, // 避免箭头边角锯齿
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            _cachedBrush = new SolidBrush(Pen.Color);
        }

        /// <summary>
        /// 核心绘制方法（.NET 9 优化）
        /// </summary>
        public override void Draw(Graphics g)
        {
            Draw(g, Point.Empty);
        }

        /// <summary>
        /// 带偏移量的绘制（修复偏移方向）
        /// </summary>
        public override void Draw(Graphics g, Point offset)
        {
            if (g == null || Pen == null)
            {
                Debug.WriteLine("绘图对象或画笔为空，跳过绘制");
                return;
            }

            try
            {
                // 正确的偏移逻辑：在截图上绘制时，需要减去截图区域的左上角坐标
                var start = new Point(_start.X - offset.X, _start.Y - offset.Y);
                var end = new Point(_end.X - offset.X, _end.Y - offset.Y);

                // .NET 9 高画质绘制配置（抗锯齿）
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 绘制主线
                g.DrawLine(_cachedPen, start, end);

                // 绘制箭头（支持双向）
                DrawArrow(g, start, end);
                if (IsDoubleArrow)
                {
                    DrawArrow(g, end, start); // 反向绘制起点箭头
                }
            }
            catch (Exception ex)
            {
                // 记录异常（而非忽略），方便调试
                Debug.WriteLine($"箭头绘制失败：{ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 按样式绘制箭头（核心优化）
        /// </summary>
        private void DrawArrow(Graphics g, Point lineStart, Point lineEnd)
        {
            try
            {
                // 计算箭头角度（基于线段方向）
                double angle = Math.Atan2(lineEnd.Y - lineStart.Y, lineEnd.X - lineStart.X);
                // 动态调整箭头长度（关联画笔宽度，避免变形）
                int actualArrowLength = Math.Max(ArrowLength, (int)(_cachedPen.Width * 3));
                
                // 计算箭头两侧点
                Point arrowPoint1 = new Point(
                    lineEnd.X - (int)(actualArrowLength * Math.Cos(angle - ArrowAngle)),
                    lineEnd.Y - (int)(actualArrowLength * Math.Sin(angle - ArrowAngle))
                );
                Point arrowPoint2 = new Point(
                    lineEnd.X - (int)(actualArrowLength * Math.Cos(angle + ArrowAngle)),
                    lineEnd.Y - (int)(actualArrowLength * Math.Sin(angle + ArrowAngle))
                );

                // 按样式绘制箭头
                switch (_arrowStyle)
                {
                    case ArrowStyle.Normal:
                        // 普通箭头：仅绘制边框，不填充
                        g.DrawLine(_cachedPen, lineEnd, arrowPoint1);
                        g.DrawLine(_cachedPen, lineEnd, arrowPoint2);
                        break;
                    case ArrowStyle.Solid:
                        // 实心箭头：填充+边框
                        Point[] solidPoints = { lineEnd, arrowPoint1, arrowPoint2 };
                        g.FillPolygon(_cachedBrush, solidPoints);
                        g.DrawPolygon(_cachedPen, solidPoints);
                        break;
                    case ArrowStyle.DoubleLine:
                        // 双线箭头：绘制双层线条
                        var penDouble = new Pen(_cachedPen.Color, _cachedPen.Width) { DashStyle = DashStyle.Solid };
                        using (penDouble)
                        {
                            // 计算双线偏移点
                            Point offsetPoint1 = GetOffsetPoint(arrowPoint1, lineEnd, DoubleLineOffset);
                            Point offsetPoint2 = GetOffsetPoint(arrowPoint2, lineEnd, DoubleLineOffset);
                            // 绘制外层线条
                            g.DrawLine(penDouble, lineEnd, offsetPoint1);
                            g.DrawLine(penDouble, lineEnd, offsetPoint2);
                            // 绘制内层线条
                            g.DrawLine(_cachedPen, lineEnd, arrowPoint1);
                            g.DrawLine(_cachedPen, lineEnd, arrowPoint2);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"箭头头部绘制失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 计算偏移点（用于双线箭头）
        /// </summary>
        private Point GetOffsetPoint(Point original, Point center, float offset)
        {
            // 计算从中心到原始点的向量
            int dx = original.X - center.X;
            int dy = original.Y - center.Y;
            // 归一化向量并偏移
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0) return original;
            float offsetX = (float)(dy / length * offset);
            float offsetY = (float)(-dx / length * offset);
            return new Point((int)(original.X + offsetX), (int)(original.Y + offsetY));
        }

        /// <summary>
        /// 释放资源（关键：解决GDI+泄漏）
        /// </summary>
        public override void Dispose()
        {
            _cachedPen?.Dispose();
            _cachedBrush?.Dispose();
            Pen?.Dispose(); // 释放基类画笔
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 空实现（保持接口兼容）
        /// </summary>
        public override void AddPoint(Point point) { }

        // 析构函数：兜底释放资源
        ~ArrowDrawObject()
        {
            Dispose();
        }
    }


}