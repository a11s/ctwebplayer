using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ctwebplayer
{
    /// <summary>
    /// 代理设置对话�?
    /// </summary>
    public partial class ProxySettingsForm : Form
    {
        private ConfigManager _configManager;
        private CheckBox chkEnableProxy = null!;
        private GroupBox grpProxySettings = null!;
        private Label lblHttpProxy = null!;
        private TextBox txtHttpProxy = null!;
        private Label lblHttpsProxy = null!;
        private TextBox txtHttpsProxy = null!;
        private Label lblSocks5 = null!;
        private TextBox txtSocks5 = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;
        private Label lblNote = null!;

        /// <summary>
        /// 构造函�?
        /// </summary>
        public ProxySettingsForm(ConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();
            LoadCurrentSettings();
        }

        /// <summary>
        /// 初始化组�?
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "代理设置";
            this.Size = new Size(450, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 启用代理复选框
            chkEnableProxy = new CheckBox
            {
                Text = "启用代理服务�?,
                Location = new Point(20, 20),
                Size = new Size(150, 25),
                Checked = false
            };
            chkEnableProxy.CheckedChanged += ChkEnableProxy_CheckedChanged;

            // 代理设置�?
            grpProxySettings = new GroupBox
            {
                Text = "代理服务器设�?,
                Location = new Point(20, 55),
                Size = new Size(400, 180),
                Enabled = false
            };

            // HTTP代理
            lblHttpProxy = new Label
            {
                Text = "HTTP 代理:",
                Location = new Point(20, 30),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpProxySettings.Controls.Add(lblHttpProxy);

            txtHttpProxy = new TextBox
            {
                Location = new Point(105, 30),
                Size = new Size(270, 25),
                PlaceholderText = "例如: http://127.0.0.1:7890"
            };
            grpProxySettings.Controls.Add(txtHttpProxy);

            // HTTPS代理
            lblHttpsProxy = new Label
            {
                Text = "HTTPS 代理:",
                Location = new Point(20, 65),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpProxySettings.Controls.Add(lblHttpsProxy);

            txtHttpsProxy = new TextBox
            {
                Location = new Point(105, 65),
                Size = new Size(270, 25),
                PlaceholderText = "例如: http://127.0.0.1:7890"
            };
            grpProxySettings.Controls.Add(txtHttpsProxy);

            // SOCKS5代理
            lblSocks5 = new Label
            {
                Text = "SOCKS5 代理:",
                Location = new Point(20, 100),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpProxySettings.Controls.Add(lblSocks5);

            txtSocks5 = new TextBox
            {
                Location = new Point(105, 100),
                Size = new Size(270, 25),
                PlaceholderText = "例如: 127.0.0.1:7890"
            };
            grpProxySettings.Controls.Add(txtSocks5);

            // 说明标签
            lblNote = new Label
            {
                Text = "注意：优先使用SOCKS5代理，其次是HTTP/HTTPS代理。\n更改代理设置后需要重启浏览器才能生效�?,
                Location = new Point(20, 140),
                Size = new Size(360, 35),
                ForeColor = Color.Gray
            };
            grpProxySettings.Controls.Add(lblNote);

            // 保存按钮
            btnSave = new Button
            {
                Text = "保存",
                Location = new Point(245, 250),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(340, 250),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // 添加控件到窗�?
            this.Controls.Add(chkEnableProxy);
            this.Controls.Add(grpProxySettings);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        /// <summary>
        /// 加载当前设置
        /// </summary>
        private void LoadCurrentSettings()
        {
            var proxyConfig = _configManager.Config.Proxy;
            if (proxyConfig != null)
            {
                chkEnableProxy.Checked = proxyConfig.Enabled;
                txtHttpProxy.Text = proxyConfig.HttpProxy ?? "";
                txtHttpsProxy.Text = proxyConfig.HttpsProxy ?? "";
                txtSocks5.Text = proxyConfig.Socks5 ?? "";
            }
        }

        /// <summary>
        /// 启用代理复选框状态改变事�?
        /// </summary>
        private void ChkEnableProxy_CheckedChanged(object sender, EventArgs e)
        {
            grpProxySettings.Enabled = chkEnableProxy.Checked;
        }

        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // 验证输入
            if (chkEnableProxy.Checked)
            {
                if (!ValidateProxyInput())
                {
                    return;
                }
            }

            try
            {
                // 创建新的代理配置
                var proxyConfig = new ProxyConfig
                {
                    Enabled = chkEnableProxy.Checked,
                    HttpProxy = txtHttpProxy.Text.Trim(),
                    HttpsProxy = txtHttpsProxy.Text.Trim(),
                    Socks5 = txtSocks5.Text.Trim()
                };

                // 保存配置
                await _configManager.UpdateProxyConfigAsync(proxyConfig);
                
                // 记录日志
                if (chkEnableProxy.Checked)
                {
                    LogManager.Instance.Info($"代理设置已更�?- HTTP: {proxyConfig.HttpProxy}, HTTPS: {proxyConfig.HttpsProxy}, SOCKS5: {proxyConfig.Socks5}");
                }
                else
                {
                    LogManager.Instance.Info("代理已禁�?);
                }

                // 提示用户需要重�?
                if (chkEnableProxy.Checked)
                {
                    MessageBox.Show("代理设置已保存。\n\n请重启浏览器以应用新的代理设置�?,
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("代理已禁用。\n\n请重启浏览器以应用更改�?,
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("保存代理配置时出�?, ex);
                MessageBox.Show($"保存配置时出错：{ex.Message}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 验证代理输入
        /// </summary>
        private bool ValidateProxyInput()
        {
            // 至少需要填写一个代�?
            if (string.IsNullOrWhiteSpace(txtHttpProxy.Text) &&
                string.IsNullOrWhiteSpace(txtHttpsProxy.Text) &&
                string.IsNullOrWhiteSpace(txtSocks5.Text))
            {
                MessageBox.Show("请至少填写一个代理服务器地址�?,
                    "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 验证HTTP代理格式
            if (!string.IsNullOrWhiteSpace(txtHttpProxy.Text))
            {
                if (!IsValidProxyFormat(txtHttpProxy.Text, true))
                {
                    MessageBox.Show("HTTP代理地址格式不正确。\n格式示例：http://127.0.0.1:7890",
                        "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtHttpProxy.Focus();
                    return false;
                }
            }

            // 验证HTTPS代理格式
            if (!string.IsNullOrWhiteSpace(txtHttpsProxy.Text))
            {
                if (!IsValidProxyFormat(txtHttpsProxy.Text, true))
                {
                    MessageBox.Show("HTTPS代理地址格式不正确。\n格式示例：http://127.0.0.1:7890",
                        "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtHttpsProxy.Focus();
                    return false;
                }
            }

            // 验证SOCKS5代理格式
            if (!string.IsNullOrWhiteSpace(txtSocks5.Text))
            {
                if (!IsValidProxyFormat(txtSocks5.Text, false))
                {
                    MessageBox.Show("SOCKS5代理地址格式不正确。\n格式示例�?27.0.0.1:7890",
                        "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSocks5.Focus();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 验证代理地址格式
        /// </summary>
        private bool IsValidProxyFormat(string proxy, bool requireProtocol)
        {
            if (requireProtocol)
            {
                // HTTP/HTTPS代理需要协议前缀
                var pattern = @"^https?://[\w\.-]+:\d{1,5}$";
                return Regex.IsMatch(proxy, pattern, RegexOptions.IgnoreCase);
            }
            else
            {
                // SOCKS5代理不需要协议前缀
                var pattern = @"^[\w\.-]+:\d{1,5}$";
                return Regex.IsMatch(proxy, pattern, RegexOptions.IgnoreCase);
            }
        }
    }
}
