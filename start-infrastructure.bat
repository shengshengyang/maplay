@echo off
REM 啟動基礎設施服務
echo Starting infrastructure services (PostgreSQL + MinIO)...
docker-compose -f compose-infrastructure.yaml up -d

echo.
echo Infrastructure services started. You can now start the application with:
echo docker-compose -f compose-app.yaml up -d --build
