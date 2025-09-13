import { ref, computed } from 'vue'
import axios from 'axios'

const user = ref(null)
const token = ref(localStorage.getItem('token'))
const permissions = ref([])

// Set up axios interceptor for authentication
if (token.value) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
}

export const useAuth = () => {
  const isAuthenticated = computed(() => !!user.value && !!token.value)
  
  const hasRole = (roleName) => {
    if (!user.value || !user.value.roles) return false
    return user.value.roles.includes(roleName)
  }
  
  const hasPermission = (permissionName) => {
    if (!permissions.value || permissions.value.length === 0) return false
    return permissions.value.includes(permissionName)
  }
  
  const login = async (username, password) => {
    try {
      const response = await axios.post('/api/auth/login', {
        username,
        password
      })
      
      if (response.data.success) {
        token.value = response.data.token
        user.value = response.data.user
        permissions.value = response.data.permissions || []
        
        localStorage.setItem('token', token.value)
        localStorage.setItem('user', JSON.stringify(user.value))
        localStorage.setItem('permissions', JSON.stringify(permissions.value))
        
        axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
        
        return { success: true, user: user.value }
      } else {
        return { success: false, message: response.data.message }
      }
    } catch (error) {
      console.error('Login error:', error)
      return { success: false, message: 'Login failed. Please try again.' }
    }
  }
  
  const register = async (name, email, password) => {
    try {
      const response = await axios.post('/api/auth/register', {
        name,
        email,
        password
      })
      
      if (response.data.success) {
        return { success: true, message: 'Registration successful! Please login.' }
      } else {
        return { success: false, message: response.data.message }
      }
    } catch (error) {
      console.error('Registration error:', error)
      return { success: false, message: 'Registration failed. Please try again.' }
    }
  }
  
  const logout = () => {
    user.value = null
    token.value = null
    permissions.value = []
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('permissions')
    delete axios.defaults.headers.common['Authorization']
  }
  
  const loadUser = () => {
    const storedUser = localStorage.getItem('user')
    const storedToken = localStorage.getItem('token')
    
    const storedPerms = localStorage.getItem('permissions')
    if (storedUser && storedToken) {
      try {
        user.value = JSON.parse(storedUser)
        token.value = storedToken
        permissions.value = storedPerms ? JSON.parse(storedPerms) : []
        axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      } catch (error) {
        console.error('Error loading user data:', error)
        logout()
      }
    }
  }
  
  // Load user on module initialization
  loadUser()
  
  return {
    user: computed(() => user.value),
    token: computed(() => token.value),
    isAuthenticated,
    hasRole,
    hasPermission,
    login,
    register,
    logout,
    loadUser
  }
}
