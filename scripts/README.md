# CTWebPlayer 构建和发布脚本

本目录包含用于构建、打包和发布 CTWebPlayer 的 PowerShell 脚本。

## 系统要求

- Windows 10 或更高版本
- PowerShell 5.1 或更高版本
- .NET 8.0 SDK
- Git（如果需要创建标签）

## 脚本说明

### 1. build.ps1 - 构建脚本

用于构建项目并生成单文件可执行文件。

**功能：**
- 清理之前的构建输出
- 还原项目依赖
- 使用 `dotnet publish` 构建项目
- 生成包含 WebView2 运行时的单文件 exe
- 复制必要的资源文件

**参数：**
- `-Configuration` (默认: "Release") - 构建配置
- `-Runtime` (默认: "win-x64") - 目标运行时
- `-OutputDir` (默认: "publish") - 输出目录

**使用示例：**
```powershell
# 使用默认设置构建
.\scripts\build.ps1

# 指定输出目录
.\scripts\build.ps1 -OutputDir "custom-output"

# Debug 模式构建
.\scripts\build.ps1 -Configuration Debug
```

### 2. package.ps1 - 打包脚本

将构建输出打包成发布版本，生成 ZIP 文件和 SHA256 校验和。

**功能：**
- 收集构建输出文件
- 创建发布目录结构
- 生成 README.txt
- 创建 ZIP 压缩包
- 计算 SHA256 哈希值
- 生成发布信息 JSON

**参数：**
- `-PublishDir` (默认: "publish") - 构建输出目录
- `-PackageDir` (默认: "release") - 打包输出目录
- `-Version` (默认: "1.0.0") - 版本号
- `-CreateInstaller` - 创建 Inno Setup 安装程序

**使用示例：**
```powershell
# 打包默认版本
.\scripts\package.ps1

# 指定版本号
.\scripts\package.ps1 -Version "1.2.3"

# 自定义目录
.\scripts\package.ps1 -PublishDir "custom-publish" -PackageDir "custom-release"

# 创建安装程序
.\scripts\package.ps1 -Version "1.2.3" -CreateInstaller
```

### 3. release.ps1 - 主发布脚本

协调整个发布流程，包括更新版本号、构建、打包等。

**功能：**
- 更新项目文件中的版本号
- 调用构建脚本
- 调用打包脚本
- 创建 CHANGELOG 模板
- 可选：创建 Git 标签
- 生成 GitHub Release 草稿

**参数：**
- `-Version` (必需) - 发布版本号
- `-Configuration` (默认: "Release") - 构建配置
- `-Runtime` (默认: "win-x64") - 目标运行时
- `-SkipBuild` - 跳过构建步骤
- `-SkipTests` - 跳过测试步骤
- `-CreateTag` - 创建 Git 标签

**使用示例：**
```powershell
# 完整发布流程
.\scripts\release.ps1 -Version "1.0.0"

# 创建带标签的发布
.\scripts\release.ps1 -Version "1.0.0" -CreateTag

# 跳过构建（如果已经构建过）
.\scripts\release.ps1 -Version "1.0.0" -SkipBuild
```

## 典型工作流程

### 1. 开发完成后的完整发布

```powershell
# 1. 确保所有代码已提交
git status

# 2. 运行完整发布流程
.\scripts\release.ps1 -Version "1.0.0" -CreateTag

# 3. 更新 CHANGELOG.md
notepad CHANGELOG.md

# 4. 提交版本更新
git add .
git commit -m "Release v1.0.0"

# 5. 推送代码和标签
git push origin main
git push origin v1.0.0
```

### 2. 仅构建测试

```powershell
# 构建项目
.\scripts\build.ps1

# 运行构建的程序测试
.\publish\ctwebplayer.exe
```

### 3. 手动打包现有构建

```powershell
# 如果已经有构建输出，仅执行打包
.\scripts\package.ps1 -Version "1.0.0-beta"
```

## 输出文件说明

### 构建输出 (publish/)
- `ctwebplayer.exe` - 主程序
- `config.json` - 配置文件（如果存在）
- `res/` - 资源文件夹
- `LICENSE` - 许可证文件
- `THIRD_PARTY_LICENSES.txt` - 第三方许可证

### 打包输出 (release/)
- `CTWebPlayer-vX.Y.Z-win-x64.zip` - 发布压缩包
- `CTWebPlayer-vX.Y.Z-checksums.txt` - SHA256 校验和
- `CTWebPlayer-vX.Y.Z-Setup.exe` - Windows 安装程序（使用 -CreateInstaller 时）
- `CTWebPlayer-vX.Y.Z-Setup.exe.sha256` - 安装程序 SHA256 校验和
- `release-info.json` - 发布信息
- `RELEASE_DRAFT.md` - GitHub Release 草稿

## 常见问题

### 1. 执行策略错误

如果遇到 "无法加载脚本" 错误，需要设置 PowerShell 执行策略：

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### 2. .NET SDK 未找到

确保已安装 .NET 8.0 SDK：

```powershell
# 检查安装的 SDK 版本
dotnet --list-sdks

# 如果未安装，从以下地址下载
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 3. 构建失败

检查以下内容：
- 项目文件 `src/ctwebplayer.csproj` 是否存在
- 所有 NuGet 包是否可以正常还原
- 查看详细错误信息

### 4. 打包失败

确保：
- 先运行 `build.ps1` 生成构建输出
- `publish` 目录存在且包含 `ctwebplayer.exe`

### 5. 安装程序创建失败

如果使用 `-CreateInstaller` 参数时失败：
- 确保已安装 Inno Setup 6.x
- 检查 `installer/setup.iss` 文件是否存在
- 查看详细错误信息

## 注意事项

1. **版本号格式**：使用语义化版本号（如 1.0.0, 1.2.3-beta）

2. **Git 标签**：使用 `-CreateTag` 参数前确保所有更改已提交

3. **发布到 GitHub**：
   - 脚本生成的文件在 `release/` 目录
   - 手动在 GitHub 上创建 Release
   - 上传 ZIP 文件和校验和文件

4. **代码签名**：
   - 目前脚本不包含代码签名功能
   - 如需签名，请在打包前手动使用 `signtool.exe`

5. **WebView2 运行时**：
   - 构建的 exe 依赖 WebView2 运行时
   - 程序会在首次运行时检查并提示安装
   
   6. **Inno Setup 安装程序**：
      - 使用 `-CreateInstaller` 参数创建 Windows 安装程序
      - 需要先安装 Inno Setup 6.x：https://jrsoftware.org/isdl.php
      - 安装程序支持静默安装（/SILENT 或 /VERYSILENT）
      - 支持自定义安装路径（/DIR="路径"）

## 维护说明

- 更新构建参数：编辑 `build.ps1` 中的 `$publishArgs`
- 修改打包内容：编辑 `package.ps1` 中的文件收集部分
- 自定义发布流程：修改 `release.ps1` 中的步骤

## 许可证

这些脚本是 CTWebPlayer 项目的一部分，遵循相同的 BSD 3-Clause 许可证。