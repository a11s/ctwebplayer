using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace ctwebplayer.test
{
    class TestLanguageStartup
    {
        static void Main()
        {
            Console.WriteLine("=== 语言启动测试 ===");
            
            // 测试配置管理器
            var configManager = new ConfigManager();
            Console.WriteLine($"配置文件语言设置: {configManager.Config.Language}");
            
            // 设置 LanguageManager
            LanguageManager.SetConfigManager(configManager);
            
            // 初始化语言
            Console.WriteLine("\n初始化语言管理器...");
            LanguageManager.Instance.Initialize();
            
            // 测试当前语言
            Console.WriteLine($"\n当前语言文化: {LanguageManager.Instance.CurrentCulture?.Name ?? "null"}");
            Console.WriteLine($"当前线程UI文化: {Thread.CurrentThread.CurrentUICulture.Name}");
            
            // 测试资源读取
            Console.WriteLine("\n测试资源读取:");
            TestResourceStrings();
            
            // 测试窗体本地化
            Console.WriteLine("\n测试窗体本地化:");
            TestFormLocalization();
            
            Console.WriteLine("\n=== 测试完成 ===");
            Console.ReadLine();
        }
        
        static void TestResourceStrings()
        {
            try
            {
                // 测试几个关键的资源字符串
                string[] testKeys = { "Form1_Title", "Form1_Button_Home", "Settings_Title", "Button_OK" };
                
                foreach (var key in testKeys)
                {
                    var value = LanguageManager.Instance.GetString(key);
                    Console.WriteLine($"  {key} = {value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine($"堆栈: {ex.StackTrace}");
            }
        }
        
        static void TestFormLocalization()
        {
            try
            {
                // 创建一个简单的测试窗体
                var form = new Form();
                form.Name = "Form1";
                form.Text = "测试窗体";
                
                var button = new Button();
                button.Name = "btnHome";
                button.Text = "主页";
                button.Tag = "RES_Form1_Button_Home";
                form.Controls.Add(button);
                
                Console.WriteLine($"  应用语言前 - 窗体标题: {form.Text}, 按钮文本: {button.Text}");
                
                // 应用语言
                LanguageManager.Instance.ApplyToForm(form);
                
                Console.WriteLine($"  应用语言后 - 窗体标题: {form.Text}, 按钮文本: {button.Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine($"堆栈: {ex.StackTrace}");
            }
        }
    }
}