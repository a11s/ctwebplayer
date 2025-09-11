using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ctwebplayer
{
    /// <summary>
    /// 综合设置窗口
    /// </summary>
    public partial class SettingsForm : Form
    {
        private ConfigManager _configManager;
        private TabControl tabControl;
        
        // 网络标签页控件
        private CheckBox chkEnableProxy;
        private GroupBox grpProxySettings;
        private TextBox txtHttpProxy;
        private TextBox txtHttpsProxy;
        private TextBox txtSocks5;
        private CheckBox chkAutoIframeNav;
        
        // 日志标签页控件
        private CheckBox chkEnableLogging;
        private ComboBox cmbLogLevel;
        private NumericUpDown numMaxFileSize;
        private Button btnViewLogs;
        private Button btnClearLogs;
        
        // 界面标签页控件
        private NumericUpDown numWindowWidth;
        private NumericUpDown numWindowHeight;
        private Button btnResetSize;
        private Label lblCurrentSize;
        
        // 底部按钮
        private Button btnSave;
        private Button btnApply;
        private Button btnCancel;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SettingsForm(ConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();
            LoadCurrentSettings();
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "应用程序设置";
            this.Size = new Size(500, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // 创建TabControl
            tabControl = new TabControl
            {
                Location = new Point(12, 12),
                Size = new Size(460, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            
            // 创建标签页
            var tabNetwork = new TabPage("网络");
            var tabLogging = new TabPage("日志");
            var tabInterface = new TabPage("界面");
            
            // 初始化各标签页
            InitializeNetworkTab(tabNetwork);
            InitializeLoggingTab(tabLogging);
            InitializeInterfaceTab(tabInterface);
            
            // 添加标签页到TabControl
            tabControl.TabPages.Add(tabNetwork);
            tabControl.TabPages.Add(tabLogging);
            tabControl.TabPages.Add(tabInterface);
            
            // 创建底部按钮
            btnSave = new Button
            {
                Text = "保存",
                Location = new Point(235, 510),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSave.Click += BtnSave_Click;
            
            btnApply = new Button
            {
                Text = "应用",
                Location = new Point(316, 510),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnApply.Click += BtnApply_Click;
            
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(397, 510),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            
            // 添加控件到窗体
            this.Controls.Add(tabControl);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnCancel);
        }
        
        /// <summary>
        /// 初始化网络标签页
        /// </summary>
        private void InitializeNetworkTab(TabPage tabPage)
        {
            // 自动导航到iframe内容
            chkAutoIframeNav = new CheckBox
            {
                Text = "自动导航到 iframe 内容",
                Location = new Point(20, 20),
                Size = new Size(200, 25),
                Checked = false
            };
            
            // 启用代理复选框
            chkEnableProxy = new CheckBox
            {
                Text = "启用代理服务器",
                Location = new Point(20, 60),
                Size = new Size(150, 25),
                Checked = false
            };
            chkEnableProxy.CheckedChanged += ChkEnableProxy_CheckedChanged;
            
            // 代理设置组
            grpProxySettings = new GroupBox
            {
                Text = "代理服务器设置",
                Location = new Point(20, 95),
                Size = new Size(410, 180),
                Enabled = false
            };
            
            // HTTP代理
            var lblHttpProxy = new Label
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
                Size = new Size(280, 25),
                PlaceholderText = "例如: http://127.0.0.1:7890"
            };
            grpProxySettings.Controls.Add(txtHttpProxy);
            
            // HTTPS代理
            var lblHttpsProxy = new Label
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
                Size = new Size(280, 25),
                PlaceholderText = "例如: http://127.0.0.1:7890"
            };
            grpProxySettings.Controls.Add(txtHttpsProxy);
            
            // SOCKS5代理
            var lblSocks5 = new Label
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
                Size = new Size(280, 25),
                PlaceholderText = "例如: 127.0.0.1:7890"
            };
            grpProxySettings.Controls.Add(txtSocks5);
            
            // 说明标签
            var lblNote = new Label
            {
                Text = "注意：优先使用SOCKS5代理，其次是HTTP/HTTPS代理。\n更改代理设置后需要重启浏览器才能生效。",
                Location = new Point(20, 140),
                Size = new Size(370, 35),
                ForeColor = Color.Gray
            };
            grpProxySettings.Controls.Add(lblNote);
            
            // 添加控件到标签页
            tabPage.Controls.Add(chkAutoIframeNav);
            tabPage.Controls.Add(chkEnableProxy);
            tabPage.Controls.Add(grpProxySettings);
        }
        
        /// <summary>
        /// 初始化日志标签页
        /// </summary>
        private void InitializeLoggingTab(TabPage tabPage)
        {
            // 启用日志
            chkEnableLogging = new CheckBox
            {
                Text = "启用日志记录",
                Location = new Point(20, 20),
                Size = new Size(150, 25),
                Checked = true
            };
            chkEnableLogging.CheckedChanged += ChkEnableLogging_CheckedChanged;
            
            // 日志级别
            var lblLogLevel = new Label
            {
                Text = "日志级别:",
                Location = new Point(20, 60),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            
            cmbLogLevel = new ComboBox
            {
                Location = new Point(105, 60),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLogLevel.Items.AddRange(new[] { "Debug", "Info", "Warning", "Error" });
            cmbLogLevel.SelectedIndex = 1; // 默认Info
            
            // 最大文件大小
            var lblMaxFileSize = new Label
            {
                Text = "最大文件大小:",
                Location = new Point(20, 100),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            
            numMaxFileSize = new NumericUpDown
            {
                Location = new Point(105, 100),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 100,
                Value = 10,
                DecimalPlaces = 0
            };
            
            var lblMB = new Label
            {
                Text = "MB",
                Location = new Point(210, 100),
                Size = new Size(30, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            // 日志操作按钮
            btnViewLogs = new Button
            {
                Text = "查看日志",
                Location = new Point(20, 150),
                Size = new Size(100, 30)
            };
            btnViewLogs.Click += BtnViewLogs_Click;
            
            btnClearLogs = new Button
            {
                Text = "清理日志",
                Location = new Point(130, 150),
                Size = new Size(100, 30)
            };
            btnClearLogs.Click += BtnClearLogs_Click;
            
            // 添加控件到标签页
            tabPage.Controls.Add(chkEnableLogging);
            tabPage.Controls.Add(lblLogLevel);
            tabPage.Controls.Add(cmbLogLevel);
            tabPage.Controls.Add(lblMaxFileSize);
            tabPage.Controls.Add(numMaxFileSize);
            tabPage.Controls.Add(lblMB);
            tabPage.Controls.Add(btnViewLogs);
            tabPage.Controls.Add(btnClearLogs);
        }
        
        /// <summary>
        /// 初始化界面标签页
        /// </summary>
        private void InitializeInterfaceTab(TabPage tabPage)
        {
            // 窗口大小设置
            var lblWindowSize = new Label
            {
                Text = "窗口大小设置",
                Location = new Point(20, 20),
                Size = new Size(100, 25),
                Font = new Font(this.Font, FontStyle.Bold)
            };
            
            // 宽度
            var lblWidth = new Label
            {
                Text = "宽度:",
                Location = new Point(20, 60),
                Size = new Size(50, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            
            numWindowWidth = new NumericUpDown
            {
                Location = new Point(75, 60),
                Size = new Size(80, 25),
                Minimum = 800,
                Maximum = 3840,
                Value = 1136,
                DecimalPlaces = 0
            };
            
            var lblWidthPx = new Label
            {
                Text = "像素",
                Location = new Point(160, 60),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            // 高度
            var lblHeight = new Label
            {
                Text = "高度:",
                Location = new Point(220, 60),
                Size = new Size(50, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            
            numWindowHeight = new NumericUpDown
            {
                Location = new Point(275, 60),
                Size = new Size(80, 25),
                Minimum = 600,
                Maximum = 2160,
                Value = 640,
                DecimalPlaces = 0
            };
            
            var lblHeightPx = new Label
            {
                Text = "像素",
                Location = new Point(360, 60),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            // 当前窗口大小
            lblCurrentSize = new Label
            {
                Text = "当前窗口大小: 未知",
                Location = new Point(20, 100),
                Size = new Size(200, 25),
                ForeColor = Color.Gray
            };
            
            // 重置按钮
            btnResetSize = new Button
            {
                Text = "重置为默认",
                Location = new Point(20, 140),
                Size = new Size(100, 30)
            };
            btnResetSize.Click += BtnResetSize_Click;
            
            // 说明
            var lblSizeNote = new Label
            {
                Text = "注意：窗口大小设置将在下次启动应用程序时生效。",
                Location = new Point(20, 190),
                Size = new Size(400, 25),
                ForeColor = Color.Gray
            };
            
            // 添加控件到标签页
            tabPage.Controls.Add(lblWindowSize);
            tabPage.Controls.Add(lblWidth);
            tabPage.Controls.Add(numWindowWidth);
            tabPage.Controls.Add(lblWidthPx);
            tabPage.Controls.Add(lblHeight);
            tabPage.Controls.Add(numWindowHeight);
            tabPage.Controls.Add(lblHeightPx);
            tabPage.Controls.Add(lblCurrentSize);
            tabPage.Controls.Add(btnResetSize);
            tabPage.Controls.Add(lblSizeNote);
        }
        
        /// <summary>
        /// 加载当前设置
        /// </summary>
        private void LoadCurrentSettings()
        {
            // 加载网络设置
            chkAutoIframeNav.Checked = _configManager.Config.EnableAutoIframeNavigation;
            
            var proxyConfig = _configManager.Config.Proxy;
            if (proxyConfig != null)
            {
                chkEnableProxy.Checked = proxyConfig.Enabled;
                txtHttpProxy.Text = proxyConfig.HttpProxy ?? "";
                txtHttpsProxy.Text = proxyConfig.HttpsProxy ?? "";
                txtSocks5.Text = proxyConfig.Socks5 ?? "";
            }
            
            // 加载日志设置
            var loggingConfig = _configManager.Config.Logging;
            if (loggingConfig != null)
            {
                chkEnableLogging.Checked = loggingConfig.Enabled;
                cmbLogLevel.SelectedItem = loggingConfig.LogLevel;
                numMaxFileSize.Value = (decimal)(loggingConfig.MaxFileSize / 1048576); // 转换为MB
            }
            
            // 加载界面设置
            if (_configManager.Config.Ui != null)
            {
                numWindowWidth.Value = _configManager.Config.Ui.WindowWidth;
                numWindowHeight.Value = _configManager.Config.Ui.WindowHeight;
            }
            
            // 更新当前窗口大小显示
            UpdateCurrentSizeLabel();
        }
        
        /// <summary>
        /// 更新当前窗口大小标签
        /// </summary>
        private void UpdateCurrentSizeLabel()
        {
            if (this.Owner != null)
            {
                lblCurrentSize.Text = $"当前窗口大小: {this.Owner.Width} x {this.Owner.Height}";
            }
        }
        
        /// <summary>
        /// 启用代理复选框状态改变事件
        /// </summary>
        private void ChkEnableProxy_CheckedChanged(object sender, EventArgs e)
        {
            grpProxySettings.Enabled = chkEnableProxy.Checked;
        }
        
        /// <summary>
        /// 启用日志复选框状态改变事件
        /// </summary>
        private void ChkEnableLogging_CheckedChanged(object sender, EventArgs e)
        {
            cmbLogLevel.Enabled = chkEnableLogging.Checked;
            numMaxFileSize.Enabled = chkEnableLogging.Checked;
            btnViewLogs.Enabled = chkEnableLogging.Checked;
            btnClearLogs.Enabled = chkEnableLogging.Checked;
        }
        
        /// <summary>
        /// 查看日志按钮点击事件
        /// </summary>
        private void BtnViewLogs_Click(object sender, EventArgs e)
        {
            using (var logViewerForm = new LogViewerForm())
            {
                logViewerForm.ShowDialog(this);
            }
        }
        
        /// <summary>
        /// 清理日志按钮点击事件
        /// </summary>
        private async void BtnClearLogs_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定要清理所有日志文件吗？\n\n这将删除所有历史日志记录。",
                "确认清理", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await LogManager.Instance.ClearLogsAsync();
                    MessageBox.Show("日志文件已清理完成。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("清理日志文件时出错", ex);
                    MessageBox.Show($"清理日志文件时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        /// <summary>
        /// 重置窗口大小按钮点击事件
        /// </summary>
        private void BtnResetSize_Click(object sender, EventArgs e)
        {
            numWindowWidth.Value = 1136;
            numWindowHeight.Value = 640;
        }
        
        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput()
        {
            // 验证代理设置
            if (chkEnableProxy.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtHttpProxy.Text) &&
                    string.IsNullOrWhiteSpace(txtHttpsProxy.Text) &&
                    string.IsNullOrWhiteSpace(txtSocks5.Text))
                {
                    MessageBox.Show("请至少填写一个代理服务器地址。",
                        "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabControl.SelectedIndex = 0; // 切换到网络标签页
                    return false;
                }
                
                // 验证HTTP代理格式
                if (!string.IsNullOrWhiteSpace(txtHttpProxy.Text))
                {
                    if (!IsValidProxyFormat(txtHttpProxy.Text, true))
                    {
                        MessageBox.Show("HTTP代理地址格式不正确。\n格式示例：http://127.0.0.1:7890",
                            "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tabControl.SelectedIndex = 0;
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
                        tabControl.SelectedIndex = 0;
                        txtHttpsProxy.Focus();
                        return false;
                    }
                }
                
                // 验证SOCKS5代理格式
                if (!string.IsNullOrWhiteSpace(txtSocks5.Text))
                {
                    if (!IsValidProxyFormat(txtSocks5.Text, false))
                    {
                        MessageBox.Show("SOCKS5代理地址格式不正确。\n格式示例：127.0.0.1:7890",
                            "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tabControl.SelectedIndex = 0;
                        txtSocks5.Focus();
                        return false;
                    }
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
        
        /// <summary>
        /// 保存设置
        /// </summary>
        private async Task<bool> SaveSettings()
        {
            if (!ValidateInput())
            {
                return false;
            }
            
            try
            {
                // 更新配置对象
                // 网络设置
                _configManager.Config.EnableAutoIframeNavigation = chkAutoIframeNav.Checked;
                
                var proxyConfig = new ProxyConfig
                {
                    Enabled = chkEnableProxy.Checked,
                    HttpProxy = txtHttpProxy.Text.Trim(),
                    HttpsProxy = txtHttpsProxy.Text.Trim(),
                    Socks5 = txtSocks5.Text.Trim()
                };
                _configManager.Config.Proxy = proxyConfig;
                
                // 日志设置
                _configManager.Config.Logging = new LoggingConfig
                {
                    Enabled = chkEnableLogging.Checked,
                    LogLevel = cmbLogLevel.SelectedItem?.ToString() ?? "Info",
                    MaxFileSize = (long)numMaxFileSize.Value * 1048576 // 转换为字节
                };
                
                // 界面设置
                await _configManager.UpdateUIConfigAsync(new UIConfig
                {
                    WindowWidth = (int)numWindowWidth.Value,
                    WindowHeight = (int)numWindowHeight.Value
                });
                
                // 保存配置
                await _configManager.SaveConfigAsync();
                
                // 应用日志设置
                if (_configManager.Config.Logging != null)
                {
                    if (Enum.TryParse<LogLevel>(_configManager.Config.Logging.LogLevel, true, out var logLevel))
                    {
                        LogManager.Instance.Configure(
                            _configManager.Config.Logging.Enabled,
                            logLevel,
                            _configManager.Config.Logging.MaxFileSize
                        );
                    }
                }
                
                LogManager.Instance.Info("设置已保存");
                
                // 提示需要重启的设置
                var needRestart = false;
                var restartMessage = "以下设置需要重启应用程序才能生效：\n";
                
                if (chkEnableProxy.Checked)
                {
                    needRestart = true;
                    restartMessage += "- 代理设置\n";
                }
                
                if (_configManager.Config.Ui != null)
                {
                    needRestart = true;
                    restartMessage += "- 窗口大小设置\n";
                }
                
                if (needRestart)
                {
                    MessageBox.Show(restartMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("保存设置时出错", ex);
                MessageBox.Show($"保存设置时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (await SaveSettings())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        
        /// <summary>
        /// 应用按钮点击事件
        /// </summary>
        private async void BtnApply_Click(object sender, EventArgs e)
        {
            if (await SaveSettings())
            {
                btnApply.Enabled = false;
                MessageBox.Show("设置已应用。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnApply.Enabled = true;
            }
        }
    }
}