using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Capto.Utilities;

namespace Capto
{
    /// <summary>
    /// 颜色项类
    /// </summary>
    public class ColorItem
    {
        public string Name { get; set; }
        public Color Color { get; set; }
    }

    
    /// <summary>
    /// 工具条管理器
    /// 负责管理工具条的创建、事件处理等
    /// </summary>
    public class ToolStripManager
    {
        /// <summary>工具条容器 Panel</summary>
        public Panel ToolStripPanel { get; private set; }
        
        /// <summary>工具条控件</summary>
        public ToolStrip ToolStrip { get; private set; }
        
        /// <summary>画笔工具按钮</summary>
        public ToolStripButton PenButton { get; private set; }
        
        /// <summary>矩形工具按钮</summary>
        public ToolStripButton RectangleButton { get; private set; }
        
        /// <summary>圆形工具按钮</summary>
        public ToolStripButton CircleButton { get; private set; }
        
        /// <summary>箭头工具按钮</summary>
        public ToolStripButton ArrowButton { get; private set; }
        
        /// <summary>模糊工具按钮</summary>
        public ToolStripButton BlurButton { get; private set; }
        
        /// <summary>文本工具按钮</summary>
        public ToolStripButton TextButton { get; private set; }
        
        /// <summary>擦除工具按钮</summary>
        public ToolStripButton EraserButton { get; private set; }
        
        /// <summary>画笔粗细下拉框</summary>
        public ToolStripDropDownButton PenSizeComboBox { get; private set; }
        
        /// <summary>箭头样式下拉框</summary>
        public ToolStripDropDownButton ArrowStyleComboBox { get; private set; }
        
        /// <summary>颜色下拉框</summary>
        public ToolStripDropDownButton ColorComboBox { get; private set; }
        
        /// <summary>复制按钮</summary>
        public ToolStripButton CopyButton { get; private set; }
        
        /// <summary>保存按钮</summary>
        public ToolStripButton SaveButton { get; private set; }
        
        /// <summary>当前绘制模式</summary>
        public DrawMode CurrentMode { get; set; }
        
        /// <summary>当前颜色</summary>
        private Color _currentColor;
        public Color CurrentColor 
        {
            get { return _currentColor; }
            set 
            {
                if (_currentColor != value)
                {
                    _currentColor = value;
                    ColorChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        /// <summary>颜色变化事件</summary>
        public event EventHandler ColorChanged;
        
        /// <summary>当前画笔大小</summary>
        private int _currentPenSize;
        public int CurrentPenSize 
        {
            get { return _currentPenSize; }
            set 
            {
                if (_currentPenSize != value)
                {
                    _currentPenSize = value;
                    PenSizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        /// <summary>当前箭头样式</summary>
        private DrawObjects.ArrowStyle _currentArrowStyle;
        public DrawObjects.ArrowStyle CurrentArrowStyle
        {
            get { return _currentArrowStyle; }
            set
            {
                if (_currentArrowStyle != value)
                {
                    _currentArrowStyle = value;
                    ArrowStyleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        /// <summary>画笔大小变化事件</summary>
        public event EventHandler PenSizeChanged;
        
        /// <summary>箭头样式变化事件</summary>
        public event EventHandler ArrowStyleChanged;
        
        /// <summary>工具条拖动事件</summary>
        public event EventHandler<ToolStripDragEventArgs> ToolStripDrag;
        
        /// <summary>工具选择事件</summary>
        public event EventHandler<ToolSelectedEventArgs> ToolSelected;
        
        /// <summary>复制按钮点击事件</summary>
        public event EventHandler CopyClicked;
        
        /// <summary>保存按钮点击事件</summary>
        public event EventHandler SaveClicked;
        
        private bool _isToolStripDragging = false;
        private Point _toolStripDragStartPoint;
        private readonly Form _parentForm;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parentForm">父表单</param>
        public ToolStripManager(Form parentForm)
        {
            _parentForm = parentForm;
            CurrentMode = DrawMode.Pen;
            CurrentColor = Color.Red;
            CurrentPenSize = 3;
            CurrentArrowStyle = DrawObjects.ArrowStyle.Normal;
            InitializeToolStrip();
        }
        
        /// <summary>
        /// 初始化工具条
        /// </summary>
        private void InitializeToolStrip()
        {
            ToolStrip = new ToolStrip
            {
                Visible = false,
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.White,
                GripStyle = ToolStripGripStyle.Hidden,
                Padding = new Padding(10, 5, 10, 5)
            };
            ToolStrip.AutoSize = true;
            ToolStrip.ImageScalingSize = new Size(48, 48);
            // 创建工具按钮
            PenButton = new ToolStripButton() { Checked = true, BackColor = Color.Transparent, Image = IconLoader.LoadIcon("pencil", Color.Black) };
            RectangleButton = new ToolStripButton() { BackColor = Color.Transparent, Image = IconLoader.LoadIcon("square", Color.Black) };
            CircleButton = new ToolStripButton() { BackColor = Color.Transparent, Image = IconLoader.LoadIcon("circle", Color.Black) };
            ArrowButton = new ToolStripButton() { BackColor = Color.Transparent, Image = IconLoader.LoadIcon("arrow-right", Color.Black) };
            BlurButton = new ToolStripButton() { Image = IconLoader.LoadIcon("grid", Color.Black) };
            TextButton = new ToolStripButton() { BackColor = Color.Transparent,Image = IconLoader.LoadIcon("A", Color.Black) };
            EraserButton = new ToolStripButton() { BackColor = Color.Transparent, Image = IconLoader.LoadIcon("eraser", Color.Black) };
            PenSizeComboBox = new ToolStripDropDownButton { Width = 80, BackColor = Color.FromArgb(220, 220, 220), ForeColor = Color.Black };
            ArrowStyleComboBox = new ToolStripDropDownButton { Width = 80, BackColor = Color.FromArgb(220, 220, 220), ForeColor = Color.Black };
            ColorComboBox = new ToolStripDropDownButton { Width = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            SaveButton = new ToolStripButton() {  Image = IconLoader.LoadIcon("download", Color.Black) };
            CopyButton = new ToolStripButton("完成") {  Image = IconLoader.LoadIcon("check", Color.Black),ForeColor = Color.Black };
            // 初始化画笔粗细下拉框
            InitializePenSizeComboBox();
            
            // 初始化箭头样式下拉框
            InitializeArrowStyleComboBox();
            
            // 初始化颜色下拉框
            InitializeColorComboBox();
            
            // 辅助方法：获取颜色名称
            string GetColorName(Color color)
            {
                var colorNames = typeof(Color).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Where(p => p.PropertyType == typeof(Color))
                    .Select(p => new { Name = p.Name, Color = (Color)p.GetValue(null) })
                    .GroupBy(c => c.Color.ToArgb())
                    .ToDictionary(g => g.Key, g => g.First().Name);

                if (colorNames.TryGetValue(color.ToArgb(), out var name))
                {
                    // 转换为中文名称
                    var nameMap = new Dictionary<string, string>
                    {
                        {"Black", "黑色"},
                        {"DarkGray", "深灰"},
                        {"Gray", "灰色"},
                        {"LightGray", "浅灰"},
                        {"White", "白色"},
                        {"DarkRed", "深红"},
                        {"Red", "红色"},
                        {"Orange", "橙色"},
                        {"Yellow", "黄色"},
                        {"Lime", "青绿"},
                        {"Green", "绿色"},
                        {"Cyan", "青色"},
                        {"Blue", "蓝色"},
                        {"DarkBlue", "深蓝"},
                        {"Purple", "紫色"},
                        {"Pink", "粉色"},
                        {"Brown", "棕色"},
                        {"Maroon", "栗色"},
                        {"Olive", "橄榄"},
                        {"Teal", "蓝绿"}
                    };
                    return nameMap.TryGetValue(name, out var chineseName) ? chineseName : name;
                }
                return "自定义";
            }
            
            // 添加按钮到工具条
            ToolStrip.Items.AddRange(new ToolStripItem[]
            {
                PenButton, RectangleButton, CircleButton, ArrowButton, BlurButton, TextButton, EraserButton,
                new ToolStripSeparator(),
                new ToolStripLabel("粗细:") { ForeColor = Color.Black },
                PenSizeComboBox,
                new ToolStripLabel("箭头:") { ForeColor = Color.Black },
                ArrowStyleComboBox,
                new ToolStripLabel("颜色:") { ForeColor = Color.Black },
                ColorComboBox,
                new ToolStripSeparator(),
                 SaveButton,CopyButton
            });
            
            // 添加事件处理
            PenButton.Click += ToolButton_Click;
            RectangleButton.Click += ToolButton_Click;
            CircleButton.Click += ToolButton_Click;
            ArrowButton.Click += ToolButton_Click;
            BlurButton.Click += ToolButton_Click;
            TextButton.Click += ToolButton_Click;
            EraserButton.Click += ToolButton_Click;
            CopyButton.Click += CopyButton_Click;
            SaveButton.Click += SaveButton_Click;
            
            // 创建 Panel 容器用于拖动
            ToolStripPanel = new Panel
            {
                Visible = false,
                BackColor = Color.Transparent,
                AutoSize = false,
                Width = 1000
            };
            ToolStrip.Location = new Point(0, 0);
            ToolStripPanel.Controls.Add(ToolStrip);
            
            // 在 Panel 上添加工具条拖动事件
            ToolStripPanel.MouseDown += ToolStrip_MouseDown;
            ToolStripPanel.MouseMove += ToolStrip_MouseMove;
            ToolStripPanel.MouseUp += ToolStrip_MouseUp;
            
            // 添加到父表单
            _parentForm.Controls.Add(ToolStripPanel);
        }

        /// <summary>
        /// 初始化颜色下拉框
        /// </summary>
        private void InitializeColorComboBox()
        {
            ColorComboBox = new ToolStripDropDownButton { Text = "红色" };

            // 1. 定义颜色数据源（参照图片中的颜色网格）
            var colors = new List<Color>
            {
                Color.Black, Color.DarkGray, Color.Gray, Color.LightGray, Color.White,
                Color.DarkRed, Color.Red, Color.Orange, Color.Yellow, Color.Lime,
                Color.Green, Color.Cyan, Color.Blue, Color.DarkBlue, Color.Purple,
                Color.Pink, Color.Brown, Color.Maroon, Color.Olive, Color.Teal
            };

            // 2. 创建颜色选择器面板
            var colorPanel = new TableLayoutPanel
            {
                ColumnCount = 5,
                RowCount = 4,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Padding = new Padding(5),
                AutoSize = true
            };

            // 3. 添加颜色按钮
            foreach (var color in colors)
            {
                var colorButton = new Button
                {
                    BackColor = color,
                    Width = 48,
                    Height = 48,
                    Margin = new Padding(2),
                    Tag = color
                };
                colorButton.Click += (sender, e) =>
                {
                    var btn = sender as Button;
                    if (btn != null)
                    {
                        var selectedColor = (Color)btn.Tag;
                        CurrentColor = selectedColor;
                        // 创建颜色块图像并设置为ColorComboBox的图像
                        ColorComboBox.Image = CreateColorBlock(selectedColor);
                        ColorComboBox.Text = "";
                        ColorComboBox.DropDown.Close();
                    }
                };
                colorPanel.Controls.Add(colorButton);
            }

            // 4. 添加"其它颜色..."按钮
            var customColorButton = new Button
            {
                Text = "其它颜色...",
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Height = 48
            };
            customColorButton.Click += (sender, e) =>
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = CurrentColor;
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        CurrentColor = colorDialog.Color;
                        // 创建颜色块图像并设置为ColorComboBox的图像
                        ColorComboBox.Image = CreateColorBlock(colorDialog.Color);
                        ColorComboBox.Text = "";
                    }
                }
                ColorComboBox.DropDown.Close();
            };
            colorPanel.Controls.Add(customColorButton);
            colorPanel.SetColumnSpan(customColorButton, 5);

            // 5. 创建下拉菜单并添加颜色面板
            var dropDown = new ToolStripDropDown();
            var host = new ToolStripControlHost(colorPanel)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            dropDown.Items.Add(host);
            ColorComboBox.DropDown = dropDown;

            // 6. 设置默认值
            CurrentColor = Color.Red;
            // 创建默认颜色块图像并设置为ColorComboBox的图像
            ColorComboBox.Image = CreateColorBlock(Color.Red);
            ColorComboBox.Text = "";
        }

        /// <summary>
        /// 创建颜色块图像
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>颜色块图像</returns>
        private Bitmap CreateColorBlock(Color color)
        {
            var colorBlock = new Bitmap(24, 16);
            using (Graphics g = Graphics.FromImage(colorBlock))
            {
                using (var brush = new SolidBrush(color))
                using (var pen = new Pen(Color.LightGray, 1))
                {
                    g.FillRectangle(brush, 0, 0, 24, 16);
                    g.DrawRectangle(pen, 0, 0, 23, 15);
                }
            }
            return colorBlock;
        }
        

        
        /// <summary>
        /// 工具按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ToolButton_Click(object sender, EventArgs e)
        {
            // 取消所有按钮的选中状态
            PenButton.Checked = false;
            RectangleButton.Checked = false;
            CircleButton.Checked = false;
            ArrowButton.Checked = false;
            BlurButton.Checked = false;
            TextButton.Checked = false;
            EraserButton.Checked = false;
            
            // 设置当前按钮为选中状态
            var button = sender as ToolStripButton;
            if (button != null)
            {
                button.Checked = true;
            }
            
            // 设置当前模式
            if (sender == PenButton)
            {
                CurrentMode = DrawMode.Pen;
            }
            else if (sender == RectangleButton)
            {
                CurrentMode = DrawMode.Rectangle;
            }
            else if (sender == CircleButton)
            {
                CurrentMode = DrawMode.Circle;
            }
            else if (sender == ArrowButton)
            {
                CurrentMode = DrawMode.Arrow;
            }
            else if (sender == BlurButton)
            {
                CurrentMode = DrawMode.Blur;
            }
            else if (sender == TextButton)
            {
                CurrentMode = DrawMode.Text;
            }
            else if (sender == EraserButton)
            {
                CurrentMode = DrawMode.Eraser;
            }
            
            // 触发工具选择事件
            ToolSelected?.Invoke(this, new ToolSelectedEventArgs(CurrentMode));
        }
        
        /// <summary>
        /// 创建线条粗细图标
        /// </summary>
        /// <param name="size">线条粗细</param>
        /// <returns>线条粗细图标</returns>
        private Bitmap CreatePenSizeIcon(int size)
        {
            int width = 48;
            int height = 24;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                int centerY = height / 2;
                Point start = new Point(5, centerY);
                Point end = new Point(width - 5, centerY);
                
                using (Pen pen = new Pen(Color.Black, size))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, start, end);
                }
            }
            return bitmap;
        }
        
        /// <summary>
        /// 初始化画笔粗细下拉框
        /// </summary>
        private void InitializePenSizeComboBox()
        {
            // 创建画笔粗细下拉菜单
            var penSizeDropDown = new ToolStripDropDown();
            
            // 添加1-15的粗细选项
            for (int i = 1; i < 16; i++)
            {
                int size = i;
                var sizeIcon = CreatePenSizeIcon(size);
                var sizeItem = new ToolStripMenuItem(size.ToString(), sizeIcon, (sender, e) =>
                {
                    CurrentPenSize = size;
                    PenSizeComboBox.Image = sizeIcon;
                    PenSizeComboBox.Text = "";
                    PenSizeComboBox.DropDown.Close();
                });
                penSizeDropDown.Items.Add(sizeItem);
            }
            
            // 设置下拉菜单
            PenSizeComboBox.DropDown = penSizeDropDown;
            
            // 设置默认值（粗细为3）
            CurrentPenSize = 3;
            var defaultIcon = CreatePenSizeIcon(3);
            PenSizeComboBox.Image = defaultIcon;
            PenSizeComboBox.Text = "";
        }
        
        /// <summary>
        /// 创建箭头样式图标
        /// </summary>
        /// <param name="style">箭头样式</param>
        /// <returns>箭头图标</returns>
        private Bitmap CreateArrowStyleIcon(DrawObjects.ArrowStyle style)
        {
            int width = 48;
            int height = 24;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                Point start = new Point(5, height / 2);
                Point end = new Point(width - 5, height / 2);
                
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    
                    // 绘制主线
                    g.DrawLine(pen, start, end);
                    
                    // 计算箭头
                    double angle = Math.PI / 6;
                    int arrowLength = 8;
                    
                    double dx = end.X - start.X;
                    double dy = end.Y - start.Y;
                    double lineAngle = Math.Atan2(dy, dx);
                    
                    Point arrowPoint1 = new Point(
                        end.X - (int)(arrowLength * Math.Cos(lineAngle - angle)),
                        end.Y - (int)(arrowLength * Math.Sin(lineAngle - angle)));
                    Point arrowPoint2 = new Point(
                        end.X - (int)(arrowLength * Math.Cos(lineAngle + angle)),
                        end.Y - (int)(arrowLength * Math.Sin(lineAngle + angle)));
                    
                    switch (style)
                    {
                        case DrawObjects.ArrowStyle.Normal:
                            // 普通箭头
                            g.DrawLine(pen, end, arrowPoint1);
                            g.DrawLine(pen, end, arrowPoint2);
                            break;
                            
                        case DrawObjects.ArrowStyle.Solid:
                            // 实心箭头
                            using (Brush brush = new SolidBrush(Color.Black))
                            {
                                Point[] points = { end, arrowPoint1, arrowPoint2 };
                                g.FillPolygon(brush, points);
                            }
                            break;
                            
                        case DrawObjects.ArrowStyle.DoubleLine:
                            // 双线箭头
                            float offset = 2f;
                            Point start1 = new Point(start.X, start.Y - (int)offset);
                            Point end1 = new Point(end.X, end.Y - (int)offset);
                            Point start2 = new Point(start.X, start.Y + (int)offset);
                            Point end2 = new Point(end.X, end.Y + (int)offset);
                            
                            g.DrawLine(pen, start1, end1);
                            g.DrawLine(pen, start2, end2);
                            g.DrawLine(pen, end1, arrowPoint1);
                            g.DrawLine(pen, end2, arrowPoint2);
                            break;
                    }
                }
            }
            return bitmap;
        }
        
        /// <summary>
        /// 初始化箭头样式下拉框
        /// </summary>
        private void InitializeArrowStyleComboBox()
        {
            // 创建箭头样式下拉菜单
            var arrowStyleDropDown = new ToolStripDropDown();
            
            // 添加普通箭头选项
            var normalArrowIcon = CreateArrowStyleIcon(DrawObjects.ArrowStyle.Normal);
            var normalArrowItem = new ToolStripMenuItem("普通", normalArrowIcon, (sender, e) =>
            {
                CurrentArrowStyle = DrawObjects.ArrowStyle.Normal;
                ArrowStyleComboBox.Image = normalArrowIcon;
                ArrowStyleComboBox.Text = "";
                ArrowStyleComboBox.DropDown.Close();
            });
            arrowStyleDropDown.Items.Add(normalArrowItem);
            
            // 添加实心箭头选项
            var solidArrowIcon = CreateArrowStyleIcon(DrawObjects.ArrowStyle.Solid);
            var solidArrowItem = new ToolStripMenuItem("实心", solidArrowIcon, (sender, e) =>
            {
                CurrentArrowStyle = DrawObjects.ArrowStyle.Solid;
                ArrowStyleComboBox.Image = solidArrowIcon;
                ArrowStyleComboBox.Text = "";
                ArrowStyleComboBox.DropDown.Close();
            });
            arrowStyleDropDown.Items.Add(solidArrowItem);
            
            // 设置下拉菜单
            ArrowStyleComboBox.DropDown = arrowStyleDropDown;
            
            // 设置默认值
            CurrentArrowStyle = DrawObjects.ArrowStyle.Normal;
            ArrowStyleComboBox.Image = normalArrowIcon;
            ArrowStyleComboBox.Text = "";
        }
        
        /// <summary>
        /// 箭头样式下拉框选择事件处理（已弃用，保留以防引用）
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ArrowStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 此方法已弃用，不再使用
        }

        /// <summary>
        /// 复制按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void CopyButton_Click(object sender, EventArgs e)
        {
            CopyClicked?.Invoke(this, e);
        }
        
        /// <summary>
        /// 保存按钮点击事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveClicked?.Invoke(this, e);
        }
        
        /// <summary>
        /// 工具条鼠标按下事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void ToolStrip_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isToolStripDragging = true;
                _toolStripDragStartPoint = new Point(e.X, e.Y);
            }
        }
        
        /// <summary>
        /// 工具条鼠标移动事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void ToolStrip_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isToolStripDragging)
            {
                Point newLocation = ToolStripPanel.Location;
                newLocation.X += e.X - _toolStripDragStartPoint.X;
                newLocation.Y += e.Y - _toolStripDragStartPoint.Y;
                
                // 确保工具条不会被拖出屏幕
                Screen screen = Screen.FromControl(ToolStripPanel);
                newLocation.X = Math.Max(0, Math.Min(newLocation.X, screen.WorkingArea.Width - ToolStripPanel.Width));
                newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, screen.WorkingArea.Height - ToolStripPanel.Height));
                
                ToolStripPanel.Location = newLocation;
                
                // 触发工具条拖动事件
                ToolStripDrag?.Invoke(this, new ToolStripDragEventArgs(newLocation));
            }
        }
        
        /// <summary>
        /// 工具条鼠标释放事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void ToolStrip_MouseUp(object sender, MouseEventArgs e)
        {
            _isToolStripDragging = false;
        }
        
        /// <summary>
        /// 显示工具条
        /// </summary>
        /// <param name="position">工具条位置</param>
        public void ShowToolStrip(Point position)
        {
            ToolStrip.Visible = true;
            ToolStripPanel.Location = position;
            ToolStripPanel.Visible = true;
        }
        
        /// <summary>
        /// 隐藏工具条
        /// </summary>
        public void HideToolStrip()
        {
            ToolStrip.Visible = false;
            ToolStripPanel.Visible = false;
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 取消事件订阅
            if (PenButton != null) PenButton.Click -= ToolButton_Click;
            if (RectangleButton != null) RectangleButton.Click -= ToolButton_Click;
            if (CircleButton != null) CircleButton.Click -= ToolButton_Click;
            if (ArrowButton != null) ArrowButton.Click -= ToolButton_Click;
            if (BlurButton != null) BlurButton.Click -= ToolButton_Click;
            if (TextButton != null) TextButton.Click -= ToolButton_Click;
            if (EraserButton != null) EraserButton.Click -= ToolButton_Click;
            if (CopyButton != null) CopyButton.Click -= CopyButton_Click;
            if (SaveButton != null) SaveButton.Click -= SaveButton_Click;
            if (ToolStripPanel != null)
            {
                ToolStripPanel.MouseDown -= ToolStrip_MouseDown;
                ToolStripPanel.MouseMove -= ToolStrip_MouseMove;
                ToolStripPanel.MouseUp -= ToolStrip_MouseUp;
            }
            
            // 释放ToolStrip和ToolStripPanel
            if (ToolStrip != null)
            {
                // 释放所有ToolStripItem中的Image资源
                foreach (ToolStripItem item in ToolStrip.Items)
                {
                    if (item.Image != null)
                    {
                        item.Image.Dispose();
                    }
                    // 检查是否为下拉按钮，释放下拉菜单中的资源
                    if (item is ToolStripDropDownButton dropDownButton && dropDownButton.DropDown != null)
                    {
                        foreach (ToolStripItem dropDownItem in dropDownButton.DropDown.Items)
                        {
                            if (dropDownItem.Image != null)
                            {
                                dropDownItem.Image.Dispose();
                            }
                        }
                        dropDownButton.DropDown.Dispose();
                    }
                }
                ToolStrip.Dispose();
            }
            
            if (ToolStripPanel != null)
            {
                // 移除所有子控件
                while (ToolStripPanel.Controls.Count > 0)
                {
                    var control = ToolStripPanel.Controls[0];
                    ToolStripPanel.Controls.Remove(control);
                    control.Dispose();
                }
                ToolStripPanel.Dispose();
            }
            
            // 清空事件委托
            ColorChanged = null;
            PenSizeChanged = null;
            ArrowStyleChanged = null;
            ToolStripDrag = null;
            ToolSelected = null;
            CopyClicked = null;
            SaveClicked = null;
        }
    }
    
    /// <summary>
    /// 工具条拖动事件参数
    /// </summary>
    public class ToolStripDragEventArgs : EventArgs
    {
        /// <summary>工具条新位置</summary>
        public Point NewLocation { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="newLocation">工具条新位置</param>
        public ToolStripDragEventArgs(Point newLocation)
        {
            NewLocation = newLocation;
        }
    }
    
    /// <summary>
    /// 工具选择事件参数
    /// </summary>
    public class ToolSelectedEventArgs : EventArgs
    {
        /// <summary>选中的工具模式</summary>
        public DrawMode SelectedTool { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="selectedTool">选中的工具模式</param>
        public ToolSelectedEventArgs(DrawMode selectedTool)
        {
            SelectedTool = selectedTool;
        }
    }
}