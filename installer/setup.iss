; CTWebPlayer Inno Setup 安装脚本
; =============================================
; 此脚本用于创建 CTWebPlayer 的 Windows 安装程序
; 使用 Inno Setup 6.x 编译

#define AppName "CTWebPlayer"
#define AppVersion "1.0.0"
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
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; 界面设置
WizardStyle=modern
WindowVisible=no
WindowShowCaption=yes
WindowStartMaximized=no
WindowResizable=no
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}

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
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; 创建桌面快捷方式（可选）
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
; 创建快速启动栏快捷方式（可选）
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; 主程序文件
Source: "..\publish\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; 配置文件
Source: "..\config.json"; DestDir: "{app}"; Flags: ignoreversion onlyifdoesntexist

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
Name: "{autoprograms}\{#AppName}\卸载 {#AppName}"; Filename: "{uninstallexe}"; Comment: "卸载 {#AppName}"
Name: "{autoprograms}\{#AppName}\许可证"; Filename: "{app}\LICENSE"; Comment: "查看许可证"

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

; 应用程序注册信息
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"

[Code]
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
    ReadmeContent.Add('Unity3D WebPlayer 专属浏览器');
    ReadmeContent.Add('');
    ReadmeContent.Add('系统要求:');
    ReadmeContent.Add('- Windows 10 或更高版本 (64位)');
    ReadmeContent.Add('- Microsoft Edge WebView2 运行时 (如未安装，程序会提示下载)');
    ReadmeContent.Add('');
    ReadmeContent.Add('使用说明:');
    ReadmeContent.Add('1. 双击运行 ctwebplayer.exe');
    ReadmeContent.Add('2. 程序会自动检查并提示安装 WebView2 运行时（如需要）');
    ReadmeContent.Add('3. 在地址栏输入 Unity WebPlayer 游戏的 URL');
    ReadmeContent.Add('4. 享受游戏！');
    ReadmeContent.Add('');
    ReadmeContent.Add('功能特性:');
    ReadmeContent.Add('- 缓存管理');
    ReadmeContent.Add('- 代理设置支持');
    ReadmeContent.Add('- CORS 处理');
    ReadmeContent.Add('- 详细日志记录');
    ReadmeContent.Add('');
    ReadmeContent.Add('许可证:');
    ReadmeContent.Add('本软件基于 BSD 3-Clause 许可证发布');
    ReadmeContent.Add('详见 LICENSE 文件');
    ReadmeContent.Add('');
    ReadmeContent.Add('第三方组件许可证详见 THIRD_PARTY_LICENSES.txt');
    ReadmeContent.Add('');
    ReadmeContent.Add('项目主页: https://github.com/a11s/ctwebplayer');
    
    ReadmeContent.SaveToFile(ReadmePath);
  finally
    ReadmeContent.Free;
  end;
end;

// 检查 WebView2 是否已安装
function IsWebView2Installed(): Boolean;
var
  ResultCode: Integer;
begin
  Result := RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', '') <> '';
  if not Result then
    Result := RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', '') <> '';
end;

// 初始化安装
procedure InitializeWizard();
begin
  // 可以在这里添加自定义的安装向导页面
end;

// 安装前准备
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  // 检查是否有运行中的程序实例
  if IsAppRunning('{#AppExeName}') then
  begin
    Result := '{#AppName} 正在运行。请关闭程序后重试。';
    Exit;
  end;
  
  Result := '';
end;

// 安装后操作
procedure CurStepChanged(CurStep: TSetupStep);
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
        if MsgBox('未检测到 Microsoft Edge WebView2 运行时。' + #13#10 + 
                  '程序需要此组件才能正常运行。' + #13#10 + #13#10 + 
                  '是否要立即下载并安装？', mbConfirmation, MB_YESNO) = IDYES then
        begin
          // 打开 WebView2 下载页面
          ShellExec('open', 'https://developer.microsoft.com/microsoft-edge/webview2/', '', '', SW_SHOW, ewNoWait, 0);
        end;
      end;
    end;
  end;
end;

// 卸载前准备
function InitializeUninstall(): Boolean;
begin
  // 检查程序是否正在运行
  if IsAppRunning('{#AppExeName}') then
  begin
    MsgBox('{#AppName} 正在运行。请关闭程序后再卸载。', mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;

// 支持静默安装的参数处理
function GetInstallDir(Param: String): String;
begin
  // 允许通过命令行参数指定安装目录
  // 例如: setup.exe /DIR="C:\MyApps\CTWebPlayer"
  Result := ExpandConstant('{autopf}\{#AppName}');
end;

// 检查应用是否正在运行
function IsAppRunning(const FileName: string): Boolean;
var
  FWMIService: Variant;
  FSWbemLocator: Variant;
  FWbemObjectSet: Variant;
begin
  Result := False;
  try
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet := FWMIService.ExecQuery(Format('SELECT Name FROM Win32_Process WHERE Name="%s"', [FileName]));
    Result := (FWbemObjectSet.Count > 0);
  except
  end;
end;