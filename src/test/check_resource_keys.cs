using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

class CheckResourceKeys
{
    static void Main()
    {
        // 1. 从资源文件中获取所有键
        var resourceKeys = new HashSet<string>();
        var resxFile = @"..\..\Resources\Strings.resx";
        
        if (File.Exists(resxFile))
        {
            var doc = XDocument.Load(resxFile);
            foreach (var data in doc.Descendants("data"))
            {
                var name = data.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(name))
                {
                    resourceKeys.Add(name);
                }
            }
        }
        
        Console.WriteLine($"找到 {resourceKeys.Count} 个资源键");
        
        // 2. 从代码中提取所有 GetString 调用
        var codeKeys = new Dictionary<string, List<string>>();
        var pattern = @"GetString\(""([^""]+)""\)";
        
        // 扫描所有 .cs 文件
        foreach (var file in Directory.GetFiles(@"..\..", "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains("test") || file.Contains("bin") || file.Contains("obj"))
                continue;
                
            var content = File.ReadAllText(file);
            var matches = Regex.Matches(content, pattern);
            
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                if (!codeKeys.ContainsKey(key))
                {
                    codeKeys[key] = new List<string>();
                }
                codeKeys[key].Add(Path.GetFileName(file));
            }
        }
        
        Console.WriteLine($"\n找到 {codeKeys.Count} 个代码中使用的键");
        
        // 3. 找出不匹配的键
        Console.WriteLine("\n不匹配的键：");
        var missingKeys = new List<string>();
        
        foreach (var kvp in codeKeys)
        {
            var codeKey = kvp.Key;
            var files = kvp.Value;
            
            // 尝试找到匹配的资源键
            var found = false;
            string matchedKey = null;
            
            // 直接匹配
            if (resourceKeys.Contains(codeKey))
            {
                found = true;
                matchedKey = codeKey;
            }
            else
            {
                // 尝试模糊匹配
                foreach (var resKey in resourceKeys)
                {
                    if (resKey.EndsWith("_" + codeKey) || resKey.Contains("_" + codeKey))
                    {
                        found = true;
                        matchedKey = resKey;
                        break;
                    }
                }
            }
            
            if (!found)
            {
                missingKeys.Add(codeKey);
                Console.WriteLine($"  键 '{codeKey}' 在文件 {string.Join(", ", files)} 中使用，但在资源文件中找不到");
            }
            else if (matchedKey != codeKey)
            {
                Console.WriteLine($"  键 '{codeKey}' 应该改为 '{matchedKey}' (在文件 {string.Join(", ", files)} 中)");
            }
        }
        
        // 4. 生成修复建议
        Console.WriteLine($"\n\n需要修复 {missingKeys.Count + codeKeys.Count} 个键");
        
        // 生成键映射
        Console.WriteLine("\n\n建议的键映射：");
        var keyMapping = new Dictionary<string, string>();
        
        foreach (var kvp in codeKeys)
        {
            var codeKey = kvp.Key;
            string suggestedKey = null;
            
            // 查找最可能的匹配
            foreach (var resKey in resourceKeys)
            {
                var resKeyLower = resKey.ToLower();
                var codeKeyLower = codeKey.ToLower();
                
                if (resKeyLower.EndsWith(codeKeyLower) || resKeyLower.Contains("_" + codeKeyLower))
                {
                    suggestedKey = resKey;
                    break;
                }
            }
            
            if (suggestedKey != null && suggestedKey != codeKey)
            {
                keyMapping[codeKey] = suggestedKey;
                Console.WriteLine($"  \"{codeKey}\" => \"{suggestedKey}\"");
            }
        }
    }
}