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
  // 可以在这里添加自定义的安装向导页面
end;

// 安装前准备
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  // 检查是否有运行中的程序实例
  if IsAppRunning('{#AppExeName}') then
  begin
    Result := '{#AppName} is running. Please close the program and try again.';
    Exit;
  end;
  
  Result := '';
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
begin
  // 检查程序是否正在运行
  if IsAppRunning('{#AppExeName}') then
  begin
    MsgBox('{#AppName} is running. Please close the program before uninstalling.', mbError, MB_OK);
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