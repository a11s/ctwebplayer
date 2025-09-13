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
        
        /// <summary>
        /// 构造函�?
        /// </summary>
        public SettingsForm(ConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();
            LoadCurrentSettings();
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
            
            // 加载Debug模式设置
            chkDebugMode.Checked = _configManager.Config.DebugMode;
            
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
        /// 启用代理复选框状态改变事�?
        /// </summary>
        private void ChkEnableProxy_CheckedChanged(object sender, EventArgs e)
        {
            grpProxySettings.Enabled = chkEnableProxy.Checked;
        }
        
        /// <summary>
        /// 启用日志复选框状态改变事�?
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
            var result = MessageBox.Show("确定要清理所有日志文件吗？\n\n这将删除所有历史日志记录�?,
                "确认清理", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await LogManager.Instance.ClearLogsAsync();
                    MessageBox.Show("日志文件已清理完成�?, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("清理日志文件时出�?, ex);
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
                    MessageBox.Show("请至少填写一个代理服务器地址�?,
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
                        MessageBox.Show("SOCKS5代理地址格式不正确。\n格式示例�?27.0.0.1:7890",
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
                    MaxFileSize = (long)numMaxFileSize.Value * 1048576 // 转换为字�?
                };
                
                // Debug模式设置
                _configManager.Config.DebugMode = chkDebugMode.Checked;
                
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
                
                LogManager.Instance.Info("设置已保�?);
                
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
                LogManager.Instance.Error("保存设置时出�?, ex);
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
                MessageBox.Show("设置已应用�?, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnApply.Enabled = true;
            }
        }
    }
}
