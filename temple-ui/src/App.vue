<template>
  <div id="app">
    <el-container>
      <el-header>
        <div class="header-content" :class="{ 'home-header': isHomeRoute }">
          <h1 class="logo">
            <el-icon><HomeFilled /></el-icon>
            <span class="logo-text">Devakaryam</span>
          </h1>

          <!-- Navigation Menu -->
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
            <el-menu-item index="/dashboard">
              <el-icon><HomeFilled /></el-icon>
              <span>Dashboard</span>
            </el-menu-item>

            <!-- Management (new main menu) -->
            <el-sub-menu v-if="hasRole('Admin') || hasRole('Staff')" index="management">
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
              <el-menu-item index="/events">
                <el-icon><Calendar /></el-icon>
                <span>Events</span>
              </el-menu-item>
            </el-sub-menu>

            <!-- Administration -->
            <el-sub-menu v-if="hasRole('Admin')" index="administration">
              <template #title>
                <el-icon><Setting /></el-icon>
                <span>Administration</span>
              </template>
              <el-menu-item index="/temples">
                <el-icon><OfficeBuilding /></el-icon>
                <span>Temples</span>
              </el-menu-item>
              <el-menu-item index="/admin/users">
                <el-icon><User /></el-icon>
                <span>Users</span>
              </el-menu-item>
              <el-menu-item index="/roles">
                <el-icon><Star /></el-icon>
                <span>Roles</span>
              </el-menu-item>
              <el-menu-item index="/user-roles">
                <el-icon><Star /></el-icon>
                <span>User Roles</span>
              </el-menu-item>
              <el-menu-item index="/admin/role-permissions">
                <el-icon><Star /></el-icon>
                <span>Role Permissions</span>
              </el-menu-item>
            </el-sub-menu>


            <!-- Shop (moved to main header, as submenu group) -->
            <el-sub-menu v-if="hasRole('Admin') || hasRole('Staff')" index="main-shop">
              <template #title>
                <el-icon><Shop /></el-icon>
                <span>Shop</span>
              </template>
              <el-menu-item index="/products">
                <el-icon><Box /></el-icon>
                <span>Products</span>
              </el-menu-item>
              <el-menu-item v-if="hasRole('Admin')" index="/categories">
                <el-icon><Collection /></el-icon>
                <span>Categories</span>
              </el-menu-item>
              <el-menu-item index="/sales">
                <el-icon><ShoppingCart /></el-icon>
                <span>Sales</span>
              </el-menu-item>
            </el-sub-menu>
          </el-menu>

          <!-- User Profile -->
          <div class="user-section">
            <UserProfile />
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
  House
} from '@element-plus/icons-vue'
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import UserProfile from './components/UserProfile.vue'
import { useAuth } from './stores/auth.js'

const route = useRoute()
const { isAuthenticated, hasRole } = useAuth()
const isHomeRoute = computed(() => route.path === '/')
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
</style>
