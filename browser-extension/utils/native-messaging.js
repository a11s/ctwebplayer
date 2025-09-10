/**
 * CTWebPlayer Resource Cache - Native Messaging
 * 处理与 CTWebPlayer 的原生消息通信
 */

export class NativeMessaging {
  constructor(hostName) {
    this.hostName = hostName;
    this.port = null;
    this.connected = false;
    this.messageHandlers = new Map();
    this.connectionHandlers = {
      onConnect: [],
      onDisconnect: [],
      onError: []
    };
  }

  /**
   * 连接到 Native Host
   * @returns {Promise<boolean>} 是否连接成功
   */
  async connect() {
    return new Promise((resolve) => {
      try {
        // 尝试连接到 Native Host
        this.port = chrome.runtime.connectNative(this.hostName);
        
        // 监听消息
        this.port.onMessage.addListener((message) => {
          this.handleMessage(message);
        });
        
        // 监听断开连接
        this.port.onDisconnect.addListener(() => {
          this.handleDisconnect();
        });
        
        // 发送初始化消息测试连接
        this.port.postMessage({ type: 'ping' });
        
        // 等待响应
        setTimeout(() => {
          if (this.connected) {
            resolve(true);
          } else {
            this.disconnect();
            resolve(false);
          }
        }, 1000);
        
      } catch (error) {
        console.error('Failed to connect to native host:', error);
        this.handleError(error);
        resolve(false);
      }
    });
  }

  /**
   * 断开连接
   */
  disconnect() {
    if (this.port) {
      this.port.disconnect();
      this.port = null;
    }
    this.connected = false;
  }

  /**
   * 发送消息
   * @param {Object} message - 要发送的消息
   * @returns {Promise<void>}
   */
  async send(message) {
    if (!this.port || !this.connected) {
      throw new Error('Not connected to native host');
    }
    
    return new Promise((resolve, reject) => {
      try {
        this.port.postMessage(message);
        resolve();
      } catch (error) {
        console.error('Failed to send message:', error);
        reject(error);
      }
    });
  }

  /**
   * 发送消息并等待响应
   * @param {Object} message - 要发送的消息
   * @param {number} timeout - 超时时间（毫秒）
   * @returns {Promise<Object>} 响应消息
   */
  async sendAndWait(message, timeout = 5000) {
    const messageId = this.generateMessageId();
    const messageWithId = { ...message, id: messageId };
    
    return new Promise((resolve, reject) => {
      // 设置超时
      const timeoutId = setTimeout(() => {
        this.messageHandlers.delete(messageId);
        reject(new Error('Message timeout'));
      }, timeout);
      
      // 注册响应处理器
      this.messageHandlers.set(messageId, (response) => {
        clearTimeout(timeoutId);
        this.messageHandlers.delete(messageId);
        resolve(response);
      });
      
      // 发送消息
      this.send(messageWithId).catch(reject);
    });
  }

  /**
   * 处理接收到的消息
   * @param {Object} message - 接收到的消息
   */
  handleMessage(message) {
    console.log('Received message from native host:', message);
    
    // 处理 ping 响应
    if (message.type === 'pong') {
      this.connected = true;
      this.triggerConnectionHandlers('onConnect');
      return;
    }
    
    // 处理带 ID 的响应
    if (message.id && this.messageHandlers.has(message.id)) {
      const handler = this.messageHandlers.get(message.id);
      handler(message);
      return;
    }
    
    // 处理特定类型的消息
    switch (message.type) {
      case 'navigate':
        this.handleNavigateMessage(message);
        break;
        
      case 'login_success':
        this.handleLoginSuccess(message);
        break;
        
      case 'error':
        this.handleError(new Error(message.error || 'Unknown error'));
        break;
        
      default:
        console.log('Unhandled message type:', message.type);
    }
  }

  /**
   * 处理导航消息
   * @param {Object} message - 导航消息
   */
  handleNavigateMessage(message) {
    if (message.url) {
      // 创建新标签页
      chrome.tabs.create({
        url: message.url,
        active: true
      }, (tab) => {
        // 如果有 cookies，设置它们
        if (message.cookies) {
          this.setCookies(message.url, message.cookies);
        }
        
        // 如果有认证令牌，存储它
        if (message.authToken) {
          chrome.storage.local.set({
            authToken: message.authToken,
            authUrl: message.url
          });
        }
      });
    }
  }

  /**
   * 处理登录成功消息
   * @param {Object} message - 登录成功消息
   */
  handleLoginSuccess(message) {
    console.log('Login successful, processing iframe URL...');
    
    // 存储登录信息
    chrome.storage.local.set({
      loginTime: Date.now(),
      userInfo: message.userInfo || {}
    });
    
    // 如果有 iframe URL，导航到它
    if (message.iframeUrl) {
      this.handleNavigateMessage({
        url: message.iframeUrl,
        cookies: message.cookies,
        authToken: message.authToken
      });
    }
  }

  /**
   * 处理断开连接
   */
  handleDisconnect() {
    console.log('Disconnected from native host');
    
    if (chrome.runtime.lastError) {
      console.error('Native host error:', chrome.runtime.lastError);
    }
    
    this.connected = false;
    this.port = null;
    this.triggerConnectionHandlers('onDisconnect');
  }

  /**
   * 处理错误
   * @param {Error} error - 错误对象
   */
  handleError(error) {
    console.error('Native messaging error:', error);
    this.triggerConnectionHandlers('onError', error);
  }

  /**
   * 设置 Cookies
   * @param {string} url - URL
   * @param {Array} cookies - Cookie 数组
   */
  async setCookies(url, cookies) {
    const urlObj = new URL(url);
    
    for (const cookie of cookies) {
      try {
        await chrome.cookies.set({
          url: url,
          name: cookie.name,
          value: cookie.value,
          domain: cookie.domain || urlObj.hostname,
          path: cookie.path || '/',
          secure: cookie.secure || urlObj.protocol === 'https:',
          httpOnly: cookie.httpOnly || false,
          sameSite: cookie.sameSite || 'lax',
          expirationDate: cookie.expirationDate || undefined
        });
      } catch (error) {
        console.error('Failed to set cookie:', cookie.name, error);
      }
    }
  }

  /**
   * 注册消息处理器
   * @param {string} type - 消息类型
   * @param {Function} handler - 处理函数
   */
  onMessage(type, handler) {
    if (!this.messageHandlers.has(type)) {
      this.messageHandlers.set(type, []);
    }
    this.messageHandlers.get(type).push(handler);
  }

  /**
   * 注册连接事件处理器
   * @param {string} event - 事件类型 (onConnect, onDisconnect, onError)
   * @param {Function} handler - 处理函数
   */
  on(event, handler) {
    if (this.connectionHandlers[event]) {
      this.connectionHandlers[event].push(handler);
    }
  }

  /**
   * 触发连接事件处理器
   * @param {string} event - 事件类型
   * @param {...any} args - 事件参数
   */
  triggerConnectionHandlers(event, ...args) {
    if (this.connectionHandlers[event]) {
      this.connectionHandlers[event].forEach(handler => {
        try {
          handler(...args);
        } catch (error) {
          console.error(`Error in ${event} handler:`, error);
        }
      });
    }
  }

  /**
   * 生成消息 ID
   * @returns {string} 唯一的消息 ID
   */
  generateMessageId() {
    return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * 检查连接状态
   * @returns {boolean} 是否已连接
   */
  isConnected() {
    return this.connected && this.port !== null;
  }

  /**
   * 获取 Native Host 信息
   * @returns {Promise<Object>} Host 信息
   */
  async getHostInfo() {
    if (!this.isConnected()) {
      throw new Error('Not connected to native host');
    }
    
    try {
      const response = await this.sendAndWait({ type: 'get_info' });
      return response.info || {};
    } catch (error) {
      console.error('Failed to get host info:', error);
      return {};
    }
  }

  /**
   * 请求 CTWebPlayer 执行操作
   * @param {string} action - 操作类型
   * @param {Object} params - 操作参数
   * @returns {Promise<Object>} 操作结果
   */
  async requestAction(action, params = {}) {
    if (!this.isConnected()) {
      throw new Error('Not connected to native host');
    }
    
    return this.sendAndWait({
      type: 'action',
      action,
      params
    });
  }
}

// 创建单例实例
let nativeMessagingInstance = null;

/**
 * 获取 Native Messaging 实例
 * @param {string} hostName - Native Host 名称
 * @returns {NativeMessaging} Native Messaging 实例
 */
export function getNativeMessaging(hostName = 'com.ctwebplayer.bridge') {
  if (!nativeMessagingInstance) {
    nativeMessagingInstance = new NativeMessaging(hostName);
  }
  return nativeMessagingInstance;
}