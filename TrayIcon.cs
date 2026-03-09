using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Capto
{
    public class TrayIcon : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private Keys _hotkey = Keys.Alt | Keys.S;
        private const int HOTKEY_ID = 1;
        private const int WM_HOTKEY = 0x0312;
        private HotkeyForm? _hotkeyForm;
        private bool _isCaptureFormOpen = false;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public TrayIcon()
        {
            try
            {
                // 尝试使用favicon.ico作为图标
                _trayIcon = new NotifyIcon
                {
                    Icon = LoadFavicon(),
                    Text = "Capto",
                    Visible = true
                };

                var contextMenu = new ContextMenuStrip();
                var captureItem = new ToolStripMenuItem("截图", null, CaptureScreen);
                var settingsItem = new ToolStripMenuItem("设置", null, Settings);
                var exitItem = new ToolStripMenuItem("退出", null, Exit);
                contextMenu.Items.Add(captureItem);
                contextMenu.Items.Add(settingsItem);
                contextMenu.Items.Add(exitItem);

                _trayIcon.ContextMenuStrip = contextMenu;
                _trayIcon.DoubleClick += Settings;

                // 确保图标可见
                _trayIcon.Visible = true;

                // 立即注册热键
                RegisterHotKey();
            }
            catch (Exception ex)
            {
                // 显示错误信息
                MessageBox.Show($"应用程序启动失败: {ex.Message}", "错误");
                // 即使出错，也要创建一个基本的托盘图标
                _trayIcon = new NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Error,
                    Text = "Capto (错误)",
                    Visible = true
                };
                var contextMenu = new ContextMenuStrip();
                var exitItem = new ToolStripMenuItem("退出", null, Exit);
                contextMenu.Items.Add(exitItem);
                _trayIcon.ContextMenuStrip = contextMenu;
            }
        }

        private System.Drawing.Icon LoadFavicon()
        {
            try
            {
                // 使用base64编码的图标 (favicon.png)
                string base64Icon = "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";
                byte[] iconBytes = System.Convert.FromBase64String(base64Icon);
                using (var stream = new System.IO.MemoryStream(iconBytes))
                {
                    using (var bmp = new System.Drawing.Bitmap(stream))
                    {
                        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
                    }
                }
            }
            catch
            {
                // 如果加载失败，使用系统图标
                return System.Drawing.SystemIcons.Information;
            }
        }

        private void CaptureScreen(object? sender, EventArgs e)
        {
            if (_isCaptureFormOpen)
            {
                return;
            }
            
            _isCaptureFormOpen = true;
            
            // 先隐藏当前窗口（如果有托盘图标窗口）
            // 然后捕获屏幕
            Bitmap? screenBitmap = null;
            try
            {
                var screenBounds = Screen.GetBounds(Point.Empty);
                screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
                using (var g = Graphics.FromImage(screenBitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
                }
            }
            catch
            {
                screenBitmap = null;
            }
            
            var captureForm = new ScreenCaptureForm(screenBitmap);
            captureForm.FormClosed += (s, args) => 
            {
                _isCaptureFormOpen = false;
                // 释放截图内存
                screenBitmap?.Dispose();
            };
            captureForm.ShowDialog();
        }

        private void Settings(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_hotkey);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                // 注销旧热键
                if (_hotkeyForm != null)
                {
                    UnregisterHotKey(_hotkeyForm.Handle, HOTKEY_ID);
                    _hotkeyForm.Dispose();
                    _hotkeyForm = null;
                }
                // 更新热键
                _hotkey = settingsForm.Hotkey;
                // 注册新热键
                RegisterHotKey();
            }
        }

        private void RegisterHotKey()
        {
            // 创建一个隐藏的表单来处理热键消息
            if (_hotkeyForm == null)
            {
                _hotkeyForm = new HotkeyForm();
                _hotkeyForm.HotkeyPressed += () => CaptureScreen(null, EventArgs.Empty);
                // 确保表单句柄被创建
                var handle = _hotkeyForm.Handle;
            }

            // 解析热键的修饰键和普通键
            uint modifiers = 0;
            Keys key = _hotkey & ~Keys.Modifiers;
            
            if ((_hotkey & Keys.Alt) == Keys.Alt)
                modifiers |= 1; // MOD_ALT
            if ((_hotkey & Keys.Control) == Keys.Control)
                modifiers |= 2; // MOD_CONTROL
            if ((_hotkey & Keys.Shift) == Keys.Shift)
                modifiers |= 4; // MOD_SHIFT
            
            // 注册热键
            bool success = RegisterHotKey(_hotkeyForm.Handle, HOTKEY_ID, modifiers, (uint)key);
            if (!success)
            {
                // 热键注册失败，可能是因为热键已被占用
                MessageBox.Show($"热键注册失败，可能已被其他程序占用", "错误");
            }
        }

        private void Exit(object? sender, EventArgs e)
        {
            // 注销热键
            if (_hotkeyForm != null)
            {
                UnregisterHotKey(_hotkeyForm.Handle, HOTKEY_ID);
                _hotkeyForm.Dispose();
            }
            _trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 注销热键
                if (_hotkeyForm != null)
                {
                    UnregisterHotKey(_hotkeyForm.Handle, HOTKEY_ID);
                    _hotkeyForm.Dispose();
                }
                _trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        // 内部表单类，用于处理热键消息
        private class HotkeyForm : Form
        {
            public event Action? HotkeyPressed;

            public HotkeyForm()
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Visible = false;
            }

            protected override void WndProc(ref Message m)
            {
                const int WM_HOTKEY = 0x0312;
                const int HOTKEY_ID = 1;

                if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
                {
                    HotkeyPressed?.Invoke();
                }
                base.WndProc(ref m);
            }
        }
    }
}