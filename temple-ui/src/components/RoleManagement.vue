<template>
  <div class="role-management-container">
    <el-card class="role-management-card">
      <template #header>
        <div class="card-header">
          <h2>Role Management</h2>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>
            Add Role
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner roles-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search roles..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-button type="info" @click="fetchRoles" title="Refresh data">
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
                  <div class="summary-value">{{ filteredRoles.length }}</div>
                  <div class="summary-label">Total Roles</div>
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
                  <div class="summary-value">{{ roles.length }}</div>
                  <div class="summary-label">Active Roles</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Empty State -->
      <el-empty v-if="!loading && roles.length === 0" description="No roles found">
        <template #default>
          <el-button type="primary" @click="openCreate">Create your first role</el-button>
        </template>
      </el-empty>

      <!-- Table -->
      <div v-else class="table-container">
        <el-table 
          :data="filteredRoles" 
          v-loading="loading" 
          stripe 
          style="width: 100%"
        >
          <el-table-column prop="roleName" label="Role Name" min-width="200" sortable>
            <template #default="scope">
              <strong>{{ scope.row.roleName }}</strong>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="Description" min-width="300">
            <template #default="scope">
              <span v-if="scope.row.description">{{ scope.row.description }}</span>
              <span v-else class="text-muted">No description</span>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <el-button size="small" @click="openEdit(scope.row)">
                <el-icon><Edit /></el-icon>
                Edit
              </el-button>
              <el-button size="small" type="danger" @click="confirmDelete(scope.row)">
                <el-icon><Delete /></el-icon>
                Delete
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-card>

    <el-dialog v-model="dialog.visible" :title="dialog.mode === 'create' ? 'Create Role' : 'Edit Role'" width="480px">
      <el-form :model="form" :rules="rules" ref="formRef" label-width="110px">
        <el-form-item label="Role Name" prop="roleName">
          <el-input v-model="form.roleName" placeholder="Enter role name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input v-model="form.description" placeholder="Enter description" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="closeDialog">Cancel</el-button>
        <el-button type="primary" :loading="saving" @click="submitDialog">
          {{ dialog.mode === 'create' ? 'Create' : 'Save' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
  </template>

<script>
import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete, UserFilled, Check, Search, Refresh } from '@element-plus/icons-vue'

export default {
  components: { Plus, Edit, Delete, UserFilled, Check, Search, Refresh },
  data() {
    return {
      loading: false,
      saving: false,
      roles: [],
      searchTerm: '',
      form: { roleId: 0, roleName: '', description: '' },
      dialog: { visible: false, mode: 'create' },
      rules: {
        roleName: [
          { required: true, message: 'Role name is required', trigger: 'blur' },
          { min: 2, max: 50, message: '2-50 characters', trigger: 'blur' }
        ],
        description: [
          { max: 200, message: 'Up to 200 characters', trigger: 'blur' }
        ]
      }
    };
  },
  computed: {
    filteredRoles() {
      if (!this.searchTerm) return this.roles
      const term = this.searchTerm.toLowerCase()
      return this.roles.filter(role => 
        role.roleName?.toLowerCase().includes(term) ||
        role.description?.toLowerCase().includes(term)
      )
    }
  },
  methods: {
    handleSearch() {
      // Search is handled by computed property
    },
    async fetchRoles() {
      this.loading = true
      try {
        const { data } = await axios.get('/api/roles')
        this.roles = data
      } catch (e) {
        ElMessage.error('Failed to load roles')
      } finally {
        this.loading = false
      }
    },
    openCreate() {
      this.dialog.mode = 'create'
      this.dialog.visible = true
      this.form = { roleId: 0, roleName: '', description: '' }
    },
    openEdit(role) {
      this.dialog.mode = 'edit'
      this.dialog.visible = true
      this.form = { roleId: role.roleId, roleName: role.roleName, description: role.description || '' }
    },
    closeDialog() {
      this.dialog.visible = false
    },
    async submitDialog() {
      if (!this.form.roleName) return
      this.saving = true
      try {
        if (this.dialog.mode === 'create') {
          await axios.post('/api/roles', { roleName: this.form.roleName, description: this.form.description })
          ElMessage.success('Role created')
        } else {
          await axios.put(`/api/roles/${this.form.roleId}`, { roleId: this.form.roleId, roleName: this.form.roleName, description: this.form.description })
          ElMessage.success('Role updated')
        }
        this.dialog.visible = false
        await this.fetchRoles()
      } catch (e) {
        ElMessage.error('Save failed')
      } finally {
        this.saving = false
      }
    },
    async confirmDelete(role) {
      try {
        await ElMessageBox.confirm(`Delete role "${role.roleName}"?`, 'Confirm', { type: 'warning' })
        await axios.delete(`/api/roles/${role.roleId}`)
        ElMessage.success('Role deleted')
        await this.fetchRoles()
      } catch (_) {}
    }
  },
  mounted() {
    this.fetchRoles();
  }
};
</script>

<style scoped>
.role-management-container {
  padding: 20px;
}

.role-management-card {
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

