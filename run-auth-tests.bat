@echo off
echo === Running Authentication Tests ===
echo.

echo Starting API server...
start /B dotnet run --project temple-api/TempleApi.csproj > nul 2>&1

echo Waiting for API to start (10 seconds)...
timeout /t 10 /nobreak > nul

echo.
echo Running backend unit tests...
cd temple-api
dotnet test Tests/TempleApi.Tests.csproj --filter "FullyQualifiedName~AuthenticationTests|FullyQualifiedName~AuthenticationIntegrationTests" --logger "console;verbosity=normal"
cd ..

echo.
echo === Test Summary ===
echo.
echo 1. Backend unit tests have been executed
echo 2. To test frontend authentication:
echo    - Open temple-ui/test-login.html in a browser
echo    - Or navigate to http://localhost:5173 (if frontend is running)
echo.
echo 3. Default test credentials:
echo    Username: admin@temple.com
echo    Password: admin123
echo.

echo Press any key to stop the API server...
pause > nul

echo Stopping API server...
taskkill /F /IM dotnet.exe > nul 2>&1

echo.
echo === Tests Complete ===
