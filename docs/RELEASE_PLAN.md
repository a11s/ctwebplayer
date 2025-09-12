# CTWebPlayer 发布版本完整计划

## 概述
这是一个为C# WinForms项目[ctwebplayer](https://github.com/a11s/ctwebplayer)制定的发布计划，基于BSD3协议。项目是一个Unity3D WebPlayer专属浏览器，支持缓存、代理、日志和CORS处理，使用.NET 8.0和WebView2。

计划覆盖发布前准备、自动更新功能、打包发布流程、GitHub Release配置，以及用户可能忽略的重要事项。使用Semantic Versioning (SemVer)，初始版本1.0.0。

**BSD3协议合规**：允许修改和分发，但修改版必须改名、声明与原作者无关、不暗示认可。第三方组件（如WebView2、Flurl）MIT许可，需包含THIRD_PARTY_LICENSES.txt。

## 可执行任务清单
以下是详细步骤，按逻辑顺序执行。每个步骤包括技术实现细节。

- [x] 分析当前项目状态并制定整体发布策略，包括BSD3协议合规要求
  - 项目：.NET 8 WinForms，依赖WebView2、Flurl、PlainHttp。无当前版本管理。构建为单exe。协议BSD3需注意分发声明。

- [ ] 更新版本号管理：在AssemblyInfo.cs或项目属性中添加版本控制，使用Semantic Versioning (SemVer)，如1.0.0
  - 在ctwebplayer.csproj添加：
    ```
    <PropertyGroup>
      <Version>1.0.0</Version>
      <AssemblyVersion>1.0.0.0</AssemblyVersion>
      <FileVersion>1.0.0.0</FileVersion>
    </PropertyGroup>
    ```
  - 使用git tag v1.0.0标记发布。工具：dotnet --version。

- [ ] 优化代码：移除调试模式代码（如debugMode配置），优化缓存和日志性能，添加Release构建配置
  - 在Form1.cs移除debugMode相关（如LogManager.Debug日志）。优化CacheManager：使用异步I/O，限制缓存大小（e.g., 1GB）。
  - 在ConfigManager移除debugMode字段。添加条件编译：#if DEBUG。

- [ ] 构建配置准备：修改ctwebplayer.csproj添加Release模式，配置dotnet publish参数（如-self-contained -r win-x64）
  - csproj更新：
    ```
    <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
      <DebugType>none</DebugType>
      <DebugSymbols>false</DebugSymbols>
      <Optimize>true</Optimize>
    </PropertyGroup>
    ```
  - Publish命令：`dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true`。

- [ ] 测试构建：在本地运行dotnet build -c Release和dotnet publish，验证exe独立运行，无依赖缺失
  - 运行`dotnet build -c Release`，检查输出。测试exe在干净Windows 10 VM中运行，确保WebView2提示正确。

- [ ] 更新文档：修改README.md添加发布说明、安装指南、更新日志模板
  - README添加部分：下载链接、安装（需WebView2）、更新说明。创建CHANGELOG.md模板：
    ```
    ## [1.0.0] - YYYY-MM-DD
    ### Added
    - ...
    ### Changed
    - ...
    ```

- [ ] 设计自动更新功能：在Form1.cs中集成GitHub API (使用Flurl) 检查最新Release，比较版本号，下载新exe并重启应用
  - 方案：启动时调用GitHub API `https://api.github.com/repos/a11s/ctwebplayer/releases/latest`。解析tag_name与当前版本比较（Assembly.GetExecutingAssembly().GetName().Version）。
  - 如果新版，显示对话框确认下载。使用Flurl.Http：`await "https://github.com/...".GetStringAsync()`。

- [ ] 实现自动更新：添加UpdateManager类，处理下载（使用HttpClient），验证文件完整性（SHA256），用户确认后替换当前exe
  - 新类UpdateManager.cs：
    ```csharp
    using System.Security.Cryptography;
    public class UpdateManager {
        public async Task<bool> CheckAndUpdateAsync() {
            // GitHub API调用
            var latest = await GetLatestRelease();
            if (latest.Tag > CurrentVersion) {
                var asset = latest.Assets.First(a => a.Name.EndsWith(".exe"));
                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(asset.Url);
                // SHA256验证（从Release notes获取hash）
                if (VerifySHA256(bytes, expectedHash)) {
                    File.WriteAllBytes("update.exe", bytes);
                    Process.Start("update.exe", "/replace"); // 自更新脚本
                    Application.Exit();
                }
            }
            return false;
        }
    }
    ```
  - 添加自更新bat：重命名当前exe为old.exe，替换为新exe，重启。

- [ ] 测试自动更新：在测试环境中模拟GitHub Release，验证下载、安装和重启流程
  - 使用本地GitHub mock或ngrok模拟API。测试版本比较、下载失败回滚。

- [ ] 打包流程：使用dotnet publish创建单文件exe，集成安装程序工具如Inno Setup创建.msi或.exe安装包
  - 工具：Inno Setup (免费)。脚本setup.iss：
    ```
    [Setup]
    AppName=CTWebPlayer
    AppVersion=1.0.0
    [Files]
    Source: "publish\ctwebplayer.exe"; DestDir: "{app}"
    Source: "config.json"; DestDir: "{app}"
    Source: "res\*"; DestDir: "{app}\res"
    [Icons]
    Name: "{autoprograms}\CTWebPlayer"; Filename: "{app}\ctwebplayer.exe"
    ```
  - 运行ISCC setup.iss生成setup.exe。

- [ ] 配置打包脚本：创建build.bat或GitHub Actions workflow，使用dotnet publish打包，包含config.json和res文件夹
  - build.bat：
    ```
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
    copy config.json publish\
    xcopy res publish\res /E /I
    ```
  - 对于安装：集成Inno Setup命令。

- [ ] GitHub Release配置：设置GitHub仓库Release模板，包含CHANGELOG.md，自动化使用Actions (yaml文件) 在tag push时创建Release并上传assets
  - 创建.github/release-template.md：标题、描述、assets列表。

- [ ] 实现GitHub Actions自动化：在.github/workflows/release.yml中定义workflow，触发于tag，运行publish并上传到Release
  - yaml：
    ```yaml
    name: Release
    on:
      push:
        tags: ['v*']
    jobs:
      build:
        runs-on: windows-latest
        steps:
        - uses: actions/checkout@v4
        - uses: actions/setup-dotnet@v4
          with: { dotnet-version: '8.0.x' }
        - run: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
        - uses: actions/upload-artifact@v4
          with: { name: release, path: publish/*.exe }
        - uses: softprops/action-gh-release@v1
          with:
            files: publish/*.exe
            generate_release_notes: true
    ```
  - 上传setup.exe作为asset。

- [ ] 测试GitHub Release：在测试分支创建tag，验证Actions运行和assets上传
  - git tag v0.1.0-test; git push origin v0.1.0-test。检查Actions日志。

- [ ] 处理数字签名：获取EV代码签名证书，使用signtool.exe签名exe和安装包，集成到打包脚本
  - 证书：从DigiCert或Sectigo获取（年费~300 USD）。命令：`signtool sign /f cert.pfx /p password /t http://timestamp.digicert.com publish.exe`。
  - 脚本中添加：if exist signtool.exe ...。

- [ ] 创建安装程序：使用Inno Setup脚本（setup.iss）打包exe、图标、许可文件，支持静默安装和卸载
  - iss扩展：[Run] 安装WebView2如果缺失。[UninstallDelete] 删除cache/logs。

- [ ] 生成更新日志：维护CHANGELOG.md，按SemVer格式记录变更，Release时自动包含
  - 使用keep a changelog格式。Actions中自动从commit生成notes。

- [ ] 许可和合规检查：确保THIRD_PARTY_LICENSES.txt更新，BSD3要求在修改分发时改名和声明无关
  - 更新THIRD_PARTY：添加所有NuGet包许可。README强调BSD3规则。

- [ ] 安全和用户注意事项：添加病毒扫描（上传VirusTotal），处理WebView2运行时依赖，文档中说明Windows 10+要求
  - 扫描：上传exe到VirusTotal API。文档： "要求Windows 10+，自动检查WebView2安装"。

- [ ] 最终验证：完整端到端测试，包括打包、Release、更新、安装
  - 在VM中：安装、运行、更新、卸载。检查日志无错误。

- [ ] 文档化完整计划：汇总所有步骤到docs/RELEASE_PLAN.md，包括Mermaid流程图
  - 本文件。

## 技术方案：自动更新功能
1. **检查更新**：Form1_Load中调用UpdateManager.CheckForUpdatesAsync()。使用Flurl：`var json = await "https://api.github.com/repos/a11s/ctwebplayer/releases/latest".GetStringAsync();` 解析JObject获取tag_name和assets。
2. **版本比较**：当前版本从AssemblyInformationalVersion获取。新版：tag_name如"v1.1.0" > 当前。
3. **下载**：HttpClient下载exe asset（需GitHub token for private? No, public）。保存为temp.exe。
4. **验证**：Release中包含SHA256 hash，计算文件hash比较。
5. **安装**：用户确认后，创建bat脚本：taskkill /f ctwebplayer.exe; move old.exe backup; copy new.exe ctwebplayer.exe; start ctwebplayer.exe。
6. **重启**：Process.Start(newPath); Application.Exit()。
7. **错误处理**：回滚、日志、网络失败重试。

**安全**：仅从GitHub下载，验证hash，禁用不安全更新。

## 打包和发布流程 Mermaid 图
```mermaid
flowchart TD
    A[开发完成] --> B[更新版本号 & CHANGELOG]
    B --> C[git tag vX.Y.Z & push]
    C --> D[GitHub Actions 触发]
    D --> E[dotnet publish - Release - self-contained]
    E --> F[签名 exe (signtool)]
    F --> G[Inno Setup 打包安装程序]
    G --> H[上传 assets 到 Release: exe, setup.exe, checksums]
    H --> I[生成 Release notes]
    I --> J[发布完成: GitHub Release 可用]
    J --> K[用户下载 & 安装]
    K --> L[应用启动, 检查更新]
    L --> M{新版?}
    M -->|是| N[下载 & 验证 & 更新]
    M -->|否| O[正常运行]
    N --> O
```

## 用户可能忽略的重要事项
- **数字签名**：无签名Windows SmartScreen警告。需EV证书，年费高，考虑免费自签名测试。
- **安装程序**：Inno Setup免费，支持许可协议（LICENSE.txt），添加桌面快捷方式。处理WebView2安装：使用Evergreen Bootstrapper。
- **更新日志**：CHANGELOG.md必备，用户信任来源。
- **WebView2依赖**：安装程序检查/安装Runtime（silent: msedgewebview2.exe /silent /install）。
- **病毒扫描**：发布前上传VirusTotal，确保0/70检测。
- **分发合规**：BSD3要求改名（如MyPlayer），AboutForm添加"基于CTWebPlayer修改，与原作者无关"。
- **测试环境**：Windows 10/11 x64，网络/离线场景，代理兼容。
- **备份**：Release前branch保护主分支。

此计划可执行，如需调整请反馈。