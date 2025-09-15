namespace ctwebplayer
{
    partial class LoginDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblMessage = new System.Windows.Forms.Label();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.btnHasAccount = new System.Windows.Forms.Button();
            this.btnNoAccount = new System.Windows.Forms.Button();
            this.btnSkip = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMessage.Location = new System.Drawing.Point(90, 30);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(320, 60);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "检测到您尚未登录。\r\n您是否已有账号？";
            this.lblMessage.Tag = "LoginDialog_lblMessage";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Location = new System.Drawing.Point(30, 30);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxIcon.TabIndex = 1;
            this.pictureBoxIcon.TabStop = false;
            this.pictureBoxIcon.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            // 
            // btnHasAccount
            // 
            this.btnHasAccount.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnHasAccount.Location = new System.Drawing.Point(90, 110);
            this.btnHasAccount.Name = "btnHasAccount";
            this.btnHasAccount.Size = new System.Drawing.Size(100, 35);
            this.btnHasAccount.TabIndex = 2;
            this.btnHasAccount.Text = "已有账号";
            this.btnHasAccount.Tag = "LoginDialog_btnHasAccount";
            this.btnHasAccount.UseVisualStyleBackColor = true;
            // 
            // btnNoAccount
            // 
            this.btnNoAccount.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnNoAccount.Location = new System.Drawing.Point(210, 110);
            this.btnNoAccount.Name = "btnNoAccount";
            this.btnNoAccount.Size = new System.Drawing.Size(100, 35);
            this.btnNoAccount.TabIndex = 3;
            this.btnNoAccount.Text = "新注册";
            this.btnNoAccount.Tag = "LoginDialog_btnNoAccount";
            this.btnNoAccount.UseVisualStyleBackColor = true;
            // 
            // btnSkip
            // 
            this.btnSkip.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSkip.Location = new System.Drawing.Point(330, 110);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(80, 35);
            this.btnSkip.TabIndex = 4;
            this.btnSkip.Text = "跳过";
            this.btnSkip.Tag = "LoginDialog_btnSkip";
            this.btnSkip.UseVisualStyleBackColor = true;
            // 
            // LoginDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 170);
            this.Controls.Add(this.btnSkip);
            this.Controls.Add(this.btnNoAccount);
            this.Controls.Add(this.btnHasAccount);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.lblMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "登录引导";
            this.Tag = "LoginDialog_Title";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Button btnHasAccount;
        private System.Windows.Forms.Button btnNoAccount;
        private System.Windows.Forms.Button btnSkip;
    }
}