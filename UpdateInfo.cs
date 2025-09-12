using System;
using System.Collections.Generic;

namespace CTWebPlayer
{
    /// <summary>
    /// 更新信息类，用于存储从 GitHub Release 获取的更新信息
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// 版本号（如：1.2.0）
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 发布标签（如：v1.2.0）
        /// </summary>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// 发布名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 发布说明（Markdown 格式）
        /// </summary>
        public string ReleaseNotes { get; set; } = string.Empty;

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// 是否为预发布版本
        /// </summary>
        public bool IsPreRelease { get; set; }

        /// <summary>
        /// 下载 URL
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// SHA256 哈希值（用于文件完整性验证）
        /// </summary>
        public string SHA256Hash { get; set; } = string.Empty;

        /// <summary>
        /// 是否为强制更新
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// 最小支持版本（低于此版本必须更新）
        /// </summary>
        public string MinimumVersion { get; set; } = string.Empty;

        /// <summary>
        /// 检查当前版本是否需要更新
        /// </summary>
        /// <returns>如果需要更新返回 true</returns>
        public bool IsUpdateRequired()
        {
            // 移除版本号前的 'v' 前缀
            string cleanVersion = Version;
            if (cleanVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                cleanVersion = cleanVersion.Substring(1);
            }

            // 使用 CTWebPlayer.Version 类的比较方法
            return CTWebPlayer.Version.CompareVersion(cleanVersion) < 0;
        }

        /// <summary>
        /// 检查是否为强制更新
        /// </summary>
        /// <returns>如果是强制更新返回 true</returns>
        public bool IsUpdateMandatory()
        {
            if (IsMandatory)
                return true;

            if (!string.IsNullOrEmpty(MinimumVersion))
            {
                // 如果当前版本低于最小支持版本，则为强制更新
                return CTWebPlayer.Version.CompareVersion(MinimumVersion) < 0;
            }

            return false;
        }

        /// <summary>
        /// 获取格式化的文件大小
        /// </summary>
        /// <returns>格式化后的文件大小字符串</returns>
        public string GetFormattedFileSize()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSize;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 从 GitHub Release API 响应创建 UpdateInfo 对象
        /// </summary>
        /// <param name="releaseJson">GitHub Release API 的 JSON 响应</param>
        /// <returns>UpdateInfo 对象</returns>
        public static UpdateInfo FromGitHubRelease(dynamic releaseJson)
        {
            var info = new UpdateInfo
            {
                TagName = releaseJson.tag_name?.ToString() ?? string.Empty,
                Name = releaseJson.name?.ToString() ?? releaseJson.tag_name?.ToString() ?? string.Empty,
                ReleaseNotes = releaseJson.body?.ToString() ?? string.Empty,
                PublishedAt = DateTime.Parse(releaseJson.published_at?.ToString() ?? DateTime.Now.ToString()),
                IsPreRelease = releaseJson.prerelease ?? false
            };

            // 从标签名中提取版本号
            info.Version = info.TagName;
            if (info.Version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                info.Version = info.Version.Substring(1);
            }

            // 查找可执行文件资源
            if (releaseJson.assets != null)
            {
                foreach (var asset in releaseJson.assets)
                {
                    string? assetName = asset.name?.ToString()?.ToLower();
                    if (!string.IsNullOrEmpty(assetName) && assetName.EndsWith(".exe") && assetName.Contains("ctwebplayer"))
                    {
                        info.DownloadUrl = asset.browser_download_url?.ToString() ?? string.Empty;
                        info.FileName = asset.name?.ToString() ?? string.Empty;
                        info.FileSize = asset.size ?? 0;
                        break;
                    }
                }
            }

            // 从发布说明中提取 SHA256 哈希值
            info.ExtractSHA256FromReleaseNotes();

            // 检查是否为强制更新（从发布说明中查找特定标记）
            if (info.ReleaseNotes.Contains("[MANDATORY]", StringComparison.OrdinalIgnoreCase))
            {
                info.IsMandatory = true;
            }

            return info;
        }

        /// <summary>
        /// 从发布说明中提取 SHA256 哈希值
        /// </summary>
        private void ExtractSHA256FromReleaseNotes()
        {
            if (string.IsNullOrEmpty(ReleaseNotes) || string.IsNullOrEmpty(FileName))
                return;

            // 查找格式如：SHA256: abc123... 或 abc123... *filename.exe
            var lines = ReleaseNotes.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // 检查是否包含文件名和 SHA256 格式
                if (line.Contains(FileName, StringComparison.OrdinalIgnoreCase))
                {
                    // 提取 64 个字符的十六进制字符串
                    var match = System.Text.RegularExpressions.Regex.Match(
                        line, 
                        @"\b[a-fA-F0-9]{64}\b"
                    );
                    
                    if (match.Success)
                    {
                        SHA256Hash = match.Value.ToUpper();
                        return;
                    }
                }
            }
        }
    }
}