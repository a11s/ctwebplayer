using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace ctwebplayer.test
{
    class TestResourceLoading
    {
        static void Main()
        {
            Console.WriteLine("=== 资源加载测试 ===\n");
            
            // 1. 列出程序集中的所有资源
            Console.WriteLine("1. 程序集中的嵌入资源:");
            var assembly = typeof(LanguageManager).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var name in resourceNames)
            {
                Console.WriteLine($"  - {name}");
            }
            
            // 2. 测试不同的资源管理器初始化方式
            Console.WriteLine("\n2. 测试 ResourceManager 初始化:");
            TestResourceManager("ctwebplayer.Strings");
            
            // 3. 测试直接读取资源流
            Console.WriteLine("\n3. 测试直接读取资源流:");
            foreach (var resourceName in resourceNames)
            {
                if (resourceName.Contains("Strings") && !resourceName.Contains(".resources"))
                {
                    Console.WriteLine($"\n  尝试读取: {resourceName}");
                    try
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                Console.WriteLine($"    成功! 流大小: {stream.Length} 字节");
                            }
                            else
                            {
                                Console.WriteLine("    失败: 流为 null");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    错误: {ex.Message}");
                    }
                }
            }
            
            Console.WriteLine("\n按任意键退出...");
            //Console.ReadKey();
        }
        
        static void TestResourceManager(string baseName)
        {
            Console.WriteLine($"\n  测试基础名称: {baseName}");
            try
            {
                var rm = new ResourceManager(baseName, typeof(LanguageManager).Assembly);
                
                // 测试多个资源键
                var testKeys = new[] { "Form1_Title", "UpdateForm", "SettingsForm", "LogViewerForm", "ProxySettingsForm", "LoginDialog_Title", "AboutForm_Title" };
                
                // 测试英文资源
                var culture = new CultureInfo("en-US");
                foreach (var key in testKeys)
                {
                    var value = rm.GetString(key, culture);
                    Console.WriteLine($"    {key} (en-US) = {value ?? "MISSING"}");
                }
                
                // 测试中文资源
                culture = new CultureInfo("zh-CN");
                foreach (var key in testKeys)
                {
                    var value = rm.GetString(key, culture);
                    Console.WriteLine($"    {key} (zh-CN) = {value ?? "MISSING"}");
                }
                
                // 调试：列出资源集中的所有键
                Console.WriteLine("\n  调试 - 列出zh-CN资源集中的所有键:");
                var resourceSet = rm.GetResourceSet(culture, true, false);
                if (resourceSet != null)
                {
                    int count = 0;
                    bool foundProxySettingsForm = false;
                    foreach (System.Collections.DictionaryEntry entry in resourceSet)
                    {
                        string key = entry.Key.ToString();
                        if (key.Contains("ProxySettings"))
                        {
                            Console.WriteLine($"    找到包含ProxySettings的键: {key}");
                            if (key == "ProxySettingsForm")
                            {
                                foundProxySettingsForm = true;
                                Console.WriteLine($"      值: {entry.Value}");
                            }
                        }
                        count++;
                    }
                    Console.WriteLine($"    总共找到 {count} 个资源键");
                    if (!foundProxySettingsForm)
                    {
                        Console.WriteLine("    注意：没有找到 ProxySettingsForm 键");
                        
                        // 尝试使用不同的ResourceManager配置
                        Console.WriteLine("\n  尝试重新创建ResourceManager:");
                        var rm2 = new ResourceManager("ctwebplayer.Strings", typeof(LanguageManager).Assembly);
                        var zhCN = new CultureInfo("zh-CN");
                        var value = rm2.GetString("ProxySettingsForm", zhCN);
                        Console.WriteLine($"    ProxySettingsForm (zh-CN) 直接获取 = {value ?? "NULL"}");
                        
                        // 尝试获取中性资源
                        var neutralValue = rm2.GetString("ProxySettingsForm", CultureInfo.InvariantCulture);
                        Console.WriteLine($"    ProxySettingsForm (Invariant) = {neutralValue ?? "NULL"}");
                    }
                }
                else
                {
                    Console.WriteLine("    无法获取资源集");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    错误: {ex.Message}");
                Console.WriteLine($"    堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}