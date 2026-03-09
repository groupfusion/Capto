using System;
using System.Drawing;

namespace Capto.DrawObjects
{
    /// <summary>
    /// 文本绘制对象
    /// 用于绘制文本
    /// </summary>
    public class TextDrawObject : DrawObject
    {
        private Point _location;
        private string _text;
        private Font _font;
        private Color _textColor;

        /// <summary>
        /// 位置
        /// </summary>
        public Point Location
        {
            get { return _location; }
        }

        /// <summary>
        /// 文本
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="location">位置</param>
        /// <param name="text">文本</param>
        /// <param name="textColor">文本颜色</param>
        public TextDrawObject(Point location, string text, Color textColor) : base(new Pen(Color.Black))
        {
            _location = location;
            _text = text;
            _textColor = textColor;
            _font = new Font("Arial", 12, FontStyle.Regular);
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="g">绘图对象</param>
        public override void Draw(Graphics g)
        {
            if (g == null || _font == null) return;
            
            try
            {
                using (var brush = new SolidBrush(_textColor))
                {
                    g.DrawString(_text, _font, brush, _location);
                }
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
            if (g == null || _font == null) return;
            
            try
            {
                var location = new Point(_location.X - offset.X, _location.Y - offset.Y);
                using (var brush = new SolidBrush(_textColor))
                {
                    g.DrawString(_text, _font, brush, location);
                }
            }
            catch (Exception)
            {
                // 忽略绘制异常，避免程序崩溃
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _font?.Dispose();
        }

        /// <summary>
        /// 添加点
        /// 文本工具不需要添加点，因为它是通过起点和输入的文本确定的
        /// </summary>
        /// <param name="point">点</param>
        public override void AddPoint(Point point)
        {
            // 文本工具不需要添加点，因为它是通过起点和输入的文本确定的
        }
    }
}