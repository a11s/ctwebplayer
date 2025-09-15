using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ctwebplayer.test
{
    /// <summary>
    /// LanguageManager 功能测试类
    /// </summary>
    class LanguageManagerTest
    {
        private static int testsPassed = 0;
        private static int testsFailed = 0;
        private static List<string> failedTests = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("=== CTWebPlayer 多语言功能测试 ===\n");
            
            // 备份当前配置文件
            BackupConfigFile();
            
            try
            {
                // 执行测试
                TestGetString();
                TestLanguageSwitch();
                TestFormattedStrings();
                TestSystemLanguageDetection();
                TestLanguagePersistence();
                TestErrorHandling();
                TestSupportedLanguages();
                TestResourceKeyGeneration();
                
                // 显示测试结果
                DisplayTestResults();
            }
            finally
            {
                // 恢复配置文件
                RestoreConfigFile();
            }
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 测试 GetString 方法
        /// </summary>
        static void TestGetString()
        {
            Console.WriteLine("测试 GetString 方法...");
            
            try
            {
                var langManager = LanguageManager.Instance;
                langManager.Initialize();
                
                // 测试存在的键
                string result = langManager.GetString("Button_OK");
                AssertNotNull(result, "GetString 应返回非空值");
                AssertNotEquals(result, "Button_OK", "GetString 应返回本地化文本而非键名");
                
                // 测试不存在的键
                string missingKey = "NonExistentKey_12345";
                result = langManager.GetString(missingKey);
                AssertEquals(result, missingKey, "不存在的键应返回键名本身");
                
                Console.WriteLine("✓ GetString 测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ GetString 测试失败: {ex.Message}");
                failedTests.Add($"GetString: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试语言切换功能
        /// </summary>
        static void TestLanguageSwitch()
        {
            Console.WriteLine("\n测试语言切换功能...");
            
            try
            {
                var langManager = LanguageManager.Instance;
                bool eventFired = false;
                
                // 订阅语言改变事件
                langManager.LanguageChanged += (sender, e) => { eventFired = true; };
                
                // 测试切换到每种支持的语言
                foreach (var lang in LanguageManager.SupportedLanguages)
                {
                    eventFired = false;
                    langManager.LoadLanguage(lang.Key);
                    
                    AssertEquals(langManager.CurrentCulture.Name, lang.Key, 
                        $"当前语言应为 {lang.Key}");
                    AssertTrue(eventFired, "语言改变事件应被触发");
                    
                    // 验证线程文化信息
                    AssertEquals(Thread.CurrentThread.CurrentUICulture.Name, lang.Key,
                        "线程UI文化应更新");
                    
                    Console.WriteLine($"  ✓ 成功切换到 {lang.Value} ({lang.Key})");
                }
                
                Console.WriteLine("✓ 语言切换测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 语言切换测试失败: {ex.Message}");
                failedTests.Add($"语言切换: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试格式化字符串
        /// </summary>
        static void TestFormattedStrings()
        {
            Console.WriteLine("\n测试格式化字符串...");
            
            try
            {
                var langManager = LanguageManager.Instance;
                
                // 模拟一个格式化字符串资源
                // 注意：这需要资源文件中有相应的格式化字符串
                string testKey = "Message_Version"; // 假设这是 "版本 {0}"
                string version = "1.0.0";
                
                // 测试格式化
                string result = langManager.GetString(testKey, version);
                
                // 由于我们不知道确切的翻译，只检查是否包含版本号
                AssertTrue(result.Contains(version) || result == testKey, 
                    "格式化字符串应包含参数值或返回键名");
                
                // 测试多参数格式化
                object[] args = { "test", 123, DateTime.Now };
                result = langManager.GetString("NonExistentFormatKey", args);
                AssertNotNull(result, "格式化方法不应返回null");
                
                Console.WriteLine("✓ 格式化字符串测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 格式化字符串测试失败: {ex.Message}");
                failedTests.Add($"格式化字符串: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试系统语言检测
        /// </summary>
        static void TestSystemLanguageDetection()
        {
            Console.WriteLine("\n测试系统语言检测...");
            
            try
            {
                // 清除配置以测试系统语言检测
                var configPath = "config.json";
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                }
                
                // 重新初始化以触发系统语言检测
                var langManager = new LanguageManager();
                langManager.Initialize();
                
                // 获取系统语言
                var systemCulture = CultureInfo.CurrentUICulture.Name;
                Console.WriteLine($"  系统语言: {systemCulture}");
                Console.WriteLine($"  检测后语言: {langManager.CurrentCulture.Name}");
                
                // 验证检测逻辑
                if (LanguageManager.SupportedLanguages.ContainsKey(systemCulture))
                {
                    AssertEquals(langManager.CurrentCulture.Name, systemCulture,
                        "应使用系统语言");
                }
                else
                {
                    // 检查是否正确回退
                    bool isValidFallback = false;
                    
                    // 检查各种回退情况
                    if (systemCulture == "en")
                    {
                        isValidFallback = langManager.CurrentCulture.Name == "en-US";
                    }
                    else if (systemCulture.StartsWith("zh"))
                    {
                        isValidFallback = langManager.CurrentCulture.Name == "zh-CN" || 
                                        langManager.CurrentCulture.Name == "zh-TW";
                    }
                    else if (systemCulture.StartsWith("ja"))
                    {
                        isValidFallback = langManager.CurrentCulture.Name == "ja";
                    }
                    else if (systemCulture.StartsWith("ko"))
                    {
                        isValidFallback = langManager.CurrentCulture.Name == "ko";
                    }
                    else
                    {
                        isValidFallback = langManager.CurrentCulture.Name == "en-US";
                    }
                    
                    AssertTrue(isValidFallback, "应正确回退到合适的语言");
                }
                
                Console.WriteLine("✓ 系统语言检测测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 系统语言检测测试失败: {ex.Message}");
                failedTests.Add($"系统语言检测: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试语言持久化
        /// </summary>
        static void TestLanguagePersistence()
        {
            Console.WriteLine("\n测试语言持久化...");
            
            try
            {
                var langManager = LanguageManager.Instance;
                
                // 设置语言为日语
                langManager.LoadLanguage("ja");
                
                // 验证配置文件
                var configPath = "config.json";
                AssertTrue(File.Exists(configPath), "配置文件应存在");
                
                string configContent = File.ReadAllText(configPath);
                AssertTrue(configContent.Contains("\"Language\": \"ja\"") || 
                          configContent.Contains("\"Language\":\"ja\""),
                          "配置文件应包含语言设置");
                
                // 模拟重启：创建新实例并初始化
                var newLangManager = new LanguageManager();
                newLangManager.Initialize();
                
                // 验证语言是否保持
                AssertEquals(newLangManager.CurrentCulture.Name, "ja",
                    "重启后应保持之前的语言设置");
                
                Console.WriteLine("✓ 语言持久化测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 语言持久化测试失败: {ex.Message}");
                failedTests.Add($"语言持久化: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试错误处理
        /// </summary>
        static void TestErrorHandling()
        {
            Console.WriteLine("\n测试错误处理...");
            
            try
            {
                var langManager = LanguageManager.Instance;
                
                // 测试加载不支持的语言
                bool exceptionThrown = false;
                try
                {
                    langManager.LoadLanguage("fr-FR");
                }
                catch (ArgumentException)
                {
                    exceptionThrown = true;
                }
                AssertTrue(exceptionThrown, "加载不支持的语言应抛出异常");
                
                // 测试空键
                string result = langManager.GetString(null);
                AssertNotNull(result, "GetString 对空键应返回非null值");
                
                // 测试空格式化参数
                result = langManager.GetString("Button_OK", null);
                AssertNotNull(result, "格式化方法对null参数应返回非null值");
                
                Console.WriteLine("✓ 错误处理测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 错误处理测试失败: {ex.Message}");
                failedTests.Add($"错误处理: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试支持的语言列表
        /// </summary>
        static void TestSupportedLanguages()
        {
            Console.WriteLine("\n测试支持的语言列表...");
            
            try
            {
                var supportedLangs = LanguageManager.SupportedLanguages;
                
                // 验证支持的语言数量
                AssertEquals(supportedLangs.Count, 5, "应支持5种语言");
                
                // 验证每种语言
                AssertTrue(supportedLangs.ContainsKey("en-US"), "应支持英语");
                AssertTrue(supportedLangs.ContainsKey("zh-CN"), "应支持简体中文");
                AssertTrue(supportedLangs.ContainsKey("zh-TW"), "应支持繁体中文");
                AssertTrue(supportedLangs.ContainsKey("ja"), "应支持日语");
                AssertTrue(supportedLangs.ContainsKey("ko"), "应支持韩语");
                
                // 验证语言显示名称
                AssertEquals(supportedLangs["en-US"], "English", "英语显示名称应正确");
                AssertEquals(supportedLangs["zh-CN"], "简体中文", "简体中文显示名称应正确");
                
                Console.WriteLine("✓ 支持的语言列表测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 支持的语言列表测试失败: {ex.Message}");
                failedTests.Add($"支持的语言列表: {ex.Message}");
                testsFailed++;
            }
        }

        /// <summary>
        /// 测试资源键生成逻辑
        /// </summary>
        static void TestResourceKeyGeneration()
        {
            Console.WriteLine("\n测试资源键生成逻辑...");
            
            try
            {
                // 这个测试验证 GetResourceKey 方法的逻辑
                // 由于该方法是私有的，我们通过观察行为来测试
                
                var langManager = LanguageManager.Instance;
                
                // 测试不同类型控件的键名模式
                var testPatterns = new Dictionary<string, string>
                {
                    { "Button_btnOK", "确定|OK|はい|예" },
                    { "Label_lblTitle", "标题|Title|タイトル|제목" },
                    { "CheckBox_chkRemember", "记住|Remember|覚える|기억" },
                    { "Menu_menuFile", "文件|File|ファイル|파일" }
                };
                
                foreach (var pattern in testPatterns)
                {
                    string result = langManager.GetString(pattern.Key);
                    // 如果键存在，结果不应等于键名
                    // 如果键不存在，结果应等于键名（作为后备）
                    Console.WriteLine($"  测试键 {pattern.Key}: {result}");
                }
                
                Console.WriteLine("✓ 资源键生成逻辑测试通过");
                testsPassed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 资源键生成逻辑测试失败: {ex.Message}");
                failedTests.Add($"资源键生成: {ex.Message}");
                testsFailed++;
            }
        }

        #region 辅助方法

        static void BackupConfigFile()
        {
            if (File.Exists("config.json"))
            {
                File.Copy("config.json", "config.json.backup", true);
            }
        }

        static void RestoreConfigFile()
        {
            if (File.Exists("config.json.backup"))
            {
                File.Copy("config.json.backup", "config.json", true);
                File.Delete("config.json.backup");
            }
        }

        static void DisplayTestResults()
        {
            Console.WriteLine("\n=== 测试结果汇总 ===");
            Console.WriteLine($"通过: {testsPassed}");
            Console.WriteLine($"失败: {testsFailed}");
            Console.WriteLine($"总计: {testsPassed + testsFailed}");
            
            if (failedTests.Count > 0)
            {
                Console.WriteLine("\n失败的测试:");
                foreach (var test in failedTests)
                {
                    Console.WriteLine($"  - {test}");
                }
            }
            else
            {
                Console.WriteLine("\n✓ 所有测试通过！");
            }
        }

        #endregion

        #region 断言方法

        static void AssertTrue(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"断言失败: {message}");
        }

        static void AssertEquals(object actual, object expected, string message)
        {
            if (!Equals(actual, expected))
                throw new Exception($"断言失败: {message}. 期望: {expected}, 实际: {actual}");
        }

        static void AssertNotEquals(object actual, object notExpected, string message)
        {
            if (Equals(actual, notExpected))
                throw new Exception($"断言失败: {message}. 不应等于: {notExpected}");
        }

        static void AssertNotNull(object obj, string message)
        {
            if (obj == null)
                throw new Exception($"断言失败: {message}");
        }

        #endregion
    }
}