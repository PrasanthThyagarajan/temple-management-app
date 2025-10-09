import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import './styles/responsive.css'
import './styles/devotional-theme.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router from './router'
import axios from 'axios'
import RoleManagement from './components/RoleManagement.vue';
import UserRegistration from './components/UserRegistration.vue';
import UserRoleConfiguration from './components/UserRoleConfiguration.vue';

// ==========================================
// AUTHENTICATION SETUP - MOVED FROM auth.js
// ==========================================

// Authentication state
let currentUser = null
let currentBasic = localStorage.getItem('basicAuth') // base64(username:password)
let currentPermissions = []

// Set up axios defaults 
axios.defaults.baseURL = 'http://localhost:5051'

if (currentBasic) {
  axios.defaults.headers.common['Authorization'] = `Basic ${currentBasic}`
}

// Axios request interceptor
axios.interceptors.request.use(
  (config) => {
    try {
      // Always send Authorization when present; do not strip for GETs
    } catch (_) { /* ignore */ }
    return config
  },
  (error) => Promise.reject(error)
)

// Axios response interceptor
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    // On 401, force logout
    const status = error.response?.status
    if (status === 401) {
      logout(false)
    }
    return Promise.reject(error)
  }
)

// Authentication functions
const logout = (redirectToHome = true) => {
  currentUser = null
  currentBasic = null
  currentPermissions = []
  localStorage.removeItem('basicAuth')
  localStorage.removeItem('user')
  localStorage.removeItem('permissions')
  delete axios.defaults.headers.common['Authorization']
  
  // Clear permissions cache
  window.rolePermissionsCache = null
  
  if (redirectToHome) {
    window.location.href = '/'
  }
}

const loadUserData = () => {
  try {
    const userRaw = localStorage.getItem('user')
    const permsRaw = localStorage.getItem('permissions')
    
    if (userRaw) {
      currentUser = JSON.parse(userRaw)
    }
    if (permsRaw) {
      currentPermissions = JSON.parse(permsRaw)
    }
  } catch (error) {
    console.error('Error loading user data:', error)
    logout(false)
  }
}

// Refresh permissions from server without requiring full logout/login
const refreshPermissionsFromServer = async () => {
  if (!currentBasic) return false
  try {
    // Fetch role-permissions for current user; backend scopes to user's roles
    const response = await axios.get('/api/admin/role-permissions')
    const rolePermissions = Array.isArray(response.data) ? response.data : []
    // Derive distinct PageName list (e.g., "BookingApproval", "Vouchers", etc.)
    const pageNames = [...new Set(rolePermissions.map(rp => rp.pageName).filter(Boolean))]
    if (pageNames.length > 0) {
      currentPermissions = pageNames
      localStorage.setItem('permissions', JSON.stringify(currentPermissions))
      // Cache the full role-permissions for page-level checks
      window.rolePermissionsCache = rolePermissions
      return true
    }
  } catch (error) {
    console.error('Failed to refresh permissions from server:', error)
  }
  return false
}

// Verify user credentials function
const verifyUserCredentials = async () => {
  if (!currentBasic) {
    console.log('No token found, user not authenticated')
    return false
  }

  try {
    console.log('Verifying user credentials...')
    const response = await axios.get('/api/auth/me')
    
    if (response.data) {
      console.log('User credentials verified successfully')
      currentUser = response.data
      localStorage.setItem('user', JSON.stringify(currentUser))
      return true
    }
  } catch (error) {
    console.error('Failed to verify user credentials:', error)
    logout(false)
    return false
  }
  
  return false
}

// Check page permissions function
const checkPagePermissions = async (pageUrl, userRoles) => {
  try {
    console.log(`🔍 Checking permissions for page: ${pageUrl}`)
    console.log(`👤 User roles: ${userRoles}`)
    
    // Call the role-permissions endpoint to get user's permissions
    const response = await axios.get('/api/admin/role-permissions')
    
    if (!response.data || !Array.isArray(response.data)) {
      console.warn('⚠️ Invalid permissions response format')
      return false
    }
    
    const rolePermissions = response.data
    console.log(`📋 Retrieved ${rolePermissions.length} role permissions`)
    
    // Check if any of the user's roles have read permission for this page URL
    for (const userRole of userRoles) {
      const rolePerms = rolePermissions.filter(rp => 
        rp.roleName === userRole && 
        rp.pageUrl === pageUrl && 
        rp.permissionName === 'Read'
      )
      
      if (rolePerms.length > 0) {
        console.log(`✅ Permission granted: Role '${userRole}' has read access to '${pageUrl}'`)
        return true
      }
    }
    
    console.log(`❌ Permission denied: No read access to '${pageUrl}' for roles: ${userRoles.join(', ')}`)
    return false
    
  } catch (error) {
    console.error('🚨 Error checking page permissions:', error)
    // If admin role, allow access even if permission check fails
    if (userRoles.includes('Admin')) {
      console.log('🔓 Admin override: Allowing access despite permission check failure')
      return true
    }
    return false
  }
}

// Show permission denied popup
const showPermissionDeniedPopup = (pageUrl, userRoles) => {
  // Create a more sophisticated popup using DOM manipulation
  const createPermissionDeniedModal = () => {
    // Remove existing modal if any
    const existingModal = document.getElementById('permission-denied-modal')
    if (existingModal) {
      existingModal.remove()
    }

    // Create modal HTML
    const modalHTML = `
      <div id="permission-denied-modal" style="
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      ">
        <div style="
          background: white;
          padding: 30px;
          border-radius: 8px;
          box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
          max-width: 500px;
          width: 90%;
          text-align: center;
        ">
          <div style="color: #f56565; font-size: 48px; margin-bottom: 20px;">🚫</div>
          <h2 style="color: #2d3748; margin-bottom: 15px; font-size: 24px;">Access Denied</h2>
          <p style="color: #4a5568; margin-bottom: 10px; line-height: 1.5;">
            You don't have permission to access:
          </p>
          <p style="color: #e53e3e; font-weight: bold; margin-bottom: 15px; font-size: 16px;">
            ${pageUrl}
          </p>
          <p style="color: #718096; margin-bottom: 20px; font-size: 14px;">
            Your roles: <strong>${userRoles.join(', ')}</strong>
          </p>
          <p style="color: #718096; margin-bottom: 25px; font-size: 14px;">
            Please contact your administrator for access.
          </p>
          <div style="display: flex; gap: 10px; justify-content: center;">
            <button id="goto-home-btn" style="
              background: #3182ce;
              color: white;
              border: none;
              padding: 10px 20px;
              border-radius: 5px;
              cursor: pointer;
              font-size: 14px;
              font-weight: 500;
            ">Go to Home</button>
            <button id="stay-here-btn" style="
              background: #e2e8f0;
              color: #4a5568;
              border: none;
              padding: 10px 20px;
              border-radius: 5px;
              cursor: pointer;
              font-size: 14px;
              font-weight: 500;
            ">Stay Here</button>
          </div>
        </div>
      </div>
    `

    // Add modal to DOM
    document.body.insertAdjacentHTML('beforeend', modalHTML)

    // Add event listeners
    document.getElementById('goto-home-btn').addEventListener('click', () => {
      document.getElementById('permission-denied-modal').remove()
      window.location.href = '/'
    })

    document.getElementById('stay-here-btn').addEventListener('click', () => {
      document.getElementById('permission-denied-modal').remove()
    })

    // Close on backdrop click
    document.getElementById('permission-denied-modal').addEventListener('click', (e) => {
      const t = e && e.target
      if (t && t instanceof HTMLElement && t.id === 'permission-denied-modal') {
        document.getElementById('permission-denied-modal').remove()
      }
    })
  }

  createPermissionDeniedModal()
}

// Authentication guard for router
const authenticationGuard = async (to, from, next) => {
  console.log(`🔐 Authentication Guard: Navigating to ${to.path}`)
  
  // Always allow access to home page and registration page
  if (to.name === 'Home' || to.path === '/' || to.name === 'UserRegistration' || to.path === '/register') {
    return next()
  }
  
  // If no token, redirect to home with login prompt
  if (!currentBasic) {
    console.log('❌ No token found, redirecting to home')
    return next({ name: 'Home', query: { login: '1', redirect: to.fullPath } })
  }
  
  // Verify credentials before allowing access
  const isValid = await verifyUserCredentials()
  if (!isValid) {
    console.log('❌ Invalid credentials, redirecting to home')
    return next({ name: 'Home', query: { login: '1', redirect: to.fullPath } })
  }
  
  // Get user roles
  const userRoles = currentUser?.roles || []
  if (userRoles.length === 0) {
    console.log('❌ No roles found for user')
    showPermissionDeniedPopup(to.path, ['No roles assigned'])
    return next(false)
  }
  
  // Allow access to user profile for all authenticated users
  if (to.path === '/profile') {
    console.log('✅ Allowing access to user profile')
    return next()
  }
  
  // Check page permissions before allowing access
  const hasPermission = await checkPagePermissions(to.path, userRoles)
  if (!hasPermission) {
    console.log(`❌ Permission denied for page: ${to.path}`)
    showPermissionDeniedPopup(to.path, userRoles)
    return next(false) // Stay on current page
  }
  
  console.log('✅ Authentication and permissions passed, allowing access')
  next()
}

// ==========================================
// APP INITIALIZATION
// ==========================================

const initApp = async () => {
  try {
    console.log('🚀 Initializing Temple Management App...')
    
    // Load existing user data
    loadUserData()
    // Attempt to refresh permissions proactively so newly seeded permissions (e.g., BookingApproval) are available without logout/login
    if (currentBasic) {
      await refreshPermissionsFromServer()
    }
    
    // Create Vue app
    const app = createApp(App)
    
    // Register Element Plus icons
    for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
      app.component(key, component)
    }
    
    // Setup Pinia store
    app.use(createPinia())
    
    // Setup router with authentication guard
    router.beforeEach(authenticationGuard)
    app.use(router)
    
    // Setup Element Plus
    app.use(ElementPlus)
    
    // Register components
    app.component('RoleManagement', RoleManagement)
    app.component('UserRegistration', UserRegistration)
    app.component('UserRoleConfiguration', UserRoleConfiguration)
    
    // Mount the app
    app.mount('#app')
    
    console.log('✅ Temple Management App initialized successfully!')
    
  } catch (error) {
    console.error('❌ Failed to initialize app:', error)
  }
}

// Export auth utilities for use in components
(window)["templeAuth"] = {
  getCurrentUser: () => currentUser,
  getCurrentToken: () => currentBasic,
  getCurrentPermissions: () => currentPermissions,
  hasRole: (roleName) => currentUser?.roles?.includes(roleName) || false,
  hasPermission: (permissionName) => currentPermissions?.includes(permissionName) || false,
  hasPagePermission: async (pageUrl, permissionName) => {
    const userRoles = currentUser?.roles || []
    if (userRoles.length === 0) return false
    if (userRoles.includes('Admin')) return true

    // Use cached permissions if available
    if (window.rolePermissionsCache) {
      return window.rolePermissionsCache.some(rp => 
        userRoles.includes(rp.roleName) && 
        rp.pageUrl === pageUrl && 
        rp.permissionName === permissionName
      )
    }

    try {
      const response = await axios.get('/api/admin/role-permissions')
      if (response.data && Array.isArray(response.data)) {
        window.rolePermissionsCache = response.data
        return response.data.some(rp => 
          userRoles.includes(rp.roleName) && 
          rp.pageUrl === pageUrl && 
          rp.permissionName === permissionName
        )
      }
    } catch (error) {
      console.error('Error checking page permission:', error)
    }
    return false
  },
  hasCreatePermission: async (pageUrl) => (window["templeAuth"].hasPagePermission(pageUrl, 'Create')),
  hasUpdatePermission: async (pageUrl) => (window["templeAuth"].hasPagePermission(pageUrl, 'Update')),
  hasDeletePermission: async (pageUrl) => (window["templeAuth"].hasPagePermission(pageUrl, 'Delete')),
  hasPageReadPermission: async (pageUrl) => {
    // Check if user has read permission for a specific page
    const userRoles = currentUser?.roles || []
    if (userRoles.length === 0) return false
    
    // Admin always has access
    if (userRoles.includes('Admin')) return true
    
    // Use cached permissions if available
    if (window.rolePermissionsCache) {
      return window.rolePermissionsCache.some(rp => 
        userRoles.includes(rp.roleName) && 
        rp.pageUrl === pageUrl && 
        rp.permissionName === 'Read'
      )
    }
    
    // Otherwise fetch permissions
    try {
      const response = await axios.get('/api/admin/role-permissions')
      if (response.data && Array.isArray(response.data)) {
        window.rolePermissionsCache = response.data
        return response.data.some(rp => 
          userRoles.includes(rp.roleName) && 
          rp.pageUrl === pageUrl && 
          rp.permissionName === 'Read'
        )
      }
    } catch (error) {
      console.error('Error checking page permission:', error)
    }
    return false
  },
  logout,
  // Expose refresh so views can trigger a manual permissions refresh on demand
  refreshPermissions: refreshPermissionsFromServer,
  
  // Register function for components to use
  register: async (fullName, email, password, nakshatra, dateOfBirth, gender) => {
    try {
      const response = await axios.post('/api/auth/register', {
        username: email, 
        email: email,
        fullName: fullName,
        gender: gender,
        password: password,
        confirmPassword: password,
        nakshatra: nakshatra,
        dateOfBirth: dateOfBirth
      })
      
      if (response.data.success) {
        return { success: true, message: 'Registration successful! Please check your email to verify your account.' }
      } else {
        return { success: false, message: response.data.message || 'Registration failed' }
      }
    } catch (error) {
      console.error('Registration error:', error)
      const message = error.response?.data?.message || 'Registration failed. Please try again.'
      return { success: false, message }
    }
  },
  
  // Login function for components to use
  login: async (username, password) => {
    try {
      // Build Basic header and set globally before calling login
      const basic = btoa(`${username}:${password}`)
      axios.defaults.headers.common['Authorization'] = `Basic ${basic}`
      // Call login to fetch user/roles/permissions
      const response = await axios.post('/api/auth/login')
      
      if (response.data.success) {
        currentUser = response.data.user
        currentPermissions = response.data.permissions || []
        
        currentBasic = basic
        localStorage.setItem('basicAuth', currentBasic)
        localStorage.setItem('user', JSON.stringify(currentUser))
        localStorage.setItem('permissions', JSON.stringify(currentPermissions))
        
        axios.defaults.headers.common['Authorization'] = `Basic ${currentBasic}`
        
        // Clear permissions cache on login
        window.rolePermissionsCache = null
        
        // Reload the page after successful login
        window.location.reload()
        
        return { success: true, user: currentUser }
      } else {
        return { success: false, message: response.data.message }
      }
    } catch (error) {
      console.error('Login error:', error)
      return { success: false, message: 'Login failed. Please try again.' }
    }
  }
}

// Start the application
initApp()
