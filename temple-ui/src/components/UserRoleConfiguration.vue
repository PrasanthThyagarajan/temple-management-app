<template>
  <div class="user-role-configuration-container">
    <el-card class="user-role-configuration-card">
      <template #header>
        <div class="card-header">
          <h2>User Role Configuration</h2>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>
            Add User Role
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner user-roles-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search user roles..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-button type="info" @click="fetchUserRoles" title="Refresh data">
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
                  <el-icon><UserFilled /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ filteredUserRoles.length }}</div>
                  <div class="summary-label">Total Assignments</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon active">
                  <el-icon><Avatar /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ uniqueUsers }}</div>
                  <div class="summary-label">Unique Users</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Table -->
      <div class="table-container">
        <el-table 
          :data="filteredUserRoles" 
          v-loading="loading" 
          stripe 
          style="width: 100%"
        >
          <el-table-column prop="user.username" label="Username" min-width="180" sortable>
            <template #default="scope">
              <div>
                <strong>{{ scope.row.user.username }}</strong>
                <br>
                <small class="text-muted">{{ scope.row.user.email }}</small>
              </div>
            </template>
          </el-table-column>
          <el-table-column prop="user.fullName" label="Full Name" min-width="200" sortable>
            <template #default="scope">
              <span v-if="scope.row.user.fullName">{{ scope.row.user.fullName }}</span>
              <span v-else class="text-muted">—</span>
            </template>
          </el-table-column>
          <el-table-column prop="role.roleName" label="Role" min-width="150" sortable>
            <template #default="scope">
              <el-tag type="primary" size="small">
                {{ scope.row.role.roleName }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="role.description" label="Role Description" min-width="250">
            <template #default="scope">
              <span v-if="scope.row.role.description">{{ scope.row.role.description }}</span>
              <span v-else class="text-muted">No description</span>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <el-button size="small" @click="openEdit(scope.row)">
                <el-icon><Edit /></el-icon>
                Edit
              </el-button>
              <el-button 
                size="small" 
                type="danger" 
                @click="confirmDeleteUserRole(scope.row.userRoleId)"
              >
                <el-icon><Delete /></el-icon>
                Delete
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-card>

    <el-dialog v-model="dialog.visible" :title="dialog.mode === 'create' ? 'Add User Role' : 'Edit User Role'" width="520px">
    <el-form :model="form" label-width="120px" :rules="rules" ref="formRef">
      <el-form-item label="User" prop="userId">
        <el-select 
          v-model="form.userId" 
          placeholder="Select User" 
          style="width: 100%"
          filterable
        >
          <el-option 
            v-for="u in users" 
            :key="u.userId" 
            :value="u.userId"
            :label="`${u.username} (${u.email})`"
          >
            <div>
              <strong>{{ u.username }}</strong>
              <br>
              <small style="color: #909399;">{{ u.email }}</small>
            </div>
          </el-option>
        </el-select>
      </el-form-item>

      <el-form-item label="Role" prop="roleId">
        <el-select 
          v-model="form.roleId" 
          placeholder="Select Role" 
          style="width: 100%"
        >
          <el-option 
            v-for="r in roles" 
            :key="r.roleId" 
            :value="r.roleId"
            :label="r.roleName"
          >
            <div>
              <strong>{{ r.roleName }}</strong>
              <br>
              <small v-if="r.description" style="color: #909399;">{{ r.description }}</small>
            </div>
          </el-option>
        </el-select>
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="closeDialog">Cancel</el-button>
      <el-button type="primary" :loading="saving" @click="submitDialog">
        <el-icon><Check /></el-icon>
        {{ dialog.mode === 'create' ? 'Assign' : 'Update' }}
      </el-button>
    </template>
    </el-dialog>
  </div>
</template>

<script>
import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete, UserFilled, Avatar, Search, Refresh, Check, Close } from '@element-plus/icons-vue'

export default {
  components: { Plus, Edit, Delete, UserFilled, Avatar, Search, Refresh, Check, Close },
  data() {
    return {
      userRoles: [],
      form: { userRoleId: 0, userId: '', roleId: '' },
      users: [],
      roles: [],
      loading: false,
      saving: false,
      dialog: { visible: false, mode: 'create' },
      searchTerm: '',
      rules: {
        userId: [
          { required: true, message: 'Please select a user', trigger: 'change' }
        ],
        roleId: [
          { required: true, message: 'Please select a role', trigger: 'change' }
        ]
      }
    };
  },
  computed: {
    filteredUserRoles() {
      if (!this.searchTerm) return this.userRoles
      const term = this.searchTerm.toLowerCase()
      return this.userRoles.filter(userRole => 
        userRole.user.username?.toLowerCase().includes(term) ||
        userRole.user.email?.toLowerCase().includes(term) ||
        userRole.user.fullName?.toLowerCase().includes(term) ||
        userRole.role.roleName?.toLowerCase().includes(term)
      )
    },
    uniqueUsers() {
      const uniqueUserIds = new Set(this.userRoles.map(ur => ur.user.userId))
      return uniqueUserIds.size
    }
  },
  methods: {
    handleSearch() {
      // Search is handled by computed property
    },
    openCreate() {
      this.dialog.mode = 'create'
      this.dialog.visible = true
      this.form = { userRoleId: 0, userId: '', roleId: '' }
      this.loadUsersAndRoles()
    },
    openEdit(userRole) {
      this.dialog.mode = 'edit'
      this.dialog.visible = true
      this.form = { 
        userRoleId: userRole.userRoleId, 
        userId: userRole.user.userId, 
        roleId: userRole.role.roleId 
      }
      this.loadUsersAndRoles()
    },
    closeDialog() {
      this.dialog.visible = false
      if (this.$refs.formRef) {
        this.$refs.formRef.clearValidate()
      }
    },
    async fetchUserRoles() {
      this.loading = true
      try {
        const { data } = await axios.get('/api/userroles')
        this.userRoles = data
      } catch (error) {
        console.error('Failed to load user roles:', error)
        ElMessage.error('Failed to load user roles')
      } finally {
        this.loading = false
      }
    },
    async loadUsersAndRoles(){
      try {
        const [usersRes, rolesRes] = await Promise.all([
          axios.get('/api/users'),
          axios.get('/api/roles')
        ])
        this.users = usersRes.data
        this.roles = rolesRes.data
      } catch (error) {
        console.error('Failed to load users and roles:', error)
        ElMessage.error('Failed to load users and roles')
      }
    },
    async submitDialog() {
      if (!this.$refs.formRef) return
      this.$refs.formRef.validate(async (valid) => {
        if (!valid) return
        this.saving = true
        try {
          if (this.dialog.mode === 'edit') {
            await this.saveEdit()
          } else {
            await this.createUserRole()
          }
        } finally {
          this.saving = false
        }
      })
    },
    async createUserRole() {
      if (!this.form.userId || !this.form.roleId) return
      
      try {
        await axios.post('/api/userroles', { 
          userId: Number(this.form.userId), 
          roleId: Number(this.form.roleId) 
        })
        ElMessage.success('User role assigned successfully')
        this.closeDialog()
        await this.fetchUserRoles()
      } catch (error) {
        console.error('Failed to create user role:', error)
        ElMessage.error('Failed to assign user role')
      }
    },
    async saveEdit() {
      try {
        await axios.put(`/api/userroles/${this.form.userRoleId}`, { 
          userRoleId: this.form.userRoleId, 
          userId: Number(this.form.userId), 
          roleId: Number(this.form.roleId) 
        })
        ElMessage.success('User role updated successfully')
        this.closeDialog()
        await this.fetchUserRoles()
      } catch (error) {
        console.error('Failed to update user role:', error)
        ElMessage.error('Failed to update user role')
      }
    },
    async confirmDeleteUserRole(userRoleId) {
      try {
        await ElMessageBox.confirm(
          'This will permanently delete the user role assignment. Continue?',
          'Warning',
          {
            confirmButtonText: 'OK',
            cancelButtonText: 'Cancel',
            type: 'warning',
          }
        )
        await this.deleteUserRole(userRoleId)
      } catch {
        // User cancelled
      }
    },
    async deleteUserRole(userRoleId) {
      try {
        await axios.delete(`/api/userroles/${userRoleId}`)
        ElMessage.success('User role deleted successfully')
        await this.fetchUserRoles()
      } catch (error) {
        console.error('Failed to delete user role:', error)
        ElMessage.error('Failed to delete user role')
      }
    }
  },
  async mounted() {
    await Promise.all([this.fetchUserRoles(), this.loadUsersAndRoles()])
  }
};
</script>

<style scoped>
.user-role-configuration-container {
  padding: 20px;
}

.user-role-configuration-card {
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

.card-header h3 {
  margin: 0;
  color: #303133;
  font-size: 18px;
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

.form-card {
  border: 2px dashed #409eff;
  background-color: #f8f9fa;
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
}
</style>

