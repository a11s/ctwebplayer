using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace ctwebplayer
{
    public partial class CookieManagerForm : Form
    {
        private List<CookieItem> cookieList = new List<CookieItem>();
        private BindingList<CookieItem> displayedCookies;
        private LanguageManager languageManager;
        private LogManager logManager;
        private BindingSource bindingSource;
        private string currentSearchText = "";
        private bool groupByDomain = false;
        private CoreWebView2 webView2;
        private CookieManager cookieManager;

        public CookieManagerForm(CoreWebView2 webView2Instance = null)
        {
            InitializeComponent();
            languageManager = LanguageManager.Instance;
            logManager = LogManager.Instance;
            
            logManager.Info($"CookieManagerForm构造函数开始，webView2Instance={webView2Instance != null}");
            
            webView2 = webView2Instance;
            if (webView2 != null)
            {
                logManager.Info($"WebView2实例存在，CookieManager={webView2.CookieManager != null}");
                
                if (webView2.CookieManager != null)
                {
                    cookieManager = new CookieManager(webView2.CookieManager, "", webView2);
                    logManager.Info("CookieManager已创建");
                }
                else
                {
                    logManager.Error("WebView2.CookieManager为null，无法创建CookieManager");
                }
            }
            else
            {
                logManager.Warning("WebView2实例为null");
            }
            
            InitializeData();
            SetupDataGridView();
            ApplyLanguage();
            
            // 如果有WebView2实例，加载真实cookies，否则加载模拟数据
            if (webView2 != null && cookieManager != null)
            {
                logManager.Info("准备异步加载真实cookies");
                _ = LoadCookiesAsync(); // 异步加载真实cookies
            }
            else
            {
                logManager.Info("加载模拟数据");
                LoadMockData(); // 临时使用模拟数据
            }
        }

        private void InitializeData()
        {
            displayedCookies = new BindingList<CookieItem>(cookieList);
            bindingSource = new BindingSource();
            bindingSource.DataSource = displayedCookies;
        }

        private void SetupDataGridView()
        {
            // 不使用数据绑定，直接操作行
            dataGridViewCookies.AutoGenerateColumns = false;
            dataGridViewCookies.DataSource = null; // 清除数据源，手动管理行
            
            // 设置DataGridView样式
            dataGridViewCookies.BorderStyle = BorderStyle.None;
            dataGridViewCookies.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dataGridViewCookies.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCookies.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dataGridViewCookies.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewCookies.BackgroundColor = Color.White;
            dataGridViewCookies.EnableHeadersVisualStyles = false;
            dataGridViewCookies.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCookies.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridViewCookies.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewCookies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dataGridViewCookies.ColumnHeadersHeight = 35;
            dataGridViewCookies.RowTemplate.Height = 30;
            
            // 添加列
            var columns = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Domain", DataPropertyName = "Domain", HeaderText = "域名", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "名称", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Value", DataPropertyName = "Value", HeaderText = "值", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Path", DataPropertyName = "Path", HeaderText = "路径", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Expires", DataPropertyName = "ExpiresDisplay", HeaderText = "过期时间", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Size", DataPropertyName = "Size", HeaderText = "大小", Width = 60 },
                new DataGridViewCheckBoxColumn { Name = "HttpOnly", DataPropertyName = "HttpOnly", HeaderText = "HttpOnly", Width = 70 },
                new DataGridViewCheckBoxColumn { Name = "Secure", DataPropertyName = "Secure", HeaderText = "Secure", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "SameSite", DataPropertyName = "SameSite", HeaderText = "SameSite", Width = 80 }
            };
            
            dataGridViewCookies.Columns.AddRange(columns);
            
            // 设置右键菜单
            var contextMenu = new ContextMenuStrip();
            var copyMenuItem = new ToolStripMenuItem("复制");
            var editMenuItem = new ToolStripMenuItem("编辑");
            var deleteMenuItem = new ToolStripMenuItem("删除");
            
            copyMenuItem.Click += CopyMenuItem_Click;
            editMenuItem.Click += EditMenuItem_Click;
            deleteMenuItem.Click += DeleteMenuItem_Click;
            
            contextMenu.Items.AddRange(new ToolStripItem[] { copyMenuItem, editMenuItem, new ToolStripSeparator(), deleteMenuItem });
            dataGridViewCookies.ContextMenuStrip = contextMenu;
        }

        private void LoadMockData()
        {
            // 添加模拟数据
            var mockCookies = new[]
            {
                new CookieItem
                {
                    Domain = ".example.com",
                    Name = "session_id",
                    Value = "abc123def456",
                    Path = "/",
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = "Lax"
                },
                new CookieItem
                {
                    Domain = ".google.com",
                    Name = "NID",
                    Value = "511=eJw...",
                    Path = "/",
                    Expires = DateTime.Now.AddMonths(6),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = "None"
                },
                new CookieItem
                {
                    Domain = "localhost",
                    Name = "debug_token",
                    Value = "dev_mode_enabled",
                    Path = "/",
                    Expires = DateTime.Now.AddHours(24),
                    HttpOnly = false,
                    Secure = false,
                    SameSite = "Strict"
                },
                new CookieItem
                {
                    Domain = ".github.com",
                    Name = "_gh_sess",
                    Value = "JKL789MNO012",
                    Path = "/",
                    Expires = DateTime.Now.AddDays(14),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = "Lax"
                },
                new CookieItem
                {
                    Domain = ".stackoverflow.com",
                    Name = "prov",
                    Value = "12345-67890",
                    Path = "/",
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = false,
                    Secure = true,
                    SameSite = "None"
                }
            };
            
            cookieList.AddRange(mockCookies);
            RefreshDisplay();
            UpdateStatusBar();
        }

        private void ApplyLanguage()
        {
            // 窗体标题
            this.Text = languageManager.GetString("CookieManager_Title", "Cookie管理器");
            
            // 工具栏按钮
            toolStripButtonRefresh.Text = languageManager.GetString("CookieManager_Refresh", "刷新");
            toolStripButtonAdd.Text = languageManager.GetString("CookieManager_Add", "添加");
            toolStripButtonDelete.Text = languageManager.GetString("CookieManager_Delete", "删除");
            toolStripButtonImport.Text = languageManager.GetString("CookieManager_Import", "导入");
            toolStripButtonExport.Text = languageManager.GetString("CookieManager_Export", "导出");
            toolStripButtonGroupByDomain.Text = languageManager.GetString("CookieManager_GroupByDomain", "按域名分组");
            
            // 搜索标签
            toolStripLabelSearch.Text = languageManager.GetString("CookieManager_Search", "搜索:");
            
            // DataGridView列头
            if (dataGridViewCookies.Columns.Count > 0)
            {
                dataGridViewCookies.Columns["Domain"].HeaderText = languageManager.GetString("CookieManager_Domain", "域名");
                dataGridViewCookies.Columns["Name"].HeaderText = languageManager.GetString("CookieManager_Name", "名称");
                dataGridViewCookies.Columns["Value"].HeaderText = languageManager.GetString("CookieManager_Value", "值");
                dataGridViewCookies.Columns["Path"].HeaderText = languageManager.GetString("CookieManager_Path", "路径");
                dataGridViewCookies.Columns["Expires"].HeaderText = languageManager.GetString("CookieManager_Expires", "过期时间");
                dataGridViewCookies.Columns["Size"].HeaderText = languageManager.GetString("CookieManager_Size", "大小");
            }
            
            // 右键菜单
            if (dataGridViewCookies.ContextMenuStrip != null)
            {
                dataGridViewCookies.ContextMenuStrip.Items[0].Text = languageManager.GetString("CookieManager_Copy", "复制");
                dataGridViewCookies.ContextMenuStrip.Items[1].Text = languageManager.GetString("CookieManager_Edit", "编辑");
                dataGridViewCookies.ContextMenuStrip.Items[3].Text = languageManager.GetString("CookieManager_Delete", "删除");
            }
        }

        private void UpdateCookieData(List<CookieItem> allCookies)
        {
            try
            {
                logManager.Info($"[UpdateCookieData] 开始更新，cookies数量: {allCookies?.Count ?? 0}");
                
                // 记录DataGridView当前状态
                logManager.Debug($"[UpdateCookieData] DataGridView状态:");
                logManager.Debug($"  - Visible: {dataGridViewCookies.Visible}");
                logManager.Debug($"  - Enabled: {dataGridViewCookies.Enabled}");
                logManager.Debug($"  - Size: {dataGridViewCookies.Size}");
                logManager.Debug($"  - Columns.Count: {dataGridViewCookies.Columns.Count}");
                logManager.Debug($"  - Rows.Count (更新前): {dataGridViewCookies.Rows.Count}");
                logManager.Debug($"  - DataSource: {dataGridViewCookies.DataSource?.GetType().Name ?? "null"}");
                logManager.Debug($"  - AutoGenerateColumns: {dataGridViewCookies.AutoGenerateColumns}");
                
                // 记录列信息
                for (int i = 0; i < dataGridViewCookies.Columns.Count; i++)
                {
                    var col = dataGridViewCookies.Columns[i];
                    logManager.Debug($"  - Column[{i}]: Name={col.Name}, DataPropertyName={col.DataPropertyName}, Visible={col.Visible}, Width={col.Width}");
                }
                
                // 清空并重新添加数据
                cookieList.Clear();
                if (allCookies != null && allCookies.Count > 0)
                {
                    cookieList.AddRange(allCookies);
                    logManager.Info($"[UpdateCookieData] 已将 {allCookies.Count} 个cookies添加到cookieList");
                    
                    // 记录前几个cookie的详细信息
                    for (int i = 0; i < Math.Min(3, allCookies.Count); i++)
                    {
                        var cookie = allCookies[i];
                        logManager.Debug($"  Cookie[{i}]: Domain={cookie.Domain}, Name={cookie.Name}, Value={cookie.Value?.Substring(0, Math.Min(20, cookie.Value?.Length ?? 0))}...");
                    }
                }
                
                // 方法1: 直接操作DataGridView.Rows（绕过数据绑定）
                logManager.Info("[UpdateCookieData] 尝试直接添加行到DataGridView");
                dataGridViewCookies.Rows.Clear();
                
                foreach (var cookie in cookieList)
                {
                    var rowIndex = dataGridViewCookies.Rows.Add();
                    var row = dataGridViewCookies.Rows[rowIndex];
                    
                    // 手动设置每个单元格的值
                    row.Cells["Domain"].Value = cookie.Domain;
                    row.Cells["Name"].Value = cookie.Name;
                    row.Cells["Value"].Value = cookie.Value;
                    row.Cells["Path"].Value = cookie.Path;
                    row.Cells["Expires"].Value = cookie.ExpiresDisplay;
                    row.Cells["Size"].Value = cookie.Size;
                    row.Cells["HttpOnly"].Value = cookie.HttpOnly;
                    row.Cells["Secure"].Value = cookie.Secure;
                    row.Cells["SameSite"].Value = cookie.SameSite;
                    
                    // 将CookieItem对象关联到行
                    row.Tag = cookie;
                }
                
                logManager.Info($"[UpdateCookieData] 直接添加了 {dataGridViewCookies.Rows.Count} 行到DataGridView");
                
                // 强制刷新
                dataGridViewCookies.Refresh();
                dataGridViewCookies.Invalidate();
                dataGridViewCookies.Update();
                Application.DoEvents(); // 强制处理所有待处理的UI事件
                
                // 记录更新后的状态
                logManager.Debug($"[UpdateCookieData] DataGridView状态(更新后):");
                logManager.Debug($"  - Rows.Count: {dataGridViewCookies.Rows.Count}");
                logManager.Debug($"  - 第一行是否可见: {(dataGridViewCookies.Rows.Count > 0 ? dataGridViewCookies.Rows[0].Visible.ToString() : "无行")}");
                
                // 更新状态栏
                UpdateStatusBar();
                
                logManager.Info($"[UpdateCookieData] 完成，成功显示 {dataGridViewCookies.Rows.Count} 个cookies");
            }
            catch (Exception ex)
            {
                logManager.Error($"[UpdateCookieData] 更新失败: {ex.Message}", ex);
                logManager.Error($"[UpdateCookieData] 堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        private void RefreshDisplay()
        {
            try
            {
                logManager.Debug($"[RefreshDisplay] 开始刷新，cookieList.Count={cookieList.Count}, searchText='{currentSearchText}', groupByDomain={groupByDomain}");
                
                var filteredCookies = cookieList.AsEnumerable();
                
                // 应用搜索过滤
                if (!string.IsNullOrWhiteSpace(currentSearchText))
                {
                    var searchLower = currentSearchText.ToLower();
                    filteredCookies = filteredCookies.Where(c =>
                        c.Domain.ToLower().Contains(searchLower) ||
                        c.Name.ToLower().Contains(searchLower) ||
                        c.Value.ToLower().Contains(searchLower));
                }
                
                // 应用分组（如果需要）
                if (groupByDomain)
                {
                    filteredCookies = filteredCookies.OrderBy(c => c.Domain).ThenBy(c => c.Name);
                }
                
                // 直接操作DataGridView的行
                dataGridViewCookies.Rows.Clear();
                
                foreach (var cookie in filteredCookies)
                {
                    var rowIndex = dataGridViewCookies.Rows.Add();
                    var row = dataGridViewCookies.Rows[rowIndex];
                    
                    row.Cells["Domain"].Value = cookie.Domain;
                    row.Cells["Name"].Value = cookie.Name;
                    row.Cells["Value"].Value = cookie.Value;
                    row.Cells["Path"].Value = cookie.Path;
                    row.Cells["Expires"].Value = cookie.ExpiresDisplay;
                    row.Cells["Size"].Value = cookie.Size;
                    row.Cells["HttpOnly"].Value = cookie.HttpOnly;
                    row.Cells["Secure"].Value = cookie.Secure;
                    row.Cells["SameSite"].Value = cookie.SameSite;
                    
                    row.Tag = cookie;
                }
                
                logManager.Debug($"[RefreshDisplay] 完成刷新，显示 {dataGridViewCookies.Rows.Count} 行");
            }
            catch (Exception ex)
            {
                logManager.Error($"[RefreshDisplay] 刷新失败: {ex.Message}", ex);
            }
        }

        private void UpdateStatusBar()
        {
            int totalCount = cookieList.Count;
            int displayedCount = displayedCookies.Count;
            int selectedCount = dataGridViewCookies.SelectedRows.Count;
            
            // 添加调试日志
            logManager.Debug($"[UpdateStatusBar] totalCount={totalCount}, displayedCount={displayedCount}, selectedCount={selectedCount}");
            
            try
            {
                // 修改调用方式，传递默认格式作为fallback
                string statusFormat = languageManager.GetString("CookieManager_StatusFormat");
                logManager.Debug($"[UpdateStatusBar] 获取到的格式字符串: '{statusFormat}'");
                
                // 使用带参数的GetString重载
                string statusText = languageManager.GetString("CookieManager_StatusFormat", totalCount, displayedCount, selectedCount);
                logManager.Debug($"[UpdateStatusBar] 格式化后的状态文本: '{statusText}'");
                
                toolStripStatusLabel.Text = statusText;
            }
            catch (Exception ex)
            {
                logManager.Error($"[UpdateStatusBar] 更新状态栏失败: {ex.Message}", ex);
                // 使用硬编码的后备文本
                toolStripStatusLabel.Text = $"总计: {totalCount} | 显示: {displayedCount} | 选中: {selectedCount}";
            }
        }
        
        // 加载真实的WebView2 cookies
        private async Task LoadCookiesAsync()
        {
            try
            {
                logManager.Info("LoadCookiesAsync开始执行");
                
                if (webView2 == null)
                {
                    logManager.Warning("WebView2实例为null，无法加载cookies");
                    return;
                }
                
                if (webView2.CookieManager == null)
                {
                    logManager.Warning("WebView2.CookieManager为null，无法加载cookies");
                    return;
                }
                
                if (cookieManager == null)
                {
                    logManager.Warning("CookieManager实例为null，无法加载cookies");
                    return;
                }
                
                logManager.Info($"WebView2实例状态: webView2={webView2 != null}, CookieManager={webView2.CookieManager != null}");
                logManager.Info($"CookieManager实例状态: {cookieManager != null}");
                
                // 显示加载进度 - 在UI线程上更新
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        toolStripStatusLabel.Text = languageManager.GetString("CookieManager_Loading", "Loading Cookies...");
                    }));
                }
                else
                {
                    toolStripStatusLabel.Text = languageManager.GetString("CookieManager_Loading", "Loading Cookies...");
                }
                
                logManager.Info("开始调用cookieManager.GetAllCookiesAsync()");
                var allCookies = await cookieManager.GetAllCookiesAsync();
                logManager.Info($"GetAllCookiesAsync返回，cookies数量: {allCookies?.Count ?? -1}");
                
                if (allCookies == null)
                {
                    logManager.Error("GetAllCookiesAsync返回null");
                    allCookies = new List<CookieItem>();
                }
                
                // 在UI线程上更新数据
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        UpdateCookieData(allCookies);
                    }));
                }
                else
                {
                    UpdateCookieData(allCookies);
                }
                
                logManager.Info($"LoadCookiesAsync完成，成功加载 {allCookies.Count} 个cookies");
            }
            catch (Exception ex)
            {
                logManager.Error($"LoadCookiesAsync失败: {ex.GetType().Name}: {ex.Message}", ex);
                logManager.Error($"堆栈跟踪: {ex.StackTrace}");
                
                // 在UI线程上显示错误消息
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show(
                            string.Format(languageManager.GetString("CookieManager_LoadError", "Failed to load cookies: {0}"), ex.Message),
                            languageManager.GetString("CookieManager_Error", "Error"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                else
                {
                    MessageBox.Show(
                        string.Format(languageManager.GetString("CookieManager_LoadError", "Failed to load cookies: {0}"), ex.Message),
                        languageManager.GetString("CookieManager_Error", "Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 事件处理程序
        private async void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            logManager.Info("刷新Cookie列表");
            
            if (webView2 != null)
            {
                await LoadCookiesAsync();
            }
            else
            {
                RefreshDisplay();
                UpdateStatusBar();
            }
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new CookieEditDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    cookieList.Add(dialog.Cookie);
                    RefreshDisplay();
                    UpdateStatusBar();
                    logManager.Info($"添加Cookie: {dialog.Cookie.Name}@{dialog.Cookie.Domain}");
                }
            }
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCookies.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    languageManager.GetString("CookieManager_SelectToDelete", "请选择要删除的Cookie"),
                    languageManager.GetString("CookieManager_Hint", "提示"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var result = MessageBox.Show(
                string.Format(languageManager.GetString("CookieManager_ConfirmDelete", "确定要删除选中的 {0} 个Cookie吗？"), 
                    dataGridViewCookies.SelectedRows.Count),
                languageManager.GetString("CookieManager_Confirm", "确认"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                var selectedCookies = new List<CookieItem>();
                foreach (DataGridViewRow row in dataGridViewCookies.SelectedRows)
                {
                    // 从Tag属性获取CookieItem对象
                    var cookie = row.Tag as CookieItem;
                    if (cookie != null)
                    {
                        selectedCookies.Add(cookie);
                    }
                }
                
                foreach (var cookie in selectedCookies)
                {
                    cookieList.Remove(cookie);
                    logManager.Info($"删除Cookie: {cookie.Name}@{cookie.Domain}");
                }
                
                RefreshDisplay();
                UpdateStatusBar();
            }
        }

        private void toolStripButtonImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                openFileDialog.Title = languageManager.GetString("CookieManager_ImportTitle", "导入Cookie");
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // TODO: 实现导入逻辑
                        MessageBox.Show(
                            languageManager.GetString("CookieManager_ImportSuccess", "Cookie导入成功"),
                            languageManager.GetString("CookieManager_Success", "成功"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        logManager.Info($"导入Cookie文件: {openFileDialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            string.Format(languageManager.GetString("CookieManager_ImportError", "导入失败: {0}"), ex.Message),
                            languageManager.GetString("CookieManager_Error", "错误"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logManager.Error($"导入Cookie失败: {ex.Message}", ex);
                    }
                }
            }
        }

        private void toolStripButtonExport_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                saveFileDialog.Title = languageManager.GetString("CookieManager_ExportTitle", "导出Cookie");
                saveFileDialog.FileName = $"cookies_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // TODO: 实现导出逻辑
                        MessageBox.Show(
                            languageManager.GetString("CookieManager_ExportSuccess", "Cookie导出成功"),
                            languageManager.GetString("CookieManager_Success", "成功"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        logManager.Info($"导出Cookie文件: {saveFileDialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            string.Format(languageManager.GetString("CookieManager_ExportError", "导出失败: {0}"), ex.Message),
                            languageManager.GetString("CookieManager_Error", "错误"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logManager.Error($"导出Cookie失败: {ex.Message}", ex);
                    }
                }
            }
        }

        private void toolStripButtonGroupByDomain_Click(object sender, EventArgs e)
        {
            groupByDomain = !groupByDomain;
            toolStripButtonGroupByDomain.Checked = groupByDomain;
            RefreshDisplay();
            logManager.Info($"按域名分组: {(groupByDomain ? "开启" : "关闭")}");
        }

        private void toolStripTextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            currentSearchText = toolStripTextBoxSearch.Text;
            RefreshDisplay();
            UpdateStatusBar();
        }

        private void dataGridViewCookies_SelectionChanged(object sender, EventArgs e)
        {
            UpdateStatusBar();
        }

        private void dataGridViewCookies_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedCookie();
            }
        }

        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewCookies.SelectedRows.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (DataGridViewRow row in dataGridViewCookies.SelectedRows)
                {
                    // 从Tag属性获取CookieItem对象
                    var cookie = row.Tag as CookieItem;
                    if (cookie != null)
                    {
                        sb.AppendLine($"{cookie.Name}={cookie.Value}");
                    }
                }
                
                if (sb.Length > 0)
                {
                    Clipboard.SetText(sb.ToString());
                    logManager.Info($"复制了 {dataGridViewCookies.SelectedRows.Count} 个Cookie到剪贴板");
                }
            }
        }

        private void EditMenuItem_Click(object sender, EventArgs e)
        {
            EditSelectedCookie();
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButtonDelete_Click(sender, e);
        }

        private void EditSelectedCookie()
        {
            if (dataGridViewCookies.SelectedRows.Count == 1)
            {
                var selectedRow = dataGridViewCookies.SelectedRows[0];
                // 从Tag属性获取CookieItem对象
                var cookie = selectedRow.Tag as CookieItem;
                if (cookie != null)
                {
                    using (var dialog = new CookieEditDialog(cookie))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // 更新Cookie
                            var index = cookieList.IndexOf(cookie);
                            if (index >= 0)
                            {
                                cookieList[index] = dialog.Cookie;
                                RefreshDisplay();
                                UpdateStatusBar();
                                logManager.Info($"编辑Cookie: {dialog.Cookie.Name}@{dialog.Cookie.Domain}");
                            }
                        }
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // 响应式调整列宽
            if (dataGridViewCookies.Columns.Count > 0 && this.Width > 0)
            {
                int totalWidth = dataGridViewCookies.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                int fixedColumnsWidth = 60 + 70 + 60 + 80; // Size, HttpOnly, Secure, SameSite
                int remainingWidth = totalWidth - fixedColumnsWidth;
                
                if (remainingWidth > 0)
                {
                    dataGridViewCookies.Columns["Domain"].Width = (int)(remainingWidth * 0.2);
                    dataGridViewCookies.Columns["Name"].Width = (int)(remainingWidth * 0.15);
                    dataGridViewCookies.Columns["Value"].Width = (int)(remainingWidth * 0.3);
                    dataGridViewCookies.Columns["Path"].Width = (int)(remainingWidth * 0.15);
                    dataGridViewCookies.Columns["Expires"].Width = (int)(remainingWidth * 0.2);
                }
            }
        }
    }

    // Cookie数据模型 - 增强版，支持WebView2操作
    public class CookieItem
    {
        public string Domain { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
        public DateTime? Expires { get; set; }
        public bool HttpOnly { get; set; }
        public bool Secure { get; set; }
        public string SameSite { get; set; }
        
        [JsonIgnore]
        public string ExpiresDisplay
        {
            get
            {
                if (Expires.HasValue)
                {
                    if (Expires.Value < DateTime.Now)
                        return "Expired";
                    return Expires.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "Session";
            }
        }
        
        [JsonIgnore]
        public int Size
        {
            get
            {
                return (Name?.Length ?? 0) + (Value?.Length ?? 0);
            }
        }
        
        public CookieItem()
        {
            Path = "/";
            SameSite = "Lax";
        }
        
        /// <summary>
        /// 从CoreWebView2Cookie创建CookieItem
        /// </summary>
        public static CookieItem FromWebView2Cookie(CoreWebView2Cookie webViewCookie)
        {
            if (webViewCookie == null)
                throw new ArgumentNullException(nameof(webViewCookie));

            var sameSiteString = webViewCookie.SameSite switch
            {
                CoreWebView2CookieSameSiteKind.None => "None",
                CoreWebView2CookieSameSiteKind.Lax => "Lax",
                CoreWebView2CookieSameSiteKind.Strict => "Strict",
                _ => "Lax"
            };

            return new CookieItem
            {
                Name = webViewCookie.Name,
                Value = webViewCookie.Value,
                Domain = webViewCookie.Domain,
                Path = webViewCookie.Path,
                Expires = webViewCookie.Expires,
                HttpOnly = webViewCookie.IsHttpOnly,
                Secure = webViewCookie.IsSecure,
                SameSite = sameSiteString
            };
        }
        
        /// <summary>
        /// 获取CoreWebView2CookieSameSiteKind枚举值
        /// </summary>
        public CoreWebView2CookieSameSiteKind GetSameSiteKind()
        {
            return SameSite?.ToLower() switch
            {
                "none" => CoreWebView2CookieSameSiteKind.None,
                "strict" => CoreWebView2CookieSameSiteKind.Strict,
                _ => CoreWebView2CookieSameSiteKind.Lax
            };
        }
        
        /// <summary>
        /// 验证Cookie数据的有效性
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(Name))
            {
                errorMessage = "Cookie名称不能为空";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Domain))
            {
                errorMessage = "Cookie域名不能为空";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Path))
            {
                errorMessage = "Cookie路径不能为空";
                return false;
            }

            // 验证域名格式
            if (!IsValidDomain(Domain))
            {
                errorMessage = "无效的域名格式";
                return false;
            }

            // 验证路径格式
            if (!Path.StartsWith("/"))
            {
                errorMessage = "路径必须以'/'开头";
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// 验证域名格式
        /// </summary>
        private bool IsValidDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            // 移除开头的点（用于子域名匹配）
            if (domain.StartsWith("."))
                domain = domain.Substring(1);

            // 简单的域名验证
            var parts = domain.Split('.');
            if (parts.Length < 1)
                return false;

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    return false;
                
                // 检查是否包含无效字符
                foreach (char c in part)
                {
                    if (!char.IsLetterOrDigit(c) && c != '-')
                        return false;
                }
            }

            return true;
        }
        
        public CookieItem Clone()
        {
            return new CookieItem
            {
                Domain = this.Domain,
                Name = this.Name,
                Value = this.Value,
                Path = this.Path,
                Expires = this.Expires,
                HttpOnly = this.HttpOnly,
                Secure = this.Secure,
                SameSite = this.SameSite
            };
        }
    }
}