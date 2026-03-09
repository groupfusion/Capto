using System;
using System.Collections.Generic;
using System.Drawing;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 绘制对象抽象类
    /// 所有绘制对象的基类
    /// </summary>
    public abstract class DrawObject : IDisposable
    {
        /// <summary>
        /// 画笔
        /// </summary>
        protected Pen Pen { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pen">画笔</param>
        public DrawObject(Pen pen)
        {
            if (pen != null)
            {
                // 创建一个新的Pen对象，避免共享同一个Pen引用
                Pen = new Pen(pen.Color, pen.Width);
            }
        }

        /// <summary>
        /// 绘制方法
        /// </summary>
        /// <param name="g">绘图对象</param>
        public abstract void Draw(Graphics g);

        /// <summary>
        /// 绘制方法（带偏移量）
        /// </summary>
        /// <param name="g">绘图对象</param>
        /// <param name="offset">偏移量</param>
        public virtual void Draw(Graphics g, Point offset)
        {
            Draw(g);
        }

        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="point">点</param>
        public abstract void AddPoint(Point point);

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            Pen?.Dispose();
        }

        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        /// <param name="p1">点1</param>
        /// <param name="p2">点2</param>
        /// <returns>距离</returns>
        protected double Distance(Point p1, Point p2)
        {
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 检查点是否在线段附近
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="lineStart">线段起点</param>
        /// <param name="lineEnd">线段终点</param>
        /// <param name="tolerance">容差</param>
        /// <returns>如果点在线段附近，返回true；否则返回false</returns>
        protected bool IsPointNearLine(Point point, Point lineStart, Point lineEnd, double tolerance)
        {
            // 计算点到线段的距离
            double dist = DistanceToLine(point, lineStart, lineEnd);
            return dist < tolerance;
        }

        /// <summary>
        /// 计算点到线段的距离
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="lineStart">线段起点</param>
        /// <param name="lineEnd">线段终点</param>
        /// <returns>点到线段的距离</returns>
        protected double DistanceToLine(Point point, Point lineStart, Point lineEnd)
        {
            // 计算线段的向量
            int vx = lineEnd.X - lineStart.X;
            int vy = lineEnd.Y - lineStart.Y;

            // 计算点到线段起点的向量
            int wx = point.X - lineStart.X;
            int wy = point.Y - lineStart.Y;

            // 计算点积
            double c1 = wx * vx + wy * vy;
            if (c1 <= 0)
            {
                // 点在起点的后面
                return Distance(point, lineStart);
            }

            // 计算线段长度的平方
            double c2 = vx * vx + vy * vy;
            if (c2 <= c1)
            {
                // 点在终点的后面
                return Distance(point, lineEnd);
            }

            // 计算点到线段的垂直距离
            double b = c1 / c2;
            Point pb = new Point(
                lineStart.X + (int)(b * vx),
                lineStart.Y + (int)(b * vy)
            );
            return Distance(point, pb);
        }

        /// <summary>
        /// 检查两条线段是否相交，考虑容差
        /// </summary>
        /// <param name="p1">第一条线段的起点</param>
        /// <param name="p2">第一条线段的终点</param>
        /// <param name="e1">第二条线段的起点</param>
        /// <param name="e2">第二条线段的终点</param>
        /// <param name="tolerance">容差</param>
        /// <returns>如果两条线段相交，返回true；否则返回false</returns>
        protected bool DoLinesIntersect(Point p1, Point p2, Point e1, Point e2, double tolerance)
        {
            // 检查第一条线段的两个端点是否在第二条线段的扩展区域内
            if (IsPointNearLine(p1, e1, e2, tolerance) || IsPointNearLine(p2, e1, e2, tolerance))
            {
                return true;
            }
            
            // 检查第二条线段的两个端点是否在第一条线段的扩展区域内
            if (IsPointNearLine(e1, p1, p2, tolerance) || IsPointNearLine(e2, p1, p2, tolerance))
            {
                return true;
            }
            
            return false;
        }
    }
}