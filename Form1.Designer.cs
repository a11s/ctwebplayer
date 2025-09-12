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
            toolStripSeparator3 = new ToolStripSeparator();
            aboutMenuItem = new ToolStripMenuItem();
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
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnBack, btnForward, btnRefresh, toolStripSeparator1, txtAddress, btnGo, toolStripSeparator2, btnSettings, btnUtilities });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1374, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnBack
            // 
            btnBack.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(36, 22);
            btnBack.Text = "后退";
            btnBack.Click += btnBack_Click;
            // 
            // btnForward
            // 
            btnForward.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(36, 22);
            btnForward.Text = "前进";
            btnForward.Click += btnForward_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(36, 22);
            btnRefresh.Text = "刷新";
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
            txtAddress.KeyDown += txtAddress_KeyDown;
            // 
            // btnGo
            // 
            btnGo.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnGo.Name = "btnGo";
            btnGo.Size = new Size(36, 22);
            btnGo.Text = "转到";
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
            btnSettings.Click += btnSettings_Click;
            //
            // btnUtilities
            //
            btnUtilities.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnUtilities.DropDownItems.AddRange(new ToolStripItem[] { toggleFullScreenMenuItem, toggleMuteMenuItem, toolStripSeparator3, aboutMenuItem });
            btnUtilities.Name = "btnUtilities";
            btnUtilities.Size = new Size(69, 22);
            btnUtilities.Text = "实用工具";
            //
            // toggleFullScreenMenuItem
            //
            toggleFullScreenMenuItem.Name = "toggleFullScreenMenuItem";
            toggleFullScreenMenuItem.ShortcutKeys = Keys.F11;
            toggleFullScreenMenuItem.Size = new Size(171, 22);
            toggleFullScreenMenuItem.Text = "全屏切换 (F11)";
            toggleFullScreenMenuItem.Click += toggleFullScreenMenuItem_Click;
            //
            // toggleMuteMenuItem
            //
            toggleMuteMenuItem.Name = "toggleMuteMenuItem";
            toggleMuteMenuItem.ShortcutKeys = Keys.F4;
            toggleMuteMenuItem.Size = new Size(171, 22);
            toggleMuteMenuItem.Text = "静音切换 (F4)";
            toggleMuteMenuItem.Click += toggleMuteMenuItem_Click;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(168, 6);
            //
            // aboutMenuItem
            //
            aboutMenuItem.Name = "aboutMenuItem";
            aboutMenuItem.Size = new Size(171, 22);
            aboutMenuItem.Text = "关于";
            aboutMenuItem.Click += aboutMenuItem_Click;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
    }
}
