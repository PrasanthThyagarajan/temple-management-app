@echo off
echo ========================================
echo Running Inventory Unit Tests Only
echo ========================================
echo.

echo [1/2] Building API...
cd temple-api
dotnet build
if %errorlevel% neq 0 (
    echo API build failed!
    exit /b %errorlevel%
)

echo.
echo [2/2] Running Inventory Unit Tests...
dotnet test --filter "InventoryServiceTests|InventoryEntityTests|InventoryDbContextTests" --logger "console;verbosity=normal"
if %errorlevel% neq 0 (
    echo Inventory tests failed!
    exit /b %errorlevel%
)

echo.
echo ========================================
echo Inventory Unit Tests Passed Successfully!
echo ========================================

cd ..
