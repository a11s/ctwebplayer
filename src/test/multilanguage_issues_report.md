# 多语言支持问题报告

## 检查日期：2025年9月14日

## 发现的问题

### 1. 硬编码的中文字符串

#### ProxySettingsForm.cs
- **MessageBox 消息**（行 222-228, 252-287）：
  - "代理设置已保存。\n\n请重启浏览器以应用新的代理设置。"
  - "代理已禁用。\n\n请重启浏览器以应用更改。"
  - "请至少填写一个代理服务器地址。"
  - "HTTP代理地址格式不正确。\n格式示例：http://127.0.0.1:7890"
  - "HTTPS代理地址格式不正确。\n格式示例：http://127.0.0.1:7890"
  - "SOCKS5代理地址格式不正确。\n格式示例：127.0.0.1:7890"
- **窗体标题**（行 42）：
  - this.Text = "代理设置";

#### UpdateForm.cs
- **按钮文本**（行 92）：
  - btnDownload.Text = "取消下载";

#### LogViewerForm.cs
- **文本内容**（行 73）：
  - txtLogContent.Text = "日志文件不存在。";

#### UpdateManager.cs
- **异常消息**（行 132, 270）：
  - throw new ArgumentException("无效的更新信息");
  - throw new InvalidOperationException("无法获取当前程序目录");

### 2. 正面发现

大多数控件都正确设置了 Tag 属性，这表明本地化基础架构已经到位：
- UpdateForm.Designer.cs：所有控件都有 Tag 属性
- SettingsForm.Designer.cs：所有控件都有 Tag 属性
- LoginDialog.Designer.cs：所有控件都有 Tag 属性
- Form1.Designer.cs：所有控件都有 Tag 属性
- AboutForm.Designer.cs：部分控件缺少 Tag 属性

### 3. 需要改进的地方

#### AboutForm.Designer.cs
以下控件缺少 Tag 属性：
- lblTitle（行 47）
- lblVersion（行 56）
- btnOK（行 75）
- lblLicenseTitle（行 87）
- lblImportantNotice（行 98）
- Form.Text（行 117）

#### SettingsForm.Designer.cs
以下控件有硬编码文本但可能需要本地化：
- lblMB（行 339）："MB" - 可能不需要本地化
- lblWidthPx（行 492）："像素" - 需要本地化
- lblHeightPx（行 461）："像素" - 需要本地化
- lblSizeNote（行 432）：包含硬编码的中文说明
- lblCurrentSize（行 452）："当前大小：1136 x 640"
- btnResetSize（行 441）："重置大小" - 可能缺少 Tag

## 建议的修复方案

### 1. 立即修复（高优先级）

1. **ProxySettingsForm.cs**：
   - 将所有 MessageBox 消息替换为 LanguageManager.GetString() 调用
   - 将窗体标题使用 LanguageManager 设置

2. **UpdateForm.cs**：
   - 将 "取消下载" 替换为资源字符串

3. **LogViewerForm.cs**：
   - 将 "日志文件不存在。" 替换为资源字符串

4. **UpdateManager.cs**：
   - 将异常消息替换为资源字符串

### 2. 中等优先级

1. **AboutForm.Designer.cs**：
   - 为所有控件添加适当的 Tag 属性

2. **SettingsForm.Designer.cs**：
   - 为 "像素" 等文本添加本地化支持
   - 检查 btnResetSize 是否有 Tag 属性

### 3. 需要添加到资源文件的新键

以下是需要添加到所有语言资源文件的新键：

```
// ProxySettingsForm
ProxySettings_Title = "代理设置"
ProxySettings_SavedMessage = "代理设置已保存。\n\n请重启浏览器以应用新的代理设置。"
ProxySettings_DisabledMessage = "代理已禁用。\n\n请重启浏览器以应用更改。"
ProxySettings_EmptyAddressError = "请至少填写一个代理服务器地址。"
ProxySettings_InvalidHttpError = "HTTP代理地址格式不正确。\n格式示例：http://127.0.0.1:7890"
ProxySettings_InvalidHttpsError = "HTTPS代理地址格式不正确。\n格式示例：http://127.0.0.1:7890"
ProxySettings_InvalidSocks5Error = "SOCKS5代理地址格式不正确。\n格式示例：127.0.0.1:7890"

// UpdateForm
UpdateForm_CancelDownload = "取消下载"

// LogViewerForm
LogViewer_FileNotExist = "日志文件不存在。"

// UpdateManager
UpdateManager_InvalidInfo = "无效的更新信息"
UpdateManager_CannotGetDirectory = "无法获取当前程序目录"

// AboutForm
AboutForm_Title = "关于 CTWebPlayer"
AboutForm_ProductTitle = "CTWebPlayer"
AboutForm_Version = "Unity3D WebPlayer 专属浏览器 v{0}"
AboutForm_OK = "确定"
AboutForm_LicenseTitle = "许可证："
AboutForm_ImportantNotice = "⚠️ 重要提示：任何修改版本必须改名并声明与原作者无关！\n详情请仔细阅读下方许可证条款。"

// SettingsForm
Settings_Pixels = "像素"
Settings_ResetSize = "重置大小"
Settings_CurrentSize = "当前大小：{0} x {1}"
Settings_SizeNote = "注意：更改窗口大小后，需要重启应用才能生效。\n当前窗口大小：{0} x {1} 像素"
```

## 测试建议

1. 修复所有硬编码字符串后，运行完整的测试套件
2. 特别注意以下场景：
   - 代理设置的所有错误消息
   - 更新下载过程中的状态切换
   - 日志查看器的错误处理
   - 关于窗口的显示

## 总结

虽然多语言基础架构已经建立良好，但仍有一些硬编码字符串需要处理。大部分问题集中在：
- 错误消息和提示信息
- 动态生成的文本
- 一些较新添加的功能（如代理设置）

建议按照优先级逐步修复这些问题，确保应用程序完全支持多语言。