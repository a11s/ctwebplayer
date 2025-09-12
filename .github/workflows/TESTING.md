# GitHub Actions 工作流程测试指南

本文档提供了测试和验证 GitHub Actions 工作流程的步骤。

## 测试前准备

1. **确保代码已提交**：
   ```bash
   git add .
   git commit -m "Add GitHub Actions workflows"
   git push origin main
   ```

2. **检查文件结构**：
   ```
   .github/
   ├── workflows/
   │   ├── ci.yml
   │   ├── release.yml
   │   └── README.md
   └── release-template.md
   ```

## CI 工作流程测试

### 测试步骤

1. **创建测试分支**：
   ```bash
   git checkout -b test/ci-workflow
   ```

2. **做一个小改动**：
   ```bash
   echo "// Test CI" >> Program.cs
   git add Program.cs
   git commit -m "Test CI workflow"
   git push origin test/ci-workflow
   ```

3. **创建 Pull Request**：
   - 在 GitHub 上创建从 `test/ci-workflow` 到 `main` 的 PR
   - 观察 CI 工作流程是否自动触发

### 验证点

- [ ] 工作流程自动启动
- [ ] Debug 和 Release 配置都能成功构建
- [ ] 代码质量检查通过
- [ ] 构建产物成功上传（如果是 PR）
- [ ] 所有步骤显示绿色勾号

## Release 工作流程测试

### 测试步骤

1. **创建测试标签**：
   ```bash
   # 先确保在主分支
   git checkout main
   git pull origin main
   
   # 创建测试标签
   git tag -a v0.0.1-test -m "Test release workflow"
   git push origin v0.0.1-test
   ```

2. **监控工作流程**：
   - 访问仓库的 Actions 页面
   - 查看 "Release" 工作流程是否触发

3. **检查发布结果**：
   - 访问仓库的 Releases 页面
   - 确认是否创建了新的 Release
   - 验证文件是否正确上传

### 验证点

- [ ] 标签推送后工作流程自动触发
- [ ] 构建成功完成
- [ ] ZIP 文件正确生成
- [ ] SHA256 校验和文件生成
- [ ] GitHub Release 自动创建
- [ ] 发布文件成功上传
- [ ] Release 说明格式正确

## 手动测试发布脚本

### 本地测试

1. **测试构建脚本**：
   ```powershell
   .\scripts\build.ps1 -Configuration Release
   ```

2. **测试打包脚本**：
   ```powershell
   .\scripts\package.ps1 -Version "0.0.1-test"
   ```

3. **测试发布脚本**（不创建标签）：
   ```powershell
   .\scripts\release.ps1 -Version "0.0.1-test" -SkipBuild
   ```

### 验证点

- [ ] 构建脚本成功运行
- [ ] 生成单文件 EXE
- [ ] 打包脚本创建 ZIP 文件
- [ ] 生成 SHA256 校验和文件
- [ ] 发布脚本更新版本号

## 清理测试数据

### 删除测试标签

```bash
# 删除本地标签
git tag -d v0.0.1-test

# 删除远程标签
git push origin --delete v0.0.1-test
```

### 删除测试 Release

1. 访问 GitHub Releases 页面
2. 找到测试 Release
3. 点击 "Delete this release"

### 删除测试分支

```bash
# 删除本地分支
git branch -d test/ci-workflow

# 删除远程分支（如果 PR 已关闭）
git push origin --delete test/ci-workflow
```

## 常见问题解决

### CI 工作流程未触发

1. 检查分支名称是否匹配配置
2. 确认文件路径正确（`.github/workflows/`）
3. 验证 YAML 语法是否正确

### Release 工作流程失败

1. 检查标签格式（必须以 `v` 开头）
2. 确认脚本文件具有执行权限
3. 查看 Actions 日志中的错误信息

### 权限问题

1. 确保仓库设置中启用了 Actions
2. 检查 `GITHUB_TOKEN` 权限设置
3. 验证是否有创建 Release 的权限

## 生产环境发布检查清单

在正式发布前，请确认以下事项：

- [ ] 所有代码已提交并推送
- [ ] CI 工作流程在主分支通过
- [ ] 版本号已正确更新
- [ ] CHANGELOG.md 已更新
- [ ] 文档已更新
- [ ] 没有调试代码
- [ ] 配置文件正确
- [ ] 第三方许可证文件已更新

## 监控和维护

### 定期检查

- Actions 使用分钟数
- 工作流程运行历史
- 失败的工作流程
- 依赖更新（actions/checkout 等）

### 优化建议

1. 使用缓存加速构建
2. 并行运行独立任务
3. 只在必要时运行某些步骤
4. 定期清理旧的工作流程运行记录

## 相关资源

- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [actions/checkout](https://github.com/actions/checkout)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [softprops/action-gh-release](https://github.com/softprops/action-gh-release)