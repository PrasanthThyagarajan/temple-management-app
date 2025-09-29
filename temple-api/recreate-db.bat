@echo off
echo Stopping API if running...
taskkill /F /IM dotnet.exe 2>nul

echo Deleting old database files...
del /Q Database\temple_management_dev.db* 2>nul

echo Database files deleted. Please restart the API to recreate the database with new schema.
echo The API will automatically create ContributionSettings and Contributions tables.
pause
