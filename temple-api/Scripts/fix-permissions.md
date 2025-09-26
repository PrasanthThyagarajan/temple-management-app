# Fix for Testuser Dashboard Permission Issue

## Problem
The testuser has the General role but cannot access the dashboard because the permission data hasn't been seeded in the database.

## Solution

### Option 1: Run the API to trigger automatic seeding
1. Navigate to the temple-api directory:
   ```
   cd temple-api
   ```

2. Run the API:
   ```
   dotnet run
   ```

3. The API will automatically run the DataSeedingService on startup, which will:
   - Create all PagePermissions entries
   - Create RolePermissions mappings
   - Assign Dashboard read permission to the General role

### Option 2: Manually seed the permissions (if automatic seeding doesn't work)

Create and run this SQL script:

```sql
-- Insert Dashboard page permissions if they don't exist
INSERT OR IGNORE INTO PagePermissions (PageName, PageUrl, PermissionId, CreatedAt, IsActive)
VALUES 
    ('Dashboard', '/dashboard', 1, datetime('now'), 1), -- Read
    ('Dashboard', '/dashboard', 2, datetime('now'), 1), -- Create
    ('Dashboard', '/dashboard', 3, datetime('now'), 1), -- Update
    ('Dashboard', '/dashboard', 4, datetime('now'), 1); -- Delete

-- Get the General role ID
SELECT RoleId FROM Roles WHERE RoleName = 'General';

-- Insert RolePermission for General role to read Dashboard
-- (Replace {GeneralRoleId} with the actual ID from above query)
INSERT OR IGNORE INTO RolePermissions (RoleId, PagePermissionId, CreatedAt, IsActive)
SELECT 
    r.RoleId,
    pp.PagePermissionId,
    datetime('now'),
    1
FROM Roles r
CROSS JOIN PagePermissions pp
WHERE r.RoleName = 'General' 
  AND pp.PageUrl = '/dashboard'
  AND pp.PermissionId = 1; -- Read permission
```

## Verification

After running either solution, verify that:

1. The testuser can now access the dashboard
2. The permissions are correctly loaded by checking the browser console for messages like:
   - "✅ Permission granted: Role 'General' has read access to '/dashboard'"

## Root Cause

The issue occurred because:
1. The database was created but the DataSeedingService hadn't run
2. Without seeding, there are no PagePermissions or RolePermissions in the database
3. The permission check correctly denies access when no permissions exist
