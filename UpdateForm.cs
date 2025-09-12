using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ctwebplayer
{
    /// <summary>
    /// 更新窗体，显示更新信息和下载进度
    /// </summary>
    public partial class UpdateForm : Form
    {
        private readonly UpdateManager _updateManager;
        private readonly CTWebPlayer.UpdateInfo _updateInfo;
        private bool _isDownloading = false;
        private string _downloadedFilePath = null;

        public UpdateForm(CTWebPlayer.UpdateInfo updateInfo)
        {
            InitializeComponent();
            
            _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
            _updateManager = new UpdateManager();
            
            // 订阅下载进度事件
            _updateManager.DownloadProgressChanged += OnDownloadProgressChanged;
            
            // 初始化界面
            InitializeUI();
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitializeUI()
        {
            // 显示版本信息
            lblCurrentVersion.Text = $"当前版本: {CTWebPlayer.Version.FullVersion}";
            lblNewVersion.Text = $"最新版本: {_updateInfo.Version}";
            lblReleaseDate.Text = $"发布时间: {_updateInfo.PublishedAt:yyyy-MM-dd HH:mm}";
            
            // 显示文件信息
            lblFileName.Text = $"文件名: {_updateInfo.FileName}";
            lblFileSize.Text = $"文件大小: {_updateInfo.GetFormattedFileSize()}";
            
            // 显示更新说明
            txtReleaseNotes.Text = _updateInfo.ReleaseNotes;
            
            // 初始化进度条
            progressBar.Value = 0;
            lblProgress.Text = "准备下载...";
            
            // 根据是否为强制更新设置按钮
            if (_updateInfo.IsUpdateMandatory())
            {
                btnSkip.Visible = false;
                btnClose.Visible = false;
                lblMandatory.Text = "这是一个强制更新，必须安装后才能继续使用。";
                lblMandatory.Visible = true;
            }
            else
            {
                btnSkip.Visible = true;
                btnClose.Visible = false;
                lblMandatory.Visible = false;
            }
            
            // 设置按钮状态
            btnDownload.Enabled = true;
            btnInstall.Enabled = false;
        }

        /// <summary>
        /// 下载按钮点击事件
        /// </summary>
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (_isDownloading)
            {
                // 取消下载
                _updateManager.CancelDownload();
                _isDownloading = false;
                btnDownload.Text = "下载更新";
                lblProgress.Text = "下载已取消";
                progressBar.Value = 0;
                return;
            }

            try
            {
                _isDownloading = true;
                btnDownload.Text = "取消下载";
                btnSkip.Enabled = false;
                btnInstall.Enabled = false;
                
                lblProgress.Text = "正在下载...";
                progressBar.Value = 0;

                // 异步下载更新
                _downloadedFilePath = await _updateManager.DownloadUpdateAsync(_updateInfo);
                
                // 下载完成
                _isDownloading = false;
                btnDownload.Visible = false;
                btnInstall.Enabled = true;
                btnInstall.Visible = true;
                lblProgress.Text = "下载完成！";
                progressBar.Value = 100;
                
                // 显示提示
                MessageBox.Show(
                    "更新下载完成！\n\n点击\"安装更新\"按钮将重启程序并应用更新。",
                    "下载完成",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                _isDownloading = false;
                btnDownload.Text = "下载更新";
                btnDownload.Enabled = true;
                btnSkip.Enabled = true;
                progressBar.Value = 0;
                
                lblProgress.Text = "下载失败";
                
                MessageBox.Show(
                    $"下载更新失败：\n{ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// 安装按钮点击事件
        /// </summary>
        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_downloadedFilePath))
            {
                MessageBox.Show(
                    "没有找到下载的更新文件。",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            var result = MessageBox.Show(
                "安装更新将关闭程序并重新启动。\n\n是否继续？",
                "确认安装",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 应用更新（将重启程序）
                    _updateManager.ApplyUpdate(_downloadedFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"安装更新失败：\n{ex.Message}",
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        /// <summary>
        /// 跳过按钮点击事件
        /// </summary>
        private void btnSkip_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要跳过此更新吗？\n\n您可以稍后通过菜单再次检查更新。",
                "跳过更新",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// 下载进度变化事件处理
        /// </summary>
        private void OnDownloadProgressChanged(object sender, DownloadProgressEventArgs e)
        {
            // 在UI线程上更新进度
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnDownloadProgressChanged(sender, e)));
                return;
            }

            progressBar.Value = e.ProgressPercentage;
            
            // 计算下载速度和剩余时间（简化版）
            var downloaded = FormatBytes(e.BytesReceived);
            var total = FormatBytes(e.TotalBytesToReceive);
            
            lblProgress.Text = $"正在下载... {downloaded} / {total} ({e.ProgressPercentage}%)";
        }

        /// <summary>
        /// 格式化字节数
        /// </summary>
        private string FormatBytes(long bytes)
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
        /// 窗体关闭事件
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isDownloading)
            {
                var result = MessageBox.Show(
                    "正在下载更新，确定要取消吗？",
                    "确认取消",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _updateManager.CancelDownload();
            }

            base.OnFormClosing(e);
        }

    }
}