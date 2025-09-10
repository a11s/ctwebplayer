/**
 * CTWebPlayer Resource Cache - Popup Script
 * 弹出页面的交互逻辑
 */

// DOM 元素引用
const elements = {
  cacheCount: document.getElementById('cache-count'),
  cacheSize: document.getElementById('cache-size'),
  hitRate: document.getElementById('hit-rate'),
  savedBandwidth: document.getElementById('saved-bandwidth'),
  nativeStatus: document.getElementById('native-status'),
  cacheStatus: document.getElementById('cache-status'),
  clearCacheBtn: document.getElementById('clear-cache'),
  exportCacheBtn: document.getElementById('export-cache'),
  openOptionsLink: document.getElementById('open-options'),
  openHelpLink: document.getElementById('open-help'),
  // 缓存规则复选框
  cacheImages: document.getElementById('cache-images'),
  cacheScripts: document.getElementById('cache-scripts'),
  cacheFonts: document.getElementById('cache-fonts'),
  cacheMedia: document.getElementById('cache-media')
};

// 初始化
document.addEventListener('DOMContentLoaded', () => {
  loadCacheStats();
  loadCacheRules();
  checkNativeConnection();
  setupEventListeners();
  
  // 定期更新统计信息
  setInterval(loadCacheStats, 1000);
});

/**
 * 加载缓存统计信息
 */
async function loadCacheStats() {
  try {
    const response = await chrome.runtime.sendMessage({ type: 'getCacheStats' });
    if (response) {
      updateStatsDisplay(response);
    }
  } catch (error) {
    console.error('Failed to load cache stats:', error);
  }
}

/**
 * 更新统计显示
 * @param {Object} stats - 统计数据
 */
function updateStatsDisplay(stats) {
  elements.cacheCount.textContent = stats.count || 0;
  elements.cacheSize.textContent = formatBytes(stats.totalSize || 0);
  elements.hitRate.textContent = `${((stats.hitRate || 0) * 100).toFixed(1)}%`;
  elements.savedBandwidth.textContent = formatBytes(stats.savedBandwidth || 0);
}

/**
 * 加载缓存规则设置
 */
async function loadCacheRules() {
  try {
    const rules = await chrome.storage.local.get('cacheRules');
    if (rules.cacheRules) {
      elements.cacheImages.checked = rules.cacheRules.images !== false;
      elements.cacheScripts.checked = rules.cacheRules.scripts !== false;
      elements.cacheFonts.checked = rules.cacheRules.fonts !== false;
      elements.cacheMedia.checked = rules.cacheRules.media !== false;
    }
  } catch (error) {
    console.error('Failed to load cache rules:', error);
  }
}

/**
 * 检查 Native Messaging 连接状态
 */
async function checkNativeConnection() {
  try {
    const response = await chrome.runtime.sendMessage({ type: 'checkNativeConnection' });
    updateConnectionStatus(response && response.connected);
  } catch (error) {
    console.error('Failed to check native connection:', error);
    updateConnectionStatus(false);
  }
}

/**
 * 更新连接状态显示
 * @param {boolean} connected - 是否已连接
 */
function updateConnectionStatus(connected) {
  const statusIndicator = elements.nativeStatus.querySelector('.status-indicator');
  const statusText = elements.nativeStatus.querySelector('span:last-child');
  
  if (connected) {
    statusIndicator.setAttribute('data-status', 'connected');
    statusText.textContent = '已连接';
  } else {
    statusIndicator.setAttribute('data-status', 'disconnected');
    statusText.textContent = '未连接';
  }
}

/**
 * 设置事件监听器
 */
function setupEventListeners() {
  // 清除缓存按钮
  elements.clearCacheBtn.addEventListener('click', async () => {
    if (confirm('确定要清除所有缓存吗？')) {
      elements.clearCacheBtn.disabled = true;
      elements.clearCacheBtn.textContent = '清除中...';
      
      try {
        const response = await chrome.runtime.sendMessage({ type: 'clearCache' });
        if (response && response.success) {
          showNotification('缓存已清除', 'success');
          loadCacheStats(); // 重新加载统计
        }
      } catch (error) {
        console.error('Failed to clear cache:', error);
        showNotification('清除缓存失败', 'error');
      } finally {
        elements.clearCacheBtn.disabled = false;
        elements.clearCacheBtn.innerHTML = `
          <svg class="icon" viewBox="0 0 24 24">
            <path d="M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"/>
          </svg>
          清除缓存
        `;
      }
    }
  });
  
  // 导出统计按钮
  elements.exportCacheBtn.addEventListener('click', async () => {
    try {
      const stats = await chrome.runtime.sendMessage({ type: 'getCacheStats' });
      const detailedStats = await chrome.runtime.sendMessage({ type: 'getDetailedStats' });
      
      const exportData = {
        timestamp: new Date().toISOString(),
        summary: stats,
        details: detailedStats,
        version: chrome.runtime.getManifest().version
      };
      
      // 创建下载
      const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const filename = `ctwebplayer-cache-stats-${Date.now()}.json`;
      
      chrome.downloads.download({
        url: url,
        filename: filename,
        saveAs: true
      });
      
      showNotification('统计数据已导出', 'success');
    } catch (error) {
      console.error('Failed to export stats:', error);
      showNotification('导出失败', 'error');
    }
  });
  
  // 缓存规则复选框
  const ruleCheckboxes = [
    { element: elements.cacheImages, key: 'images' },
    { element: elements.cacheScripts, key: 'scripts' },
    { element: elements.cacheFonts, key: 'fonts' },
    { element: elements.cacheMedia, key: 'media' }
  ];
  
  ruleCheckboxes.forEach(({ element, key }) => {
    element.addEventListener('change', async () => {
      const rules = await chrome.storage.local.get('cacheRules') || {};
      if (!rules.cacheRules) rules.cacheRules = {};
      rules.cacheRules[key] = element.checked;
      
      await chrome.storage.local.set({ cacheRules: rules.cacheRules });
      chrome.runtime.sendMessage({ type: 'updateCacheRules', rules: rules.cacheRules });
    });
  });
  
  // 设置链接
  elements.openOptionsLink.addEventListener('click', (e) => {
    e.preventDefault();
    chrome.runtime.openOptionsPage();
  });
  
  // 帮助链接
  elements.openHelpLink.addEventListener('click', (e) => {
    e.preventDefault();
    chrome.tabs.create({ url: 'https://github.com/ctwebplayer/extension/wiki' });
  });
}

/**
 * 格式化字节数
 * @param {number} bytes - 字节数
 * @returns {string} 格式化后的字符串
 */
function formatBytes(bytes) {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * 显示通知
 * @param {string} message - 消息内容
 * @param {string} type - 消息类型 (success, error, info)
 */
function showNotification(message, type = 'info') {
  // 创建通知元素
  const notification = document.createElement('div');
  notification.className = `notification notification-${type}`;
  notification.textContent = message;
  
  // 添加到页面
  document.body.appendChild(notification);
  
  // 显示动画
  setTimeout(() => {
    notification.classList.add('show');
  }, 10);
  
  // 3秒后移除
  setTimeout(() => {
    notification.classList.remove('show');
    setTimeout(() => {
      notification.remove();
    }, 300);
  }, 3000);
}

// 监听来自 background script 的消息
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.type === 'statsUpdated') {
    updateStatsDisplay(request.stats);
  }
});