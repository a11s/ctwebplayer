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
            // ���ð汾��Ϣ
            lblVersion.Text = $"{CTWebPlayer.Version.Description} v{CTWebPlayer.Version.FullVersion}";
            
            // ���ô��ڱ���
            this.Text = $"���� CTWebPlayer {CTWebPlayer.Version.FullVersion}";
            
            // �������֤�ı�
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
                
                // �����ı����ʼλ�õ�����
                txtLicense.SelectionStart = 0;
                txtLicense.SelectionLength = 0;
                txtLicense.ScrollToCaret();
            }
            catch (Exception ex)
            {
                txtLicense.Text = $"�޷��������֤�ļ���{ex.Message}\r\n\r\n" + GetEmbeddedLicenseText();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string GetEmbeddedLicenseText()
        {
            return @"CTWebPlayer ���֤
���� BSD 3-Clause License �޸�

��Ȩ���� (c) 2025, CTWebPlayer ������
��������Ȩ����

��������������������£�������Դ����Ͷ�������ʽ���·ַ���ʹ�ã������Ƿ��޸ģ�

1. Դ��������·ַ����뱣��������Ȩ�������������б����������������

2. ��������ʽ�����·ַ��������ĵ���/�������ṩ�Ĳ����и���������Ȩ������
   �������б����������������

3. δ���ض�����������ɣ�����ʹ�ð�Ȩ�����˵����ƻ��乱���ߵ��������Ͽɻ�
   �ƹ�ӱ���������Ĳ�Ʒ��

4. ����Ҫ���κ��޸İ汾���룺
   - ʹ�ò�ͬ�����ƣ�����ʹ��""CTWebPlayer""���κ���������
   - ����ر��������޸İ汾
   - �������κη�ʽ��ʾԭ���߶��޸İ汾���Ͽɻ򵣱�
   - ����������λ������""����CTWebPlayer�޸ģ�����ԭ�����޹�""

5. δ��������Ȩ�����������������ҵĿ�ġ�

����������
������ɰ�Ȩ�����˺͹�����""��ԭ��""�ṩ�����ṩ�κ���ʾ��ʾ�ı�֤��������������
�����ԡ��ض���;�����Ժͷ���Ȩ�Եı�֤�����κ�����£��������ں�ͬ���ϡ���Ȩ��Ϊ
���������棬��Ȩ�����˻����߾�������ʹ�ñ�������޷�ʹ�ñ�������������κ����⡢
�𺦻��������θ��𣬼�ʹ�ѱ���֪���ܷ��������𺦡�

�ر�������
�κλ��ڱ�������޸İ汾�����������⡢�𺦻������Σ������޸������ге�����ԭ����
�޹ء��û�Ӧ���ӹٷ�������ȡ���������ȷ������������ԺͰ�ȫ�ԡ�";
        }
    }
}

