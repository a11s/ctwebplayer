namespace ctwebplayer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            toolStrip1 = new ToolStrip();
            btnHome = new ToolStripButton();
            btnBack = new ToolStripButton();
            btnForward = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            txtAddress = new ToolStripTextBox();
            btnGo = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnSettings = new ToolStripButton();
            btnUtilities = new ToolStripDropDownButton();
            toggleFullScreenMenuItem = new ToolStripMenuItem();
            toggleMuteMenuItem = new ToolStripMenuItem();
            screenshotMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            logoutMenuItem = new ToolStripMenuItem();
            forumMenuItem = new ToolStripMenuItem();
            discordMenuItem = new ToolStripMenuItem();
            githubMenuItem = new ToolStripMenuItem();
            mobileDownloadMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            checkUpdateMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            aboutMenuItem = new ToolStripMenuItem();
            donateToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            progressBar = new ToolStripProgressBar();
            webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView2).BeginInit();
            SuspendLayout();
            //
            // toolStrip1
            //
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnHome, btnBack, btnForward, btnRefresh, toolStripSeparator1, txtAddress, btnGo, toolStripSeparator2, btnSettings, btnUtilities });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1374, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            //
            // btnHome
            //
            btnHome.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(36, 22);
            btnHome.Text = "主页";
            btnHome.Tag = "Form1_toolStrip1_btnHome";
            btnHome.Click += btnHome_Click;
            // 
            // btnBack
            // 
            btnBack.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(36, 22);
            btnBack.Text = "后退";
            btnBack.Tag = "Form1_toolStrip1_btnBack";
            btnBack.Click += btnBack_Click;
            // 
            // btnForward
            // 
            btnForward.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(36, 22);
            btnForward.Text = "前进";
            btnForward.Tag = "Form1_toolStrip1_btnForward";
            btnForward.Click += btnForward_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(36, 22);
            btnRefresh.Text = "刷新";
            btnRefresh.Tag = "Form1_toolStrip1_btnRefresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // txtAddress
            //
            txtAddress.AutoSize = false;
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(600, 25);
            txtAddress.Tag = "Form1_toolStrip1_txtAddress";
            txtAddress.KeyDown += txtAddress_KeyDown;
            // 
            // btnGo
            // 
            btnGo.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnGo.Name = "btnGo";
            btnGo.Size = new Size(36, 22);
            btnGo.Text = "转到";
            btnGo.Tag = "Form1_toolStrip1_btnGo";
            btnGo.Click += btnGo_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // btnSettings
            // 
            btnSettings.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(36, 22);
            btnSettings.Text = "设置";
            btnSettings.Tag = "Form1_toolStrip1_btnSettings";
            btnSettings.Click += btnSettings_Click;
            //
            // btnUtilities
            //
            btnUtilities.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnUtilities.DropDownItems.AddRange(new ToolStripItem[] { toggleFullScreenMenuItem, toggleMuteMenuItem, screenshotMenuItem, toolStripSeparator3, logoutMenuItem, forumMenuItem, discordMenuItem, githubMenuItem, mobileDownloadMenuItem, toolStripSeparator5, checkUpdateMenuItem, toolStripSeparator4, aboutMenuItem, donateToolStripMenuItem });
            btnUtilities.Name = "btnUtilities";
            btnUtilities.Size = new Size(69, 22);
            btnUtilities.Text = "实用工具";
            btnUtilities.Tag = "Form1_toolStrip1_btnUtilities";
            //
            // toggleFullScreenMenuItem
            //
            toggleFullScreenMenuItem.Name = "toggleFullScreenMenuItem";
            toggleFullScreenMenuItem.ShortcutKeys = Keys.F11;
            toggleFullScreenMenuItem.Size = new Size(171, 22);
            toggleFullScreenMenuItem.Text = "全屏切换 (F11)";
            toggleFullScreenMenuItem.Tag = "Form1_toolStrip1_btnUtilities_toggleFullScreenMenuItem";
            toggleFullScreenMenuItem.Click += toggleFullScreenMenuItem_Click;
            //
            // toggleMuteMenuItem
            //
            toggleMuteMenuItem.Name = "toggleMuteMenuItem";
            toggleMuteMenuItem.ShortcutKeys = Keys.F4;
            toggleMuteMenuItem.Size = new Size(171, 22);
            toggleMuteMenuItem.Text = "静音切换 (F4)";
            toggleMuteMenuItem.Tag = "Form1_toolStrip1_btnUtilities_toggleMuteMenuItem";
            toggleMuteMenuItem.Click += toggleMuteMenuItem_Click;
            //
            // screenshotMenuItem
            //
            screenshotMenuItem.Name = "screenshotMenuItem";
            screenshotMenuItem.ShortcutKeys = Keys.F10;
            screenshotMenuItem.Size = new Size(171, 22);
            screenshotMenuItem.Text = "截图 (F10)";
            screenshotMenuItem.Tag = "Form1_Tool_Screenshot";
            screenshotMenuItem.Click += screenshotMenuItem_Click;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(168, 6);
            //
            // logoutMenuItem
            //
            logoutMenuItem.Name = "logoutMenuItem";
            logoutMenuItem.Size = new Size(171, 22);
            logoutMenuItem.Text = "退出登录";
            logoutMenuItem.Tag = "Form1_toolStrip1_btnUtilities_logoutMenuItem";
            logoutMenuItem.Click += logoutMenuItem_Click;
            //
            // forumMenuItem
            //
            forumMenuItem.Name = "forumMenuItem";
            forumMenuItem.Size = new Size(171, 22);
            forumMenuItem.Text = "官方讨论区";
            forumMenuItem.Tag = "Form1_toolStrip1_btnUtilities_forumMenuItem";
            forumMenuItem.Click += forumMenuItem_Click;
            //
            // discordMenuItem
            //
            discordMenuItem.Name = "discordMenuItem";
            discordMenuItem.Size = new Size(171, 22);
            discordMenuItem.Text = "官方Discord";
            discordMenuItem.Tag = "Form1_toolStrip1_btnUtilities_discordMenuItem";
            discordMenuItem.Click += discordMenuItem_Click;
            //
            // githubMenuItem
            //
            githubMenuItem.Name = "githubMenuItem";
            githubMenuItem.Size = new Size(171, 22);
            githubMenuItem.Text = "GitHub 源码";
            githubMenuItem.Tag = "Form1_toolStrip1_btnUtilities_githubMenuItem";
            githubMenuItem.Click += githubMenuItem_Click;
            //
            // mobileDownloadMenuItem
            //
            mobileDownloadMenuItem.Name = "mobileDownloadMenuItem";
            mobileDownloadMenuItem.Size = new Size(171, 22);
            mobileDownloadMenuItem.Text = "手机版下载";
            mobileDownloadMenuItem.Tag = "Form1_toolStrip1_btnUtilities_mobileDownloadMenuItem";
            mobileDownloadMenuItem.Click += mobileDownloadMenuItem_Click;
            //
            // toolStripSeparator5
            //
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(168, 6);
            //
            // checkUpdateMenuItem
            //
            checkUpdateMenuItem.Name = "checkUpdateMenuItem";
            checkUpdateMenuItem.Size = new Size(171, 22);
            checkUpdateMenuItem.Text = "检查更新";
            checkUpdateMenuItem.Tag = "Form1_toolStrip1_btnUtilities_checkUpdateMenuItem";
            checkUpdateMenuItem.Click += checkUpdateMenuItem_Click;
            //
            // toolStripSeparator4
            //
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(168, 6);
            //
            // aboutMenuItem
            //
            aboutMenuItem.Name = "aboutMenuItem";
            aboutMenuItem.Size = new Size(171, 22);
            aboutMenuItem.Text = "关于";
            aboutMenuItem.Tag = "Form1_toolStrip1_btnUtilities_aboutMenuItem";
            aboutMenuItem.Click += aboutMenuItem_Click;
            //
            // donateToolStripMenuItem
            //
            donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            donateToolStripMenuItem.Size = new Size(171, 22);
            donateToolStripMenuItem.Text = "捐助";
            donateToolStripMenuItem.Tag = "Form1_toolStrip1_btnUtilities_donateToolStripMenuItem";
            donateToolStripMenuItem.Click += donateToolStripMenuItem_Click;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
            statusStrip1.Location = new Point(0, 842);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1374, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(1359, 17);
            statusLabel.Spring = true;
            statusLabel.Text = "就绪";
            statusLabel.Tag = "Form1_statusStrip1_statusLabel";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(100, 18);
            progressBar.Visible = false;
            //
            // webView2
            //
            webView2.AllowExternalDrop = true;
            webView2.CreationProperties = null;
            webView2.DefaultBackgroundColor = Color.White;
            webView2.Dock = DockStyle.Fill;
            webView2.Location = new Point(0, 25);
            webView2.Name = "webView2";
            webView2.Size = new Size(1374, 817);
            webView2.TabIndex = 2;
            webView2.ZoomFactor = 1D;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1374, 864);
            Controls.Add(webView2);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "Form1";
            StartPosition = FormStartPosition.Manual;
            Text = "Unity3D WebPlayer 专属浏览器";
            Tag = "Form1_Title";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripButton btnBack;
        private System.Windows.Forms.ToolStripButton btnForward;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox txtAddress;
        private System.Windows.Forms.ToolStripButton btnGo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripDropDownButton btnUtilities;
        private System.Windows.Forms.ToolStripMenuItem toggleFullScreenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleMuteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem screenshotMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem logoutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forumMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discordMenuItem;
        private System.Windows.Forms.ToolStripMenuItem githubMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mobileDownloadMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem checkUpdateMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem donateToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
    }
}
