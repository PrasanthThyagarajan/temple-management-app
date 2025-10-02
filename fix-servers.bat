@echo off
echo Fixing Temple Management App Servers...
echo.

echo Step 1: Killing existing dotnet processes...
taskkill /F /IM dotnet.exe >nul 2>&1
if %errorlevel% == 0 (
    echo ✅ Dotnet processes killed successfully
) else (
    echo ℹ️ No dotnet processes found to kill
)

echo.
echo Step 2: Waiting for ports to be released...
timeout /t 3 /nobreak >nul

echo.
echo Step 3: Checking if ports are free...
netstat -ano | findstr :5051 >nul
if %errorlevel% == 0 (
    echo ❌ Port 5051 is still in use
    echo Please manually kill the process using Task Manager
    pause
    exit /b 1
) else (
    echo ✅ Port 5051 is free
)

echo.
echo Step 4: Starting API server...
cd temple-api
start "Temple API" cmd /k "dotnet run --urls http://localhost:5051"

echo.
echo Step 5: Waiting for API server to start...
timeout /t 5 /nobreak >nul

echo.
echo Step 6: Checking API server status...
curl -s -o nul -w "%%{http_code}" http://localhost:5051/api/health >nul 2>&1
if %errorlevel% == 0 (
    echo ✅ API server is running on http://localhost:5051
) else (
    echo ℹ️ API server is starting up (this is normal)
)

echo.
echo Step 7: Frontend server status...
netstat -ano | findstr :5175 >nul
if %errorlevel% == 0 (
    echo ✅ Frontend server is running on http://localhost:5175
) else (
    echo ❌ Frontend server is not running
    echo Starting frontend server...
    cd ..\temple-ui
    start "Temple UI" cmd /k "npm run dev"
)

echo.
echo 🎉 Setup complete!
echo.
echo 📋 Server URLs:
echo   • Frontend: http://localhost:5175
echo   • API:      http://localhost:5051
echo.
echo 🔍 To test the Inventory page:
echo   1. Open http://localhost:5175 in your browser
echo   2. Login with your credentials
echo   3. Navigate to Administration → Inventory
echo.
pause
