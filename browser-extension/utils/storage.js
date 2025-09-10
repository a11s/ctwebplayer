/**
 * CTWebPlayer Resource Cache - Storage Utilities
 * 封装 Chrome Storage API，提供统一的存储接口
 */

export class StorageManager {
  constructor() {
    this.storage = chrome.storage.local;
    this.syncStorage = chrome.storage.sync;
  }

  /**
   * 获取存储项
   * @param {string|string[]} keys - 要获取的键或键数组
   * @returns {Promise<Object>} 存储的值
   */
  async get(keys) {
    return new Promise((resolve, reject) => {
      this.storage.get(keys, (result) => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve(result);
        }
      });
    });
  }

  /**
   * 设置存储项
   * @param {Object} items - 要存储的键值对
   * @returns {Promise<void>}
   */
  async set(items) {
    return new Promise((resolve, reject) => {
      this.storage.set(items, () => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve();
        }
      });
    });
  }

  /**
   * 删除存储项
   * @param {string|string[]} keys - 要删除的键或键数组
   * @returns {Promise<void>}
   */
  async remove(keys) {
    return new Promise((resolve, reject) => {
      this.storage.remove(keys, () => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve();
        }
      });
    });
  }

  /**
   * 清空所有存储
   * @returns {Promise<void>}
   */
  async clear() {
    return new Promise((resolve, reject) => {
      this.storage.clear(() => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve();
        }
      });
    });
  }

  /**
   * 获取存储使用情况
   * @returns {Promise<Object>} 包含 bytesInUse 的对象
   */
  async getBytesInUse(keys = null) {
    return new Promise((resolve, reject) => {
      this.storage.getBytesInUse(keys, (bytesInUse) => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve(bytesInUse);
        }
      });
    });
  }

  /**
   * 获取同步存储项
   * @param {string|string[]} keys - 要获取的键或键数组
   * @returns {Promise<Object>} 存储的值
   */
  async getSync(keys) {
    return new Promise((resolve, reject) => {
      this.syncStorage.get(keys, (result) => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve(result);
        }
      });
    });
  }

  /**
   * 设置同步存储项
   * @param {Object} items - 要存储的键值对
   * @returns {Promise<void>}
   */
  async setSync(items) {
    return new Promise((resolve, reject) => {
      this.syncStorage.set(items, () => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve();
        }
      });
    });
  }

  /**
   * 监听存储变化
   * @param {Function} callback - 变化回调函数
   * @param {string} namespace - 命名空间 ('local' 或 'sync')
   */
  onChanged(callback, namespace = 'local') {
    chrome.storage.onChanged.addListener((changes, areaName) => {
      if (areaName === namespace) {
        callback(changes);
      }
    });
  }

  /**
   * 获取设置项
   * @returns {Promise<Object>} 设置对象
   */
  async getSettings() {
    const defaultSettings = {
      cacheEnabled: true,
      cacheRules: {
        images: true,
        scripts: true,
        styles: true,
        fonts: true,
        media: true
      },
      maxCacheSize: 500 * 1024 * 1024, // 500MB
      autoCleanup: true,
      cleanupInterval: 24 * 60 * 60 * 1000, // 24小时
      debugMode: false
    };

    const stored = await this.get('settings');
    return { ...defaultSettings, ...(stored.settings || {}) };
  }

  /**
   * 保存设置项
   * @param {Object} settings - 设置对象
   * @returns {Promise<void>}
   */
  async saveSettings(settings) {
    await this.set({ settings });
  }

  /**
   * 获取缓存白名单
   * @returns {Promise<string[]>} 白名单域名数组
   */
  async getWhitelist() {
    const result = await this.get('whitelist');
    return result.whitelist || [
      '*.cloudgame.com',
      '*.cdn-cloudgame.com'
    ];
  }

  /**
   * 保存缓存白名单
   * @param {string[]} whitelist - 白名单域名数组
   * @returns {Promise<void>}
   */
  async saveWhitelist(whitelist) {
    await this.set({ whitelist });
  }

  /**
   * 获取缓存黑名单
   * @returns {Promise<string[]>} 黑名单域名数组
   */
  async getBlacklist() {
    const result = await this.get('blacklist');
    return result.blacklist || [];
  }

  /**
   * 保存缓存黑名单
   * @param {string[]} blacklist - 黑名单域名数组
   * @returns {Promise<void>}
   */
  async saveBlacklist(blacklist) {
    await this.set({ blacklist });
  }

  /**
   * 记录缓存活动
   * @param {Object} activity - 活动信息
   * @returns {Promise<void>}
   */
  async logActivity(activity) {
    const activities = await this.getActivities();
    activities.push({
      ...activity,
      timestamp: Date.now()
    });

    // 只保留最近 1000 条记录
    if (activities.length > 1000) {
      activities.splice(0, activities.length - 1000);
    }

    await this.set({ activities });
  }

  /**
   * 获取缓存活动记录
   * @param {number} limit - 返回记录数限制
   * @returns {Promise<Array>} 活动记录数组
   */
  async getActivities(limit = 100) {
    const result = await this.get('activities');
    const activities = result.activities || [];
    return activities.slice(-limit);
  }

  /**
   * 清除活动记录
   * @returns {Promise<void>}
   */
  async clearActivities() {
    await this.remove('activities');
  }

  /**
   * 获取缓存元数据
   * @param {string} url - 资源 URL
   * @returns {Promise<Object|null>} 元数据对象
   */
  async getCacheMetadata(url) {
    const key = `meta_${this.hashUrl(url)}`;
    const result = await this.get(key);
    return result[key] || null;
  }

  /**
   * 保存缓存元数据
   * @param {string} url - 资源 URL
   * @param {Object} metadata - 元数据对象
   * @returns {Promise<void>}
   */
  async saveCacheMetadata(url, metadata) {
    const key = `meta_${this.hashUrl(url)}`;
    await this.set({ [key]: metadata });
  }

  /**
   * 删除缓存元数据
   * @param {string} url - 资源 URL
   * @returns {Promise<void>}
   */
  async removeCacheMetadata(url) {
    const key = `meta_${this.hashUrl(url)}`;
    await this.remove(key);
  }

  /**
   * 批量删除缓存元数据
   * @param {string[]} urls - 资源 URL 数组
   * @returns {Promise<void>}
   */
  async removeCacheMetadataBatch(urls) {
    const keys = urls.map(url => `meta_${this.hashUrl(url)}`);
    await this.remove(keys);
  }

  /**
   * 简单的 URL 哈希函数
   * @param {string} url - URL 字符串
   * @returns {string} 哈希值
   */
  hashUrl(url) {
    let hash = 0;
    for (let i = 0; i < url.length; i++) {
      const char = url.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32bit integer
    }
    return Math.abs(hash).toString(36);
  }

  /**
   * 导出所有存储数据
   * @returns {Promise<Object>} 所有存储的数据
   */
  async exportAll() {
    return new Promise((resolve, reject) => {
      this.storage.get(null, (items) => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve(items);
        }
      });
    });
  }

  /**
   * 导入存储数据
   * @param {Object} data - 要导入的数据
   * @returns {Promise<void>}
   */
  async importData(data) {
    await this.clear();
    await this.set(data);
  }
}

// 创建单例实例
export const storage = new StorageManager();