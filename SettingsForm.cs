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
            this.Icon = Capto.Utilities.IconUtils.GetAppIcon();

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