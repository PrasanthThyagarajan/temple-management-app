<template>
  <div class="devotees-container">
    <el-card class="devotees-card">
      <template #header>
        <div class="card-header">
          <h2>Devotee Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            Add Devotee
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner devotees-banner"></div>

      <!-- Search and Filters -->
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
              @change="handleTempleFilter"
            >
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
              @change="handleStatusFilter"
            >
              <el-option label="Active" value="Active" />
              <el-option label="Inactive" value="Inactive" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="24" :md="6" :lg="6" :xl="6">
            <el-button @click="loadDevotees" :loading="loading" class="refresh-btn">
              <el-icon><Refresh /></el-icon>
              <span class="btn-text">Refresh</span>
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Devotees Table -->
      <div class="table-container">
        <el-table
          :data="devotees"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
        >
          <el-table-column prop="name" label="Name" min-width="150" show-overflow-tooltip />
          <el-table-column prop="email" label="Email" min-width="200" show-overflow-tooltip />
          <el-table-column prop="phone" label="Phone" width="130" />
          <el-table-column prop="templeName" label="Temple" min-width="150" show-overflow-tooltip />
          <el-table-column prop="membershipDate" label="Member Since" width="120">
            <template #default="scope">
              {{ formatDate(scope.row.membershipDate) }}
            </template>
          </el-table-column>
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
                <el-button size="small" @click.stop="editDevotee(scope.row)">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteDevotee(scope.row.id)"
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
          :total="totalDevotees"
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
            :rows="3"
            placeholder="Enter address"
          />
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
        <el-form-item label="Status" prop="status">
          <el-select v-model="devoteeForm.status" placeholder="Select status">
            <el-option label="Active" value="Active" />
            <el-option label="Inactive" value="Inactive" />
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
          <el-button type="primary" @click="saveDevotee" :loading="saving">
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
          <el-descriptions-item label="Status">
            <el-tag :type="selectedDevotee.status === 'Active' ? 'success' : 'warning'">
              {{ selectedDevotee.status }}
            </el-tag>
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
import { Plus, Search, Refresh, Edit, Delete } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const devotees = ref([])
const temples = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const templeFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const totalDevotees = ref(0)
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingDevotee = ref(null)
const selectedDevotee = ref(null)

// Form data
const devoteeForm = reactive({
  name: '',
  email: '',
  phone: '',
  address: '',
  templeId: '',
  membershipDate: '',
  dateOfBirth: '',
  gender: '',
  status: 'Active',
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
  status: [{ required: true, message: 'Status is required', trigger: 'change' }]
}

const devoteeFormRef = ref()

// API base URL
const API_BASE = 'http://localhost:5051/api'

// Methods
const loadDevotees = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/devotees`)
    devotees.value = response.data
    totalDevotees.value = response.data.length
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

const handleSearch = () => {
  if (searchTerm.value.trim()) {
    searchDevotees(searchTerm.value)
  } else {
    loadDevotees()
  }
}

const searchDevotees = async (term) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/devotees/search/${term}`)
    devotees.value = response.data
    totalDevotees.value = response.data.length
  } catch (error) {
    console.error('Error searching devotees:', error)
    ElMessage.error('Failed to search devotees')
  } finally {
    loading.value = false
  }
}

const handleTempleFilter = () => {
  if (templeFilter.value) {
    filterByTemple(templeFilter.value)
  } else {
    loadDevotees()
  }
}

const filterByTemple = async (templeId) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/temples/${templeId}/devotees`)
    devotees.value = response.data
    totalDevotees.value = response.data.length
  } catch (error) {
    console.error('Error filtering by temple:', error)
    ElMessage.error('Failed to filter devotees by temple')
  } finally {
    loading.value = false
  }
}

const handleStatusFilter = () => {
  if (statusFilter.value) {
    filterByStatus(statusFilter.value)
  } else {
    loadDevotees()
  }
}

const filterByStatus = () => {
  devotees.value = devotees.value.filter(devotee => devotee.status === statusFilter.value)
  totalDevotees.value = devotees.value.length
}

const editDevotee = (devotee) => {
  editingDevotee.value = devotee
  Object.assign(devoteeForm, devotee)
  showCreateDialog.value = true
}

const saveDevotee = async () => {
  try {
    await devoteeFormRef.value.validate()
    saving.value = true
    
    if (editingDevotee.value) {
      // Update existing devotee
      await axios.put(`${API_BASE}/devotees/${editingDevotee.value.id}`, devoteeForm)
      ElMessage.success('Devotee updated successfully')
    } else {
      // Create new devotee
      await axios.post(`${API_BASE}/devotees`, devoteeForm)
      ElMessage.success('Devotee created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadDevotees()
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
    name: '',
    email: '',
    phone: '',
    address: '',
    templeId: '',
    membershipDate: '',
    dateOfBirth: '',
    gender: '',
    status: 'Active',
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
  loadDevotees()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadDevotees()
}

// Lifecycle
onMounted(() => {
  loadDevotees()
  loadTemples()
})
</script>

<style scoped>
.devotees-container {
  padding: 20px;
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
