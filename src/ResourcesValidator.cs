using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ctwebplayer
{
    public class ResourcesValidator
    {
        public static void ValidateResourceFiles()
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var resourceFiles = new Dictionary<string, string>
            {
                {"English (Base)", "Strings.resx"},
                {"Chinese Simplified", "Strings.zh-CN.resx"},
                {"Chinese Traditional", "Strings.zh-TW.resx"},
                {"Japanese", "Strings.ja.resx"},
                {"Korean", "Strings.ko.resx"}
            };

            var allKeys = new Dictionary<string, HashSet<string>>();
            var errors = new List<string>();

            // 首先加载基础文件的所有键
            var baseFile = Path.Combine(basePath, "Strings.resx");
            HashSet<string> baseKeys = null;

            try
            {
                baseKeys = LoadResourceKeys(baseFile);
                allKeys["English (Base)"] = baseKeys;
                LogManager.Instance.Info($"Base resource file has {baseKeys.Count} keys");
            }
            catch (Exception ex)
            {
                errors.Add($"Error loading base resource file: {ex.Message}");
                LogManager.Instance.Error($"Failed to load base resource file", ex);
                return;
            }

            // 检查每个语言文件
            foreach (var kvp in resourceFiles.Skip(1)) // Skip base file
            {
                var filePath = Path.Combine(basePath, kvp.Value);
                var language = kvp.Key;

                try
                {
                    var keys = LoadResourceKeys(filePath);
                    allKeys[language] = keys;

                    // 找出缺失的键
                    var missingKeys = baseKeys.Except(keys).ToList();
                    var extraKeys = keys.Except(baseKeys).ToList();

                    if (missingKeys.Any())
                    {
                        errors.Add($"{language} missing {missingKeys.Count} keys: {string.Join(", ", missingKeys.Take(5))}...");
                        LogManager.Instance.Warning($"{language} missing keys: {string.Join(", ", missingKeys)}");
                    }

                    if (extraKeys.Any())
                    {
                        errors.Add($"{language} has {extraKeys.Count} extra keys: {string.Join(", ", extraKeys.Take(5))}...");
                        LogManager.Instance.Warning($"{language} extra keys: {string.Join(", ", extraKeys)}");
                    }

                    LogManager.Instance.Info($"{language}: {keys.Count} keys (missing: {missingKeys.Count}, extra: {extraKeys.Count})");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error loading {language} resource file: {ex.Message}");
                    LogManager.Instance.Error($"Failed to load {language} resource file", ex);
                }
            }

            // 记录验证结果
            if (errors.Any())
            {
                LogManager.Instance.Error("Resource validation found issues:");
                foreach (var error in errors)
                {
                    LogManager.Instance.Error($"  - {error}");
                }
            }
            else
            {
                LogManager.Instance.Info("All resource files validated successfully!");
            }

            // 输出详细报告
            GenerateDetailedReport(allKeys, baseKeys);
        }

        private static HashSet<string> LoadResourceKeys(string filePath)
        {
            var keys = new HashSet<string>();
            
            try
            {
                var doc = XDocument.Load(filePath);
                var dataElements = doc.Descendants("data")
                    .Where(e => e.Attribute("name") != null);

                foreach (var element in dataElements)
                {
                    var name = element.Attribute("name")?.Value;
                    if (!string.IsNullOrEmpty(name))
                    {
                        keys.Add(name);
                    }
                }
            }
            catch (System.Xml.XmlException xmlEx)
            {
                // XML格式错误
                LogManager.Instance.Error($"XML parsing error in {Path.GetFileName(filePath)} at line {xmlEx.LineNumber}, position {xmlEx.LinePosition}: {xmlEx.Message}");
                throw new InvalidOperationException($"XML format error in {Path.GetFileName(filePath)} at line {xmlEx.LineNumber}", xmlEx);
            }

            return keys;
        }

        private static void GenerateDetailedReport(Dictionary<string, HashSet<string>> allKeys, HashSet<string> baseKeys)
        {
            var reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResourceValidationReport.txt");
            
            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("=== Resource Files Validation Report ===");
                writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();
                writer.WriteLine($"Base keys count: {baseKeys.Count}");
                writer.WriteLine();

                foreach (var kvp in allKeys)
                {
                    writer.WriteLine($"[{kvp.Key}]");
                    writer.WriteLine($"  Total keys: {kvp.Value.Count}");
                    
                    if (kvp.Key != "English (Base)")
                    {
                        var missingKeys = baseKeys.Except(kvp.Value).ToList();
                        var extraKeys = kvp.Value.Except(baseKeys).ToList();
                        
                        if (missingKeys.Any())
                        {
                            writer.WriteLine($"  Missing keys ({missingKeys.Count}):");
                            foreach (var key in missingKeys)
                            {
                                writer.WriteLine($"    - {key}");
                            }
                        }
                        
                        if (extraKeys.Any())
                        {
                            writer.WriteLine($"  Extra keys ({extraKeys.Count}):");
                            foreach (var key in extraKeys)
                            {
                                writer.WriteLine($"    - {key}");
                            }
                        }
                        
                        if (!missingKeys.Any() && !extraKeys.Any())
                        {
                            writer.WriteLine("  ✓ All keys match base file");
                        }
                    }
                    writer.WriteLine();
                }
            }

            LogManager.Instance.Info($"Detailed validation report saved to: {reportPath}");
        }

        public static void RunValidation()
        {
            try
            {
                LogManager.Instance.Info("Starting resource files validation...");
                ValidateResourceFiles();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("Resource validation failed", ex);
            }
        }
    }
}