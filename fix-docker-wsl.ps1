# Docker Desktop WSL2 修復腳本
# 解決 PostgreSQL 容器在 Windows 上的相容性問題

Write-Host "=== Docker Desktop WSL2 修復工具 ===" -ForegroundColor Cyan
Write-Host ""

# 檢查 WSL2 狀態
Write-Host "1. 檢查 WSL2 狀態..." -ForegroundColor Yellow
wsl --list --verbose
Write-Host ""

# 檢查 Docker Desktop 是否運行
Write-Host "2. 檢查 Docker Desktop 狀態..." -ForegroundColor Yellow
docker version 2>&1 | Select-Object -First 3
Write-Host ""

# 清理所有容器和 volumes
Write-Host "3. 清理舊的 Docker 容器和 volumes..." -ForegroundColor Yellow
Read-Host "按 Enter 繼續清理，或 Ctrl+C 取消"

docker ps -a | Select-Object -Skip 1 | ForEach-Object {
    $container = $_ -split '\s+' | Select-Object -Last 1
    docker rm $container -f
}

docker volume prune -f

Write-Host "清理完成！" -ForegroundColor Green
Write-Host ""

# 修復 WSL2
Write-Host "4. 修復 WSL2 網路..." -ForegroundColor Yellow
Write-Host "執行以下命令重置 WSL 網路："
Write-Host "wsl --shutdown" -ForegroundColor Cyan
Write-Host "然後重新啟動 Docker Desktop" -ForegroundColor Cyan
Write-Host ""

# 建議的 Docker Desktop 設定
Write-Host "5. 建議的 Docker Desktop 設定：" -ForegroundColor Yellow
Write-Host "   - 開啟 Docker Desktop Settings" -ForegroundColor White
Write-Host "   - General: 確認使用 WSL 2 引擎" -ForegroundColor White
Write-Host "   - Resources → Memory: 至少 4GB" -ForegroundColor White
Write-Host "   - Resources → WSL Integration: 啟用" -ForegroundColor White
Write-Host "   - Resources → File sharing: 確認專案目錄已啟用" -ForegroundColor White
Write-Host ""

Write-Host "修復步驟完成！現在可以嘗試：" -ForegroundColor Green
Write-Host "docker-compose -f compose-fix-windows.yaml up -d" -ForegroundColor Cyan
