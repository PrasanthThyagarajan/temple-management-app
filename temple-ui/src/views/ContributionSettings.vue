<template>
  <div class="contribution-settings-container">
    <!-- Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="8">
        <el-card class="summary-card">
          <el-statistic title="Total Contributions" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Money /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="8">
        <el-card class="summary-card active">
          <el-statistic title="Active Contributions" :value="summaryStats.active">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><CircleCheck /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="8">
        <el-card class="summary-card">
          <el-statistic title="Filtered Results" :value="filteredBeforePagination.length">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #e6a23c;"><Filter /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="contribution-settings-card">
      <template #header>
        <div class="card-header">
          <h2>Contribution Settings</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Contribution Setting
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search by name or description..."
              @input="handleSearch"
              clearable
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8">
            <el-select v-model="eventFilter" placeholder="Filter by Event" clearable @change="handleFilterChange">
              <el-option
                v-for="event in events"
                :key="event.id"
                :label="event.name"
                :value="event.id"
              />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8">
            <el-select v-model="typeFilter" placeholder="Filter by Type" clearable @change="handleFilterChange">
              <el-option label="Single" value="Single" />
              <el-option label="Recurring" value="Recurring" />
            </el-select>
          </el-col>
        </el-row>
      </div>

      <!-- Data Table -->
      <div class="table-container">
        <el-table
          :data="paginatedContributions"
          v-loading="loading"
          stripe
          @row-click="viewContributionDetails"
          @sort-change="handleTableSortChange"
        >
          <el-table-column prop="name" label="Name" min-width="150" sortable="custom" show-overflow-tooltip />
          <el-table-column prop="eventName" label="Event" min-width="150" show-overflow-tooltip />
          <el-table-column prop="contributionType" label="Type" width="100" align="center">
            <template #default="scope">
              <el-tag :type="scope.row.contributionType === 'Single' ? 'primary' : 'success'">
                {{ scope.row.contributionType }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="amount" label="Amount" width="120" align="right" sortable="custom">
            <template #default="scope">
              ₹{{ scope.row.amount.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column label="Recurring Details" width="150" align="center">
            <template #default="scope">
              <span v-if="scope.row.contributionType === 'Recurring'">
                Day {{ scope.row.recurringDay }} ({{ scope.row.recurringFrequency }})
              </span>
              <span v-else>—</span>
            </template>
          </el-table-column>
          <el-table-column prop="createdAt" label="Created Date" width="120" sortable="custom">
            <template #default="scope">
              {{ formatDate(scope.row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right" v-if="canUpdate || canDelete">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editContribution(scope.row)" v-if="canUpdate">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button 
                  size="small" 
                  type="danger" 
                  @click.stop="deleteContribution(scope.row.id)" 
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

      <!-- Pagination -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :small="false"
          :disabled="false"
          :background="true"
          layout="total, sizes, prev, pager, next, jumper"
          :total="filteredBeforePagination.length"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingContribution ? 'Edit Contribution Setting' : 'Create Contribution Setting'"
      width="600px"
      @close="resetForm"
    >
      <el-form
        ref="contributionFormRef"
        :model="contributionForm"
        :rules="contributionRules"
        label-width="140px"
      >
        <el-form-item label="Name" prop="name">
          <el-input v-model="contributionForm.name" placeholder="Enter contribution name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="contributionForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter description"
          />
        </el-form-item>
        <el-form-item label="Event" prop="eventId">
          <el-select v-model="contributionForm.eventId" placeholder="Select event" style="width: 100%">
            <el-option
              v-for="event in events"
              :key="event.id"
              :label="event.name"
              :value="event.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Type" prop="contributionType">
          <el-radio-group v-model="contributionForm.contributionType">
            <el-radio label="Single">Single Payment</el-radio>
            <el-radio label="Recurring">Recurring Payment</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="Amount" prop="amount">
          <el-input-number
            v-model="contributionForm.amount"
            :min="0"
            :precision="2"
            style="width: 100%"
            placeholder="Enter amount"
          />
        </el-form-item>
        
        <!-- Recurring Settings -->
        <div v-if="contributionForm.contributionType === 'Recurring'">
          <el-form-item label="Recurring Day" prop="recurringDay">
            <el-input-number
              v-model="contributionForm.recurringDay"
              :min="1"
              :max="31"
              style="width: 100%"
              placeholder="Day of month (1-31)"
            />
          </el-form-item>
          <el-form-item label="Frequency" prop="recurringFrequency">
            <el-select v-model="contributionForm.recurringFrequency" placeholder="Select frequency" style="width: 100%">
              <el-option label="Monthly" value="Monthly" />
              <el-option label="Quarterly" value="Quarterly" />
              <el-option label="Yearly" value="Yearly" />
            </el-select>
          </el-form-item>
        </div>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveContribution" :loading="saving" v-if="editingContribution ? canUpdate : canCreate">
            {{ editingContribution ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Contribution Setting Details"
      width="700px"
    >
      <div v-if="selectedContribution" class="contribution-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Name">
            {{ selectedContribution.name }}
          </el-descriptions-item>
          <el-descriptions-item label="Event">{{ selectedContribution.eventName }}</el-descriptions-item>
          <el-descriptions-item label="Type">
            <el-tag :type="selectedContribution.contributionType === 'Single' ? 'primary' : 'success'">
              {{ selectedContribution.contributionType }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Amount">
            ₹{{ selectedContribution.amount.toLocaleString() }}
          </el-descriptions-item>
          <el-descriptions-item v-if="selectedContribution.contributionType === 'Recurring'" label="Recurring Day">
            {{ selectedContribution.recurringDay }}
          </el-descriptions-item>
          <el-descriptions-item v-if="selectedContribution.contributionType === 'Recurring'" label="Frequency">
            {{ selectedContribution.recurringFrequency }}
          </el-descriptions-item>
          <el-descriptions-item label="Created Date">
            {{ formatDate(selectedContribution.createdAt) }}
          </el-descriptions-item>
          <el-descriptions-item label="Updated Date">
            {{ formatDate(selectedContribution.updatedAt) }}
          </el-descriptions-item>
          <el-descriptions-item label="Description" :span="2">
            {{ selectedContribution.description || 'No description available' }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Filter, Edit, Delete, View, Money, CircleCheck } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const contributions = ref([])
const events = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const eventFilter = ref('')
const typeFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('id-desc')
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingContribution = ref(null)
const selectedContribution = ref(null)

// Form data
const contributionForm = reactive({
  name: '',
  description: '',
  eventId: '',
  contributionType: 'Single',
  amount: 0,
  recurringDay: null,
  recurringFrequency: ''
})

// Form validation rules
const contributionRules = {
  name: [{ required: true, message: 'Name is required', trigger: 'blur' }],
  eventId: [{ required: true, message: 'Event selection is required', trigger: 'change' }],
  contributionType: [{ required: true, message: 'Type is required', trigger: 'change' }],
  amount: [{ required: true, message: 'Amount is required', trigger: 'blur' }],
  recurringDay: [
    { 
      validator: (rule, value, callback) => {
        if (contributionForm.contributionType === 'Recurring' && (!value || value < 1 || value > 31)) {
          callback(new Error('Recurring day must be between 1 and 31'))
        } else {
          callback()
        }
      }, 
      trigger: 'blur' 
    }
  ],
  recurringFrequency: [
    { 
      validator: (rule, value, callback) => {
        if (contributionForm.contributionType === 'Recurring' && !value) {
          callback(new Error('Frequency is required for recurring contributions'))
        } else {
          callback()
        }
      }, 
      trigger: 'change' 
    }
  ]
}

const contributionFormRef = ref()

// API base URL
const API_BASE = '/api'

// Permissions for this page
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

const refreshPermissions = async () => {
  try {
    if (window && window["templeAuth"]) {
      canCreate.value = await window["templeAuth"].hasCreatePermission('/contribution-settings')
      canUpdate.value = await window["templeAuth"].hasUpdatePermission('/contribution-settings')
      canDelete.value = await window["templeAuth"].hasDeletePermission('/contribution-settings')
    }
  } catch (_) { /* ignore */ }
}

// Summary Statistics
const summaryStats = computed(() => {
  return {
    total: contributions.value.length,
    active: contributions.value.filter(c => c.isActive).length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...contributions.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(c => 
      c.name.toLowerCase().includes(term) ||
      c.description.toLowerCase().includes(term) ||
      c.eventName?.toLowerCase().includes(term)
    )
  }
  
  // Apply event filter
  if (eventFilter.value) {
    result = result.filter(c => c.eventId === eventFilter.value)
  }
  
  // Apply type filter
  if (typeFilter.value) {
    result = result.filter(c => c.contributionType === typeFilter.value)
  }
  
  // Apply sorting
  if (sortBy.value) {
    const [field, direction] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal = a[field]
      let bVal = b[field]
      
      if (field === 'amount') {
        aVal = parseFloat(aVal) || 0
        bVal = parseFloat(bVal) || 0
      }
      
      if (direction === 'asc') {
        return aVal > bVal ? 1 : -1
      } else {
        return aVal < bVal ? 1 : -1
      }
    })
  }
  
  return result
})

const paginatedContributions = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredBeforePagination.value.slice(start, end)
})

// API Functions
const loadContributions = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/contribution-settings`)
    contributions.value = response.data
  } catch (error) {
    console.error('Error loading contributions:', error)
    ElMessage.error('Failed to load contribution settings')
  } finally {
    loading.value = false
  }
}

const loadEvents = async () => {
  try {
    const response = await axios.get(`${API_BASE}/events`)
    events.value = response.data
  } catch (error) {
    console.error('Error loading events:', error)
  }
}

const handleSearch = () => {
  currentPage.value = 1
}

const handleFilterChange = () => {
  currentPage.value = 1
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'id-desc'
  } else {
    sortBy.value = `${prop}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const editContribution = (contribution) => {
  editingContribution.value = contribution
  Object.assign(contributionForm, {
    name: contribution.name,
    description: contribution.description,
    eventId: contribution.eventId,
    contributionType: contribution.contributionType,
    amount: contribution.amount,
    recurringDay: contribution.recurringDay,
    recurringFrequency: contribution.recurringFrequency
  })
  showCreateDialog.value = true
}

const saveContribution = async () => {
  try {
    await contributionFormRef.value.validate()
    saving.value = true
    
    const formData = {
      name: contributionForm.name,
      description: contributionForm.description,
      eventId: contributionForm.eventId,
      contributionType: contributionForm.contributionType,
      amount: contributionForm.amount,
      recurringDay: contributionForm.contributionType === 'Recurring' ? contributionForm.recurringDay : null,
      recurringFrequency: contributionForm.contributionType === 'Recurring' ? contributionForm.recurringFrequency : null
    }
    
    if (editingContribution.value) {
      // Update existing contribution
      await axios.put(`${API_BASE}/contribution-settings/${editingContribution.value.id}`, formData)
      ElMessage.success('Contribution setting updated successfully')
    } else {
      // Create new contribution
      await axios.post(`${API_BASE}/contribution-settings`, formData)
      ElMessage.success('Contribution setting created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadContributions()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save contribution setting')
    }
  } finally {
    saving.value = false
  }
}

const deleteContribution = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this contribution setting?',
      'Warning',
      {
        confirmButtonText: 'OK',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/contribution-settings/${id}`)
    ElMessage.success('Contribution setting deleted successfully')
    loadContributions()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('Failed to delete contribution setting')
    }
  }
}

const viewContributionDetails = (contribution) => {
  selectedContribution.value = contribution
  showDetailsDialog.value = true
}

const resetForm = () => {
  editingContribution.value = null
  Object.assign(contributionForm, {
    name: '',
    description: '',
    eventId: '',
    contributionType: 'Single',
    amount: 0,
    recurringDay: null,
    recurringFrequency: ''
  })
  contributionFormRef.value?.resetFields()
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
  await loadEvents()
  await loadContributions()
  await refreshPermissions()
})
</script>

<style scoped>
.contribution-settings-container {
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

.contribution-settings-card {
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

.contribution-details {
  padding: 20px 0;
}

.dialog-footer {
  text-align: right;
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.btn-text {
  margin-left: 4px;
}

/* Responsive Design */
.table-container {
  overflow-x: auto;
  margin-bottom: 20px;
}

@media (max-width: 768px) {
  .btn-text {
    display: none;
  }
  
  .action-buttons {
    flex-direction: column;
    gap: 4px;
  }
}
</style>
