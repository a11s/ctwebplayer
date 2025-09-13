namespace ctwebplayer
{
    partial class AboutForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.txtLicense = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblLicenseTitle = new System.Windows.Forms.Label();
            this.lblImportantNotice = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(146, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "CTWebPlayer";
            //
            // lblVersion
            //
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(14, 45);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(222, 13);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Unity3D WebPlayer 专属浏览器 v1.0.0";
            //
            // txtLicense
            //
            this.txtLicense.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLicense.Location = new System.Drawing.Point(12, 140);
            this.txtLicense.Multiline = true;
            this.txtLicense.Name = "txtLicense";
            this.txtLicense.ReadOnly = true;
            this.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLicense.Size = new System.Drawing.Size(560, 250);
            this.txtLicense.TabIndex = 2;
            //
            // btnOK
            //
            this.btnOK.Location = new System.Drawing.Point(497, 406);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // lblLicenseTitle
            //
            this.lblLicenseTitle.AutoSize = true;
            this.lblLicenseTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblLicenseTitle.Location = new System.Drawing.Point(12, 120);
            this.lblLicenseTitle.Name = "lblLicenseTitle";
            this.lblLicenseTitle.Size = new System.Drawing.Size(65, 17);
            this.lblLicenseTitle.TabIndex = 4;
            this.lblLicenseTitle.Text = "许可证：";
            //
            // lblImportantNotice
            //
            this.lblImportantNotice.AutoSize = true;
            this.lblImportantNotice.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblImportantNotice.ForeColor = System.Drawing.Color.Red;
            this.lblImportantNotice.Location = new System.Drawing.Point(14, 70);
            this.lblImportantNotice.Name = "lblImportantNotice";
            this.lblImportantNotice.Size = new System.Drawing.Size(377, 30);
            this.lblImportantNotice.TabIndex = 5;
            this.lblImportantNotice.Text = "?? 重要提示：任何修改版本必须改名并声明与原作者无关！\r\n" +
                "详情请仔细阅读下方许可证条款。";
            //
            // AboutForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 441);
            this.Controls.Add(this.lblImportantNotice);
            this.Controls.Add(this.lblLicenseTitle);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtLicense);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "关于 CTWebPlayer";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TextBox txtLicense;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblLicenseTitle;
        private System.Windows.Forms.Label lblImportantNotice;
    }
}
