@echo off
REM CTWebPlayer 安装程序测试脚本启动器
REM 确保 PowerShell 脚本以正确的编码运行

echo =======================================
echo CTWebPlayer Installer Test Runner
echo =======================================
echo.

REM 设置代码页为 GBK (936) 以支持中文
chcp 936 >nul 2>&1

REM 检查 PowerShell 是否可用
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Error: PowerShell not found in system PATH.
    echo Please ensure PowerShell is installed.
    pause
    exit /b 1
)

REM 检查测试脚本是否存在
if not exist "test-installer.ps1" (
    echo Error: test-installer.ps1 not found.
    echo Please run this script from the installer directory.
    pause
    exit /b 1
)

echo Starting test script...
echo.

REM 运行 PowerShell 脚本，设置执行策略为 Bypass，并使用 UTF-8 编码
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "test-installer.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Test script exited with error code: %ERRORLEVEL%
    pause
)

exit /b %ERRORLEVEL%