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
    /// 日志查看器窗�?
    /// </summary>
    public partial class LogViewerForm : Form
    {
        private System.Timers.Timer refreshTimer = null!; // �?SetupRefreshTimer 中初始化
        private long lastFilePosition = 0;
        private LogLevel selectedLogLevel = LogLevel.Debug;

        /// <summary>
        /// 构造函�?
        /// </summary>
        public LogViewerForm()
        {
            InitializeComponent();
            LoadLogContent();
            SetupRefreshTimer();
        }

        /// <summary>
        /// 设置刷新定时�?
        /// </summary>
        private void SetupRefreshTimer()
        {
            refreshTimer = new System.Timers.Timer(1000); // 每秒刷新一�?
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.AutoReset = true;
            refreshTimer.Enabled = chkAutoRefresh.Checked;
        }

        /// <summary>
        /// 定时器触发事�?
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
                    txtLogContent.Text = "日志文件不存在�?;
                    UpdateFileInfo(0);
                    return;
                }

                // 读取整个文件
                var allLines = File.ReadAllLines(logFilePath);
                var filteredLines = FilterLogLines(allLines);
                
                txtLogContent.Text = string.Join(Environment.NewLine, filteredLines);
                
                // 滚动到底�?
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
                    return; // 没有新内�?
                }

                // 读取新内�?
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
                                // 追加新内�?
                                if (txtLogContent.Text.Length > 0)
                                {
                                    txtLogContent.AppendText(Environment.NewLine);
                                }
                                txtLogContent.AppendText(string.Join(Environment.NewLine, filteredLines));
                                
                                // 滚动到底�?
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
                // 忽略读取错误，下次重�?
                System.Diagnostics.Debug.WriteLine($"LoadNewLogContent error: {ex.Message}");
            }
        }

        /// <summary>
        /// 过滤日志�?
        /// </summary>
        private string[] FilterLogLines(string[] lines)
        {
            return lines.Where(line => ShouldShowLogLine(line)).ToArray();
        }

        /// <summary>
        /// 判断是否应该显示日志�?
        /// </summary>
        private bool ShouldShowLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            // 检查日志级�?
            if (line.Contains("[Debug]") && selectedLogLevel > LogLevel.Debug)
                return false;
            if (line.Contains("[Info]") && selectedLogLevel > LogLevel.Info)
                return false;
            if (line.Contains("[Warning]") && selectedLogLevel > LogLevel.Warning)
                return false;
            
            // Error级别总是显示，或者是堆栈跟踪等续�?
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
        /// 格式化文件大�?
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
            var result = MessageBox.Show("确定要清空日志文件吗？\n\n此操作不可恢复�?,
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
                    MessageBox.Show("日志已清空�?, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// 窗体关闭时清理资�?
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }
    }
}
