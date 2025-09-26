import { ref, computed } from 'vue'

// Authentication is now handled in main.js
// This store acts as a Vue-reactive bridge to the global auth functions

const user = ref(null)
const token = ref(null)
const permissions = ref([])

// Initialize from global auth state
const initializeFromGlobal = () => {
  if (window.templeAuth) {
    user.value = window.templeAuth.getCurrentUser()
    token.value = window.templeAuth.getCurrentToken()
    permissions.value = window.templeAuth.getCurrentPermissions()
  }
}

// Auto-update when storage changes (for multi-tab sync)
const updateFromGlobal = () => {
  initializeFromGlobal()
}

// Listen for storage changes
window.addEventListener('storage', updateFromGlobal)

// Initialize immediately if global auth is available
if (typeof window !== 'undefined') {
  // Use a timeout to ensure main.js has initialized
  setTimeout(initializeFromGlobal, 100)
}

export const useAuth = () => {
  // Computed properties
  const isAuthenticated = computed(() => {
    if (window.templeAuth) {
      return !!(window.templeAuth.getCurrentUser() && window.templeAuth.getCurrentToken())
    }
    return !!(user.value && token.value)
  })
  
  // Auth functions that delegate to global auth
  const hasRole = (roleName) => {
    if (window.templeAuth) {
      return window.templeAuth.hasRole(roleName)
    }
    return user.value?.roles?.includes(roleName) || false
  }
  
  const hasPermission = (permissionName) => {
    if (window.templeAuth) {
      return window.templeAuth.hasPermission(permissionName)
    }
    return permissions.value?.includes(permissionName) || false
  }
  
  const hasPageReadPermission = async (pageUrl) => {
    if (window.templeAuth) {
      return window.templeAuth.hasPageReadPermission(pageUrl)
    }
    return false
  }
  
  const login = async (username, password) => {
    if (window.templeAuth) {
      const result = await window.templeAuth.login(username, password)
      // Update reactive refs after login
      updateFromGlobal()
      return result
    }
    return { success: false, message: 'Authentication system not available' }
  }
  
  const logout = (redirectToLogin = true) => {
    if (window.templeAuth) {
      window.templeAuth.logout(redirectToLogin)
      // Update reactive refs after logout
      updateFromGlobal()
    }
  }
  
  const register = async (fullName, email, password, nakshatra, dateOfBirth, gender) => {
    if (window.templeAuth) {
      const result = await window.templeAuth.register(fullName, email, password, nakshatra, dateOfBirth, gender)
      return result
    }
    return { success: false, message: 'Authentication system not available' }
  }
  
  const loadUser = () => {
    // This is now handled automatically by main.js
    updateFromGlobal()
  }
  
  // Keep reactive refs updated
  const refreshAuthState = () => {
    updateFromGlobal()
  }
  
  return {
    user: computed(() => {
      // Always get fresh data from global auth
      if (window.templeAuth) {
        return window.templeAuth.getCurrentUser()
      }
      return user.value
    }),
    token: computed(() => {
      // Always get fresh data from global auth
      if (window.templeAuth) {
        return window.templeAuth.getCurrentToken()
      }
      return token.value
    }),
    permissions: computed(() => {
      // Always get fresh data from global auth
      if (window.templeAuth) {
        return window.templeAuth.getCurrentPermissions()
      }
      return permissions.value
    }),
    isAuthenticated,
    hasRole,
    hasPermission,
    hasPageReadPermission,
    login,
    logout,
    register,
    loadUser,
    refreshAuthState
  }
}