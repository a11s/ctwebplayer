using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CTWebPlayer.Test
{
    class CheckMissingTranslations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== 多语言资源文件缺失翻译检查工具 ===\n");

            // 资源文件路径
            string resourcesPath = Path.Combine("..", "Resources");
            string outputPath = "translate.txt";

            // 定义要检查的语言文件
            var languageFiles = new Dictionary<string, string>
            {
                { "日文 (ja)", "Strings.ja.resx" },
                { "韩语 (ko)", "Strings.ko.resx" },
                { "简体中文 (zh-CN)", "Strings.zh-CN.resx" },
                { "繁体中文 (zh-TW)", "Strings.zh-TW.resx" }
            };

            // 主文件（英文）
            string mainFile = Path.Combine(resourcesPath, "Strings.resx");

            try
            {
                // 1. 解析主文件，获取所有键值对
                Console.WriteLine($"正在解析主文件: {mainFile}");
                var mainKeys = ParseResxFile(mainFile);
                Console.WriteLine($"主文件包含 {mainKeys.Count} 个键\n");

                // 2. 存储每个语言的缺失键
                var missingTranslations = new Dictionary<string, List<KeyValuePair<string, string>>>();

                // 3. 检查每个语言文件
                foreach (var lang in languageFiles)
                {
                    string langFile = Path.Combine(resourcesPath, lang.Value);
                    Console.WriteLine($"正在检查 {lang.Key} ({lang.Value})...");

                    if (!File.Exists(langFile))
                    {
                        Console.WriteLine($"  警告: 文件不存在 - {langFile}");
                        continue;
                    }

                    var langKeys = ParseResxFile(langFile);
                    var missing = new List<KeyValuePair<string, string>>();

                    // 查找缺失的键
                    foreach (var mainKey in mainKeys)
                    {
                        if (!langKeys.ContainsKey(mainKey.Key))
                        {
                            missing.Add(mainKey);
                        }
                    }

                    if (missing.Count > 0)
                    {
                        missingTranslations[lang.Key] = missing;
                        Console.WriteLine($"  发现 {missing.Count} 个缺失的翻译键");
                    }
                    else
                    {
                        Console.WriteLine($"  所有键都已翻译");
                    }
                }

                // 4. 生成报告文件
                GenerateTranslateFile(outputPath, missingTranslations);

                // 5. 输出统计信息
                Console.WriteLine("\n=== 统计结果 ===");
                Console.WriteLine($"主文件总键数: {mainKeys.Count}");
                
                int totalMissing = 0;
                foreach (var lang in missingTranslations)
                {
                    Console.WriteLine($"{lang.Key}: 缺失 {lang.Value.Count} 个翻译");
                    totalMissing += lang.Value.Count;
                }

                Console.WriteLine($"\n总计缺失翻译数: {totalMissing}");
                Console.WriteLine($"\n翻译报告已生成: {Path.GetFullPath(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 解析 resx 文件，提取所有的键值对
        /// </summary>
        static Dictionary<string, string> ParseResxFile(string filePath)
        {
            var result = new Dictionary<string, string>();

            try
            {
                var doc = XDocument.Load(filePath);
                var dataElements = doc.Root.Elements("data")
                    .Where(e => e.Attribute("name") != null && !e.Attribute("name").Value.StartsWith(">>"));

                foreach (var element in dataElements)
                {
                    string key = element.Attribute("name").Value;
                    string value = element.Element("value")?.Value ?? "";
                    
                    // 跳过资源管理器生成的元数据
                    if (key.StartsWith("$") || key == ">>")
                        continue;

                    result[key] = value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"解析文件 {filePath} 时出错: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// 生成翻译报告文件
        /// </summary>
        static void GenerateTranslateFile(string outputPath, Dictionary<string, List<KeyValuePair<string, string>>> missingTranslations)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("===== 多语言资源文件缺失翻译报告 =====");
            sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("说明：以下是各语言文件中缺失的翻译键及其在主文件（英文）中的值。");
            sb.AppendLine("格式：键名 => 英文值");
            sb.AppendLine("=" + new string('=', 70));
            sb.AppendLine();

            foreach (var lang in missingTranslations.OrderBy(x => x.Key))
            {
                if (lang.Value.Count == 0)
                    continue;

                sb.AppendLine($"【{lang.Key}】 - 缺失 {lang.Value.Count} 个翻译");
                sb.AppendLine(new string('-', 60));

                // 按键名排序
                var sortedMissing = lang.Value.OrderBy(x => x.Key);

                foreach (var item in sortedMissing)
                {
                    sb.AppendLine($"{item.Key}");
                    sb.AppendLine($"=> {item.Value}");
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            // 生成所有缺失键的汇总列表
            var allMissingKeys = new HashSet<string>();
            foreach (var lang in missingTranslations)
            {
                foreach (var item in lang.Value)
                {
                    allMissingKeys.Add(item.Key);
                }
            }

            if (allMissingKeys.Count > 0)
            {
                sb.AppendLine("===== 所有缺失键汇总 =====");
                sb.AppendLine($"共 {allMissingKeys.Count} 个唯一键需要翻译");
                sb.AppendLine(new string('-', 60));

                foreach (var key in allMissingKeys.OrderBy(x => x))
                {
                    sb.AppendLine(key);
                }
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }
    }
}