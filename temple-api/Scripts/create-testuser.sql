-- SQL script to create or update testuser in the database

-- Check if user exists
SELECT 'Current users in database:' as Info;
SELECT UserId, Username, Email, FullName, IsActive, IsVerified 
FROM Users;

-- Insert testuser if it doesn't exist
INSERT OR IGNORE INTO Users (
    Username, 
    Email, 
    FullName, 
    PasswordHash, 
    IsActive, 
    IsVerified, 
    CreatedAt
)
VALUES (
    'testuser@example.com',
    'testuser@example.com', 
    'Test User',
    'cGFzc3dvcmQxMjM=', -- Base64 encoded 'password123'
    1, -- Active
    1, -- Verified
    datetime('now')
);

-- Update if already exists but inactive
UPDATE Users 
SET IsActive = 1, 
    IsVerified = 1,
    PasswordHash = 'cGFzc3dvcmQxMjM=' -- Ensure password is correct
WHERE Email = 'testuser@example.com';

-- Get the user ID
SELECT UserId FROM Users WHERE Email = 'testuser@example.com';

-- Assign General role to testuser if not already assigned
INSERT OR IGNORE INTO UserRoles (UserId, RoleId, CreatedAt, IsActive)
SELECT 
    u.UserId,
    r.RoleId,
    datetime('now'),
    1
FROM Users u
CROSS JOIN Roles r
WHERE u.Email = 'testuser@example.com' 
  AND r.RoleName = 'General'
  AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur 
    WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId
  );

-- Verify the user was created/updated
SELECT 'After update - testuser status:' as Info;
SELECT u.UserId, u.Username, u.Email, u.FullName, u.IsActive, u.IsVerified,
       GROUP_CONCAT(r.RoleName) as Roles
FROM Users u
LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'testuser@example.com'
GROUP BY u.UserId;
