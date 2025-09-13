using System;

namespace ctwebplayer
{
    /// <summary>
    /// 版本管理类，包含应用程序版本相关的常量和方法
    /// </summary>
    public static class Version
    {
        /// <summary>
        /// 主版本号 - 不兼容的 API 修改时增�?
        /// </summary>
        public const int Major = 1;

        /// <summary>
        /// 次版本号 - 向下兼容的功能性新增时增加
        /// </summary>
        public const int Minor = 0;

        /// <summary>
        /// 修订�?- 向下兼容的问题修正时增加
        /// </summary>
        public const int Patch = 0;

        /// <summary>
        /// 预发布版本标识（如：alpha、beta、rc），正式版为�?
        /// </summary>
        public const string PreRelease = "";

        /// <summary>
        /// 构建元数据，可以包含构建日期、提交哈希等
        /// </summary>
        public const string BuildMetadata = "";

        /// <summary>
        /// 获取完整的版本号字符串（遵循 SemVer 规范�?
        /// </summary>
        public static string FullVersion
        {
            get
            {
                var version = $"{Major}.{Minor}.{Patch}";
                
                if (!string.IsNullOrEmpty(PreRelease))
                {
                    version += $"-{PreRelease}";
                }
                
                if (!string.IsNullOrEmpty(BuildMetadata))
                {
                    version += $"+{BuildMetadata}";
                }
                
                return version;
            }
        }

        /// <summary>
        /// 获取程序集版本（用于 AssemblyVersion�?
        /// </summary>
        public static string AssemblyVersion => $"{Major}.{Minor}.{Patch}.0";

        /// <summary>
        /// 获取文件版本（用�?FileVersion�?
        /// </summary>
        public static string FileVersion => $"{Major}.{Minor}.{Patch}.0";

        /// <summary>
        /// 获取产品版本（用�?ProductVersion�?
        /// </summary>
        public static string ProductVersion => FullVersion;

        /// <summary>
        /// 获取版本发布日期
        /// </summary>
        public static DateTime ReleaseDate => new DateTime(2025, 9, 12);

        /// <summary>
        /// 获取版本描述信息
        /// </summary>
        public static string Description => "CTWebPlayer - Unity3D WebPlayer 专属浏览�?;

        /// <summary>
        /// 检查是否为预发布版�?
        /// </summary>
        public static bool IsPreRelease => !string.IsNullOrEmpty(PreRelease);

        /// <summary>
        /// 比较版本�?
        /// </summary>
        /// <param name="otherVersion">要比较的版本号字符串</param>
        /// <returns>如果当前版本较新返回 1，相同返�?0，较旧返�?-1</returns>
        public static int CompareVersion(string otherVersion)
        {
            try
            {
                // 移除 'v' 前缀（如果存在）
                if (otherVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    otherVersion = otherVersion.Substring(1);
                }

                var current = new System.Version(Major, Minor, Patch);
                var other = System.Version.Parse(otherVersion.Split('-')[0]); // 移除预发布标�?
                
                return current.CompareTo(other);
            }
            catch
            {
                return 0; // 如果解析失败，认为版本相�?
            }
        }
    }
}
