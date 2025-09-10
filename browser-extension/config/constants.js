/**
 * CTWebPlayer Resource Cache - Constants Configuration
 * 插件的常量配置文件
 */

// Native Messaging Host 名称
export const NATIVE_HOST_NAME = 'com.ctwebplayer.bridge';

// 调试模式标志
export const DEBUG_MODE = true;

// 缓存配置
export const CACHE_CONFIG = {
  // 资源类型缓存时长（毫秒）
  ttl: {
    'image': 7 * 24 * 60 * 60 * 1000,  // 7天
    'script': 24 * 60 * 60 * 1000,     // 1天
    'style': 24 * 60 * 60 * 1000,      // 1天
    'font': 30 * 24 * 60 * 60 * 1000,  // 30天
    'media': 3 * 60 * 60 * 1000,       // 3小时
    'other': 12 * 60 * 60 * 1000       // 12小时
  },
  
  // 最大缓存大小（字节）
  maxSize: 500 * 1024 * 1024, // 500MB
  
  // 缓存清理策略
  evictionPolicy: 'LRU', // Least Recently Used
  
  // 需要缓存的资源模式（正则表达式）
  patterns: [
    /\.(jpg|jpeg|png|gif|webp|svg)$/i,     // 图片
    /\.(js|mjs)$/i,                         // JavaScript
    /\.(css)$/i,                            // 样式表
    /\.(woff|woff2|ttf|eot)$/i,            // 字体
    /\.(mp4|webm|ogg|mp3|wav)$/i,          // 媒体文件
    /\.(data|wasm)$/i,                      // Unity WebGL 数据文件
  ],
  
  // 需要缓存的路径模式（正则表达式）
  pathPatterns: [
    /\/patch\/files\//i,                    // CDN 上的游戏资源文件路径
  ],
  
  // 需要缓存的域名模式
  domains: [
    /^https?:\/\/[^\/]*\.cloudgame\.com/i,
    /^https?:\/\/[^\/]*\.cdn-cloudgame\.com/i,
    'patch-ct-labs.ecchi.xxx',
    'hefc.hrbzqy.com'
  ],
  
  // 排除的资源模式
  excludePatterns: [
    /\?nocache=/i,                          // 带有 nocache 参数
    /\.(html|htm)$/i,                       // HTML 文件
    /\/api\//i,                             // API 请求
    /\/ws\//i,                              // WebSocket
  ]
};

// 性能配置
export const PERFORMANCE_CONFIG = {
  // 批量处理大小
  batchSize: 10,
  
  // 清理间隔（毫秒）
  cleanupInterval: 60 * 60 * 1000, // 1小时
  
  // 统计更新间隔（毫秒）
  statsUpdateInterval: 5000, // 5秒
  
  // 最大并发请求数
  maxConcurrentRequests: 5,
  
  // 请求超时时间（毫秒）
  requestTimeout: 30000, // 30秒
};

// UI 配置
export const UI_CONFIG = {
  // 弹出窗口尺寸
  popupWidth: 400,
  popupHeight: 600,
  
  // 通知显示时长（毫秒）
  notificationDuration: 3000,
  
  // 动画持续时间（毫秒）
  animationDuration: 300,
  
  // 主题颜色
  theme: {
    primary: '#667eea',
    secondary: '#764ba2',
    success: '#4caf50',
    warning: '#ff9800',
    error: '#f44336',
    info: '#2196f3'
  }
};

// 日志配置
export const LOG_CONFIG = {
  // 日志级别
  level: 'info', // 'debug', 'info', 'warn', 'error'
  
  // 是否在控制台输出
  console: true,
  
  // 是否保存到存储
  storage: true,
  
  // 最大日志条数
  maxLogs: 1000,
  
  // 日志保留时间（毫秒）
  retention: 7 * 24 * 60 * 60 * 1000 // 7天
};

// API 端点
export const API_ENDPOINTS = {
  // 统计上报地址
  analytics: 'https://analytics.ctwebplayer.com/v1/report',
  
  // 配置更新地址
  config: 'https://config.ctwebplayer.com/v1/extension',
  
  // 反馈地址
  feedback: 'https://feedback.ctwebplayer.com/v1/submit'
};

// 存储键名
export const STORAGE_KEYS = {
  settings: 'settings',
  stats: 'stats',
  whitelist: 'whitelist',
  blacklist: 'blacklist',
  activities: 'activities',
  authToken: 'authToken',
  userInfo: 'userInfo',
  lastCleanup: 'lastCleanup',
  cacheRules: 'cacheRules'
};

// 消息类型
export const MESSAGE_TYPES = {
  // 与 background script 通信
  GET_CACHE_STATS: 'getCacheStats',
  CLEAR_CACHE: 'clearCache',
  UPDATE_CACHE_RULES: 'updateCacheRules',
  GET_DETAILED_STATS: 'getDetailedStats',
  CHECK_NATIVE_CONNECTION: 'checkNativeConnection',
  
  // 与 content script 通信
  GAME_IFRAME_DETECTED: 'gameIframeDetected',
  RESOURCE_TIMING: 'resourceTiming',
  NETWORK_STATUS: 'networkStatus',
  PAGE_UNLOAD: 'pageUnload',
  STATS_UPDATED: 'statsUpdated',
  
  // 与 Native Host 通信
  PING: 'ping',
  PONG: 'pong',
  NAVIGATE: 'navigate',
  LOGIN_SUCCESS: 'login_success',
  GET_INFO: 'get_info',
  ACTION: 'action',
  ERROR: 'error'
};

// 错误代码
export const ERROR_CODES = {
  NATIVE_HOST_NOT_FOUND: 'NATIVE_HOST_NOT_FOUND',
  NATIVE_HOST_DISCONNECTED: 'NATIVE_HOST_DISCONNECTED',
  CACHE_INIT_FAILED: 'CACHE_INIT_FAILED',
  STORAGE_QUOTA_EXCEEDED: 'STORAGE_QUOTA_EXCEEDED',
  INVALID_URL: 'INVALID_URL',
  NETWORK_ERROR: 'NETWORK_ERROR',
  PERMISSION_DENIED: 'PERMISSION_DENIED'
};

// 默认值
export const DEFAULTS = {
  // 默认用户代理
  userAgent: 'CTWebPlayer-Extension/1.0.0',
  
  // 默认语言
  language: 'zh-CN',
  
  // 默认编码
  encoding: 'UTF-8',
  
  // 默认重试次数
  maxRetries: 3,
  
  // 默认重试延迟（毫秒）
  retryDelay: 1000
};

// 正则表达式模式
export const REGEX_PATTERNS = {
  // URL 验证
  url: /^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$/,
  
  // 域名验证
  domain: /^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$/i,
  
  // IP 地址验证
  ip: /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/,
  
  // 文件扩展名提取
  fileExtension: /\.([a-zA-Z0-9]+)(?:[?#]|$)/
};

// 快捷键配置
export const KEYBOARD_SHORTCUTS = {
  // 显示/隐藏性能统计
  toggleStats: {
    key: 'C',
    modifiers: ['ctrl', 'shift']
  },
  
  // 清除缓存
  clearCache: {
    key: 'X',
    modifiers: ['ctrl', 'shift']
  },
  
  // 打开设置
  openSettings: {
    key: 'S',
    modifiers: ['ctrl', 'shift']
  }
};

// MIME 类型映射
export const MIME_TYPES = {
  // 图片
  'jpg': 'image/jpeg',
  'jpeg': 'image/jpeg',
  'png': 'image/png',
  'gif': 'image/gif',
  'webp': 'image/webp',
  'svg': 'image/svg+xml',
  'ico': 'image/x-icon',
  
  // 脚本
  'js': 'application/javascript',
  'mjs': 'application/javascript',
  
  // 样式
  'css': 'text/css',
  
  // 字体
  'woff': 'font/woff',
  'woff2': 'font/woff2',
  'ttf': 'font/ttf',
  'eot': 'application/vnd.ms-fontobject',
  'otf': 'font/otf',
  
  // 媒体
  'mp4': 'video/mp4',
  'webm': 'video/webm',
  'ogg': 'video/ogg',
  'mp3': 'audio/mpeg',
  'wav': 'audio/wav',
  
  // Unity WebGL
  'wasm': 'application/wasm',
  'data': 'application/octet-stream',
  
  // 其他
  'json': 'application/json',
  'xml': 'application/xml',
  'html': 'text/html',
  'htm': 'text/html'
};

// 可缓存的文件扩展名列表
export const CACHEABLE_EXTENSIONS = [
  'jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'ico',  // 图片
  'js', 'mjs',                                         // 脚本
  'css',                                               // 样式
  'woff', 'woff2', 'ttf', 'eot', 'otf',              // 字体
  'mp4', 'webm', 'ogg', 'mp3', 'wav',                // 媒体
  'data', 'wasm'                                       // Unity WebGL
];

// 导出所有配置
export default {
  NATIVE_HOST_NAME,
  CACHE_CONFIG,
  PERFORMANCE_CONFIG,
  UI_CONFIG,
  LOG_CONFIG,
  API_ENDPOINTS,
  STORAGE_KEYS,
  MESSAGE_TYPES,
  ERROR_CODES,
  DEFAULTS,
  REGEX_PATTERNS,
  KEYBOARD_SHORTCUTS,
  MIME_TYPES,
  CACHEABLE_EXTENSIONS
};