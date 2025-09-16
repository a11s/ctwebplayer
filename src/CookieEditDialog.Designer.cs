namespace ctwebplayer
{
    partial class CookieEditDialog
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
            this.components = new System.ComponentModel.Container();
            this.groupBoxProperties = new System.Windows.Forms.GroupBox();
            this.buttonSetSessionCookie = new System.Windows.Forms.Button();
            this.dateTimePickerExpires = new System.Windows.Forms.DateTimePicker();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.textBoxValue = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxDomain = new System.Windows.Forms.TextBox();
            this.labelExpires = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.labelValue = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.labelDomain = new System.Windows.Forms.Label();
            this.groupBoxSecurity = new System.Windows.Forms.GroupBox();
            this.comboBoxSameSite = new System.Windows.Forms.ComboBox();
            this.labelSameSite = new System.Windows.Forms.Label();
            this.checkBoxSecure = new System.Windows.Forms.CheckBox();
            this.checkBoxHttpOnly = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBoxProperties.SuspendLayout();
            this.groupBoxSecurity.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxProperties
            // 
            this.groupBoxProperties.Controls.Add(this.buttonSetSessionCookie);
            this.groupBoxProperties.Controls.Add(this.dateTimePickerExpires);
            this.groupBoxProperties.Controls.Add(this.textBoxPath);
            this.groupBoxProperties.Controls.Add(this.textBoxValue);
            this.groupBoxProperties.Controls.Add(this.textBoxName);
            this.groupBoxProperties.Controls.Add(this.textBoxDomain);
            this.groupBoxProperties.Controls.Add(this.labelExpires);
            this.groupBoxProperties.Controls.Add(this.labelPath);
            this.groupBoxProperties.Controls.Add(this.labelValue);
            this.groupBoxProperties.Controls.Add(this.labelName);
            this.groupBoxProperties.Controls.Add(this.labelDomain);
            this.groupBoxProperties.Location = new System.Drawing.Point(12, 12);
            this.groupBoxProperties.Name = "groupBoxProperties";
            this.groupBoxProperties.Size = new System.Drawing.Size(460, 210);
            this.groupBoxProperties.TabIndex = 0;
            this.groupBoxProperties.TabStop = false;
            this.groupBoxProperties.Text = "Cookie属性";
            // 
            // buttonSetSessionCookie
            // 
            this.buttonSetSessionCookie.Location = new System.Drawing.Point(350, 171);
            this.buttonSetSessionCookie.Name = "buttonSetSessionCookie";
            this.buttonSetSessionCookie.Size = new System.Drawing.Size(100, 23);
            this.buttonSetSessionCookie.TabIndex = 11;
            this.buttonSetSessionCookie.Text = "会话Cookie";
            this.buttonSetSessionCookie.UseVisualStyleBackColor = true;
            // 
            // dateTimePickerExpires
            // 
            this.dateTimePickerExpires.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerExpires.Location = new System.Drawing.Point(80, 171);
            this.dateTimePickerExpires.Name = "dateTimePickerExpires";
            this.dateTimePickerExpires.ShowCheckBox = true;
            this.dateTimePickerExpires.Size = new System.Drawing.Size(264, 23);
            this.dateTimePickerExpires.TabIndex = 10;
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new System.Drawing.Point(80, 139);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(370, 23);
            this.textBoxPath.TabIndex = 8;
            this.textBoxPath.Text = "/";
            // 
            // textBoxValue
            // 
            this.textBoxValue.Location = new System.Drawing.Point(80, 91);
            this.textBoxValue.Multiline = true;
            this.textBoxValue.Name = "textBoxValue";
            this.textBoxValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxValue.Size = new System.Drawing.Size(370, 40);
            this.textBoxValue.TabIndex = 6;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(80, 59);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(370, 23);
            this.textBoxName.TabIndex = 4;
            // 
            // textBoxDomain
            // 
            this.textBoxDomain.Location = new System.Drawing.Point(80, 27);
            this.textBoxDomain.Name = "textBoxDomain";
            this.textBoxDomain.Size = new System.Drawing.Size(370, 23);
            this.textBoxDomain.TabIndex = 2;
            // 
            // labelExpires
            // 
            this.labelExpires.AutoSize = true;
            this.labelExpires.Location = new System.Drawing.Point(15, 175);
            this.labelExpires.Name = "labelExpires";
            this.labelExpires.Size = new System.Drawing.Size(59, 15);
            this.labelExpires.TabIndex = 9;
            this.labelExpires.Text = "过期时间:";
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(39, 142);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(35, 15);
            this.labelPath.TabIndex = 7;
            this.labelPath.Text = "路径:";
            // 
            // labelValue
            // 
            this.labelValue.AutoSize = true;
            this.labelValue.Location = new System.Drawing.Point(51, 94);
            this.labelValue.Name = "labelValue";
            this.labelValue.Size = new System.Drawing.Size(23, 15);
            this.labelValue.TabIndex = 5;
            this.labelValue.Text = "值:";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(39, 62);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 15);
            this.labelName.TabIndex = 3;
            this.labelName.Text = "名称:";
            // 
            // labelDomain
            // 
            this.labelDomain.AutoSize = true;
            this.labelDomain.Location = new System.Drawing.Point(39, 30);
            this.labelDomain.Name = "labelDomain";
            this.labelDomain.Size = new System.Drawing.Size(35, 15);
            this.labelDomain.TabIndex = 1;
            this.labelDomain.Text = "域名:";
            // 
            // groupBoxSecurity
            // 
            this.groupBoxSecurity.Controls.Add(this.comboBoxSameSite);
            this.groupBoxSecurity.Controls.Add(this.labelSameSite);
            this.groupBoxSecurity.Controls.Add(this.checkBoxSecure);
            this.groupBoxSecurity.Controls.Add(this.checkBoxHttpOnly);
            this.groupBoxSecurity.Location = new System.Drawing.Point(12, 228);
            this.groupBoxSecurity.Name = "groupBoxSecurity";
            this.groupBoxSecurity.Size = new System.Drawing.Size(460, 110);
            this.groupBoxSecurity.TabIndex = 1;
            this.groupBoxSecurity.TabStop = false;
            this.groupBoxSecurity.Text = "安全设置";
            // 
            // comboBoxSameSite
            // 
            this.comboBoxSameSite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSameSite.FormattingEnabled = true;
            this.comboBoxSameSite.Location = new System.Drawing.Point(80, 73);
            this.comboBoxSameSite.Name = "comboBoxSameSite";
            this.comboBoxSameSite.Size = new System.Drawing.Size(150, 23);
            this.comboBoxSameSite.TabIndex = 3;
            // 
            // labelSameSite
            // 
            this.labelSameSite.AutoSize = true;
            this.labelSameSite.Location = new System.Drawing.Point(18, 76);
            this.labelSameSite.Name = "labelSameSite";
            this.labelSameSite.Size = new System.Drawing.Size(56, 15);
            this.labelSameSite.TabIndex = 2;
            this.labelSameSite.Text = "SameSite:";
            // 
            // checkBoxSecure
            // 
            this.checkBoxSecure.AutoSize = true;
            this.checkBoxSecure.Location = new System.Drawing.Point(120, 35);
            this.checkBoxSecure.Name = "checkBoxSecure";
            this.checkBoxSecure.Size = new System.Drawing.Size(62, 19);
            this.checkBoxSecure.TabIndex = 1;
            this.checkBoxSecure.Text = "Secure";
            this.checkBoxSecure.UseVisualStyleBackColor = true;
            // 
            // checkBoxHttpOnly
            // 
            this.checkBoxHttpOnly.AutoSize = true;
            this.checkBoxHttpOnly.Location = new System.Drawing.Point(18, 35);
            this.checkBoxHttpOnly.Name = "checkBoxHttpOnly";
            this.checkBoxHttpOnly.Size = new System.Drawing.Size(75, 19);
            this.checkBoxHttpOnly.TabIndex = 0;
            this.checkBoxHttpOnly.Text = "HttpOnly";
            this.checkBoxHttpOnly.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(316, 354);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 25);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(397, 354);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 25);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // CookieEditDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 391);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxSecurity);
            this.Controls.Add(this.groupBoxProperties);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CookieEditDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "编辑Cookie";
            this.groupBoxProperties.ResumeLayout(false);
            this.groupBoxProperties.PerformLayout();
            this.groupBoxSecurity.ResumeLayout(false);
            this.groupBoxSecurity.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxProperties;
        private System.Windows.Forms.Button buttonSetSessionCookie;
        private System.Windows.Forms.DateTimePicker dateTimePickerExpires;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.TextBox textBoxValue;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxDomain;
        private System.Windows.Forms.Label labelExpires;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.Label labelValue;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelDomain;
        private System.Windows.Forms.GroupBox groupBoxSecurity;
        private System.Windows.Forms.ComboBox comboBoxSameSite;
        private System.Windows.Forms.Label labelSameSite;
        private System.Windows.Forms.CheckBox checkBoxSecure;
        private System.Windows.Forms.CheckBox checkBoxHttpOnly;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}