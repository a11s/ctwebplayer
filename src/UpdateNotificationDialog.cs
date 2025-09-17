using System;
using System.Drawing;
using System.Windows.Forms;

namespace ctwebplayer
{
    /// <summary>
    /// 启动时的更新通知对话框
    /// </summary>
    public partial class UpdateNotificationDialog : Form
    {
        /// <summary>
        /// 对话框结果类型
        /// </summary>
        public enum UpdateChoice
        {
            UpdateNow,      // 立即更新
            SkipVersion,    // 跳过此版本
            RemindLater     // 稍后提醒
        }

        private CTWebPlayer.UpdateInfo _updateInfo;
        private UpdateChoice _userChoice = UpdateChoice.RemindLater;
        
        // 控件声明
        private Panel panelTop;
        private PictureBox pictureBoxIcon;
        private Label lblTitle;
        private Label lblMessage;
        private Panel panelContent;
        private Label lblCurrentVersion;
        private Label lblNewVersion;
        private Label lblFileSize;
        private Label lblReleaseDate;
        private CheckBox chkDontShowAgain;
        private TextBox txtReleaseNotes;
        private Panel panelBottom;
        private Button btnUpdateNow;
        private Button btnSkipVersion;
        private Button btnRemindLater;

        public UpdateNotificationDialog(CTWebPlayer.UpdateInfo updateInfo)
        {
            _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
            InitializeComponent();
            InitializeUI();
        }

        /// <summary>
        /// 获取用户选择
        /// </summary>
        public UpdateChoice UserChoice => _userChoice;

        /// <summary>
        /// 是否不再显示启动提示
        /// </summary>
        public bool DontShowAgain => chkDontShowAgain?.Checked ?? false;

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponent()
        {
            // 窗体设置
            this.Text = LanguageManager.Instance.GetString("UpdateNotification_Title");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(550, 500);
            this.Icon = SystemIcons.Information;

            // 顶部面板
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // 图标
            pictureBoxIcon = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = SystemIcons.Information.ToBitmap()
            };

            // 标题
            lblTitle = new Label
            {
                Location = new Point(70, 15),
                AutoSize = false,
                Size = new Size(450, 25),
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                Text = LanguageManager.Instance.GetString("UpdateNotification_NewVersionAvailable")
            };

            // 消息
            lblMessage = new Label
            {
                Location = new Point(70, 45),
                AutoSize = false,
                Size = new Size(450, 20),
                Text = LanguageManager.Instance.GetString("UpdateNotification_UpdateMessage")
            };

            panelTop.Controls.AddRange(new Control[] { pictureBoxIcon, lblTitle, lblMessage });

            // 内容面板
            panelContent = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(510, 310)
            };

            // 版本信息
            int yPos = 10;
            
            lblCurrentVersion = new Label
            {
                Location = new Point(10, yPos),
                AutoSize = false,
                Size = new Size(490, 20),
                Text = string.Format(LanguageManager.Instance.GetString("UpdateNotification_CurrentVersion"), 
                    CTWebPlayer.Version.FullVersion)
            };
            yPos += 25;

            lblNewVersion = new Label
            {
                Location = new Point(10, yPos),
                AutoSize = false,
                Size = new Size(490, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                ForeColor = Color.Green,
                Text = string.Format(LanguageManager.Instance.GetString("UpdateNotification_NewVersion"), 
                    _updateInfo.Version)
            };
            yPos += 25;

            lblFileSize = new Label
            {
                Location = new Point(10, yPos),
                AutoSize = false,
                Size = new Size(490, 20),
                Text = string.Format(LanguageManager.Instance.GetString("UpdateNotification_FileSize"), 
                    _updateInfo.GetFormattedFileSize())
            };
            yPos += 25;

            lblReleaseDate = new Label
            {
                Location = new Point(10, yPos),
                AutoSize = false,
                Size = new Size(490, 20),
                Text = string.Format(LanguageManager.Instance.GetString("UpdateNotification_ReleaseDate"), 
                    _updateInfo.PublishedAt.ToString("yyyy-MM-dd HH:mm"))
            };
            yPos += 35;

            // 更新说明标签
            var lblReleaseNotesTitle = new Label
            {
                Location = new Point(10, yPos),
                AutoSize = false,
                Size = new Size(490, 20),
                Text = LanguageManager.Instance.GetString("UpdateNotification_ReleaseNotes"),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            yPos += 25;

            // 更新说明文本框
            txtReleaseNotes = new TextBox
            {
                Location = new Point(10, yPos),
                Size = new Size(490, 120),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Text = _updateInfo.ReleaseNotes
            };
            yPos += 130;

            // 不再提示复选框
            chkDontShowAgain = new CheckBox
            {
                Location = new Point(10, yPos),
                AutoSize = true,
                Text = LanguageManager.Instance.GetString("UpdateNotification_DontShowAgain")
            };

            panelContent.Controls.AddRange(new Control[] 
            { 
                lblCurrentVersion, 
                lblNewVersion, 
                lblFileSize, 
                lblReleaseDate,
                lblReleaseNotesTitle,
                txtReleaseNotes,
                chkDontShowAgain
            });

            // 底部按钮面板
            panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60
            };

            // 立即更新按钮（主要操作）
            btnUpdateNow = new Button
            {
                Location = new Point(210, 15),
                Size = new Size(100, 30),
                Text = LanguageManager.Instance.GetString("UpdateNotification_UpdateNow"),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            btnUpdateNow.FlatAppearance.BorderSize = 0;
            btnUpdateNow.Click += BtnUpdateNow_Click;

            // 跳过此版本按钮
            btnSkipVersion = new Button
            {
                Location = new Point(320, 15),
                Size = new Size(100, 30),
                Text = LanguageManager.Instance.GetString("UpdateNotification_SkipVersion"),
                FlatStyle = FlatStyle.Flat
            };
            btnSkipVersion.Click += BtnSkipVersion_Click;

            // 稍后提醒按钮
            btnRemindLater = new Button
            {
                Location = new Point(430, 15),
                Size = new Size(100, 30),
                Text = LanguageManager.Instance.GetString("UpdateNotification_RemindLater"),
                FlatStyle = FlatStyle.Flat
            };
            btnRemindLater.Click += BtnRemindLater_Click;

            panelBottom.Controls.AddRange(new Control[] { btnUpdateNow, btnSkipVersion, btnRemindLater });

            // 添加到窗体
            this.Controls.AddRange(new Control[] { panelTop, panelContent, panelBottom });

            // 设置默认按钮
            this.AcceptButton = btnUpdateNow;
            this.CancelButton = btnRemindLater;
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 应用语言设置
            LanguageManager.Instance.ApplyToForm(this);
            
            // 如果是强制更新，禁用跳过按钮
            if (_updateInfo.IsUpdateMandatory())
            {
                btnSkipVersion.Enabled = false;
                btnRemindLater.Enabled = false;
                chkDontShowAgain.Visible = false;
                
                lblMessage.Text = LanguageManager.Instance.GetString("UpdateNotification_MandatoryUpdate");
                lblMessage.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// 立即更新按钮点击事件
        /// </summary>
        private void BtnUpdateNow_Click(object sender, EventArgs e)
        {
            _userChoice = UpdateChoice.UpdateNow;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 跳过此版本按钮点击事件
        /// </summary>
        private void BtnSkipVersion_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                LanguageManager.Instance.GetString("UpdateNotification_ConfirmSkip"),
                LanguageManager.Instance.GetString("UpdateNotification_ConfirmSkipTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _userChoice = UpdateChoice.SkipVersion;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// 稍后提醒按钮点击事件
        /// </summary>
        private void BtnRemindLater_Click(object sender, EventArgs e)
        {
            _userChoice = UpdateChoice.RemindLater;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 显示更新通知对话框
        /// </summary>
        public static UpdateChoice ShowUpdateNotification(IWin32Window owner, CTWebPlayer.UpdateInfo updateInfo, out bool dontShowAgain)
        {
            using (var dialog = new UpdateNotificationDialog(updateInfo))
            {
                var result = dialog.ShowDialog(owner);
                dontShowAgain = dialog.DontShowAgain;
                return dialog.UserChoice;
            }
        }
    }
}