using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ctwebplayer
{
    /// <summary>
    /// 管理 WebView2 的 Cookie 操作
    /// </summary>
    public class CookieManager
    {
        private readonly CoreWebView2CookieManager _cookieManager;
        private readonly string _baseDomain;
        private readonly CoreWebView2 _coreWebView2;

        /// <summary>
        /// Cookie 检测结果
        /// </summary>
        private class CookieDetectionResult
        {
            public bool found { get; set; }
            public string domain { get; set; } = "";
            public string url { get; set; } = "";
            public int cookieCount { get; set; }
            public List<CookieInfo>? cookies { get; set; }
        }

        /// <summary>
        /// Cookie 信息
        /// </summary>
        private class CookieInfo
        {
            public string name { get; set; } = "";
            public string value { get; set; } = "";
        }

        /// <summary>
        /// 初始化 CookieManager
        /// </summary>
        /// <param name="cookieManager">WebView2 的 CoreWebView2CookieManager 实例</param>
        /// <param name="baseDomain">基础域名，如 "ero-labs.live"</param>
        public CookieManager(CoreWebView2CookieManager cookieManager, string baseDomain)
        {
            _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
            _baseDomain = baseDomain ?? throw new ArgumentNullException(nameof(baseDomain));
            _coreWebView2 = null!;
        }

        /// <summary>
        /// 初始化 CookieManager（新构造函数，支持 JavaScript 检测）
        /// </summary>
        /// <param name="cookieManager">WebView2 的 CoreWebView2CookieManager 实例</param>
        /// <param name="baseDomain">基础域名，如 "ero-labs.live"</param>
        /// <param name="coreWebView2">CoreWebView2 实例，用于执行 JavaScript</param>
        public CookieManager(CoreWebView2CookieManager cookieManager, string baseDomain, CoreWebView2 coreWebView2)
        {
            _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
            _baseDomain = baseDomain ?? throw new ArgumentNullException(nameof(baseDomain));
            _coreWebView2 = coreWebView2 ?? throw new ArgumentNullException(nameof(coreWebView2));
        }

        /// <summary>
        /// 检查指定 cookie 是否存在
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <returns>如果存在返回 true，否则返回 false</returns>
        public async Task<bool> HasCookieAsync(string cookieName)
        {
            try
            {
                // 如果有 CoreWebView2 实例，优先使用 JavaScript 方法
                if (_coreWebView2 != null)
                {
                    LogManager.Instance.Info($"使用 JavaScript 方法检测 cookie: {cookieName}");
                    return await HasCookieJsAsync(cookieName);
                }

                // 否则使用原有的 CookieManager API 方法
                LogManager.Instance.Info($"使用 CookieManager API 方法检测 cookie: {cookieName}");
                
                // 检查完整域名的 cookies
                var uri = new Uri($"https://{_baseDomain}");
                var cookies = await GetCookiesAsync(uri.ToString());
                
                LogManager.Instance.Info($"检查域名 {_baseDomain} 的 cookies，找到 {cookies.Count} 个");
                foreach (var cookie in cookies)
                {
                    LogManager.Instance.Debug($"Cookie: {cookie.Name} = {cookie.Value} (Domain: {cookie.Domain})");
                }
                
                // 检查 cookie 是否存在且有有效值
                var targetCookie = cookies.FirstOrDefault(c => c.Name == cookieName);
                if (targetCookie != null)
                {
                    if (!string.IsNullOrWhiteSpace(targetCookie.Value))
                    {
                        LogManager.Instance.Info($"在域名 {_baseDomain} 找到有效的 cookie: {cookieName} = {targetCookie.Value}");
                        return true;
                    }
                    else
                    {
                        LogManager.Instance.Warning($"在域名 {_baseDomain} 找到 cookie '{cookieName}'，但值为空或只包含空格");
                    }
                }
                
                // 如果域名包含子域名，也检查父域名的 cookies
                // 例如：如果是 mg.ero-labs.live，也检查 .ero-labs.live
                var parts = _baseDomain.Split('.');
                if (parts.Length > 2)
                {
                    var parentDomain = string.Join(".", parts.Skip(1));
                    var parentUri = new Uri($"https://{parentDomain}");
                    var parentCookies = await GetCookiesAsync(parentUri.ToString());
                    
                    LogManager.Instance.Info($"检查父域名 {parentDomain} 的 cookies，找到 {parentCookies.Count} 个");
                    
                    // 检查父域名的 cookie 是否存在且有有效值
                    var parentTargetCookie = parentCookies.FirstOrDefault(c => c.Name == cookieName);
                    if (parentTargetCookie != null)
                    {
                        if (!string.IsNullOrWhiteSpace(parentTargetCookie.Value))
                        {
                            LogManager.Instance.Info($"在父域名 {parentDomain} 找到有效的 cookie: {cookieName} = {parentTargetCookie.Value}");
                            return true;
                        }
                        else
                        {
                            LogManager.Instance.Warning($"在父域名 {parentDomain} 找到 cookie '{cookieName}'，但值为空或只包含空格");
                        }
                    }
                }
                
                LogManager.Instance.Info($"未找到 cookie: {cookieName}");
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"检查 Cookie 失败: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 使用 JavaScript 检查指定 cookie 是否存在
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <returns>如果存在返回 true，否则返回 false</returns>
        public async Task<bool> HasCookieJsAsync(string cookieName)
        {
            try
            {
                if (_coreWebView2 == null)
                {
                    LogManager.Instance.Warning("CoreWebView2 实例为空，无法使用 JavaScript 检测 cookie");
                    return false;
                }

                // 使用 JavaScript 检测 cookie
                var script = $@"
                    (function() {{
                        var cookies = document.cookie.split(';');
                        var allCookies = [];
                        var targetCookieName = '{cookieName}';
                        var found = false;
                        var targetCookieValue = '';
                        
                        for (var i = 0; i < cookies.length; i++) {{
                            var cookie = cookies[i].trim();
                            if (cookie) {{
                                var parts = cookie.split('=');
                                var name = parts[0];
                                var value = parts.slice(1).join('=');
                                allCookies.push({{name: name, value: value}});
                                
                                if (name === targetCookieName) {{
                                    targetCookieValue = value;
                                    // 检查 cookie 值是否有效（不为空且不只包含空格）
                                    if (value && value.trim().length > 0) {{
                                        found = true;
                                    }}
                                }}
                            }}
                        }}
                        
                        // 记录所有检测到的 cookies 用于调试
                        console.log('All cookies detected:', allCookies);
                        console.log('Current domain:', window.location.hostname);
                        console.log('Target cookie name:', targetCookieName);
                        console.log('Target cookie value:', targetCookieValue);
                        console.log('Cookie found and valid:', found);
                        
                        return JSON.stringify({{
                            found: found,
                            domain: window.location.hostname,
                            url: window.location.href,
                            cookieCount: allCookies.length,
                            cookies: allCookies,
                            targetCookieName: targetCookieName,
                            targetCookieValue: targetCookieValue
                        }});
                    }})();
                ";

                var result = await _coreWebView2.ExecuteScriptAsync(script);
                
                // 解析结果（结果是 JSON 格式的字符串）
                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    // 移除可能的外层引号并解析 JSON
                    var jsonResult = result.Trim('"').Replace("\\\"", "\"");
                    var data = System.Text.Json.JsonSerializer.Deserialize<CookieDetectionResult>(jsonResult);
                    
                    if (data != null)
                    {
                        LogManager.Instance.Info($"JavaScript Cookie 检测结果：");
                        LogManager.Instance.Info($"  - 当前域名：{data.domain}");
                        LogManager.Instance.Info($"  - 当前 URL：{data.url}");
                        LogManager.Instance.Info($"  - Cookie 总数：{data.cookieCount}");
                        LogManager.Instance.Info($"  - 目标 Cookie 存在且有效：{data.found}");
                        
                        // 记录目标 cookie 的值
                        if (data.cookies != null)
                        {
                            var targetCookie = data.cookies.FirstOrDefault(c => c.name == cookieName);
                            if (targetCookie != null)
                            {
                                if (string.IsNullOrWhiteSpace(targetCookie.value))
                                {
                                    LogManager.Instance.Warning($"  - 目标 Cookie '{cookieName}' 存在但值为空或只包含空格");
                                }
                                else
                                {
                                    LogManager.Instance.Info($"  - 目标 Cookie '{cookieName}' 值：{targetCookie.value}");
                                }
                            }
                            
                            // 记录所有检测到的 cookies（调试级别）
                            foreach (var cookie in data.cookies)
                            {
                                LogManager.Instance.Debug($"  - Cookie: {cookie.name} = {cookie.value}");
                            }
                        }
                        
                        return data.found;
                    }
                }
                
                LogManager.Instance.Warning("JavaScript Cookie 检测返回空结果");
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"JavaScript Cookie 检测失败: {ex.Message}", ex);
                // 如果 JavaScript 方法失败，回退到原方法
                return await HasCookieApiAsync(cookieName);
            }
        }

        /// <summary>
        /// 使用 CookieManager API 检查指定 cookie 是否存在（原方法）
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <returns>如果存在返回 true，否则返回 false</returns>
        private async Task<bool> HasCookieApiAsync(string cookieName)
        {
            try
            {
                // 检查完整域名的 cookies
                var uri = new Uri($"https://{_baseDomain}");
                var cookies = await GetCookiesAsync(uri.ToString());
                
                LogManager.Instance.Info($"API 方法：检查域名 {_baseDomain} 的 cookies，找到 {cookies.Count} 个");
                foreach (var cookie in cookies)
                {
                    LogManager.Instance.Debug($"Cookie: {cookie.Name} = {cookie.Value} (Domain: {cookie.Domain})");
                }
                
                // 检查 cookie 是否存在且有有效值
                var targetCookie = cookies.FirstOrDefault(c => c.Name == cookieName);
                if (targetCookie != null)
                {
                    if (!string.IsNullOrWhiteSpace(targetCookie.Value))
                    {
                        LogManager.Instance.Info($"API 方法：在域名 {_baseDomain} 找到有效的 cookie: {cookieName} = {targetCookie.Value}");
                        return true;
                    }
                    else
                    {
                        LogManager.Instance.Warning($"API 方法：在域名 {_baseDomain} 找到 cookie '{cookieName}'，但值为空或只包含空格");
                    }
                }
                
                // 如果域名包含子域名，也检查父域名的 cookies
                // 例如：如果是 mg.ero-labs.live，也检查 .ero-labs.live
                var parts = _baseDomain.Split('.');
                if (parts.Length > 2)
                {
                    var parentDomain = string.Join(".", parts.Skip(1));
                    var parentUri = new Uri($"https://{parentDomain}");
                    var parentCookies = await GetCookiesAsync(parentUri.ToString());
                    
                    LogManager.Instance.Info($"API 方法：检查父域名 {parentDomain} 的 cookies，找到 {parentCookies.Count} 个");
                    
                    // 检查父域名的 cookie 是否存在且有有效值
                    var parentTargetCookie = parentCookies.FirstOrDefault(c => c.Name == cookieName);
                    if (parentTargetCookie != null)
                    {
                        if (!string.IsNullOrWhiteSpace(parentTargetCookie.Value))
                        {
                            LogManager.Instance.Info($"API 方法：在父域名 {parentDomain} 找到有效的 cookie: {cookieName} = {parentTargetCookie.Value}");
                            return true;
                        }
                        else
                        {
                            LogManager.Instance.Warning($"API 方法：在父域名 {parentDomain} 找到 cookie '{cookieName}'，但值为空或只包含空格");
                        }
                    }
                }
                
                LogManager.Instance.Info($"API 方法：未找到 cookie: {cookieName}");
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"API Cookie 检查失败: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取指定 URI 的所有 cookies
        /// </summary>
        /// <param name="uri">目标 URI</param>
        /// <returns>Cookie 列表</returns>
        private async Task<List<CoreWebView2Cookie>> GetCookiesAsync(string uri)
        {
            var tcs = new TaskCompletionSource<List<CoreWebView2Cookie>>();
            var cookies = new List<CoreWebView2Cookie>();

            try
            {
                await Task.Run(() =>
                {
                    _cookieManager.GetCookiesAsync(uri).ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully && task.Result != null)
                        {
                            foreach (var cookie in task.Result)
                            {
                                cookies.Add(cookie);
                            }
                        }
                        tcs.SetResult(cookies);
                    });
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"获取 Cookies 失败: {ex.Message}");
                tcs.SetResult(cookies);
                return cookies;
            }
        }

        /// <summary>
        /// 设置 Cookie
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <param name="value">Cookie 值</param>
        /// <param name="domain">域名</param>
        /// <param name="path">路径</param>
        /// <returns>设置是否成功</returns>
        public async Task<bool> SetCookieAsync(string name, string value, string domain = null, string path = "/")
        {
            try
            {
                domain = domain ?? _baseDomain;
                var cookie = _cookieManager.CreateCookie(name, value, domain, path);
                await Task.Run(() => _cookieManager.AddOrUpdateCookie(cookie));
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"设置 Cookie 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除指定名称的 Cookie
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <returns>删除是否成功</returns>
        public async Task<bool> DeleteCookieAsync(string cookieName)
        {
            try
            {
                var uri = new Uri($"https://{_baseDomain}");
                await Task.Run(() => _cookieManager.DeleteCookies(cookieName, uri.ToString()));
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"删除 Cookie 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除所有 Cookies
        /// </summary>
        /// <returns>删除是否成功</returns>
        public async Task<bool> DeleteAllCookiesAsync()
        {
            try
            {
                await Task.Run(() => _cookieManager.DeleteAllCookies());
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"删除所有 Cookies 失败: {ex.Message}");
                return false;
            }
        }
    }
}