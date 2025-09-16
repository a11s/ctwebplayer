using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace ctwebplayer
{
    /// <summary>
    /// 语言管理器，负责管理应用程序的多语言资源
    /// </summary>
    public class LanguageManager
    {
        private static readonly Lazy<LanguageManager> _instance = new Lazy<LanguageManager>(() => new LanguageManager());
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;
        private static ConfigManager _configManager;

        /// <summary>
        /// 获取LanguageManager单例实例
        /// </summary>
        public static LanguageManager Instance => _instance.Value;

        /// <summary>
        /// 获取当前语言文化信息
        /// </summary>
        public CultureInfo CurrentCulture => _currentCulture;

        /// <summary>
        /// 语言改变事件
        /// </summary>
        public event EventHandler LanguageChanged;

        /// <summary>
        /// 获取支持的语言列表
        /// </summary>
        public static readonly Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>
        {
            { "en-US", "English" },
            { "zh-CN", "简体中文" },
            { "zh-TW", "繁體中文" },
            { "ja", "日本語" },
            { "ko", "한국어" }
        };

        /// <summary>
        /// 私有构造函数（单例模式）
        /// </summary>
        private LanguageManager()
        {
            // 初始化资源管理器
            _resourceManager = new ResourceManager("ctwebplayer.Resources.Strings", typeof(LanguageManager).Assembly);
            
            // 配置管理器将在 SetConfigManager 中设置
        }

        /// <summary>
        /// 设置配置管理器实例
        /// </summary>
        /// <param name="configManager">配置管理器实例</param>
        public static void SetConfigManager(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>
        /// 初始化语言设置（公开方法，确保在应用启动时调用）
        /// </summary>
        public void Initialize()
        {
            InitializeLanguage();
        }

        /// <summary>
        /// 初始化语言设置
        /// </summary>
        private void InitializeLanguage()
        {
            try
            {
                LogManager.Instance.Info($"开始初始化语言，ConfigManager 是否为空: {_configManager == null}");
                
                string savedLanguage = GetSavedLanguage();
                LogManager.Instance.Info($"从配置读取的语言: {savedLanguage ?? "null"}");
                
                if (!string.IsNullOrEmpty(savedLanguage) && SupportedLanguages.ContainsKey(savedLanguage))
                {
                    // 使用保存的语言
                    LogManager.Instance.Info($"使用保存的语言设置: {savedLanguage}");
                    LoadLanguage(savedLanguage);
                }
                else
                {
                    // 自动检测系统语言
                    LogManager.Instance.Info("没有有效的保存语言，使用系统语言检测");
                    DetectSystemLanguage();
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"语言初始化失败: {ex.Message}", ex);
                // 回退到默认英语
                LoadLanguage("en-US");
            }
        }

        /// <summary>
        /// 获取保存的语言设置
        /// </summary>
        private string GetSavedLanguage()
        {
            // 从配置中获取语言设置
            return _configManager?.Config?.Language ?? "zh-CN";
        }

        /// <summary>
        /// 保存语言设置
        /// </summary>
        private void SaveLanguage(string cultureName)
        {
            // 保存到配置文件
            if (_configManager?.Config != null)
            {
                _configManager.Config.Language = cultureName;
                _configManager.SaveConfig();
            }
        }

        /// <summary>
        /// 自动检测系统语言
        /// </summary>
        private void DetectSystemLanguage()
        {
            try
            {
                // 获取系统当前语言
                CultureInfo systemCulture = CultureInfo.CurrentUICulture;
                string originalCulture = systemCulture.Name;
                string cultureName = originalCulture;

                // 处理英语中性文化
                if (cultureName == "en")
                {
                    cultureName = "en-US";
                }

                LogManager.Instance.Info($"检测到系统语言: {originalCulture}, 映射到: {cultureName}");

                // 检查是否支持该语言
                if (SupportedLanguages.ContainsKey(cultureName))
                {
                    LogManager.Instance.Info($"使用支持的系统语言: {cultureName}");
                    LoadLanguage(cultureName);
                }
                else if (cultureName.StartsWith("zh"))
                {
                    // 中文的特殊处理
                    string zhVariant = cultureName.Contains("TW") || cultureName.Contains("HK") || cultureName.Contains("MO") ? "zh-TW" : "zh-CN";
                    LogManager.Instance.Info($"系统中文变体不支持，使用: {zhVariant}");
                    LoadLanguage(zhVariant);
                }
                else if (cultureName.StartsWith("ja"))
                {
                    LogManager.Instance.Info("使用日语");
                    LoadLanguage("ja");
                }
                else if (cultureName.StartsWith("ko"))
                {
                    LogManager.Instance.Info("使用韩语");
                    LoadLanguage("ko");
                }
                else
                {
                    // 默认使用英语
                    LogManager.Instance.Info($"系统语言 {cultureName} 不支持，回退到英语 (en-US)");
                    LoadLanguage("en-US");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"系统语言检测失败: {ex.Message}", ex);
                // 回退到默认英语
                LoadLanguage("en-US");
            }
        }

        /// <summary>
        /// 加载指定的语言
        /// </summary>
        /// <param name="cultureName">语言文化名称（如：zh-CN）</param>
        public void LoadLanguage(string cultureName)
        {
            if (!SupportedLanguages.ContainsKey(cultureName))
            {
                throw new ArgumentException($"不支持的语言: {cultureName}");
            }

            try
            {
                _currentCulture = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentUICulture = _currentCulture;
                Thread.CurrentThread.CurrentCulture = _currentCulture;
                
                // 保存语言设置
                SaveLanguage(cultureName);
                
                LogManager.Instance.Info($"语言已切换到: {SupportedLanguages[cultureName]} ({cultureName})");

                // 触发语言改变事件
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"切换语言失败: {ex.Message}", ex);
                // 回退到英语
                try
                {
                    _currentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = _currentCulture;
                    Thread.CurrentThread.CurrentCulture = _currentCulture;
                    SaveLanguage("en-US");
                    LogManager.Instance.Info("语言加载失败，回退到英语");
                }
                catch
                {
                    // 如果英语也失败，使用不变量
                    _currentCulture = CultureInfo.InvariantCulture;
                }
                throw;
            }
        }

        /// <summary>
        /// 获取指定键的本地化字符串
        /// </summary>
        /// <param name="key">资源键</param>
        /// <returns>本地化的字符串</returns>
        public string GetString(string key)
        {
            try
            {
                // 诊断日志：记录正在查找的键和当前文化
                LogManager.Instance.Debug($"[LanguageManager] 正在查找资源键: {key}, 当前语言: {_currentCulture.Name}");
                
                string value = _resourceManager.GetString(key, _currentCulture);
                if (string.IsNullOrEmpty(value))
                {
                    // 尝试从默认资源获取
                    LogManager.Instance.Debug($"[LanguageManager] 在 {_currentCulture.Name} 中找不到键 {key}，尝试默认资源");
                    value = _resourceManager.GetString(key, CultureInfo.InvariantCulture);
                    
                    if (string.IsNullOrEmpty(value))
                    {
                        LogManager.Instance.Warning($"[LanguageManager] 找不到资源键: {key} (在所有资源文件中都不存在)");
                        // 诊断信息：列出可能的原因
                        LogManager.Instance.Debug($"[LanguageManager] 可能的原因: 1) 键名拼写错误 2) 资源文件中缺少此键 3) 资源文件未正确加载");
                        return key; // 返回键名作为后备
                    }
                    else
                    {
                        LogManager.Instance.Debug($"[LanguageManager] 在默认资源中找到键 {key}");
                    }
                }
                else
                {
                    LogManager.Instance.Debug($"[LanguageManager] 成功找到资源键 {key} = {value.Substring(0, Math.Min(50, value.Length))}...");
                }
                return value;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[LanguageManager] 获取资源字符串失败 - 键: {key}, 错误: {ex.Message}", ex);
                return key; // 返回键名作为后备
            }
        }

        /// <summary>
        /// 获取格式化的本地化字符串
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化后的本地化字符串</returns>
        public string GetString(string key, params object[] args)
        {
            string format = GetString(key);
            try
            {
                // 添加调试日志
                LogManager.Instance.Debug($"[LanguageManager.GetString] 格式化 - 键: {key}, 格式字符串: '{format}', 参数数量: {args?.Length ?? 0}");
                
                // 如果格式字符串就是key本身（找不到资源），或者不包含占位符，返回默认格式
                if (format == key || !format.Contains("{"))
                {
                    LogManager.Instance.Warning($"[LanguageManager.GetString] 资源键 '{key}' 找不到或格式无效，使用默认格式");
                    // 根据参数数量构建默认格式
                    if (args != null && args.Length > 0)
                    {
                        if (key == "CookieManager_StatusFormat" && args.Length == 3)
                        {
                            // 为CookieManager_StatusFormat提供默认格式
                            format = "Total: {0} | Displayed: {1} | Selected: {2}";
                        }
                        else
                        {
                            // 通用默认格式
                            var placeholders = string.Join(" | ", Enumerable.Range(0, args.Length).Select(i => $"{{{i}}}"));
                            format = placeholders;
                        }
                        LogManager.Instance.Debug($"[LanguageManager.GetString] 使用默认格式: '{format}'");
                    }
                    else
                    {
                        return format;
                    }
                }
                
                // 记录参数详情
                if (args != null && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        LogManager.Instance.Debug($"[LanguageManager.GetString] 参数[{i}]: {args[i]?.ToString() ?? "null"}");
                    }
                }
                
                return string.Format(format, args);
            }
            catch (FormatException fex)
            {
                LogManager.Instance.Error($"格式化资源字符串失败 - 键: {key}, 格式: '{format}', 参数数量: {args?.Length ?? 0}, 错误: {fex.Message}", fex);
                // 返回一个安全的字符串
                if (args != null && args.Length > 0)
                {
                    return string.Join(" | ", args.Select(a => a?.ToString() ?? ""));
                }
                return format;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"格式化资源字符串失败 - 键: {key}, 错误: {ex.Message}", ex);
                return format;
            }
        }

        /// <summary>
        /// 应用语言到窗体
        /// </summary>
        /// <param name="form">要应用语言的窗体</param>
        public void ApplyToForm(Form form)
        {
            if (form == null) return;

            try
            {
                // 更新窗体标题
                string titleKey = $"{form.Name}_Title";
                string title = GetString(titleKey);
                if (title != titleKey)
                {
                    form.Text = title;
                }

                // 递归更新所有控件
                ApplyToControls(form.Controls);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"应用语言到窗体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 递归应用语言到控件集合
        /// </summary>
        /// <param name="controls">控件集合</param>
        private void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // 获取控件的资源键
                string resourceKey = GetResourceKey(control);
                
                if (!string.IsNullOrEmpty(resourceKey))
                {
                    string localizedText = GetString(resourceKey);
                    if (localizedText != resourceKey)
                    {
                        control.Text = localizedText;
                    }
                }

                // 处理特殊控件
                switch (control)
                {
                    case MenuStrip menuStrip:
                        ApplyToMenuItems(menuStrip.Items);
                        break;
                    case ToolStrip toolStrip:
                        ApplyToToolStripItems(toolStrip.Items);
                        break;
                    case ListView listView:
                        ApplyToListViewColumns(listView.Columns);
                        break;
                    case DataGridView dataGridView:
                        ApplyToDataGridViewColumns(dataGridView.Columns);
                        break;
                }

                // 递归处理子控件
                if (control.Controls.Count > 0)
                {
                    ApplyToControls(control.Controls);
                }
            }
        }

        /// <summary>
        /// 获取控件的资源键
        /// </summary>
        /// <param name="control">控件</param>
        /// <returns>资源键</returns>
        private string GetResourceKey(Control control)
        {
            // 如果控件有Tag属性设置为资源键，优先使用
            if (control.Tag is string tagKey && !string.IsNullOrEmpty(tagKey))
            {
                // 如果 Tag 以 "RES_" 开头，去掉前缀
                if (tagKey.StartsWith("RES_"))
                {
                    return tagKey.Substring(4);
                }
                return tagKey;
            }
            
            // 根据控件类型和名称生成资源键
            if (control is Button)
                return $"Button_{control.Name}";
            else if (control is Label)
                return $"Label_{control.Name}";
            else if (control is CheckBox)
                return $"CheckBox_{control.Name}";
            else if (control is RadioButton)
                return $"RadioButton_{control.Name}";
            else if (control is GroupBox)
                return $"GroupBox_{control.Name}";
            else if (control is TabPage)
                return $"TabPage_{control.Name}";

            return null;
        }

        /// <summary>
        /// 应用语言到菜单项
        /// </summary>
        /// <param name="items">菜单项集合</param>
        private void ApplyToMenuItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    string resourceKey = $"Menu_{menuItem.Name}";
                    string localizedText = GetString(resourceKey);
                    if (localizedText != resourceKey)
                    {
                        menuItem.Text = localizedText;
                    }

                    // 递归处理子菜单
                    if (menuItem.DropDownItems.Count > 0)
                    {
                        ApplyToMenuItems(menuItem.DropDownItems);
                    }
                }
            }
        }

        /// <summary>
        /// 应用语言到工具栏项
        /// </summary>
        /// <param name="items">工具栏项集合</param>
        private void ApplyToToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                // 如果项有 Tag 属性设置为资源键，使用它
                if (item.Tag is string resourceKey && !string.IsNullOrEmpty(resourceKey))
                {
                    string localizedText = GetString(resourceKey);
                    if (localizedText != resourceKey)
                    {
                        item.Text = localizedText;
                    }
                }
                
                // 递归处理子项
                if (item is ToolStripDropDownButton dropDownButton && dropDownButton.DropDownItems.Count > 0)
                {
                    ApplyToToolStripItems(dropDownButton.DropDownItems);
                }
                else if (item is ToolStripMenuItem menuItem && menuItem.DropDownItems.Count > 0)
                {
                    ApplyToToolStripItems(menuItem.DropDownItems);
                }
            }
        }

        /// <summary>
        /// 应用语言到ListView列
        /// </summary>
        /// <param name="columns">列集合</param>
        private void ApplyToListViewColumns(ListView.ColumnHeaderCollection columns)
        {
            foreach (ColumnHeader column in columns)
            {
                string resourceKey = $"Column_{column.Name}";
                string localizedText = GetString(resourceKey);
                if (localizedText != resourceKey)
                {
                    column.Text = localizedText;
                }
            }
        }

        /// <summary>
        /// 应用语言到DataGridView列
        /// </summary>
        /// <param name="columns">列集合</param>
        private void ApplyToDataGridViewColumns(DataGridViewColumnCollection columns)
        {
            foreach (DataGridViewColumn column in columns)
            {
                string resourceKey = $"Column_{column.Name}";
                string localizedText = GetString(resourceKey);
                if (localizedText != resourceKey)
                {
                    column.HeaderText = localizedText;
                }
            }
        }

        /// <summary>
        /// 显示语言选择对话框
        /// </summary>
        /// <param name="owner">父窗体</param>
        /// <returns>是否成功选择了新语言</returns>
        public bool ShowLanguageDialog(Form owner)
        {
            using (var dialog = new Form())
            {
                dialog.Text = GetString("Settings_Language_Title");
                dialog.Size = new System.Drawing.Size(300, 200);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var listBox = new ListBox
                {
                    Dock = DockStyle.Top,
                    Height = 120
                };

                // 添加支持的语言
                foreach (var lang in SupportedLanguages)
                {
                    int index = listBox.Items.Add(lang.Value);
                    if (lang.Key == _currentCulture.Name)
                    {
                        listBox.SelectedIndex = index;
                    }
                }

                var btnOK = new Button
                {
                    Text = GetString("Settings_Button_OK"),
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(50, 130),
                    Size = new System.Drawing.Size(75, 23)
                };

                var btnCancel = new Button
                {
                    Text = GetString("Settings_Button_Cancel"),
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(175, 130),
                    Size = new System.Drawing.Size(75, 23)
                };

                dialog.Controls.Add(listBox);
                dialog.Controls.Add(btnOK);
                dialog.Controls.Add(btnCancel);

                if (dialog.ShowDialog(owner) == DialogResult.OK && listBox.SelectedIndex >= 0)
                {
                    string selectedLanguage = SupportedLanguages.Keys.ElementAt(listBox.SelectedIndex);
                    if (selectedLanguage != _currentCulture.Name)
                    {
                        LoadLanguage(selectedLanguage);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}