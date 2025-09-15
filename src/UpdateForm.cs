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
        private string? _downloadedFilePath = null;

        public UpdateForm(CTWebPlayer.UpdateInfo updateInfo)
        {
            InitializeComponent();
            LanguageManager.Instance.ApplyToForm(this);
            
            _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
            _updateManager = new UpdateManager();
            
            // 订阅下载进度事件
            _updateManager.DownloadProgressChanged += OnDownloadProgressChanged;
            
            // 初始化界面
            InitializeUI();
            LanguageManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitializeUI()
        {
            // 显示版本信息
            lblCurrentVersion.Text = LanguageManager.Instance.GetString("UpdateForm_grpVersionInfo_lblCurrentVersion") + CTWebPlayer.Version.FullVersion;
            lblNewVersion.Text = LanguageManager.Instance.GetString("UpdateForm_grpVersionInfo_lblNewVersion") + _updateInfo.Version;
            lblReleaseDate.Text = LanguageManager.Instance.GetString("UpdateForm_grpVersionInfo_lblReleaseDate") + _updateInfo.PublishedAt.ToString("yyyy-MM-dd HH:mm");
            
            // 显示文件信息
            lblFileName.Text = LanguageManager.Instance.GetString("UpdateForm_grpFileInfo_lblFileName") + _updateInfo.FileName;
            lblFileSize.Text = LanguageManager.Instance.GetString("UpdateForm_grpFileInfo_lblFileSize") + _updateInfo.GetFormattedFileSize();
            
            // 显示更新说明
            txtReleaseNotes.Text = _updateInfo.ReleaseNotes;
            
            // 初始化进度条
            progressBar.Value = 0;
            lblProgress.Text = LanguageManager.Instance.GetString("UpdateForm_lblProgress_Ready");
            
            // 根据是否为强制更新设置按钮
            if (_updateInfo.IsUpdateMandatory())
            {
                btnSkip.Visible = false;
                btnClose.Visible = false;
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
                btnDownload.Text = LanguageManager.Instance.GetString("UpdateForm_btnDownload");
                lblProgress.Text = LanguageManager.Instance.GetString("UpdateForm_lblProgress_Cancelled");
                progressBar.Value = 0;
                return;
            }

            try
            {
                _isDownloading = true;
                btnDownload.Text = LanguageManager.Instance.GetString("UpdateForm_btnDownload_Cancel");
                btnSkip.Enabled = false;
                btnInstall.Enabled = false;
                
                lblProgress.Text = LanguageManager.Instance.GetString("UpdateForm_lblProgress_Downloading");
                progressBar.Value = 0;

                // 异步下载更新
                _downloadedFilePath = await _updateManager.DownloadUpdateAsync(_updateInfo);
                
                // 下载完成
                _isDownloading = false;
                btnDownload.Visible = false;
                btnInstall.Enabled = true;
                btnInstall.Visible = true;
                lblProgress.Text = LanguageManager.Instance.GetString("UpdateForm_lblProgress_Complete");
                progressBar.Value = 100;
                
                // 显示提示
                MessageBox.Show(
                    LanguageManager.Instance.GetString("UpdateForm_Msg_DownloadComplete"),
                    LanguageManager.Instance.GetString("UpdateForm_Msg_DownloadCompleteTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                _isDownloading = false;
                btnDownload.Text = LanguageManager.Instance.GetString("UpdateForm_btnDownload");
                btnDownload.Enabled = true;
                btnSkip.Enabled = true;
                progressBar.Value = 0;
                
                lblProgress.Text = LanguageManager.Instance.GetString("UpdateForm_lblProgress_Failed");
                
                MessageBox.Show(
                    string.Format(LanguageManager.Instance.GetString("UpdateForm_Msg_DownloadError"), ex.Message),
                    LanguageManager.Instance.GetString("UpdateForm_Msg_ErrorTitle"),
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
                    LanguageManager.Instance.GetString("UpdateForm_Msg_NoFile"),
                    LanguageManager.Instance.GetString("UpdateForm_Msg_ErrorTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            var result = MessageBox.Show(
                LanguageManager.Instance.GetString("UpdateForm_Msg_ConfirmInstall"),
                LanguageManager.Instance.GetString("UpdateForm_Msg_ConfirmInstallTitle"),
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
                        string.Format(LanguageManager.Instance.GetString("UpdateForm_Msg_InstallError"), ex.Message),
                        LanguageManager.Instance.GetString("UpdateForm_Msg_ErrorTitle"),
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
                LanguageManager.Instance.GetString("UpdateForm_Msg_ConfirmSkip"),
                LanguageManager.Instance.GetString("UpdateForm_Msg_ConfirmSkipTitle"),
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
            
            lblProgress.Text = string.Format(LanguageManager.Instance.GetString("UpdateForm_lblProgress_Progress"), downloaded, total, e.ProgressPercentage);
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
                    LanguageManager.Instance.GetString("UpdateForm_Msg_ConfirmCancel"),
                    LanguageManager.Instance.GetString("UpdateForm_Msg_ConfirmCancelTitle"),
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

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            LanguageManager.Instance.ApplyToForm(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.Instance.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }

    }
}