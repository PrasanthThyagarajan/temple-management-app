<template>
  <div class="categories-container">
    <el-card class="header-card">
      <div class="header-content">
        <div class="header-left">
          <h2>Product Categories</h2>
          <p>Manage product categories for your temple shop</p>
        </div>
        <div class="header-right">
          <el-button type="primary" :icon="Plus" @click="showAddDialog">
            Add Category
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- Devotional Banner -->
    <div class="devotional-banner categories-banner"></div>

    <!-- Summary Cards -->
    <div class="summary-cards">
      <el-card class="summary-card">
        <div class="summary-content">
          <div class="summary-icon total">
            <el-icon><List /></el-icon>
          </div>
          <div class="summary-text">
            <div class="summary-value">{{ categories.length }}</div>
            <div class="summary-label">Total Categories</div>
          </div>
        </div>
      </el-card>
      
      <el-card class="summary-card">
        <div class="summary-content">
          <div class="summary-icon active">
            <el-icon><Check /></el-icon>
          </div>
          <div class="summary-text">
            <div class="summary-value">{{ activeCategories }}</div>
            <div class="summary-label">Active Categories</div>
          </div>
        </div>
      </el-card>
      
      <el-card class="summary-card">
        <div class="summary-content">
          <div class="summary-icon inactive">
            <el-icon><Close /></el-icon>
          </div>
          <div class="summary-text">
            <div class="summary-value">{{ inactiveCategories }}</div>
            <div class="summary-label">Inactive Categories</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- Filters and Search -->
    <el-card class="filter-card">
      <div class="filter-content">
        <div class="filter-left">
          <el-input
            v-model="searchTerm"
            placeholder="Search categories..."
            :prefix-icon="Search"
            @input="handleSearch"
            clearable
          />
          <el-select
            v-model="statusFilter"
            placeholder="Filter by status"
            @change="handleStatusFilter"
            clearable
          >
            <el-option label="Active" value="active" />
            <el-option label="Inactive" value="inactive" />
          </el-select>
        </div>
        <div class="filter-right">
          <el-button :icon="Refresh" @click="loadCategories">
            Refresh
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- Categories Table -->
    <el-card class="table-card">
      <div class="table-container">
        <el-table
          :data="filteredCategories"
          v-loading="loading"
          stripe
          style="width: 100%"
        >
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="name" label="Name" min-width="150" />
        <el-table-column prop="description" label="Description" min-width="200" show-overflow-tooltip />
        <el-table-column prop="sortOrder" label="Sort Order" width="120" align="center" />
        <el-table-column label="Status" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'">
              {{ row.isActive ? 'Active' : 'Inactive' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="Created" width="120">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="200" align="center">
          <template #default="{ row }">
            <el-button
              type="primary"
              size="small"
              :icon="Edit"
              @click="showEditDialog(row)"
            >
              <span class="btn-text">Edit</span>
            </el-button>
            <el-button
              :type="row.isActive ? 'warning' : 'success'"
              size="small"
              :icon="Switch"
              @click="toggleStatus(row)"
            >
              <span class="btn-text">{{ row.isActive ? 'Deactivate' : 'Activate' }}</span>
            </el-button>
            <el-button
              type="danger"
              size="small"
              :icon="Delete"
              @click="confirmDelete(row)"
            >
              <span class="btn-text">Delete</span>
            </el-button>
          </template>
        </el-table-column>
        </el-table>
      </div>
    </el-card>

    <!-- Add/Edit Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? 'Edit Category' : 'Add Category'"
      width="500px"
      @close="resetForm"
    >
      <el-form
        ref="categoryFormRef"
        :model="categoryForm"
        :rules="formRules"
        label-width="120px"
      >
        <el-form-item label="Name" prop="name">
          <el-input v-model="categoryForm.name" placeholder="Enter category name" />
        </el-form-item>
        
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="categoryForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter category description"
          />
        </el-form-item>
        
        <el-form-item label="Sort Order" prop="sortOrder">
          <el-input-number
            v-model="categoryForm.sortOrder"
            :min="0"
            :max="999"
            placeholder="Sort order"
          />
        </el-form-item>
        
        <el-form-item label="Status" prop="isActive">
          <el-switch
            v-model="categoryForm.isActive"
            active-text="Active"
            inactive-text="Inactive"
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">Cancel</el-button>
          <el-button type="primary" @click="saveCategory" :loading="saving">
            {{ isEditing ? 'Update' : 'Create' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, List, Check, Close, Switch } from '@element-plus/icons-vue'
import axios from 'axios'

// API Configuration
const API_BASE = 'http://localhost:5051/api'

// Reactive data
const categories = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const statusFilter = ref('')
const dialogVisible = ref(false)
const isEditing = ref(false)
const editingId = ref(null)

// Form data
const categoryForm = reactive({
  name: '',
  description: '',
  sortOrder: 0,
  isActive: true
})

// Form validation rules
const formRules = {
  name: [
    { required: true, message: 'Please enter category name', trigger: 'blur' },
    { min: 2, max: 100, message: 'Name must be between 2 and 100 characters', trigger: 'blur' }
  ],
  description: [
    { max: 500, message: 'Description cannot exceed 500 characters', trigger: 'blur' }
  ],
  sortOrder: [
    { type: 'number', min: 0, max: 999, message: 'Sort order must be between 0 and 999', trigger: 'blur' }
  ]
}

// Computed properties
const filteredCategories = computed(() => {
  let filtered = categories.value

  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    filtered = filtered.filter(category =>
      category.name.toLowerCase().includes(term) ||
      category.description.toLowerCase().includes(term)
    )
  }

  if (statusFilter.value) {
    filtered = filtered.filter(category => {
      if (statusFilter.value === 'active') return category.isActive
      if (statusFilter.value === 'inactive') return !category.isActive
      return true
    })
  }

  return filtered
})

const activeCategories = computed(() => {
  return categories.value.filter(category => category.isActive).length
})

const inactiveCategories = computed(() => {
  return categories.value.filter(category => !category.isActive).length
})

// Methods
const loadCategories = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/categories`)
    categories.value = response.data
  } catch (error) {
    console.error('Error loading categories:', error)
    ElMessage.error('Failed to load categories')
  } finally {
    loading.value = false
  }
}

const showAddDialog = () => {
  isEditing.value = false
  editingId.value = null
  resetForm()
  dialogVisible.value = true
}

const showEditDialog = (category) => {
  isEditing.value = true
  editingId.value = category.id
  Object.assign(categoryForm, {
    name: category.name,
    description: category.description,
    sortOrder: category.sortOrder,
    isActive: category.isActive
  })
  dialogVisible.value = true
}

const saveCategory = async () => {
  try {
    saving.value = true
    
    if (isEditing.value) {
      await axios.put(`${API_BASE}/categories/${editingId.value}`, categoryForm)
      ElMessage.success('Category updated successfully')
    } else {
      await axios.post(`${API_BASE}/categories`, categoryForm)
      ElMessage.success('Category created successfully')
    }
    
    dialogVisible.value = false
    await loadCategories()
  } catch (error) {
    console.error('Error saving category:', error)
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save category')
    }
  } finally {
    saving.value = false
  }
}

const toggleStatus = async (category) => {
  try {
    await axios.put(`${API_BASE}/categories/${category.id}/toggle-status`)
    ElMessage.success(`Category ${category.isActive ? 'deactivated' : 'activated'} successfully`)
    await loadCategories()
  } catch (error) {
    console.error('Error toggling category status:', error)
    ElMessage.error('Failed to update category status')
  }
}

const confirmDelete = async (category) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete the category "${category.name}"? This action cannot be undone.`,
      'Confirm Delete',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    await axios.delete(`${API_BASE}/categories/${category.id}`)
    ElMessage.success('Category deleted successfully')
    await loadCategories()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting category:', error)
      ElMessage.error('Failed to delete category')
    }
  }
}

const handleSearch = () => {
  // Search is handled by computed property
}

const handleStatusFilter = () => {
  // Filter is handled by computed property
}

const resetForm = () => {
  Object.assign(categoryForm, {
    name: '',
    description: '',
    sortOrder: 0,
    isActive: true
  })
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString()
}

// Lifecycle
onMounted(() => {
  loadCategories()
})
</script>

<style scoped>
.categories-container {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.header-card {
  margin-bottom: 20px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-left h2 {
  margin: 0 0 5px 0;
  color: #303133;
}

.header-left p {
  margin: 0;
  color: #606266;
  font-size: 14px;
}

.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.summary-card {
  border: none;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.summary-content {
  display: flex;
  align-items: center;
  padding: 10px 0;
}

.summary-icon {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 15px;
  font-size: 20px;
  color: white;
}

.summary-icon.total {
  background-color: #409eff;
}

.summary-icon.active {
  background-color: #67c23a;
}

.summary-icon.inactive {
  background-color: #f56c6c;
}

.summary-text {
  flex: 1;
}

.summary-value {
  font-size: 24px;
  font-weight: bold;
  color: #303133;
  line-height: 1;
}

.summary-label {
  font-size: 14px;
  color: #909399;
  margin-top: 5px;
}

.filter-card {
  margin-bottom: 20px;
}

.filter-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.filter-left {
  display: flex;
  align-items: center;
}

.table-card {
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.table-container {
  width: 100%;
  overflow-x: auto;
}

.dialog-footer {
  text-align: right;
}

.dialog-footer .el-button {
  margin-left: 10px;
}

/* Responsive */
@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .filter-content {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .filter-left {
    flex-direction: column;
    gap: 10px;
  }

  .filter-left .el-input,
  .filter-left .el-select {
    width: 100% !important;
  }

  .summary-cards {
    grid-template-columns: 1fr 1fr;
  }

  .table-card .el-button .btn-text {
    display: none;
  }
}

@media (max-width: 480px) {
  .summary-cards {
    grid-template-columns: 1fr;
  }
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
</style>
