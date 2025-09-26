# PowerShell script to set up testuser in the database

$dbPath = "..\Database\temple_management_dev.db"
$sqlScript = "create-testuser.sql"

Write-Host "=== Setting up Test User ===" -ForegroundColor Cyan

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "✗ Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "  Please ensure the API has been run at least once to create the database." -ForegroundColor Yellow
    exit 1
}

# Check if sqlite3 is available
try {
    $sqliteVersion = sqlite3 --version 2>$null
    Write-Host "✓ SQLite found: $sqliteVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ SQLite not found. Installing via npm..." -ForegroundColor Yellow
    npm install -g sqlite3-cli
}

# Run the SQL script
Write-Host "`nExecuting SQL script to create/update testuser..." -ForegroundColor Yellow
try {
    $output = sqlite3 $dbPath ".read $sqlScript" 2>&1
    Write-Host $output
    Write-Host "✓ Script executed successfully!" -ForegroundColor Green
} catch {
    Write-Host "✗ Error executing script: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Test User Credentials ===" -ForegroundColor Cyan
Write-Host "Username/Email: testuser@example.com" -ForegroundColor Yellow
Write-Host "Password: password123" -ForegroundColor Yellow

Write-Host "`n=== How to Test ===" -ForegroundColor Cyan
Write-Host "1. Start the API: cd .. && dotnet run" -ForegroundColor Gray
Write-Host "2. Use the login endpoint with Basic Auth" -ForegroundColor Gray
Write-Host "3. Or use the test HTML page: temple-ui/test-login.html" -ForegroundColor Gray
