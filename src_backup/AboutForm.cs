using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTWebPlayer;

namespace ctwebplayer
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            // 设置版本信息
            lblVersion.Text = $"{CTWebPlayer.Version.Description} v{CTWebPlayer.Version.FullVersion}";
            
            // 设置窗口标题
            this.Text = $"关于 CTWebPlayer {CTWebPlayer.Version.FullVersion}";
            
            // 加载许可证文本
            try
            {
                string licensePath = Path.Combine(Application.StartupPath, "LICENSE");
                if (File.Exists(licensePath))
                {
                    txtLicense.Text = File.ReadAllText(licensePath, Encoding.UTF8);
                }
                else
                {
                    txtLicense.Text = GetEmbeddedLicenseText();
                }
                
                // 设置文本框初始位置到顶部
                txtLicense.SelectionStart = 0;
                txtLicense.SelectionLength = 0;
                txtLicense.ScrollToCaret();
            }
            catch (Exception ex)
            {
                txtLicense.Text = $"无法加载许可证文件：{ex.Message}\r\n\r\n" + GetEmbeddedLicenseText();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string GetEmbeddedLicenseText()
        {
            return @"CTWebPlayer 许可证
基于 BSD 3-Clause License 修改

版权所有 (c) 2025, CTWebPlayer 开发者
保留所有权利。

在满足以下条件的情况下，允许以源代码和二进制形式重新分发和使用，无论是否修改：

1. 源代码的重新分发必须保留上述版权声明、此条件列表和以下免责声明。

2. 二进制形式的重新分发必须在文档和/或其他提供的材料中复制上述版权声明、
   此条件列表和以下免责声明。

3. 未经特定事先书面许可，不得使用版权持有人的名称或其贡献者的名称来认可或
   推广从本软件派生的产品。

4. 【重要】任何修改版本必须：
   - 使用不同的名称，不得使用""CTWebPlayer""或任何相似名称
   - 清楚地标明这是修改版本
   - 不得以任何方式暗示原作者对修改版本的认可或担保
   - 必须在显著位置声明""基于CTWebPlayer修改，但与原作者无关""

5. 未经书面授权，本软件不得用于商业目的。

免责声明：
本软件由版权持有人和贡献者""按原样""提供，不提供任何明示或暗示的保证，包括但不限于
适销性、特定用途适用性和非侵权性的保证。在任何情况下，无论是在合同诉讼、侵权行为
或其他方面，版权持有人或贡献者均不对因使用本软件或无法使用本软件而产生的任何索赔、
损害或其他责任负责，即使已被告知可能发生此类损害。

特别声明：
任何基于本软件的修改版本所产生的问题、损害或法律责任，均由修改者自行承担，与原作者
无关。用户应当从官方渠道获取本软件，以确保软件的完整性和安全性。";
        }
    }
}

