# GitHub Actions 工作流程说明

本项目使用 GitHub Actions 实现自动化构建、测试和发布流程。

## 工作流程文件

### 1. CI 工作流程 (ci.yml)

**触发条件**：
- 推送到 `main`、`master`、`develop` 分支
- 创建或更新 Pull Request

**功能**：
- 在 Windows 环境下构建项目（Debug 和 Release 两种配置）
- 运行测试（如果有测试项目）
- 代码质量检查
- 安全扫描
- 检查第三方许可证

**使用场景**：
- 每次提交代码时自动验证构建是否成功
- PR 合并前确保代码质量

### 2. Release 工作流程 (release.yml)

**触发条件**：
- 创建以 `v` 开头的标签（如 `v1.0.0`）

**功能**：
- 自动构建发布版本
- 生成 ZIP 压缩包
- 计算文件 SHA256 哈希值
- 创建 GitHub Release
- 上传发布文件

## 使用方法

### 发布新版本

1. **更新版本号**（可选，工作流会自动处理）：
   ```powershell
   # 在本地运行发布脚本
   .\scripts\release.ps1 -Version "1.0.0"
   ```

2. **创建并推送标签**：
   ```bash
   # 创建标签
   git tag -a v1.0.0 -m "Release version 1.0.0"
   
   # 推送标签到 GitHub
   git push origin v1.0.0
   ```

3. **自动发布流程**：
   - GitHub Actions 会自动触发发布工作流程
   - 构建完成后会自动创建 Release
   - 发布文件会自动上传

### 手动创建 GitHub Release

如果需要手动创建 Release，可以使用更新后的发布脚本：

```powershell
# 设置 GitHub Token
$env:GITHUB_TOKEN = "your-github-token"

# 运行发布脚本并创建 GitHub Release
.\scripts\release.ps1 -Version "1.0.0" -CreateTag -CreateGitHubRelease
```

## 配置要求

### GitHub Token

工作流程使用内置的 `GITHUB_TOKEN`，无需额外配置。

### 环境变量

本地使用发布脚本创建 GitHub Release 时需要设置：
- `GITHUB_TOKEN`: 具有 `repo` 权限的 Personal Access Token

## 工作流程详解

### CI 工作流程步骤

1. **检出代码**：获取最新代码
2. **设置 .NET SDK**：安装 .NET 8.0
3. **还原依赖**：`dotnet restore`
4. **构建项目**：Debug 和 Release 配置
5. **运行测试**：如果存在测试项目
6. **代码分析**：仅在 Release 配置下运行
7. **发布构建**：生成单文件可执行程序
8. **上传构建产物**：PR 时可下载调试

### Release 工作流程步骤

1. **检出代码**：包含完整 git 历史
2. **设置 .NET SDK**：安装 .NET 8.0
3. **获取版本号**：从标签名提取
4. **构建项目**：运行 `build.ps1`
5. **打包文件**：运行 `package.ps1`
6. **计算哈希**：生成 SHA256 校验和
7. **生成更新日志**：基于 git 提交记录
8. **准备发布说明**：使用模板文件
9. **创建 Release**：自动发布到 GitHub
10. **上传文件**：ZIP 包和校验和文件

## 注意事项

1. **版本号格式**：必须使用语义化版本，如 `v1.0.0`
2. **分支保护**：建议为主分支启用保护规则
3. **发布权限**：只有具有写权限的用户可以创建标签和发布
4. **构建缓存**：Actions 会自动缓存 .NET 包以加快构建速度
5. **并行构建**：CI 会同时构建 Debug 和 Release 版本

## 故障排除

### CI 构建失败

1. 检查是否有编译错误
2. 确认所有依赖包版本兼容
3. 查看 Actions 日志获取详细错误信息

### Release 发布失败

1. 确认标签格式正确（`v` + 版本号）
2. 检查是否有权限创建 Release
3. 验证构建和打包脚本是否正常工作

### 常见问题

**Q: 如何跳过 CI 检查？**
A: 在提交信息中包含 `[skip ci]` 或 `[ci skip]`

**Q: 如何重新运行失败的工作流程？**
A: 在 Actions 页面点击 "Re-run jobs"

**Q: 如何测试工作流程？**
A: 可以在功能分支创建测试标签，如 `v0.0.1-test`

## 相关文件

- `.github/workflows/ci.yml` - CI 工作流程配置
- `.github/workflows/release.yml` - 发布工作流程配置
- `.github/release-template.md` - Release 说明模板
- `scripts/build.ps1` - 构建脚本
- `scripts/package.ps1` - 打包脚本
- `scripts/release.ps1` - 发布脚本