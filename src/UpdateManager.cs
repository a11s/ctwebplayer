using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl.Http;

namespace ctwebplayer
{
    /// <summary>
    /// 更新管理器，负责检查、下载和应用程序更新
    /// </summary>
    public class UpdateManager
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/a11s/ctwebplayer/releases/latest";
        private const string USER_AGENT = "CTWebPlayer-UpdateChecker/1.0";
        private const string UPDATE_TEMP_FILE = "ctwebplayer_update.exe";
        private const string UPDATE_BATCH_FILE = "update.bat";
        
        private readonly LogManager _logger;
        private CTWebPlayer.UpdateInfo? _latestUpdate;
        private CancellationTokenSource? _downloadCancellation;

        /// <summary>
        /// 下载进度变化事件
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;

        /// <summary>
        /// 更新检查完成事件
        /// </summary>
        public event EventHandler<UpdateCheckCompletedEventArgs>? UpdateCheckCompleted;

        public UpdateManager()
        {
            _logger = LogManager.Instance;
        }

        /// <summary>
        /// 异步检查更新
        /// </summary>
        /// <param name="showNoUpdateMessage">如果没有更新是否显示消息</param>
        /// <returns>更新信息，如果没有更新则返回 null</returns>
        public async Task<CTWebPlayer.UpdateInfo?> CheckForUpdatesAsync(bool showNoUpdateMessage = true)
        {
            try
            {
                _logger.Info(LanguageManager.Instance.GetString("UpdateManager_CheckingUpdate"));

                // 配置 Flurl 请求
                var response = await GITHUB_API_URL
                    .WithHeader("User-Agent", USER_AGENT)
                    .WithHeader("Accept", "application/vnd.github.v3+json")
                    .GetStringAsync();

                dynamic? json = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                if (json == null)
                {
                    throw new Exception("无法解析 GitHub API 响应");
                }
                _latestUpdate = CTWebPlayer.UpdateInfo.FromGitHubRelease(json);

                _logger.Info(string.Format(LanguageManager.Instance.GetString("UpdateManager_LatestVersionInfo"), _latestUpdate.Version));

                // 检查是否需要更新
                if (_latestUpdate.IsUpdateRequired())
                {
                    _logger.Info(string.Format(LanguageManager.Instance.GetString("UpdateManager_NewVersionFound"), _latestUpdate.Version, CTWebPlayer.Version.FullVersion));
                    UpdateCheckCompleted?.Invoke(this, new UpdateCheckCompletedEventArgs
                    {
                        UpdateAvailable = true,
                        UpdateInfo = _latestUpdate
                    });
                    return _latestUpdate;
                }
                else
                {
                    _logger.Info(LanguageManager.Instance.GetString("UpdateManager_CurrentVersion"));
                    if (showNoUpdateMessage)
                    {
                        MessageBox.Show(
                            $"当前已是最新版本 ({CTWebPlayer.Version.FullVersion})",
                            "检查更新",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    UpdateCheckCompleted?.Invoke(this, new UpdateCheckCompletedEventArgs
                    {
                        UpdateAvailable = false,
                        UpdateInfo = null
                    });
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(LanguageManager.Instance.GetString("UpdateManager_CheckFailed"), ex.Message), ex);
                
                if (showNoUpdateMessage)
                {
                    MessageBox.Show(
                        $"检查更新失败: {ex.Message}",
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                
                UpdateCheckCompleted?.Invoke(this, new UpdateCheckCompletedEventArgs
                {
                    UpdateAvailable = false,
                    UpdateInfo = null,
                    Error = ex
                });
                
                return null;
            }
        }

        /// <summary>
        /// 异步下载更新
        /// </summary>
        /// <param name="updateInfo">更新信息</param>
        /// <returns>下载的文件路径，如果失败则返回 null</returns>
        public async Task<string?> DownloadUpdateAsync(CTWebPlayer.UpdateInfo updateInfo)
        {
            if (updateInfo == null || string.IsNullOrEmpty(updateInfo.DownloadUrl))
            {
                throw new ArgumentException("无效的更新信息");
            }

            _downloadCancellation = new CancellationTokenSource();
            string tempFile = Path.Combine(Path.GetTempPath(), UPDATE_TEMP_FILE);

            try
            {
                _logger.Info(string.Format(LanguageManager.Instance.GetString("UpdateManager_DownloadingFile"), updateInfo.DownloadUrl));

                // 删除旧的临时文件
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                // 下载文件
                var response = await updateInfo.DownloadUrl
                    .WithHeader("User-Agent", USER_AGENT)
                    .GetAsync(cancellationToken: _downloadCancellation.Token);

                // 获取文件大小
                var totalBytes = response.ResponseMessage.Content.Headers.ContentLength ?? updateInfo.FileSize;

                // 创建文件流
                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var stream = await response.ResponseMessage.Content.ReadAsStreamAsync();
                    var buffer = new byte[8192];
                    long totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _downloadCancellation.Token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, _downloadCancellation.Token);
                        totalBytesRead += bytesRead;

                        // 报告进度
                        var progress = totalBytes > 0 ? (int)((totalBytesRead * 100) / totalBytes) : 0;
                        DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs
                        {
                            BytesReceived = totalBytesRead,
                            TotalBytesToReceive = totalBytes,
                            ProgressPercentage = progress
                        });
                    }
                }

                _logger.Info(LanguageManager.Instance.GetString("UpdateManager_DownloadCompleteVerification"));

                // 验证文件完整性
                if (!string.IsNullOrEmpty(updateInfo.SHA256Hash))
                {
                    if (!await VerifyFileHashAsync(tempFile, updateInfo.SHA256Hash))
                    {
                        throw new Exception("文件完整性验证失败");
                    }
                    _logger.Info(LanguageManager.Instance.GetString("UpdateManager_VerificationPassed"));
                }
                else
                {
                    _logger.Warning(LanguageManager.Instance.GetString("UpdateManager_NoHashSkipVerification"));
                }

                return tempFile;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(LanguageManager.Instance.GetString("UpdateManager_DownloadFailed"), ex.Message), ex);
                
                // 清理临时文件
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
                
                throw;
            }
        }

        /// <summary>
        /// 取消下载
        /// </summary>
        public void CancelDownload()
        {
            _downloadCancellation?.Cancel();
            _logger.Info(LanguageManager.Instance.GetString("UpdateManager_DownloadCancelled"));
        }

        /// <summary>
        /// 验证文件的 SHA256 哈希值
        /// </summary>
        private async Task<bool> VerifyFileHashAsync(string filePath, string expectedHash)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var sha256 = SHA256.Create())
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = sha256.ComputeHash(stream);
                        var hashString = BitConverter.ToString(hash).Replace("-", "").ToUpper();
                        
                        _logger.Debug(string.Format(LanguageManager.Instance.GetString("UpdateManager_ComputedHash"), hashString));
                        _logger.Debug(string.Format(LanguageManager.Instance.GetString("UpdateManager_ExpectedHash"), expectedHash));
                        
                        return hashString.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format(LanguageManager.Instance.GetString("UpdateManager_HashCalculationFailed"), ex.Message), ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// 应用更新（重启程序）
        /// </summary>
        /// <param name="updateFilePath">更新文件路径</param>
        public void ApplyUpdate(string updateFilePath)
        {
            if (!File.Exists(updateFilePath))
            {
                throw new FileNotFoundException("更新文件不存在", updateFilePath);
            }

            try
            {
                _logger.Info(LanguageManager.Instance.GetString("UpdateManager_ApplyingUpdate"));

                // 获取当前程序路径
                string currentExePath = Application.ExecutablePath;
                string? currentDir = Path.GetDirectoryName(currentExePath);
                if (string.IsNullOrEmpty(currentDir))
                {
                    throw new InvalidOperationException("无法获取当前程序目录");
                }
                string currentExeName = Path.GetFileName(currentExePath);

                // 创建更新批处理脚本
                string batchContent = $@"@echo off
echo 正在更新 CTWebPlayer...
echo.

REM 等待主程序退出
timeout /t 2 /nobreak > nul

REM 备份当前程序
if exist ""{currentExeName}.bak"" del ""{currentExeName}.bak""
move ""{currentExeName}"" ""{currentExeName}.bak""

REM 复制新文件
copy /Y ""{updateFilePath}"" ""{currentExeName}""

REM 启动新程序
start """" ""{currentExeName}""

REM 清理临时文件
timeout /t 5 /nobreak > nul
if exist ""{updateFilePath}"" del ""{updateFilePath}""
if exist ""{UPDATE_BATCH_FILE}"" del ""{UPDATE_BATCH_FILE}""

exit
";

                string batchPath = Path.Combine(currentDir, UPDATE_BATCH_FILE);
                File.WriteAllText(batchPath, batchContent);

                // 启动批处理文件
                var processInfo = new ProcessStartInfo
                {
                    FileName = batchPath,
                    WorkingDirectory = currentDir,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Process.Start(processInfo);

                _logger.Info(LanguageManager.Instance.GetString("UpdateManager_BatchStarted"));

                // 退出当前程序
                Application.Exit();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(LanguageManager.Instance.GetString("UpdateManager_ApplyFailed"), ex.Message), ex);
                throw;
            }
        }

        /// <summary>
        /// 获取最后检查的更新信息
        /// </summary>
        public CTWebPlayer.UpdateInfo? GetLatestUpdateInfo()
        {
            return _latestUpdate;
        }
    }

    /// <summary>
    /// 下载进度事件参数
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public long BytesReceived { get; set; }
        public long TotalBytesToReceive { get; set; }
        public int ProgressPercentage { get; set; }
    }

    /// <summary>
    /// 更新检查完成事件参数
    /// </summary>
    public class UpdateCheckCompletedEventArgs : EventArgs
    {
        public bool UpdateAvailable { get; set; }
        public CTWebPlayer.UpdateInfo? UpdateInfo { get; set; }
        public Exception? Error { get; set; }
    }
}