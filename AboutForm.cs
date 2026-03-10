using System;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using Capto.Utilities;
using QRCoder;

namespace Capto
{
    public class AboutForm : Form
    {
        private readonly string _wechatId = "https://u.wechat.com/EFHKl209RbFOzFt5QDYXsmQ?s=4";
        private static AboutForm? _instance;
        private static readonly object _lock = new object();
        
        public static void ShowAbout(IWin32Window? owner = null)
        {
            lock (_lock)
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new AboutForm();
                    _instance.Show(owner);
                }
                else
                {
                    _instance.Activate();
                    if (_instance.WindowState == FormWindowState.Minimized)
                    {
                        _instance.WindowState = FormWindowState.Normal;
                    }
                }
            }
        }
        
        public AboutForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "关于 Capto";
            this.Size = new Size(840, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.TopMost = true;
            
            // 设置窗口图标
            this.Icon = Capto.Utilities.IconUtils.GetAppIcon();
            
            var titleLabel = new Label
            {
                Text = "Capto",
                Font = new Font("Microsoft YaHei UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(20, 20),
                AutoSize = true
            };
            
           var descLabel = new Label
            {
                Text = "一款简洁高效的截图工具",
                Font = new Font("Microsoft YaHei UI", 10),
                ForeColor = Color.FromArgb(96, 96, 96),
                Location = new Point(260, 55),
                AutoSize = true
            };
            
            var licenseLabel = new Label
            {
                Text = GetLicenseInfo(),
                Font = new Font("Microsoft YaHei UI", 10),
                ForeColor = Color.FromArgb(96, 96, 96),
                Location = new Point(20, 400),
                AutoSize = true
            };
           
            var systemInfoTextBox = new RichTextBox
            {
                Text = SystemInfoHelper.GetDetailedSystemInfo(),
                Font = new Font("Microsoft YaHei UI", 9),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(50, 120),
                Size = new Size(450, 300),
            
                ReadOnly = true,
                BorderStyle = BorderStyle.Fixed3D,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            
            
            
            var separator1 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(20, 90),
                Size = new Size(790, 2)
            };
            
            var qrPicture = new PictureBox
            {
                Location = new Point(550, 120),
                Size = new Size(200, 200),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle
            };
            var qrContent = GenerateQRContent();
            qrPicture.Image = GenerateQRCode(qrContent);
            
            // 添加二维码下方文字
            var qrTextLabel = new Label
            {
                Text = "扫码加微信",
                Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 255, 136, 0), // 橙色
                Location = new Point(550, 330),
                AutoSize = true
            };
            
            var copyButton = new Button
            {
                Text = "复制",
                Location = new Point(500, 450),
                Size = new Size(100, 38),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(64, 64, 64),
                Font = new Font("Microsoft YaHei UI", 9)
            };
            var closeButton = new Button
            {
                Text = "确定",
                Location = new Point(640, 450),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(64, 64, 64),
                Font = new Font("Microsoft YaHei UI", 9)
            };
            
            this.Controls.Add(titleLabel);
            this.Controls.Add(systemInfoTextBox);
            this.Controls.Add(descLabel);
            this.Controls.Add(licenseLabel);
            this.Controls.Add(separator1);
            this.Controls.Add(qrPicture);
            this.Controls.Add(qrTextLabel);
            this.Controls.Add(copyButton);
            this.Controls.Add(closeButton);
            
            this.AcceptButton = closeButton;
            closeButton.Click += CloseButton_Click;
            copyButton.Click += CopyButton_Click;
        }
        
        private void CloseButton_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
        
        private void CopyButton_Click(object? sender, EventArgs e)
        {
            try
            {
                string systemInfo = SystemInfoHelper.GetDetailedSystemInfo();
                Clipboard.SetText(systemInfo);
                // 显示复制成功提示
                var originalText = ((Button)sender).Text;
                ((Button)sender).Text = "已复制";
                // 2秒后恢复按钮文本
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(t =>
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke(new Action(() =>
                        {
                            ((Button)sender).Text = originalText;
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误");
            }
        }
        
        private string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            return "1.0.0";
        }
        
        private string GenerateQRContent()
        {
            return $"{_wechatId}";
        }
        
        private Bitmap GenerateQRCode(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new QRCode(qrCodeData);
            
            // 生成橙色二维码，带有微信图标
            var colorDark = Color.FromArgb(255, 255, 136, 0); // 橙色
            var colorLight = Color.White;
            
            using var qrBitmap = qrCode.GetGraphic(6, colorDark, colorLight, GetWechatIcon());
            
            return new Bitmap(qrBitmap);
        }
        
        private Bitmap GetWechatIcon()
        {
            // 创建一个简单的微信图标（两个对话气泡）
            var iconSize = 60;
            var icon = new Bitmap(iconSize, iconSize);
            using var graphics = Graphics.FromImage(icon);
            
            var color = Color.FromArgb(255, 255, 136, 0); // 橙色
            var brush = new SolidBrush(color);
            
            // 绘制背景方块
            graphics.FillRectangle(brush, 0, 0, iconSize, iconSize);
            
            // 绘制白色对话气泡
            var bubbleBrush = new SolidBrush(Color.White);
            
            // 第一个气泡
            graphics.FillEllipse(bubbleBrush, 10, 15, 20, 20);
            
            // 第二个气泡
            graphics.FillEllipse(bubbleBrush, 30, 10, 20, 20);
            
            return icon;
        }
        
        private string GetLicenseInfo()
        {
            try
            {
                var licensePath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "license.lic");
                if (System.IO.File.Exists(licensePath))
                {
                    var licenseKey = System.IO.File.ReadAllText(licensePath);
                    var licenseInfo = Capto.Utilities.LicenseManager.GetLicenseInfo(licenseKey);
                    if (licenseInfo != null)
                    {
                        return $"许可证到期日期: {licenseInfo.ExpirationDate.ToString("yyyy-MM-dd")}";
                    }
                }
                return "未找到有效许可证";
            }
            catch
            {
                return "许可证信息获取失败";
            }
        }
    }
}
