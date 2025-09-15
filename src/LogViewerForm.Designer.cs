namespace ctwebplayer
{
    partial class LogViewerForm
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
            this.topPanel = new System.Windows.Forms.Panel();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.chkAutoRefresh = new System.Windows.Forms.CheckBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblFileInfo = new System.Windows.Forms.Label();
            this.txtLogContent = new System.Windows.Forms.TextBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.topPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            //
            this.topPanel.Controls.Add(this.lblLogLevel);
            this.topPanel.Controls.Add(this.cmbLogLevel);
            this.topPanel.Controls.Add(this.chkAutoRefresh);
            this.topPanel.Controls.Add(this.btnRefresh);
            this.topPanel.Controls.Add(this.btnClear);
            this.topPanel.Controls.Add(this.lblFileInfo);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 5);
            this.topPanel.Size = new System.Drawing.Size(584, 50);
            this.topPanel.TabIndex = 0;
            this.topPanel.Tag = "LogViewerForm_topPanel";
            // 
            // lblLogLevel
            //
            this.lblLogLevel.Location = new System.Drawing.Point(10, 15);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(90, 25);
            this.lblLogLevel.TabIndex = 0;
            this.lblLogLevel.Tag = "LogViewerForm_topPanel_lblLogLevel";
            this.lblLogLevel.Text = "日志级别过滤:";
            this.lblLogLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.cmbLogLevel.Location = new System.Drawing.Point(105, 12);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.SelectedIndex = 0;
            this.cmbLogLevel.Size = new System.Drawing.Size(100, 25);
            this.cmbLogLevel.TabIndex = 1;
            this.cmbLogLevel.Tag = "LogViewerForm_topPanel_cmbLogLevel";
            this.cmbLogLevel.SelectedIndexChanged += new System.EventHandler(this.CmbLogLevel_SelectedIndexChanged);
            // 
            // chkAutoRefresh
            //
            this.chkAutoRefresh.AutoSize = true;
            this.chkAutoRefresh.Checked = true;
            this.chkAutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoRefresh.Location = new System.Drawing.Point(220, 15);
            this.chkAutoRefresh.Name = "chkAutoRefresh";
            this.chkAutoRefresh.Size = new System.Drawing.Size(80, 25);
            this.chkAutoRefresh.TabIndex = 2;
            this.chkAutoRefresh.Tag = "LogViewerForm_topPanel_chkAutoRefresh";
            this.chkAutoRefresh.Text = "自动刷新";
            this.chkAutoRefresh.UseVisualStyleBackColor = true;
            this.chkAutoRefresh.CheckedChanged += new System.EventHandler(this.ChkAutoRefresh_CheckedChanged);
            // 
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(310, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 28);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Tag = "LogViewerForm_topPanel_btnRefresh";
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnClear
            //
            this.btnClear.Location = new System.Drawing.Point(390, 10);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 28);
            this.btnClear.TabIndex = 4;
            this.btnClear.Tag = "LogViewerForm_topPanel_btnClear";
            this.btnClear.Text = "清空日志";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // lblFileInfo
            //
            this.lblFileInfo.ForeColor = System.Drawing.Color.Gray;
            this.lblFileInfo.Location = new System.Drawing.Point(480, 15);
            this.lblFileInfo.Name = "lblFileInfo";
            this.lblFileInfo.Size = new System.Drawing.Size(300, 25);
            this.lblFileInfo.TabIndex = 5;
            this.lblFileInfo.Tag = "LogViewerForm_topPanel_lblFileInfo";
            this.lblFileInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLogContent
            //
            this.txtLogContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtLogContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogContent.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLogContent.ForeColor = System.Drawing.Color.LightGray;
            this.txtLogContent.Location = new System.Drawing.Point(0, 50);
            this.txtLogContent.Multiline = true;
            this.txtLogContent.Name = "txtLogContent";
            this.txtLogContent.ReadOnly = true;
            this.txtLogContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogContent.Size = new System.Drawing.Size(584, 321);
            this.txtLogContent.TabIndex = 1;
            this.txtLogContent.Tag = "LogViewerForm_txtLogContent";
            // 
            // bottomPanel
            //
            this.bottomPanel.Controls.Add(this.btnClose);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 371);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(10, 5, 10, 10);
            this.bottomPanel.Size = new System.Drawing.Size(584, 40);
            this.bottomPanel.TabIndex = 2;
            this.bottomPanel.Tag = "LogViewerForm_bottomPanel";
            // 
            // btnClose
            //
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Location = new System.Drawing.Point(494, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.TabIndex = 0;
            this.btnClose.Tag = "LogViewerForm_bottomPanel_btnClose";
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // LogViewerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.txtLogContent);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.topPanel);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "LogViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "LogViewerForm_Title";
            this.Text = "日志查看器";
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.CheckBox chkAutoRefresh;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblFileInfo;
        private System.Windows.Forms.TextBox txtLogContent;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button btnClose;
    }
}