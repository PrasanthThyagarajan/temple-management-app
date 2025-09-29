<template>
  <div class="temples-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Temples" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><OfficeBuilding /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card active">
          <el-statistic title="Active Temples" :value="summaryStats.active">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><House /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="States Covered" :value="summaryStats.states">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><Location /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Filtered Results" :value="filteredBeforePagination.length">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #e6a23c;"><Filter /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="temples-card">
      <template #header>
        <div class="card-header">
          <h2>Temple Management</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Temple
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner temples-banner"></div>

      <!-- Enhanced Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search temples..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-select
              v-model="statusFilter"
              placeholder="Filter by Status"
              clearable
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Status" value="" />
              <el-option label="Active" value="Active" />
              <el-option label="Inactive" value="Inactive" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-select
              v-model="sortBy"
              placeholder="Sort by"
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
              <el-option label="Recently Added" value="id-desc" />
              <el-option label="Oldest First" value="id-asc" />
              <el-option label="Established (Newest)" value="year-desc" />
              <el-option label="Established (Oldest)" value="year-asc" />
            </el-select>
          </el-col>
        </el-row>
      </div>

      <!-- Temples Table -->
      <div class="table-container">
        <el-table
          :data="paginatedTemples"
          v-loading="loading"
          stripe
          style="width: 100%"
          @sort-change="handleTableSortChange"
        >
          <el-table-column 
            prop="name" 
            label="Temple Name" 
            min-width="200" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column 
            prop="city" 
            label="City" 
            width="120" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column 
            prop="state" 
            label="State" 
            width="120" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column 
            prop="establishedYear" 
            label="Established" 
            width="100" 
            sortable="custom"
          />
          <el-table-column 
            prop="deity" 
            label="Main Deity" 
            width="150" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column label="Actions" width="200" fixed="right" v-if="canUpdate || canDelete">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editTemple(scope.row)" v-if="canUpdate">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteTemple(scope.row.id)"
                  v-if="canDelete"
                >
                  <el-icon><Delete /></el-icon>
                  <span class="btn-text">Delete</span>
                </el-button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <!-- Enhanced Pagination -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="filteredBeforePagination.length"
          :background="true"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Create/Edit Temple Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingTemple ? 'Edit Temple' : 'Add New Temple'"
      :width="dialogWidth"
      :fullscreen="isMobile"
      :close-on-click-modal="false"
    >
      <el-form
        ref="templeFormRef"
        :model="templeForm"
        :rules="templeRules"
        label-width="120px"
      >
        <el-form-item label="Temple Name" prop="name">
          <el-input v-model="templeForm.name" placeholder="Enter temple name" />
        </el-form-item>
        <el-form-item label="City" prop="city">
          <el-input v-model="templeForm.city" placeholder="Enter city" />
        </el-form-item>
        <el-form-item label="State" prop="state">
          <el-input v-model="templeForm.state" placeholder="Enter state" />
        </el-form-item>
        <el-form-item label="Address" prop="address">
          <el-input
            v-model="templeForm.address"
            type="textarea"
            :rows="3"
            placeholder="Enter full address"
          />
        </el-form-item>
        <el-form-item label="Established Year" prop="establishedYear">
          <el-date-picker
            v-model="templeForm.establishedYear"
            type="year"
            placeholder="Select year"
            format="YYYY"
            value-format="YYYY"
          />
        </el-form-item>
        <el-form-item label="Main Deity" prop="deity">
          <el-input v-model="templeForm.deity" placeholder="Enter main deity" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="templeForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter temple description"
          />
        </el-form-item>
        <el-form-item label="Status" prop="status">
          <el-select v-model="templeForm.status" placeholder="Select status">
            <el-option label="Active" value="Active" />
            <el-option label="Inactive" value="Inactive" />
            <el-option label="Under Construction" value="Under Construction" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveTemple" :loading="saving" v-if="canCreate || canUpdate">
            {{ editingTemple ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, OfficeBuilding, House, Location, Filter } from '@element-plus/icons-vue'
import axios from 'axios'

// Reactive data
const temples = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('name-asc')
const showCreateDialog = ref(false)
const editingTemple = ref(null)

// Form data
const templeForm = reactive({
  name: '',
  city: '',
  state: '',
  address: '',
  establishedYear: '',
  deity: '',
  description: ''
})

// Form validation rules
const templeRules = {
  name: [{ required: true, message: 'Temple name is required', trigger: 'blur' }],
  city: [{ required: true, message: 'City is required', trigger: 'blur' }],
  state: [{ required: true, message: 'State is required', trigger: 'blur' }],
  address: [{ required: true, message: 'Address is required', trigger: 'blur' }],
  establishedYear: [{ required: true, message: 'Established year is required', trigger: 'change' }],
  deity: [{ required: true, message: 'Main deity is required', trigger: 'blur' }],
}

const templeFormRef = ref()

// Responsive data
const isMobile = ref(false)
const dialogWidth = ref('600px')

// API base URL
const API_BASE = '/api'

// Permission states
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

// Summary Statistics
const summaryStats = computed(() => {
  return {
    total: temples.value.length,
    states: [...new Set(temples.value.map(t => t.state).filter(Boolean))].length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...temples.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(t =>
      t.name?.toLowerCase().includes(term) ||
      t.city?.toLowerCase().includes(term) ||
      t.state?.toLowerCase().includes(term) ||
      t.deity?.toLowerCase().includes(term) ||
      t.address?.toLowerCase().includes(term)
    )
  }
  
  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(t => t.status === statusFilter.value)
  }
  
  // Apply sorting
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      
      switch(field) {
        case 'name':
          aVal = (a.name || '').toLowerCase()
          bVal = (b.name || '').toLowerCase()
          break
        case 'year':
          aVal = parseInt(a.establishedYear) || 0
          bVal = parseInt(b.establishedYear) || 0
          break
        case 'id':
          aVal = a.id
          bVal = b.id
          break
        default:
          aVal = a[field]
          bVal = b[field]
      }
      
      if (order === 'asc') {
        return aVal < bVal ? -1 : aVal > bVal ? 1 : 0
      } else {
        return aVal > bVal ? -1 : aVal < bVal ? 1 : 0
      }
    })
  }
  
  return result
})

// Paginated data
const paginatedTemples = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

// Responsive methods
const checkScreenSize = () => {
  isMobile.value = window.innerWidth <= 768
  dialogWidth.value = isMobile.value ? '95%' : '600px'
}

const handleResize = () => {
  checkScreenSize()
}

// Methods
const loadTemples = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/temples`)
    temples.value = response.data
  } catch (error) {
    console.error('Error loading temples:', error)
    ElMessage.error('Failed to load temples')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  currentPage.value = 1
}

const handleFilterChange = () => {
  currentPage.value = 1
}

const handleSortChange = () => {
  // Sorting doesn't require resetting pagination
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'name-asc'
  } else {
    let field = prop
    if (prop === 'establishedYear') field = 'year'
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const editTemple = (temple) => {
  editingTemple.value = temple
  Object.assign(templeForm, temple)
  showCreateDialog.value = true
}

const saveTemple = async () => {
  try {
    await templeFormRef.value.validate()
    saving.value = true
    
    if (editingTemple.value) {
      // Update existing temple
      await axios.put(`${API_BASE}/temples/${editingTemple.value.id}`, templeForm)
      ElMessage.success('Temple updated successfully')
    } else {
      // Create new temple
      await axios.post(`${API_BASE}/temples`, templeForm)
      ElMessage.success('Temple created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadTemples()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save temple')
    }
  } finally {
    saving.value = false
  }
}

const deleteTemple = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this temple?',
      'Warning',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/temples/${id}`)
    ElMessage.success('Temple deleted successfully')
    loadTemples()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting temple:', error)
      ElMessage.error('Failed to delete temple')
    }
  }
}


const resetForm = () => {
  editingTemple.value = null
  Object.assign(templeForm, {
    name: '',
    city: '',
    state: '',
    address: '',
    establishedYear: '',
    deity: '',
    description: '',
  })
  templeFormRef.value?.resetFields()
}

const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

// Lifecycle
onMounted(async () => {
  loadTemples()
  checkScreenSize()
  window.addEventListener('resize', handleResize)
  
  // Check permissions
  if (window["templeAuth"]) {
    canCreate.value = await window["templeAuth"].hasCreatePermission('/temples')
    canUpdate.value = await window["templeAuth"].hasUpdatePermission('/temples')
    canDelete.value = await window["templeAuth"].hasDeletePermission('/temples')
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.temples-container {
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

.temples-card {
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

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.temple-details {
  padding: 20px 0;
}

.dialog-footer {
  text-align: right;
}

/* Responsive Design */
.table-container {
  overflow-x: auto;
  margin-bottom: 20px;
}

.action-buttons {
  display: flex;
  gap: 5px;
  flex-wrap: wrap;
}

.btn-text {
  display: inline;
}

.refresh-btn {
  width: 100%;
}

@media (max-width: 768px) {
  .temples-container {
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
    flex-direction: column;
    gap: 3px;
  }
  
  .action-buttons .el-button {
    width: 100%;
    font-size: 12px;
  }
  
  .btn-text {
    display: none;
  }
  
  .refresh-btn .btn-text {
    display: inline;
  }
  
  .pagination-container {
    text-align: center;
  }
  
  .pagination-container .el-pagination {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .temples-container {
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
  
  .pagination-container .el-pagination {
    font-size: 12px;
  }
  
  .pagination-container .el-pagination .el-pager li {
    min-width: 28px;
    height: 28px;
    line-height: 28px;
  }
}

/* Tablet Styles */
@media (min-width: 769px) and (max-width: 1024px) {
  .temples-container {
    padding: 15px;
  }
  
  .action-buttons .el-button {
    font-size: 13px;
  }
}

/* Dialog Responsive Styles */
@media (max-width: 768px) {
  .el-dialog {
    margin: 0 !important;
    width: 100% !important;
    height: 100% !important;
    max-height: 100% !important;
  }
  
  .el-dialog__body {
    padding: 15px !important;
  }
  
  .el-form-item__label {
    width: 100px !important;
    text-align: left !important;
  }
  
  .el-form-item__content {
    margin-left: 100px !important;
  }
  
  .el-textarea__inner {
    min-height: 80px !important;
  }
  
  .dialog-footer {
    text-align: center !important;
    padding: 10px 0 !important;
  }
  
  .dialog-footer .el-button {
    width: 100%;
    margin: 5px 0;
  }
}

@media (max-width: 480px) {
  .el-dialog__body {
    padding: 10px !important;
  }
  
  .el-form-item__label {
    width: 80px !important;
    font-size: 12px !important;
  }
  
  .el-form-item__content {
    margin-left: 80px !important;
  }
  
  .el-input__inner,
  .el-textarea__inner {
    font-size: 14px !important;
  }
}
</style>
