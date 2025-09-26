-- Script to debug testuser permissions issue

-- 1. Check if testuser exists and their role
SELECT 'Testuser details:' as Info;
SELECT u.UserId, u.Username, u.Email, u.FullName, u.IsActive, u.IsVerified,
       GROUP_CONCAT(r.RoleName) as Roles
FROM Users u
LEFT JOIN UserRoles ur ON u.UserId = ur.UserId AND ur.IsActive = 1
LEFT JOIN Roles r ON ur.RoleId = r.RoleId AND r.IsActive = 1
WHERE u.Email = 'testuser@example.com'
GROUP BY u.UserId;

-- 2. Check what permissions the General role has
SELECT 'General role permissions:' as Info;
SELECT pp.PageName, pp.PageUrl, 
       CASE pp.PermissionId 
           WHEN 1 THEN 'Read'
           WHEN 2 THEN 'Create'
           WHEN 3 THEN 'Update'
           WHEN 4 THEN 'Delete'
           ELSE 'Unknown'
       END as Permission
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.RoleId
JOIN PagePermissions pp ON rp.PagePermissionId = pp.PagePermissionId
WHERE r.RoleName = 'General' AND rp.IsActive = 1
ORDER BY pp.PageUrl, pp.PermissionId;

-- 3. Specifically check Dashboard permissions for General role
SELECT 'Dashboard permissions for General role:' as Info;
SELECT r.RoleName, pp.PageName, pp.PageUrl, 
       CASE pp.PermissionId 
           WHEN 1 THEN 'Read'
           WHEN 2 THEN 'Create'
           WHEN 3 THEN 'Update'
           WHEN 4 THEN 'Delete'
           ELSE 'Unknown'
       END as Permission,
       rp.IsActive as RolePermissionActive,
       pp.IsActive as PagePermissionActive
FROM Roles r
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN PagePermissions pp ON rp.PagePermissionId = pp.PagePermissionId
WHERE r.RoleName = 'General' 
  AND pp.PageUrl = '/dashboard'
  AND pp.PermissionId = 1; -- Read permission

-- 4. Check if Dashboard page permission exists at all
SELECT 'Dashboard page permissions in database:' as Info;
SELECT PagePermissionId, PageName, PageUrl, PermissionId,
       CASE PermissionId 
           WHEN 1 THEN 'Read'
           WHEN 2 THEN 'Create'
           WHEN 3 THEN 'Update'
           WHEN 4 THEN 'Delete'
           ELSE 'Unknown'
       END as Permission,
       IsActive
FROM PagePermissions
WHERE PageUrl = '/dashboard';

-- 5. Check all roles that have Dashboard read permission
SELECT 'All roles with Dashboard read permission:' as Info;
SELECT r.RoleName, pp.PageName, pp.PageUrl, 
       CASE pp.PermissionId 
           WHEN 1 THEN 'Read'
           WHEN 2 THEN 'Create'
           WHEN 3 THEN 'Update'
           WHEN 4 THEN 'Delete'
           ELSE 'Unknown'
       END as Permission
FROM Roles r
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN PagePermissions pp ON rp.PagePermissionId = pp.PagePermissionId
WHERE pp.PageUrl = '/dashboard' 
  AND pp.PermissionId = 1
  AND rp.IsActive = 1
  AND r.IsActive = 1;

-- 6. Count of permissions by role
SELECT 'Permission count by role:' as Info;
SELECT r.RoleName, COUNT(*) as TotalPermissions,
       SUM(CASE WHEN pp.PermissionId = 1 THEN 1 ELSE 0 END) as ReadPermissions
FROM Roles r
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN PagePermissions pp ON rp.PagePermissionId = pp.PagePermissionId
WHERE rp.IsActive = 1
GROUP BY r.RoleName;
