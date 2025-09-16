# Cookie Manager 使用文档

## 概述

CTWebPlayer 内置了强大的Cookie管理器，让您可以轻松管理WebView2浏览器中的所有Cookies。该功能支持查看、添加、编辑、删除、导入和导出Cookies，为开发和调试提供便利。

## 功能特性

### 核心功能
- 🔍 **查看所有Cookies** - 显示当前浏览器中的所有Cookies
- ➕ **添加新Cookie** - 手动创建新的Cookie
- ✏️ **编辑Cookie** - 修改现有Cookie的属性
- 🗑️ **删除Cookie** - 删除单个或批量删除Cookies
- 📥 **导入Cookies** - 从JSON文件导入Cookies
- 📤 **导出Cookies** - 将Cookies导出为JSON文件
- 🔎 **搜索功能** - 快速查找特定的Cookie
- 📊 **分组显示** - 按域名分组查看Cookies

### 安全特性
- 支持HttpOnly标记
- 支持Secure标记
- 支持SameSite属性（None、Lax、Strict）
- 会话Cookie和持久Cookie管理

## 访问方式

### 1. 通过自定义协议访问

在浏览器地址栏输入以下任一URL：
```
ct://settings/cookie
ct://settings/cookies
```

### 2. 通过网页链接访问

在网页中添加链接：
```html
<a href="ct://settings/cookie">打开Cookie管理器</a>
```

### 3. 通过测试页面访问

打开测试页面 `test/cookie_manager_test.html`，点击"打开Cookie管理器"按钮。

## 使用指南

### 查看Cookies

1. 打开Cookie管理器后，所有Cookies会自动加载并显示在列表中
2. 每个Cookie显示以下信息：
   - **域名（Domain）** - Cookie所属的域名
   - **名称（Name）** - Cookie的名称
   - **值（Value）** - Cookie的值
   - **路径（Path）** - Cookie的路径
   - **过期时间（Expires）** - Cookie的过期时间（会话Cookie显示"Session"）
   - **大小（Size）** - Cookie值的字节大小

3. 状态栏显示统计信息：
   - 总计：所有Cookies的数量
   - 显示：当前显示的Cookies数量（搜索过滤后）
   - 已选：选中的Cookies数量

### 添加新Cookie

1. 点击工具栏的"添加"按钮
2. 在弹出的对话框中填写：
   - **域名** - 例如：`.example.com`（前导点表示包含子域名）
   - **名称** - Cookie的名称
   - **值** - Cookie的值
   - **路径** - 默认为 `/`
   - **过期时间** - 选择日期时间或留空创建会话Cookie
   - **安全选项**：
     - HttpOnly - 防止JavaScript访问
     - Secure - 仅通过HTTPS传输
     - SameSite - 跨站请求策略
3. 点击"确定"保存

### 编辑Cookie

1. 在列表中选择要编辑的Cookie
2. 右键点击选择"编辑"或双击Cookie行
3. 在编辑对话框中修改Cookie属性
4. 点击"确定"保存更改

### 删除Cookies

#### 删除单个Cookie
1. 选择要删除的Cookie
2. 点击工具栏的"删除"按钮或按Delete键
3. 确认删除操作

#### 批量删除
1. 按住Ctrl键选择多个Cookies
2. 点击"删除"按钮
3. 确认批量删除操作

### 搜索Cookies

1. 在搜索框中输入关键词
2. 搜索支持：
   - 域名搜索
   - Cookie名称搜索
   - Cookie值搜索
3. 实时过滤显示匹配的Cookies

### 导入Cookies

1. 点击工具栏的"导入"按钮
2. 选择JSON格式的Cookie文件
3. 确认导入操作
4. 查看导入结果报告

**JSON文件格式示例：**
```json
[
  {
    "Domain": ".example.com",
    "Name": "session_id",
    "Value": "abc123",
    "Path": "/",
    "Expires": "2025-12-31T23:59:59",
    "HttpOnly": true,
    "Secure": true,
    "SameSite": "Lax"
  }
]
```

### 导出Cookies

1. 选择要导出的Cookies（不选择则导出全部）
2. 点击工具栏的"导出"按钮
3. 选择保存位置和文件名
4. Cookies将以JSON格式保存

### 分组查看

1. 勾选"按域名分组"复选框
2. Cookies将按域名分组显示
3. 可以折叠/展开各个域名组

## 右键菜单功能

在Cookie列表上右键点击，可以快速访问以下功能：
- **复制** - 复制Cookie的名称和值
- **编辑** - 编辑选中的Cookie
- **删除** - 删除选中的Cookie

## 快捷键

- `F5` - 刷新Cookie列表
- `Ctrl+A` - 全选所有Cookies
- `Delete` - 删除选中的Cookies
- `Ctrl+C` - 复制选中的Cookie信息
- `双击` - 编辑Cookie

## 技术特点

### 1. 实时同步
Cookie管理器直接与WebView2的Cookie存储同步，所有更改立即生效。

### 2. 异步操作
所有Cookie操作都是异步执行的，不会阻塞UI界面。

### 3. 错误处理
完善的错误处理机制，操作失败时会显示详细的错误信息。

### 4. 多语言支持
支持简体中文、繁体中文、日文、韩文和英文界面。

## 使用场景

### 1. 开发调试
- 测试不同的Cookie值对应用的影响
- 模拟不同的用户状态
- 调试Cookie相关的功能

### 2. 登录管理
- 保存和恢复登录状态
- 切换不同的用户账号
- 清理过期的登录信息

### 3. 隐私保护
- 查看网站设置的Cookies
- 删除跟踪Cookies
- 管理第三方Cookies

### 4. 测试自动化
- 导入预设的测试Cookies
- 批量设置测试环境
- 导出测试结果

## 注意事项

1. **HttpOnly Cookies**
   - 标记为HttpOnly的Cookies无法通过JavaScript访问
   - 但可以在Cookie管理器中查看和编辑

2. **Secure Cookies**
   - 标记为Secure的Cookies只能通过HTTPS连接传输
   - 在HTTP页面中可能无法正常工作

3. **SameSite属性**
   - `None` - 允许跨站请求（需要Secure标记）
   - `Lax` - 部分跨站请求允许（默认值）
   - `Strict` - 严格限制跨站请求

4. **域名格式**
   - `.example.com` - 包含所有子域名
   - `example.com` - 仅限该域名
   - `subdomain.example.com` - 仅限特定子域名

5. **会话Cookie**
   - 不设置过期时间的Cookie为会话Cookie
   - 浏览器关闭时自动删除

## 故障排除

### Cookie不显示
- 确保WebView2已正确初始化
- 检查是否有权限访问Cookie存储
- 尝试刷新Cookie列表

### 添加Cookie失败
- 检查域名格式是否正确
- 确保必填字段都已填写
- 验证过期时间是否有效

### 导入失败
- 检查JSON文件格式是否正确
- 确保文件编码为UTF-8
- 验证Cookie属性的有效性

### 导出失败
- 检查是否有写入权限
- 确保磁盘空间充足
- 验证文件路径是否有效

## 相关文件

- 主程序：`src/CookieManagerForm.cs`
- Cookie管理类：`src/CookieManager.cs`
- Cookie编辑对话框：`src/CookieEditDialog.cs`
- Cookie数据模型：`src/CookieItem.cs`
- 协议处理：`src/CustomProtocolHandler.cs`
- 测试页面：`test/cookie_manager_test.html`

## 更新日志

### v1.0.0 (2025-01-16)
- 初始版本发布
- 支持基本的CRUD操作
- 实现导入/导出功能
- 添加多语言支持
- 集成ct://协议访问

## 联系与支持

如有问题或建议，请访问：
- GitHub: https://github.com/a11s/ctwebplayer
- 提交Issue或Pull Request

---
*本文档最后更新时间：2025年1月16日*