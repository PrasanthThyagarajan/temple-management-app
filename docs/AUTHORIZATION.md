# Temple Management Application - Authorization & Authentication Documentation

## Overview

This document provides comprehensive details about the authentication and authorization system implemented in the Temple Management Application. The system uses a configuration-based, role-based access control (RBAC) model with granular permissions for different features.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Authentication System](#authentication-system)
3. [Authorization System](#authorization-system)
4. [Database Schema](#database-schema)
5. [Configuration](#configuration)
6. [Implementation Details](#implementation-details)
7. [API Endpoints](#api-endpoints)
8. [Frontend Integration](#frontend-integration)
9. [Testing](#testing)
10. [Security Considerations](#security-considerations)

## Architecture Overview

The authorization system consists of:

- **Basic Authentication**: HTTP Basic Auth for API authentication
- **Claims-Based Authorization**: User identity and roles stored as claims
- **Configuration-Based Permissions**: Endpoint permissions defined in `appsettings.json`
- **Middleware-Based Enforcement**: Custom middleware intercepts requests and enforces permissions
- **Database-Backed RBAC**: Roles and permissions stored in database with many-to-many relationships

### Key Components

1. **Backend (ASP.NET Core)**
   - `BasicAuthenticationHandler`: Handles Basic Auth credentials
   - `PermissionAuthorizationMiddleware`: Enforces permission checks
   - `DataSeedingService`: Seeds initial roles, users, and permissions
   - Entity Framework Core models for users, roles, and permissions

2. **Frontend (Vue.js)**
   - Axios interceptors for auth headers
   - Vue Router guards for route protection
   - Pinia store for auth state management
   - Dynamic UI element visibility based on permissions

## Authentication System

### Basic Authentication

The system uses HTTP Basic Authentication where credentials are sent as a Base64-encoded string in the `Authorization` header.

#### Format
```
Authorization: Basic base64(username:password)
```

#### Implementation

**BasicAuthenticationHandler.cs**
```csharp
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract credentials from Authorization header
        var authHeader = Request.Headers["Authorization"].ToString();
        
        // Decode and validate credentials
        var credentials = ExtractCredentials(authHeader);
        
        // Authenticate user against database
        var user = await AuthenticateUser(credentials.Username, credentials.Password);
        
        // Create claims principal
        var claims = BuildClaims(user);
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
```

### Password Storage

Passwords are stored as Base64-encoded strings in the database for simplicity. In production, use proper password hashing (e.g., BCrypt, Argon2).

### User Claims

The following claims are added upon successful authentication:
- `ClaimTypes.NameIdentifier`: User ID
- `ClaimTypes.Name`: Username
- `ClaimTypes.Email`: Email address
- `ClaimTypes.Role`: User roles (multiple)
- `userid`: User ID (custom claim)
- `fullname`: Full name (custom claim)

## Authorization System

### Permission Model

The system uses a four-level permission model:

1. **Read** (PermissionId = 1): View access
2. **Create** (PermissionId = 2): Create new records
3. **Update** (PermissionId = 3): Edit existing records
4. **Delete** (PermissionId = 4): Remove records

### Configuration-Based Authorization

Endpoint permissions are configured in `appsettings.json`:

```json
{
  "Authorization": {
    "EnablePermissionBasedAuth": true,
    "DefaultRequireAuthentication": true,
    "PublicEndpoints": [
      "/api/auth/login",
      "/api/auth/register",
      "/api/auth/verify",
      "/api/temples"
    ],
    "EndpointPermissions": {
      "/api/users": {
        "GET": "Read",
        "POST": "Create",
        "PUT": "Update",
        "DELETE": "Delete"
      },
      "/api/devotees": {
        "GET": "Read",
        "POST": "Create",
        "PUT": "Update",
        "DELETE": "Delete"
      }
      // ... more endpoints
    }
  }
}
```

### Middleware Implementation

**PermissionAuthorizationMiddleware.cs**

The middleware:
1. Checks if the endpoint is public
2. Validates user authentication
3. Maps API endpoints to frontend page URLs
4. Queries database for user permissions
5. Returns 403 Forbidden if access denied

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Skip if disabled
    if (!_authSettings.EnablePermissionBasedAuth)
    {
        await _next(context);
        return;
    }

    var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
    
    // Allow public endpoints
    if (IsPublicEndpoint(path))
    {
        await _next(context);
        return;
    }

    // Check authentication
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        context.Response.StatusCode = 401;
        return;
    }

    // Get required permission
    var permission = GetRequiredPermission(path, context.Request.Method);
    
    // Check user permission
    var hasPermission = await CheckUserPermission(userId, pageUrl, permission);
    
    if (!hasPermission)
    {
        context.Response.StatusCode = 403;
        return;
    }

    await _next(context);
}
```

## Database Schema

### Tables

1. **Users**
   - UserId (PK)
   - Username
   - Email
   - PasswordHash
   - IsActive
   - IsVerified

2. **Roles**
   - RoleId (PK)
   - RoleName
   - Description
   - IsActive

3. **UserRoles** (Junction)
   - UserRoleId (PK)
   - UserId (FK)
   - RoleId (FK)
   - IsActive

4. **PagePermissions**
   - PagePermissionId (PK)
   - PageName
   - PageUrl
   - PermissionId (1-4)
   - IsActive

5. **RolePermissions** (Junction)
   - RolePermissionId (PK)
   - RoleId (FK)
   - PagePermissionId (FK)
   - IsActive

### Relationships

```
Users <--> UserRoles <--> Roles
                           |
                           v
                    RolePermissions
                           |
                           v
                    PagePermissions
```

## Configuration

### Backend Configuration

**Program.cs**
```csharp
// Add authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// Add authorization
builder.Services.AddAuthorization();

// Add middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PermissionAuthorizationMiddleware>();
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for Basic Auth
    });
});
```

## Implementation Details

### Data Seeding

The `DataSeedingService` automatically seeds:

1. **Default Roles**
   - Admin: Full access to all features
   - General: Limited access to specific features

2. **Default Users**
   - admin / admin123 (Admin role)
   - prachu2net@gmail.com / sedW6VBx (General role)
   - prachu2test@gmail.com / rc79HRBd (General role)

3. **Page Permissions**
   - All CRUD permissions for each feature page
   - Mapped to frontend routes (e.g., `/devotees`, `/donations`)

4. **Role Permissions**
   - Admin: All permissions
   - General: Specific read/create/update permissions

### Endpoint to Page URL Mapping

The middleware maps API endpoints to frontend page URLs:

```csharp
private string DeterminePageUrl(string endpoint)
{
    return endpoint switch
    {
        var e when e.StartsWith("/api/users") => "/admin/users",
        var e when e.StartsWith("/api/roles") => "/roles",
        var e when e.StartsWith("/api/devotees") => "/devotees",
        var e when e.StartsWith("/api/donations") => "/donations",
        var e when e.StartsWith("/api/events") => "/events",
        // ... more mappings
        _ => endpoint.Replace("/api", "")
    };
}
```

## API Endpoints

### Authentication Endpoints

1. **POST /api/auth/login**
   - Public endpoint
   - Validates credentials
   - Returns user info and roles

2. **POST /api/auth/register**
   - Public endpoint
   - Creates new user account
   - Sends verification email

3. **GET /api/auth/verify**
   - Public endpoint
   - Verifies email with code

4. **GET /api/auth/me**
   - Protected endpoint
   - Returns current user info

5. **GET /api/users/me/permissions**
   - Protected endpoint
   - Returns user's permissions

### Permission Endpoints

1. **GET /api/admin/role-permissions**
   - Admin only
   - Returns all role-permission mappings

2. **GET /api/permissions**
   - Admin only
   - Returns all page permissions

## Frontend Integration

### Axios Configuration

```javascript
// Request interceptor - adds auth header
axios.interceptors.request.use(
  (config) => {
    const authString = localStorage.getItem('auth')
    if (authString) {
      const credentials = btoa(`${username}:${password}`)
      config.headers.Authorization = `Basic ${credentials}`
    }
    return config
  }
)

// Response interceptor - handles 401
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Logout and redirect to login
      logout()
      router.push('/login')
    }
    return Promise.reject(error)
  }
)
```

### Route Guards

```javascript
router.beforeEach(async (to, from, next) => {
  const isAuthenticated = window.templeAuth.isAuthenticated()
  
  if (to.meta.requiresAuth && !isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresPermission) {
    const hasPermission = await window.templeAuth.hasPageReadPermission(to.path)
    if (!hasPermission) {
      next('/unauthorized')
    } else {
      next()
    }
  } else {
    next()
  }
})
```

### Dynamic UI Elements

```vue
<template>
  <div>
    <!-- Show Add button only if user has Create permission -->
    <el-button v-if="canCreate" @click="showAddDialog">
      Add New
    </el-button>
    
    <!-- Show Edit/Delete only if user has respective permissions -->
    <el-table :data="items">
      <el-table-column label="Actions">
        <template #default="scope">
          <el-button v-if="canUpdate" @click="edit(scope.row)">
            Edit
          </el-button>
          <el-button v-if="canDelete" @click="delete(scope.row)">
            Delete
          </el-button>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAuth } from '@/stores/auth'

const { hasCreatePermission, hasUpdatePermission, hasDeletePermission } = useAuth()

const canCreate = computed(() => hasCreatePermission('/devotees'))
const canUpdate = computed(() => hasUpdatePermission('/devotees'))
const canDelete = computed(() => hasDeletePermission('/devotees'))
</script>
```

## Testing

### Unit Tests

Authentication tests verify:
- Login with valid/invalid credentials
- Protected endpoint access
- Role and permission retrieval
- Email verification flow
- Password updates

### Integration Tests

Test the complete auth flow:
1. User registration
2. Email verification
3. Login
4. Access protected resources
5. Permission-based access control

### Test Data

Tests use in-memory database with isolated test data:
- Test user: testuser@example.com / password123
- Admin role with full permissions

## Security Considerations

### Current Implementation

1. **Basic Auth Limitations**
   - Credentials sent with every request
   - No built-in expiration
   - Vulnerable to replay attacks

2. **Password Storage**
   - Currently using Base64 encoding (NOT secure)
   - Should use proper hashing (BCrypt, Argon2)

3. **HTTPS Required**
   - Basic Auth must only be used over HTTPS
   - Credentials are visible in plain text otherwise

### Recommended Improvements

1. **Use JWT Tokens**
   - Stateless authentication
   - Built-in expiration
   - Refresh token support

2. **Implement Proper Password Hashing**
   ```csharp
   // Use BCrypt
   var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
   var isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
   ```

3. **Add Rate Limiting**
   - Prevent brute force attacks
   - Limit login attempts

4. **Implement Account Lockout**
   - Lock accounts after failed attempts
   - Require admin unlock or time-based unlock

5. **Add Two-Factor Authentication**
   - SMS or email OTP
   - Authenticator app support

6. **Audit Logging**
   - Log all authentication attempts
   - Track permission changes
   - Monitor suspicious activities

7. **Session Management**
   - Implement proper session timeout
   - Single sign-on support
   - Remember me functionality

## Migration Guide

To implement this authorization system in a new project:

1. **Database Setup**
   - Run migrations to create auth tables
   - Seed initial data with `DataSeedingService`

2. **Backend Configuration**
   - Add authentication services
   - Configure middleware pipeline
   - Set up CORS with credentials

3. **Frontend Setup**
   - Configure Axios interceptors
   - Implement auth store
   - Add route guards
   - Update UI components

4. **Testing**
   - Add integration tests
   - Test permission scenarios
   - Verify security measures

## Test Results

### Comprehensive Test Summary (September 26, 2025)

#### Admin User Testing
- **Credentials**: admin / admin123
- **Role**: Admin
- **Results**: Full access to all endpoints ✅
  - GET /api/auth/me - SUCCESS
  - GET /api/users - SUCCESS 
  - GET /api/users/me/permissions - SUCCESS
  - GET /api/admin/role-permissions - SUCCESS (92 permissions)

#### General User Testing
1. **User 1**: prachu2net@gmail.com / sedW6VBx
2. **User 2**: prachu2test@gmail.com / rc79HRBd
- **Role**: General
- **Permissions**: 
  - Home: Read
  - Dashboard: Read
  - Devotees: Read
  - Donations: Read, Create, Update
  - Events: Read
  - EventExpenses: Read, Create, Update
- **Test Results**:
  - ✅ Can access permitted endpoints
  - ❌ Blocked from admin endpoints (403)
  - ❌ Cannot delete resources (403)

#### Negative Test Cases
- ❌ No authentication header → 401 Unauthorized
- ❌ Invalid credentials → 401 Unauthorized  
- ❌ Insufficient permissions → 403 Forbidden
- ✅ Public endpoints accessible without auth

## Known Issues and Fixes

### CORS Configuration Issue
**Problem**: Admin users redirected to login when accessing UI menu links.

**Root Cause**: Missing `.AllowCredentials()` in CORS policy prevented Basic Auth headers in cross-origin requests.

**Fix Applied**:
```csharp
policy.WithOrigins(...)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();  // Critical for Basic Auth
```

### Authentication Flow in Frontend

The authentication has been centralized in `main.js` with the following benefits:

1. **Early Authentication**: Credentials verified before router loads
2. **Centralized Security**: All auth logic in one place
3. **Automatic Verification**: Every navigation triggers credential check
4. **Backward Compatibility**: Existing components work unchanged

**Global Auth API** (`window.templeAuth`):
```javascript
// State management
getCurrentUser()
getCurrentToken()
getCurrentPermissions()

// Permission checks
hasRole(roleName)
hasPermission(permissionName)
hasPagePermission(pageUrl, permissionName)
hasPageReadPermission(pageUrl)
hasCreatePermission(pageUrl)
hasUpdatePermission(pageUrl)
hasDeletePermission(pageUrl)

// Auth actions
login(username, password)
logout()
```

### Menu Permission Implementation

Menu items in the UI dynamically show/hide based on user permissions:

1. Each menu item checks `isMenuVisible(pageUrl)`
2. Sub-menus only show if at least one child is visible
3. Permissions are cached for performance
4. Admin role always has full access

## Architecture Decisions

### Why Basic Authentication?

1. **Simplicity**: Easy to implement and understand
2. **Stateless**: No session management required
3. **Compatible**: Works with all HTTP clients
4. **Migration Path**: Easy to upgrade to JWT later

### Why Configuration-Based Permissions?

1. **Flexibility**: Change permissions without code changes
2. **Maintainability**: All permissions in one place
3. **Testability**: Easy to test different configurations
4. **Scalability**: Add new endpoints without code changes

### Database Design Rationale

The many-to-many relationship model allows:
- Multiple roles per user
- Multiple permissions per role
- Granular control at page and operation level
- Easy permission inheritance

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check credentials are correct
   - Verify user is active and verified
   - Ensure Authorization header is sent

2. **403 Forbidden**
   - User lacks required permission
   - Check role assignments
   - Verify permission mappings

3. **Menu items not visible**
   - Clear browser cache
   - Check role permissions in database
   - Verify DataSeedingService has run

4. **CORS errors**
   - Ensure `.AllowCredentials()` is set
   - Check allowed origins match frontend URL
   - Verify browser is sending credentials

## Conclusion

This authorization system provides a flexible, configuration-based approach to securing the Temple Management Application. While using Basic Authentication for simplicity, the architecture supports easy migration to more secure authentication methods like JWT tokens. The granular permission system allows fine-grained control over user access to different features and operations.

The system has been thoroughly tested with multiple user scenarios and edge cases, demonstrating robust security enforcement at both API and UI levels.
