# Clean-UnusedResourceKeys.ps1 使用文档

## 目录
- [脚本概述](#脚本概述)
- [系统要求](#系统要求)
- [安装和配置](#安装和配置)
- [参数说明](#参数说明)
- [使用示例](#使用示例)
- [工作原理](#工作原理)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)
- [注意事项](#注意事项)

## 脚本概述

`Clean-UnusedResourceKeys.ps1` 是一个PowerShell脚本，用于自动清理.NET项目中resx资源文件里不再使用的资源key。该脚本通过扫描项目中的所有源代码文件，智能识别实际使用的资源key，然后与资源文件中定义的key进行比较，找出并可选择性地删除那些未被引用的资源项。

### 主要功能
- 自动扫描Designer.cs文件中的Tag属性引用
- 检测代码文件中的GetString方法调用
- 支持多语言资源文件（简体中文、繁体中文、日语、韩语）
- 提供WhatIf模式进行安全预览
- 支持自动备份原始文件
- 生成详细的清理报告
- 支持排除特定模式的key

## 系统要求

### 前置条件
- **PowerShell**: 5.0 或更高版本
- **操作系统**: Windows 7/10/11 或 Windows Server 2012+
- **.NET项目**: 包含resx资源文件的C#项目
- **文件权限**: 对项目目录的读写权限

### 检查PowerShell版本
```powershell
$PSVersionTable.PSVersion
```

## 安装和配置

### 1. 下载脚本
将脚本文件放置在项目的 `scripts` 目录下：
```
your-project/
├── scripts/
│   └── Clean-UnusedResourceKeys.ps1
├── src/
│   ├── Resources/
│   │   ├── Strings.resx
│   │   ├── Strings.zh-CN.resx
│   │   └── ...
│   └── ...
```

### 2. 设置执行策略
如果遇到执行策略限制，请以管理员身份运行PowerShell并执行：
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### 3. 验证脚本路径
确保脚本可以访问到项目文件：
```powershell
cd your-project\scripts
Test-Path ..\src\Resources\Strings.resx
```

## 参数说明

### -WhatIf
- **类型**: Switch参数
- **默认值**: False
- **说明**: 启用预览模式，仅显示将要删除的内容，不实际执行删除操作
- **用途**: 在实际删除前安全地查看哪些key会被删除

### -Backup
- **类型**: Switch参数
- **默认值**: False
- **说明**: 在删除key之前自动备份原始resx文件
- **备份位置**: 在Resources目录下创建backup子目录，文件名包含时间戳

### -ExcludeKeys
- **类型**: String数组
- **默认值**: 空数组
- **说明**: 指定不应被删除的key模式，支持通配符(*)
- **示例**: @("Message_*", "Common_*", "System_*")

### -ProjectPath
- **类型**: String
- **默认值**: 脚本所在目录的父目录
- **说明**: 项目根目录路径，用于指定要扫描的项目位置

## 使用示例

### 示例1：预览模式（推荐首次使用）
```powershell
.\Clean-UnusedResourceKeys.ps1 -WhatIf
```
此命令会扫描所有文件并显示将要删除的key，但不会实际删除任何内容。

### 示例2：执行清理并备份
```powershell
.\Clean-UnusedResourceKeys.ps1 -Backup
```
执行实际的清理操作，并在删除前备份所有将被修改的resx文件。

### 示例3：排除特定模式的key
```powershell
.\Clean-UnusedResourceKeys.ps1 -ExcludeKeys "Message_*", "Dialog_*" -Backup
```
删除未使用的key，但保留所有以"Message_"或"Dialog_"开头的key。

### 示例4：指定项目路径
```powershell
.\Clean-UnusedResourceKeys.ps1 -ProjectPath "C:\Projects\MyApp" -WhatIf
```
对指定路径的项目执行扫描。

### 示例5：完整的安全清理流程
```powershell
# 步骤1：先预览
.\Clean-UnusedResourceKeys.ps1 -WhatIf

# 步骤2：查看生成的报告
notepad .\unused-resource-keys-report.txt

# 步骤3：如果确认无误，执行清理
.\Clean-UnusedResourceKeys.ps1 -Backup -ExcludeKeys "Common_*"
```

## 工作原理

### 1. 扫描阶段
脚本执行以下扫描操作：

#### Designer.cs文件扫描
- 查找所有 `*.Designer.cs` 文件
- 使用正则表达式匹配 `.Tag = "ResourceKey"` 模式
- 收集所有Tag属性中引用的资源key

#### 代码文件扫描
- 查找所有 `*.cs` 文件（排除Designer.cs和生成的文件）
- 检测以下GetString调用模式：
  - `GetString("key")`
  - `GetString('key')`
  - `GetString(@"key")`
  - 字符串插值中的GetString调用

### 2. 分析阶段
- 读取所有resx文件中定义的key
- 将已使用的key与定义的key进行比较
- 应用排除规则过滤
- 生成未使用key的列表

### 3. 报告生成
创建详细报告文件 `unused-resource-keys-report.txt`，包含：
- 扫描统计信息
- 未使用key的完整列表
- 各语言文件的分布情况

### 4. 清理阶段（可选）
- 请求用户确认
- 备份原始文件（如果启用）
- 从resx文件中删除未使用的节点
- 保持XML格式和编码不变

## 最佳实践

### 1. 建立清理流程
```powershell
# 定期清理流程
# 每月执行一次
$date = Get-Date -Format "yyyy-MM"
.\Clean-UnusedResourceKeys.ps1 -WhatIf | Tee-Object "cleanup-preview-$date.log"
```

### 2. 版本控制集成
在清理前后使用Git查看变更：
```bash
# 清理前创建分支
git checkout -b cleanup/remove-unused-keys

# 执行清理
powershell .\scripts\Clean-UnusedResourceKeys.ps1 -Backup

# 查看变更
git diff

# 提交变更
git add -A
git commit -m "chore: 清理未使用的资源key"
```

### 3. 团队协作建议
- 在团队中统一执行清理操作的时机
- 保留业务相关的key模式（使用ExcludeKeys参数）
- 清理后进行完整的功能测试

### 4. 排除规则设置
创建标准的排除规则集：
```powershell
$standardExcludes = @(
    "Common_*",      # 通用资源
    "System_*",      # 系统消息
    "Error_*",       # 错误信息
    "Validation_*"   # 验证消息
)

.\Clean-UnusedResourceKeys.ps1 -ExcludeKeys $standardExcludes -Backup
```

## 常见问题

### Q1: 脚本报告"找到0个已使用的key"
**解决方案**：
1. 检查项目路径是否正确
2. 确认资源文件的引用方式是否符合扫描模式
3. 查看是否有自定义的资源访问方法

### Q2: 执行时提示"未经数字签名"
**解决方案**：
```powershell
# 方法1：解除阻止
Unblock-File -Path .\Clean-UnusedResourceKeys.ps1

# 方法2：临时绕过
powershell -ExecutionPolicy Bypass -File .\Clean-UnusedResourceKeys.ps1
```

### Q3: 备份文件占用太多空间
**解决方案**：
定期清理旧备份：
```powershell
# 删除30天前的备份
Get-ChildItem .\src\Resources\backup\*.resx | 
    Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-30) } | 
    Remove-Item
```

### Q4: 某些动态生成的key被误删
**解决方案**：
1. 使用ExcludeKeys参数排除动态key的模式
2. 在代码中添加注释标记动态key
3. 考虑修改代码使用静态引用

### Q5: 脚本运行速度慢
**优化建议**：
1. 减少扫描范围（指定更具体的ProjectPath）
2. 排除不必要的目录（如node_modules、bin、obj）
3. 关闭实时杀毒软件扫描

## 注意事项

### ⚠️ 重要警告

1. **数据安全**
   - 始终在版本控制系统中执行清理操作
   - 首次使用必须启用 -WhatIf 参数
   - 生产环境清理前务必备份

2. **特殊情况**
   - 动态生成的资源key可能被误判为未使用
   - 反射调用的资源可能无法被检测到
   - 第三方库引用的资源需要手动排除

3. **性能考虑**
   - 大型项目可能需要较长扫描时间
   - 建议在非高峰时段执行
   - 可以考虑分模块逐步清理

4. **兼容性问题**
   - 确保所有resx文件使用UTF-8编码
   - 检查是否有自定义的资源管理器
   - 注意不同.NET版本的资源访问差异

### 🔒 安全建议

1. **权限控制**
   - 限制脚本执行权限给特定用户
   - 避免在共享环境中存储备份

2. **审计追踪**
   - 保留所有清理报告
   - 在版本控制中记录清理操作

3. **测试验证**
   - 清理后执行完整的单元测试
   - 验证多语言功能是否正常

### 📋 清理检查清单

在执行清理前，请确认：
- [ ] 已提交所有待处理的代码变更
- [ ] 已创建新的Git分支
- [ ] 已运行WhatIf模式查看影响
- [ ] 已检查动态key的使用情况
- [ ] 已设置适当的排除规则
- [ ] 已准备好回滚方案

---

**版本信息**: v1.0  
**最后更新**: 2025-09-15  
**维护者**: 开发团队  
**问题反馈**: 请提交到项目的Issue追踪系统