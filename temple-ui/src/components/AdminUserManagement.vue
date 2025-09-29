<template>
  <div class="admin-user-management-container">
    <el-card class="admin-user-management-card">
      <template #header>
        <div class="card-header">
          <h2>User Management</h2>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>
            <span class="btn-text">Add User</span>
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner users-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search users..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-button type="info" @click="load" title="Refresh data">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Summary Cards -->
      <div class="summary-cards">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><User /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ users.length }}</div>
                  <div class="summary-label">Total Users</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon active">
                  <el-icon><Check /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ activeUsers }}</div>
                  <div class="summary-label">Active Users</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Table -->
      <div class="table-container">
        <el-table 
          :data="filteredUsers" 
          v-loading="loading" 
          stripe 
          style="width: 100%"
        >
          <el-table-column prop="userId" label="ID" width="80" sortable />
          <el-table-column prop="username" label="Username" min-width="160" show-overflow-tooltip sortable>
            <template #default="scope">
              <strong>{{ scope.row.username }}</strong>
            </template>
          </el-table-column>
          <el-table-column prop="email" label="Email" min-width="200" show-overflow-tooltip sortable />
          <el-table-column prop="fullName" label="Full Name" min-width="180" show-overflow-tooltip sortable />
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="openEdit(scope.row)">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button size="small" type="danger" @click.stop="confirmDelete(scope.row.userId)">
                  <el-icon><Delete /></el-icon>
                  <span class="btn-text">Delete</span>
                </el-button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="pagination-container">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="users.length"
          layout="total, prev, pager, next"
        />
      </div>
    </el-card>

    <el-dialog v-model="dialog.visible" :title="dialog.mode === 'create' ? 'Create User' : 'Edit User'" width="600px">
      <el-form :model="form" :rules="rules" ref="formRef" label-width="120px">
        <el-form-item label="Email" prop="email">
          <el-input v-model="form.email" placeholder="Email" />
        </el-form-item>
        <el-form-item label="Full Name" prop="name">
          <el-input v-model="form.name" placeholder="Full name" />
        </el-form-item>
        <el-form-item label="Address" prop="address">
          <el-input v-model="form.address" type="textarea" :rows="2" placeholder="Address" />
        </el-form-item>
        <el-form-item label="Password" prop="password">
          <el-input v-model="form.password" type="password" placeholder="Password" :disabled="dialog.mode==='edit'" />
        </el-form-item>
        <el-form-item label="Nakshatra" prop="nakshatra">
          <el-select
            v-model="form.nakshatra"
            placeholder="Select Nakshatra"
            clearable
          >
            <el-option
              v-for="nakshatra in nakshatras"
              :key="nakshatra"
              :label="nakshatra"
              :value="nakshatra"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Date of Birth" prop="dateOfBirth">
          <el-date-picker
            v-model="form.dateOfBirth"
            type="datetime"
            placeholder="Select date and time"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button v-if="dialog.mode==='edit'" type="warning" :loading="resetting" @click="resetPassword">
            Reset Password
          </el-button>
          <el-button @click="dialog.visible = false">Cancel</el-button>
          <el-button type="primary" :loading="saving" @click="submitDialog">{{ dialog.mode==='create' ? 'Create' : 'Save' }}</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete, User, Check, Search, Refresh } from '@element-plus/icons-vue'

export default {
  name: 'AdminUserManagement',
  components: { Plus, Edit, Delete, User, Check, Search, Refresh },
  data(){
    return {
      loading: false,
      saving: false,
      resetting: false,
      users: [],
      searchTerm: '',
      currentPage: 1,
      pageSize: 20,
      dialog: { visible: false, mode: 'create' },
      form: { userId: 0, email: '', name: '', address: '', password: '', nakshatra: '', dateOfBirth: null },
      nakshatras: [
          'Aswathy','Bharani','Karthika','Rohini','Makayiram','Thiruvathira',
          'Punartham','Pooyam','Ayilyam','Makam','Pooram','Uthram',
          'Hastham','Chithira','Chothi','Visakham','Anizham','Thrikketta',
          'Moolam','Pooradam','Uthradam','Thiruvonam','Avittam','Chathayam',
          'Pooruruttathi','Uthirattathi','Revathi'

      ],
      rules: {
        email: [ { required: true, message: 'Email is required', trigger: 'blur' } ],
        name: [ { required: true, message: 'Full name is required', trigger: 'blur' } ],
        password: [ { required: true, message: 'Password is required', trigger: 'blur' } ]
      },
    }
  },
  computed: {
    activeUsers() {
      return this.users.filter(user => user.isActive).length
    },
    filteredUsers() {
      if (!this.searchTerm) return this.users
      const term = this.searchTerm.toLowerCase()
      return this.users.filter(user => 
        user.username?.toLowerCase().includes(term) ||
        user.email?.toLowerCase().includes(term) ||
        user.fullName?.toLowerCase().includes(term)
      )
    }
  },
  methods: {
    handleSearch() {
      // Search is handled by computed property
    },
    async load(){
      this.loading = true
      try {
        const { data } = await axios.get('/api/users')
        this.users = data
      } finally {
        this.loading = false
      }
    },
    openCreate(){
      this.dialog.mode = 'create'
      this.dialog.visible = true
      this.form = { userId: 0, email: '', name: '', address: '', password: '', nakshatra: '', dateOfBirth: null }
    },
    openEdit(u){
      this.dialog.mode = 'edit'
      this.dialog.visible = true
      this.form = { userId: u.userId, email: u.email, name: u.fullName, address: u.address || '', password: '', nakshatra: u.nakshatra || '', dateOfBirth: u.dateOfBirth || null }
    },
    async submitDialog(){
      this.saving = true
      try {
        if (this.dialog.mode === 'create'){
          await axios.post('/api/users', { 
            email: this.form.email, 
            name: this.form.name, 
            address: this.form.address,
            password: this.form.password,
            nakshatra: this.form.nakshatra,
            dateOfBirth: this.form.dateOfBirth
          })
          ElMessage.success('User created')
        } else {
          await axios.put(`/api/users/${this.form.userId}`, { 
            email: this.form.email, 
            name: this.form.name, 
            address: this.form.address,
            password: this.form.password,
            nakshatra: this.form.nakshatra,
            dateOfBirth: this.form.dateOfBirth
          })
          ElMessage.success('User updated')
        }
        this.dialog.visible = false
        await this.load()
      } catch (e) {
        ElMessage.error('Operation failed')
      } finally {
        this.saving = false
      }
    },
    async resetPassword(){
      try {
        this.resetting = true
        const { data } = await axios.post(`/api/users/${this.form.userId}/reset-password`)
        const pwd = data?.newPassword
        if (pwd){
          await ElMessageBox.alert(
            `Password has been reset. New temporary password:\n\n${pwd}\n\nPlease share securely or have the user change it after login.`,
            'Password Reset',
            { confirmButtonText: 'OK' }
          )
        } else {
          ElMessage.success('Password reset')
        }
      } catch (e) {
        ElMessage.error('Failed to reset password')
      } finally {
        this.resetting = false
      }
    },
    async confirmDelete(id){
      try {
        await ElMessageBox.confirm('Delete this user?', 'Confirm', { type: 'warning' })
        await axios.delete(`/api/users/${id}`)
        ElMessage.success('User deleted')
        await this.load()
      } catch(_){ }
    }
  },
  async mounted(){
    await this.load()
  }
}
</script>

<style scoped>
.admin-user-management-container {
  padding: 20px;
}

.admin-user-management-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h2 {
  margin: 0;
  color: #303133;
}

.search-filters {
  margin-bottom: 20px;
}

.summary-cards {
  margin-bottom: 20px;
}

.summary-card {
  text-align: center;
}

.summary-content {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 15px;
}

.summary-icon {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: white;
}

.summary-icon.total {
  background-color: #409eff;
}

.summary-icon.active {
  background-color: #67c23a;
}

.summary-text {
  text-align: left;
}

.summary-value {
  font-size: 24px;
  font-weight: bold;
  color: #303133;
}

.summary-label {
  font-size: 14px;
  color: #909399;
}

.table-container {
  width: 100%;
  overflow-x: auto;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.btn-text {
  margin-left: 4px;
}

.dialog-footer {
  text-align: right;
}

.text-muted {
  color: #909399;
  font-style: italic;
}

.devotional-banner {
  width: 100%;
  min-height: 140px;
  border-radius: 8px;
  margin-bottom: 16px;
  background: linear-gradient(135deg, rgba(168,50,26,0.85), rgba(221,146,39,0.85)), var(--devotional-banner-bg), var(--devi-fallback);
  background-size: cover;
  background-position: center;
}

/* Responsive */
@media (max-width: 768px) {
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .search-filters .el-row {
    row-gap: 10px;
  }

  .summary-value {
    font-size: 20px;
  }

  .summary-label {
    font-size: 12px;
  }
  
  .btn-text {
    display: none;
  }
}
</style>
