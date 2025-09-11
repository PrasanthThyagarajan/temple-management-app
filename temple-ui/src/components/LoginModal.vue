<template>
  <el-dialog
    v-model="visible"
    title="Login"
    width="400px"
    :before-close="handleClose"
    center
  >
    <el-form
      ref="loginFormRef"
      :model="loginForm"
      :rules="loginRules"
      label-width="80px"
      class="login-form"
    >
      <el-form-item label="Username" prop="username">
        <el-input
          v-model="loginForm.username"
          placeholder="Enter your username"
          prefix-icon="User"
        />
      </el-form-item>
      
      <el-form-item label="Password" prop="password">
        <el-input
          v-model="loginForm.password"
          type="password"
          placeholder="Enter your password"
          prefix-icon="Lock"
          show-password
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleClose">Cancel</el-button>
        <el-button type="primary" @click="handleLogin" :loading="loading">
          Login
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { useAuth } from '../stores/auth.js'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'login-success'])

const { login } = useAuth()
const visible = ref(props.modelValue)
const loading = ref(false)
const loginFormRef = ref()

const loginForm = reactive({
  username: '',
  password: ''
})

const loginRules = {
  username: [
    { required: true, message: 'Please enter username', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Please enter password', trigger: 'blur' },
    { min: 6, message: 'Password must be at least 6 characters', trigger: 'blur' }
  ]
}

const handleClose = () => {
  visible.value = false
  emit('update:modelValue', false)
}

const handleLogin = async () => {
  if (!loginFormRef.value) return
  
  try {
    await loginFormRef.value.validate()
    loading.value = true
    
    const result = await login(loginForm.username, loginForm.password)
    
    if (result.success) {
      ElMessage.success('Login successful!')
      emit('login-success', result.user)
      handleClose()
    } else {
      ElMessage.error(result.message || 'Login failed')
    }
  } catch (error) {
    console.error('Login error:', error)
    ElMessage.error('Login failed. Please try again.')
  } finally {
    loading.value = false
  }
}

// Watch for prop changes
watch(() => props.modelValue, (newVal) => {
  visible.value = newVal
})
</script>

<style scoped>
.login-form {
  padding: 20px 0;
}

.dialog-footer {
  text-align: right;
}

:deep(.el-dialog__header) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 20px;
  margin: -20px -20px 20px -20px;
  border-radius: 8px 8px 0 0;
}

:deep(.el-dialog__title) {
  color: white;
  font-weight: 600;
}

:deep(.el-dialog__headerbtn .el-dialog__close) {
  color: white;
}
</style>
