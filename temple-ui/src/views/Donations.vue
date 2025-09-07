<template>
  <div class="donations-container">
    <el-card class="donations-card">
      <template #header>
        <div class="card-header">
          <h2>Donation Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            Add Donation
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-input
              v-model="searchTerm"
              placeholder="Search donations..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
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
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select
              v-model="typeFilter"
              placeholder="Filter by Type"
              clearable
              @change="handleTypeFilter"
            >
              <el-option label="Cash" value="Cash" />
              <el-option label="Check" value="Check" />
              <el-option label="Online" value="Online" />
              <el-option label="Other" value="Other" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="4" :lg="4" :xl="4">
            <el-date-picker
              v-model="dateRange"
              type="daterange"
              range-separator="to"
              start-placeholder="Start date"
              end-placeholder="End date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              @change="handleDateFilter"
              style="width: 100%"
            />
          </el-col>
          <el-col :xs="24" :sm="24" :md="6" :lg="6" :xl="6">
            <div class="action-buttons">
              <el-button @click="loadDonations" :loading="loading" class="refresh-btn">
                <el-icon><Refresh /></el-icon>
                <span class="btn-text">Refresh</span>
              </el-button>
              <el-button type="success" @click="exportDonations" class="export-btn">
                <el-icon><Download /></el-icon>
                <span class="btn-text">Export</span>
              </el-button>
            </div>
          </el-col>
        </el-row>
      </div>

      <!-- Devotional Banner -->
      <div class="devotional-banner donations-banner"></div>

      <!-- Summary Cards -->
      <div class="summary-cards">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><Money /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">₹{{ totalAmount.toLocaleString() }}</div>
                  <div class="summary-label">Total Donations</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon count">
                  <el-icon><Document /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ totalDonations }}</div>
                  <div class="summary-label">Total Count</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon average">
                  <el-icon><TrendCharts /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">₹{{ averageAmount.toLocaleString() }}</div>
                  <div class="summary-label">Average</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon recent">
                  <el-icon><Calendar /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ recentDonations }}</div>
                  <div class="summary-label">This Month</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Donations Table -->
      <div class="table-container">
        <el-table
          :data="donations"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
        >
          <el-table-column prop="donorName" label="Donor Name" min-width="150" show-overflow-tooltip />
          <el-table-column prop="templeName" label="Temple" min-width="150" show-overflow-tooltip />
          <el-table-column prop="amount" label="Amount" width="120">
            <template #default="scope">
              ₹{{ scope.row.amount.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="donationType" label="Type" width="100">
            <template #default="scope">
              <el-tag :type="getTypeTagType(scope.row.donationType)" size="small">
                {{ scope.row.donationType }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="donationDate" label="Date" width="120">
            <template #default="scope">
              {{ formatDate(scope.row.donationDate) }}
            </template>
          </el-table-column>
          <el-table-column prop="purpose" label="Purpose" min-width="150" show-overflow-tooltip />
          <el-table-column prop="status" label="Status" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.status === 'Completed' ? 'success' : 'warning'" size="small">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editDonation(scope.row)">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteDonation(scope.row.id)"
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
          :total="totalDonations"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Create/Edit Donation Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingDonation ? 'Edit Donation' : 'Add New Donation'"
      width="700px"
    >
      <el-form
        ref="donationFormRef"
        :model="donationForm"
        :rules="donationRules"
        label-width="120px"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Donor Name" prop="donorName">
              <el-input v-model="donationForm.donorName" placeholder="Enter donor name" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Donor Email" prop="donorEmail">
              <el-input v-model="donationForm.donorEmail" placeholder="Enter donor email" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Donor Phone" prop="donorPhone">
              <el-input v-model="donationForm.donorPhone" placeholder="Enter donor phone" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Temple" prop="templeId">
              <el-select
                v-model="donationForm.templeId"
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
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Amount" prop="amount">
              <el-input-number
                v-model="donationForm.amount"
                :min="0"
                :precision="2"
                style="width: 100%"
                placeholder="Enter amount"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Donation Type" prop="donationType">
              <el-select v-model="donationForm.donationType" placeholder="Select type">
                <el-option label="Cash" value="Cash" />
                <el-option label="Check" value="Check" />
                <el-option label="Online" value="Online" />
                <el-option label="Other" value="Other" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Donation Date" prop="donationDate">
              <el-date-picker
                v-model="donationForm.donationDate"
                type="date"
                placeholder="Select donation date"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Status" prop="status">
              <el-select v-model="donationForm.status" placeholder="Select status">
                <el-option label="Pending" value="Pending" />
                <el-option label="Completed" value="Completed" />
                <el-option label="Failed" value="Failed" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="Purpose" prop="purpose">
          <el-input
            v-model="donationForm.purpose"
            type="textarea"
            :rows="3"
            placeholder="Enter donation purpose"
          />
        </el-form-item>
        <el-form-item label="Notes" prop="notes">
          <el-input
            v-model="donationForm.notes"
            type="textarea"
            :rows="3"
            placeholder="Enter any additional notes"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveDonation" :loading="saving">
            {{ editingDonation ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Donation Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Donation Details"
      width="700px"
    >
      <div v-if="selectedDonation" class="donation-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Donor Name">
            {{ selectedDonation.donorName }}
          </el-descriptions-item>
          <el-descriptions-item label="Status">
            <el-tag :type="selectedDonation.status === 'Completed' ? 'success' : 'warning'">
              {{ selectedDonation.status }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Amount">
            ₹{{ selectedDonation.amount.toLocaleString() }}
          </el-descriptions-item>
          <el-descriptions-item label="Donation Type">
            <el-tag :type="getTypeTagType(selectedDonation.donationType)">
              {{ selectedDonation.donationType }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Temple">{{ selectedDonation.templeName }}</el-descriptions-item>
          <el-descriptions-item label="Donation Date">
            {{ formatDate(selectedDonation.donationDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="Donor Email">{{ selectedDonation.donorEmail }}</el-descriptions-item>
          <el-descriptions-item label="Donor Phone">{{ selectedDonation.donorPhone }}</el-descriptions-item>
          <el-descriptions-item label="Purpose" :span="2">
            {{ selectedDonation.purpose }}
          </el-descriptions-item>
          <el-descriptions-item label="Notes" :span="2">
            {{ selectedDonation.notes || 'No notes available' }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Download, Money, Document, TrendCharts, Calendar } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const donations = ref([])
const temples = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const templeFilter = ref('')
const typeFilter = ref('')
const dateRange = ref([])
const currentPage = ref(1)
const pageSize = ref(20)
const totalDonations = ref(0)
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingDonation = ref(null)
const selectedDonation = ref(null)

// Form data
const donationForm = reactive({
  donorName: '',
  donorEmail: '',
  donorPhone: '',
  templeId: '',
  amount: 0,
  donationType: 'Cash',
  donationDate: '',
  purpose: '',
  status: 'Pending',
  notes: ''
})

// Form validation rules
const donationRules = {
  donorName: [{ required: true, message: 'Donor name is required', trigger: 'blur' }],
  templeId: [{ required: true, message: 'Temple selection is required', trigger: 'change' }],
  amount: [{ required: true, message: 'Amount is required', trigger: 'blur' }],
  donationType: [{ required: true, message: 'Donation type is required', trigger: 'change' }],
  donationDate: [{ required: true, message: 'Donation date is required', trigger: 'change' }],
  purpose: [{ required: true, message: 'Purpose is required', trigger: 'blur' }],
  status: [{ required: true, message: 'Status is required', trigger: 'change' }]
}

const donationFormRef = ref()

// API base URL
const API_BASE = 'http://localhost:5051/api'

// Computed properties
const totalAmount = computed(() => {
  return donations.value.reduce((sum, donation) => sum + donation.amount, 0)
})

const averageAmount = computed(() => {
  if (donations.value.length === 0) return 0
  return Math.round(totalAmount.value / donations.value.length)
})

const recentDonations = computed(() => {
  const currentMonth = dayjs().month()
  const currentYear = dayjs().year()
  return donations.value.filter(donation => {
    const donationDate = dayjs(donation.donationDate)
    return donationDate.month() === currentMonth && donationDate.year() === currentYear
  }).length
})

// Methods
const loadDonations = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/donations`)
    donations.value = response.data
    totalDonations.value = response.data.length
  } catch (error) {
    console.error('Error loading donations:', error)
    ElMessage.error('Failed to load donations')
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
    searchDonations(searchTerm.value)
  } else {
    loadDonations()
  }
}

const searchDonations = async (term) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/donations/search/${term}`)
    donations.value = response.data
    totalDonations.value = response.data.length
  } catch (error) {
    console.error('Error searching donations:', error)
    ElMessage.error('Failed to search donations')
  } finally {
    loading.value = false
  }
}

const handleTempleFilter = () => {
  if (templeFilter.value) {
    filterByTemple(templeFilter.value)
  } else {
    loadDonations()
  }
}

const filterByTemple = async (templeId) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/temples/${templeId}/donations`)
    donations.value = response.data
    totalDonations.value = response.data.length
  } catch (error) {
    console.error('Error filtering by temple:', error)
    ElMessage.error('Failed to filter donations by temple')
  } finally {
    loading.value = false
  }
}

const handleTypeFilter = () => {
  if (typeFilter.value) {
    donations.value = donations.value.filter(donation => donation.donationType === typeFilter.value)
    totalDonations.value = donations.value.length
  } else {
    loadDonations()
  }
}

const handleDateFilter = () => {
  if (dateRange.value && dateRange.value.length === 2) {
    const startDate = dayjs(dateRange.value[0])
    const endDate = dayjs(dateRange.value[1])
    donations.value = donations.value.filter(donation => {
      const donationDate = dayjs(donation.donationDate)
      return donationDate.isAfter(startDate) && donationDate.isBefore(endDate)
    })
    totalDonations.value = donations.value.length
  } else {
    loadDonations()
  }
}

const editDonation = (donation) => {
  editingDonation.value = donation
  Object.assign(donationForm, donation)
  showCreateDialog.value = true
}

const saveDonation = async () => {
  try {
    await donationFormRef.value.validate()
    saving.value = true
    
    if (editingDonation.value) {
      // Update existing donation
      await axios.put(`${API_BASE}/donations/${editingDonation.value.id}`, donationForm)
      ElMessage.success('Donation updated successfully')
    } else {
      // Create new donation
      await axios.post(`${API_BASE}/donations`, donationForm)
      ElMessage.success('Donation created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadDonations()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save donation')
    }
  } finally {
    saving.value = false
  }
}

const deleteDonation = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this donation?',
      'Warning',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/donations/${id}`)
    ElMessage.success('Donation deleted successfully')
    loadDonations()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting donation:', error)
      ElMessage.error('Failed to delete donation')
    }
  }
}

const handleRowClick = (row) => {
  selectedDonation.value = row
  showDetailsDialog.value = true
}

const resetForm = () => {
  editingDonation.value = null
  Object.assign(donationForm, {
    donorName: '',
    donorEmail: '',
    donorPhone: '',
    templeId: '',
    amount: 0,
    donationType: 'Cash',
    donationDate: '',
    purpose: '',
    status: 'Pending',
    notes: ''
  })
  donationFormRef.value?.resetFields()
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY')
}

const getTypeTagType = (type) => {
  const typeMap = {
    'Cash': 'success',
    'Check': 'warning',
    'Online': 'primary',
    'Other': 'info'
  }
  return typeMap[type] || 'info'
}

const exportDonations = () => {
  // Simple CSV export
  const headers = ['Donor Name', 'Temple', 'Amount', 'Type', 'Date', 'Purpose', 'Status']
  const csvContent = [
    headers.join(','),
    ...donations.value.map(donation => [
      donation.donorName,
      donation.templeName,
      donation.amount,
      donation.donationType,
      donation.donationDate,
      donation.purpose,
      donation.status
    ].join(','))
  ].join('\n')
  
  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `donations-${dayjs().format('YYYY-MM-DD')}.csv`
  a.click()
  window.URL.revokeObjectURL(url)
  
  ElMessage.success('Donations exported successfully')
}

const handleSizeChange = (val) => {
  pageSize.value = val
  loadDonations()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadDonations()
}

// Lifecycle
onMounted(() => {
  loadDonations()
  loadTemples()
})
</script>

<style scoped>
.donations-container {
  padding: 20px;
}

.donations-card {
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
  background-color: #67c23a;
}

.summary-icon.count {
  background-color: #409eff;
}

.summary-icon.average {
  background-color: #e6a23c;
}

.summary-icon.recent {
  background-color: #f56c6c;
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

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.donation-details {
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

.refresh-btn,
.export-btn {
  flex: 1;
  min-width: 0;
}

@media (max-width: 768px) {
  .donations-container {
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
    gap: 5px;
  }
  
  .action-buttons .el-button {
    width: 100%;
    font-size: 12px;
  }
  
  .btn-text {
    display: none;
  }
  
  .refresh-btn .btn-text,
  .export-btn .btn-text {
    display: inline;
  }
  
  .summary-cards {
    margin-bottom: 15px;
  }
  
  .summary-card {
    margin-bottom: 10px;
  }
  
  .summary-content {
    gap: 10px;
  }
  
  .summary-icon {
    width: 40px;
    height: 40px;
    font-size: 20px;
  }
  
  .summary-value {
    font-size: 20px;
  }
  
  .summary-label {
    font-size: 12px;
  }
  
  .pagination-container {
    text-align: center;
  }
  
  .pagination-container .el-pagination {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .donations-container {
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
  
  .summary-icon {
    width: 35px;
    height: 35px;
    font-size: 18px;
  }
  
  .summary-value {
    font-size: 18px;
  }
  
  .summary-label {
    font-size: 11px;
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
  .donations-container {
    padding: 15px;
  }
  
  .action-buttons .el-button {
    font-size: 13px;
  }
  
  .summary-icon {
    width: 45px;
    height: 45px;
    font-size: 22px;
  }
  
  .summary-value {
    font-size: 22px;
  }
}
</style>
