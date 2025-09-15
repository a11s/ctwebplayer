# PowerShell script to update missing translations

# UpdateManager translations for zh-CN
$zhCNUpdates = @{
    "UpdateManager_DownloadingFile" = "正在下载：{0}"
    "UpdateManager_ComputedHash" = "计算的哈希值：{0}"
    "UpdateManager_NoHashSkipVerification" = "未提供哈希值，跳过验证。"
    "UpdateManager_LatestVersionInfo" = "最新版本：{0}"
    "UpdateManager_CurrentVersion" = "您正在使用最新版本。"
    "UpdateManager_ApplyFailed" = "应用更新失败：{0}"
    "UpdateManager_BatchStarted" = "更新批处理文件已启动。"
    "UpdateManager_CheckingUpdate" = "正在检查更新..."
    "UpdateManager_ApplyingUpdate" = "正在应用更新..."
    "UpdateManager_DownloadCancelled" = "下载已取消。"
    "UpdateManager_HashCalculationFailed" = "哈希计算失败：{0}"
    "UpdateManager_DownloadFailed" = "下载失败：{0}"
    "UpdateManager_VerificationPassed" = "验证通过。"
    "UpdateManager_NewVersionFound" = "发现新版本：{0}（当前：{1}）"
    "UpdateManager_ExpectedHash" = "预期的哈希值：{0}"
    "UpdateManager_CheckFailed" = "更新检查失败：{0}"
    "UpdateManager_DownloadCompleteVerification" = "下载完成，正在验证..."
}

# UpdateManager translations for zh-TW
$zhTWUpdates = @{
    "SettingsForm_Msg_Applied" = "設定已套用。"
    "SettingsForm_Msg_ProxyRequired" = "請至少提供一個代理伺服器位址。"
    "Form1_statusStrip1_statusLabel" = "就緒"
    "UpdateManager_DownloadingFile" = "正在下載：{0}"
    "UpdateManager_ComputedHash" = "計算的雜湊值：{0}"
    "UpdateManager_NoHashSkipVerification" = "未提供雜湊值，略過驗證。"
    "UpdateManager_LatestVersionInfo" = "最新版本：{0}"
    "UpdateManager_CurrentVersion" = "您正在使用最新版本。"
    "UpdateManager_ApplyFailed" = "套用更新失敗：{0}"
    "UpdateManager_BatchStarted" = "更新批次檔案已啟動。"
    "UpdateManager_CheckingUpdate" = "正在檢查更新..."
    "UpdateManager_ApplyingUpdate" = "正在套用更新..."
    "UpdateManager_DownloadCancelled" = "下載已取消。"
    "UpdateManager_HashCalculationFailed" = "雜湊計算失敗：{0}"
    "UpdateManager_DownloadFailed" = "下載失敗：{0}"
    "UpdateManager_VerificationPassed" = "驗證通過。"
    "UpdateManager_NewVersionFound" = "發現新版本：{0}（目前：{1}）"
    "UpdateManager_ExpectedHash" = "預期的雜湊值：{0}"
    "UpdateManager_CheckFailed" = "更新檢查失敗：{0}"
    "UpdateManager_DownloadCompleteVerification" = "下載完成，正在驗證..."
}

# UpdateManager translations for ja
$jaUpdates = @{
    "UpdateManager_DownloadingFile" = "ダウンロード中：{0}"
    "UpdateManager_ComputedHash" = "計算されたハッシュ：{0}"
    "UpdateManager_NoHashSkipVerification" = "ハッシュが提供されていないため、検証をスキップします。"
    "UpdateManager_LatestVersionInfo" = "最新バージョン：{0}"
    "UpdateManager_CurrentVersion" = "最新バージョンを使用しています。"
    "UpdateManager_ApplyFailed" = "更新の適用に失敗しました：{0}"
    "UpdateManager_BatchStarted" = "更新バッチファイルが開始されました。"
    "UpdateManager_CheckingUpdate" = "更新を確認中..."
    "UpdateManager_ApplyingUpdate" = "更新を適用中..."
    "UpdateManager_DownloadCancelled" = "ダウンロードがキャンセルされました。"
    "UpdateManager_HashCalculationFailed" = "ハッシュ計算に失敗しました：{0}"
    "UpdateManager_DownloadFailed" = "ダウンロードに失敗しました：{0}"
    "UpdateManager_VerificationPassed" = "検証に合格しました。"
    "UpdateManager_NewVersionFound" = "新しいバージョンが見つかりました：{0}（現在：{1}）"
    "UpdateManager_ExpectedHash" = "期待されるハッシュ：{0}"
    "UpdateManager_CheckFailed" = "更新チェックに失敗しました：{0}"
    "UpdateManager_DownloadCompleteVerification" = "ダウンロード完了、検証中..."
}

# UpdateManager translations for ko
$koUpdates = @{
    "UpdateManager_DownloadingFile" = "다운로드 중: {0}"
    "UpdateManager_ComputedHash" = "계산된 해시: {0}"
    "UpdateManager_NoHashSkipVerification" = "해시가 제공되지 않아 확인을 건너뜁니다."
    "UpdateManager_LatestVersionInfo" = "최신 버전: {0}"
    "UpdateManager_CurrentVersion" = "최신 버전을 사용하고 있습니다."
    "UpdateManager_ApplyFailed" = "업데이트 적용 실패: {0}"
    "UpdateManager_BatchStarted" = "업데이트 배치 파일이 시작되었습니다."
    "UpdateManager_CheckingUpdate" = "업데이트 확인 중..."
    "UpdateManager_ApplyingUpdate" = "업데이트 적용 중..."
    "UpdateManager_DownloadCancelled" = "다운로드가 취소되었습니다."
    "UpdateManager_HashCalculationFailed" = "해시 계산 실패: {0}"
    "UpdateManager_DownloadFailed" = "다운로드 실패: {0}"
    "UpdateManager_VerificationPassed" = "확인이 통과되었습니다."
    "UpdateManager_NewVersionFound" = "새 버전 발견: {0} (현재: {1})"
    "UpdateManager_ExpectedHash" = "예상 해시: {0}"
    "UpdateManager_CheckFailed" = "업데이트 확인 실패: {0}"
    "UpdateManager_DownloadCompleteVerification" = "다운로드 완료, 확인 중..."
}

Write-Host "Translation update script created."
Write-Host "Please manually update the resource files with these translations."