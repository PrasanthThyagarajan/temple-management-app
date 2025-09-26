# Temple Management Database Viewer (PowerShell)
Write-Host "🗄️  Temple Management Database Viewer" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

$dbPath = Join-Path $PSScriptRoot "..\Database\temple_management_dev.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Database file not found: $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Database found: $dbPath" -ForegroundColor Green
Write-Host ""

# Check if sqlite3 is available
try {
    $sqliteVersion = sqlite3 -version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "📊 Database Tables:" -ForegroundColor Yellow
        Write-Host "==================" -ForegroundColor Yellow
        
        # List tables
        $tables = sqlite3 $dbPath ".tables"
        Write-Host $tables
        
        Write-Host ""
        Write-Host "💰 Sample Expense Data:" -ForegroundColor Yellow
        Write-Host "======================" -ForegroundColor Yellow
        
        # Show sample expense data
        $expenseQuery = @"
SELECT 
    'ID: ' || Id || 
    ' | Type: ' || CASE WHEN EventExpenseId IS NOT NULL THEN 'Item' ELSE 'Service' END ||
    ' | Event: ' || EventId ||
    ' | Price: ₹' || COALESCE(Price, 0) ||
    ' | Created: ' || CreatedAt as ExpenseInfo
FROM Expenses 
ORDER BY CreatedAt DESC 
LIMIT 10;
"@
        
        $expenses = sqlite3 $dbPath $expenseQuery
        if ($expenses) {
            $expenses | ForEach-Object { Write-Host $_ }
        } else {
            Write-Host "   No expenses found"
        }
        
        Write-Host ""
        Write-Host "📦 Expense Items:" -ForegroundColor Yellow
        Write-Host "=================" -ForegroundColor Yellow
        
        $itemsQuery = "SELECT 'ID: ' || Id || ' | Name: ' || Name || ' | Desc: ' || COALESCE(Description, 'N/A') FROM EventExpenses LIMIT 5;"
        $items = sqlite3 $dbPath $itemsQuery
        if ($items) {
            $items | ForEach-Object { Write-Host $_ }
        } else {
            Write-Host "   No expense items found"
        }
        
    } else {
        Write-Host "⚠️  SQLite CLI not found. Please install SQLite or use the Cursor extension method." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "💡 Alternative: Install SQLite Viewer extension in Cursor:" -ForegroundColor Cyan
        Write-Host "   1. Press Ctrl+Shift+X" -ForegroundColor White
        Write-Host "   2. Search for 'SQLite Viewer'" -ForegroundColor White
        Write-Host "   3. Install the extension" -ForegroundColor White
        Write-Host "   4. Right-click on the .db file and select 'Open Database'" -ForegroundColor White
    }
} catch {
    Write-Host "⚠️  SQLite CLI not available. Use Cursor extension instead." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✨ Database exploration complete!" -ForegroundColor Green
