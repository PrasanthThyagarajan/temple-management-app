<template>
  <div class="role-permissions-container">
    <el-card class="role-permissions-card">
      <template #header>
        <div class="card-header">
          <h2>System Permissions</h2>
          <p style="color: #909399; font-size: 14px; margin: 5px 0 0 0;">View all available system permissions (Admin Only)</p>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner permissions-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search permissions..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-button type="info" @click="fetchPermissions" title="Refresh data">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Summary Cards -->
      <div class="summary-cards" v-if="!loading">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><Key /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ permissions.length }}</div>
                  <div class="summary-label">Total Permissions</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon active">
                  <el-icon><Filter /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ filteredPermissions.length }}</div>
                  <div class="summary-label">Filtered Results</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-container">
        <el-skeleton :rows="5" animated />
      </div>

      <!-- Permissions Table -->
      <div v-else class="table-container">
        <el-table 
          :data="filteredPermissions" 
          stripe 
          style="width: 100%"
        >
          <el-table-column prop="permissionId" label="ID" width="80" sortable />
          <el-table-column prop="permissionName" label="Permission Name" min-width="250" sortable>
            <template #default="scope">
              <strong>{{ scope.row.permissionName }}</strong>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="Description" min-width="400">
            <template #default="scope">
              <span v-if="scope.row.description">{{ scope.row.description }}</span>
              <span v-else class="text-muted">No description</span>
            </template>
          </el-table-column>
          <el-table-column label="System Permission" width="150" align="center">
            <template #default="scope">
              <el-tag type="primary" size="small">
                Active
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-card>
  </div>
</template>

<script>
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'
import { useAuth } from '@/stores/auth'
import { Key, Filter, Search, Refresh } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

export default {
  name: 'RolePermissions',
  components: { Key, Filter, Search, Refresh },
  setup() {
    const { hasRole } = useAuth()
    const permissions = ref([])
    const loading = ref(false)
    const searchTerm = ref('')

    // Computed property for filtered permissions
    const filteredPermissions = computed(() => {
      if (!searchTerm.value) return permissions.value
      const term = searchTerm.value.toLowerCase()
      return permissions.value.filter(permission => 
        permission.permissionName?.toLowerCase().includes(term) ||
        permission.description?.toLowerCase().includes(term)
      )
    })

    const fetchPermissions = async () => {
      if (!hasRole('Admin')) {
        ElMessage.error('Only Admin can view permissions.')
        return
      }
      
      loading.value = true
      try {
        console.log('Fetching permissions...')
        const response = await axios.get('/api/permissions')
        console.log('Permissions response:', response.data)
        permissions.value = response.data
        ElMessage.success('Permissions loaded successfully')
      } catch (error) {
        console.error('Failed to fetch permissions:', error)
        console.error('Error response:', error.response?.data)
        console.error('Error status:', error.response?.status)
        ElMessage.error(`Failed to load permissions: ${error.response?.data?.message || error.message}`)
      } finally {
        loading.value = false
      }
    }

    const handleSearch = () => {
      // Search is handled by computed property
    }

    onMounted(async () => {
      await fetchPermissions()
    })

    return {
      permissions,
      filteredPermissions,
      loading,
      searchTerm,
      fetchPermissions,
      handleSearch
    }
  }
}
</script>

<style scoped>
.role-permissions-container {
  padding: 20px;
}

.role-permissions-card {
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

.loading-container {
  margin: 20px 0;
}

.message-container {
  margin-top: 20px;
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
