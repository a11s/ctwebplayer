# CTWebPlayer {{VERSION}}

发布日期: {{DATE}}

## 📥 下载

| 文件 | 大小 | SHA256 |
|------|------|--------|
| [CTWebPlayer-{{VERSION}}-win-x64.zip](../../releases/download/{{TAG}}/CTWebPlayer-{{VERSION}}-win-x64.zip) | {{SIZE}} | `{{SHA256}}` |

## 📋 系统要求

- **操作系统**: Windows 10 或更高版本 (64位)
- **运行时**: Microsoft Edge WebView2 运行时
- **.NET**: 不需要单独安装（已包含在程序中）

## 🚀 新功能

- 

## 💪 改进

- 

## 🐛 修复

- 

## 📝 其他更改

- 

## 📦 安装说明

### 首次安装

1. 下载 `CTWebPlayer-{{VERSION}}-win-x64.zip` 文件
2. 解压到您选择的目录（如 `C:\Program Files\CTWebPlayer`）
3. 运行 `ctwebplayer.exe`
4. 如果提示需要安装 WebView2 运行时，请按照提示进行安装

### 从旧版本升级

1. 关闭正在运行的 CTWebPlayer
2. 下载新版本的 ZIP 文件
3. 解压并覆盖原有文件
4. 运行新版本的 `ctwebplayer.exe`

**注意**: 您的设置和缓存数据将被保留。

## 🔒 文件验证

请使用以下 SHA256 哈希值验证下载文件的完整性：

```
{{SHA256}}  CTWebPlayer-{{VERSION}}-win-x64.zip
```

您也可以下载 [CTWebPlayer-{{VERSION}}-checksums.txt](../../releases/download/{{TAG}}/CTWebPlayer-{{VERSION}}-checksums.txt) 文件查看所有文件的哈希值。

## 🏗️ 从源代码构建

如果您想从源代码构建项目：

```powershell
# 克隆仓库
git clone https://github.com/a11s/ctwebplayer.git
cd ctwebplayer

# 构建发布版本
.\scripts\release.ps1 -Version {{VERSION}}
```

## 📄 许可证

本软件基于 [BSD 3-Clause 许可证](https://github.com/a11s/ctwebplayer/blob/main/LICENSE) 发布。

第三方组件的许可证信息请查看 [THIRD_PARTY_LICENSES.txt](https://github.com/a11s/ctwebplayer/blob/main/THIRD_PARTY_LICENSES.txt)。

## 🙏 致谢

感谢所有贡献者和用户的支持！

## 🐞 问题反馈

如果您遇到任何问题，请在 [GitHub Issues](https://github.com/a11s/ctwebplayer/issues) 中报告。

---

**完整更新日志**: https://github.com/a11s/ctwebplayer/compare/{{PREVIOUS_TAG}}...{{TAG}}