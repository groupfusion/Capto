using System;
using System.Windows.Forms;

namespace Capto
{
    public class SettingsForm : Form
    {
        private Label? _hotkeyLabel;
        private TextBox? _hotkeyTextBox;
        private Button? _saveButton;
        private Button? _cancelButton;
        private Keys _currentHotkey;
        private bool _isRecording;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Keys Hotkey { get; set; }

        public SettingsForm(Keys currentHotkey)
        {
            this.Text = "设置";
            this.Size = new System.Drawing.Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 设置窗口图标
            try
            {
                // 使用与托盘图标相同的base64编码图标
                string base64Icon = "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";
                byte[] iconBytes = System.Convert.FromBase64String(base64Icon);
                using (var stream = new System.IO.MemoryStream(iconBytes))
                {
                    using (var bmp = new System.Drawing.Bitmap(stream))
                    {
                        this.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());
                    }
                }
            }
            catch
            {
                // 如果加载失败，使用系统图标
                this.Icon = System.Drawing.SystemIcons.Information;
            }

            Hotkey = currentHotkey;
            _currentHotkey = currentHotkey;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // 设置表单的KeyPreview属性为true，确保表单能在控件之前接收键盘事件
            this.KeyPreview = true;

            _hotkeyLabel = new Label
            {
                Text = "全局热键:",
                Location = new System.Drawing.Point(20, 30),
                Size = new System.Drawing.Size(80, 50)
            };

            _hotkeyTextBox = new TextBox
            {
                Text = GetHotkeyString(Hotkey),
                Location = new System.Drawing.Point(100, 30),
                Size = new System.Drawing.Size(150, 50),
                ReadOnly = true
            };
            _hotkeyTextBox.MouseDown += HotkeyTextBox_MouseDown;

            _saveButton = new Button
            {
                Text = "保存",
                Location = new System.Drawing.Point(80, 70),
                Size = new System.Drawing.Size(70, 50)
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                Text = "取消",
                Location = new System.Drawing.Point(160, 70),
                Size = new System.Drawing.Size(70, 50)
            };
            _cancelButton.Click += CancelButton_Click;

            this.Controls.Add(_hotkeyLabel);
            this.Controls.Add(_hotkeyTextBox);
            this.Controls.Add(_saveButton);
            this.Controls.Add(_cancelButton);

            this.KeyDown += SettingsForm_KeyDown;
        }

        private void HotkeyTextBox_MouseDown(object? sender, MouseEventArgs e)
        {
            _isRecording = true;
            if (_hotkeyTextBox != null)
            {
                // 清除原有内容，显示提示信息
                _hotkeyTextBox.Text = "按下快捷键.";
                // 确保光标停留在输入框
                _hotkeyTextBox.Focus();
            }
        }

        private void SettingsForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_isRecording)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                
                // 直接使用e.KeyData，它已经包含了修饰键和按键
                Keys keyData = e.KeyData;
                
                // 检查是否是有效的热键组合
                Keys key = keyData & ~Keys.Modifiers;
                if (key != Keys.None && key != Keys.Menu)
                {
                    _currentHotkey = keyData;
                    if (_hotkeyTextBox != null)
                    {
                        _hotkeyTextBox.Text = GetHotkeyString(_currentHotkey);
                        // 保持录制状态，允许再次按下任何键重新设置
                        _isRecording = true;
                        // 确保光标停留在输入框
                        _hotkeyTextBox.Focus();
                    }
                }
                else if ((keyData & Keys.Alt) == Keys.Alt || (keyData & Keys.Control) == Keys.Control || (keyData & Keys.Shift) == Keys.Shift)
                {
                    // 如果只按下了修饰键，继续录制
                    if (_hotkeyTextBox != null)
                    {
                        _hotkeyTextBox.Text = GetHotkeyString(keyData);
                    }
                }
            }
        }

        private string GetHotkeyString(Keys hotkey)
        {
            string result = "";
            if ((hotkey & Keys.Alt) == Keys.Alt)
                result += "Alt+";
            if ((hotkey & Keys.Control) == Keys.Control)
                result += "Ctrl+";
            if ((hotkey & Keys.Shift) == Keys.Shift)
                result += "Shift+";
            // 过滤掉修饰键，只保留实际的按键
            // 不使用Keys.Menu过滤，因为它可能会影响正常按键
            Keys key = hotkey & ~Keys.Modifiers;
            if (key != Keys.None)
                result += key.ToString();
            return result;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            Hotkey = _currentHotkey;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}