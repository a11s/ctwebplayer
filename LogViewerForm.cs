using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

namespace ctwebplayer
{
    /// <summary>
    /// 日志查看器窗体
    /// </summary>
    public partial class LogViewerForm : Form
    {
        private TextBox txtLogContent;
        private ComboBox cmbLogLevel;
        private CheckBox chkAutoRefresh;
        private Button btnRefresh;
        private Button btnClear;
        private Button btnClose;
        private Label lblLogLevel;
        private Label lblFileInfo;
        private System.Timers.Timer refreshTimer;
        private long lastFilePosition = 0;
        private LogLevel selectedLogLevel = LogLevel.Debug;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogViewerForm()
        {
            InitializeComponent();
            LoadLogContent();
            SetupRefreshTimer();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "日志查看器";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);

            // 创建顶部面板
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10, 10, 10, 5)
            };

            // 日志级别标签
            lblLogLevel = new Label
            {
                Text = "日志级别过滤:",
                Location = new Point(10, 15),
                Size = new Size(90, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            topPanel.Controls.Add(lblLogLevel);

            // 日志级别下拉框
            cmbLogLevel = new ComboBox
            {
                Location = new Point(105, 12),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLogLevel.Items.AddRange(new object[] { "Debug", "Info", "Warning", "Error" });
            cmbLogLevel.SelectedIndex = 0;
            cmbLogLevel.SelectedIndexChanged += CmbLogLevel_SelectedIndexChanged;
            topPanel.Controls.Add(cmbLogLevel);

            // 自动刷新复选框
            chkAutoRefresh = new CheckBox
            {
                Text = "自动刷新",
                Location = new Point(220, 15),
                Size = new Size(80, 25),
                Checked = true
            };
            chkAutoRefresh.CheckedChanged += ChkAutoRefresh_CheckedChanged;
            topPanel.Controls.Add(chkAutoRefresh);

            // 刷新按钮
            btnRefresh = new Button
            {
                Text = "刷新",
                Location = new Point(310, 10),
                Size = new Size(70, 28)
            };
            btnRefresh.Click += BtnRefresh_Click;
            topPanel.Controls.Add(btnRefresh);

            // 清空日志按钮
            btnClear = new Button
            {
                Text = "清空日志",
                Location = new Point(390, 10),
                Size = new Size(80, 28)
            };
            btnClear.Click += BtnClear_Click;
            topPanel.Controls.Add(btnClear);

            // 文件信息标签
            lblFileInfo = new Label
            {
                Text = "",
                Location = new Point(480, 15),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray
            };
            topPanel.Controls.Add(lblFileInfo);

            // 创建日志内容文本框
            txtLogContent = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Dock = DockStyle.Fill
            };

            // 创建底部面板
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(10, 5, 10, 10)
            };

            // 关闭按钮
            btnClose = new Button
            {
                Text = "关闭",
                Size = new Size(80, 28),
                Dock = DockStyle.Right,
                DialogResult = DialogResult.OK
            };
            bottomPanel.Controls.Add(btnClose);

            // 添加控件到窗体
            this.Controls.Add(txtLogContent);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        /// <summary>
        /// 设置刷新定时器
        /// </summary>
        private void SetupRefreshTimer()
        {
            refreshTimer = new System.Timers.Timer(1000); // 每秒刷新一次
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.AutoReset = true;
            refreshTimer.Enabled = chkAutoRefresh.Checked;
        }

        /// <summary>
        /// 定时器触发事件
        /// </summary>
        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LoadNewLogContent()));
            }
            else
            {
                LoadNewLogContent();
            }
        }

        /// <summary>
        /// 加载日志内容
        /// </summary>
        private void LoadLogContent()
        {
            try
            {
                var logFilePath = LogManager.Instance.GetLogFilePath();
                if (!File.Exists(logFilePath))
                {
                    txtLogContent.Text = "日志文件不存在。";
                    UpdateFileInfo(0);
                    return;
                }

                // 读取整个文件
                var allLines = File.ReadAllLines(logFilePath);
                var filteredLines = FilterLogLines(allLines);
                
                txtLogContent.Text = string.Join(Environment.NewLine, filteredLines);
                
                // 滚动到底部
                txtLogContent.SelectionStart = txtLogContent.Text.Length;
                txtLogContent.ScrollToCaret();

                // 更新文件位置
                var fileInfo = new FileInfo(logFilePath);
                lastFilePosition = fileInfo.Length;
                UpdateFileInfo(fileInfo.Length);
            }
            catch (Exception ex)
            {
                txtLogContent.Text = $"读取日志文件时出错：{ex.Message}";
            }
        }

        /// <summary>
        /// 加载新的日志内容（增量加载）
        /// </summary>
        private void LoadNewLogContent()
        {
            try
            {
                var logFilePath = LogManager.Instance.GetLogFilePath();
                if (!File.Exists(logFilePath))
                {
                    return;
                }

                var fileInfo = new FileInfo(logFilePath);
                if (fileInfo.Length <= lastFilePosition)
                {
                    return; // 没有新内容
                }

                // 读取新内容
                using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek(lastFilePosition, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        var newContent = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(newContent))
                        {
                            var newLines = newContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                            var filteredLines = FilterLogLines(newLines);
                            
                            if (filteredLines.Any())
                            {
                                // 追加新内容
                                if (txtLogContent.Text.Length > 0)
                                {
                                    txtLogContent.AppendText(Environment.NewLine);
                                }
                                txtLogContent.AppendText(string.Join(Environment.NewLine, filteredLines));
                                
                                // 滚动到底部
                                txtLogContent.SelectionStart = txtLogContent.Text.Length;
                                txtLogContent.ScrollToCaret();
                            }
                        }
                    }
                }

                lastFilePosition = fileInfo.Length;
                UpdateFileInfo(fileInfo.Length);
            }
            catch (Exception ex)
            {
                // 忽略读取错误，下次重试
                System.Diagnostics.Debug.WriteLine($"LoadNewLogContent error: {ex.Message}");
            }
        }

        /// <summary>
        /// 过滤日志行
        /// </summary>
        private string[] FilterLogLines(string[] lines)
        {
            return lines.Where(line => ShouldShowLogLine(line)).ToArray();
        }

        /// <summary>
        /// 判断是否应该显示日志行
        /// </summary>
        private bool ShouldShowLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            // 检查日志级别
            if (line.Contains("[Debug]") && selectedLogLevel > LogLevel.Debug)
                return false;
            if (line.Contains("[Info]") && selectedLogLevel > LogLevel.Info)
                return false;
            if (line.Contains("[Warning]") && selectedLogLevel > LogLevel.Warning)
                return false;
            
            // Error级别总是显示，或者是堆栈跟踪等续行
            return true;
        }

        /// <summary>
        /// 更新文件信息
        /// </summary>
        private void UpdateFileInfo(long fileSize)
        {
            var sizeText = FormatFileSize(fileSize);
            lblFileInfo.Text = $"文件大小: {sizeText}";
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 日志级别选择改变事件
        /// </summary>
        private void CmbLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedLogLevel = (LogLevel)cmbLogLevel.SelectedIndex;
            LoadLogContent();
        }

        /// <summary>
        /// 自动刷新复选框改变事件
        /// </summary>
        private void ChkAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            refreshTimer.Enabled = chkAutoRefresh.Checked;
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadLogContent();
        }

        /// <summary>
        /// 清空日志按钮点击事件
        /// </summary>
        private async void BtnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定要清空日志文件吗？\n\n此操作不可恢复。",
                "确认清空", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    refreshTimer.Enabled = false;
                    await LogManager.Instance.ClearLogFileAsync();
                    txtLogContent.Clear();
                    lastFilePosition = 0;
                    UpdateFileInfo(0);
                    MessageBox.Show("日志已清空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清空日志时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    refreshTimer.Enabled = chkAutoRefresh.Checked;
                }
            }
        }

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }
    }
}