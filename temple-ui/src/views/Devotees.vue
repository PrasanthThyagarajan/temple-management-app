<template>
  <div class="devotees-container">
    <!-- Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Devotees" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><User /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Temples Represented" :value="summaryStats.temples">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><OfficeBuilding /></el-icon>
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

    <el-card class="devotees-card">
      <template #header>
        <div class="card-header">
          <h2>Devotee Management</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Devotee
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner devotees-banner"></div>

      <!-- Enhanced Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-input
              v-model="searchTerm"
              placeholder="Search devotees..."
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
              v-model="templeFilter"
              placeholder="Filter by Temple"
              clearable
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Temple" value="" />
              <el-option
                v-for="temple in temples"
                :key="temple.id"
                :label="temple.name"
                :value="temple.id"
              />
            </el-select>
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
          <el-col :xs="24" :sm="24" :md="6" :lg="6" :xl="6">
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
              <el-option label="Member Since (Newest)" value="membership-desc" />
              <el-option label="Member Since (Oldest)" value="membership-asc" />
            </el-select>
          </el-col>
        </el-row>
      </div>

      <!-- Devotees Table -->
      <div class="table-container">
        <el-table
          :data="paginatedDevotees"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
          @sort-change="handleTableSortChange"
        >
          <el-table-column 
            prop="name" 
            label="Name" 
            min-width="150" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column 
            prop="email" 
            label="Email" 
            min-width="200" 
            show-overflow-tooltip 
          />
          <el-table-column prop="phone" label="Phone" width="130" />
          <el-table-column 
            prop="templeName" 
            label="Temple" 
            min-width="150" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column 
            prop="membershipDate" 
            label="Member Since" 
            width="120"
            sortable="custom"
          >
            <template #default="scope">
              {{ formatDate(scope.row.membershipDate) }}
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right" v-if="canUpdate || canDelete">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editDevotee(scope.row)" v-if="canUpdate">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteDevotee(scope.row.id)"
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

    <!-- Create/Edit Devotee Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingDevotee ? 'Edit Devotee' : 'Add New Devotee'"
      width="600px"
    >
      <el-form
        ref="devoteeFormRef"
        :model="devoteeForm"
        :rules="devoteeRules"
        label-width="120px"
      >
        <el-form-item label="Existing User" prop="userId" v-if="!editingDevotee">
          <el-select
            v-model="devoteeForm.userId"
            placeholder="Select existing user (optional)"
            clearable
            filterable
            @change="handleUserSelection"
            style="width: 100%"
            :no-data-text="activeUsers.length === 0 ? 'No available users (all users are already linked to devotees)' : 'No matching data'"
          >
            <el-option
              v-for="user in activeUsers"
              :key="user.userId"
              :label="`${user.fullName} (${user.email})`"
              :value="user.userId"
            />
          </el-select>
          <div style="color: #909399; font-size: 12px; margin-top: 4px;">
            {{ activeUsers.length === 0 ? 'All eligible users are already registered as devotees. (Admin users are excluded)' : 'Leave empty to create a new user account. Admin users are not shown.' }}
          </div>
        </el-form-item>
        <el-form-item label="Full Name" prop="name">
          <el-input v-model="devoteeForm.name" placeholder="Enter full name" />
        </el-form-item>
        <el-form-item label="Email" prop="email">
          <el-input v-model="devoteeForm.email" placeholder="Enter email address" />
        </el-form-item>
        <el-form-item label="Phone" prop="phone">
          <el-input v-model="devoteeForm.phone" placeholder="Enter phone number" />
        </el-form-item>
        <el-form-item label="Address" prop="address">
          <el-input
            v-model="devoteeForm.address"
            type="textarea"
            :rows="2"
            placeholder="Enter street address"
          />
        </el-form-item>
        <el-form-item label="City" prop="city">
          <el-input v-model="devoteeForm.city" placeholder="Enter city" />
        </el-form-item>
        <el-form-item label="State" prop="state">
          <el-input v-model="devoteeForm.state" placeholder="Enter state" />
        </el-form-item>
        <el-form-item label="Postal Code" prop="postalCode">
          <el-input v-model="devoteeForm.postalCode" placeholder="Enter postal code" />
        </el-form-item>
        <el-form-item label="Temple" prop="templeId">
          <el-select
            v-model="devoteeForm.templeId"
            placeholder="Select temple"
            style="width: 100%"
          >
            <el-option
              v-for="temple in temples"
              :key="temple.id"
              :label="temple.name"
              :value="temple.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Membership Date" prop="membershipDate">
          <el-date-picker
            v-model="devoteeForm.membershipDate"
            type="date"
            placeholder="Select membership date"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="Date of Birth" prop="dateOfBirth">
          <el-date-picker
            v-model="devoteeForm.dateOfBirth"
            type="date"
            placeholder="Select date of birth"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="Gender" prop="gender">
          <el-select v-model="devoteeForm.gender" placeholder="Select gender">
            <el-option label="Male" value="Male" />
            <el-option label="Female" value="Female" />
            <el-option label="Other" value="Other" />
          </el-select>
        </el-form-item>
        <el-form-item label="Notes" prop="notes">
          <el-input
            v-model="devoteeForm.notes"
            type="textarea"
            :rows="3"
            placeholder="Enter any additional notes"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveDevotee" :loading="saving" v-if="editingDevotee ? canUpdate : canCreate">
            {{ editingDevotee ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Devotee Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Devotee Details"
      width="700px"
    >
      <div v-if="selectedDevotee" class="devotee-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Full Name">
            {{ selectedDevotee.name }}
          </el-descriptions-item>
          <el-descriptions-item label="Email">{{ selectedDevotee.email }}</el-descriptions-item>
          <el-descriptions-item label="Phone">{{ selectedDevotee.phone }}</el-descriptions-item>
          <el-descriptions-item label="Temple">{{ selectedDevotee.templeName }}</el-descriptions-item>
          <el-descriptions-item label="Membership Date">
            {{ formatDate(selectedDevotee.membershipDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="Date of Birth">
            {{ formatDate(selectedDevotee.dateOfBirth) }}
          </el-descriptions-item>
          <el-descriptions-item label="Gender">{{ selectedDevotee.gender }}</el-descriptions-item>
          <el-descriptions-item label="Address" :span="2">
            {{ selectedDevotee.address }}
          </el-descriptions-item>
          <el-descriptions-item label="Notes" :span="2">
            {{ selectedDevotee.notes || 'No notes available' }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, User, UserFilled, OfficeBuilding, Filter } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const devotees = ref([])
const temples = ref([])
const activeUsers = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const templeFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('id-desc')
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingDevotee = ref(null)
const selectedDevotee = ref(null)

// Form data
const devoteeForm = reactive({
  userId: null,
  name: '',
  email: '',
  phone: '',
  address: '',
  city: '',
  state: '',
  postalCode: '',
  templeId: '',
  membershipDate: '',
  dateOfBirth: '',
  gender: '',
  notes: ''
})

// Form validation rules
const devoteeRules = {
  name: [{ required: true, message: 'Full name is required', trigger: 'blur' }],
  email: [
    { required: true, message: 'Email is required', trigger: 'blur' },
    { type: 'email', message: 'Please enter a valid email', trigger: 'blur' }
  ],
  phone: [{ required: true, message: 'Phone number is required', trigger: 'blur' }],
  templeId: [{ required: true, message: 'Temple selection is required', trigger: 'change' }],
  membershipDate: [{ required: true, message: 'Membership date is required', trigger: 'change' }],
  gender: [{ required: true, message: 'Gender selection is required', trigger: 'change' }],
}

const devoteeFormRef = ref()

// API base URL
const API_BASE = '/api'

// Permissions for this page
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

const refreshPermissions = async () => {
  try {
    if (window && window["templeAuth"]) {
      canCreate.value = await window["templeAuth"].hasCreatePermission('/devotees')
      canUpdate.value = await window["templeAuth"].hasUpdatePermission('/devotees')
      canDelete.value = await window["templeAuth"].hasDeletePermission('/devotees')
    }
  } catch (_) { /* ignore */ }
}

// Summary Statistics
const summaryStats = computed(() => {
  return {
    total: devotees.value.length,
    temples: [...new Set(devotees.value.map(d => d.templeId))].length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...devotees.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(d =>
      d.name?.toLowerCase().includes(term) ||
      d.email?.toLowerCase().includes(term) ||
      d.phone?.includes(term) ||
      d.templeName?.toLowerCase().includes(term)
    )
  }
  
  // Apply temple filter
  if (templeFilter.value) {
    result = result.filter(d => d.templeId === parseInt(templeFilter.value))
  }
  
  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(d => d.status === statusFilter.value)
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
        case 'id':
          aVal = a.id
          bVal = b.id
          break
        case 'membership':
          aVal = new Date(a.membershipDate || 0).getTime()
          bVal = new Date(b.membershipDate || 0).getTime()
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
const paginatedDevotees = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

// Methods
const loadDevotees = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/devotees`)
    devotees.value = response.data.map(d => ({
      ...d,
      name: d.fullName || d.name || '',
      templeName: temples.value.find(t => t.id === d.templeId)?.name || 'Unknown'
    }))
  } catch (error) {
    console.error('Error loading devotees:', error)
    ElMessage.error('Failed to load devotees')
  } finally {
    loading.value = false
  }
}

const loadTemples = async () => {
  try {
    const response = await axios.get(`${API_BASE}/temples`)
    temples.value = response.data
  } catch (error) {
    console.error('Error loading temples:', error)
  }
}

const loadActiveUsers = async () => {
  try {
    const response = await axios.get(`${API_BASE}/devotees/available-users`)
    // Users are already filtered on the backend
    activeUsers.value = response.data
  } catch (error) {
    console.error('Error loading available users:', error)
    activeUsers.value = []
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

const handleUserSelection = (userId) => {
  if (userId) {
    const selectedUser = activeUsers.value.find(u => u.userId === userId)
    if (selectedUser) {
      // Pre-fill form with user data
      devoteeForm.name = selectedUser.fullName
      devoteeForm.email = selectedUser.email
      devoteeForm.phone = selectedUser.phoneNumber || ''
      devoteeForm.address = selectedUser.address || ''
      devoteeForm.gender = selectedUser.gender || ''
      devoteeForm.dateOfBirth = selectedUser.dateOfBirth || ''
    }
  }
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'id-desc'
  } else {
    let field = prop
    if (prop === 'templeName') field = 'temple'
    else if (prop === 'membershipDate') field = 'membership'
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const editDevotee = (devotee) => {
  editingDevotee.value = devotee
  Object.assign(devoteeForm, devotee)
  devoteeForm.name = devotee.name || devotee.fullName || ''
  showCreateDialog.value = true
}

const saveDevotee = async () => {
  try {
    await devoteeFormRef.value.validate()
    saving.value = true
    
    // Clean up form data before sending
    const formData = {
      name: devoteeForm.name,
      email: devoteeForm.email,
      phone: devoteeForm.phone,
      address: devoteeForm.address,
      city: devoteeForm.city,
      state: devoteeForm.state,
      postalCode: devoteeForm.postalCode,
      templeId: devoteeForm.templeId,
      dateOfBirth: devoteeForm.dateOfBirth || null,
      gender: devoteeForm.gender,
      userId: devoteeForm.userId || 0
    }
    
    if (editingDevotee.value) {
      // Update existing devotee
      await axios.put(`${API_BASE}/devotees/${editingDevotee.value.id}`, formData)
      ElMessage.success('Devotee updated successfully')
    } else {
      // Create new devotee and show generated password if provided
      const response = await axios.post(`${API_BASE}/devotees`, formData)
      const pwd = response?.data?.generatedPassword
      if (pwd) {
        await ElMessageBox.alert(
          `User account created. Generated temporary password:\n\n${pwd}\n\nPlease share securely or the user can reset after login.`,
          'Account Created',
          { confirmButtonText: 'OK' }
        )
      } else {
        ElMessage.success('Devotee created successfully')
      }
    }
    
    showCreateDialog.value = false
    resetForm()
    loadDevotees()
    loadActiveUsers() // Refresh available users list
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save devotee')
    }
  } finally {
    saving.value = false
  }
}

const deleteDevotee = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this devotee?',
      'Warning',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/devotees/${id}`)
    ElMessage.success('Devotee deleted successfully')
    loadDevotees()
    loadActiveUsers() // Refresh available users list
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting devotee:', error)
      ElMessage.error('Failed to delete devotee')
    }
  }
}

const handleRowClick = (row) => {
  selectedDevotee.value = row
  showDetailsDialog.value = true
}

const resetForm = () => {
  editingDevotee.value = null
  Object.assign(devoteeForm, {
    userId: null,
    name: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    templeId: '',
    membershipDate: '',
    dateOfBirth: '',
    gender: '',
    notes: ''
  })
  devoteeFormRef.value?.resetFields()
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY')
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
  await loadTemples()
  await loadActiveUsers()
  await loadDevotees()
  await refreshPermissions()
})
</script>

<style scoped>
.devotees-container {
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

.devotees-card {
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

.devotee-details {
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
  flex-wrap: wrap;
}

.btn-text {
  display: inline;
}

.refresh-btn {
  width: 100%;
}

@media (max-width: 768px) {
  .devotees-container {
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
  .devotees-container {
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
  .devotees-container {
    padding: 15px;
  }
  
  .action-buttons .el-button {
    font-size: 13px;
  }
}
</style>
