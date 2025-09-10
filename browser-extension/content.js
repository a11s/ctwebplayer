/**
 * CTWebPlayer Resource Cache - Content Script
 * 注入到云游戏页面，监听和修改 DOM，与 background script 通信
 */

console.log('CTWebPlayer Cache Extension - Content Script loaded');

// 监听页面加载完成
document.addEventListener('DOMContentLoaded', () => {
  console.log('Page loaded, checking for game iframe...');
  
  // 监听 iframe 的动态加载
  observeIframeCreation();
  
  // 检查现有的 iframe
  checkExistingIframes();
});

/**
 * 使用 MutationObserver 监听 iframe 的创建
 */
function observeIframeCreation() {
  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      mutation.addedNodes.forEach((node) => {
        if (node.tagName === 'IFRAME' && isGameIframe(node)) {
          console.log('Game iframe detected:', node.src);
          handleGameIframe(node);
        }
      });
    });
  });
  
  // 监听整个文档的变化
  observer.observe(document.body, {
    childList: true,
    subtree: true
  });
}

/**
 * 检查页面上现有的 iframe
 */
function checkExistingIframes() {
  const iframes = document.querySelectorAll('iframe');
  iframes.forEach((iframe) => {
    if (isGameIframe(iframe)) {
      console.log('Found existing game iframe:', iframe.src);
      handleGameIframe(iframe);
    }
  });
}

/**
 * 判断是否为游戏 iframe
 * @param {HTMLIFrameElement} iframe
 * @returns {boolean}
 */
function isGameIframe(iframe) {
  // 根据 iframe 的属性判断是否为游戏 iframe
  const src = iframe.src || '';
  const id = iframe.id || '';
  const className = iframe.className || '';
  
  // 检查常见的游戏 iframe 标识
  return (
    id.includes('game') ||
    className.includes('game') ||
    src.includes('/game/') ||
    src.includes('cloudgame.com')
  );
}

/**
 * 处理游戏 iframe
 * @param {HTMLIFrameElement} iframe
 */
function handleGameIframe(iframe) {
  // 发送消息给 background script
  chrome.runtime.sendMessage({
    type: 'gameIframeDetected',
    url: iframe.src,
    timestamp: Date.now()
  });
  
  // 尝试注入性能监控脚本
  injectPerformanceMonitor(iframe);
}

/**
 * 注入性能监控脚本到 iframe
 * @param {HTMLIFrameElement} iframe
 */
function injectPerformanceMonitor(iframe) {
  // 等待 iframe 加载完成
  iframe.addEventListener('load', () => {
    try {
      // 尝试访问 iframe 内容（可能因跨域而失败）
      const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
      
      // 创建性能监控脚本
      const script = iframeDoc.createElement('script');
      script.textContent = `
        // 监控资源加载性能
        const observer = new PerformanceObserver((list) => {
          const entries = list.getEntries();
          entries.forEach((entry) => {
            if (entry.entryType === 'resource') {
              // 发送资源加载信息到父窗口
              window.parent.postMessage({
                type: 'resourceTiming',
                name: entry.name,
                duration: entry.duration,
                size: entry.transferSize
              }, '*');
            }
          });
        });
        observer.observe({ entryTypes: ['resource'] });
      `;
      iframeDoc.head.appendChild(script);
      console.log('Performance monitor injected into iframe');
    } catch (error) {
      console.log('Cannot access iframe content (cross-origin):', error.message);
    }
  });
}

// 监听来自 iframe 的消息
window.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'resourceTiming') {
    // 转发资源加载信息给 background script
    chrome.runtime.sendMessage({
      type: 'resourceTiming',
      data: event.data
    });
  }
});

// 添加页面性能统计显示
function createPerformanceOverlay() {
  const overlay = document.createElement('div');
  overlay.id = 'ctwebplayer-performance-overlay';
  overlay.style.cssText = `
    position: fixed;
    top: 10px;
    right: 10px;
    background: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 10px;
    border-radius: 5px;
    font-family: monospace;
    font-size: 12px;
    z-index: 999999;
    display: none;
  `;
  
  document.body.appendChild(overlay);
  
  // 定期更新性能数据
  setInterval(async () => {
    const stats = await chrome.runtime.sendMessage({ type: 'getCacheStats' });
    if (stats) {
      overlay.innerHTML = `
        <div>CTWebPlayer Cache Stats</div>
        <div>Cached Items: ${stats.count}</div>
        <div>Cache Size: ${formatBytes(stats.totalSize)}</div>
        <div>Hit Rate: ${(stats.hitRate * 100).toFixed(1)}%</div>
      `;
    }
  }, 1000);
  
  return overlay;
}

/**
 * 格式化字节数
 * @param {number} bytes
 * @returns {string}
 */
function formatBytes(bytes) {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// 监听键盘快捷键
document.addEventListener('keydown', (event) => {
  // Ctrl+Shift+C 切换性能统计显示
  if (event.ctrlKey && event.shiftKey && event.key === 'C') {
    const overlay = document.getElementById('ctwebplayer-performance-overlay') || createPerformanceOverlay();
    overlay.style.display = overlay.style.display === 'none' ? 'block' : 'none';
  }
  
  // Ctrl+Shift+X 清除缓存
  if (event.ctrlKey && event.shiftKey && event.key === 'X') {
    if (confirm('Clear CTWebPlayer cache?')) {
      chrome.runtime.sendMessage({ type: 'clearCache' }, (response) => {
        if (response.success) {
          alert('Cache cleared successfully');
        }
      });
    }
  }
});

// 监听页面可见性变化
document.addEventListener('visibilitychange', () => {
  if (document.hidden) {
    console.log('Page hidden, pausing resource monitoring');
  } else {
    console.log('Page visible, resuming resource monitoring');
  }
});

// 监听网络状态变化
window.addEventListener('online', () => {
  console.log('Network online');
  chrome.runtime.sendMessage({ type: 'networkStatus', online: true });
});

window.addEventListener('offline', () => {
  console.log('Network offline');
  chrome.runtime.sendMessage({ type: 'networkStatus', online: false });
});

// 页面卸载时的清理工作
window.addEventListener('beforeunload', () => {
  console.log('Page unloading, cleaning up...');
  // 发送页面关闭消息
  chrome.runtime.sendMessage({ type: 'pageUnload' });
});