<template>
  <el-dropdown @command="handleCommand" v-if="user">
    <div class="user-profile">
      <el-avatar :size="32" :src="userAvatar">
        {{ user.fullName?.charAt(0)?.toUpperCase() || 'U' }}
      </el-avatar>
      <span class="user-name">{{ user.fullName || user.username }}</span>
      <el-icon class="el-icon--right">
        <arrow-down />
      </el-icon>
    </div>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item command="profile">
          <el-icon><User /></el-icon>
          Profile
        </el-dropdown-item>
        <el-dropdown-item command="settings">
          <el-icon><Setting /></el-icon>
          Settings
        </el-dropdown-item>
        <el-dropdown-item divided command="logout">
          <el-icon><SwitchButton /></el-icon>
          Logout
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
  
  <div v-else class="login-buttons">
    <el-button type="primary" @click="showLogin = true">Login</el-button>
    <el-button @click="showRegister = true">Register</el-button>
  </div>

  <!-- Login Modal -->
  <LoginModal
    v-model="showLogin"
    @login-success="handleLoginSuccess"
  />

  <!-- Register Modal -->
  <RegisterModal
    v-model="showRegister"
    @register-success="handleRegisterSuccess"
  />
</template>

<script setup>
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { User, Setting, SwitchButton, ArrowDown } from '@element-plus/icons-vue'
import LoginModal from './LoginModal.vue'
import RegisterModal from './RegisterModal.vue'
import { useAuth } from '../stores/auth.js'

const { user, isAuthenticated, logout } = useAuth()
const showLogin = ref(false)
const showRegister = ref(false)

const userAvatar = computed(() => {
  // You can add avatar URL logic here
  return null
})

const handleCommand = (command) => {
  switch (command) {
    case 'profile':
      // Navigate to profile page or show profile modal
      ElMessage.info('Profile feature coming soon!')
      break
    case 'settings':
      // Navigate to settings page
      ElMessage.info('Settings feature coming soon!')
      break
    case 'logout':
      handleLogout()
      break
  }
}

const handleLoginSuccess = (userData) => {
  showLogin.value = false
  ElMessage.success(`Welcome back, ${userData.fullName || userData.username}!`)
}

const handleRegisterSuccess = () => {
  showRegister.value = false
  showLogin.value = true
  ElMessage.info('Please login with your new account')
}

const handleLogout = () => {
  logout()
  ElMessage.success('Logged out successfully')
}
</script>

<style scoped>
.user-profile {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.user-profile:hover {
  background-color: var(--el-color-primary-light-9);
}

.user-name {
  font-weight: 500;
  color: var(--el-text-color-primary);
}

.login-buttons {
  display: flex;
  gap: 8px;
}

:deep(.el-dropdown-menu__item) {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
