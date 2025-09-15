using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ctwebplayer
{
    /// <summary>
    /// 综合设置窗口
    /// </summary>
    public partial class SettingsForm : Form
    {
        private ConfigManager _configManager;
        private readonly Dictionary<string, string> _languageMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SettingsForm(ConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();

            // 初始化语言映射
            _languageMap = new Dictionary<string, string>
            {
                { "简体中文", "zh-CN" },
                { "English", "en-US" },
                { "繁體中文", "zh-TW" },
                { "日本語", "ja" },
                { "한국어", "ko" }
            };

            // 订阅语言选择事件
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
            
            // 应用语言设置
            LanguageManager.Instance.ApplyToForm(this);
            
            LoadCurrentSettings();
        }

        /// <summary>
        /// 加载当前设置
        /// </summary>
        private void LoadCurrentSettings()
        {
            // 加载网络设置
            chkAutoIframeNav.Checked = _configManager.Config.EnableAutoIframeNavigation;
            txtBaseURL.Text = _configManager.Config.BaseURL ?? "";

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

            // 加载登录引导设置
            var loginConfig = _configManager.Config.Login;
            if (loginConfig != null)
            {
                chkEnableLogin.Checked = loginConfig.Enabled;
                chkShowSkipButton.Checked = loginConfig.SkipEnabled;
                txtCookieName.Text = loginConfig.CookieName ?? "";
                txtLoginUrl.Text = loginConfig.LoginUrl ?? "";
                txtRegisterUrl.Text = loginConfig.RegisterUrl ?? "";
            }

            // 更新当前窗口大小显示
            UpdateCurrentSizeLabel();

            // 设置语言选择
            string currentLang = _configManager.Config.Language ?? string.Empty;
            string displayName = LanguageManager.SupportedLanguages.TryGetValue(currentLang, out var langName)
                ? langName
                : LanguageManager.SupportedLanguages["zh-CN"];
            if (cmbLanguage.Items.Contains(displayName))
            {
                cmbLanguage.SelectedItem = displayName;
            }
            else
            {
                cmbLanguage.SelectedIndex = 0; // 默认简体中文
            }
        }

        /// <summary>
        /// 更新当前窗口大小标签
        /// </summary>
        private void UpdateCurrentSizeLabel()
        {
            if (this.Owner != null)
            {
                lblCurrentSize.Text = $"{LanguageManager.Instance.GetString("SettingsForm_tabControl_tabInterface_lblCurrentSize")}: {this.Owner.Width} x {this.Owner.Height}";
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
        /// 启用登录引导复选框状态改变事件
        /// </summary>
        private void ChkEnableLogin_CheckedChanged(object sender, EventArgs e)
        {
            chkShowSkipButton.Enabled = chkEnableLogin.Checked;
            txtCookieName.Enabled = chkEnableLogin.Checked;
            txtLoginUrl.Enabled = chkEnableLogin.Checked;
            txtRegisterUrl.Enabled = chkEnableLogin.Checked;
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
            var result = MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_ConfirmClearLogs"),
                LanguageManager.Instance.GetString("Form1_Confirm_Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await LogManager.Instance.ClearLogsAsync();
                    MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_LogsCleared"), LanguageManager.Instance.GetString("Message_Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("清理日志文件时出错", ex);
                    MessageBox.Show($"{LanguageManager.Instance.GetString("SettingsForm_Msg_ClearLogsError")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 重置窗口大小按钮点击事件
        /// </summary>
        private void BtnResetSize_Click(object sender, EventArgs e)
        {
            numWindowWidth.Value = 1236;
            numWindowHeight.Value = 840;
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput()
        {
            // 验证 baseURL
            if (!string.IsNullOrWhiteSpace(txtBaseURL.Text))
            {
                if (!IsValidUrl(txtBaseURL.Text))
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_BaseURLInvalid"),
                        LanguageManager.Instance.GetString("SettingsForm_Msg_InputError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabControl.SelectedIndex = 0; // 切换到网络标签页
                    txtBaseURL.Focus();
                    return false;
                }
            }
            else
            {
                MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_BaseURLEmpty"),
                    LanguageManager.Instance.GetString("SettingsForm_Msg_InputError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedIndex = 0; // 切换到网络标签页
                txtBaseURL.Focus();
                return false;
            }

            // 验证代理设置
            if (chkEnableProxy.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtHttpProxy.Text) &&
                    string.IsNullOrWhiteSpace(txtHttpsProxy.Text) &&
                    string.IsNullOrWhiteSpace(txtSocks5.Text))
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_ProxyRequired"),
                        LanguageManager.Instance.GetString("SettingsForm_Msg_InputError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabControl.SelectedIndex = 0; // 切换到网络标签页
                    return false;
                }

                // 验证HTTP代理格式
                if (!string.IsNullOrWhiteSpace(txtHttpProxy.Text))
                {
                    if (!IsValidProxyFormat(txtHttpProxy.Text, true))
                    {
                        MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_HttpProxyInvalid"),
                            LanguageManager.Instance.GetString("SettingsForm_Msg_InputError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_HttpsProxyInvalid"),
                            LanguageManager.Instance.GetString("SettingsForm_Msg_InputError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_Socks5Invalid"),
                            LanguageManager.Instance.GetString("SettingsForm_Msg_InputError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        /// 验证URL格式
        /// </summary>
        private bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                var uri = new Uri(url);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch
            {
                return false;
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
                _configManager.Config.BaseURL = txtBaseURL.Text.Trim();

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

                // Debug模式设置
                _configManager.Config.DebugMode = chkDebugMode.Checked;

                // 界面设置
                await _configManager.UpdateUIConfigAsync(new UIConfig
                {
                    WindowWidth = (int)numWindowWidth.Value,
                    WindowHeight = (int)numWindowHeight.Value
                });

                // 登录引导设置
                await _configManager.UpdateLoginConfigAsync(new LoginConfig
                {
                    Enabled = chkEnableLogin.Checked,
                    SkipEnabled = chkShowSkipButton.Checked,
                    CookieName = txtCookieName.Text.Trim(),
                    LoginUrl = txtLoginUrl.Text.Trim(),
                    RegisterUrl = txtRegisterUrl.Text.Trim()
                });

                // 保存语言设置
                if (cmbLanguage.SelectedItem is string selectedDisplay && _languageMap.TryGetValue(selectedDisplay, out string langCode))
                {
                    _configManager.Config.Language = langCode;
                }
                else
                {
                    _configManager.Config.Language = "zh-CN"; // 默认
                }

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
                var restartMessage = LanguageManager.Instance.GetString("SettingsForm_Msg_RestartRequired") + "\n";

                if (chkEnableProxy.Checked)
                {
                    needRestart = true;
                    restartMessage += "- " + LanguageManager.Instance.GetString("SettingsForm_tabControl_tabNetwork") + "\n";
                }

                if (_configManager.Config.Ui != null)
                {
                    needRestart = true;
                    restartMessage += "- " + LanguageManager.Instance.GetString("SettingsForm_tabControl_tabInterface") + "\n";
                }

                if (needRestart)
                {
                    MessageBox.Show(restartMessage, LanguageManager.Instance.GetString("Message_Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("保存设置时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("SettingsForm_Msg_SaveError")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_Applied"), LanguageManager.Instance.GetString("Message_Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnApply.Enabled = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var result = MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Proxy_HelpMessage"), LanguageManager.Instance.GetString("SettingsForm_Proxy_HelpTitle"), MessageBoxButtons.OKCancel);
            if (result == DialogResult.Yes)
            {
                string url = "https://zfj.so/auth/register?code=e913e45e0f";
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// 语言选择改变事件处理
        /// </summary>
        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLanguage.SelectedItem is string displayName && _languageMap.TryGetValue(displayName, out string code))
            {
                try
                {
                    LanguageManager.Instance.LoadLanguage(code);
                    
                    // 应用到当前窗体
                    LanguageManager.Instance.ApplyToForm(this);
                    
                    // 提示用户
                    MessageBox.Show(LanguageManager.Instance.GetString("SettingsForm_Msg_LanguageChanged"),
                        LanguageManager.Instance.GetString("Message_Info"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"切换语言失败: {ex.Message}", ex);
                    MessageBox.Show($"{LanguageManager.Instance.GetString("Message_Error")}: {ex.Message}",
                        LanguageManager.Instance.GetString("Message_Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}