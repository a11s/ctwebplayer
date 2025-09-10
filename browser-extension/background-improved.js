/**
 * CTWebPlayer Resource Cache - Background Service Worker (改进版)
 * 使用 webRequest API 的 onBeforeRequest 和 filterResponseData 来避免 CORS 问题
 */

import { CacheManager } from './utils/cache-manager.js';
import { NativeMessaging } from './utils/native-messaging.js';
import { CACHE_CONFIG, NATIVE_HOST_NAME, DEBUG_MODE } from './config/constants.js';

// 初始化缓存管理器
const cacheManager = new CacheManager();
const nativeMessaging = new NativeMessaging(NATIVE_HOST_NAME);

// 存储待缓存的响应数据
const pendingCacheData = new Map();

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
        // 标记这个请求需要缓存
        pendingCacheData.set(details.requestId, {
          url: details.url,
          shouldCache: true
        });
      }
    }
    return {};
  },
  { urls: ["<all_urls>"] },
  ["blocking"]
);

// 使用 onBeforeRequest 的 filterResponseData 来捕获响应数据
chrome.webRequest.onBeforeRequest.addListener(
  (details) => {
    const pendingCache = pendingCacheData.get(details.requestId);
    if (pendingCache && pendingCache.shouldCache) {
      // 创建一个过滤器来捕获响应数据
      const filter = chrome.webRequest.filterResponseData(details.requestId);
      const chunks = [];
      
      filter.ondata = (event) => {
        // 收集数据块
        chunks.push(new Uint8Array(event.data));
        // 将数据传递给页面
        filter.write(event.data);
      };
      
      filter.onstop = async () => {
        // 合并所有数据块
        const totalLength = chunks.reduce((sum, chunk) => sum + chunk.length, 0);
        const data = new Uint8Array(totalLength);
        let offset = 0;
        for (const chunk of chunks) {
          data.set(chunk, offset);
          offset += chunk.length;
        }
        
        // 缓存数据
        try {
          const metadata = {
            contentType: details.responseHeaders?.find(h => h.name.toLowerCase() === 'content-type')?.value,
            headers: details.responseHeaders?.reduce((acc, h) => {
              acc[h.name] = h.value;
              return acc;
            }, {}),
            size: data.byteLength
          };
          
          await cacheManager.store(pendingCache.url, data.buffer, metadata);
          console.log(`[Caching] Successfully cached: ${pendingCache.url}`);
        } catch (error) {
          console.error(`[Caching] Failed to cache: ${pendingCache.url}`, error);
        }
        
        // 清理
        pendingCacheData.delete(details.requestId);
        filter.close();
      };
      
      filter.onerror = () => {
        console.error(`[Filter] Error processing response for: ${pendingCache.url}`);
        pendingCacheData.delete(details.requestId);
        filter.close();
      };
    }
  },
  { urls: ["<all_urls>"] },
  ["blocking", "responseHeaders"]
);

// 清理未完成的请求
chrome.webRequest.onCompleted.addListener(
  (details) => {
    pendingCacheData.delete(details.requestId);
  },
  { urls: ["<all_urls>"] }
);

chrome.webRequest.onErrorOccurred.addListener(
  (details) => {
    pendingCacheData.delete(details.requestId);
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