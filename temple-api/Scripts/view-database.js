const sqlite3 = require('sqlite3').verbose();
const path = require('path');

// Database path
const dbPath = path.join(__dirname, '../Database/temple_management_dev.db');

console.log('🗄️  Temple Management Database Viewer');
console.log('=====================================');
console.log(`Database: ${dbPath}\n`);

// Open database
const db = new sqlite3.Database(dbPath, sqlite3.OPEN_READONLY, (err) => {
    if (err) {
        console.error('❌ Error opening database:', err.message);
        return;
    }
    console.log('✅ Connected to SQLite database\n');
    
    // List all tables
    db.all("SELECT name FROM sqlite_master WHERE type='table'", [], (err, tables) => {
        if (err) {
            console.error('❌ Error fetching tables:', err.message);
            return;
        }
        
        console.log('📋 Available Tables:');
        console.log('===================');
        tables.forEach((table, index) => {
            console.log(`${index + 1}. ${table.name}`);
        });
        
        console.log('\n🔍 Sample Data from Key Tables:');
        console.log('===============================\n');
        
        // Show sample data from important tables
        const importantTables = ['Expenses', 'EventExpenses', 'Events', 'Devotees'];
        let completed = 0;
        
        importantTables.forEach(tableName => {
            // Check if table exists
            const tableExists = tables.some(t => t.name === tableName);
            if (!tableExists) {
                console.log(`⚠️  Table '${tableName}' not found`);
                completed++;
                if (completed === importantTables.length) {
                    db.close();
                }
                return;
            }
            
            db.all(`SELECT * FROM ${tableName} LIMIT 5`, [], (err, rows) => {
                if (err) {
                    console.error(`❌ Error fetching data from ${tableName}:`, err.message);
                } else {
                    console.log(`📊 ${tableName} (showing first 5 records):`);
                    console.log('-'.repeat(50));
                    if (rows.length === 0) {
                        console.log('   No data found');
                    } else {
                        console.table(rows);
                    }
                    console.log('');
                }
                
                completed++;
                if (completed === importantTables.length) {
                    console.log('✨ Database exploration complete!');
                    db.close((err) => {
                        if (err) {
                            console.error('❌ Error closing database:', err.message);
                        } else {
                            console.log('🔒 Database connection closed');
                        }
                    });
                }
            });
        });
    });
});
