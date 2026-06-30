@echo off
echo Checking Docker Desktop configuration...
echo.

echo 1. Docker version:
docker version
echo.

echo 2. Docker info:
docker info
echo.

echo 3. Checking WSL2 integration:
wsl --list --verbose
echo.

echo 4. Testing PostgreSQL compatibility:
echo    Pulling PostgreSQL test image...
docker pull postgres:16-alpine
echo.
echo    Running PostgreSQL test container...
docker run --rm --name test-postgres -e POSTGRES_PASSWORD=test postgres:16-alpine
if %ERRORLEVEL% EQU 0 (
    echo    ✓ PostgreSQL runs successfully
) else (
    echo    ✗ PostgreSQL failed to start
    echo.
    echo    Possible solutions:
    echo    1. Update Docker Desktop to latest version
    echo    2. Enable WSL2 backend in Docker Desktop settings
    echo    3. Increase memory allocated to Docker Desktop (Settings → Resources)
    echo    4. Restart Docker Desktop
)
echo.

echo 5. Cleaning up test containers:
docker ps -a | findstr test-postgres && docker rm test-postgres

echo.
echo Check complete. If PostgreSQL test failed, try the solutions above.
