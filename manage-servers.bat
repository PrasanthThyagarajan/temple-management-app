@echo off
echo Temple Management App - Server Manager
echo =====================================

:menu
echo.
echo 1. Start API Server (port 5051)
echo 2. Start Frontend Server (port 5173)
echo 3. Stop API Server
echo 4. Stop Frontend Server
echo 5. Check Server Status
echo 6. Exit
echo.
set /p choice="Enter your choice (1-6): "

if "%choice%"=="1" goto start_api
if "%choice%"=="2" goto start_frontend
if "%choice%"=="3" goto stop_api
if "%choice%"=="4" goto stop_frontend
if "%choice%"=="5" goto check_status
if "%choice%"=="6" goto exit
goto menu

:start_api
echo Starting API Server...
cd temple-api
start "Temple API" dotnet run
cd ..
echo API Server started in new window
goto menu

:start_frontend
echo Starting Frontend Server...
cd temple-ui
start "Temple UI" npm run dev
cd ..
echo Frontend Server started in new window
goto menu

:stop_api
echo Stopping API Server (port 5051)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5051') do taskkill /PID %%a /F >nul 2>&1
echo API Server stopped
goto menu

:stop_frontend
echo Stopping Frontend Server (port 5173)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5173') do taskkill /PID %%a /F >nul 2>&1
echo Frontend Server stopped
goto menu

:check_status
echo Checking Server Status...
echo.
echo API Server (port 5051):
netstat -ano | findstr :5051
if errorlevel 1 echo   Not running
echo.
echo Frontend Server (port 5173):
netstat -ano | findstr :5173
if errorlevel 1 echo   Not running
goto menu

:exit
echo Goodbye!
pause
