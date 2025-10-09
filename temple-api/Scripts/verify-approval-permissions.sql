-- Script to verify BookingApproval and ExpenseApproval permissions were added

-- 1. Check if the approval permissions exist
SELECT 'Approval Permissions in Database:' as Info;
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
WHERE PageName IN ('BookingApproval', 'ExpenseApproval')
ORDER BY PageName, PermissionId;

-- 2. Check if Admin role has these permissions
SELECT 'Admin Role Approval Permissions:' as Info;
SELECT r.RoleName, pp.PageName, pp.PageUrl, 
       CASE pp.PermissionId 
           WHEN 1 THEN 'Read'
           WHEN 2 THEN 'Create'
           WHEN 3 THEN 'Update'
           WHEN 4 THEN 'Delete'
           ELSE 'Unknown'
       END as Permission,
       rp.IsActive as RolePermissionActive
FROM Roles r
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN PagePermissions pp ON rp.PagePermissionId = pp.PagePermissionId
WHERE r.RoleName = 'Admin' 
  AND pp.PageName IN ('BookingApproval', 'ExpenseApproval')
ORDER BY pp.PageName, pp.PermissionId;

-- 3. Count total approval permissions
SELECT 'Summary:' as Info;
SELECT 
    COUNT(DISTINCT CASE WHEN PageName = 'BookingApproval' THEN PagePermissionId END) as BookingApprovalPerms,
    COUNT(DISTINCT CASE WHEN PageName = 'ExpenseApproval' THEN PagePermissionId END) as ExpenseApprovalPerms
FROM PagePermissions
WHERE PageName IN ('BookingApproval', 'ExpenseApproval') AND IsActive = 1;

