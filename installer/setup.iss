; CTWebPlayer Inno Setup 安装脚本
; =============================================
; 此脚本用于创建 CTWebPlayer 的 Windows 安装程序
; 使用 Inno Setup 6.x 编译

#define AppName "CTWebPlayer"
#define AppVersion "1.2.0"
#define AppPublisher "CTWebPlayer Team"
#define AppURL "https://github.com/a11s/ctwebplayer"
#define AppExeName "ctwebplayer.exe"
#define AppCopyright "Copyright © 2025 CTWebPlayer Team. All rights reserved."
#define AppDescription "Unity3D WebPlayer 专属浏览器"
#define AppId "{{B8F9E4C2-7A5D-4E3B-9C1F-8A2D6E5B3F7C}"

[Setup]
; 应用程序基本信息
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
AppCopyright={#AppCopyright}
AppComments={#AppDescription}

; 安装目录设置
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=no
AllowNoIcons=yes

; 输出设置
OutputDir=..\release
OutputBaseFilename={#AppName}-v{#AppVersion}-Setup
SetupIconFile=..\res\c001_01_Icon_Texture.ico
Compression=lzma2/max
SolidCompression=yes
CompressionThreads=auto

; 系统要求
MinVersion=10.0
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

; 权限设置
; 始终要求管理员权限，以确保能终止进程和写入系统目录
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
; 显示 UAC 提示时的权限提升信息
InfoBeforeFile=
InfoAfterFile=

; 界面设置
WizardStyle=modern
WindowVisible=no
WindowShowCaption=yes
WindowStartMaximized=no
WindowResizable=no
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}

; 语言设置
ShowLanguageDialog=auto
LanguageDetectionMethod=uilanguage

; 静默安装支持
; 使用 /SILENT 或 /VERYSILENT 参数

; 版本信息
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppDescription}
VersionInfoTextVersion={#AppVersion}
VersionInfoCopyright={#AppCopyright}
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}

[Languages]
; 英语（默认）
Name: "english"; MessagesFile: "compiler:Default.isl"
; 简体中文
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
; 繁体中文
Name: "chinesetraditional"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
; 日语
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
; 朝鲜语（韩语）
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
; 创建桌面快捷方式（可选）
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
; 创建快速启动栏快捷方式（可选）
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; 主程序文件
Source: "..\publish\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; 配置文件
; 注释掉 config.json 的安装，因为包含测试信息，不应该打包分发
; Source: "..\config.json"; DestDir: "{app}"; Flags: ignoreversion onlyifdoesntexist

; 资源文件
Source: "..\res\*"; DestDir: "{app}\res"; Flags: ignoreversion recursesubdirs createallsubdirs

; 许可证文件
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\THIRD_PARTY_LICENSES.txt"; DestDir: "{app}"; Flags: ignoreversion

; README 文件（安装时生成）
; 将在 [Code] 部分创建

; WebView2 运行时（如果需要单独包含）
; Source: "MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
; 开始菜单快捷方式
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Comment: "{#AppDescription}"
Name: "{autoprograms}\{#AppName}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Comment: "{#AppDescription}"
Name: "{autoprograms}\{#AppName}\Uninstall {#AppName}"; Filename: "{uninstallexe}"; Comment: "Uninstall {#AppName}"
Name: "{autoprograms}\{#AppName}\License"; Filename: "{app}\LICENSE"; Comment: "View License"

; 桌面快捷方式（如果选择）
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon; Comment: "{#AppDescription}"

; 快速启动栏快捷方式（如果选择）
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon; Comment: "{#AppDescription}"

[Run]
; 安装后检查并安装 WebView2 运行时（如果需要）
; Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "正在安装 Microsoft Edge WebView2 运行时..."; Flags: skipifsilent

; 安装后运行程序（可选）
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; 卸载前的清理操作（如果需要）

[UninstallDelete]
; 卸载时删除额外的文件和目录
Type: filesandordirs; Name: "{app}\cache"
Type: filesandordirs; Name: "{app}\logs"
Type: files; Name: "{app}\config.json"
Type: dirifempty; Name: "{app}"

[Registry]
; 文件关联（如果需要）
; 例如：关联特定的 URL 协议或文件类型
; Root: HKLM; Subkey: "Software\Classes\ctwebplayer"; ValueType: string; ValueName: ""; ValueData: "CTWebPlayer Protocol"; Flags: uninsdeletekey
; Root: HKLM; Subkey: "Software\Classes\ctwebplayer"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Flags: uninsdeletekey
; Root: HKLM; Subkey: "Software\Classes\ctwebplayer\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName},0"
; Root: HKLM; Subkey: "Software\Classes\ctwebplayer\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""

; 应用程序注册信息 - 改为写入当前用户注册表，避免权限问题
; 原本写入 HKLM 需要管理员权限，但程序本身不使用这些注册表项，所以改为写入 HKCU 更安全
Root: HKCU; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"

; 如果确实需要在 HKLM 中记录安装信息，可以添加错误处理
; Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey uninsdeletekeyifempty dontcreatekey
; Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"; Flags: dontcreatekey

[Code]
// 检查是否以管理员权限运行
function IsAdminLoggedOn(): Boolean;
begin
  Result := IsAdminInstallMode();
end;

// 使用 tasklist 命令检查进程（备用方案）
// 声明在前，供其他函数调用
function CheckProcessByTasklist(const FileName: string): Boolean;
var
  ResultCode: Integer;
  TempFile: string;
  FileContent: TStringList;
begin
  Result := False;
  TempFile := ExpandConstant('{tmp}\process_check.txt');
  
  if Exec(ExpandConstant('{cmd}'), '/C tasklist /FI "IMAGENAME eq ' + FileName + '" > "' + TempFile + '"',
          '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if FileExists(TempFile) then
    begin
      FileContent := TStringList.Create;
      try
        FileContent.LoadFromFile(TempFile);
        Result := Pos(FileName, FileContent.Text) > 0;
      finally
        FileContent.Free;
        DeleteFile(TempFile);
      end;
    end;
  end;
end;

// 检查应用是否正在运行（排除安装程序自身）
function IsAppRunning(const FileName: string): Boolean;
var
  FWMIService: Variant;
  FSWbemLocator: Variant;
  FWbemObjectSet: Variant;
  FWbemObject: Variant;
  ProcessPath: String;
  InstallerPath: String;
  I: Integer;
  ActualCount: Integer;
begin
  Result := False;
  ActualCount := 0;
  InstallerPath := UpperCase(ExpandConstant('{srcexe}'));
  
  try
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet := FWMIService.ExecQuery(Format('SELECT Name, ExecutablePath FROM Win32_Process WHERE Name="%s"', [FileName]));
    
    // 遍历所有匹配的进程
    for I := 0 to FWbemObjectSet.Count - 1 do
    begin
      try
        FWbemObject := FWbemObjectSet.ItemIndex(I);
        ProcessPath := '';
        
        // 尝试获取进程路径
        try
          ProcessPath := FWbemObject.ExecutablePath;
        except
          // 如果获取路径失败，仍然计数这个进程
          ProcessPath := '';
        end;
        
        // 排除路径相同的进程（安装程序自身）
        // 如果路径为空（可能是系统进程），则计入
        // 如果路径不同于安装程序路径，则计入
        if (ProcessPath = '') or (UpperCase(ProcessPath) <> InstallerPath) then
        begin
          Inc(ActualCount);
        end;
      except
        // 忽略单个进程的错误
      end;
    end;
    
    Result := (ActualCount > 0);
  except
    // 如果 WMI 失败，尝试使用 tasklist 命令（但无法排除安装程序）
    Result := CheckProcessByTasklist(FileName);
    
    // 如果检测到进程且安装程序名称与目标程序相同，显示警告
    if Result and (CompareText(ExtractFileName(ExpandConstant('{srcexe}')), FileName) = 0) then
    begin
      Result := False; // 假设是误报
      MsgBox('注意：检测到可能的命名冲突。' + #13#10 +
             '如果程序确实在运行，请手动关闭后继续。',
             mbInformation, MB_OK);
    end;
  end;
end;

// 终止指定的进程（使用 WMI）
function KillProcessByWMI(const FileName: string): Boolean;
var
  FWMIService: Variant;
  FSWbemLocator: Variant;
  FWbemObjectSet: Variant;
  I: Integer;
begin
  Result := False;
  try
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet := FWMIService.ExecQuery(Format('SELECT * FROM Win32_Process WHERE Name="%s"', [FileName]));
    
    // 简化的方式遍历进程
    for I := 0 to FWbemObjectSet.Count - 1 do
    begin
      try
        FWbemObjectSet.ItemIndex(I).Terminate();
        Result := True;
      except
        // 忽略单个进程终止失败
      end;
    end;
  except
    // WMI 失败时返回 False
  end;
end;

// 使用 taskkill 命令终止进程（备用方案）
function KillProcessByTaskKill(const FileName: string): Boolean;
var
  ResultCode: Integer;
begin
  // 使用 /F (强制) 和 /IM (镜像名称) 参数
  Result := Exec('taskkill', '/F /IM "' + FileName + '" /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := Result and (ResultCode = 0);
end;

// 智能终止进程（先尝试 WMI，失败则使用 taskkill）
function KillProcess(const FileName: string): Boolean;
begin
  Result := KillProcessByWMI(FileName);
  
  // 如果 WMI 方法失败，尝试使用 taskkill
  if not Result then
  begin
    Result := KillProcessByTaskKill(FileName);
  end;
  
  // 等待进程完全结束
  if Result then
    Sleep(500);
end;

// 获取进程数量
function GetProcessCount(const FileName: string): Integer;
var
  FWMIService: Variant;
  FSWbemLocator: Variant;
  FWbemObjectSet: Variant;
begin
  Result := 0;
  try
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet := FWMIService.ExecQuery(Format('SELECT Name FROM Win32_Process WHERE Name="%s"', [FileName]));
    Result := FWbemObjectSet.Count;
  except
    // 发生错误时返回 0
  end;
end;

// 创建 README.txt 文件
procedure CreateReadmeFile();
var
  ReadmeContent: TStringList;
  ReadmePath: string;
begin
  ReadmePath := ExpandConstant('{app}\README.txt');
  ReadmeContent := TStringList.Create;
  try
    ReadmeContent.Add('CTWebPlayer v' + '{#AppVersion}');
    ReadmeContent.Add('===================');
    ReadmeContent.Add('');
    ReadmeContent.Add('Unity3D WebPlayer Browser');
    ReadmeContent.Add('');
    ReadmeContent.Add('System Requirements:');
    ReadmeContent.Add('- Windows 10 or later (64-bit)');
    ReadmeContent.Add('- Microsoft Edge WebView2 Runtime (will prompt to download if not installed)');
    ReadmeContent.Add('');
    ReadmeContent.Add('How to Use:');
    ReadmeContent.Add('1. Double-click ctwebplayer.exe to run');
    ReadmeContent.Add('2. The program will check and prompt to install WebView2 Runtime if needed');
    ReadmeContent.Add('3. Enter the Unity WebPlayer game URL in the address bar');
    ReadmeContent.Add('4. Enjoy the game!');
    ReadmeContent.Add('');
    ReadmeContent.Add('Features:');
    ReadmeContent.Add('- Cache management');
    ReadmeContent.Add('- Proxy settings support');
    ReadmeContent.Add('- CORS handling');
    ReadmeContent.Add('- Detailed logging');
    ReadmeContent.Add('');
    ReadmeContent.Add('License:');
    ReadmeContent.Add('This software is released under the BSD 3-Clause License');
    ReadmeContent.Add('See LICENSE file for details');
    ReadmeContent.Add('');
    ReadmeContent.Add('Third-party component licenses can be found in THIRD_PARTY_LICENSES.txt');
    ReadmeContent.Add('');
    ReadmeContent.Add('Project Homepage: https://github.com/a11s/ctwebplayer');
    
    ReadmeContent.SaveToFile(ReadmePath);
  finally
    ReadmeContent.Free;
  end;
end;

// 检查 WebView2 是否已安装
function IsWebView2Installed(): Boolean;
var
  ResultCode: Integer;
  Version: String;
begin
  Result := RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
  if not Result then
    Result := RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
end;

// 初始化安装
procedure InitializeWizard();
begin
  // 检查管理员权限
  if not IsAdminLoggedOn() then
  begin
    MsgBox('警告：安装程序未以管理员权限运行。' + #13#10 +
           '某些功能（如自动关闭运行中的程序）可能无法正常工作。' + #13#10 +
           '建议右键点击安装程序并选择"以管理员身份运行"。',
           mbInformation, MB_OK);
  end;
end;

// 初始化安装时的权限检查
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
  InstallerPath: String;
  InstallerName: String;
  DebugMsg: String;
begin
  Result := True;
  
  // 获取安装程序信息用于调试
  InstallerPath := ExpandConstant('{srcexe}');
  InstallerName := ExtractFileName(InstallerPath);
  
  // 调试：记录安装程序信息
  DebugMsg := '安装程序调试信息：' + #13#10 +
              '• 安装程序路径: ' + InstallerPath + #13#10 +
              '• 安装程序名称: ' + InstallerName + #13#10 +
              '• 目标程序名称: {#AppExeName}' + #13#10;
              
  // 检查是否存在命名冲突
  if CompareText(InstallerName, '{#AppExeName}') = 0 then
  begin
    MsgBox('错误：安装程序名称与目标程序名称相同！' + #13#10 + #13#10 +
           DebugMsg + #13#10 +
           '请将安装程序重命名后再运行（例如：setup.exe）。',
           mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  // 检查安装程序是否在目标目录运行
  if Pos(UpperCase(ExpandConstant('{autopf}\{#AppName}')), UpperCase(InstallerPath)) > 0 then
  begin
    MsgBox('错误：安装程序正在目标安装目录中运行！' + #13#10 + #13#10 +
           DebugMsg + #13#10 +
           '请将安装程序移动到其他位置（如桌面）后再运行。',
           mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  // 如果不是管理员权限，提示用户
  if not IsAdminLoggedOn() then
  begin
    if MsgBox('安装程序需要管理员权限才能：' + #13#10 +
              '• 自动关闭运行中的程序' + #13#10 +
              '• 安装到系统目录' + #13#10 +
              '• 创建卸载信息' + #13#10 + #13#10 +
              '是否以管理员权限重新启动安装程序？',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      // 修复：使用 ewWaitUntilTerminated 确保原进程终止
      if ShellExec('runas', InstallerPath, '', '', SW_SHOW, ewWaitUntilTerminated, ErrorCode) then
      begin
        // 成功启动提升权限的进程
        Result := False; // 终止当前安装
      end
      else
      begin
        MsgBox('无法以管理员权限启动安装程序。' + #13#10 +
               '错误代码: ' + IntToStr(ErrorCode) + #13#10 + #13#10 +
               '请右键点击安装程序，选择"以管理员身份运行"。',
               mbError, MB_OK);
        Result := False;
      end;
    end;
  end;
end;

// 安装前准备
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  RetryCount: Integer;
  ProcessCount: Integer;
  UserChoice: Integer;
begin
  Result := '';
  RetryCount := 0;
  
  // 检查是否有运行中的程序实例
  while IsAppRunning('{#AppExeName}') do
  begin
    ProcessCount := GetProcessCount('{#AppExeName}');
    
    if RetryCount = 0 then
    begin
      // 第一次检测到进程时，询问用户
      UserChoice := MsgBox(
        Format('{#AppName} 正在运行 (%d 个进程)。', [ProcessCount]) + #13#10 +
        '安装程序需要关闭它才能继续。' + #13#10 + #13#10 +
        '点击"是"自动关闭程序' + #13#10 +
        '点击"否"手动关闭程序' + #13#10 +
        '点击"取消"终止安装',
        mbConfirmation, MB_YESNOCANCEL);
        
      case UserChoice of
        IDYES:
          begin
            // 用户选择自动关闭
            if not KillProcess('{#AppExeName}') then
            begin
              // 如果标准方法失败，检查权限并给出更详细的提示
              if not IsAdminLoggedOn() then
              begin
                Result := '需要管理员权限才能自动关闭程序。' + #13#10 +
                         '请以管理员身份运行安装程序，或手动关闭程序。';
                Exit;
              end
              else if MsgBox('无法自动关闭程序。' + #13#10 +
                            '请尝试手动关闭程序后继续。',
                            mbConfirmation, MB_OKCANCEL) = IDCANCEL then
              begin
                Result := '安装已取消。';
                Exit;
              end;
            end;
          end;
        IDNO:
          begin
            // 用户选择手动关闭
            MsgBox('请手动关闭 {#AppName}，然后点击"确定"继续安装。', mbInformation, MB_OK);
          end;
        IDCANCEL:
          begin
            Result := '用户取消了安装。';
            Exit;
          end;
      end;
    end
    else
    begin
      // 重试时自动尝试关闭
      KillProcess('{#AppExeName}');
    end;
    
    Inc(RetryCount);
    
    // 等待一段时间让进程完全结束
    Sleep(1000);
    
    // 如果尝试次数过多，退出
    if RetryCount > 5 then
    begin
      Result := '无法关闭 {#AppName}。' + #13#10 +
                '请确保您有足够的权限，或手动结束进程后重试。';
      Exit;
    end;
    
    // 再次检查进程是否还在运行
    if not IsAppRunning('{#AppExeName}') then
      Break;
  end;
end;

// 安装后操作
procedure CurStepChanged(CurStep: TSetupStep);
var
  ErrorCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    // 创建 README.txt 文件
    CreateReadmeFile();
    
    // 检查 WebView2 是否已安装
    if not IsWebView2Installed() then
    begin
      if not WizardSilent() then
      begin
        if MsgBox('Microsoft Edge WebView2 Runtime not detected.' + #13#10 +
                  'This component is required for the program to run properly.' + #13#10 + #13#10 +
                  'Would you like to download and install it now?', mbConfirmation, MB_YESNO) = IDYES then
        begin
          // 打开 WebView2 下载页面
          ShellExec('open', 'https://developer.microsoft.com/microsoft-edge/webview2/', '', '', SW_SHOW, ewNoWait, ErrorCode);
        end;
      end;
    end;
  end;
end;

// 卸载前准备
function InitializeUninstall(): Boolean;
var
  RetryCount: Integer;
  UserChoice: Integer;
begin
  Result := True;
  RetryCount := 0;
  
  // 检查程序是否正在运行
  while IsAppRunning('{#AppExeName}') do
  begin
    if RetryCount = 0 then
    begin
      UserChoice := MsgBox(
        '{#AppName} 正在运行。' + #13#10 +
        '卸载程序需要关闭它才能继续。' + #13#10 + #13#10 +
        '是否自动关闭程序？',
        mbConfirmation, MB_YESNO);
        
      if UserChoice = IDNO then
      begin
        MsgBox('请手动关闭 {#AppName} 后再试。', mbError, MB_OK);
        Result := False;
        Exit;
      end;
    end;
    
    // 尝试终止进程
    if not KillProcess('{#AppExeName}') then
    begin
      // 如果失败，可能是权限问题
      if MsgBox('无法自动关闭程序（可能需要管理员权限）。' + #13#10 +
               '请手动关闭程序后点击"重试"，或点击"取消"终止卸载。',
               mbError, MB_RETRYCANCEL) = IDCANCEL then
      begin
        Result := False;
        Exit;
      end;
    end;
    
    Inc(RetryCount);
    Sleep(1000);
    
    if RetryCount > 5 then
    begin
      MsgBox('无法关闭 {#AppName}。请手动结束进程后再卸载。', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;
end;

// 支持静默安装的参数处理
function GetInstallDir(Param: String): String;
begin
  // 允许通过命令行参数指定安装目录
  // 例如: setup.exe /DIR="C:\MyApps\CTWebPlayer"
  Result := ExpandConstant('{autopf}\{#AppName}');
end;