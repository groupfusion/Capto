using System;
using System.Drawing;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 矩形绘制对象
    /// 用于绘制矩形
    /// </summary>
    public class RectangleDrawObject : DrawObject
    {
        private Rectangle _rectangle;

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
        /// <param name="pen">画笔</param>
        public RectangleDrawObject(Rectangle rect, Pen pen) : base(pen)
        {
            _rectangle = rect;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="g">绘图对象</param>
        public override void Draw(Graphics g)
        {
            if (g == null || Pen == null) return;
            
            try
            {
                g.DrawRectangle(Pen, _rectangle);
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
            if (g == null || Pen == null) return;
            
            try
            {
                var rect = new Rectangle(_rectangle.X - offset.X, _rectangle.Y - offset.Y, _rectangle.Width, _rectangle.Height);
                g.DrawRectangle(Pen, rect);
            }
            catch (Exception)
            {
                // 忽略绘制异常，避免程序崩溃
            }
        }

        /// <summary>
        /// 添加点
        /// 矩形工具不需要添加点，因为它是通过起点和终点确定的
        /// </summary>
        /// <param name="point">点</param>
        public override void AddPoint(Point point)
        {
            // 矩形工具不需要添加点，因为它是通过起点和终点确定的
        }
    }
}