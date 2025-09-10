/**
 * CTWebPlayer Resource Cache - Cache Manager
 * 使用 chrome.storage.local 和 IndexedDB 混合管理资源缓存
 * - chrome.storage.local 用于存储元数据和小文件
 * - IndexedDB 用于存储大文件数据
 */

import { DEBUG_MODE } from '../config/constants.js';

export class CacheManager {
  constructor() {
    this.dbName = 'CTWebPlayerCache';
    this.version = 1;
    this.storeName = 'resources';
    this.db = null;
    this.stats = {
      hits: 0,
      misses: 0,
      totalSize: 0,
      savedBandwidth: 0
    };
    this.metadataPrefix = 'cache_meta_';
    this.indexKey = 'cache_index';
    this.statsKey = 'cache_stats';
    this.maxStorageSize = 500 * 1024 * 1024; // 500MB
  }

  /**
   * 初始化数据库和存储
   */
  async init() {
    if (DEBUG_MODE) {
      console.log('[CacheManager] Initializing...');
    }
    
    // 初始化 chrome.storage.local
    await this.initChromeStorage();
    
    // 初始化 IndexedDB
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, this.version);
      
      request.onerror = () => {
        console.error('Failed to open IndexedDB:', request.error);
        reject(request.error);
      };
      
      request.onsuccess = () => {
        this.db = request.result;
        console.log('[CacheManager] IndexedDB initialized');
        this.loadStats();
        resolve();
      };
      
      request.onupgradeneeded = (event) => {
        const db = event.target.result;
        
        // 创建资源存储
        if (!db.objectStoreNames.contains(this.storeName)) {
          const store = db.createObjectStore(this.storeName, { keyPath: 'url' });
          store.createIndex('timestamp', 'timestamp', { unique: false });
          store.createIndex('size', 'size', { unique: false });
          store.createIndex('type', 'type', { unique: false });
        }
        
        // 创建统计存储
        if (!db.objectStoreNames.contains('stats')) {
          db.createObjectStore('stats', { keyPath: 'key' });
        }
      };
    });
  }

  /**
   * 初始化 Chrome Storage
   */
  async initChromeStorage() {
    try {
      // 加载统计信息
      const result = await chrome.storage.local.get([this.statsKey, this.indexKey]);
      
      if (result[this.statsKey]) {
        this.stats = result[this.statsKey];
      }
      
      if (!result[this.indexKey]) {
        // 初始化索引
        await chrome.storage.local.set({ [this.indexKey]: [] });
      }
      
      if (DEBUG_MODE) {
        console.log('[CacheManager] Chrome Storage initialized');
        console.log('[CacheManager] Current stats:', this.stats);
      }
    } catch (error) {
      console.error('[CacheManager] Failed to initialize Chrome Storage:', error);
    }
  }

  /**
   * 存储资源
   * @param {string} url - 资源 URL
   * @param {ArrayBuffer} data - 资源数据
   * @param {Object} metadata - 元数据
   */
  async store(url, data, metadata = {}) {
    if (!this.db) {
      throw new Error('Database not initialized');
    }

    const size = data.byteLength || data.length;
    const timestamp = Date.now();
    const type = this.getResourceType(url, metadata.contentType);
    
    if (DEBUG_MODE) {
      console.log(`[CacheManager] Storing: ${url}, Size: ${(size / 1024).toFixed(2)} KB`);
    }

    try {
      // 检查存储空间
      const canStore = await this.checkStorageSpace(size);
      if (!canStore) {
        console.warn('[CacheManager] Storage space insufficient, cleaning up...');
        await this.cleanupOldestResources(size);
      }

      // 存储元数据到 chrome.storage.local
      const metaKey = this.metadataPrefix + this.hashUrl(url);
      const metaData = {
        url,
        metadata,
        timestamp,
        size,
        type,
        hits: 0
      };
      
      // 对于小文件（< 1MB），直接存储在 chrome.storage.local
      if (size < 1024 * 1024) {
        metaData.data = Array.from(new Uint8Array(data));
        await chrome.storage.local.set({ [metaKey]: metaData });
        
        if (DEBUG_MODE) {
          console.log(`[CacheManager] Small file stored in chrome.storage.local: ${url}`);
        }
      } else {
        // 大文件存储在 IndexedDB
        const transaction = this.db.transaction([this.storeName], 'readwrite');
        const store = transaction.objectStore(this.storeName);
        
        const record = {
          url,
          data,
          metadata,
          timestamp,
          size,
          type,
          hits: 0
        };
        
        await new Promise((resolve, reject) => {
          const request = store.put(record);
          
          request.onsuccess = () => {
            if (DEBUG_MODE) {
              console.log(`[CacheManager] Large file stored in IndexedDB: ${url}`);
            }
            resolve();
          };
          
          request.onerror = () => {
            console.error('[CacheManager] Failed to store in IndexedDB:', request.error);
            reject(request.error);
          };
        });
        
        // 在 chrome.storage.local 中只存储元数据
        await chrome.storage.local.set({ [metaKey]: metaData });
      }
      
      // 更新索引
      await this.updateIndex(url);
      
      // 更新统计
      this.updateStats('totalSize', this.stats.totalSize + size);
      
      return metaData;
    } catch (error) {
      console.error('[CacheManager] Failed to store resource:', error);
      throw error;
    }
  }

  /**
   * 获取资源
   * @param {string} url - 资源 URL
   * @returns {Promise<Object|null>} 缓存的资源对象
   */
  async get(url) {
    if (!this.db) {
      throw new Error('Database not initialized');
    }

    if (DEBUG_MODE) {
      console.log(`[CacheManager] Getting: ${url}`);
    }

    try {
      // 先从 chrome.storage.local 获取元数据
      const metaKey = this.metadataPrefix + this.hashUrl(url);
      const result = await chrome.storage.local.get(metaKey);
      
      if (!result[metaKey]) {
        if (DEBUG_MODE) {
          console.log(`[CacheManager] Cache miss: ${url}`);
        }
        this.stats.misses++;
        this.saveStats();
        return null;
      }
      
      const metaData = result[metaKey];
      
      // 检查是否过期
      if (this.isExpired(metaData)) {
        if (DEBUG_MODE) {
          console.log(`[CacheManager] Cache expired: ${url}`);
        }
        await this.delete(url);
        this.stats.misses++;
        this.saveStats();
        return null;
      }
      
      // 如果数据存储在 chrome.storage.local（小文件）
      if (metaData.data) {
        const data = new Uint8Array(metaData.data).buffer;
        
        // 更新命中统计
        metaData.hits++;
        await chrome.storage.local.set({ [metaKey]: metaData });
        
        this.stats.hits++;
        this.stats.savedBandwidth += metaData.size;
        this.saveStats();
        
        if (DEBUG_MODE) {
          console.log(`[CacheManager] Cache hit (chrome.storage): ${url}`);
        }
        
        return {
          data,
          metadata: metaData.metadata,
          size: metaData.size
        };
      }
      
      // 大文件从 IndexedDB 获取
      const transaction = this.db.transaction([this.storeName], 'readonly');
      const store = transaction.objectStore(this.storeName);
      
      return new Promise((resolve, reject) => {
        const request = store.get(url);
        
        request.onsuccess = async () => {
          const result = request.result;
          
          if (result) {
            // 更新命中统计
            metaData.hits++;
            await chrome.storage.local.set({ [metaKey]: metaData });
            
            this.stats.hits++;
            this.stats.savedBandwidth += result.size;
            this.saveStats();
            
            if (DEBUG_MODE) {
              console.log(`[CacheManager] Cache hit (IndexedDB): ${url}`);
            }
            
            resolve(result);
          } else {
            // IndexedDB 中没有找到，可能是数据不一致
            console.warn(`[CacheManager] Metadata exists but data missing: ${url}`);
            await this.delete(url);
            this.stats.misses++;
            this.saveStats();
            resolve(null);
          }
        };
        
        request.onerror = () => {
          console.error('[CacheManager] Failed to get from IndexedDB:', request.error);
          reject(request.error);
        };
      });
    } catch (error) {
      console.error('[CacheManager] Failed to get resource:', error);
      this.stats.misses++;
      this.saveStats();
      return null;
    }
  }

  /**
   * 删除资源
   * @param {string} url - 资源 URL
   */
  async delete(url) {
    if (!this.db) {
      throw new Error('Database not initialized');
    }

    if (DEBUG_MODE) {
      console.log(`[CacheManager] Deleting: ${url}`);
    }

    try {
      // 获取元数据
      const metaKey = this.metadataPrefix + this.hashUrl(url);
      const result = await chrome.storage.local.get(metaKey);
      
      if (result[metaKey]) {
        const metaData = result[metaKey];
        
        // 从 chrome.storage.local 删除
        await chrome.storage.local.remove(metaKey);
        
        // 如果是大文件，从 IndexedDB 删除
        if (!metaData.data) {
          const transaction = this.db.transaction([this.storeName], 'readwrite');
          const store = transaction.objectStore(this.storeName);
          
          await new Promise((resolve, reject) => {
            const deleteRequest = store.delete(url);
            
            deleteRequest.onsuccess = () => resolve();
            deleteRequest.onerror = () => reject(deleteRequest.error);
          });
        }
        
        // 更新索引
        await this.removeFromIndex(url);
        
        // 更新统计
        this.updateStats('totalSize', this.stats.totalSize - metaData.size);
        
        if (DEBUG_MODE) {
          console.log(`[CacheManager] Deleted: ${url}`);
        }
      }
    } catch (error) {
      console.error('[CacheManager] Failed to delete resource:', error);
      throw error;
    }
  }

  /**
   * 清空所有缓存
   */
  async clear() {
    if (!this.db) {
      throw new Error('Database not initialized');
    }

    console.log('[CacheManager] Clearing all cache...');

    try {
      // 清空 IndexedDB
      const transaction = this.db.transaction([this.storeName], 'readwrite');
      const store = transaction.objectStore(this.storeName);
      
      await new Promise((resolve, reject) => {
        const request = store.clear();
        
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
      });
      
      // 获取所有缓存键并删除
      const result = await chrome.storage.local.get(null);
      const keysToRemove = Object.keys(result).filter(key => 
        key.startsWith(this.metadataPrefix) || 
        key === this.indexKey
      );
      
      if (keysToRemove.length > 0) {
        await chrome.storage.local.remove(keysToRemove);
      }
      
      // 重置统计
      this.stats = {
        hits: 0,
        misses: 0,
        totalSize: 0,
        savedBandwidth: 0
      };
      this.saveStats();
      
      // 重新初始化索引
      await chrome.storage.local.set({ [this.indexKey]: [] });
      
      console.log('[CacheManager] Cache cleared');
    } catch (error) {
      console.error('[CacheManager] Failed to clear cache:', error);
      throw error;
    }
  }

  /**
   * 清理过期缓存
   */
  async cleanup() {
    if (!this.db) {
      throw new Error('Database not initialized');
    }

    console.log('[CacheManager] Running cleanup...');
    
    try {
      const result = await chrome.storage.local.get(null);
      const expiredUrls = [];
      
      // 检查所有缓存项
      for (const [key, value] of Object.entries(result)) {
        if (key.startsWith(this.metadataPrefix) && this.isExpired(value)) {
          expiredUrls.push(value.url);
        }
      }
      
      // 删除过期资源
      if (expiredUrls.length > 0) {
        await Promise.all(expiredUrls.map(url => this.delete(url)));
        console.log(`[CacheManager] Cleaned up ${expiredUrls.length} expired resources`);
      }
      
      return expiredUrls.length;
    } catch (error) {
      console.error('[CacheManager] Cleanup failed:', error);
      return 0;
    }
  }

  /**
   * 获取缓存统计信息
   */
  async getStats() {
    if (!this.db) {
      return this.stats;
    }

    try {
      const result = await chrome.storage.local.get(this.indexKey);
      const index = result[this.indexKey] || [];
      
      const hitRate = this.stats.hits + this.stats.misses > 0
        ? this.stats.hits / (this.stats.hits + this.stats.misses)
        : 0;
      
      return {
        count,
        totalSize: this.stats.totalSize,
        hitRate,
        hits: this.stats.hits,
        misses: this.stats.misses,
        savedBandwidth: this.stats.savedBandwidth
      };
    } catch (error) {
      console.error('[CacheManager] Failed to get stats:', error);
      return this.stats;
    }
  }

  /**
   * 获取详细统计信息
   */
  async getDetailedStats() {
    if (!this.db) {
      return null;
    }

    try {
      const result = await chrome.storage.local.get(null);
      const stats = {
        byType: {},
        topResources: [],
        oldestResources: []
      };
      
      const resources = [];
      
      // 收集所有资源信息
      for (const [key, value] of Object.entries(result)) {
        if (key.startsWith(this.metadataPrefix)) {
          resources.push(value);
          
          // 按类型统计
          if (!stats.byType[value.type]) {
            stats.byType[value.type] = {
              count: 0,
              size: 0
            };
          }
          stats.byType[value.type].count++;
          stats.byType[value.type].size += value.size;
        }
      }
      
      // 排序获取 top 资源
      resources.sort((a, b) => b.hits - a.hits);
      stats.topResources = resources.slice(0, 10).map(r => ({
        url: r.url,
        hits: r.hits,
        size: r.size,
        type: r.type
      }));
      
      // 获取最老的资源
      resources.sort((a, b) => a.timestamp - b.timestamp);
      stats.oldestResources = resources.slice(0, 10).map(r => ({
        url: r.url,
        age: Date.now() - r.timestamp,
        size: r.size,
        type: r.type
      }));
      
      return stats;
    } catch (error) {
      console.error('[CacheManager] Failed to get detailed stats:', error);
      return null;
    }
  }

  /**
   * 判断资源是否过期
   * @param {Object} resource - 资源对象
   * @returns {boolean}
   */
  isExpired(resource) {
    const ttlConfig = {
      'image': 7 * 24 * 60 * 60 * 1000,  // 7天
      'script': 24 * 60 * 60 * 1000,     // 1天
      'style': 24 * 60 * 60 * 1000,      // 1天
      'font': 30 * 24 * 60 * 60 * 1000,  // 30天
      'media': 3 * 60 * 60 * 1000,       // 3小时
      'other': 12 * 60 * 60 * 1000       // 12小时
    };
    
    const ttl = ttlConfig[resource.type] || ttlConfig.other;
    return Date.now() - resource.timestamp > ttl;
  }

  /**
   * 获取资源类型
   * @param {string} url - 资源 URL
   * @param {string} contentType - Content-Type
   * @returns {string}
   */
  getResourceType(url, contentType) {
    // 根据 Content-Type 判断
    if (contentType) {
      if (contentType.includes('image')) return 'image';
      if (contentType.includes('javascript')) return 'script';
      if (contentType.includes('css')) return 'style';
      if (contentType.includes('font')) return 'font';
      if (contentType.includes('video') || contentType.includes('audio')) return 'media';
      if (contentType.includes('wasm')) return 'other'; // WebAssembly
    }
    
    // 根据文件扩展名判断
    const ext = url.split('.').pop().toLowerCase().split('?')[0]; // 移除查询参数
    const extMap = {
      'jpg': 'image', 'jpeg': 'image', 'png': 'image', 'gif': 'image', 'webp': 'image', 'svg': 'image',
      'js': 'script', 'mjs': 'script',
      'css': 'style',
      'woff': 'font', 'woff2': 'font', 'ttf': 'font', 'eot': 'font',
      'mp4': 'media', 'webm': 'media', 'ogg': 'media', 'mp3': 'media', 'wav': 'media',
      'wasm': 'other', 'data': 'other' // Unity WebGL 文件
    };
    
    return extMap[ext] || 'other';
  }

  /**
   * 加载统计信息
   */
  async loadStats() {
    try {
      const result = await chrome.storage.local.get(this.statsKey);
      if (result[this.statsKey]) {
        this.stats = result[this.statsKey];
      }
    } catch (error) {
      console.error('[CacheManager] Failed to load stats:', error);
    }
  }

  /**
   * 保存统计信息
   */
  async saveStats() {
    try {
      await chrome.storage.local.set({ [this.statsKey]: this.stats });
    } catch (error) {
      console.error('[CacheManager] Failed to save stats:', error);
    }
  }

  /**
   * 更新统计信息
   * @param {string} key - 统计项
   * @param {number} value - 新值
   */
  updateStats(key, value) {
    this.stats[key] = value;
    this.saveStats();
  }

  /**
   * 生成 URL 的哈希值
   * @param {string} url - URL
   * @returns {string} 哈希值
   */
  hashUrl(url) {
    // 简单的哈希函数
    let hash = 0;
    for (let i = 0; i < url.length; i++) {
      const char = url.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32bit integer
    }
    return Math.abs(hash).toString(36);
  }

  /**
   * 更新索引
   * @param {string} url - URL
   */
  async updateIndex(url) {
    try {
      const result = await chrome.storage.local.get(this.indexKey);
      const index = result[this.indexKey] || [];
      
      if (!index.includes(url)) {
        index.push(url);
        await chrome.storage.local.set({ [this.indexKey]: index });
      }
    } catch (error) {
      console.error('[CacheManager] Failed to update index:', error);
    }
  }

  /**
   * 从索引中移除
   * @param {string} url - URL
   */
  async removeFromIndex(url) {
    try {
      const result = await chrome.storage.local.get(this.indexKey);
      const index = result[this.indexKey] || [];
      
      const newIndex = index.filter(u => u !== url);
      await chrome.storage.local.set({ [this.indexKey]: newIndex });
    } catch (error) {
      console.error('[CacheManager] Failed to remove from index:', error);
    }
  }

  /**
   * 检查存储空间
   * @param {number} size - 需要的空间大小
   * @returns {boolean} 是否有足够空间
   */
  async checkStorageSpace(size) {
    try {
      const quota = await navigator.storage.estimate();
      const available = quota.quota - quota.usage;
      
      if (DEBUG_MODE) {
        console.log(`[CacheManager] Storage: ${(quota.usage / 1024 / 1024).toFixed(2)} MB used of ${(quota.quota / 1024 / 1024).toFixed(2)} MB`);
        console.log(`[CacheManager] Available: ${(available / 1024 / 1024).toFixed(2)} MB`);
      }
      
      // 保留 10MB 的缓冲空间
      return available > size + 10 * 1024 * 1024;
    } catch (error) {
      console.error('[CacheManager] Failed to check storage space:', error);
      // 如果无法获取存储信息，检查当前缓存大小
      return this.stats.totalSize + size < this.maxStorageSize;
    }
  }

  /**
   * 清理最老的资源以腾出空间
   * @param {number} requiredSize - 需要的空间大小
   */
  async cleanupOldestResources(requiredSize) {
    try {
      const result = await chrome.storage.local.get(null);
      const resources = [];
      
      // 收集所有资源
      for (const [key, value] of Object.entries(result)) {
        if (key.startsWith(this.metadataPrefix)) {
          resources.push(value);
        }
      }
      
      // 按时间戳排序（最老的在前）
      resources.sort((a, b) => a.timestamp - b.timestamp);
      
      let freedSpace = 0;
      const urlsToDelete = [];
      
      // 删除最老的资源直到有足够空间
      for (const resource of resources) {
        if (freedSpace >= requiredSize) break;
        
        urlsToDelete.push(resource.url);
        freedSpace += resource.size;
      }
      
      if (urlsToDelete.length > 0) {
        await Promise.all(urlsToDelete.map(url => this.delete(url)));
        console.log(`[CacheManager] Cleaned up ${urlsToDelete.length} resources to free ${(freedSpace / 1024 / 1024).toFixed(2)} MB`);
      }
    } catch (error) {
      console.error('[CacheManager] Failed to cleanup oldest resources:', error);
    }
  }
}
      const count = index.length;