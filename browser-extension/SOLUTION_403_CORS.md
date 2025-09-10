# 403 Forbidden 和 CORS 错误解决方案

## 问题诊断

### 1. 403 Forbidden 错误
- **原因**：服务器端（hefc.hrbzqy.com）实施了防盗链保护，检查请求的 `Referer` 头
- **表现**：直接访问 `/patch/node3/` 路径下的资源返回 403 错误

### 2. CORS 错误
- **原因**：插件使用 `fetch()` API 重新请求资源时，被浏览器的 CORS 策略阻止
- **表现**：从 `patch-ct-labs.ecchi.xxx` 访问 `hefc.hrbzqy.com` 的资源时缺少 `Access-Control-Allow-Origin` 头

## 解决方案

### 方案 1：使用 declarativeNetRequest API（推荐）

已实施的更改：

1. **更新了 manifest.json**
   - 添加了 `declarativeNetRequest` 和 `declarativeNetRequestWithHostAccess` 权限
   - 配置了规则资源文件 `rules.json`

2. **创建了 rules.json**
   - 规则 1：为 `hefc.hrbzqy.com` 的响应添加 CORS 头
   - 规则 2：修改请求头，设置正确的 `Referer` 并移除 `Origin`

### 方案 2：改进的 background.js（备选）

创建了 `background-improved.js`，使用 `filterResponseData` API 直接从原始响应中获取数据，避免二次请求。

### 方案 3：临时使用 Manifest V2（应急）

创建了 `manifest-v2.json`，可以使用更强大的 webRequest API。

## 实施步骤

1. **使用方案 1（推荐）**：
   ```bash
   # 确保 rules.json 和更新的 manifest.json 已就位
   # 重新加载插件
   ```

2. **如果方案 1 不够，添加更多规则**：
   ```json
   {
     "id": 3,
     "priority": 2,
     "action": {
       "type": "modifyHeaders",
       "requestHeaders": [
         {
           "header": "User-Agent",
           "operation": "set",
           "value": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
         }
       ]
     },
     "condition": {
       "urlFilter": "*hefc.hrbzqy.com*",
       "resourceTypes": 