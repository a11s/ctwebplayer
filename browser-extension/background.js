/**
 * CTWebPlayer Resource Cache - Background Service Worker
 * 负责管理插件生命周期、资源拦截和缓存管理
 */

import { CacheManager } from './utils/cache-manager.js';
import { NativeMessaging } from './utils/native-messaging.js';
import { CACHE_CONFIG, NATIVE_HOST_NAME, DEBUG_MODE } from './config/constants.js';

// 初始化缓存管理器
const cacheManager = new CacheManager();
const nativeMessaging = new NativeMessaging(NATIVE_HOST_NAME);

// Service Worker 激活时初始化
self.addEventListener('activate', async (event) => {
  console.log('CTWebPlayer Cache Extension activated');
  await cacheManager.init();
  if (DEBUG_MODE) {
    console.log('Debug mode enabled');
    console.log('Configured domains:', CACHE_CONFIG.domains);
    console.log('Cache patterns:', CACHE_CONFIG.patterns);
    console.log('Path patterns:', CACHE_CONFIG.pathPatterns);
  }
});

// 监听来自 CTWebPlayer 的 Native Messaging 消息
nativeMessaging.onMessage((message) => {
  console.log('Received message from CTWebPlayer:', message);
  
  if (message.type === 'navigate') {
    // 创建新标签页并导航到游戏 URL
    chrome.tabs.create({
      url: message.url,
      active: true
    }, (tab) => {
      // 如果有认证信息，设置 cookies
      if (message.cookies) {
        const url = new URL(message.url);
        message.cookies.forEach(cookie => {
          chrome.cookies.set({
            url: message.url,
            name: cookie.name,
            value: cookie.value,
            domain: url.hostname,
            path: cookie.path || '/'
          });
        });
      }
    });
  }
});

// 资源请求拦截 - 在请求发送前检查缓存
chrome.webRequest.onBeforeRequest.addListener(
  async (details) => {
    if (DEBUG_MODE) {
      console.log(`[Request] ${details.method} ${details.url}`);
      const url = new URL(details.url);
      console.log(`[Request] Domain: ${url.hostname}, Path: ${url.pathname}`);
    }
    
    // 检查是否为需要缓存的资源
    const shouldCacheResult = shouldCache(details.url);
    if (DEBUG_MODE) {
      console.log(`[Cache Decision] URL: ${details.url}, Should Cache: ${shouldCacheResult}`);
    }
    
    if (shouldCacheResult) {
      // 尝试从缓存获取
      const cachedData = await cacheManager.get(details.url);
      if (cachedData) {
        console.log(`[Cache Hit] ${details.url}`);
        if (DEBUG_MODE) {
          console.log(`[Cache Hit] Size: ${(cachedData.size / 1024).toFixed(2)} KB`);
        }
        // 返回缓存的数据
        return {
          redirectUrl: createDataUrl(cachedData)
        };
      } else {
        if (DEBUG_MODE) {
          console.log(`[Cache Miss] ${details.url}`);
        }
      }
    }
    return {};
  },
  { urls: ["<all_urls>"] },
  ["blocking"]
);

// 响应完成后缓存资源
chrome.webRequest.onCompleted.addListener(
  async (details) => {
    if (shouldCache(details.url) && details.statusCode === 200) {
      console.log(`[Caching] Starting to cache: ${details.url}`);
      // 获取响应内容并缓存
      try {
        // 添加调试日志
        if (DEBUG_MODE) {
          console.log(`[Caching] Request headers from original request:`, details.requestHeaders);
          console.log(`[Caching] Response headers from original request:`, details.responseHeaders);
        }
        
        // 尝试使用原始请求的头信息
        const fetchOptions = {
          method: 'GET',
          mode: 'no-cors', // 尝试使用 no-cors 模式
          credentials: 'include', // 包含 cookies
          headers: {}
        };
        
        // 复制原始请求的重要头信息
        if (details.requestHeaders) {
          const importantHeaders = ['Referer', 'User-Agent', 'Cookie', 'Authorization'];
          details.requestHeaders.forEach(header => {
            if (importantHeaders.includes(header.name)) {
              fetchOptions.headers[header.name] = header.value;
              if (DEBUG_MODE) {
                console.log(`[Caching] Copying header: ${header.name} = ${header.value}`);
              }
            }
          });
        }
        
        console.log(`[Caching] Fetching with options:`, fetchOptions);
        const response = await fetch(details.url, fetchOptions);
        
        if (!response.ok) {
          console.error(`[Caching] Fetch failed: ${response.status} ${response.statusText}`);
          if (DEBUG_MODE) {
            console.log(`[Caching] Response headers:`, response.headers);
          }
          return;
        }
        
        const data = await response.arrayBuffer();
        
        // 对于大文件（如 .data 和 .wasm），记录文件大小
        const fileSize = data.byteLength;
        const fileSizeMB = (fileSize / (1024 * 1024)).toFixed(2);
        if (fileSize > 10 * 1024 * 1024) { // 大于 10MB
          console.log(`[Caching] Large file: ${details.url} (${fileSizeMB} MB)`);
        }
        
        if (DEBUG_MODE) {
          console.log(`[Caching] File size: ${fileSizeMB} MB`);
          console.log(`[Caching] Content-Type: ${response.headers.get('content-type')}`);
        }
        
        const metadata = {
          contentType: response.headers.get('content-type'),
          headers: Object.fromEntries(response.headers.entries()),
          size: fileSize
        };
        await cacheManager.store(details.url, data, metadata);
        console.log(`[Caching] Successfully cached: ${details.url}`);
      } catch (error) {
        console.error(`[Caching] Failed to cache: ${details.url}`, error);
        if (DEBUG_MODE) {
          console.error(`[Caching] Error details:`, {
            message: error.message,
            stack: error.stack,
            url: details.url,
            statusCode: details.statusCode
          });
        }
      }
    } else if (DEBUG_MODE && details.statusCode !== 200) {
      console.log(`[Response] Non-200 status: ${details.url}, Status: ${details.statusCode}`);
    }
  },
  { urls: ["<all_urls>"] }
);

// 监听来自 content script 的消息
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.type === 'getCacheStats') {
    // 返回缓存统计信息
    cacheManager.getStats().then(stats => {
      sendResponse(stats);
    });
    return true; // 表示异步响应
  }
  
  if (request.type === 'clearCache') {
    // 清除缓存
    cacheManager.clear().then(() => {
      sendResponse({ success: true });
    });
    return true;
  }
});

// 监听来自外部的消息（CTWebPlayer）
chrome.runtime.onMessageExternal.addListener(
  (request, sender, sendResponse) => {
    console.log('External message received:', request);
    
    if (request.type === 'navigate') {
      // 创建新标签页并导航到游戏 URL
      chrome.tabs.create({
        url: request.url,
        active: true
      }, (tab) => {
        // 注入认证信息
        if (request.authToken) {
          chrome.cookies.set({
            url: request.url,
            name: 'auth_token',
            value: request.authToken
          });
        }
        sendResponse({ success: true, tabId: tab.id });
      });
      return true; // 异步响应
    }
  }
);

/**
 * 判断资源是否需要缓存
 * @param {string} url - 资源 URL
 * @returns {boolean}
 */
function shouldCache(url) {
  try {
    const urlObj = new URL(url);
    
    // 检查域名是否在允许列表中
    const domainMatch = CACHE_CONFIG.domains.some(domain => {
      if (typeof domain === 'string') {
        return urlObj.hostname === domain || urlObj.hostname.endsWith('.' + domain);
      } else if (domain instanceof RegExp) {
        return domain.test(url);
      }
      return false;
    });
    
    if (!domainMatch) {
      if (DEBUG_MODE) {
        console.log(`[Domain Check] Domain not in whitelist: ${urlObj.hostname}`);
      }
      return false;
    }
    
    // 检查排除模式
    if (CACHE_CONFIG.excludePatterns && CACHE_CONFIG.excludePatterns.some(pattern => pattern.test(url))) {
      if (DEBUG_MODE) {
        console.log(`[Exclude Pattern] URL excluded from caching: ${url}`);
      }
      return false;
    }
    
    // 首先检查路径模式（优先级高于扩展名）
    if (CACHE_CONFIG.pathPatterns && CACHE_CONFIG.pathPatterns.some(pattern => pattern.test(url))) {
      if (DEBUG_MODE) {
        console.log(`[Path Pattern] Matched for caching: ${url}`);
      }
      return true;
    }
    
    // 检查 URL 是否匹配缓存模式
    // 注意：.data 和 .wasm 文件可能很大（几十MB到几百MB）
    // 这些文件对于 Unity WebGL 游戏至关重要，需要确保缓存
    const patternMatch = CACHE_CONFIG.patterns.some(pattern => pattern.test(url));
    if (DEBUG_MODE && patternMatch) {
      console.log(`[File Pattern] Matched for caching: ${url}`);
    }
    
    return patternMatch;
  } catch (error) {
    if (DEBUG_MODE) {
      console.error(`[shouldCache] Error processing URL: ${url}`, error);
    }
    return false;
  }
}

/**
 * 创建 Data URL
 * @param {Object} cachedData - 缓存的数据对象
 * @returns {string} Data URL
 */
function createDataUrl(cachedData) {
  const { data, metadata } = cachedData;
  const contentType = metadata.contentType || 'application/octet-stream';
  
  // 将 ArrayBuffer 转换为 Base64
  const base64 = btoa(String.fromCharCode(...new Uint8Array(data)));
  return `data:${contentType};base64,${base64}`;
}

// 定期清理过期缓存
setInterval(async () => {
  console.log('Running cache cleanup...');
  await cacheManager.cleanup();
}, 60 * 60 * 1000); // 每小时执行一次

// 监听插件安装/更新事件
chrome.runtime.onInstalled.addListener((details) => {
  if (details.reason === 'install') {
    console.log('[Extension] Installed successfully');
    console.log('[Extension] Version:', chrome.runtime.getManifest().version);
    if (DEBUG_MODE) {
      console.log('[Extension] Debug mode is ON');
      console.log('[Extension] Monitored domains:', CACHE_CONFIG.domains);
    }
    // 打开配置页面或显示欢迎信息
    chrome.tabs.create({
      url: chrome.runtime.getURL('popup.html')
    });
  } else if (details.reason === 'update') {
    console.log('[Extension] Updated to version', chrome.runtime.getManifest().version);
    if (DEBUG_MODE) {
      console.log('[Extension] Debug mode is ON');
    }
  }
});

// 插件启动时输出初始化日志
console.log('[Extension] CTWebPlayer Cache Extension starting...');
console.log('[Extension] Version:', chrome.runtime.getManifest().version);
if (DEBUG_MODE) {
  console.log('[Extension] Debug mode enabled');
  console.log('[Extension] Configured domains:', CACHE_CONFIG.domains);
  console.log('[Extension] Cache patterns:', CACHE_CONFIG.patterns);
}