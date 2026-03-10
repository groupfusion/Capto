using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Capto.DrawObjects;
using Capto.Utilities;

namespace Capto
{
    /// <summary>
    /// 绘制模式枚举
    /// 定义了不同的绘图工具类型
    /// </summary>
    public enum DrawMode
    {
        /// <summary>画笔工具</summary>
        Pen,
        /// <summary>矩形工具</summary>
        Rectangle,
        /// <summary>圆形工具</summary>
        Circle,
        /// <summary>箭头工具</summary>
        Arrow,
        /// <summary>模糊工具</summary>
        Blur,
        /// <summary>文本工具</summary>
        Text,
        /// <summary>擦除工具</summary>
        Eraser
    }

    /// <summary>
    /// 截图表单类
    /// 实现完整的截图和绘图功能
    /// </summary>
    public class ScreenCaptureForm : Form
    {
        /// <summary>屏幕截图 bitmap</summary>
        private Bitmap? _screenBitmap;

        public ScreenCaptureForm() : this(null)
        {
        }

        public ScreenCaptureForm(Bitmap? screenBitmap)
        {
            _screenBitmap = screenBitmap;

            // 禁用高DPI缩放，确保截图时使用真实屏幕分辨率
            this.AutoScaleMode = AutoScaleMode.None;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            // 启用双缓冲，减少闪烁
            this.DoubleBuffered = true;

            // 最小化初始化
            // 不加载图标
            // 不初始化集合，只在需要时创建
            // 不创建画笔，只在需要时创建
            // 不创建CanvasTextBox，只在需要时创建

            // 只添加必要的事件处理程序
            this.MouseDown += ScreenCaptureForm_MouseDown;
            this.KeyDown += ScreenCaptureForm_KeyDown;
            this.Paint += ScreenCaptureForm_Paint;
        }

        /// <summary>鼠标开始位置</summary>
        private Point _startPoint;
        /// <summary>鼠标当前位置</summary>
        private Point _currentPoint;
        /// <summary>截图区域矩形</summary>
        private Rectangle _captureRect;
        /// <summary>是否正在拖动鼠标选择区域</summary>
        private bool _isDragging;
        /// <summary>当前画笔绘制对象</summary>
        private DrawObject? _currentPenObject;
        /// <summary>工具条管理器</summary>
        private ToolStripManager? _toolStripManager;
        /// <summary>绘制对象列表</summary>
        private List<DrawObject>? _drawObjects;
        /// <summary>当前画笔</summary>
        private Pen? _currentPen;
        /// <summary>画布上的文本输入框</summary>
        private TextBox? _canvasTextBox;
        /// <summary>文本列表</summary>
        private List<TextDrawObject>? _textList;

        // 事件处理程序添加标志
        private bool _eventHandlersAdded = false;

        /// <summary>
        /// 延迟添加事件处理程序
        /// </summary>
        private void AddEventHandlers()
        {
            if (!_eventHandlersAdded)
            {
                // 添加Paint事件处理程序
                this.Paint += ScreenCaptureForm_Paint;
                
                // 添加MouseMove事件处理程序
                this.MouseMove += ScreenCaptureForm_MouseMove;
                
                // 添加MouseUp事件处理程序
                this.MouseUp += ScreenCaptureForm_MouseUp;
                
                // 设置标志，表示事件处理程序已添加
                _eventHandlersAdded = true;
            }
        }

        /// <summary>
        /// 捕获整个屏幕
        /// </summary>
        private void CaptureScreen()
        {
            try
            {
                var screenBounds = Screen.GetBounds(Point.Empty); // 获取屏幕边界
                _screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height); // 创建屏幕大小的bitmap
                using (var g = Graphics.FromImage(_screenBitmap)) // 创建绘图对象
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size); // 复制屏幕内容到bitmap
                }
            }
            catch (Exception ex)
            {
                Capto.Utilities.Logger.Error($"Error capturing screen: {ex.Message}", ex);
                // 如果捕获屏幕失败，创建一个默认的空白 bitmap
                _screenBitmap = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(_screenBitmap))
                {
                    g.Clear(Color.White);
                }
            }
        }



        /// <summary>
        /// 初始化工具条
        /// </summary>
        private void InitializeToolStrip()
        {
            // 创建工具条管理器
            _toolStripManager = new ToolStripManager(this);
            
            // 添加事件处理
            if (_toolStripManager != null)
            {
                _toolStripManager.ToolSelected += ToolStripManager_ToolSelected;
                _toolStripManager.CopyClicked += ToolStripManager_CopyClicked;
                _toolStripManager.SaveClicked += ToolStripManager_SaveClicked;
                _toolStripManager.PenSizeChanged += ToolStripManager_PenSizeChanged;
                _toolStripManager.ColorChanged += ToolStripManager_ColorChanged;
            }
        }

        /// <summary>
        /// 工具选择事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ToolStripManager_ToolSelected(object sender, ToolSelectedEventArgs e)
        {
            // 工具选择事件处理
        }
        
        /// <summary>
        /// 画笔大小变化事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ToolStripManager_PenSizeChanged(object sender, EventArgs e)
        {
            if (_toolStripManager != null && _currentPen != null)
            {
                // 保存当前颜色
                Color currentColor = _currentPen.Color;
                // 释放旧的画笔
                _currentPen.Dispose();
                // 创建新的画笔，使用新的大小和当前颜色
                _currentPen = new Pen(currentColor, _toolStripManager.CurrentPenSize);
            }
        }
        
        /// <summary>
        /// 颜色变化事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ToolStripManager_ColorChanged(object sender, EventArgs e)
        {
            if (_toolStripManager != null && _currentPen != null)
            {
                // 保存当前大小
                float currentSize = _currentPen.Width;
                // 释放旧的画笔
                _currentPen.Dispose();
                // 创建新的画笔，使用当前大小和新的颜色
                _currentPen = new Pen(_toolStripManager.CurrentColor, currentSize);
            }
        }

        /// <summary>
        /// 复制按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ToolStripManager_CopyClicked(object sender, EventArgs e)
        {
            using (var captureImage = GetCaptureImage())
            {
                if (captureImage != null)
                {
                    Clipboard.SetImage(captureImage);
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 保存按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ToolStripManager_SaveClicked(object sender, EventArgs e)
        {
            using (var captureImage = GetCaptureImage())
            {
                if (captureImage != null)
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "PNG 图片|*.png";
                        sfd.FileName = $"截图_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            captureImage.Save(sfd.FileName, ImageFormat.Png);
                            this.Close();
                        }
                    }
                }
            }
        }



        /// <summary>
        /// 鼠标按下事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void ScreenCaptureForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            // 第一次点击时添加所有必要的事件处理程序
            AddEventHandlers();

            _startPoint = e.Location;
            _isDragging = true;

            // 第一次点击时初始化必要的对象
            if (_drawObjects == null)
            {
                _drawObjects = new List<DrawObject>();
            }
            if (_textList == null)
            {
                _textList = new List<TextDrawObject>();
            }
            if (_currentPen == null)
            {
                _currentPen = new Pen(Color.Red, 3);
            }

            // 如果工具条可见，处理绘制工具的初始化
            if (_toolStripManager != null && _toolStripManager.ToolStripPanel.Visible)
            {
                // 处理文本工具的特殊逻辑
                if (_toolStripManager.CurrentMode == DrawMode.Text && _canvasTextBox != null)
                {
                    // 重新计算字体高度和位置
                    using (Graphics g = this.CreateGraphics())
                    {
                        // 测量字体高度
                        Size textSize = TextRenderer.MeasureText("Hg", _canvasTextBox.Font);
                        int fontHeight = textSize.Height;
                        
                        // 计算垂直偏移，使光标与点击位置对齐
                        int yOffset = (fontHeight / 2) - 2;
                        _canvasTextBox.Location = new Point(e.X, e.Y - yOffset);
                    }
                    
                    // 确保输入框大小一致
                    _canvasTextBox.Width = 150;
                    _canvasTextBox.Height = 40;
                    
                    // 完全重置输入框状态
                    _canvasTextBox.Text = "";
                    _canvasTextBox.ForeColor = _toolStripManager.CurrentColor;
                    
                    // 显示输入框并获取焦点
                    _canvasTextBox.Visible = true;
                    _canvasTextBox.BringToFront();
                    
                    // 立即获取焦点，确保光标位置正确
                    _canvasTextBox.Focus();
                    _canvasTextBox.SelectionStart = 0;
                    _canvasTextBox.SelectionLength = 0;
                    
                    return;
                }
                
                InitializeDrawingTool(e.Location);
            }
            else
            {
                // 第一次点击时捕获屏幕
                if (_screenBitmap == null)
                {
                    CaptureScreen();
                }
            }
        }

        /// <summary>
        /// 初始化绘制工具
        /// </summary>
        /// <param name="startPoint">起始点</param>
        private void InitializeDrawingTool(Point startPoint)
        {
            if (_toolStripManager == null) return;

            switch (_toolStripManager.CurrentMode)
            {
                case DrawMode.Pen:
                    if (_currentPen != null)
                    {
                        _currentPenObject = new PenDrawObject(startPoint, _currentPen);
                    }
                    break;
                case DrawMode.Eraser:
                    _currentPenObject = new EraserDrawObject(startPoint, _toolStripManager.CurrentPenSize);
                    break;
            }
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void ScreenCaptureForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            _currentPoint = e.Location;
            
            // 处理绘制工具的点添加
            UpdateDrawingTool(e.Location);
            
            this.Invalidate();
        }

        /// <summary>
        /// 更新绘制工具
        /// </summary>
        /// <param name="currentPoint">当前点</param>
        private void UpdateDrawingTool(Point currentPoint)
        {
            if (_toolStripManager == null || _currentPenObject == null)
                return;

            // 对于画笔和橡皮擦工具，添加点到对应的DrawObject
            if (_toolStripManager.CurrentMode == DrawMode.Pen || 
                _toolStripManager.CurrentMode == DrawMode.Eraser)
            {
                _currentPenObject.AddPoint(currentPoint);
            }
            // 文本工具不需要更新点
        }

        /// <summary>
        /// 鼠标释放事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void ScreenCaptureForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            _currentPoint = e.Location;
            _isDragging = false;

            // 处理截图区域选择
            if (_toolStripManager == null || !_toolStripManager.ToolStripPanel.Visible)
            {
                HandleScreenshotSelection();
            }
            // 处理绘制操作
            else
            {
                HandleDrawingOperation();
            }

            this.Invalidate();
        }

        /// <summary>
        /// 处理截图区域选择
        /// </summary>
        private void HandleScreenshotSelection()
        {
            _captureRect = GetRectangle(_startPoint, _currentPoint);
            if (_captureRect.Width > 0 && _captureRect.Height > 0)
            {
                // 初始化工具条（延迟初始化）
                if (_toolStripManager == null)
                {
                    InitializeToolStrip();
                }
                
                // 初始化CanvasTextBox（延迟初始化）
                if (_canvasTextBox == null)
                {
                    _canvasTextBox = new TransparentTextBox
                    {
                        Visible = false,
                        BackColor = Color.Gray,
                        ForeColor = Color.Red,
                        Font = new Font("Arial", 12),
                        Multiline = true,
                        Width = 150,
                        Height = 40,
                        Padding = new Padding(0, 0, 0, 0)
                    };
                    _canvasTextBox.KeyDown += CanvasTextBox_KeyDown;
                    _canvasTextBox.LostFocus += CanvasTextBox_LostFocus;
                    this.Controls.Add(_canvasTextBox);
                }
                
                // 计算工具条的位置
                Screen screen = Screen.FromPoint(_captureRect.Location);
                int toolStripX, toolStripY;
                
                // 检查是否是全屏截图
                bool isFullScreen = _captureRect.Top == 0 && 
                                   _captureRect.Left == 0 && 
                                   _captureRect.Width == screen.Bounds.Width && 
                                   _captureRect.Height == screen.Bounds.Height;
                
                if (isFullScreen)
                {
                    // 全屏截图时，工具条显示在屏幕顶部中央
                    if (_toolStripManager != null)
                    {
                        toolStripX = (screen.WorkingArea.Width - _toolStripManager.ToolStripPanel.Width) / 2;
                        toolStripY = 10; // 顶部留出一些边距
                    }
                    else
                    {
                        toolStripX = 10;
                        toolStripY = 10;
                    }
                }
                else
                {
                    // 非全屏截图时，工具条显示在截图区域的下边
                    toolStripX = _captureRect.Left; // 与截图区域左对齐
                    toolStripY = _captureRect.Bottom; // 截图区域底部
                    
                    // 检查工具条是否会超出屏幕底部，如果是则显示在截图区域上方
                    if (_toolStripManager != null && toolStripY + _toolStripManager.ToolStripPanel.Height > screen.WorkingArea.Bottom)
                    {
                        toolStripY = _captureRect.Top - _toolStripManager.ToolStripPanel.Height; // 显示在截图区域上方
                    }
                    
                    // 确保工具条不会超出屏幕右侧
                    if (_toolStripManager != null && toolStripX + _toolStripManager.ToolStripPanel.Width > screen.WorkingArea.Right)
                    {
                        toolStripX = screen.WorkingArea.Right - _toolStripManager.ToolStripPanel.Width - 10;
                    }
                    
                    // 确保工具条不会超出屏幕左侧
                    if (toolStripX < 0)
                    {
                        toolStripX = 10;
                    }
                }
                
                _toolStripManager?.ShowToolStrip(new Point(toolStripX, toolStripY));
                
                // 确保工具条浮动在截图上方
                if (_toolStripManager != null)
                {
                    _toolStripManager.ToolStripPanel.BringToFront();
                    _toolStripManager.ToolStrip.BringToFront();
                }
            }
        }

        /// <summary>
        /// 处理绘制操作
        /// </summary>
        private void HandleDrawingOperation()
        {
            if (_toolStripManager == null || _drawObjects == null) return;

            // 处理画笔工具
            if (_toolStripManager.CurrentMode == DrawMode.Pen && _currentPenObject != null)
            {
                _drawObjects.Add(_currentPenObject);
                _currentPenObject = null;
            }
            // 处理橡皮擦工具
            else if (_toolStripManager.CurrentMode == DrawMode.Eraser && _currentPenObject is EraserDrawObject eraserObject)
            {
                eraserObject.Erase(_drawObjects);
                _currentPenObject = null;
            }
            // 处理其他工具
            else
            {
                var drawObject = CreateDrawObject(_startPoint, _currentPoint);
                if (drawObject != null)
                {
                    _drawObjects.Add(drawObject);
                }
            }
        }

        /// <summary>
        /// 绘制事件处理
        /// 负责绘制屏幕截图、半透明遮罩、绘制对象等
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">绘制事件参数</param>
        private void ScreenCaptureForm_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            // 绘制屏幕截图或默认背景
            if (_screenBitmap != null)
            {
                g.DrawImage(_screenBitmap, 0, 0);
            }
            else
            {
                // 绘制默认背景
                g.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);
            }

            // 处理工具条不可见的情况（截图区域选择模式）
            if (_toolStripManager == null || !_toolStripManager.ToolStripPanel.Visible)
            {
                using (var brush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                {
                    if (_isDragging && _screenBitmap != null)
                    {
                        // 绘制半透明遮罩，只显示选择区域
                        var rect = GetRectangle(_startPoint, _currentPoint);
                        g.FillRectangle(brush, 0, 0, this.Width, rect.Top);
                        g.FillRectangle(brush, 0, rect.Top, rect.Left, rect.Height);
                        g.FillRectangle(brush, rect.Right, rect.Top, this.Width - rect.Right, rect.Height);
                        g.FillRectangle(brush, 0, rect.Bottom, this.Width, this.Height - rect.Bottom);
                    }
                    else if (_screenBitmap != null)
                    {
                        // 绘制全屏半透明遮罩
                        g.FillRectangle(brush, 0, 0, this.Width, this.Height);
                    }
                }
            }
            // 处理工具条可见的情况（绘制模式）
            else
            {
                // 绘制半透明遮罩，只显示截图区域
                using (var brush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                {
                    g.FillRectangle(brush, 0, 0, this.Width, _captureRect.Top);
                    g.FillRectangle(brush, 0, _captureRect.Top, _captureRect.Left, _captureRect.Height);
                    g.FillRectangle(brush, _captureRect.Right, _captureRect.Top, this.Width - _captureRect.Right, _captureRect.Height);
                    g.FillRectangle(brush, 0, _captureRect.Bottom, this.Width, this.Height - _captureRect.Bottom);
                }

                // 绘制所有绘制对象
                if (_drawObjects != null)
                {
                    foreach (var drawObject in _drawObjects)
                    {
                        drawObject.Draw(g);
                    }
                }

                // 处理正在拖动的情况
                if (_isDragging)
                {
                    // 如果是画笔工具，绘制当前PenDrawObject
                    if (_toolStripManager != null && _toolStripManager.CurrentMode == DrawMode.Pen && _currentPenObject != null)
                    {
                        _currentPenObject.Draw(g);
                    }
                    // 如果是橡皮擦工具，绘制当前EraserDrawObject（留影效果）
                    else if (_toolStripManager != null && _toolStripManager.CurrentMode == DrawMode.Eraser && _currentPenObject != null)
                    {
                        _currentPenObject.Draw(g);
                    }
                    // 其他工具（除了文本工具）
                    else if (_toolStripManager != null && _toolStripManager.CurrentMode != DrawMode.Eraser && _toolStripManager.CurrentMode != DrawMode.Text)
                    {
                        // 创建并绘制临时绘制对象
                        var drawObject = CreateDrawObject(_startPoint, _currentPoint);
                        drawObject?.Draw(g);
                    }
                }
            }
        }

        private void ScreenCaptureForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 画布文本输入框按键事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">键盘事件参数</param>
        private void CanvasTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (_canvasTextBox != null && !string.IsNullOrEmpty(_canvasTextBox.Text) && _drawObjects != null && _textList != null)
                {
                    // 创建文本绘制对象并添加到绘制对象列表
                    var textObject = new TextDrawObject(
                        new Point(_canvasTextBox.Left, _canvasTextBox.Top),
                        _canvasTextBox.Text,
                        _canvasTextBox.ForeColor
                    );
                    _drawObjects.Add(textObject);
                    _textList.Add(textObject);
                    _canvasTextBox.Visible = false;
                    this.Invalidate();
                }
                else if (_canvasTextBox != null)
                {
                    // 输入为空，仅隐藏输入框
                    _canvasTextBox.Visible = false;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // 按ESC键取消输入并关闭截图
                if (_canvasTextBox != null)
                {
                    _canvasTextBox.Visible = false;
                }
                this.Close();
            }
        }

        /// <summary>
        /// 画布文本输入框失去焦点事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void CanvasTextBox_LostFocus(object? sender, EventArgs e)
        {
            if (_canvasTextBox != null && !string.IsNullOrEmpty(_canvasTextBox.Text) && _drawObjects != null && _textList != null)
            {
                // 计算文本放置位置，确保文本在点击位置的下一个像素
                using (Graphics g = this.CreateGraphics())
                {
                    // 测量字体高度
                    Size textSize = TextRenderer.MeasureText("Hg", _canvasTextBox.Font);
                    int fontHeight = textSize.Height;
                    
                    // 计算垂直偏移，使文本基线与点击位置对齐
                    int yOffset = (fontHeight / 2) - 2;
                    
                    // 创建文本绘制对象并添加到绘制对象列表
                    var textObject = new TextDrawObject(
                        new Point(_canvasTextBox.Left, _canvasTextBox.Top + yOffset),
                        _canvasTextBox.Text,
                        _canvasTextBox.ForeColor
                    );
                    _drawObjects.Add(textObject);
                    _textList.Add(textObject);
                }
                _canvasTextBox.Visible = false;
                this.Invalidate();
            }
            else if (_canvasTextBox != null)
            {
                // 输入为空，仅隐藏输入框
                _canvasTextBox.Visible = false;
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y)
            );
        }

        /// <summary>
        /// 创建绘制对象
        /// 根据当前选择的工具模式创建对应的绘制对象
        /// </summary>
        /// <param name="start">开始点</param>
        /// <param name="end">结束点</param>
        /// <returns>绘制对象，如果创建失败则返回null</returns>
        private DrawObject? CreateDrawObject(Point start, Point end)
        {
            if (_currentPen == null || _toolStripManager == null) return null;
            
            switch (_toolStripManager.CurrentMode)
            {
                case DrawMode.Pen:
                    // 创建画笔绘制对象
                    return new PenDrawObject(start, _currentPen);
                case DrawMode.Rectangle:
                    // 创建矩形绘制对象
                    return new RectangleDrawObject(GetRectangle(start, end), _currentPen);
                case DrawMode.Circle:
                    // 创建圆形绘制对象
                    var rect = GetRectangle(start, end);
                    return new CircleDrawObject(rect, _currentPen);
                case DrawMode.Arrow:
                    // 创建箭头绘制对象
                    return new ArrowDrawObject(start, end, _currentPen, _toolStripManager.CurrentArrowStyle);
                case DrawMode.Blur:
                    // 创建模糊绘制对象
                    var blurRect = GetRectangle(start, end);
                    return new BlurDrawObject(blurRect, _screenBitmap);
                case DrawMode.Text:
                    // 文本工具通过MouseDown事件处理，不需要通过CreateDrawObject创建
                    return null;
                case DrawMode.Eraser:
                    // 橡皮擦工具通过MouseDown/MouseMove/MouseUp事件处理，不需要通过CreateDrawObject创建
                    return null;
                default:
                    return null;
            }
        }

        private Bitmap? GetCaptureImage()
        {
            try
            {
                if (_captureRect.Width <= 0 || _captureRect.Height <= 0 || _screenBitmap == null) return null;

                var bitmap = new Bitmap(_captureRect.Width, _captureRect.Height);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(_screenBitmap, 0, 0, _captureRect, GraphicsUnit.Pixel);
                    if (_drawObjects != null)
                    {
                        foreach (var drawObject in _drawObjects)
                        {
                            try
                            {
                                drawObject.Draw(g, _captureRect.Location);
                            }
                            catch (Exception ex)
                            {
                                Capto.Utilities.Logger.Error($"Error drawing object: {ex.Message}", ex);
                                // 继续绘制其他对象
                            }
                        }
                    }
                }
                
                // 添加水印
                WatermarkHelper.AddWatermark(bitmap);
                
                return bitmap;
            }
            catch (Exception ex)
            {
                Capto.Utilities.Logger.Error($"Error creating capture image: {ex.Message}", ex);
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 取消事件订阅
                if (_toolStripManager != null)
                {
                    _toolStripManager.ToolSelected -= ToolStripManager_ToolSelected;
                    _toolStripManager.CopyClicked -= ToolStripManager_CopyClicked;
                    _toolStripManager.SaveClicked -= ToolStripManager_SaveClicked;
                    _toolStripManager.PenSizeChanged -= ToolStripManager_PenSizeChanged;
                    _toolStripManager.ColorChanged -= ToolStripManager_ColorChanged;
                    _toolStripManager.Dispose();
                }
                
                _screenBitmap?.Dispose();
                _currentPen?.Dispose();
                _canvasTextBox?.Dispose();
                if (_drawObjects != null)
                {
                    foreach (var drawObject in _drawObjects)
                    {
                        drawObject.Dispose();
                    }
                    _drawObjects.Clear();
                }
                _textList?.Clear();
            }
            base.Dispose(disposing);
        }

        // 工具条拖动事件处理

    }
}