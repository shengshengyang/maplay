@echo off
REM 啟動應用程式服務
echo Starting Maplay application...
echo Make sure infrastructure services are running first!
echo.
docker-compose -f compose-app.yaml up -d --build

echo.
echo Application started. Check logs with:
echo docker-compose -f compose-app.yaml logs -f maplay
