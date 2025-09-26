@echo off
echo ========================================
echo Running All Tests for Temple Management
echo ========================================
echo.

echo [1/3] Building API...
cd temple-api
dotnet build
if %errorlevel% neq 0 (
    echo API build failed!
    exit /b %errorlevel%
)

echo.
echo [2/3] Running API Tests...
dotnet test --logger "console;verbosity=detailed"
if %errorlevel% neq 0 (
    echo API tests failed!
    exit /b %errorlevel%
)

echo.
echo [3/3] Running UI Tests...
cd ..\temple-ui
npm test
if %errorlevel% neq 0 (
    echo UI tests failed!
    exit /b %errorlevel%
)

echo.
echo ========================================
echo All Tests Passed Successfully!
echo ========================================

cd ..
