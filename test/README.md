# CTWebPlayer 测试文档

本目录包含 CTWebPlayer 项目的各种测试文件和测试脚本。

## 测试文件概览

### 1. HTML 测试页面

#### resource_loading_test.html
- **用途**：测试 CTWebPlayer 的资源加载和缓存功能
- **测试内容**：
  - 外部 CSS 资源加载（Bootstrap、Font Awesome）
  - 外部 JavaScript 资源加载（Bootstrap、jQuery）
  - 图片资源加载（placeholder 图片）
  - API 请求（JSONPlaceholder API）
- **运行方式**：在 CTWebPlayer 中打开此文件，观察资源加载和缓存行为

#### cache_test.html
- **用途**：测试缓存功能的完整性
- **测试内容**：验证各种资源的缓存机制是否正常工作
- **详细说明**：参见 `README_CACHE_TEST.md`

#### webview2_data_test.html
- **用途**：测试 WebView2 用户数据目录功能
- **测试内容**：
  - 验证用户数据目录的创建和管理
  - 测试会话数据的持久化
  - 检查缓存目录的正确性

### 2. PowerShell 测试脚本

#### release-test.ps1
- **用途**：自动化发布测试，验证版本一致性、文件完整性和构建过程
- **功能**：
  - 版本一致性检查（csproj、Git 标签）
  - 必需文件存在性检查
  - 构建过程测试
  - 打包过程测试（Inno Setup）
- **运行方式**：
  ```powershell
  # 基本用法
  .\test\release-test.ps1
  
  # 指定版本测试
  .\test\release-test.ps1 -Version "1.0.0"
  
  # 清理旧构建并测试
  .\test\release-test.ps1 -Clean
  
  # 显示详细输出
  .\test\release-test.ps1 -Verbose
  ```

### 3. 测试说明文档

#### debug_test_instructions.md
- **用途**：提供调试模式下的测试指南
- **内容**：详细的调试测试步骤和注意事项

#### README_CACHE_TEST.md
- **用途**：缓存测试的详细文档
- **内容**：缓存测试的具体步骤、预期结果和验证方法

## 运行测试的推荐顺序

1. **功能测试**
   - 先运行 `resource_loading_test.html` 验证基本资源加载
   - 然后运行 `cache_test.html` 验证缓存功能
   - 最后运行 `webview2_data_test.html` 验证数据持久化

2. **发布前测试**
   - 运行 `release-test.ps1` 进行完整的发布验证
   - 确保所有检查项都通过后再进行发布

## 测试环境要求

- Windows 10 或更高版本
- .NET 6.0 或更高版本
- WebView2 Runtime
- PowerShell 5.0 或更高版本（用于运行测试脚本）
- Inno Setup（用于打包测试，可选）

## 常见问题

### Q: 资源加载测试失败？
A: 检查网络连接，确保可以访问外部资源（CDN、API）

### Q: 发布测试脚本报错？
A: 确保已安装所有必需的开发工具，并且项目结构符合预期

### Q: 缓存测试结果不一致？
A: 清理缓存目录后重新测试，注意检查缓存策略设置

## 贡献指南

添加新测试时，请：
1. 在适当的类别下创建测试文件
2. 更新本 README.md 文件
3. 如果测试较复杂，创建单独的说明文档
4. 确保测试可以独立运行，不依赖特定环境配置