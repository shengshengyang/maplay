@echo off
echo === Windows Docker PostgreSQL 修復版啟動 ===
echo.

echo 第一步：清理舊容器和資料...
docker-compose -f compose.yaml down 2>nul
docker volume prune -f

echo.
echo 第二步：啟動修復版本（PostgreSQL 14 + 相容性修復）...
docker-compose -f compose-fix-windows.yaml up -d

echo.
echo 檢查服務狀態...
timeout /t 10 /nobreak >nul
docker-compose -f compose-fix-windows.yaml ps

echo.
echo === 啟動完成 ===
echo.
echo 如果 PostgreSQL 還是失敗，請執行修復腳本：
echo .\fix-docker-wsl.ps1
echo.
echo 然後檢查 Docker Desktop 設定：
echo - Settings → General：WSL 2 引擎
echo - Settings → Resources → Memory：至少 4GB
echo - Settings → Resources → WSL Integration：啟用
echo.
