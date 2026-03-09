using System.Collections.Generic;
using System;
using System.Drawing;
using System.Linq;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 画笔绘制对象
    /// 用于绘制自由曲线
    /// </summary>
    public class PenDrawObject : DrawObject
    {
        private List<Point> _points;
        private const int MAX_POINTS = 5000; // 最大点数量限制

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="pen">画笔</param>
        public PenDrawObject(Point start, Pen pen) : base(pen)
        {
            _points = new List<Point>();
            _points.Add(start);
        }

        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="point">点</param>
        public override void AddPoint(Point point)
        {
            // 只添加与上一个点有一定距离的点，减少点的数量
            if (_points.Count == 0 || Distance(_points[^1], point) > 2) // 增加距离阈值到2
            {
                // 限制点数量，超过最大值时简化点
                if (_points.Count >= MAX_POINTS)
                {
                    SimplifyPoints();
                }
                _points.Add(point);
            }
        }

        /// <summary>
        /// 简化点数据
        /// </summary>
        private void SimplifyPoints()
        {
            if (_points.Count <= 1000) return;

            // 每隔一个点保留一个
            var simplifiedPoints = new List<Point>();
            for (int i = 0; i < _points.Count; i += 2)
            {
                simplifiedPoints.Add(_points[i]);
            }
            // 确保包含最后一个点
            if (simplifiedPoints.Last() != _points.Last())
            {
                simplifiedPoints.Add(_points.Last());
            }
            _points = simplifiedPoints;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="g">绘图对象</param>
        public override void Draw(Graphics g)
        {
            if (_points.Count >= 2)
            {
                // 使用平滑曲线绘制线条
                DrawSmoothCurve(g, _points.ToArray());
            }
        }

        /// <summary>
        /// 绘制（带偏移量）
        /// </summary>
        /// <param name="g">绘图对象</param>
        /// <param name="offset">偏移量</param>
        public override void Draw(Graphics g, Point offset)
        {
            if (_points.Count >= 2)
            {
                var offsetPoints = new Point[_points.Count];
                for (int i = 0; i < _points.Count; i++)
                {
                    offsetPoints[i] = new Point(_points[i].X - offset.X, _points[i].Y - offset.Y);
                }
                DrawSmoothCurve(g, offsetPoints);
            }
        }

        /// <summary>
        /// 绘制平滑曲线
        /// </summary>
        /// <param name="g">绘图对象</param>
        /// <param name="points">点数组</param>
        private void DrawSmoothCurve(Graphics g, Point[] points)
        {
            if (g == null || points == null || points.Length < 2 || Pen == null) return;

            // 使用GraphicsPath绘制平滑曲线
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                try
                {
                    path.AddCurve(points, 0.5f); // 0.5f是曲线平滑度，值越大曲线越平滑
                    g.DrawPath(Pen, path);
                }
                catch (Exception)
                {
                    // 忽略绘制异常，避免程序崩溃
                }
            }
        }

        /// <summary>
        /// 尝试部分擦除
        /// </summary>
        /// <param name="eraserPoints">橡皮擦路径点</param>
        /// <param name="eraserSize">橡皮擦大小</param>
        /// <returns>如果擦除后还有点，则返回true；否则返回false</returns>
        public bool TryErase(List<Point> eraserPoints, int eraserSize)
        {
            if (_points.Count < 2) return false;

            // 创建一个新的点列表，用于存储未被擦除的点
            List<Point> remainingPoints = new List<Point>();
            
            // 检查每个线段是否与橡皮擦路径相交
            for (int i = 0; i < _points.Count - 1; i++)
            {
                Point p1 = _points[i];
                Point p2 = _points[i + 1];
                bool segmentErased = false;
                
                // 检查当前线段是否与橡皮擦路径中的任何线段相交
                for (int j = 0; j < eraserPoints.Count - 1; j++)
                {
                    Point e1 = eraserPoints[j];
                    Point e2 = eraserPoints[j + 1];
                    
                    // 检查线段是否与橡皮擦路径的线段相交
                    if (base.DoLinesIntersect(p1, p2, e1, e2, eraserSize))
                    {
                        segmentErased = true;
                        break;
                    }
                }
                
                // 如果当前线段没有被擦除，并且是第一个点或者前一个点没有被擦除，则添加当前点
                if (!segmentErased && (i == 0 || !remainingPoints.Contains(p1)))
                {
                    remainingPoints.Add(p1);
                }
            }
            
            // 检查最后一个点是否需要保留
            if (_points.Count > 0 && !remainingPoints.Contains(_points.Last()))
            {
                remainingPoints.Add(_points.Last());
            }
            
            // 更新点列表
            _points = remainingPoints;

            // 如果剩余的点少于2个，则返回false，表示需要移除整个对象
            return _points.Count >= 2;
        }
    }
}