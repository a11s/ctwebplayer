using System;
using System.Collections.Generic;

namespace ctwebplayer
{
    /// <summary>
    /// 协议路由类，用于解析和存储ct://协议的路由信息
    /// </summary>
    public class ProtocolRoute
    {
        /// <summary>
        /// 协议名称（如：ct）
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 主机/命令（如：settings、about）
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 路径（如：/cookie、/proxy）
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 查询参数
        /// </summary>
        public Dictionary<string, string> QueryParameters { get; set; }

        /// <summary>
        /// 原始URL
        /// </summary>
        public string OriginalUrl { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolRoute()
        {
            QueryParameters = new Dictionary<string, string>();
            Protocol = string.Empty;
            Host = string.Empty;
            Path = string.Empty;
            OriginalUrl = string.Empty;
        }

        /// <summary>
        /// 从URL解析路由信息
        /// </summary>
        /// <param name="url">要解析的URL</param>
        /// <returns>解析后的ProtocolRoute对象，如果解析失败返回null</returns>
        public static ProtocolRoute? Parse(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                var route = new ProtocolRoute
                {
                    OriginalUrl = url
                };

                // 检查是否是ct://协议
                if (!url.StartsWith("ct://", StringComparison.OrdinalIgnoreCase))
                    return null;

                // 解析URI
                var uri = new Uri(url);
                
                route.Protocol = uri.Scheme;
                route.Host = uri.Host.ToLowerInvariant();
                route.Path = uri.AbsolutePath.ToLowerInvariant();

                // 解析查询参数
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var queryString = uri.Query.TrimStart('?');
                    var queryPairs = queryString.Split('&');
                    
                    foreach (var pair in queryPairs)
                    {
                        var keyValue = pair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            var key = Uri.UnescapeDataString(keyValue[0]);
                            var value = Uri.UnescapeDataString(keyValue[1]);
                            route.QueryParameters[key] = value;
                        }
                    }
                }

                return route;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"解析协议路由失败：{url}", ex);
                return null;
            }
        }

        /// <summary>
        /// 判断是否匹配指定的路由模式
        /// </summary>
        /// <param name="host">主机名</param>
        /// <param name="path">路径（可选）</param>
        /// <returns>是否匹配</returns>
        public bool Matches(string host, string? path = null)
        {
            if (!Host.Equals(host, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(path))
            {
                return Path.Equals(path, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        /// <summary>
        /// 获取完整的路由路径（host + path）
        /// </summary>
        /// <returns>完整路径</returns>
        public string GetFullPath()
        {
            if (string.IsNullOrEmpty(Path) || Path == "/")
                return Host;
            
            return $"{Host}{Path}";
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns>路由信息的字符串表示</returns>
        public override string ToString()
        {
            return $"Protocol: {Protocol}, Host: {Host}, Path: {Path}, Query: {QueryParameters.Count} params";
        }
    }
}