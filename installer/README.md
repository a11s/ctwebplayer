# CTWebPlayer 安装程序

本目录包含使用 Inno Setup 创建 Windows 安装程序的相关文件。

## 文件说明

- `setup.iss` - Inno Setup 脚本文件，定义了安装程序的配置和行为
- `build-installer.ps1` - PowerShell 脚本，用于自动化构建安装程序
- `README.md` - 本文档

## 系统要求

### 构建安装程序需要：
- Windows 10 或更高版本
- Inno Setup 6.x 或更高版本
- PowerShell 5.1 或更高版本
- 已完成的项目构建（在 `publish` 目录中）

### 安装程序支持的系统：
- Windows 10 64位或更高版本
- Microsoft Edge WebView2 运行时（安装程序会检测并提示安装）

## 安装 Inno Setup

1. 访问官方网站：https://jrsoftware.org/isdl.php
2. 下载 Inno Setup 6（推荐使用 Unicode 版本）
3. 运行安装程序，使用默认设置即可
4. 确保安装路径在系统 PATH 中，或安装到默认位置

## 创建安装程序

### 方法一：使用打包脚本（推荐）

```powershell
# 在项目根目录运行
.\scripts\package.ps1 -CreateInstaller -Version "1.0.0"
```

这将自动：
1. 检查构建输出
2. 调用 Inno Setup 编译器
3. 生成安装程序和 SHA256 校验和

### 方法二：直接运行构建脚本

```powershell
# 在 installer 目录运行
.\build-installer.ps1 -Version "1.0.0"

# 或指定发布目录
.\build-installer.ps1 -PublishDir "..\custom-publish" -Version "1.0.0"

# 静默模式（不显示 Inno Setup 编译输出）
.\build-installer.ps1 -Silent
```

### 方法三：手动使用 Inno Setup

1. 打开 Inno Setup Compiler
2. 加载 `setup.iss` 文件
3. 点击"Compile"按钮
4. 安装程序将生成在 `release` 目录

## 安装程序功能

### 基本功能
- 自动检测系统架构（仅支持 64 位）
- 检查管理员权限
- 创建开始菜单快捷方式
- 可选创建桌面快捷方式
- 完整的卸载支持
- 多语言支持（简体中文、英文）

### WebView2 运行时处理
- 安装后自动检测 WebView2 是否已安装
- 如未安装，提示用户下载
- 可选集成 WebView2 离线安装包

### 文件管理
- 安装主程序 `ctwebplayer.exe`
- 复制配置文件 `config.json`（保留已存在的配置）
- 安装资源文件和图标
- 包含许可证文件

### 卸载功能
- 完全删除程序文件
- 清理缓存和日志目录
- 移除注册表项
- 删除快捷方式

## 自定义安装程序

### 修改应用信息

编辑 `setup.iss` 文件顶部的定义：

```iss
#define AppName "CTWebPlayer"
#define AppVersion "1.0.0"
#define AppPublisher "Your Company"
#define AppURL "https://your-website.com"
```

### 添加文件关联

在 `[Registry]` 部分添加：

```iss
; 关联 .ctw 文件
Root: HKLM; Subkey: "Software\Classes\.ctw"; ValueType: string; ValueName: ""; ValueData: "CTWebPlayerFile"
Root: HKLM; Subkey: "Software\Classes\CTWebPlayerFile"; ValueType: string; ValueName: ""; ValueData: "CTWebPlayer File"
Root: HKLM; Subkey: "Software\Classes\CTWebPlayerFile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\ctwebplayer.exe,0"
Root: HKLM; Subkey: "Software\Classes\CTWebPlayerFile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\ctwebplayer.exe"" ""%1"""
```

### 添加 URL 协议

```iss
; 注册 ctwebplayer:// 协议
Root: HKLM; Subkey: "Software\Classes\ctwebplayer"; ValueType: string; ValueName: ""; ValueData: "CTWebPlayer Protocol"
Root: HKLM; Subkey: "Software\Classes\ctwebplayer"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""
```

### 集成 WebView2 离线安装

1. 下载 WebView2 离线安装包
2. 将文件放在 `installer` 目录
3. 修改 `setup.iss`：

```iss
[Files]
Source: "MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Run]
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "正在安装 Microsoft Edge WebView2 运行时..."; Flags: skipifsilent
```

## 命令行安装

安装程序支持多种命令行参数：

### 静默安装
```cmd
CTWebPlayer-v1.0.0-Setup.exe /SILENT
```

### 完全静默（无进度条）
```cmd
CTWebPlayer-v1.0.0-Setup.exe /VERYSILENT
```

### 自定义安装路径
```cmd
CTWebPlayer-v1.0.0-Setup.exe /DIR="C:\MyApps\CTWebPlayer"
```

### 不创建快捷方式
```cmd
CTWebPlayer-v1.0.0-Setup.exe /TASKS="!desktopicon,!quicklaunchicon"
```

### 组合使用
```cmd
CTWebPlayer-v1.0.0-Setup.exe /VERYSILENT /DIR="D:\Programs\CTWebPlayer" /TASKS="desktopicon"
```

## 故障排除

### 1. Inno Setup 未找到

**问题**：运行脚本时提示找不到 ISCC.exe

**解决方案**：
- 确保已安装 Inno Setup
- 检查安装路径是否正确
- 将 Inno Setup 安装目录添加到 PATH 环境变量
- 或修改 `build-installer.ps1` 中的路径

### 2. 编译失败

**问题**：Inno Setup 编译时出错

**常见原因**：
- 文件路径错误：确保所有源文件存在
- 图标文件缺失：检查 `res\c001_01_Icon_Texture.ico`
- 输出目录不存在：脚本会自动创建

### 3. 权限问题

**问题**：安装时提示权限不足

**解决方案**：
- 确保以管理员身份运行安装程序
- 或修改 `PrivilegesRequired` 设置为 `lowest`

### 4. WebView2 检测失败

**问题**：已安装 WebView2 但仍提示未安装

**解决方案**：
- 检查注册表检测逻辑
- 更新 WebView2 检测代码
- 考虑使用 WebView2 API 进行检测

## 最佳实践

1. **版本管理**
   - 始终更新版本号
   - 保持版本号与项目一致
   - 使用语义化版本号

2. **测试**
   - 在干净的虚拟机上测试安装
   - 测试升级场景
   - 验证卸载是否干净

3. **签名**
   - 对安装程序进行数字签名
   - 避免 Windows SmartScreen 警告
   - 提高用户信任度

4. **国际化**
   - 支持多语言安装界面
   - 根据系统语言自动选择

5. **日志**
   - 启用安装日志：`/LOG="install.log"`
   - 便于故障排除

## 参考资源

- [Inno Setup 官方文档](https://jrsoftware.org/ishelp/)
- [Inno Setup 示例脚本](https://github.com/jrsoftware/issrc/tree/main/Examples)
- [WebView2 文档](https://docs.microsoft.com/edge/webview2/)
- [Windows 安装程序最佳实践](https://docs.microsoft.com/windows/win32/msi/windows-installer-best-practices)

## 许可证

安装程序脚本遵循 CTWebPlayer 项目的 BSD 3-Clause 许可证。