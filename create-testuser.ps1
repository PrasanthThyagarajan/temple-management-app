# PowerShell script to create testuser in the database

Write-Host "=== Creating Test User ===" -ForegroundColor Cyan

$apiUrl = "http://localhost:5051"

# First, check if the API is running
Write-Host "`nChecking if API is running..." -ForegroundColor Yellow
try {
    $health = Invoke-WebRequest -Uri "$apiUrl/health" -Method GET -ErrorAction Stop
    Write-Host "✓ API is running" -ForegroundColor Green
} catch {
    Write-Host "✗ API is not running. Please start it first with: cd temple-api && dotnet run" -ForegroundColor Red
    exit 1
}

# Register the test user
Write-Host "`nRegistering testuser..." -ForegroundColor Yellow
$registerData = @{
    email = "testuser@example.com"
    password = "password123"
    fullName = "Test User"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$apiUrl/api/auth/register" -Method POST `
        -ContentType "application/json" -Body $registerData
    
    if ($response.success) {
        Write-Host "✓ User registered successfully!" -ForegroundColor Green
        Write-Host "  Note: User is inactive until email verification" -ForegroundColor Yellow
        
        # Since we're in development, let's activate the user directly
        Write-Host "`nActivating user (skipping email verification for testing)..." -ForegroundColor Yellow
        
        # For development, we'll need to update the user directly in the database
        # This would normally be done through email verification
        Write-Host "  To activate the user manually:" -ForegroundColor Cyan
        Write-Host "  1. Run SQLite browser or use the database viewer" -ForegroundColor Gray
        Write-Host "  2. Update the Users table:" -ForegroundColor Gray
        Write-Host "     SET IsActive = 1, IsVerified = 1" -ForegroundColor Gray
        Write-Host "     WHERE Email = 'testuser@example.com'" -ForegroundColor Gray
        
    } else {
        Write-Host "✗ Registration failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorBody -and $errorBody.message) {
        Write-Host "✗ Registration failed: $($errorBody.message)" -ForegroundColor Red
    } else {
        Write-Host "✗ Registration error: $_" -ForegroundColor Red
    }
}

Write-Host "`n=== Alternative: Use Existing Admin User ===" -ForegroundColor Cyan
Write-Host "You can login with the default admin user:" -ForegroundColor Green
Write-Host "  Username: admin@temple.com" -ForegroundColor Yellow
Write-Host "  Password: admin123" -ForegroundColor Yellow

Write-Host "`n=== To Test Login ===" -ForegroundColor Cyan
Write-Host "After activating the user, test with:" -ForegroundColor Green
Write-Host '  $auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("testuser@example.com:password123"))' -ForegroundColor Gray
Write-Host '  Invoke-RestMethod -Uri "http://localhost:5051/api/auth/login" -Method POST -Headers @{"Authorization"="Basic $auth"}' -ForegroundColor Gray
