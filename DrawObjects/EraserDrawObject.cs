using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 橡皮擦绘制对象
    /// 用于擦除其他绘制对象
    /// </summary>
    public class EraserDrawObject : DrawObject
    {
        private List<Point> _points;
        private int _eraserSize;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="eraserSize">橡皮擦大小</param>
        public EraserDrawObject(Point start, int eraserSize) : base(new Pen(Color.White, eraserSize))
        {
            _points = new List<Point>();
            _points.Add(start);
            _eraserSize = eraserSize;
            // 设置画笔为白色，模拟擦除效果
            Pen.Color = Color.White;
            Pen.Width = eraserSize;
            Pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            Pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        }

        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="point">点</param>
        public override void AddPoint(Point point)
        {
            _points.Add(point);
        }

        /// <summary>
        /// 绘制
        /// 实现橡皮擦的留影效果，从浅到深，从细到粗，只显示最近的5-6个点，使线条更流畅
        /// </summary>
        /// <param name="g">绘图对象</param>
        public override void Draw(Graphics g)
        {
            // 绘制橡皮擦的留影效果，只显示最近的5-6个点
            if (_points.Count > 1)
            {
                // 确定要显示的点的起始索引
                int startIndex = Math.Max(0, _points.Count - 6);
                int totalPoints = _points.Count - startIndex;
                
                // 创建一个新的点列表，只包含要显示的点
                List<Point> displayPoints = _points.GetRange(startIndex, totalPoints);
                
                // 对显示点进行插值，增加中间点以提高流畅度
                List<Point> smoothedPoints = SmoothPoints(displayPoints, 3);
                
                // 绘制平滑后的路径，使用渐变效果
                if (smoothedPoints.Count > 1)
                {
                    for (int i = 0; i < smoothedPoints.Count - 1; i++)
                    {
                        // 计算当前点的比例，从0到1（基于平滑后的点）
                        float ratio = (float)i / (smoothedPoints.Count - 1);
                        // 透明度从低到高（50到150）
                        int alpha = (int)(50 * (1 - ratio) + 150);
                        // 线条宽度从细到粗（1像素到20像素）
                        float width = 1f * (1 - ratio) + 20f * ratio;
                        
                        using (var eraserPen = new Pen(Color.FromArgb(alpha, Color.White), width))
                        {
                            eraserPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                            eraserPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            g.DrawLine(eraserPen, smoothedPoints[i], smoothedPoints[i + 1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制（带偏移量）
        /// 实现橡皮擦的留影效果，从浅到深，从细到粗，只显示最近的5-6个点，使线条更流畅
        /// </summary>
        /// <param name="g">绘图对象</param>
        /// <param name="offset">偏移量</param>
        public override void Draw(Graphics g, Point offset)
        {
            // 绘制带偏移量的橡皮擦留影效果，只显示最近的5-6个点
            if (_points.Count > 1)
            {
                // 确定要显示的点的起始索引
                int startIndex = Math.Max(0, _points.Count - 6);
                int totalPoints = _points.Count - startIndex;
                
                // 创建一个新的点列表，只包含要显示的点，并应用偏移量
                List<Point> displayPoints = new List<Point>();
                for (int i = startIndex; i < _points.Count; i++)
                {
                    displayPoints.Add(new Point(_points[i].X - offset.X, _points[i].Y - offset.Y));
                }
                
                // 对显示点进行插值，增加中间点以提高流畅度
                List<Point> smoothedPoints = SmoothPoints(displayPoints, 3);
                
                // 绘制平滑后的路径，使用渐变效果
                if (smoothedPoints.Count > 1)
                {
                    for (int i = 0; i < smoothedPoints.Count - 1; i++)
                    {
                        // 计算当前点的比例，从0到1（基于平滑后的点）
                        float ratio = (float)i / (smoothedPoints.Count - 1);
                        // 透明度从低到高（50到150）
                        int alpha = (int)(50 * (1 - ratio) + 150);
                        // 线条宽度从细到粗（1像素到20像素）
                        float width = 1f * (1 - ratio) + 20f * ratio;
                        
                        using (var eraserPen = new Pen(Color.FromArgb(alpha, Color.White), width))
                        {
                            eraserPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                            eraserPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            g.DrawLine(eraserPen, smoothedPoints[i], smoothedPoints[i + 1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 对 points 列表进行平滑处理，增加中间点以提高流畅度
        /// </summary>
        /// <param name="points">原始点列表</param>
        /// <param name="interpolationCount">每两个点之间的插值点数</param>
        /// <returns>平滑后的点列表</returns>
        private List<Point> SmoothPoints(List<Point> points, int interpolationCount)
        {
            List<Point> smoothedPoints = new List<Point>();
            
            if (points.Count < 2)
            {
                return points;
            }
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point p1 = points[i];
                Point p2 = points[i + 1];
                
                // 添加第一个点
                smoothedPoints.Add(p1);
                
                // 在两个点之间插入中间点
                for (int j = 1; j <= interpolationCount; j++)
                {
                    float t = (float)j / (interpolationCount + 1);
                    int x = (int)(p1.X + (p2.X - p1.X) * t);
                    int y = (int)(p1.Y + (p2.Y - p1.Y) * t);
                    smoothedPoints.Add(new Point(x, y));
                }
            }
            
            // 添加最后一个点
            smoothedPoints.Add(points[points.Count - 1]);
            
            return smoothedPoints;
        }

        /// <summary>
        /// 执行擦除操作
        /// </summary>
        /// <param name="drawObjects">绘制对象列表</param>
        public void Erase(List<DrawObject> drawObjects)
        {
            if (_points.Count < 2) return;

            // 检查并移除与橡皮擦路径相交的绘制对象
            for (int i = drawObjects.Count - 1; i >= 0; i--)
            {
                if (drawObjects[i] is PenDrawObject penObject)
                {
                    // 对于PenDrawObject，检查是否与橡皮擦路径相交
                    if (IsIntersectWithEraser(penObject))
                    {
                        // 直接移除整个对象
                        drawObjects.RemoveAt(i);
                    }
                }
                else if (drawObjects[i] is RectangleDrawObject rectObject)
                {
                    // 对于RectangleDrawObject，检查是否与橡皮擦路径相交
                    if (IsIntersectWithEraser(rectObject))
                    {
                        drawObjects.RemoveAt(i);
                    }
                }
                else if (drawObjects[i] is CircleDrawObject circleObject)
                {
                    // 对于CircleDrawObject，检查是否与橡皮擦路径相交
                    if (IsIntersectWithEraser(circleObject))
                    {
                        drawObjects.RemoveAt(i);
                    }
                }
                else if (drawObjects[i] is ArrowDrawObject arrowObject)
                {
                    // 对于ArrowDrawObject，检查是否与橡皮擦路径相交
                    if (IsIntersectWithEraser(arrowObject))
                    {
                        drawObjects.RemoveAt(i);
                    }
                }
                else if (drawObjects[i] is BlurDrawObject blurObject)
                {
                    // 对于BlurDrawObject，检查是否与橡皮擦路径相交
                    if (IsIntersectWithEraser(blurObject))
                    {
                        drawObjects.RemoveAt(i);
                    }
                }
                else if (drawObjects[i] is TextDrawObject textObject)
                {
                    // 对于TextDrawObject，检查是否与橡皮擦路径相交
                    if (IsIntersectWithEraser(textObject))
                    {
                        drawObjects.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 检查画笔绘制对象是否与橡皮擦路径相交
        /// </summary>
        /// <param name="penObject">画笔绘制对象</param>
        /// <returns>如果相交，返回true；否则返回false</returns>
        private bool IsIntersectWithEraser(PenDrawObject penObject)
        {
            // 检查画笔绘制对象的每个线段是否与橡皮擦路径相交
            var points = GetPenObjectPoints(penObject);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point p1 = points[i];
                Point p2 = points[i + 1];
                
                // 检查当前线段是否与橡皮擦路径中的任何线段相交
                for (int j = 0; j < _points.Count - 1; j++)
                {
                    Point e1 = _points[j];
                    Point e2 = _points[j + 1];
                    
                    // 检查线段是否与橡皮擦路径的线段相交，使用更大的容差
                    if (base.DoLinesIntersect(p1, p2, e1, e2, _eraserSize + 10))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取画笔绘制对象的点列表
        /// </summary>
        /// <param name="penObject">画笔绘制对象</param>
        /// <returns>点列表</returns>
        private List<Point> GetPenObjectPoints(PenDrawObject penObject)
        {
            // 通过反射获取画笔绘制对象的点列表
            var pointsField = penObject.GetType().GetField("_points", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pointsField != null)
            {
                return pointsField.GetValue(penObject) as List<Point>;
            }
            return new List<Point>();
        }

        /// <summary>
        /// 检查矩形是否与橡皮擦路径相交
        /// </summary>
        /// <param name="rectObject">矩形绘制对象</param>
        /// <returns>如果相交，返回true；否则返回false</returns>
        private bool IsIntersectWithEraser(RectangleDrawObject rectObject)
        {
            // 这里需要实现矩形与橡皮擦路径相交的检查逻辑
            // 简化实现：检查橡皮擦路径中的任何点是否在矩形的扩展区域内
            foreach (var point in _points)
            {
                // 扩展矩形边界，考虑橡皮擦大小
                var extendedRect = new Rectangle(
                    rectObject.Rectangle.X - _eraserSize,
                    rectObject.Rectangle.Y - _eraserSize,
                    rectObject.Rectangle.Width + _eraserSize * 2,
                    rectObject.Rectangle.Height + _eraserSize * 2
                );

                if (extendedRect.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查圆形是否与橡皮擦路径相交
        /// </summary>
        /// <param name="circleObject">圆形绘制对象</param>
        /// <returns>如果相交，返回true；否则返回false</returns>
        private bool IsIntersectWithEraser(CircleDrawObject circleObject)
        {
            // 这里需要实现圆形与橡皮擦路径相交的检查逻辑
            // 简化实现：检查橡皮擦路径中的任何点是否在圆形的扩展区域内
            foreach (var point in _points)
            {
                // 计算圆心和半径
                var center = new Point(
                    circleObject.Rectangle.X + circleObject.Rectangle.Width / 2,
                    circleObject.Rectangle.Y + circleObject.Rectangle.Height / 2
                );
                var radius = Math.Min(circleObject.Rectangle.Width, circleObject.Rectangle.Height) / 2;

                // 检查点是否在扩展的圆形区域内
                if (base.Distance(point, center) < radius + _eraserSize)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查箭头是否与橡皮擦路径相交
        /// </summary>
        /// <param name="arrowObject">箭头绘制对象</param>
        /// <returns>如果相交，返回true；否则返回false</returns>
        private bool IsIntersectWithEraser(ArrowDrawObject arrowObject)
        {
            // 这里需要实现箭头与橡皮擦路径相交的检查逻辑
            // 简化实现：检查橡皮擦路径中的任何点是否在箭头的扩展区域内
            foreach (var point in _points)
            {
                // 检查点是否在箭头的扩展线段上，使用更大的容差提高灵敏度
                if (base.IsPointNearLine(point, arrowObject.StartPoint, arrowObject.EndPoint, _eraserSize + 15))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查模糊区域是否与橡皮擦路径相交
        /// </summary>
        /// <param name="blurObject">模糊绘制对象</param>
        /// <returns>如果相交，返回true；否则返回false</returns>
        private bool IsIntersectWithEraser(BlurDrawObject blurObject)
        {
            // 这里需要实现模糊区域与橡皮擦路径相交的检查逻辑
            // 简化实现：检查橡皮擦路径中的任何点是否在模糊区域的扩展区域内
            foreach (var point in _points)
            {
                // 扩展模糊区域边界，考虑橡皮擦大小
                var extendedRect = new Rectangle(
                    blurObject.Rectangle.X - _eraserSize,
                    blurObject.Rectangle.Y - _eraserSize,
                    blurObject.Rectangle.Width + _eraserSize * 2,
                    blurObject.Rectangle.Height + _eraserSize * 2
                );

                if (extendedRect.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查文本是否与橡皮擦路径相交
        /// </summary>
        /// <param name="textObject">文本绘制对象</param>
        /// <returns>如果相交，返回true；否则返回false</returns>
        private bool IsIntersectWithEraser(TextDrawObject textObject)
        {
            // 这里需要实现文本与橡皮擦路径相交的检查逻辑
            // 简化实现：检查橡皮擦路径中的任何点是否在文本区域的扩展区域内
            foreach (var point in _points)
            {
                // 扩展文本区域边界，考虑橡皮擦大小
                var extendedRect = new Rectangle(
                    textObject.Location.X - _eraserSize,
                    textObject.Location.Y - _eraserSize,
                    textObject.Text.Length * 10 + _eraserSize * 2, // 简化的文本宽度估算
                    20 + _eraserSize * 2 // 简化的文本高度估算
                );

                if (extendedRect.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }


    }
}