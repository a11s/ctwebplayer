namespace ctwebplayer
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabNetwork = new System.Windows.Forms.TabPage();
            this.chkAutoIframeNav = new System.Windows.Forms.CheckBox();
            this.chkEnableProxy = new System.Windows.Forms.CheckBox();
            this.grpProxySettings = new System.Windows.Forms.GroupBox();
            this.lblProxyNote = new System.Windows.Forms.Label();
            this.txtSocks5 = new System.Windows.Forms.TextBox();
            this.lblSocks5 = new System.Windows.Forms.Label();
            this.txtHttpsProxy = new System.Windows.Forms.TextBox();
            this.lblHttpsProxy = new System.Windows.Forms.Label();
            this.txtHttpProxy = new System.Windows.Forms.TextBox();
            this.lblHttpProxy = new System.Windows.Forms.Label();
            this.tabLogging = new System.Windows.Forms.TabPage();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.btnViewLogs = new System.Windows.Forms.Button();
            this.lblMB = new System.Windows.Forms.Label();
            this.numMaxFileSize = new System.Windows.Forms.NumericUpDown();
            this.lblMaxFileSize = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.chkEnableLogging = new System.Windows.Forms.CheckBox();
            this.tabInterface = new System.Windows.Forms.TabPage();
            this.lblSizeNote = new System.Windows.Forms.Label();
            this.btnResetSize = new System.Windows.Forms.Button();
            this.lblCurrentSize = new System.Windows.Forms.Label();
            this.lblHeightPx = new System.Windows.Forms.Label();
            this.numWindowHeight = new System.Windows.Forms.NumericUpDown();
            this.lblHeight = new System.Windows.Forms.Label();
            this.lblWidthPx = new System.Windows.Forms.Label();
            this.numWindowWidth = new System.Windows.Forms.NumericUpDown();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblWindowSize = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabNetwork.SuspendLayout();
            this.grpProxySettings.SuspendLayout();
            this.tabLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFileSize)).BeginInit();
            this.tabInterface.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWindowWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWindowHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabNetwork);
            this.tabControl.Controls.Add(this.tabLogging);
            this.tabControl.Controls.Add(this.tabInterface);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(460, 300);
            this.tabControl.TabIndex = 0;
            // 
            // tabNetwork
            // 
            this.tabNetwork.Controls.Add(this.chkAutoIframeNav);
            this.tabNetwork.Controls.Add(this.chkEnableProxy);
            this.tabNetwork.Controls.Add(this.grpProxySettings);
            this.tabNetwork.Location = new System.Drawing.Point(4, 24);
            this.tabNetwork.Name = "tabNetwork";
            this.tabNetwork.Padding = new System.Windows.Forms.Padding(3);
            this.tabNetwork.Size = new System.Drawing.Size(452, 272);
            this.tabNetwork.TabIndex = 0;
            this.tabNetwork.Text = "网络";
            this.tabNetwork.UseVisualStyleBackColor = true;
            // 
            // chkAutoIframeNav
            // 
            this.chkAutoIframeNav.AutoSize = true;
            this.chkAutoIframeNav.Location = new System.Drawing.Point(20, 20);
            this.chkAutoIframeNav.Name = "chkAutoIframeNav";
            this.chkAutoIframeNav.Size = new System.Drawing.Size(200, 25);
            this.chkAutoIframeNav.TabIndex = 0;
            this.chkAutoIframeNav.Text = "自动导航到 iframe 内容";
            this.chkAutoIframeNav.UseVisualStyleBackColor = true;
            // 
            // chkEnableProxy
            // 
            this.chkEnableProxy.AutoSize = true;
            this.chkEnableProxy.Location = new System.Drawing.Point(20, 60);
            this.chkEnableProxy.Name = "chkEnableProxy";
            this.chkEnableProxy.Size = new System.Drawing.Size(150, 25);
            this.chkEnableProxy.TabIndex = 1;
            this.chkEnableProxy.Text = "启用代理服务器";
            this.chkEnableProxy.UseVisualStyleBackColor = true;
            this.chkEnableProxy.CheckedChanged += new System.EventHandler(this.ChkEnableProxy_CheckedChanged);
            // 
            // grpProxySettings
            // 
            this.grpProxySettings.Controls.Add(this.lblProxyNote);
            this.grpProxySettings.Controls.Add(this.txtSocks5);
            this.grpProxySettings.Controls.Add(this.lblSocks5);
            this.grpProxySettings.Controls.Add(this.txtHttpsProxy);
            this.grpProxySettings.Controls.Add(this.lblHttpsProxy);
            this.grpProxySettings.Controls.Add(this.txtHttpProxy);
            this.grpProxySettings.Controls.Add(this.lblHttpProxy);
            this.grpProxySettings.Enabled = false;
            this.grpProxySettings.Location = new System.Drawing.Point(20, 95);
            this.grpProxySettings.Name = "grpProxySettings";
            this.grpProxySettings.Size = new System.Drawing.Size(410, 180);
            this.grpProxySettings.TabIndex = 2;
            this.grpProxySettings.TabStop = false;
            this.grpProxySettings.Text = "代理服务器设置";
            // 
            // lblProxyNote
            // 
            this.lblProxyNote.ForeColor = System.Drawing.Color.Gray;
            this.lblProxyNote.Location = new System.Drawing.Point(20, 140);
            this.lblProxyNote.Name = "lblProxyNote";
            this.lblProxyNote.Size = new System.Drawing.Size(370, 35);
            this.lblProxyNote.TabIndex = 6;
            this.lblProxyNote.Text = "注意：优先使用SOCKS5代理，其次是HTTP/HTTPS代理。\r\n更改代理设置后需要重启浏览器才能生效。";
            // 
            // txtSocks5
            // 
            this.txtSocks5.Location = new System.Drawing.Point(105, 100);
            this.txtSocks5.Name = "txtSocks5";
            this.txtSocks5.PlaceholderText = "例如: 127.0.0.1:7890";
            this.txtSocks5.Size = new System.Drawing.Size(280, 25);
            this.txtSocks5.TabIndex = 5;
            // 
            // lblSocks5
            // 
            this.lblSocks5.Location = new System.Drawing.Point(20, 100);
            this.lblSocks5.Name = "lblSocks5";
            this.lblSocks5.Size = new System.Drawing.Size(80, 25);
            this.lblSocks5.TabIndex = 4;
            this.lblSocks5.Text = "SOCKS5 代理:";
            this.lblSocks5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHttpsProxy
            // 
            this.txtHttpsProxy.Location = new System.Drawing.Point(105, 65);
            this.txtHttpsProxy.Name = "txtHttpsProxy";
            this.txtHttpsProxy.PlaceholderText = "例如: http://127.0.0.1:7890";
            this.txtHttpsProxy.Size = new System.Drawing.Size(280, 25);
            this.txtHttpsProxy.TabIndex = 3;
            // 
            // lblHttpsProxy
            // 
            this.lblHttpsProxy.Location = new System.Drawing.Point(20, 65);
            this.lblHttpsProxy.Name = "lblHttpsProxy";
            this.lblHttpsProxy.Size = new System.Drawing.Size(80, 25);
            this.lblHttpsProxy.TabIndex = 2;
            this.lblHttpsProxy.Text = "HTTPS 代理:";
            this.lblHttpsProxy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHttpProxy
            // 
            this.txtHttpProxy.Location = new System.Drawing.Point(105, 30);
            this.txtHttpProxy.Name = "txtHttpProxy";
            this.txtHttpProxy.PlaceholderText = "例如: http://127.0.0.1:7890";
            this.txtHttpProxy.Size = new System.Drawing.Size(280, 25);
            this.txtHttpProxy.TabIndex = 1;
            // 
            // lblHttpProxy
            // 
            this.lblHttpProxy.Location = new System.Drawing.Point(20, 30);
            this.lblHttpProxy.Name = "lblHttpProxy";
            this.lblHttpProxy.Size = new System.Drawing.Size(80, 25);
            this.lblHttpProxy.TabIndex = 0;
            this.lblHttpProxy.Text = "HTTP 代理:";
            this.lblHttpProxy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabLogging
            // 
            this.tabLogging.Controls.Add(this.btnClearLogs);
            this.tabLogging.Controls.Add(this.btnViewLogs);
            this.tabLogging.Controls.Add(this.lblMB);
            this.tabLogging.Controls.Add(this.numMaxFileSize);
            this.tabLogging.Controls.Add(this.lblMaxFileSize);
            this.tabLogging.Controls.Add(this.cmbLogLevel);
            this.tabLogging.Controls.Add(this.lblLogLevel);
            this.tabLogging.Controls.Add(this.chkEnableLogging);
            this.tabLogging.Location = new System.Drawing.Point(4, 24);
            this.tabLogging.Name = "tabLogging";
            this.tabLogging.Padding = new System.Windows.Forms.Padding(3);
            this.tabLogging.Size = new System.Drawing.Size(452, 272);
            this.tabLogging.TabIndex = 1;
            this.tabLogging.Text = "日志";
            this.tabLogging.UseVisualStyleBackColor = true;
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Location = new System.Drawing.Point(130, 150);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(100, 30);
            this.btnClearLogs.TabIndex = 7;
            this.btnClearLogs.Text = "清理日志";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.BtnClearLogs_Click);
            // 
            // btnViewLogs
            // 
            this.btnViewLogs.Location = new System.Drawing.Point(20, 150);
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new System.Drawing.Size(100, 30);
            this.btnViewLogs.TabIndex = 6;
            this.btnViewLogs.Text = "查看日志";
            this.btnViewLogs.UseVisualStyleBackColor = true;
            this.btnViewLogs.Click += new System.EventHandler(this.BtnViewLogs_Click);
            // 
            // lblMB
            // 
            this.lblMB.Location = new System.Drawing.Point(210, 100);
            this.lblMB.Name = "lblMB";
            this.lblMB.Size = new System.Drawing.Size(30, 25);
            this.lblMB.TabIndex = 5;
            this.lblMB.Text = "MB";
            this.lblMB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numMaxFileSize
            // 
            this.numMaxFileSize.Location = new System.Drawing.Point(105, 100);
            this.numMaxFileSize.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMaxFileSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxFileSize.Name = "numMaxFileSize";
            this.numMaxFileSize.Size = new System.Drawing.Size(100, 25);
            this.numMaxFileSize.TabIndex = 4;
            this.numMaxFileSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblMaxFileSize
            // 
            this.lblMaxFileSize.Location = new System.Drawing.Point(20, 100);
            this.lblMaxFileSize.Name = "lblMaxFileSize";
            this.lblMaxFileSize.Size = new System.Drawing.Size(80, 25);
            this.lblMaxFileSize.TabIndex = 3;
            this.lblMaxFileSize.Text = "最大文件大小:";
            this.lblMaxFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbLogLevel
            // 
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] {
            "Debug",
            "Info",
            "Warning",
            "Error"});
            this.cmbLogLevel.Location = new System.Drawing.Point(105, 60);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(150, 25);
            this.cmbLogLevel.TabIndex = 2;
            // 
            // lblLogLevel
            // 
            this.lblLogLevel.Location = new System.Drawing.Point(20, 60);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(80, 25);
            this.lblLogLevel.TabIndex = 1;
            this.lblLogLevel.Text = "日志级别:";
            this.lblLogLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkEnableLogging
            // 
            this.chkEnableLogging.AutoSize = true;
            this.chkEnableLogging.Checked = true;
            this.chkEnableLogging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableLogging.Location = new System.Drawing.Point(20, 20);
            this.chkEnableLogging.Name = "chkEnableLogging";
            this.chkEnableLogging.Size = new System.Drawing.Size(150, 25);
            this.chkEnableLogging.TabIndex = 0;
            this.chkEnableLogging.Text = "启用日志记录";
            this.chkEnableLogging.UseVisualStyleBackColor = true;
            this.chkEnableLogging.CheckedChanged += new System.EventHandler(this.ChkEnableLogging_CheckedChanged);
            // 
            // tabInterface
            // 
            this.tabInterface.Controls.Add(this.lblSizeNote);
            this.tabInterface.Controls.Add(this.btnResetSize);
            this.tabInterface.Controls.Add(this.lblCurrentSize);
            this.tabInterface.Controls.Add(this.lblHeightPx);
            this.tabInterface.Controls.Add(this.numWindowHeight);
            this.tabInterface.Controls.Add(this.lblHeight);
            this.tabInterface.Controls.Add(this.lblWidthPx);
            this.tabInterface.Controls.Add(this.numWindowWidth);
            this.tabInterface.Controls.Add(this.lblWidth);
            this.tabInterface.Controls.Add(this.lblWindowSize);
            this.tabInterface.Location = new System.Drawing.Point(4, 24);
            this.tabInterface.Name = "tabInterface";
            this.tabInterface.Padding = new System.Windows.Forms.Padding(3);
            this.tabInterface.Size = new System.Drawing.Size(452, 272);
            this.tabInterface.TabIndex = 2;
            this.tabInterface.Text = "界面";
            this.tabInterface.UseVisualStyleBackColor = true;
            // 
            // lblSizeNote
            // 
            this.lblSizeNote.ForeColor = System.Drawing.Color.Gray;
            this.lblSizeNote.Location = new System.Drawing.Point(20, 200);
            this.lblSizeNote.Name = "lblSizeNote";
            this.lblSizeNote.Size = new System.Drawing.Size(410, 40);
            this.lblSizeNote.TabIndex = 9;
            this.lblSizeNote.Text = "注意：更改窗口大小后，需要重启应用才能生效。\r\n当前窗口大小：1136 x 640 像素";
            // 
            // btnResetSize
            // 
            this.btnResetSize.Location = new System.Drawing.Point(20, 150);
            this.btnResetSize.Name = "btnResetSize";
            this.btnResetSize.Size = new System.Drawing.Size(100, 30);
            this.btnResetSize.TabIndex = 8;
            this.btnResetSize.Text = "重置大小";
            this.btnResetSize.UseVisualStyleBackColor = true;
            this.btnResetSize.Click += new System.EventHandler(this.BtnResetSize_Click);
            // 
            // lblCurrentSize
            // 
            this.lblCurrentSize.Location = new System.Drawing.Point(130, 155);
            this.lblCurrentSize.Name = "lblCurrentSize";
            this.lblCurrentSize.Size = new System.Drawing.Size(200, 20);
            this.lblCurrentSize.TabIndex = 7;
            this.lblCurrentSize.Text = "当前大小：1136 x 640";
            // 
            // lblHeightPx
            // 
            this.lblHeightPx.Location = new System.Drawing.Point(360, 60);
            this.lblHeightPx.Name = "lblHeightPx";
            this.lblHeightPx.Size = new System.Drawing.Size(40, 25);
            this.lblHeightPx.TabIndex = 6;
            this.lblHeightPx.Text = "像素";
            this.lblHeightPx.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numWindowHeight
            // 
            this.numWindowHeight.Location = new System.Drawing.Point(275, 60);
            this.numWindowHeight.Maximum = new decimal(new int[] {
            2160,
            0,
            0,
            0});
            this.numWindowHeight.Minimum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numWindowHeight.Name = "numWindowHeight";
            this.numWindowHeight.Size = new System.Drawing.Size(80, 25);
            this.numWindowHeight.TabIndex = 5;
            this.numWindowHeight.Value = new decimal(new int[] {
            640,
            0,
            0,
            0});
            // 
            // lblHeight
            // 
            this.lblHeight.Location = new System.Drawing.Point(220, 60);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(50, 25);
            this.lblHeight.TabIndex = 4;
            this.lblHeight.Text = "高度:";
            this.lblHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWidthPx
            // 
            this.lblWidthPx.Location = new System.Drawing.Point(160, 60);
            this.lblWidthPx.Name = "lblWidthPx";
            this.lblWidthPx.Size = new System.Drawing.Size(40, 25);
            this.lblWidthPx.TabIndex = 3;
            this.lblWidthPx.Text = "像素";
            this.lblWidthPx.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numWindowWidth
            // 
            this.numWindowWidth.Location = new System.Drawing.Point(75, 60);
            this.numWindowWidth.Maximum = new decimal(new int[] {
            3840,
            0,
            0,
            0});
            this.numWindowWidth.Minimum = new decimal(new int[] {
            800,
            0,
            0,
            0});
            this.numWindowWidth.Name = "numWindowWidth";
            this.numWindowWidth.Size = new System.Drawing.Size(80, 25);
            this.numWindowWidth.TabIndex = 2;
            this.numWindowWidth.Value = new decimal(new int[] {
            1136,
            0,
            0,
            0});
            // 
            // lblWidth
            // 
            this.lblWidth.Location = new System.Drawing.Point(20, 60);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(50, 25);
            this.lblWidth.TabIndex = 1;
            this.lblWidth.Text = "宽度:";
            this.lblWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWindowSize
            // 
            this.lblWindowSize.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblWindowSize.Location = new System.Drawing.Point(20, 20);
            this.lblWindowSize.Name = "lblWindowSize";
            this.lblWindowSize.Size = new System.Drawing.Size(100, 25);
            this.lblWindowSize.TabIndex = 0;
            this.lblWindowSize.Text = "窗口大小设置";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(302, 320);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(383, 320);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 30);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "应用";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(221, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置";
            this.tabControl.ResumeLayout(false);
            this.tabNetwork.ResumeLayout(false);
            this.tabNetwork.PerformLayout();
            this.grpProxySettings.ResumeLayout(false);
            this.grpProxySettings.PerformLayout();
            this.tabLogging.ResumeLayout(false);
            this.tabLogging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFileSize)).EndInit();
            this.tabInterface.ResumeLayout(false);
            this.tabInterface.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numWindowWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWindowHeight)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabNetwork;
        private System.Windows.Forms.CheckBox chkAutoIframeNav;
        private System.Windows.Forms.CheckBox chkEnableProxy;
        private System.Windows.Forms.GroupBox grpProxySettings;
        private System.Windows.Forms.Label lblProxyNote;
        private System.Windows.Forms.TextBox txtSocks5;
        private System.Windows.Forms.Label lblSocks5;
        private System.Windows.Forms.TextBox txtHttpsProxy;
        private System.Windows.Forms.Label lblHttpsProxy;
        private System.Windows.Forms.TextBox txtHttpProxy;
        private System.Windows.Forms.Label lblHttpProxy;
        private System.Windows.Forms.TabPage tabLogging;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.Button btnViewLogs;
        private System.Windows.Forms.Label lblMB;
        private System.Windows.Forms.NumericUpDown numMaxFileSize;
        private System.Windows.Forms.Label lblMaxFileSize;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.CheckBox chkEnableLogging;
        private System.Windows.Forms.TabPage tabInterface;
        private System.Windows.Forms.Label lblSizeNote;
        private System.Windows.Forms.Button btnResetSize;
        private System.Windows.Forms.Label lblCurrentSize;
        private System.Windows.Forms.Label lblHeightPx;
        private System.Windows.Forms.NumericUpDown numWindowHeight;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Label lblWidthPx;
        private System.Windows.Forms.NumericUpDown numWindowWidth;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblWindowSize;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
    }
}