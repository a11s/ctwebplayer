using System;
using System.Drawing;
using System.Windows.Forms;

namespace ctwebplayer
{
    public partial class CookieEditDialog : Form
    {
        private LanguageManager languageManager;
        private CookieItem originalCookie;
        
        public CookieItem Cookie { get; private set; }
        
        // 添加新Cookie的构造函数
        public CookieEditDialog() : this(null)
        {
        }
        
        // 编辑现有Cookie的构造函数
        public CookieEditDialog(CookieItem cookie)
        {
            InitializeComponent();
            languageManager = LanguageManager.Instance;
            
            // 必须先设置控件，再加载数据
            SetupControls();  // 移到前面，确保ComboBox已经有项目
            
            if (cookie != null)
            {
                originalCookie = cookie;
                Cookie = cookie.Clone();
                LoadCookieData();  // 现在可以安全地设置SelectedIndex了
            }
            else
            {
                Cookie = new CookieItem();
                SetDefaultValues();
            }
            
            ApplyLanguage();
        }
        
        private void SetupControls()
        {
            // 设置SameSite下拉框选项
            comboBoxSameSite.Items.AddRange(new[] { "None", "Lax", "Strict" });
            comboBoxSameSite.SelectedIndex = 1; // 默认选择Lax
            
            // 设置日期时间选择器
            dateTimePickerExpires.Format = DateTimePickerFormat.Custom;
            dateTimePickerExpires.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dateTimePickerExpires.ShowCheckBox = true;
            
            // 设置验证事件
            textBoxDomain.Validating += TextBoxDomain_Validating;
            textBoxName.Validating += TextBoxName_Validating;
            
            // 设置按钮事件
            buttonOK.Click += ButtonOK_Click;
            buttonCancel.Click += ButtonCancel_Click;
            buttonSetSessionCookie.Click += ButtonSetSessionCookie_Click;
            
            // 设置工具提示
            var toolTip = new ToolTip();
            toolTip.SetToolTip(checkBoxHttpOnly, "启用后，Cookie只能通过HTTP(S)协议访问，JavaScript无法访问");
            toolTip.SetToolTip(checkBoxSecure, "启用后，Cookie只能通过HTTPS协议传输");
            toolTip.SetToolTip(comboBoxSameSite, "控制Cookie的跨站请求发送策略");
            toolTip.SetToolTip(buttonSetSessionCookie, "设置为会话Cookie（浏览器关闭时删除）");
        }
        
        private void LoadCookieData()
        {
            if (Cookie == null) return;
            
            textBoxDomain.Text = Cookie.Domain;
            textBoxName.Text = Cookie.Name;
            textBoxValue.Text = Cookie.Value;
            textBoxPath.Text = Cookie.Path;
            
            if (Cookie.Expires.HasValue)
            {
                dateTimePickerExpires.Checked = true;
                dateTimePickerExpires.Value = Cookie.Expires.Value;
            }
            else
            {
                dateTimePickerExpires.Checked = false;
            }
            
            checkBoxHttpOnly.Checked = Cookie.HttpOnly;
            checkBoxSecure.Checked = Cookie.Secure;
            
            // 设置SameSite值 - 现在ComboBox已经有项目了
            int sameSiteIndex = comboBoxSameSite.Items.IndexOf(Cookie.SameSite);
            if (sameSiteIndex >= 0)
            {
                comboBoxSameSite.SelectedIndex = sameSiteIndex;
            }
            else
            {
                // 如果找不到匹配的值，设置为默认的Lax（索引1）
                if (comboBoxSameSite.Items.Count > 1)
                {
                    comboBoxSameSite.SelectedIndex = 1; // 默认Lax
                }
                else if (comboBoxSameSite.Items.Count > 0)
                {
                    comboBoxSameSite.SelectedIndex = 0; // 至少选择第一个
                }
            }
        }
        
        private void SetDefaultValues()
        {
            textBoxPath.Text = "/";
            dateTimePickerExpires.Checked = false;
            checkBoxHttpOnly.Checked = false;
            checkBoxSecure.Checked = false;
            comboBoxSameSite.SelectedIndex = 1; // Lax
        }
        
        private void ApplyLanguage()
        {
            // 窗体标题
            this.Text = originalCookie == null ? 
                languageManager.GetString("CookieEdit_TitleAdd", "添加Cookie") : 
                languageManager.GetString("CookieEdit_TitleEdit", "编辑Cookie");
            
            // 标签
            labelDomain.Text = languageManager.GetString("CookieEdit_Domain", "域名:");
            labelName.Text = languageManager.GetString("CookieEdit_Name", "名称:");
            labelValue.Text = languageManager.GetString("CookieEdit_Value", "值:");
            labelPath.Text = languageManager.GetString("CookieEdit_Path", "路径:");
            labelExpires.Text = languageManager.GetString("CookieEdit_Expires", "过期时间:");
            labelSameSite.Text = languageManager.GetString("CookieEdit_SameSite", "SameSite:");
            
            // 复选框
            checkBoxHttpOnly.Text = languageManager.GetString("CookieEdit_HttpOnly", "HttpOnly");
            checkBoxSecure.Text = languageManager.GetString("CookieEdit_Secure", "Secure");
            
            // 按钮
            buttonOK.Text = languageManager.GetString("CookieEdit_OK", "确定");
            buttonCancel.Text = languageManager.GetString("CookieEdit_Cancel", "取消");
            buttonSetSessionCookie.Text = languageManager.GetString("CookieEdit_SessionCookie", "会话Cookie");
            
            // 分组框
            groupBoxProperties.Text = languageManager.GetString("CookieEdit_Properties", "Cookie属性");
            groupBoxSecurity.Text = languageManager.GetString("CookieEdit_Security", "安全设置");
        }
        
        private void TextBoxDomain_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxDomain.Text))
            {
                errorProvider.SetError(textBoxDomain, languageManager.GetString("CookieEdit_DomainRequired", "域名不能为空"));
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(textBoxDomain, "");
            }
        }
        
        private void TextBoxName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                errorProvider.SetError(textBoxName, languageManager.GetString("CookieEdit_NameRequired", "名称不能为空"));
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(textBoxName, "");
            }
        }
        
        private void ButtonSetSessionCookie_Click(object sender, EventArgs e)
        {
            dateTimePickerExpires.Checked = false;
        }
        
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            // 验证所有输入
            if (!ValidateChildren())
            {
                return;
            }
            
            // 保存Cookie数据
            Cookie.Domain = textBoxDomain.Text.Trim();
            Cookie.Name = textBoxName.Text.Trim();
            Cookie.Value = textBoxValue.Text;
            Cookie.Path = textBoxPath.Text.Trim();
            
            if (dateTimePickerExpires.Checked)
            {
                Cookie.Expires = dateTimePickerExpires.Value;
            }
            else
            {
                Cookie.Expires = null;
            }
            
            Cookie.HttpOnly = checkBoxHttpOnly.Checked;
            Cookie.Secure = checkBoxSecure.Checked;
            Cookie.SameSite = comboBoxSameSite.SelectedItem?.ToString() ?? "Lax";
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // 设置焦点到第一个输入框
            if (string.IsNullOrWhiteSpace(textBoxDomain.Text))
            {
                textBoxDomain.Focus();
            }
            else if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                textBoxName.Focus();
            }
            else
            {
                textBoxValue.Focus();
            }
        }
    }
}