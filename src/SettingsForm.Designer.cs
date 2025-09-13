namespace ctwebplayer
{
    public partial class SettingsForm
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
            components = new System.ComponentModel.Container();
            tabControl = new TabControl();
            tabNetwork = new TabPage();
            lblBaseURL = new Label();
            txtBaseURL = new TextBox();
            chkAutoIframeNav = new CheckBox();
            chkEnableProxy = new CheckBox();
            grpProxySettings = new GroupBox();
            lblProxyNote = new Label();
            txtSocks5 = new TextBox();
            lblSocks5 = new Label();
            txtHttpsProxy = new TextBox();
            lblHttpsProxy = new Label();
            txtHttpProxy = new TextBox();
            lblHttpProxy = new Label();
            tabLogging = new TabPage();
            chkDebugMode = new CheckBox();
            btnClearLogs = new Button();
            btnViewLogs = new Button();
            lblMB = new Label();
            numMaxFileSize = new NumericUpDown();
            lblMaxFileSize = new Label();
            cmbLogLevel = new ComboBox();
            lblLogLevel = new Label();
            chkEnableLogging = new CheckBox();
            tabInterface = new TabPage();
            lblSizeNote = new Label();
            btnResetSize = new Button();
            lblCurrentSize = new Label();
            lblHeightPx = new Label();
            numWindowHeight = new NumericUpDown();
            lblHeight = new Label();
            lblWidthPx = new Label();
            numWindowWidth = new NumericUpDown();
            lblWidth = new Label();
            lblWindowSize = new Label();
            tabLogin = new TabPage();
            grpLoginSettings = new GroupBox();
            chkEnableLogin = new CheckBox();
            chkShowSkipButton = new CheckBox();
            lblCookieName = new Label();
            txtCookieName = new TextBox();
            lblLoginUrl = new Label();
            txtLoginUrl = new TextBox();
            lblRegisterUrl = new Label();
            txtRegisterUrl = new TextBox();
            btnSave = new Button();
            btnApply = new Button();
            btnCancel = new Button();
            toolTip1 = new ToolTip(components);
            linkLabel1 = new LinkLabel();
            tabControl.SuspendLayout();
            tabNetwork.SuspendLayout();
            grpProxySettings.SuspendLayout();
            tabLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxFileSize).BeginInit();
            tabInterface.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numWindowHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWindowWidth).BeginInit();
            tabLogin.SuspendLayout();
            grpLoginSettings.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabNetwork);
            tabControl.Controls.Add(tabLogging);
            tabControl.Controls.Add(tabInterface);
            tabControl.Controls.Add(tabLogin);
            tabControl.Location = new Point(9, 11);
            tabControl.Margin = new Padding(2, 3, 2, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(358, 283);
            tabControl.TabIndex = 0;
            // 
            // tabNetwork
            // 
            tabNetwork.Controls.Add(linkLabel1);
            tabNetwork.Controls.Add(lblBaseURL);
            tabNetwork.Controls.Add(txtBaseURL);
            tabNetwork.Controls.Add(chkAutoIframeNav);
            tabNetwork.Controls.Add(chkEnableProxy);
            tabNetwork.Controls.Add(grpProxySettings);
            tabNetwork.Location = new Point(4, 26);
            tabNetwork.Margin = new Padding(2, 3, 2, 3);
            tabNetwork.Name = "tabNetwork";
            tabNetwork.Padding = new Padding(2, 3, 2, 3);
            tabNetwork.Size = new Size(350, 253);
            tabNetwork.TabIndex = 0;
            tabNetwork.Text = "网络";
            tabNetwork.UseVisualStyleBackColor = true;
            // 
            // lblBaseURL
            // 
            lblBaseURL.AutoSize = true;
            lblBaseURL.Location = new Point(16, 52);
            lblBaseURL.Margin = new Padding(2, 0, 2, 0);
            lblBaseURL.Name = "lblBaseURL";
            lblBaseURL.Size = new Size(68, 17);
            lblBaseURL.TabIndex = 2;
            lblBaseURL.Text = "主站域名：";
            // 
            // txtBaseURL
            // 
            txtBaseURL.Location = new Point(82, 49);
            txtBaseURL.Margin = new Padding(2, 3, 2, 3);
            txtBaseURL.Name = "txtBaseURL";
            txtBaseURL.PlaceholderText = "例如: https://game.ero-labs.live";
            txtBaseURL.Size = new Size(250, 23);
            txtBaseURL.TabIndex = 3;
            // 
            // chkAutoIframeNav
            // 
            chkAutoIframeNav.AutoSize = true;
            chkAutoIframeNav.Location = new Point(16, 19);
            chkAutoIframeNav.Margin = new Padding(2, 3, 2, 3);
            chkAutoIframeNav.Name = "chkAutoIframeNav";
            chkAutoIframeNav.Size = new Size(156, 21);
            chkAutoIframeNav.TabIndex = 0;
            chkAutoIframeNav.Text = "自动导航到 iframe 内容";
            chkAutoIframeNav.UseVisualStyleBackColor = true;
            // 
            // chkEnableProxy
            // 
            chkEnableProxy.AutoSize = true;
            chkEnableProxy.Location = new Point(16, 90);
            chkEnableProxy.Margin = new Padding(2, 3, 2, 3);
            chkEnableProxy.Name = "chkEnableProxy";
            chkEnableProxy.Size = new Size(111, 21);
            chkEnableProxy.TabIndex = 4;
            chkEnableProxy.Text = "启用代理服务器";
            chkEnableProxy.UseVisualStyleBackColor = true;
            chkEnableProxy.CheckedChanged += ChkEnableProxy_CheckedChanged;
            // 
            // grpProxySettings
            // 
            grpProxySettings.Controls.Add(lblProxyNote);
            grpProxySettings.Controls.Add(txtSocks5);
            grpProxySettings.Controls.Add(lblSocks5);
            grpProxySettings.Controls.Add(txtHttpsProxy);
            grpProxySettings.Controls.Add(lblHttpsProxy);
            grpProxySettings.Controls.Add(txtHttpProxy);
            grpProxySettings.Controls.Add(lblHttpProxy);
            grpProxySettings.Enabled = false;
            grpProxySettings.Location = new Point(16, 123);
            grpProxySettings.Margin = new Padding(2, 3, 2, 3);
            grpProxySettings.Name = "grpProxySettings";
            grpProxySettings.Padding = new Padding(2, 3, 2, 3);
            grpProxySettings.Size = new Size(319, 132);
            grpProxySettings.TabIndex = 5;
            grpProxySettings.TabStop = false;
            grpProxySettings.Text = "代理服务器设置";
            // 
            // lblProxyNote
            // 
            lblProxyNote.ForeColor = Color.Gray;
            lblProxyNote.Location = new Point(16, 94);
            lblProxyNote.Margin = new Padding(2, 0, 2, 0);
            lblProxyNote.Name = "lblProxyNote";
            lblProxyNote.Size = new Size(288, 33);
            lblProxyNote.TabIndex = 6;
            lblProxyNote.Text = "注意：优先使用SOCKS5代理，其次是HTTP/HTTPS代理。\r\n更改代理设置后需要重启浏览器才能生效。";
            // 
            // txtSocks5
            // 
            txtSocks5.Location = new Point(82, 61);
            txtSocks5.Margin = new Padding(2, 3, 2, 3);
            txtSocks5.Name = "txtSocks5";
            txtSocks5.PlaceholderText = "例如: 127.0.0.1:7890";
            txtSocks5.Size = new Size(219, 23);
            txtSocks5.TabIndex = 5;
            // 
            // lblSocks5
            // 
            lblSocks5.Location = new Point(16, 61);
            lblSocks5.Margin = new Padding(2, 0, 2, 0);
            lblSocks5.Name = "lblSocks5";
            lblSocks5.Size = new Size(62, 24);
            lblSocks5.TabIndex = 4;
            lblSocks5.Text = "SOCKS5 代理:";
            lblSocks5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtHttpsProxy
            // 
            txtHttpsProxy.Location = new Point(226, 28);
            txtHttpsProxy.Margin = new Padding(2, 3, 2, 3);
            txtHttpsProxy.Name = "txtHttpsProxy";
            txtHttpsProxy.PlaceholderText = "例如: http://127.0.0.1:7890";
            txtHttpsProxy.Size = new Size(75, 23);
            txtHttpsProxy.TabIndex = 3;
            // 
            // lblHttpsProxy
            // 
            lblHttpsProxy.Location = new Point(163, 28);
            lblHttpsProxy.Margin = new Padding(2, 0, 2, 0);
            lblHttpsProxy.Name = "lblHttpsProxy";
            lblHttpsProxy.Size = new Size(58, 24);
            lblHttpsProxy.TabIndex = 2;
            lblHttpsProxy.Text = "HTTPS:";
            lblHttpsProxy.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtHttpProxy
            // 
            txtHttpProxy.Location = new Point(82, 28);
            txtHttpProxy.Margin = new Padding(2, 3, 2, 3);
            txtHttpProxy.Name = "txtHttpProxy";
            txtHttpProxy.PlaceholderText = "例如: http://127.0.0.1:7890";
            txtHttpProxy.Size = new Size(75, 23);
            txtHttpProxy.TabIndex = 1;
            // 
            // lblHttpProxy
            // 
            lblHttpProxy.Location = new Point(16, 28);
            lblHttpProxy.Margin = new Padding(2, 0, 2, 0);
            lblHttpProxy.Name = "lblHttpProxy";
            lblHttpProxy.Size = new Size(62, 24);
            lblHttpProxy.TabIndex = 0;
            lblHttpProxy.Text = "HTTP:";
            lblHttpProxy.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tabLogging
            // 
            tabLogging.Controls.Add(chkDebugMode);
            tabLogging.Controls.Add(btnClearLogs);
            tabLogging.Controls.Add(btnViewLogs);
            tabLogging.Controls.Add(lblMB);
            tabLogging.Controls.Add(numMaxFileSize);
            tabLogging.Controls.Add(lblMaxFileSize);
            tabLogging.Controls.Add(cmbLogLevel);
            tabLogging.Controls.Add(lblLogLevel);
            tabLogging.Controls.Add(chkEnableLogging);
            tabLogging.Location = new Point(4, 26);
            tabLogging.Margin = new Padding(2, 3, 2, 3);
            tabLogging.Name = "tabLogging";
            tabLogging.Padding = new Padding(2, 3, 2, 3);
            tabLogging.Size = new Size(350, 253);
            tabLogging.TabIndex = 1;
            tabLogging.Text = "日志";
            tabLogging.UseVisualStyleBackColor = true;
            // 
            // chkDebugMode
            // 
            chkDebugMode.AutoSize = true;
            chkDebugMode.Location = new Point(16, 179);
            chkDebugMode.Margin = new Padding(2, 3, 2, 3);
            chkDebugMode.Name = "chkDebugMode";
            chkDebugMode.Size = new Size(90, 21);
            chkDebugMode.TabIndex = 8;
            chkDebugMode.Text = "Debug模式";
            toolTip1.SetToolTip(chkDebugMode, "开启后将记录所有HTTP请求及缓存状态到request.log文件");
            chkDebugMode.UseVisualStyleBackColor = true;
            // 
            // btnClearLogs
            // 
            btnClearLogs.Location = new Point(101, 142);
            btnClearLogs.Margin = new Padding(2, 3, 2, 3);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(78, 28);
            btnClearLogs.TabIndex = 7;
            btnClearLogs.Text = "清理日志";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += BtnClearLogs_Click;
            // 
            // btnViewLogs
            // 
            btnViewLogs.Location = new Point(16, 142);
            btnViewLogs.Margin = new Padding(2, 3, 2, 3);
            btnViewLogs.Name = "btnViewLogs";
            btnViewLogs.Size = new Size(78, 28);
            btnViewLogs.TabIndex = 6;
            btnViewLogs.Text = "查看日志";
            btnViewLogs.UseVisualStyleBackColor = true;
            btnViewLogs.Click += BtnViewLogs_Click;
            // 
            // lblMB
            // 
            lblMB.Location = new Point(163, 94);
            lblMB.Margin = new Padding(2, 0, 2, 0);
            lblMB.Name = "lblMB";
            lblMB.Size = new Size(23, 24);
            lblMB.TabIndex = 5;
            lblMB.Text = "MB";
            lblMB.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numMaxFileSize
            // 
            numMaxFileSize.Location = new Point(82, 94);
            numMaxFileSize.Margin = new Padding(2, 3, 2, 3);
            numMaxFileSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMaxFileSize.Name = "numMaxFileSize";
            numMaxFileSize.Size = new Size(78, 23);
            numMaxFileSize.TabIndex = 4;
            numMaxFileSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblMaxFileSize
            // 
            lblMaxFileSize.Location = new Point(16, 94);
            lblMaxFileSize.Margin = new Padding(2, 0, 2, 0);
            lblMaxFileSize.Name = "lblMaxFileSize";
            lblMaxFileSize.Size = new Size(62, 24);
            lblMaxFileSize.TabIndex = 3;
            lblMaxFileSize.Text = "最大文件大小:";
            lblMaxFileSize.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbLogLevel
            // 
            cmbLogLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLogLevel.FormattingEnabled = true;
            cmbLogLevel.Items.AddRange(new object[] { "Debug", "Info", "Warning", "Error" });
            cmbLogLevel.Location = new Point(82, 57);
            cmbLogLevel.Margin = new Padding(2, 3, 2, 3);
            cmbLogLevel.Name = "cmbLogLevel";
            cmbLogLevel.Size = new Size(118, 25);
            cmbLogLevel.TabIndex = 2;
            // 
            // lblLogLevel
            // 
            lblLogLevel.Location = new Point(16, 57);
            lblLogLevel.Margin = new Padding(2, 0, 2, 0);
            lblLogLevel.Name = "lblLogLevel";
            lblLogLevel.Size = new Size(62, 24);
            lblLogLevel.TabIndex = 1;
            lblLogLevel.Text = "日志级别:";
            lblLogLevel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkEnableLogging
            // 
            chkEnableLogging.AutoSize = true;
            chkEnableLogging.Checked = true;
            chkEnableLogging.CheckState = CheckState.Checked;
            chkEnableLogging.Location = new Point(16, 19);
            chkEnableLogging.Margin = new Padding(2, 3, 2, 3);
            chkEnableLogging.Name = "chkEnableLogging";
            chkEnableLogging.Size = new Size(99, 21);
            chkEnableLogging.TabIndex = 0;
            chkEnableLogging.Text = "启用日志记录";
            chkEnableLogging.UseVisualStyleBackColor = true;
            chkEnableLogging.CheckedChanged += ChkEnableLogging_CheckedChanged;
            // 
            // tabInterface
            // 
            tabInterface.Controls.Add(lblSizeNote);
            tabInterface.Controls.Add(btnResetSize);
            tabInterface.Controls.Add(lblCurrentSize);
            tabInterface.Controls.Add(lblHeightPx);
            tabInterface.Controls.Add(numWindowHeight);
            tabInterface.Controls.Add(lblHeight);
            tabInterface.Controls.Add(lblWidthPx);
            tabInterface.Controls.Add(numWindowWidth);
            tabInterface.Controls.Add(lblWidth);
            tabInterface.Controls.Add(lblWindowSize);
            tabInterface.Location = new Point(4, 26);
            tabInterface.Margin = new Padding(2, 3, 2, 3);
            tabInterface.Name = "tabInterface";
            tabInterface.Padding = new Padding(2, 3, 2, 3);
            tabInterface.Size = new Size(350, 253);
            tabInterface.TabIndex = 2;
            tabInterface.Text = "界面";
            tabInterface.UseVisualStyleBackColor = true;
            // 
            // lblSizeNote
            // 
            lblSizeNote.ForeColor = Color.Gray;
            lblSizeNote.Location = new Point(16, 189);
            lblSizeNote.Margin = new Padding(2, 0, 2, 0);
            lblSizeNote.Name = "lblSizeNote";
            lblSizeNote.Size = new Size(319, 38);
            lblSizeNote.TabIndex = 9;
            lblSizeNote.Text = "注意：更改窗口大小后，需要重启应用才能生效。\r\n当前窗口大小：1136 x 640 像素";
            // 
            // btnResetSize
            // 
            btnResetSize.Location = new Point(16, 142);
            btnResetSize.Margin = new Padding(2, 3, 2, 3);
            btnResetSize.Name = "btnResetSize";
            btnResetSize.Size = new Size(78, 28);
            btnResetSize.TabIndex = 8;
            btnResetSize.Text = "重置大小";
            btnResetSize.UseVisualStyleBackColor = true;
            btnResetSize.Click += BtnResetSize_Click;
            // 
            // lblCurrentSize
            // 
            lblCurrentSize.Location = new Point(101, 146);
            lblCurrentSize.Margin = new Padding(2, 0, 2, 0);
            lblCurrentSize.Name = "lblCurrentSize";
            lblCurrentSize.Size = new Size(156, 19);
            lblCurrentSize.TabIndex = 7;
            lblCurrentSize.Text = "当前大小：1136 x 640";
            // 
            // lblHeightPx
            // 
            lblHeightPx.Location = new Point(280, 57);
            lblHeightPx.Margin = new Padding(2, 0, 2, 0);
            lblHeightPx.Name = "lblHeightPx";
            lblHeightPx.Size = new Size(31, 24);
            lblHeightPx.TabIndex = 6;
            lblHeightPx.Text = "像素";
            lblHeightPx.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numWindowHeight
            // 
            numWindowHeight.Location = new Point(214, 57);
            numWindowHeight.Margin = new Padding(2, 3, 2, 3);
            numWindowHeight.Maximum = new decimal(new int[] { 2160, 0, 0, 0 });
            numWindowHeight.Minimum = new decimal(new int[] { 600, 0, 0, 0 });
            numWindowHeight.Name = "numWindowHeight";
            numWindowHeight.Size = new Size(62, 23);
            numWindowHeight.TabIndex = 5;
            numWindowHeight.Value = new decimal(new int[] { 640, 0, 0, 0 });
            // 
            // lblHeight
            // 
            lblHeight.Location = new Point(171, 57);
            lblHeight.Margin = new Padding(2, 0, 2, 0);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new Size(39, 24);
            lblHeight.TabIndex = 4;
            lblHeight.Text = "高度:";
            lblHeight.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblWidthPx
            // 
            lblWidthPx.Location = new Point(124, 57);
            lblWidthPx.Margin = new Padding(2, 0, 2, 0);
            lblWidthPx.Name = "lblWidthPx";
            lblWidthPx.Size = new Size(31, 24);
            lblWidthPx.TabIndex = 3;
            lblWidthPx.Text = "像素";
            lblWidthPx.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numWindowWidth
            // 
            numWindowWidth.Location = new Point(58, 57);
            numWindowWidth.Margin = new Padding(2, 3, 2, 3);
            numWindowWidth.Maximum = new decimal(new int[] { 3840, 0, 0, 0 });
            numWindowWidth.Minimum = new decimal(new int[] { 800, 0, 0, 0 });
            numWindowWidth.Name = "numWindowWidth";
            numWindowWidth.Size = new Size(62, 23);
            numWindowWidth.TabIndex = 2;
            numWindowWidth.Value = new decimal(new int[] { 1136, 0, 0, 0 });
            // 
            // lblWidth
            // 
            lblWidth.Location = new Point(16, 57);
            lblWidth.Margin = new Padding(2, 0, 2, 0);
            lblWidth.Name = "lblWidth";
            lblWidth.Size = new Size(39, 24);
            lblWidth.TabIndex = 1;
            lblWidth.Text = "宽度:";
            lblWidth.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblWindowSize
            // 
            lblWindowSize.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblWindowSize.Location = new Point(16, 19);
            lblWindowSize.Margin = new Padding(2, 0, 2, 0);
            lblWindowSize.Name = "lblWindowSize";
            lblWindowSize.Size = new Size(78, 24);
            lblWindowSize.TabIndex = 0;
            lblWindowSize.Text = "窗口大小设置";
            //
            // tabLogin
            //
            tabLogin.Controls.Add(grpLoginSettings);
            tabLogin.Location = new Point(4, 26);
            tabLogin.Margin = new Padding(2, 3, 2, 3);
            tabLogin.Name = "tabLogin";
            tabLogin.Padding = new Padding(2, 3, 2, 3);
            tabLogin.Size = new Size(350, 253);
            tabLogin.TabIndex = 3;
            tabLogin.Text = "登录引导";
            tabLogin.UseVisualStyleBackColor = true;
            //
            // grpLoginSettings
            //
            grpLoginSettings.Controls.Add(chkEnableLogin);
            grpLoginSettings.Controls.Add(chkShowSkipButton);
            grpLoginSettings.Controls.Add(lblCookieName);
            grpLoginSettings.Controls.Add(txtCookieName);
            grpLoginSettings.Controls.Add(lblLoginUrl);
            grpLoginSettings.Controls.Add(txtLoginUrl);
            grpLoginSettings.Controls.Add(lblRegisterUrl);
            grpLoginSettings.Controls.Add(txtRegisterUrl);
            grpLoginSettings.Location = new Point(16, 19);
            grpLoginSettings.Margin = new Padding(2, 3, 2, 3);
            grpLoginSettings.Name = "grpLoginSettings";
            grpLoginSettings.Padding = new Padding(2, 3, 2, 3);
            grpLoginSettings.Size = new Size(319, 215);
            grpLoginSettings.TabIndex = 0;
            grpLoginSettings.TabStop = false;
            grpLoginSettings.Text = "登录引导设置";
            //
            // chkEnableLogin
            //
            chkEnableLogin.AutoSize = true;
            chkEnableLogin.Location = new Point(16, 28);
            chkEnableLogin.Margin = new Padding(2, 3, 2, 3);
            chkEnableLogin.Name = "chkEnableLogin";
            chkEnableLogin.Size = new Size(99, 21);
            chkEnableLogin.TabIndex = 0;
            chkEnableLogin.Text = "启用登录引导";
            chkEnableLogin.UseVisualStyleBackColor = true;
            chkEnableLogin.CheckedChanged += ChkEnableLogin_CheckedChanged;
            //
            // chkShowSkipButton
            //
            chkShowSkipButton.AutoSize = true;
            chkShowSkipButton.Location = new Point(16, 57);
            chkShowSkipButton.Margin = new Padding(2, 3, 2, 3);
            chkShowSkipButton.Name = "chkShowSkipButton";
            chkShowSkipButton.Size = new Size(99, 21);
            chkShowSkipButton.TabIndex = 1;
            chkShowSkipButton.Text = "显示跳过按钮";
            chkShowSkipButton.UseVisualStyleBackColor = true;
            //
            // lblCookieName
            //
            lblCookieName.Location = new Point(16, 90);
            lblCookieName.Margin = new Padding(2, 0, 2, 0);
            lblCookieName.Name = "lblCookieName";
            lblCookieName.Size = new Size(87, 24);
            lblCookieName.TabIndex = 2;
            lblCookieName.Text = "Cookie 名称:";
            lblCookieName.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtCookieName
            //
            txtCookieName.Location = new Point(107, 90);
            txtCookieName.Margin = new Padding(2, 3, 2, 3);
            txtCookieName.Name = "txtCookieName";
            txtCookieName.PlaceholderText = "例如: erolabsnickname";
            txtCookieName.Size = new Size(194, 23);
            txtCookieName.TabIndex = 3;
            //
            // lblLoginUrl
            //
            lblLoginUrl.Location = new Point(16, 123);
            lblLoginUrl.Margin = new Padding(2, 0, 2, 0);
            lblLoginUrl.Name = "lblLoginUrl";
            lblLoginUrl.Size = new Size(87, 24);
            lblLoginUrl.TabIndex = 4;
            lblLoginUrl.Text = "登录页路径:";
            lblLoginUrl.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtLoginUrl
            //
            txtLoginUrl.Location = new Point(107, 123);
            txtLoginUrl.Margin = new Padding(2, 3, 2, 3);
            txtLoginUrl.Name = "txtLoginUrl";
            txtLoginUrl.PlaceholderText = "例如: /cn/login.html";
            txtLoginUrl.Size = new Size(194, 23);
            txtLoginUrl.TabIndex = 5;
            //
            // lblRegisterUrl
            //
            lblRegisterUrl.Location = new Point(16, 156);
            lblRegisterUrl.Margin = new Padding(2, 0, 2, 0);
            lblRegisterUrl.Name = "lblRegisterUrl";
            lblRegisterUrl.Size = new Size(87, 24);
            lblRegisterUrl.TabIndex = 6;
            lblRegisterUrl.Text = "注册页地址:";
            lblRegisterUrl.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtRegisterUrl
            //
            txtRegisterUrl.Location = new Point(107, 156);
            txtRegisterUrl.Margin = new Padding(2, 3, 2, 3);
            txtRegisterUrl.Name = "txtRegisterUrl";
            txtRegisterUrl.PlaceholderText = "完整的注册页URL";
            txtRegisterUrl.Size = new Size(194, 23);
            txtRegisterUrl.TabIndex = 7;
            //
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Location = new Point(235, 302);
            btnSave.Margin = new Padding(2, 3, 2, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(58, 28);
            btnSave.TabIndex = 1;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            // 
            // btnApply
            // 
            btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnApply.Location = new Point(298, 302);
            btnApply.Margin = new Padding(2, 3, 2, 3);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(58, 28);
            btnApply.TabIndex = 2;
            btnApply.Text = "应用";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += BtnApply_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(172, 302);
            btnCancel.Margin = new Padding(2, 3, 2, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(58, 28);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(176, 94);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(104, 17);
            linkLabel1.TabIndex = 6;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "没有代理服务器？";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // SettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(376, 341);
            Controls.Add(btnSave);
            Controls.Add(btnApply);
            Controls.Add(btnCancel);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(2, 3, 2, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "设置";
            tabControl.ResumeLayout(false);
            tabNetwork.ResumeLayout(false);
            tabNetwork.PerformLayout();
            grpProxySettings.ResumeLayout(false);
            grpProxySettings.PerformLayout();
            tabLogging.ResumeLayout(false);
            tabLogging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxFileSize).EndInit();
            tabInterface.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numWindowHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWindowWidth).EndInit();
            tabLogin.ResumeLayout(false);
            grpLoginSettings.ResumeLayout(false);
            grpLoginSettings.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabNetwork;
        private System.Windows.Forms.Label lblBaseURL;
        private System.Windows.Forms.TextBox txtBaseURL;
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
        private System.Windows.Forms.CheckBox chkDebugMode;
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
        private System.Windows.Forms.TabPage tabLogin;
        private System.Windows.Forms.GroupBox grpLoginSettings;
        private System.Windows.Forms.CheckBox chkEnableLogin;
        private System.Windows.Forms.CheckBox chkShowSkipButton;
        private System.Windows.Forms.Label lblCookieName;
        private System.Windows.Forms.TextBox txtCookieName;
        private System.Windows.Forms.Label lblLoginUrl;
        private System.Windows.Forms.TextBox txtLoginUrl;
        private System.Windows.Forms.Label lblRegisterUrl;
        private System.Windows.Forms.TextBox txtRegisterUrl;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip toolTip1;
        private LinkLabel linkLabel1;
    }
}