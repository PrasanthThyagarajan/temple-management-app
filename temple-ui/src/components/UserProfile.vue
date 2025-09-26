<template>
  <div class="user-profile-container">
    <div class="profile-header">
      <div class="header-content">
        <div class="avatar-section">
          <el-avatar :size="100" class="profile-avatar">
            <el-icon size="40"><User /></el-icon>
          </el-avatar>
          <div class="user-info">
            <h2 class="user-name">{{ userDetails?.fullName || 'User' }}</h2>
            <p class="user-email">{{ userDetails?.email }}</p>
            <div class="user-badges">
              <el-tag v-for="role in userDetails?.roles" :key="role" type="success" size="small">
                {{ role }}
              </el-tag>
            </div>
          </div>
        </div>
        <div class="header-actions">
          <el-button type="warning" :loading="resettingPassword" @click="resetPassword">
            <el-icon><Lock /></el-icon>
            Reset Password
          </el-button>
        </div>
      </div>
    </div>

    <div class="profile-content">
      <el-row :gutter="24">
        <!-- Main Profile Form -->
        <el-col :xs="24" :sm="24" :md="16" :lg="16">
          <el-card class="profile-form-card" shadow="hover">
            <template #header>
              <div class="card-title">
                <el-icon><EditPen /></el-icon>
                <span>Edit Profile Information</span>
              </div>
            </template>

            <el-form
              ref="formRef"
              :model="form"
              :rules="rules"
              :label-width="labelWidth"
              :label-position="labelPosition"
              class="profile-form"
            >
              <!-- Personal Information Section -->
              <div class="form-section">
                <h3 class="section-title">Personal Information</h3>
                
                <el-row :gutter="16">
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Full Name" prop="name">
                      <el-input 
                        v-model="form.name" 
                        placeholder="Enter your full name"
                        :prefix-icon="User"
                      />
                    </el-form-item>
                  </el-col>
                  
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Email Address" prop="email">
                      <el-input 
                        v-model="form.email" 
                        placeholder="Enter your email"
                        :prefix-icon="Message"
                      />
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-row :gutter="16">
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Gender" prop="gender">
                      <el-select v-model="form.gender" placeholder="Select Gender" style="width: 100%">
                        <el-option label="Male" value="Male">
                          <el-icon><Male /></el-icon>
                          <span style="margin-left: 8px;">Male</span>
                        </el-option>
                        <el-option label="Female" value="Female">
                          <el-icon><Female /></el-icon>
                          <span style="margin-left: 8px;">Female</span>
                        </el-option>
                        <el-option label="Other" value="Other">
                          <span>Other</span>
                        </el-option>
                      </el-select>
                    </el-form-item>
                  </el-col>
                  
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Phone Number" prop="phoneNumber">
                      <el-input 
                        v-model="form.phoneNumber" 
                        placeholder="Enter your phone number"
                        :prefix-icon="Phone"
                      />
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-row :gutter="16">
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Date of Birth" prop="dateOfBirth">
                      <el-date-picker 
                        v-model="form.dateOfBirth" 
                        type="date" 
                        placeholder="Select date of birth"
                        value-format="YYYY-MM-DD"
                        style="width: 100%"
                      />
                    </el-form-item>
                  </el-col>
                  
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Nakshatra" prop="nakshatra">
                      <el-input 
                        v-model="form.nakshatra" 
                        placeholder="Enter your nakshatra"
                        :prefix-icon="Star"
                      />
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-form-item label="Address" prop="address">
                  <el-input 
                    v-model="form.address" 
                    type="textarea" 
                    :rows="3"
                    placeholder="Enter your complete address"
                    resize="none"
                  />
                </el-form-item>
              </div>

              <!-- Security Section -->
              <div class="form-section">
                <h3 class="section-title">Security Settings</h3>
                <p class="section-description">Leave password fields empty to keep your current password</p>
                
                <el-row :gutter="16">
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="New Password" prop="password">
                      <el-input 
                        v-model="form.password" 
                        type="password" 
                        show-password
                        placeholder="Enter new password"
                        :prefix-icon="Lock"
                      />
                    </el-form-item>
                  </el-col>
                  
                  <el-col :xs="24" :sm="24" :md="12">
                    <el-form-item label="Confirm Password" prop="confirmPassword">
                      <el-input 
                        v-model="form.confirmPassword" 
                        type="password" 
                        show-password
                        placeholder="Confirm new password"
                        :prefix-icon="Lock"
                      />
                    </el-form-item>
                  </el-col>
                </el-row>
              </div>

              <!-- Form Actions -->
              <div class="form-actions">
                <el-button type="primary" size="large" :loading="saving" @click="save">
                  <el-icon><Check /></el-icon>
                  Save Changes
                </el-button>
                <el-button size="large" @click="resetForm">
                  <el-icon><Refresh /></el-icon>
                  Reset Form
                </el-button>
              </div>
            </el-form>
          </el-card>
        </el-col>

        <!-- Profile Summary Sidebar -->
        <el-col :xs="24" :sm="24" :md="8" :lg="8">
          <el-card class="profile-summary-card" shadow="hover">
            <template #header>
              <div class="card-title">
                <el-icon><InfoFilled /></el-icon>
                <span>Account Summary</span>
              </div>
            </template>

            <div class="summary-content">
              <div class="summary-item">
                <div class="summary-label">Account Status</div>
                <div class="summary-value">
                  <el-tag type="success" size="large">
                    <el-icon><CircleCheckFilled /></el-icon>
                    Active
                  </el-tag>
                </div>
              </div>

              <div class="summary-item" v-if="userDetails?.createdAt">
                <div class="summary-label">Member Since</div>
                <div class="summary-value">{{ formatDate(userDetails.createdAt) }}</div>
              </div>

              <div class="summary-item" v-if="userDetails?.roles?.length">
                <div class="summary-label">Assigned Roles</div>
                <div class="summary-value">
                  <div class="roles-list">
                    <el-tag 
                      v-for="role in userDetails.roles" 
                      :key="role" 
                      type="info" 
                      class="role-tag"
                    >
                      {{ role }}
                    </el-tag>
                  </div>
                </div>
              </div>

              <div class="summary-item">
                <div class="summary-label">Last Updated</div>
                <div class="summary-value">{{ new Date().toLocaleDateString() }}</div>
              </div>
            </div>

            <el-divider />

            <div class="quick-actions">
              <h4>Quick Actions</h4>
              <el-button text @click="$router.push('/dashboard')">
                <el-icon><DataBoard /></el-icon>
                Go to Dashboard
              </el-button>
              <el-button text @click="$router.push('/')">
                <el-icon><House /></el-icon>
                Back to Home
              </el-button>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  User, Lock, Check, Refresh, EditPen, InfoFilled, CircleCheckFilled,
  Message, Phone, Star, Male, Female, DataBoard, House
} from '@element-plus/icons-vue'

const formRef = ref()
const saving = ref(false)
const resettingPassword = ref(false)
const userDetails = ref(null)

const form = reactive({
  email: '',
  name: '',
  gender: '',
  phoneNumber: '',
  address: '',
  nakshatra: '',
  dateOfBirth: '',
  password: '',
  confirmPassword: ''
})

// Responsive computed properties
const labelWidth = computed(() => {
  return window.innerWidth < 768 ? '100px' : '140px'
})

const labelPosition = computed(() => {
  return window.innerWidth < 768 ? 'top' : 'right'
})

// Validation rules
const validatePasswordMatch = (rule, value, callback) => {
  if (form.password && value !== form.password) {
    callback(new Error('Passwords do not match'))
  } else {
    callback()
  }
}

const rules = {
  email: [
    { required: true, message: 'Email is required', trigger: 'blur' },
    { type: 'email', message: 'Please enter a valid email', trigger: 'blur' }
  ],
  name: [
    { required: true, message: 'Full name is required', trigger: 'blur' },
    { min: 2, message: 'Name must be at least 2 characters', trigger: 'blur' }
  ],
  phoneNumber: [
    { pattern: /^[\d\s\-\+\(\)]+$/, message: 'Please enter a valid phone number', trigger: 'blur' }
  ],
  password: [
    { min: 6, message: 'Password must be at least 6 characters', trigger: 'blur' }
  ],
  confirmPassword: [
    { validator: validatePasswordMatch, trigger: 'blur' }
  ]
}

// Load user profile data
const loadProfile = async () => {
  try {
    const { data } = await axios.get('/api/users/me')
    userDetails.value = data
    
    // Populate form
    form.email = data.email || ''
    form.name = data.fullName || ''
    form.gender = data.gender || ''
    form.phoneNumber = data.phoneNumber || ''
    form.address = data.address || ''
    form.nakshatra = data.nakshatra || ''
    form.dateOfBirth = data.dateOfBirth ? data.dateOfBirth.substring(0, 10) : ''
    
    // Clear password fields
    form.password = ''
    form.confirmPassword = ''
  } catch (error) {
    console.error('Failed to load profile:', error)
    ElMessage.error('Failed to load profile data')
  }
}

// Save profile changes
const save = async () => {
  try {
    if (!formRef.value) return
    await formRef.value.validate()
    
    saving.value = true
    
    const updateData = {
      email: form.email,
      name: form.name,
      gender: form.gender,
      phoneNumber: form.phoneNumber,
      address: form.address,
      nakshatra: form.nakshatra,
      dateOfBirth: form.dateOfBirth || null
    }

    // Include password only if provided
    if (form.password && form.password.trim()) {
      updateData.password = form.password
    }

    await axios.put('/api/users/me', updateData)
    
    ElMessage.success('Profile updated successfully!')
    
    // Clear password fields and reload
    form.password = ''
    form.confirmPassword = ''
    await loadProfile()
    
  } catch (error) {
    console.error('Save error:', error)
    if (error.response?.data?.message) {
      ElMessage.error(error.response.data.message)
    } else {
      ElMessage.error('Failed to update profile')
    }
  } finally {
    saving.value = false
  }
}

// Reset password
const resetPassword = async () => {
  try {
    await ElMessageBox.confirm(
      'This will generate a new password and send it to your email. Continue?',
      'Reset Password',
      {
        confirmButtonText: 'Yes, Reset',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    resettingPassword.value = true
    
    const { data: userData } = await axios.get('/api/users/me')
    const { data } = await axios.post(`/api/users/${userData.userId}/reset-password`)
    
    await ElMessageBox.alert(
      `Your new password is: ${data.newPassword}\n\nPlease save this password and change it after logging in.`,
      'Password Reset Successful',
      {
        confirmButtonText: 'Got it',
        type: 'success'
      }
    )
    
    ElMessage.success('Password reset successfully! Check your email for confirmation.')
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Reset password error:', error)
      ElMessage.error('Failed to reset password')
    }
  } finally {
    resettingPassword.value = false
  }
}

// Reset form to original values
const resetForm = () => {
  loadProfile()
  ElMessage.info('Form reset to original values')
}

// Format date helper
const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// Load profile on component mount
onMounted(() => {
  loadProfile()
})
</script>

<style scoped>
.user-profile-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  padding: 20px;
}

.profile-header {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  margin-bottom: 24px;
  overflow: hidden;
}

.header-content {
  padding: 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.avatar-section {
  display: flex;
  align-items: center;
  gap: 24px;
}

.profile-avatar {
  background: rgba(255, 255, 255, 0.2);
  border: 4px solid rgba(255, 255, 255, 0.3);
  color: white;
}

.user-info h2 {
  margin: 0 0 8px 0;
  font-size: 28px;
  font-weight: 600;
}

.user-email {
  margin: 0 0 12px 0;
  opacity: 0.9;
  font-size: 16px;
}

.user-badges {
  display: flex;
  gap: 8px;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.profile-content {
  max-width: 1200px;
  margin: 0 auto;
}

.profile-form-card,
.profile-summary-card {
  border-radius: 12px;
  border: none;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.card-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  font-size: 16px;
  color: var(--el-text-color-primary);
}

.profile-form {
  padding: 8px 0;
}

.form-section {
  margin-bottom: 32px;
  padding-bottom: 24px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.form-section:last-of-type {
  border-bottom: none;
  margin-bottom: 24px;
}

.section-title {
  margin: 0 0 16px 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-title::before {
  content: '';
  width: 4px;
  height: 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 2px;
}

.section-description {
  margin: 0 0 16px 0;
  color: var(--el-text-color-regular);
  font-size: 14px;
}

.form-actions {
  display: flex;
  gap: 16px;
  justify-content: flex-start;
  padding-top: 24px;
  border-top: 1px solid var(--el-border-color-lighter);
}

.summary-content {
  padding: 8px 0;
}

.summary-item {
  margin-bottom: 20px;
}

.summary-item:last-child {
  margin-bottom: 0;
}

.summary-label {
  font-size: 14px;
  color: var(--el-text-color-regular);
  margin-bottom: 4px;
  font-weight: 500;
}

.summary-value {
  font-size: 16px;
  color: var(--el-text-color-primary);
  font-weight: 600;
}

.roles-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.role-tag {
  align-self: flex-start;
}

.quick-actions h4 {
  margin: 0 0 12px 0;
  font-size: 16px;
  color: var(--el-text-color-primary);
}

.quick-actions .el-button {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 8px 0;
  justify-content: flex-start;
  padding: 8px 0;
}

/* Responsive Design */
@media (max-width: 768px) {
  .user-profile-container {
    padding: 12px;
  }
  
  .header-content {
    flex-direction: column;
    gap: 24px;
    text-align: center;
    padding: 24px;
  }
  
  .avatar-section {
    flex-direction: column;
    gap: 16px;
  }
  
  .user-info h2 {
    font-size: 24px;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .form-actions .el-button {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .header-content {
    padding: 16px;
  }
  
  .profile-form-card :deep(.el-card__body),
  .profile-summary-card :deep(.el-card__body) {
    padding: 16px;
  }
}

/* Form styling enhancements */
.profile-form :deep(.el-form-item__label) {
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.profile-form :deep(.el-input__inner),
.profile-form :deep(.el-textarea__inner) {
  border-radius: 8px;
  border: 2px solid var(--el-border-color);
  transition: all 0.3s ease;
}

.profile-form :deep(.el-input:focus-within .el-input__inner),
.profile-form :deep(.el-textarea:focus-within .el-textarea__inner) {
  border-color: #667eea;
  box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.1);
}

.profile-form :deep(.el-select .el-input__inner) {
  border-radius: 8px;
}

/* Button styling */
.form-actions .el-button {
  border-radius: 8px;
  padding: 12px 24px;
  font-weight: 600;
}

.header-actions .el-button {
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.2);
  border: 2px solid rgba(255, 255, 255, 0.3);
  color: white;
}

.header-actions .el-button:hover {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.5);
}
</style>