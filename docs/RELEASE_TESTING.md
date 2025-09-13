# CTWebPlayer 发布测试指南

## 概述

本指南详细说明了 CTWebPlayer 项目发布前的测试流程，确保软件质量、功能完整性和用户体验。测试覆盖本地验证、自动化构建、自动更新功能、安装程序以及 GitHub Actions 集成。

测试基于 Semantic Versioning (SemVer)，假设当前版本为 1.0.0。所有测试应在干净的 Windows 10/11 虚拟机环境中进行，避免本地开发环境的影响。

## 发布前检查清单

在开始任何测试前，确保以下项目已完成：

- [ ] **版本管理**
  - 在 `src/ctwebplayer.csproj` 中更新 `<Version>1.0.0</Version>` 和相关版本属性
  - 创建 Git tag：`git tag v1.0.0`
  - 更新 `CHANGELOG.md` 以记录变更（使用 Keep a Changelog 格式）

- [ ] **代码审查**
  - 移除所有调试代码（例如 `#if DEBUG` 条件下的日志）
  - 检查第三方依赖许可（`THIRD_PARTY_LICENSES.txt`）
  - 验证 BSD3 协议合规：确保 README.md 包含分发声明

- [ ] **文档更新**
  - 更新 `README.md`：添加安装指南、系统要求（Windows 10+，WebView2 Runtime）
  - 验证 `docs/RELEASE_PLAN.md` 中的流程与实际匹配
  - 准备 GitHub Release 模板（`.github/release-template.md`）

- [ ] **构建配置**
  - 确认 `src/ctwebplayer.csproj` 包含 Release 配置（无调试符号，优化启用）
  - 测试 dotnet publish 命令：`dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish`

- [ ] **安全检查**
  - 运行本地病毒扫描（使用 Windows Defender 或 VirusTotal 上传）
  - 检查日志和缓存路径权限（应允许用户读写）
  - 验证 WebView2 依赖：确保应用能正确提示安装 Runtime

- [ ] **许可文件**
  - 包含 `LICENSE` (BSD3) 和 `THIRD_PARTY_LICENSES.txt`
  - Inno Setup 脚本 (`installer/setup.iss`) 中引用许可协议

## 本地测试步骤

### 1. 构建和打包测试

```powershell
# 步骤 1: 清理并构建
dotnet clean
dotnet build -c Release

# 步骤 2: 发布单文件 exe
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

# 步骤 3: 复制附加文件
copy config.json publish\
xcopy res publish\res /E /I

# 步骤 4: 构建安装程序
cd installer
.\build-installer.ps1
cd ..

# 验证输出
# - publish/ctwebplayer.exe (约 100MB，包含 WebView2)
# - output/setup.exe (Inno Setup 安装包)
```

**预期结果**：
- 构建无错误或警告
- exe 文件大小合理（自包含模式下较大）
- 附加文件正确复制（config.json, res/ 目录）

### 2. 功能测试

在干净 VM 中运行 `publish/ctwebplayer.exe`：

- [ ] **启动测试**
  - 应用正常启动，无崩溃
  - 检查 WebView2 安装提示（如果缺失）
  - 加载默认页面（Unity WebPlayer 测试页）

- [ ] **核心功能测试**
  - **缓存**：加载资源，验证 `cache/` 目录创建和文件存储
  - **代理**：配置代理设置，测试网络请求通过代理
  - **日志**：启用日志，检查 `logs/` 目录和 LogViewerForm
  - **CORS**：测试浏览器扩展集成，验证跨域请求处理
  - **设置**：修改配置，保存并重启应用，验证持久化

- [ ] **UI/UX 测试**
  - AboutForm：显示正确版本和许可信息
  - SettingsForm：所有控件响应正常
  - ProxySettingsForm：输入验证和保存
  - LogViewerForm：实时日志显示和过滤

- [ ] **错误处理测试**
  - 网络断开：应用优雅降级，显示错误消息
  - 无效 URL：WebView2 错误处理
  - 权限问题：缓存/日志目录无写权限时的行为

### 3. 性能测试

- [ ] 启动时间 < 5 秒
- [ ] 内存使用 < 200MB（空闲状态）
- [ ] 加载 Unity WebPlayer 页面 < 10 秒
- [ ] CPU 使用率正常（无持续高负载）

## GitHub Actions 测试

### 1. CI 工作流测试

在 `.github/workflows/ci.yml` 中配置 CI 测试：

```yaml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with: { dotnet-version: '8.0.x' }
    - run: dotnet restore
    - run: dotnet build --no-restore -c Release
    - run: dotnet test --no-build --configuration Release --logger "console;verbosity=detailed"
    - run: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

**测试步骤**：
- Push 代码到测试分支
- 检查 Actions 标签页：构建成功，无测试失败
- 下载 artifact（如果配置），验证 exe 在本地运行

**预期结果**：
- 所有单元测试通过（如果有 test/ 项目）
- 发布输出无警告
- 构建时间 < 5 分钟

### 2. Release 工作流测试

在 `.github/workflows/release.yml` 中配置：

```yaml
name: Release
on:
  push:
    tags: ['v*']
jobs:
  release:
    runs-on: windows-latest
    steps:
    # ... 构建步骤
    - uses: softprops/action-gh-release@v1
      with:
        files: |
          publish/ctwebplayer.exe
          output/setup.exe
        generate_release_notes: true
```

**测试步骤**：
- 创建测试 tag：`git tag v0.1.0-test && git push origin v0.1.0-test`
- 验证 Release 创建，assets 上传成功
- 检查 release notes 生成

**预期结果**：
- Release 页面显示 exe 和 setup.exe 下载链接
- 文件完整性：本地下载验证大小和 MD5

## 自动更新功能测试

### 1. 手动模拟测试
**前提**：确保 src/UpdateManager.cs 已实现（参考 docs/RELEASE_PLAN.md）


- [ ] **版本检查**
  - 修改当前 exe 版本为 1.0.0
  - 在 GitHub Release 创建 v1.0.1，上传新 exe
  - 运行应用，验证 API 调用：`https://api.github.com/repos/a11s/ctwebplayer/releases/latest`
  - 检查版本比较逻辑（tag_name vs Assembly Version）

- [ ] **下载和验证**
  - 确认下载 asset（exe 文件）
  - 验证 SHA256 hash（在 Release notes 中提供）
  - 测试网络失败回滚

- [ ] **安装流程**
  - 用户确认对话框显示正确
  - 下载到 temp.exe
  - 创建自更新 bat 脚本：kill 进程，替换文件，重启
  - 验证重启后版本更新

**预期结果**：
- 更新成功，无文件损坏
- 旧版本备份（old.exe）
- 日志记录更新事件

### 2. 端到端测试

- 在 VM 中安装旧版，触发更新
- 验证新版功能正常
- 测试离线模式：无更新提示

**常见问题**：
- API 限速：使用 GitHub token
- Hash 不匹配：重新计算并更新 Release notes
- 自更新失败：权限提升或手动安装

## 安装程序测试

使用 Inno Setup 生成的 `setup.exe`：

### 1. 安装测试

```powershell
# 静默安装
output/setup.exe /SILENT /DIR="C:\Program Files\CTWebPlayer"

# 交互安装
output/setup.exe
```

- [ ] **文件安装**
  - exe 到 {app} 目录
  - config.json 和 res/ 到正确位置
  - 桌面/开始菜单快捷方式创建

- [ ] **依赖检查**
  - WebView2 Runtime 自动安装（如果缺失）
  - 许可协议显示（LICENSE.txt）

- [ ] **权限测试**
  - 管理员安装：Program Files
  - 用户安装：AppData

### 2. 卸载测试

- 通过控制面板卸载
- 验证文件删除（保留用户数据？配置 [UninstallDelete]）
- 快捷方式移除

**预期结果**：
- 安装 < 1 分钟
- 卸载无残留文件
- 注册表清理（如果配置）

## 常见问题和解决方案

### 构建问题

1. **dotnet publish 失败：WebView2 依赖缺失**
   - 解决方案：确保 NuGet 包 Microsoft.Web.WebView2 更新到最新
   - 检查：`dotnet restore --force`

2. **单文件发布过大**
   - 解决方案：使用 `-p:PublishTrimmed=true`（谨慎，可能破坏 WebView2）
   - 替代：分文件发布 + Inno Setup 打包

### 功能问题

1. **自动更新 API 403 错误**
   - 原因：GitHub rate limit
   - 解决方案：添加 GitHub token 到 ConfigManager，或延迟检查

2. **缓存目录权限错误**
   - 原因：安装到只读目录
   - 解决方案：使用 %LOCALAPPDATA%\CTWebPlayer\cache

3. **WebView2 崩溃**
   - 原因：Runtime 版本不兼容
   - 解决方案：Evergreen Bootstrapper 在安装程序中

### 测试环境问题

1. **VM 中网络代理干扰**
   - 解决方案：禁用 VM 代理，使用应用内置代理设置

2. **GitHub Actions 超时**
   - 原因：Windows runner 慢
   - 解决方案：优化 publish 参数，添加 cache 步骤

3. **签名相关（如果未来启用）**
   - 跳过签名：接受 SmartScreen 警告
   - 测试：用户需点击"更多信息 > 仍要运行"

### 调试技巧

- 启用详细日志：config.json 中 "logLevel": "Debug"
- 使用 Process Monitor 监控文件/注册表访问
- Wireshark 捕获网络流量（更新 API）
- Event Viewer 检查 Windows 事件

## 参考资源

- [Inno Setup 文档](https://jrsoftware.org/ishelp/)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-dotnet)
- [WebView2 测试指南](https://docs.microsoft.com/en-us/microsoft-edge/webview2/concepts/testing)
- [Semantic Versioning](https://semver.org/)

此指南应在每个发布周期执行。建议自动化尽可能多的检查（见 test/release-test.ps1）。