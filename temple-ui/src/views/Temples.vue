<template>
  <div class="temples-container">
    <el-card class="temples-card">
      <template #header>
        <div class="card-header">
          <h2>Temple Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            Add Temple
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner temples-banner"></div>

      <!-- Search and Filters -->
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
            <el-input
              v-model="locationFilter.city"
              placeholder="City"
              clearable
              @input="handleLocationFilter"
            />
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-input
              v-model="locationFilter.state"
              placeholder="State"
              clearable
              @input="handleLocationFilter"
            />
          </el-col>
          <el-col :xs="24" :sm="24" :md="4" :lg="4" :xl="4">
            <el-button @click="loadTemples" :loading="loading" class="refresh-btn">
              <el-icon><Refresh /></el-icon>
              <span class="btn-text">Refresh</span>
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Temples Table -->
      <div class="table-container">
        <el-table
          :data="temples"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
        >
          <el-table-column prop="name" label="Temple Name" min-width="200" show-overflow-tooltip />
          <el-table-column prop="city" label="City" width="120" show-overflow-tooltip />
          <el-table-column prop="state" label="State" width="120" show-overflow-tooltip />
          <el-table-column prop="establishedYear" label="Established" width="100" />
          <el-table-column prop="deity" label="Main Deity" width="150" show-overflow-tooltip />
          <el-table-column prop="status" label="Status" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.status === 'Active' ? 'success' : 'warning'" size="small">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editTemple(scope.row)">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteTemple(scope.row.id)"
                >
                  <el-icon><Delete /></el-icon>
                  <span class="btn-text">Delete</span>
                </el-button>
              </div>
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
          :total="totalTemples"
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
          <el-button type="primary" @click="saveTemple" :loading="saving">
            {{ editingTemple ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Temple Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Temple Details"
      :width="dialogWidth"
      :fullscreen="isMobile"
    >
      <div v-if="selectedTemple" class="temple-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Temple Name">
            {{ selectedTemple.name }}
          </el-descriptions-item>
          <el-descriptions-item label="Status">
            <el-tag :type="selectedTemple.status === 'Active' ? 'success' : 'warning'">
              {{ selectedTemple.status }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="City">{{ selectedTemple.city }}</el-descriptions-item>
          <el-descriptions-item label="State">{{ selectedTemple.state }}</el-descriptions-item>
          <el-descriptions-item label="Established Year">
            {{ selectedTemple.establishedYear }}
          </el-descriptions-item>
          <el-descriptions-item label="Main Deity">{{ selectedTemple.deity }}</el-descriptions-item>
          <el-descriptions-item label="Address" :span="2">
            {{ selectedTemple.address }}
          </el-descriptions-item>
          <el-descriptions-item label="Description" :span="2">
            {{ selectedTemple.description }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete } from '@element-plus/icons-vue'
import axios from 'axios'

// Reactive data
const temples = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const locationFilter = reactive({ city: '', state: '' })
const currentPage = ref(1)
const pageSize = ref(20)
const totalTemples = ref(0)
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingTemple = ref(null)
const selectedTemple = ref(null)

// Form data
const templeForm = reactive({
  name: '',
  city: '',
  state: '',
  address: '',
  establishedYear: '',
  deity: '',
  description: '',
  status: 'Active'
})

// Form validation rules
const templeRules = {
  name: [{ required: true, message: 'Temple name is required', trigger: 'blur' }],
  city: [{ required: true, message: 'City is required', trigger: 'blur' }],
  state: [{ required: true, message: 'State is required', trigger: 'blur' }],
  address: [{ required: true, message: 'Address is required', trigger: 'blur' }],
  establishedYear: [{ required: true, message: 'Established year is required', trigger: 'change' }],
  deity: [{ required: true, message: 'Main deity is required', trigger: 'blur' }],
  status: [{ required: true, message: 'Status is required', trigger: 'change' }]
}

const templeFormRef = ref()

// Responsive data
const isMobile = ref(false)
const dialogWidth = ref('600px')

// API base URL
const API_BASE = 'http://localhost:5051/api'

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
    totalTemples.value = response.data.length
  } catch (error) {
    console.error('Error loading temples:', error)
    ElMessage.error('Failed to load temples')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  if (searchTerm.value.trim()) {
    searchTemples(searchTerm.value)
  } else {
    loadTemples()
  }
}

const searchTemples = async (term) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/temples/search/${term}`)
    temples.value = response.data
    totalTemples.value = response.data.length
  } catch (error) {
    console.error('Error searching temples:', error)
    ElMessage.error('Failed to search temples')
  } finally {
    loading.value = false
  }
}

const handleLocationFilter = () => {
  if (locationFilter.city || locationFilter.state) {
    filterByLocation()
  } else {
    loadTemples()
  }
}

const filterByLocation = async () => {
  try {
    loading.value = true
    const params = new URLSearchParams()
    if (locationFilter.city) params.append('city', locationFilter.city)
    if (locationFilter.state) params.append('state', locationFilter.state)
    
    const response = await axios.get(`${API_BASE}/temples/location/${locationFilter.city}?state=${locationFilter.state}`)
    temples.value = response.data
    totalTemples.value = response.data.length
  } catch (error) {
    console.error('Error filtering by location:', error)
    ElMessage.error('Failed to filter temples by location')
  } finally {
    loading.value = false
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

const handleRowClick = (row) => {
  selectedTemple.value = row
  showDetailsDialog.value = true
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
    status: 'Active'
  })
  templeFormRef.value?.resetFields()
}

const handleSizeChange = (val) => {
  pageSize.value = val
  loadTemples()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadTemples()
}

// Lifecycle
onMounted(() => {
  loadTemples()
  checkScreenSize()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.temples-container {
  padding: 20px;
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
