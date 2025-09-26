<template>
  <div id="app">
    <el-container>
      <el-header>
        <div class="header-content" :class="{ 'home-header': isHomeRoute }">
          <h1 class="logo">
            <el-icon><HomeFilled /></el-icon>
            <span class="logo-text">Devakaryam</span>
          </h1>

          <!-- Navigation Menu (only on non-home pages) -->
          <el-menu
            v-if="!isHomeRoute"
            mode="horizontal"
            :router="true"
            :default-active="$route.path"
            :ellipsis="false"
            class="nav-menu"
          >
            <!-- Home -->
            <el-menu-item index="/">
              <el-icon><House /></el-icon>
              <span>Home</span>
            </el-menu-item>

            <!-- Dashboard -->
            <el-menu-item v-if="isAuthenticated" index="/dashboard">
              <el-icon><HomeFilled /></el-icon>
              <span>Dashboard</span>
            </el-menu-item>

            <!-- Events (moved to main header) -->
            <el-sub-menu v-if="isAuthenticated" index="events">
              <template #title>
                <el-icon><Calendar /></el-icon>
                <span>Events</span>
              </template>
              <el-menu-item index="/events">
                <el-icon><Calendar /></el-icon> 
                <span>Events</span>
              </el-menu-item>
              <el-menu-item index="/event-expenses">
                <el-icon><ShoppingCart /></el-icon>
                <span>Event Expenses</span>
              </el-menu-item>
              <el-menu-item index="/event-expense-items">
                <el-icon><Box /></el-icon>
                <span>Expense Items</span>
              </el-menu-item>
              <el-menu-item index="/event-expense-services">
                <el-icon><Setting /></el-icon>
                <span>Expense Services</span>
              </el-menu-item>
              <el-menu-item index="/vouchers">
                <el-icon><Tickets /></el-icon>
                <span>Vouchers</span>
              </el-menu-item>
            </el-sub-menu>

            <!-- Management (updated) -->
            <el-sub-menu v-if="isAuthenticated" index="management">
              <template #title>
                <el-icon><Collection /></el-icon>
                <span>Management</span>
              </template>
              <el-menu-item index="/devotees">
                <el-icon><User /></el-icon>
                <span>Devotees</span>
              </el-menu-item>
              <el-menu-item index="/donations">
                <el-icon><Money /></el-icon>
                <span>Donations</span>
              </el-menu-item>
            </el-sub-menu>

            <!-- Administration -->
            <el-sub-menu v-if="isAuthenticated && (isMenuVisible('/temples') || isMenuVisible('/areas') || isMenuVisible('/admin/users') || isMenuVisible('/roles') || isMenuVisible('/user-roles') || isMenuVisible('/admin/role-permissions'))" index="administration">
              <template #title>
                <el-icon><Setting /></el-icon>
                <span>Administration</span>
              </template>
              <el-menu-item v-if="isMenuVisible('/temples')" index="/temples">
                <el-icon><OfficeBuilding /></el-icon>
                <span>Temples</span>
              </el-menu-item>
              <el-menu-item v-if="isMenuVisible('/areas')" index="/areas">
                <el-icon><OfficeBuilding /></el-icon>
                <span>Areas</span>
              </el-menu-item>
              <el-menu-item v-if="isMenuVisible('/admin/users')" index="/admin/users">
                <el-icon><User /></el-icon>
                <span>Users</span>
              </el-menu-item>
              <el-menu-item v-if="isMenuVisible('/roles')" index="/roles">
                <el-icon><Star /></el-icon>
                <span>Roles</span>
              </el-menu-item>
              <el-menu-item v-if="isMenuVisible('/user-roles')" index="/user-roles">
                <el-icon><Star /></el-icon>
                <span>User Roles</span>
              </el-menu-item>
              <el-menu-item v-if="isMenuVisible('/admin/role-permissions')" index="/admin/role-permissions">
                <el-icon><Star /></el-icon>
                <span>Role Permissions</span>
              </el-menu-item>
            </el-sub-menu>

            <!-- Shop (moved to main header, as submenu group) -->
            <el-sub-menu v-if="isAuthenticated" index="main-shop">
              <template #title>
                <el-icon><Shop /></el-icon>
                <span>Shop</span>
              </template>
              <el-menu-item index="/products">
                <el-icon><Box /></el-icon>
                <span>Products</span>
              </el-menu-item>
              <el-menu-item index="/categories">
                <el-icon><Collection /></el-icon>
                <span>Categories</span>
              </el-menu-item>
              <el-menu-item index="/sales">
                <el-icon><ShoppingCart /></el-icon>
                <span>Sales</span>
              </el-menu-item>
            </el-sub-menu>
          </el-menu>

          <!-- User Menu or Auth Actions -->
          <div class="auth-section">
            <!-- User Menu (when authenticated) -->
            <div v-if="isAuthenticated || (user && user.username) || hasBasicAuth || localUser" class="user-menu">
              <el-dropdown @command="handleUserCommand">
                <div class="user-dropdown-trigger">
                  <el-avatar :size="32" class="user-avatar">
                    <el-icon><User /></el-icon>
                  </el-avatar>
                  <span class="username">{{ user?.fullName || user?.username || localUser?.fullName || localUser?.username || 'User' }}</span>
                  <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
                </div>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="profile">
                      <el-icon><User /></el-icon>
                      <span>My Profile</span>
                    </el-dropdown-item>
                    <el-dropdown-item command="dashboard">
                      <el-icon><HomeFilled /></el-icon>
                      <span>Dashboard</span>
                    </el-dropdown-item>
                    <el-dropdown-item divided command="logout">
                      <el-icon><SwitchButton /></el-icon>
                      <span>Logout</span>
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </div>

        </div>
      </el-header>

      <el-main>
        <router-view />
      </el-main>

      <el-footer>
        <p>&copy; 2024 Devakaryam - Temple Management System</p>
      </el-footer>
    </el-container>
  </div>
</template>

<script setup>
import { 
  HomeFilled, 
  OfficeBuilding,
  User, 
  Money, 
  Calendar, 
  Box, 
  Collection, 
  ShoppingCart,
  Shop,
  Setting,
  Star,
  House,
  Tickets,
  ArrowDown,
  SwitchButton
} from '@element-plus/icons-vue'
import { computed, ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import UserProfile from './components/UserProfile.vue'
import { useAuth } from './stores/auth.js'

const route = useRoute()
const router = useRouter()
const { hasRole, hasPageReadPermission, isAuthenticated, user, token, logout, refreshAuthState } = useAuth()
const isHomeRoute = computed(() => route.path === '/')

// Force refresh auth state periodically to ensure menu shows up
onMounted(() => {
  refreshAuthState()
  // Check localStorage directly for debugging
  console.log('localStorage basicAuth:', localStorage.getItem('basicAuth'))
  console.log('localStorage user:', localStorage.getItem('user'))
  
  // Refresh auth state every few seconds to ensure reactivity
  setInterval(() => {
    refreshAuthState()
  }, 3000)
})

// Create reactive refs to check auth state directly
const hasBasicAuth = ref(false)
const localUser = ref(null)

// Check auth state directly from localStorage and global
const checkDirectAuthState = () => {
  hasBasicAuth.value = !!localStorage.getItem('basicAuth')
  const userStr = localStorage.getItem('user')
  localUser.value = userStr ? JSON.parse(userStr) : null
  
  // Force the auth store to update
  if (window.templeAuth) {
    // Update global auth state
    const globalUser = window.templeAuth.getCurrentUser()
    const globalToken = window.templeAuth.getCurrentToken()
    console.log('Direct auth check:', {
      hasBasicAuth: hasBasicAuth.value,
      localUser: localUser.value,
      globalUser: globalUser,
      globalToken: globalToken,
      isAuthenticated: isAuthenticated.value
    })
  }
}

// Check auth state on mount and periodically
onMounted(() => {
  checkDirectAuthState()
  setInterval(checkDirectAuthState, 2000)
  
  // Also force refresh the auth state immediately
  setTimeout(() => {
    refreshAuthState()
    checkDirectAuthState()
  }, 1000)
})

// Menu visibility state
const menuPermissions = ref({
  '/dashboard': false,
  '/events': false,
  '/event-expenses': false,
  '/event-expense-items': false,
  '/event-expense-services': false,
  '/vouchers': false,
  '/devotees': false,
  '/donations': false,
  '/temples': false,
  '/areas': false,
  '/admin/users': false,
  '/roles': false,
  '/user-roles': false,
  '/admin/role-permissions': false,
  '/products': false,
  '/categories': false,
  '/sales': false
})

// Function to check all menu permissions
const checkMenuPermissions = async () => {
  if (!isAuthenticated.value) {
    // Reset all permissions if not authenticated
    Object.keys(menuPermissions.value).forEach(key => {
      menuPermissions.value[key] = false
    })
    return
  }

  // Check each menu item permission (for admin-only items)
  for (const pageUrl of Object.keys(menuPermissions.value)) {
    menuPermissions.value[pageUrl] = await hasPageReadPermission(pageUrl)
  }
}

// Watch for authentication changes
watch([isAuthenticated, user], async () => {
  await checkMenuPermissions()
}, { immediate: true })

// Also check on mount
onMounted(async () => {
  await checkMenuPermissions()
})

// Helper function to check if a menu item should be visible
const isMenuVisible = (pageUrl) => {
  return menuPermissions.value[pageUrl] || false
}

// Handle user dropdown commands
const handleUserCommand = (command) => {
  switch (command) {
    case 'profile':
      router.push('/profile')
      break
    case 'dashboard':
      router.push('/dashboard')
      break
    case 'logout':
      logout()
      ElMessage.success('Logged out successfully')
      router.push('/')
      break
  }
}

// Simple profile navigation
const goToProfile = () => {
  router.push('/profile')
}

// Test login function
const testLogin = async () => {
  try {
    if (window.templeAuth) {
      const result = await window.templeAuth.login('admin', 'admin123')
      if (result.success) {
        ElMessage.success('Login successful!')
        refreshAuthState()
        checkDirectAuthState()
      } else {
        ElMessage.error(result.message || 'Login failed')
      }
    } else {
      ElMessage.error('Auth system not available')
    }
  } catch (error) {
    console.error('Test login error:', error)
    ElMessage.error('Login failed')
  }
}

// Debug logging for authentication status
console.log('isAuthenticated value:', isAuthenticated.value)
console.log('Current user:', user.value)
console.log('User roles:', user.value?.roles)
console.log('hasRole Admin:', hasRole('Admin'))
console.log('localStorage user:', localStorage.getItem('user'))
</script>

<style scoped>
#app {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.el-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 0;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  position: relative;
}

.header-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 100%;
  padding: 0 20px;
  position: relative;
}

.user-section {
  margin-left: auto;
  display: flex;
  align-items: center;
}

.home-header {
  justify-content: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 0;
  font-size: 24px;
  font-weight: bold;
}

.logo .el-icon {
  font-size: 28px;
}

.logo-text {
  display: inline;
}

/* NAV MENU FIX */
.nav-menu {
  background: transparent;
  border: none;
  display: flex;
  flex-wrap: nowrap !important;   /* don't wrap items */
  overflow: visible !important;   /* prevent ... overflow */
}


/* Menu item styling */
.nav-menu :deep(.el-menu-item) {
  color: #ffffff !important; /* not selected */
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
  white-space: nowrap;
}

.nav-menu :deep(.el-menu-item:hover) {
  background-color: rgba(255, 255, 255, 0.08);
}

.nav-menu :deep(.el-menu-item.is-active) {
  background-color: transparent;
  color: #d32f2f !important; /* selected red */
  border-bottom: 2px solid #d32f2f;
}

.nav-menu :deep(.el-sub-menu > .el-sub-menu__title) {
  color: #ffffff !important; /* not selected */
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
  white-space: nowrap;
}
.nav-menu :deep(.el-sub-menu > .el-sub-menu__title .el-icon),
.nav-menu :deep(.el-sub-menu > .el-sub-menu__title .el-sub-menu__icon-arrow) {
  color: #ffffff !important;
}
.nav-menu :deep(.el-sub-menu.is-active > .el-sub-menu__title .el-icon),
.nav-menu :deep(.el-sub-menu.is-active > .el-sub-menu__title .el-sub-menu__icon-arrow) {
  color: #d32f2f !important;
}

.nav-menu .el-sub-menu.is-active > .el-sub-menu__title {
  color: #d32f2f; /* selected red */
}

.nav-menu .el-sub-menu .el-sub-menu__title:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.nav-menu .el-sub-menu .el-menu {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.nav-menu .el-sub-menu .el-menu .el-menu-item {
  color: #ffffff !important; /* make submenu font white */
  font-weight: 500;
  padding: 0 20px;
  height: 40px;
  line-height: 40px;
  display: flex;
  align-items: center;
  gap: 10px;
}
.nav-menu .el-sub-menu .el-menu .el-menu-item .el-icon { color: #ffffff !important; }

/* Auth Section Styles */
.auth-section {
  margin-left: auto;
  display: flex;
  align-items: center;
}

.user-menu {
  display: flex;
  align-items: center;
}

.auth-buttons {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.user-dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.3s ease;
  color: white;
}

.user-dropdown-trigger:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.user-avatar {
  background: rgba(255, 255, 255, 0.2);
  border: 2px solid rgba(255, 255, 255, 0.3);
  color: white;
}

.username {
  font-weight: 500;
  font-size: 14px;
  color: white;
}

.dropdown-arrow {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.8);
}

/* User dropdown menu */
.user-menu :deep(.el-dropdown-menu) {
  background: white;
  border: 1px solid #ebeef5;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 160px;
}

.user-menu :deep(.el-dropdown-menu__item) {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  color: #606266;
  font-size: 14px;
}

.user-menu :deep(.el-dropdown-menu__item:hover) {
  background-color: #f5f7fa;
  color: #409eff;
}

.user-menu :deep(.el-dropdown-menu__item .el-icon) {
  color: #909399;
}

.user-menu :deep(.el-dropdown-menu__item:hover .el-icon) {
  color: #409eff;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .auth-section {
    margin-left: 16px;
  }
  
  .username {
    display: none;
  }
  
  .user-dropdown-trigger {
    padding: 8px;
  }
  
  .auth-buttons .el-button {
    padding: 4px 8px;
    font-size: 12px;
  }
}
</style>
