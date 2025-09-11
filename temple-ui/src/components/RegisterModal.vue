<template>
  <el-dialog
    v-model="visible"
    title="Register"
    width="500px"
    :before-close="handleClose"
    center
  >
    <el-form
      ref="registerFormRef"
      :model="registerForm"
      :rules="registerRules"
      label-width="100px"
      class="register-form"
    >
      <el-form-item label="Full Name" prop="name">
        <el-input
          v-model="registerForm.name"
          placeholder="Enter your full name"
          prefix-icon="User"
        />
      </el-form-item>
      
      <el-form-item label="Email" prop="email">
        <el-input
          v-model="registerForm.email"
          type="email"
          placeholder="Enter your email"
          prefix-icon="Message"
        />
      </el-form-item>
      
      <el-form-item label="Password" prop="password">
        <el-input
          v-model="registerForm.password"
          type="password"
          placeholder="Enter your password"
          prefix-icon="Lock"
          show-password
        />
      </el-form-item>
      
      <el-form-item label="Confirm Password" prop="confirmPassword">
        <el-input
          v-model="registerForm.confirmPassword"
          type="password"
          placeholder="Confirm your password"
          prefix-icon="Lock"
          show-password
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleClose">Cancel</el-button>
        <el-button type="primary" @click="handleRegister" :loading="loading">
          Register
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

const emit = defineEmits(['update:modelValue', 'register-success'])

const { register } = useAuth()
const visible = ref(props.modelValue)
const loading = ref(false)
const registerFormRef = ref()

const registerForm = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const validateConfirmPassword = (rule, value, callback) => {
  if (value !== registerForm.password) {
    callback(new Error('Passwords do not match'))
  } else {
    callback()
  }
}

const registerRules = {
  name: [
    { required: true, message: 'Please enter your full name', trigger: 'blur' }
  ],
  email: [
    { required: true, message: 'Please enter email', trigger: 'blur' },
    { type: 'email', message: 'Please enter a valid email', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Please enter password', trigger: 'blur' },
    { min: 6, message: 'Password must be at least 6 characters', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: 'Please confirm password', trigger: 'blur' },
    { validator: validateConfirmPassword, trigger: 'blur' }
  ]
}

const handleClose = () => {
  visible.value = false
  emit('update:modelValue', false)
  // Reset form
  Object.keys(registerForm).forEach(key => {
    registerForm[key] = ''
  })
  if (registerFormRef.value) {
    registerFormRef.value.resetFields()
  }
}

const handleRegister = async () => {
  if (!registerFormRef.value) return
  
  try {
    await registerFormRef.value.validate()
    loading.value = true
    
    const result = await register(registerForm.name, registerForm.email, registerForm.password)
    
    if (result.success) {
      ElMessage.success('Registration successful! Please login.')
      emit('register-success')
      handleClose()
    } else {
      ElMessage.error(result.message || 'Registration failed')
    }
  } catch (error) {
    console.error('Registration error:', error)
    ElMessage.error('Registration failed. Please try again.')
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
.register-form {
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
