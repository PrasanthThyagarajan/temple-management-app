<template>
  <div class="contributions-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Contributions" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Money /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Amount" :value="`₹${summaryStats.totalAmount.toLocaleString()}`">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><CreditCard /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="This Month" :value="summaryStats.thisMonth">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><Calendar /></el-icon>
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

    <el-card class="contributions-card">
      <template #header>
        <div class="card-header">
          <h2>Contribution Management</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Contribution
          </el-button>
        </div>
      </template>

      <!-- Enhanced Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner contributions-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-input
              v-model="searchTerm"
              placeholder="Search contributions..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-select v-model="eventFilter" placeholder="Filter by Event" clearable @change="handleFilterChange">
              <el-option
                v-for="event in events"
                :key="event.id"
                :label="event.name"
                :value="event.id"
              />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-select v-model="devoteeFilter" placeholder="Filter by Devotee" clearable @change="handleFilterChange">
              <el-option
                v-for="devotee in devotees"
                :key="devotee.id"
                :label="devotee.fullName"
                :value="devotee.id"
              />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-select v-model="paymentMethodFilter" placeholder="Filter by Payment Method" clearable @change="handleFilterChange">
              <el-option label="Cash" value="Cash" />
              <el-option label="Card" value="Card" />
              <el-option label="Online" value="Online" />
              <el-option label="Bank Transfer" value="Bank Transfer" />
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
          <el-table-column prop="eventName" label="Event" min-width="150" sortable="custom" show-overflow-tooltip />
          <el-table-column prop="devoteeName" label="Devotee" min-width="150" sortable="custom" show-overflow-tooltip />
          <el-table-column prop="contributionSettingName" label="Contribution Type" min-width="150" show-overflow-tooltip />
          <el-table-column prop="amount" label="Amount" width="120" align="right" sortable="custom">
            <template #default="scope">
              ₹{{ scope.row.amount.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="paymentMethod" label="Payment Method" width="120" align="center">
            <template #default="scope">
              <el-tag :type="getPaymentMethodTagType(scope.row.paymentMethod)">
                {{ scope.row.paymentMethod }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="contributionDate" label="Date" width="120" sortable="custom">
            <template #default="scope">
              {{ formatDate(scope.row.contributionDate) }}
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
      :title="editingContribution ? 'Edit Contribution' : 'Create Contribution'"
      width="700px"
      @close="resetForm"
    >
      <el-form
        ref="contributionFormRef"
        :model="contributionForm"
        :rules="contributionRules"
        label-width="140px"
      >
        <el-form-item label="Event" prop="eventId">
          <el-select 
            v-model="contributionForm.eventId" 
            placeholder="Select event" 
            style="width: 100%"
            @change="handleEventChange"
          >
            <el-option
              v-for="event in events"
              :key="event.id"
              :label="event.name"
              :value="event.id"
            />
          </el-select>
        </el-form-item>
        
        <el-form-item label="Contribution Type" prop="contributionSettingId">
          <el-select 
            v-model="contributionForm.contributionSettingId" 
            placeholder="Select contribution type" 
            style="width: 100%"
            @change="handleContributionSettingChange"
            :disabled="!contributionForm.eventId"
          >
            <el-option
              v-for="setting in filteredContributionSettings"
              :key="setting.id"
              :label="`${setting.name} - ₹${setting.amount}`"
              :value="setting.id"
            />
          </el-select>
        </el-form-item>
        
        <el-form-item label="Devotee" prop="devoteeId">
          <el-select v-model="contributionForm.devoteeId" placeholder="Select devotee" style="width: 100%">
            <el-option
              v-for="devotee in devotees"
              :key="devotee.id"
              :label="devotee.fullName"
              :value="devotee.id"
            />
          </el-select>
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
        
        <el-form-item label="Contribution Date" prop="contributionDate">
          <el-date-picker
            v-model="contributionForm.contributionDate"
            type="datetime"
            placeholder="Select date and time"
            style="width: 100%"
            format="YYYY-MM-DD HH:mm"
            value-format="YYYY-MM-DD HH:mm:ss"
          />
        </el-form-item>
        
        <el-form-item label="Payment Method" prop="paymentMethod">
          <el-select v-model="contributionForm.paymentMethod" placeholder="Select payment method" style="width: 100%">
            <el-option label="Cash" value="Cash" />
            <el-option label="Card" value="Card" />
            <el-option label="Online" value="Online" />
            <el-option label="Bank Transfer" value="Bank Transfer" />
            <el-option label="Cheque" value="Cheque" />
          </el-select>
        </el-form-item>
        
        <el-form-item label="Transaction Ref" prop="transactionReference">
          <el-input
            v-model="contributionForm.transactionReference"
            placeholder="Enter transaction reference (optional)"
          />
        </el-form-item>
        
        <el-form-item label="Notes" prop="notes">
          <el-input
            v-model="contributionForm.notes"
            type="textarea"
            :rows="3"
            placeholder="Enter notes (optional)"
          />
        </el-form-item>
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
      title="Contribution Details"
      width="700px"
    >
      <div v-if="selectedContribution" class="contribution-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Event">{{ selectedContribution.eventName }}</el-descriptions-item>
          <el-descriptions-item label="Devotee">{{ selectedContribution.devoteeName }}</el-descriptions-item>
          <el-descriptions-item label="Contribution Type">{{ selectedContribution.contributionSettingName }}</el-descriptions-item>
          <el-descriptions-item label="Amount">₹{{ selectedContribution.amount.toLocaleString() }}</el-descriptions-item>
          <el-descriptions-item label="Payment Method">
            <el-tag :type="getPaymentMethodTagType(selectedContribution.paymentMethod)">
              {{ selectedContribution.paymentMethod }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Contribution Date">{{ formatDate(selectedContribution.contributionDate) }}</el-descriptions-item>
          <el-descriptions-item label="Transaction Reference">{{ selectedContribution.transactionReference || 'N/A' }}</el-descriptions-item>
          <el-descriptions-item label="Created Date">{{ formatDate(selectedContribution.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="Notes" :span="2">{{ selectedContribution.notes || 'No notes available' }}</el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Filter, Edit, Delete, View, Money, CreditCard, Calendar } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const contributions = ref([])
const events = ref([])
const devotees = ref([])
const contributionSettings = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const eventFilter = ref('')
const devoteeFilter = ref('')
const paymentMethodFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('contributionDate-desc')
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingContribution = ref(null)
const selectedContribution = ref(null)

// Form data
const contributionForm = reactive({
  eventId: '',
  devoteeId: '',
  contributionSettingId: '',
  amount: 0,
  contributionDate: new Date().toISOString().slice(0, 19),
  paymentMethod: 'Cash',
  transactionReference: '',
  notes: ''
})

// Form validation rules
const contributionRules = {
  eventId: [{ required: true, message: 'Event selection is required', trigger: 'change' }],
  devoteeId: [{ required: true, message: 'Devotee selection is required', trigger: 'change' }],
  contributionSettingId: [{ required: true, message: 'Contribution type selection is required', trigger: 'change' }],
  amount: [{ required: true, message: 'Amount is required', trigger: 'blur' }],
  contributionDate: [{ required: true, message: 'Contribution date is required', trigger: 'change' }],
  paymentMethod: [{ required: true, message: 'Payment method is required', trigger: 'change' }]
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
      canCreate.value = await window["templeAuth"].hasCreatePermission('/contributions')
      canUpdate.value = await window["templeAuth"].hasUpdatePermission('/contributions')
      canDelete.value = await window["templeAuth"].hasDeletePermission('/contributions')
    }
  } catch (_) { /* ignore */ }
}

// Computed properties
const filteredContributionSettings = computed(() => {
  if (!contributionForm.eventId) return []
  return contributionSettings.value.filter(cs => cs.eventId === contributionForm.eventId)
})

// Summary Statistics
const summaryStats = computed(() => {
  const total = contributions.value.length
  const totalAmount = contributions.value.reduce((sum, c) => sum + c.amount, 0)
  const thisMonth = contributions.value.filter(c => {
    const contributionDate = new Date(c.contributionDate)
    const now = new Date()
    return contributionDate.getMonth() === now.getMonth() && contributionDate.getFullYear() === now.getFullYear()
  }).length

  return { total, totalAmount, thisMonth }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...contributions.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(c => 
      c.eventName?.toLowerCase().includes(term) ||
      c.devoteeName?.toLowerCase().includes(term) ||
      c.contributionSettingName?.toLowerCase().includes(term) ||
      c.notes?.toLowerCase().includes(term)
    )
  }
  
  // Apply event filter
  if (eventFilter.value) {
    result = result.filter(c => c.eventId === eventFilter.value)
  }
  
  // Apply devotee filter
  if (devoteeFilter.value) {
    result = result.filter(c => c.devoteeId === devoteeFilter.value)
  }
  
  // Apply payment method filter
  if (paymentMethodFilter.value) {
    result = result.filter(c => c.paymentMethod === paymentMethodFilter.value)
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
      } else if (field === 'contributionDate') {
        aVal = new Date(aVal)
        bVal = new Date(bVal)
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
    const response = await axios.get(`${API_BASE}/contributions`)
    contributions.value = response.data
  } catch (error) {
    console.error('Error loading contributions:', error)
    ElMessage.error('Failed to load contributions')
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

const loadDevotees = async () => {
  try {
    const response = await axios.get(`${API_BASE}/devotees`)
    devotees.value = response.data
  } catch (error) {
    console.error('Error loading devotees:', error)
  }
}

const loadContributionSettings = async () => {
  try {
    const response = await axios.get(`${API_BASE}/contribution-settings`)
    contributionSettings.value = response.data
  } catch (error) {
    console.error('Error loading contribution settings:', error)
  }
}

const handleEventChange = () => {
  // Reset contribution setting when event changes
  contributionForm.contributionSettingId = ''
  contributionForm.amount = 0
}

const handleContributionSettingChange = () => {
  // Auto-fill amount when contribution setting is selected
  const selectedSetting = contributionSettings.value.find(cs => cs.id === contributionForm.contributionSettingId)
  if (selectedSetting) {
    contributionForm.amount = selectedSetting.amount
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
    sortBy.value = 'contributionDate-desc'
  } else {
    sortBy.value = `${prop}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const editContribution = (contribution) => {
  editingContribution.value = contribution
  Object.assign(contributionForm, {
    eventId: contribution.eventId,
    devoteeId: contribution.devoteeId,
    contributionSettingId: contribution.contributionSettingId,
    amount: contribution.amount,
    contributionDate: dayjs(contribution.contributionDate).format('YYYY-MM-DD HH:mm:ss'),
    paymentMethod: contribution.paymentMethod,
    transactionReference: contribution.transactionReference || '',
    notes: contribution.notes || ''
  })
  showCreateDialog.value = true
}

const saveContribution = async () => {
  try {
    await contributionFormRef.value.validate()
    saving.value = true
    
    const formData = {
      eventId: contributionForm.eventId,
      devoteeId: contributionForm.devoteeId,
      contributionSettingId: contributionForm.contributionSettingId,
      amount: contributionForm.amount,
      contributionDate: contributionForm.contributionDate,
      paymentMethod: contributionForm.paymentMethod,
      transactionReference: contributionForm.transactionReference,
      notes: contributionForm.notes
    }
    
    if (editingContribution.value) {
      // Update existing contribution
      await axios.put(`${API_BASE}/contributions/${editingContribution.value.id}`, formData)
      ElMessage.success('Contribution updated successfully')
    } else {
      // Create new contribution
      await axios.post(`${API_BASE}/contributions`, formData)
      ElMessage.success('Contribution created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadContributions()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save contribution')
    }
  } finally {
    saving.value = false
  }
}

const deleteContribution = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this contribution?',
      'Warning',
      {
        confirmButtonText: 'OK',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/contributions/${id}`)
    ElMessage.success('Contribution deleted successfully')
    loadContributions()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('Failed to delete contribution')
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
    eventId: '',
    devoteeId: '',
    contributionSettingId: '',
    amount: 0,
    contributionDate: new Date().toISOString().slice(0, 19),
    paymentMethod: 'Cash',
    transactionReference: '',
    notes: ''
  })
  contributionFormRef.value?.resetFields()
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY HH:mm')
}

const getPaymentMethodTagType = (method) => {
  const types = {
    'Cash': '',
    'Card': 'success',
    'Online': 'info',
    'Bank Transfer': 'warning',
    'Cheque': 'danger'
  }
  return types[method] || ''
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
  await Promise.all([
    loadEvents(),
    loadDevotees(),
    loadContributionSettings(),
    loadContributions(),
    refreshPermissions()
  ])
})
</script>

<style scoped>
.contributions-container {
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

.contributions-card {
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

.devotional-banner {
  height: 4px;
  background: linear-gradient(90deg, #ff6b35 0%, #f7931e 25%, #ffd23f 50%, #06ffa5 75%, #1fb3d3 100%);
  border-radius: 2px;
  margin-bottom: 20px;
}

.contributions-banner {
  background: linear-gradient(90deg, #ff9a56 0%, #ffad56 25%, #ffc93c 50%, #06d6a0 75%, #118ab2 100%);
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
