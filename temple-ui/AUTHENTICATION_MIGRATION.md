# Authentication Migration Guide

## Overview
Authentication has been moved from `auth.js` to `main.js` to verify user credentials **before** the router initializes.

## New Authentication Flow

### 1. **Application Startup (main.js)**
```javascript
// 1. Load existing user data from localStorage
// 2. Set up axios interceptors  
// 3. Initialize Vue app
// 4. Setup router with authentication guard
// 5. Mount app
```

### 2. **Authentication Guard**
```javascript
// Before ANY route navigation:
// 1. Check if token exists
// 2. Verify credentials with /api/auth/me
// 3. Check role/permission requirements
// 4. Allow or redirect to home
```

### 3. **Global Auth API**
Available via `window.templeAuth`:
```javascript
// Get current state
window.templeAuth.getCurrentUser()
window.templeAuth.getCurrentToken() 
window.templeAuth.getCurrentPermissions()

// Check permissions
window.templeAuth.hasRole('Admin')
window.templeAuth.hasPermission('ExpenseApproval')

// Auth actions
window.templeAuth.login(username, password)
window.templeAuth.logout()
```

## Component Usage

### **Option 1: Use Vue Auth Store (Recommended)**
```javascript
// In Vue components
import { useAuth } from '@/stores/auth'

export default {
  setup() {
    const { isAuthenticated, hasRole, hasPermission, login, logout } = useAuth()
    
    // Use as before - no changes needed!
    return { isAuthenticated, hasRole, hasPermission, login, logout }
  }
}
```

### **Option 2: Use Global Auth Directly**
```javascript
// In any JavaScript code
const isAdmin = window.templeAuth.hasRole('Admin')
const canApprove = window.templeAuth.hasPermission('ExpenseApproval')
```

## Key Benefits

### ✅ **Early Authentication**
- Credentials verified BEFORE router loads
- Invalid tokens caught immediately
- No unauthorized page flashes

### ✅ **Centralized Security**
- All auth logic in one place (main.js)
- Consistent across entire app
- Easy to debug and maintain

### ✅ **Automatic Verification**
- Every navigation triggers credential check
- Invalid sessions automatically logged out
- Real-time security validation

### ✅ **Backward Compatibility**
- Existing components work unchanged
- useAuth() store still functional
- Gradual migration possible

## Migration Steps

### **For New Components:**
```javascript
// Direct global usage
const auth = window.templeAuth
if (auth.hasRole('Admin')) {
  // Admin only code
}
```

### **For Existing Components:**
No changes needed! The `useAuth()` store automatically bridges to the new system.

## Debugging

Check browser console for authentication flow:
```
🚀 Initializing Temple Management App...
🔐 Authentication Guard: Navigating to /admin/role-permissions
✅ Authentication passed, allowing access
✅ Temple Management App initialized successfully!
```

## Error Handling

Invalid credentials automatically:
- Remove from localStorage
- Clear axios headers
- Redirect to home page
- Show login prompt
