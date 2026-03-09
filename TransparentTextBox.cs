using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

/// <summary>
/// 支持透明背景的自定义 TextBox
/// </summary>
public class TransparentTextBox : TextBox
{
    // 透明度值（0-255，0 全透，255 不透明）
    private int _transparency = 180; // 默认半透明
    
    // 边框宽度
    private int _borderWidth = 1; // 默认边框宽度
    
    // 边框颜色
    private Color _borderColor = Color.Transparent; // 默认边框颜色
    
    // 焦点时的边框颜色
    private Color _focusBorderColor = Color.Transparent; // 焦点时的边框颜色
    
    // 圆角半径
    private int _cornerRadius = 4; // 默认圆角半径

    public TransparentTextBox()
    {
        // 开启双缓冲，避免闪烁
        DoubleBuffered = true;
        // 关闭原生背景绘制
        SetStyle(ControlStyles.SupportsTransparentBackColor | 
                 ControlStyles.Opaque | 
                 ControlStyles.DoubleBuffer |
                 ControlStyles.UserPaint, true);
        BackColor = Color.Transparent; // 关键：设置背景为透明
        BorderStyle = BorderStyle.None; // 禁用默认边框，使用自定义边框
        // 启用多行模式
        Multiline = true;
        // 禁用自动滚动条
        ScrollBars = ScrollBars.None;
        // 不设置默认字体，使用外部设置的字体
    }

    // 重写创建参数，移除原生背景
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT：让控件忽略背景，透显父控件
            return cp;
        }
    }

    // 重写背景绘制，自定义透明效果
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // 不调用基类方法（避免绘制默认背景）
        if (Parent != null)
        {
            // 绘制半透明背景（带圆角）
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(_transparency, BackColor)))
            {
                if (_cornerRadius > 0)
                {
                    e.Graphics.FillRoundedRectangle(brush, ClientRectangle, _cornerRadius);
                }
                else
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
        }
    }

    // 重写绘制，保证文字清晰并绘制自定义边框
    protected override void OnPaint(PaintEventArgs e)
    {
        // 不调用基类方法，完全自定义绘制
        if (Parent != null)
        {
            // 绘制半透明背景（带圆角）
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(_transparency, BackColor)))
            {
                if (_cornerRadius > 0)
                {
                    e.Graphics.FillRoundedRectangle(brush, ClientRectangle, _cornerRadius);
                }
                else
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
            
            // 计算文本绘制区域（考虑内边距）
            Rectangle textRect = ClientRectangle;
            textRect.Inflate(-Padding.Left, -Padding.Top);
            
            // 绘制文字
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor, BackColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            
            // 绘制自定义边框
            if (_borderWidth > 0)
            {
                // 根据是否获得焦点选择边框颜色
                Color borderColor = Focused ? _focusBorderColor : _borderColor;
                using (Pen pen = new Pen(borderColor, _borderWidth))
                {
                    // 绘制带圆角的边框
                    if (_cornerRadius > 0)
                    {
                        e.Graphics.DrawRoundedRectangle(pen, ClientRectangle, _cornerRadius);
                    }
                    else
                    {
                        // 绘制边框，考虑边框宽度
                        int offset = _borderWidth / 2;
                        e.Graphics.DrawRectangle(pen, offset, offset, ClientSize.Width - _borderWidth, ClientSize.Height - _borderWidth);
                    }
                }
            }
        }
    }
    
    // 重写鼠标点击事件，确保点击时的透明度一致
    protected override void OnMouseDown(MouseEventArgs e)
    {
        // 调用基类方法以确保正常的鼠标事件处理
        base.OnMouseDown(e);
        Focus();
    }
    
    // 重写获得焦点事件，确保获得焦点时的视觉效果
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        // 保存当前大小
        int currentWidth = Width;
        int currentHeight = Height;
        Invalidate(); // 强制重绘，更新焦点状态
        // 确保大小不变
        Width = currentWidth;
        Height = currentHeight;
    }
    
    // 重写失去焦点事件，确保失去焦点时的视觉效果
    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        // 保存当前大小
        int currentWidth = Width;
        int currentHeight = Height;
        Invalidate(); // 强制重绘，更新焦点状态
        // 确保大小不变
        Width = currentWidth;
        Height = currentHeight;
    }
    
    // 重写文本改变事件，确保输入文字后的显示一致
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        // 保存当前高度，只调整宽度
        int currentHeight = Height;
        
        // 计算文本所需宽度
        if (!string.IsNullOrEmpty(Text))
        {
            // 使用更精确的方法计算文本宽度
            using (Graphics g = CreateGraphics())
            {
                // 使用Graphics.MeasureString获取更准确的文本宽度
                SizeF textSizeF = g.MeasureString(Text, Font);
                int textWidth = (int)Math.Ceiling(textSizeF.Width);
                // 考虑内边距和一些额外空间
                int newWidth = Math.Max(200, Math.Min(800, textWidth + Padding.Left + Padding.Right + 30));
                Width = newWidth;
            }
        }
        else
        {
            // 空文本时恢复默认宽度
            Width = 200;
        }
        
        // 确保高度不变
        Height = currentHeight;
        Invalidate(); // 强制重绘，确保显示一致
    }
    
    // 重写布局事件，确保大小一致
    protected override void OnLayout(LayoutEventArgs levent)
    {
        // 保存当前大小
        int currentWidth = Width;
        int currentHeight = Height;
        base.OnLayout(levent);
        // 确保大小不变
        Width = currentWidth;
        Height = currentHeight;
    }
    
    // 重写键盘事件，确保输入时的正常处理
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
    }
    
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
    }
    
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
    }
    
    // 重写鼠标事件，确保正常的鼠标交互
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
    }
    
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
    }
}

// 扩展方法，用于绘制圆角矩形
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle rect, int cornerRadius)
    {
        using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
        {
            graphics.FillPath(brush, path);
        }
    }
    
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rect, int cornerRadius)
    {
        using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
        {
            graphics.DrawPath(pen, path);
        }
    }
    
    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
    {
        GraphicsPath path = new GraphicsPath();
        
        // 左上角
        path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
        // 右上角
        path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
        // 右下角
        path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
        // 左下角
        path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
        
        path.CloseFigure();
        return path;
    }
}