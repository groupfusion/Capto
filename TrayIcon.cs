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
                    Text = "Capto截图",
                    Visible = true
                };

                var contextMenu = new ContextMenuStrip();
                var captureItem = new ToolStripMenuItem("截图", null, CaptureScreen);
                var settingsItem = new ToolStripMenuItem("设置", null, Settings);
                var aboutItem = new ToolStripMenuItem("关于", null, ShowAbout);
                var exitItem = new ToolStripMenuItem("退出", null, Exit);
                contextMenu.Items.Add(captureItem);
                contextMenu.Items.Add(settingsItem);
                contextMenu.Items.Add(aboutItem);
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
                var aboutItem = new ToolStripMenuItem("关于", null, ShowAbout);
                var exitItem = new ToolStripMenuItem("退出", null, Exit);
                contextMenu.Items.Add(aboutItem);
                contextMenu.Items.Add(exitItem);
                _trayIcon.ContextMenuStrip = contextMenu;
            }
        }

        private System.Drawing.Icon LoadFavicon()
        {
            return Capto.Utilities.IconUtils.GetAppIcon();
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

        private void ShowAbout(object? sender, EventArgs e)
        {
            AboutForm.ShowAbout();
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