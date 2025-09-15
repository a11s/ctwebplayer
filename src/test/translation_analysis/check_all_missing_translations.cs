using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CheckAllMissingTranslations
{
    class Program
    {
        static void Main(string[] args)
        {
            string resourcePath = @"../../Resources";
            var languages = new Dictionary<string, string>
            {
                { "en", "Strings.resx" },
                { "zh-CN", "Strings.zh-CN.resx" },
                { "zh-TW", "Strings.zh-TW.resx" },
                { "ja", "Strings.ja.resx" },
                { "ko", "Strings.ko.resx" }
            };

            // 存储每个语言的keys和values
            var languageData = new Dictionary<string, Dictionary<string, string>>();

            // 读取所有语言文件
            foreach (var lang in languages)
            {
                string filePath = Path.Combine(resourcePath, lang.Value);
                languageData[lang.Key] = ReadResourceFile(filePath);
            }

            // 收集所有语言中的所有唯一key
            var allKeys = new HashSet<string>();
            foreach (var lang in languageData)
            {
                foreach (var key in lang.Value.Keys)
                {
                    allKeys.Add(key);
                }
            }

            Console.WriteLine($"Total unique keys across all languages: {allKeys.Count}");
            Console.WriteLine("\n=== Cross-Language Key Analysis ===\n");

            // 创建一个矩阵来显示每个key在各个语言中的存在情况
            var keyPresenceMatrix = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var key in allKeys)
            {
                keyPresenceMatrix[key] = new Dictionary<string, bool>();
                foreach (var lang in languages)
                {
                    keyPresenceMatrix[key][lang.Key] = languageData[lang.Key].ContainsKey(key);
                }
            }

            // 找出在某些语言中存在但在其他语言中缺失的key
            var inconsistentKeys = keyPresenceMatrix
                .Where(kv => kv.Value.Values.Any(v => v) && kv.Value.Values.Any(v => !v))
                .OrderBy(kv => kv.Key)
                .ToList();

            if (inconsistentKeys.Any())
            {
                Console.WriteLine($"Found {inconsistentKeys.Count} keys that exist in some languages but not in others:");
                Console.WriteLine("\nKey\t\t\t\t\ten\tzh-CN\tzh-TW\tja\tko");
                Console.WriteLine(new string('-', 80));
                
                foreach (var kv in inconsistentKeys)
                {
                    string key = kv.Key;
                    if (key.Length > 40) key = key.Substring(0, 37) + "...";
                    Console.Write($"{key,-40}\t");
                    
                    foreach (var lang in languages)
                    {
                        Console.Write(kv.Value[lang.Key] ? "✓\t" : "✗\t");
                    }
                    Console.WriteLine();
                }
            }

            // 详细报告每个语言缺失的key
            Console.WriteLine("\n\n=== Detailed Missing Keys by Language ===");
            
            foreach (var lang in languages)
            {
                Console.WriteLine($"\n--- {lang.Key} ({lang.Value}) ---");
                var missingKeys = allKeys.Where(key => !languageData[lang.Key].ContainsKey(key)).ToList();
                
                if (missingKeys.Any())
                {
                    Console.WriteLine($"Missing {missingKeys.Count} keys:");
                    foreach (var key in missingKeys.OrderBy(k => k))
                    {
                        // 找到这个key在哪个语言中存在，并显示其值作为参考
                        var referenceValue = "";
                        var referenceLanguage = "";
                        foreach (var otherLang in languages.Where(l => l.Key != lang.Key))
                        {
                            if (languageData[otherLang.Key].ContainsKey(key))
                            {
                                referenceValue = languageData[otherLang.Key][key];
                                referenceLanguage = otherLang.Key;
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(referenceValue))
                        {
                            Console.WriteLine($"  - {key}");
                            Console.WriteLine($"    Reference ({referenceLanguage}): \"{referenceValue}\"");
                        }
                        else
                        {
                            Console.WriteLine($"  - {key} (no reference found)");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No missing keys!");
                }
            }

            // 特别检查用户提到的新keys
            Console.WriteLine("\n=== Specific Keys Check (New) ===");
            var specificNewKeys = new[] 
            { 
                "SettingsForm_Msg_BaseURLEmpty",
                "SettingsForm_Msg_BaseURLInvalid",
                "SettingsForm_Msg_ClearLogsError"
            };

            foreach (var key in specificNewKeys)
            {
                Console.WriteLine($"\n{key}:");
                foreach (var lang in languages)
                {
                    if (languageData[lang.Key].ContainsKey(key))
                    {
                        Console.WriteLine($"  {lang.Key}: \"{languageData[lang.Key][key]}\"");
                    }
                    else
                    {
                        Console.WriteLine($"  {lang.Key}: MISSING");
                    }
                }
            }
        }

        static Dictionary<string, string> ReadResourceFile(string filePath)
        {
            var result = new Dictionary<string, string>();
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return result;
            }

            try
            {
                XDocument doc = XDocument.Load(filePath);
                var dataElements = doc.Descendants("data")
                    .Where(e => e.Attribute("name") != null && e.Element("value") != null);

                foreach (var element in dataElements)
                {
                    string name = element.Attribute("name").Value;
                    string value = element.Element("value").Value;
                    result[name] = value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {filePath}: {ex.Message}");
            }

            return result;
        }
    }
}