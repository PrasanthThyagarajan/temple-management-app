const sqlite3 = require('sqlite3').verbose();
const path = require('path');

// Database path
const dbPath = path.join(__dirname, '../Database/temple_management_dev.db');

console.log('💰 Temple Management - Expense Data Viewer');
console.log('==========================================');
console.log(`Database: ${dbPath}\n`);

// Open database
const db = new sqlite3.Database(dbPath, sqlite3.OPEN_READONLY, (err) => {
    if (err) {
        console.error('❌ Error opening database:', err.message);
        return;
    }
    console.log('✅ Connected to SQLite database\n');
    
    // Query expenses with related data
    const expenseQuery = `
        SELECT 
            e.Id,
            e.EventExpenseId,
            e.ExpenseServiceId,
            e.EventId,
            e.Price,
            e.IsApprovalNeed,
            e.CreatedAt,
            ei.Name as ItemName,
            es.Name as ServiceName
        FROM Expenses e
        LEFT JOIN EventExpenses ei ON e.EventExpenseId = ei.Id
        LEFT JOIN ExpenseServices es ON e.ExpenseServiceId = es.Id
        ORDER BY e.CreatedAt DESC
        LIMIT 20
    `;
    
    db.all(expenseQuery, [], (err, expenses) => {
        if (err) {
            console.error('❌ Error fetching expenses:', err.message);
            return;
        }
        
        console.log('💰 Recent Expenses (Last 20):');
        console.log('=============================');
        
        if (expenses.length === 0) {
            console.log('   No expenses found in database');
        } else {
            expenses.forEach((expense, index) => {
                console.log(`${index + 1}. Expense ID: ${expense.Id}`);
                console.log(`   Type: ${expense.EventExpenseId ? 'Item' : 'Service'}`);
                console.log(`   Name: ${expense.ItemName || expense.ServiceName || 'N/A'}`);
                console.log(`   Event ID: ${expense.EventId}`);
                console.log(`   Price: ₹${expense.Price || 0}`);
                console.log(`   Approval Needed: ${expense.IsApprovalNeed ? 'Yes' : 'No'}`);
                console.log(`   Created: ${expense.CreatedAt}`);
                console.log('   ' + '-'.repeat(40));
            });
        }
        
        // Get expense items
        db.all('SELECT * FROM EventExpenses ORDER BY CreatedAt DESC LIMIT 10', [], (err, items) => {
            if (err) {
                console.error('❌ Error fetching expense items:', err.message);
            } else {
                console.log('\n📦 Expense Items:');
                console.log('================');
                if (items.length === 0) {
                    console.log('   No expense items found');
                } else {
                    console.table(items);
                }
            }
            
            // Get expense services
            db.all('SELECT * FROM ExpenseServices ORDER BY CreatedAt DESC LIMIT 10', [], (err, services) => {
                if (err) {
                    console.error('❌ Error fetching expense services:', err.message);
                } else {
                    console.log('\n🛠️  Expense Services:');
                    console.log('===================');
                    if (services.length === 0) {
                        console.log('   No expense services found');
                    } else {
                        console.table(services);
                    }
                }
                
                // Get events
                db.all('SELECT Id, Name, EventDate, CreatedAt FROM Events ORDER BY EventDate DESC LIMIT 10', [], (err, events) => {
                    if (err) {
                        console.error('❌ Error fetching events:', err.message);
                    } else {
                        console.log('\n🎉 Recent Events:');
                        console.log('================');
                        if (events.length === 0) {
                            console.log('   No events found');
                        } else {
                            console.table(events);
                        }
                    }
                    
                    console.log('\n✨ Expense data exploration complete!');
                    db.close();
                });
            });
        });
    });
});
