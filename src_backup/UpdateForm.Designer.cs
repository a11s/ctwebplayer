namespace ctwebplayer
{
    partial class UpdateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Labels
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCurrentVersion;
        private System.Windows.Forms.Label lblNewVersion;
        private System.Windows.Forms.Label lblReleaseDate;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblMandatory;
        private System.Windows.Forms.Label lblReleaseNotesTitle;

        // Buttons
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnSkip;
        private System.Windows.Forms.Button btnClose;

        // Other controls
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtReleaseNotes;
        private System.Windows.Forms.GroupBox grpVersionInfo;
        private System.Windows.Forms.GroupBox grpFileInfo;
        private System.Windows.Forms.GroupBox grpReleaseNotes;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            // Initialize controls
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.lblNewVersion = new System.Windows.Forms.Label();
            this.lblReleaseDate = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblMandatory = new System.Windows.Forms.Label();
            this.lblReleaseNotesTitle = new System.Windows.Forms.Label();
            
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnSkip = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtReleaseNotes = new System.Windows.Forms.TextBox();
            this.grpVersionInfo = new System.Windows.Forms.GroupBox();
            this.grpFileInfo = new System.Windows.Forms.GroupBox();
            this.grpReleaseNotes = new System.Windows.Forms.GroupBox();
            
            this.grpVersionInfo.SuspendLayout();
            this.grpFileInfo.SuspendLayout();
            this.grpReleaseNotes.SuspendLayout();
            this.SuspendLayout();
            
            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(180, 22);
            this.lblTitle.Text = "发现新版�?;
            
            // grpVersionInfo
            this.grpVersionInfo.Controls.Add(this.lblCurrentVersion);
            this.grpVersionInfo.Controls.Add(this.lblNewVersion);
            this.grpVersionInfo.Controls.Add(this.lblReleaseDate);
            this.grpVersionInfo.Location = new System.Drawing.Point(12, 50);
            this.grpVersionInfo.Name = "grpVersionInfo";
            this.grpVersionInfo.Size = new System.Drawing.Size(560, 90);
            this.grpVersionInfo.TabIndex = 1;
            this.grpVersionInfo.TabStop = false;
            this.grpVersionInfo.Text = "版本信息";
            
            // lblCurrentVersion
            this.lblCurrentVersion.AutoSize = true;
            this.lblCurrentVersion.Location = new System.Drawing.Point(15, 25);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(100, 17);
            this.lblCurrentVersion.Text = "当前版本: ";
            
            // lblNewVersion
            this.lblNewVersion.AutoSize = true;
            this.lblNewVersion.Location = new System.Drawing.Point(15, 45);
            this.lblNewVersion.Name = "lblNewVersion";
            this.lblNewVersion.Size = new System.Drawing.Size(100, 17);
            this.lblNewVersion.Text = "最新版�? ";
            
            // lblReleaseDate
            this.lblReleaseDate.AutoSize = true;
            this.lblReleaseDate.Location = new System.Drawing.Point(15, 65);
            this.lblReleaseDate.Name = "lblReleaseDate";
            this.lblReleaseDate.Size = new System.Drawing.Size(100, 17);
            this.lblReleaseDate.Text = "发布时间: ";
            
            // grpFileInfo
            this.grpFileInfo.Controls.Add(this.lblFileName);
            this.grpFileInfo.Controls.Add(this.lblFileSize);
            this.grpFileInfo.Location = new System.Drawing.Point(12, 150);
            this.grpFileInfo.Name = "grpFileInfo";
            this.grpFileInfo.Size = new System.Drawing.Size(560, 70);
            this.grpFileInfo.TabIndex = 2;
            this.grpFileInfo.TabStop = false;
            this.grpFileInfo.Text = "文件信息";
            
            // lblFileName
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(15, 25);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(100, 17);
            this.lblFileName.Text = "文件�? ";
            
            // lblFileSize
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.Location = new System.Drawing.Point(15, 45);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(100, 17);
            this.lblFileSize.Text = "文件大小: ";
            
            // grpReleaseNotes
            this.grpReleaseNotes.Controls.Add(this.txtReleaseNotes);
            this.grpReleaseNotes.Location = new System.Drawing.Point(12, 230);
            this.grpReleaseNotes.Name = "grpReleaseNotes";
            this.grpReleaseNotes.Size = new System.Drawing.Size(560, 220);
            this.grpReleaseNotes.TabIndex = 3;
            this.grpReleaseNotes.TabStop = false;
            this.grpReleaseNotes.Text = "更新说明";
            
            // txtReleaseNotes
            this.txtReleaseNotes.Location = new System.Drawing.Point(10, 25);
            this.txtReleaseNotes.Multiline = true;
            this.txtReleaseNotes.Name = "txtReleaseNotes";
            this.txtReleaseNotes.ReadOnly = true;
            this.txtReleaseNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtReleaseNotes.Size = new System.Drawing.Size(540, 185);
            this.txtReleaseNotes.TabIndex = 0;
            
            // progressBar
            this.progressBar.Location = new System.Drawing.Point(12, 500);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(560, 23);
            this.progressBar.TabIndex = 4;
            
            // lblProgress
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(12, 480);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(100, 17);
            this.lblProgress.Text = "准备下载...";
            
            // lblMandatory
            this.lblMandatory.AutoSize = true;
            this.lblMandatory.ForeColor = System.Drawing.Color.Red;
            this.lblMandatory.Location = new System.Drawing.Point(12, 460);
            this.lblMandatory.Name = "lblMandatory";
            this.lblMandatory.Size = new System.Drawing.Size(300, 17);
            this.lblMandatory.Text = "这是一个强制更新，必须安装后才能继续使用�?;
            this.lblMandatory.Visible = false;
            
            // btnDownload
            this.btnDownload.Location = new System.Drawing.Point(240, 540);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(100, 30);
            this.btnDownload.TabIndex = 5;
            this.btnDownload.Text = "下载更新";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            
            // btnInstall
            this.btnInstall.Location = new System.Drawing.Point(240, 540);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(100, 30);
            this.btnInstall.TabIndex = 6;
            this.btnInstall.Text = "安装更新";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Visible = false;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            
            // btnSkip
            this.btnSkip.Location = new System.Drawing.Point(350, 540);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(100, 30);
            this.btnSkip.TabIndex = 7;
            this.btnSkip.Text = "稍后提醒";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.btnSkip_Click);
            
            // btnClose
            this.btnClose.Location = new System.Drawing.Point(470, 540);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Visible = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            
            // UpdateForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 581);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.grpVersionInfo);
            this.Controls.Add(this.grpFileInfo);
            this.Controls.Add(this.grpReleaseNotes);
            this.Controls.Add(this.lblMandatory);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnSkip);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "软件更新";
            
            this.grpVersionInfo.ResumeLayout(false);
            this.grpVersionInfo.PerformLayout();
            this.grpFileInfo.ResumeLayout(false);
            this.grpFileInfo.PerformLayout();
            this.grpReleaseNotes.ResumeLayout(false);
            this.grpReleaseNotes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
