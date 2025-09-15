using System;
using System.Drawing;
using System.Windows.Forms;

namespace ctwebplayer
{
    /// <summary>
    /// 登录引导对话框
    /// </summary>
    public partial class LoginDialog : Form
    {
        /// <summary>
        /// 对话框结果类型
        /// </summary>
        public enum DialogResultType
        {
            HasAccount,   // 已有账号
            NoAccount,    // 新注册
            Skip          // 跳过
        }

        /// <summary>
        /// 用户选择的结果
        /// </summary>
        public DialogResultType UserChoice { get; private set; }

        private readonly bool _skipEnabled;

        /// <summary>
        /// 初始化登录对话框
        /// </summary>
        /// <param name="skipEnabled">是否显示跳过按钮</param>
        public LoginDialog(bool skipEnabled = false)
        {
            _skipEnabled = skipEnabled;
            InitializeComponent();
            LanguageManager.Instance.ApplyToForm(this);
            InitializeUI();
            LanguageManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 设置窗体属性
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = this.Owner?.Icon;

            // 根据是否启用跳过按钮调整布局
            if (!_skipEnabled)
            {
                btnSkip.Visible = false;
                // 调整窗体高度
                this.Height -= 10;
            }

            // 设置按钮事件
            btnHasAccount.Click += (s, e) =>
            {
                UserChoice = DialogResultType.HasAccount;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnNoAccount.Click += (s, e) =>
            {
                UserChoice = DialogResultType.NoAccount;
                DialogResult = DialogResult.OK;
                Close();
            };

            if (_skipEnabled)
            {
                btnSkip.Click += (s, e) =>
                {
                    UserChoice = DialogResultType.Skip;
                    DialogResult = DialogResult.Cancel;
                    Close();
                };
            }

            // 设置ESC键行为
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (_skipEnabled)
                    {
                        UserChoice = DialogResultType.Skip;
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                }
            };
        }

        /// <summary>
        /// 显示登录对话框并返回用户选择
        /// </summary>
        /// <param name="owner">父窗口</param>
        /// <param name="skipEnabled">是否启用跳过按钮</param>
        /// <returns>用户选择的结果</returns>
        public static DialogResultType ShowLoginDialog(IWin32Window owner, bool skipEnabled = false)
        {
            using (var dialog = new LoginDialog(skipEnabled))
            {
                dialog.ShowDialog(owner);
                return dialog.UserChoice;
            }
        }
        private void OnLanguageChanged(object sender, EventArgs e)
        {
            LanguageManager.Instance.ApplyToForm(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.Instance.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }

    }
}