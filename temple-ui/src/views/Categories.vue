<template>
  <div class="categories-container">
    <!-- Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="8">
        <el-card class="summary-card">
          <el-statistic title="Total Categories" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Collection /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="8">
        <el-card class="summary-card active">
          <el-statistic title="Active Categories" :value="summaryStats.active">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><Check /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="8">
        <el-card class="summary-card">
          <el-statistic title="Inactive Categories" :value="summaryStats.inactive">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #f56c6c;"><Close /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="categories-card">
      <template #header>
        <div class="card-header">
          <h2>Category Management</h2>
          <el-button type="primary" @click="showAddDialog">
            <el-icon><Plus /></el-icon>
            Add Category
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner categories-banner"></div>

      <!-- Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search categories..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4">
            <el-select
              v-model="statusFilter"
              placeholder="Filter by Status"
              clearable
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Status" value="" />
              <el-option label="Active" value="active" />
              <el-option label="Inactive" value="inactive" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4">
            <el-select
              v-model="sortBy"
              placeholder="Sort by"
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
              <el-option label="Sort Order" value="sortOrder-asc" />
              <el-option label="Recently Added" value="id-desc" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="24" :md="8">
            <div class="action-buttons">
              <el-button @click="refreshCategories" :loading="loading" class="refresh-btn">
                <el-icon><Refresh /></el-icon>
                <span class="btn-text">Refresh</span>
              </el-button>
            </div>
          </el-col>
        </el-row>
      </div>

      <!-- Table View -->
      <div class="table-container">
        <el-table
          :data="paginatedCategories"
          v-loading="loading"
          stripe
          style="width: 100%"
          @sort-change="handleTableSortChange"
        >
          <el-table-column prop="id" label="ID" width="80" sortable="custom" />
          <el-table-column 
            prop="name" 
            label="Category Name" 
            min-width="200" 
            sortable="custom"
          >
            <template #default="{ row }">
              <strong>{{ row.name }}</strong>
            </template>
          </el-table-column>
          <el-table-column 
            prop="description" 
            label="Description" 
            min-width="300" 
            show-overflow-tooltip 
          >
            <template #default="{ row }">
              <span v-if="row.description">{{ row.description }}</span>
              <span v-else style="color: #909399; font-style: italic;">No description</span>
            </template>
          </el-table-column>
          <el-table-column 
            prop="sortOrder" 
            label="Sort Order" 
            width="120" 
            align="center" 
            sortable="custom"
          />
          <el-table-column label="Status" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'danger'">
                {{ row.isActive ? 'Active' : 'Inactive' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="createdAt" label="Created Date" width="160" sortable="custom">
            <template #default="{ row }">
              {{ formatDate(row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="180" align="center">
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

      <!-- Pagination -->
      <div class="pagination-container">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="filteredBeforePagination.length"
          :background="true"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Add/Edit Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? 'Edit Category' : 'Add Category'"
      :width="dialogWidth"
      :fullscreen="isMobileDialog"
      @close="resetForm"
    >
      <el-form :model="categoryForm" :rules="rules" ref="categoryFormRef" :label-width="isMobileDialog ? '100px' : '120px'">
        <el-form-item label="Category Name" prop="name">
          <el-input v-model="categoryForm.name" placeholder="Enter category name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="categoryForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter category description (optional)"
          />
        </el-form-item>
        <el-form-item label="Sort Order" prop="sortOrder">
          <el-input-number
            v-model="categoryForm.sortOrder"
            :min="0"
            :max="9999"
            controls-position="right"
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
          <el-button type="primary" @click="submitForm" :loading="submitting">
            {{ isEditing ? 'Update' : 'Create' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Collection, Check, Close, Switch, Filter } from '@element-plus/icons-vue'
import axios from 'axios'

// API Configuration
const API_BASE = '/api'

// Reactive data
const categories = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const currentCategory = ref(null)

// Responsive properties
const windowWidth = ref(window.innerWidth)
const isMobileDialog = computed(() => windowWidth.value < 768)
const dialogWidth = computed(() => {
  if (windowWidth.value < 576) return '100%'
  if (windowWidth.value < 768) return '95%'
  if (windowWidth.value < 992) return '90%'
  return '500px'
})

// Form data
const categoryForm = reactive({
  name: '',
  description: '',
  sortOrder: 0,
  isActive: true
})

// Form validation rules
const rules = {
  name: [
    { required: true, message: 'Please enter category name', trigger: 'blur' },
    { min: 2, max: 100, message: 'Length should be between 2 to 100', trigger: 'blur' }
  ],
  sortOrder: [
    { required: true, message: 'Please enter sort order', trigger: 'blur' }
  ]
}

// Filter and pagination
const searchTerm = ref('')
const statusFilter = ref('')
const sortBy = ref('name-asc')
const currentPage = ref(1)
const pageSize = ref(10)
const categoryFormRef = ref(null)

// Computed properties
const summaryStats = computed(() => {
  return {
    total: categories.value.length,
    active: categories.value.filter(c => c.isActive).length,
    inactive: categories.value.filter(c => !c.isActive).length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...categories.value]

  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(c =>
      c.name?.toLowerCase().includes(term) ||
      c.description?.toLowerCase().includes(term)
    )
  }

  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(c => {
      if (statusFilter.value === 'active') return c.isActive
      if (statusFilter.value === 'inactive') return !c.isActive
      return true
    })
  }

  // Apply sorting
  const [field, order] = sortBy.value.split('-')
  result.sort((a, b) => {
    let aVal = a[field]
    let bVal = b[field]
    
    if (typeof aVal === 'string') {
      aVal = aVal.toLowerCase()
      bVal = bVal.toLowerCase()
    }
    
    if (order === 'asc') {
      return aVal > bVal ? 1 : -1
    } else {
      return aVal < bVal ? 1 : -1
    }
  })

  return result
})

const paginatedCategories = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredBeforePagination.value.slice(start, end)
})

// Methods
const loadCategories = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${API_BASE}/categories`)
    categories.value = response.data
  } catch (error) {
    console.error('Error loading categories:', error)
    ElMessage.error('Failed to load categories')
  } finally {
    loading.value = false
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: '2-digit'
  })
}

const handleSearch = () => {
  currentPage.value = 1
}

const handleFilterChange = () => {
  currentPage.value = 1
}

const handleSortChange = () => {
  currentPage.value = 1
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'name-asc'
  } else {
    const orderStr = order === 'ascending' ? 'asc' : 'desc'
    sortBy.value = `${prop}-${orderStr}`
  }
}

const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

const showAddDialog = () => {
  isEditing.value = false
  resetForm()
  dialogVisible.value = true
}

const showEditDialog = (row) => {
  isEditing.value = true
  currentCategory.value = row
  Object.assign(categoryForm, {
    name: row.name,
    description: row.description || '',
    sortOrder: row.sortOrder,
    isActive: row.isActive
  })
  dialogVisible.value = true
}

const resetForm = () => {
  if (categoryFormRef.value) {
    categoryFormRef.value.resetFields()
  }
  Object.assign(categoryForm, {
    name: '',
    description: '',
    sortOrder: 0,
    isActive: true
  })
  currentCategory.value = null
}

const submitForm = async () => {
  const valid = await categoryFormRef.value.validate()
  if (!valid) return

  submitting.value = true
  try {
    const payload = {
      name: categoryForm.name,
      description: categoryForm.description || null,
      sortOrder: categoryForm.sortOrder,
      isActive: categoryForm.isActive
    }

    if (isEditing.value && currentCategory.value) {
      await axios.put(`${API_BASE}/categories/${currentCategory.value.id}`, payload)
      ElMessage.success('Category updated successfully')
    } else {
      await axios.post(`${API_BASE}/categories`, payload)
      ElMessage.success('Category created successfully')
    }

    dialogVisible.value = false
    await loadCategories()
  } catch (error) {
    console.error('Error saving category:', error)
    ElMessage.error('Failed to save category')
  } finally {
    submitting.value = false
  }
}

const toggleStatus = async (row) => {
  try {
    await axios.put(`${API_BASE}/categories/${row.id}`, {
      ...row,
      isActive: !row.isActive
    })
    ElMessage.success(`Category ${!row.isActive ? 'activated' : 'deactivated'} successfully`)
    await loadCategories()
  } catch (error) {
    console.error('Error toggling category status:', error)
    ElMessage.error('Failed to update category status')
  }
}

const confirmDelete = (row) => {
  ElMessageBox.confirm(
    `Are you sure you want to delete "${row.name}"? This action cannot be undone.`,
    'Delete Category',
    {
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      type: 'warning',
      confirmButtonClass: 'el-button--danger'
    }
  ).then(async () => {
    try {
      await axios.delete(`${API_BASE}/categories/${row.id}`)
      ElMessage.success('Category deleted successfully')
      await loadCategories()
    } catch (error) {
      console.error('Error deleting category:', error)
      ElMessage.error('Failed to delete category')
    }
  }).catch(() => {
    // User cancelled the operation
  })
}

const refreshCategories = () => {
  loadCategories()
}

// Window resize handler
const handleResize = () => {
  windowWidth.value = window.innerWidth
}

// Lifecycle
onMounted(() => {
  loadCategories()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.categories-container {
  padding: 20px;
}

.summary-cards {
  margin-bottom: 20px;
}

.summary-card {
  transition: all 0.3s;
}

.summary-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 12px 0 rgba(0, 0, 0, 0.15);
}

.summary-card .el-statistic {
  padding: 20px 0;
}

.summary-card.active {
  border-left: 4px solid #67c23a;
}

.categories-card {
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

.search-filters .el-row {
  row-gap: 10px;
}

.table-container {
  overflow-x: auto;
  margin-bottom: 20px;
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

.action-buttons {
  display: flex;
  gap: 5px;
  flex-wrap: nowrap;
  align-items: center;
}

.btn-text {
  display: inline;
}

.refresh-btn {
  flex: 1;
  min-width: 0;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.dialog-footer {
  text-align: right;
}

@media (max-width: 768px) {
  .categories-container {
    padding: 10px;
  }
  
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .card-header h2 {
    font-size: 20px;
  }
  
  .search-filters {
    margin-bottom: 15px;
  }
  
  .action-buttons {
    flex-direction: row;
    flex-wrap: wrap;
    gap: 8px;
  }
  
  .action-buttons .el-button {
    flex: 1 1 48%;
    min-width: 140px;
    font-size: 12px;
  }
  
  .btn-text {
    display: none;
  }
  
  .refresh-btn .btn-text {
    display: inline;
  }
  
  .summary-cards {
    margin-bottom: 15px;
  }
  
  .summary-card {
    margin-bottom: 10px;
  }
  
  .pagination-container {
    text-align: center;
  }
  
  .pagination-container .el-pagination {
    justify-content: center;
  }

  /* Dialog responsive styles */
  .el-dialog__body {
    padding: 10px 15px;
    max-height: calc(100vh - 180px);
    overflow-y: auto;
  }

  /* Make dialog scrollable on mobile */
  .el-dialog.is-fullscreen .el-dialog__body {
    max-height: calc(100vh - 120px);
    overflow-y: auto;
  }

  .el-form-item__label {
    font-size: 13px;
  }

  /* Adjust form label width for mobile */
  .el-dialog .el-form .el-form-item__label {
    width: 100px !important;
  }
}

@media (max-width: 480px) {
  .categories-container {
    padding: 5px;
  }
  
  .card-header h2 {
    font-size: 18px;
  }
  
  .search-filters {
    margin-bottom: 10px;
  }
  
  .action-buttons .el-button {
    font-size: 11px;
    padding: 5px 8px;
  }
  
  .summary-card {
    margin-bottom: 10px;
  }
  
  .pagination-container .el-pagination {
    font-size: 12px;
  }
  
  .pagination-container .el-pagination .el-pager li {
    min-width: 28px;
    height: 28px;
    line-height: 28px;
  }

  /* Extra small screen dialog adjustments */
  .el-dialog .el-form .el-form-item__label {
    width: 80px !important;
    font-size: 12px;
    text-align: left !important;
  }

  /* Dialog footer responsive */
  .dialog-footer {
    display: flex;
    gap: 10px;
    justify-content: flex-end;
    flex-wrap: wrap;
  }

  .dialog-footer .el-button {
    flex: 1;
    min-width: 80px;
  }
}

@media (min-width: 769px) and (max-width: 1024px) {
  .categories-container {
    padding: 15px;
  }
  
  .action-buttons .el-button {
    font-size: 13px;
  }
}
</style>