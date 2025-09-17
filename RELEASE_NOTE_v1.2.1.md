# CTWebPlayer v1.2.1 Release Notes

发布日期：2025年9月17日

## 📋 版本概述

CTWebPlayer v1.2.1 是一个维护版本，主要专注于改善用户体验和修复关键的线程安全问题。此版本优化了窗口标题显示，让用户能够更直观地了解当前的登录状态和玩家信息。

## ✨ 主要更新

### 🎯 窗口标题智能显示
- **动态标题栏**：窗口标题现在会智能显示网页标题和玩家昵称
  - 已登录用户：显示格式为 `网页标题 - 玩家昵称`
  - 未登录用户：显示格式为 `网页标题 - （未登录）`
- **实时更新**：标题会随着页面导航和登录状态的变化而动态更新
- **智能识别**：自动从 Cookie 中读取玩家的 `erolabsnickname`
- **完整字符支持**：正确处理 URL 编码的中文等特殊字符

### 🔧 稳定性改进
- **线程安全增强**
  - 修复了退出登录时可能出现的线程错误
  - 确保所有 CoreWebView2 操作在 UI 线程中执行
  - 优化了 Cookie 操作的线程安全性
  - 解决了 `DeleteAllCookies` 方法引发的 `InvalidOperationException`

### 📈 代码质量优化
- 减少编译警告（从 52 个降至 50 个）
- 优化异步方法实现，移除不必要的 `await` 调用
- 使用 `Task.FromResult` 替代 `Task.CompletedTask` 提升性能

## 🚀 升级建议

### 推荐所有用户升级
此版本修复了重要的线程安全问题，特别是在频繁登录/退出操作时的稳定性问题。建议所有用户升级到此版本以获得更好的使用体验。

### 升级方式
1. **自动更新**：启动程序后会自动检查更新并提示安装
2. **手动下载**：从 [GitHub Releases](https://github.com/a11s/ctwebplayer/releases/tag/v1.2.1) 页面下载最新版本

## 📝 版本对比

### 自 v1.2.0 以来的累积更新：
- ✅ 窗口标题智能显示玩家信息
- ✅ 修复线程安全相关问题
- ✅ 代码质量和性能优化
- ✅ F10 游戏截图功能（v1.2.0）
- ✅ Unity WebGL 兼容性改进（v1.2.0）

## 🔗 相关链接

- [完整更新日志](https://github.com/a11s/ctwebplayer/blob/main/CHANGELOG.md)
- [问题反馈](https://github.com/a11s/ctwebplayer/issues)
- [下载页面](https://github.com/a11s/ctwebplayer/releases/tag/v1.2.1)

## 🙏 致谢

感谢所有用户的反馈和支持！您的建议帮助我们不断改进产品。

---

**CTWebPlayer** - 让云游戏体验更流畅  
版权所有 © 2025