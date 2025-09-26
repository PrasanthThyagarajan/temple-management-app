<template>
  <div class="vouchers-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Vouchers" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Tickets /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card pending">
          <el-statistic title="Pending Approval" :value="summaryStats.pending">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #e6a23c;"><Clock /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card approved">
          <el-statistic title="Approved" :value="summaryStats.approved">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><Check /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic 
            title="Total Amount" 
            :value="summaryStats.totalAmount" 
            prefix="₹"
            :precision="2"
          >
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><Money /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="vouchers-card">
      <template #header>
        <div class="card-header">
          <h2>Vouchers</h2>
          <div class="header-actions">
            <el-button type="info" @click="loadVouchers" :loading="loading">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
            <el-button-group>
              <el-button 
                :type="viewMode === 'table' ? 'primary' : 'default'" 
                @click="viewMode = 'table'"
              >
                <el-icon><Grid /></el-icon>
                Table View
              </el-button>
              <el-button 
                :type="viewMode === 'cards' ? 'primary' : 'default'" 
                @click="viewMode = 'cards'"
              >
                <el-icon><Box /></el-icon>
                Card View
              </el-button>
            </el-button-group>
          </div>
        </div>
      </template>

      <!-- Enhanced Filters -->
      <div class="filters-section">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6">
            <el-input 
              v-model="searchQuery" 
              placeholder="Search vouchers..." 
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="3">
            <el-select 
              v-model="filterEvent" 
              placeholder="Filter by Event" 
              clearable 
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Event" value="" />
              <el-option
                v-for="event in events"
                :key="event.id"
                :label="event.name"
                :value="event.id"
              />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="3">
            <el-select 
              v-model="filterType" 
              placeholder="Filter by Type" 
              clearable 
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Type" value="" />
              <el-option label="Expense" value="Service" />
              <el-option label="Item" value="Event" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="3">
            <el-select 
              v-model="filterStatus" 
              placeholder="Filter by Status" 
              clearable 
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Status" value="" />
              <el-option label="Pending Approval" value="pending" />
              <el-option label="Approved" value="approved" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="3">
            <el-select 
              v-model="filterRequestedBy" 
              placeholder="Filter by Requested By" 
              clearable 
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any User" value="" />
              <el-option
                v-for="user in uniqueRequestedByUsers"
                :key="user"
                :label="user"
                :value="user"
              />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="3">
            <el-select 
              v-model="filterUserRole" 
              placeholder="Filter by User Role" 
              clearable 
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Role" value="" />
              <el-option
                v-for="role in uniqueUserRoles"
                :key="role"
                :label="role"
                :value="role"
              />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="3">
            <el-select 
              v-model="filterApprovedBy" 
              placeholder="Filter by Approved By" 
              clearable 
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Approver" value="" />
              <el-option
                v-for="approver in uniqueApprovedByUsers"
                :key="approver"
                :label="approver"
                :value="approver"
              />
            </el-select>
          </el-col>
        </el-row>
        <el-row :gutter="20" style="margin-top: 10px;">
          <el-col :xs="24" :sm="12" :md="6">
            <el-select
              v-model="sortBy"
              placeholder="Sort by"
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Date (Newest)" value="date-desc" />
              <el-option label="Date (Oldest)" value="date-asc" />
              <el-option label="Amount (High to Low)" value="amount-desc" />
              <el-option label="Amount (Low to High)" value="amount-asc" />
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
            </el-select>
          </el-col>
        </el-row>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-container">
        <el-skeleton :rows="5" animated />
      </div>

      <!-- Table View -->
      <div v-else-if="viewMode === 'table' && !loading" class="table-view">
        <el-table 
          :data="paginatedVouchers" 
          stripe 
          style="width: 100%"
          @sort-change="handleTableSortChange"
        >
          <el-table-column 
            label="Event Name" 
            min-width="150"
            sortable="custom"
            prop="eventName"
          >
            <template #default="scope">
              {{ getEventName(scope.row.eventId) }}
            </template>
          </el-table-column>
          <el-table-column 
            label="Name" 
            min-width="180"
            sortable="custom"
            prop="expenseName"
          >
            <template #default="scope">
              <div>
                <strong>{{ getExpenseName(scope.row) }}</strong>
                <br>
                <el-tag size="small" :type="scope.row.voucherType === 'Service' ? 'primary' : 'success'">
                  {{ scope.row.voucherType === 'Service' ? 'Expense' : 'Item' }}
                </el-tag>
              </div>
            </template>
          </el-table-column>
          <el-table-column 
            prop="price" 
            label="Price" 
            width="120"
            sortable="custom"
          >
            <template #default="scope">
              ₹{{ formatPrice(scope.row.price) }}
            </template>
          </el-table-column>
          <el-table-column label="Quantity" width="100">
            <template #default="scope">
              {{ scope.row.quantity || 1 }}
            </template>
          </el-table-column>
          <el-table-column label="Total" width="120">
            <template #default="scope">
              ₹{{ formatPrice((scope.row.price || 0) * (scope.row.quantity || 1)) }}
            </template>
          </el-table-column>
          <el-table-column label="Date" width="180">
            <template #default="scope">
              {{ formatDate(scope.row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="Requested By" width="150">
            <template #default="scope">
              <div v-if="scope.row.requestedByUserName">
                {{ scope.row.requestedByUserName }}
                <br>
                <small class="text-muted">{{ scope.row.requestedByUserRole }}</small>
              </div>
              <span v-else class="text-muted">—</span>
            </template>
          </el-table-column>
          <el-table-column 
            label="User Role" 
            width="120"
            sortable="custom"
            prop="requestedByUserRole"
          >
            <template #default="scope">
              <el-tag v-if="scope.row.requestedByUserRole" size="small" type="info">
                {{ scope.row.requestedByUserRole }}
              </el-tag>
              <span v-else class="text-muted">—</span>
            </template>
          </el-table-column>
          <el-table-column 
            label="Approved By" 
            width="150"
            sortable="custom"
            prop="approvedByUserName"
          >
            <template #default="scope">
              <div v-if="scope.row.approvedByUserName">
                <strong>{{ scope.row.approvedByUserName }}</strong>
                <br>
                <small class="text-muted">{{ formatDate(scope.row.approvedOn) }}</small>
              </div>
              <span v-else class="text-muted">—</span>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="120" fixed="right">
            <template #default="scope">
              <el-button
                v-if="!scope.row.isApproved && canApprove(scope.row)"
                size="small"
                type="success"
                @click="approveVoucher(scope.row)"
                :loading="approvingIds.includes(scope.row.id)"
              >
                <el-icon><Check /></el-icon>
                Approve
              </el-button>
              <el-button
                v-else-if="scope.row.isApproved"
                size="small"
                type="info"
                disabled
              >
                <el-icon><Check /></el-icon>
                Approved
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Enhanced Pagination -->
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
      </div>

      <!-- Card View -->
      <div v-else-if="viewMode === 'cards' && !loading" class="cards-view">
        <el-row :gutter="20">
          <el-col 
            v-for="voucher in paginatedVouchers" 
            :key="voucher.id" 
            :xs="24" 
            :sm="12" 
            :md="8" 
            :lg="6"
          >
            <el-card class="voucher-card" :class="{ 'approved': voucher.isApproved }">
              <template #header>
                <div class="voucher-card-header">
                  <h4>{{ getExpenseName(voucher) }}</h4>
                  <el-tag 
                    :type="voucher.isApproved ? 'success' : 'warning'"
                    effect="dark"
                    size="small"
                  >
                    {{ voucher.isApproved ? 'Approved' : 'Pending' }}
                  </el-tag>
                </div>
              </template>
              
              <div class="voucher-details">
                <div class="detail-item">
                  <span class="label">Event:</span>
                  <span class="value">{{ getEventName(voucher.eventId) }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Type:</span>
                  <el-tag size="small" :type="voucher.voucherType === 'Service' ? 'primary' : 'success'">
                    {{ voucher.voucherType === 'Service' ? 'Expense' : 'Item' }}
                  </el-tag>
                </div>
                <div class="detail-item">
                  <span class="label">Price:</span>
                  <span class="value price">₹{{ formatPrice(voucher.price) }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Quantity:</span>
                  <span class="value">{{ voucher.quantity || 1 }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Total:</span>
                  <span class="value total">₹{{ formatPrice((voucher.price || 0) * (voucher.quantity || 1)) }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Date:</span>
                  <span class="value">{{ formatDate(voucher.createdAt) }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Requested By:</span>
                  <span class="value">
                    <div v-if="voucher.requestedByUserName">
                      {{ voucher.requestedByUserName }}
                      <br>
                      <el-tag v-if="voucher.requestedByUserRole" size="small" type="info">
                        {{ voucher.requestedByUserRole }}
                      </el-tag>
                    </div>
                    <span v-else>—</span>
                  </span>
                </div>
                <div v-if="voucher.isApproved && voucher.approvedByUserName" class="detail-item">
                  <span class="label">Approved By:</span>
                  <span class="value">
                    {{ voucher.approvedByUserName }}
                    <br>
                    <small class="text-muted">{{ formatDate(voucher.approvedOn) }}</small>
                  </span>
                </div>
              </div>

              <div class="voucher-actions">
                <el-button
                  v-if="!voucher.isApproved && canApprove(voucher)"
                  type="success"
                  size="small"
                  @click="approveVoucher(voucher)"
                  :loading="approvingIds.includes(voucher.id)"
                  style="width: 100%"
                >
                  <el-icon><Check /></el-icon>
                  Approve Voucher
                </el-button>
                <el-button
                  v-else-if="voucher.isApproved"
                  type="info"
                  size="small"
                  disabled
                  style="width: 100%"
                >
                  <el-icon><Check /></el-icon>
                  Already Approved
                </el-button>
              </div>
            </el-card>
          </el-col>
        </el-row>

        <!-- Enhanced Pagination for Card View -->
        <div class="pagination-container">
          <el-pagination
            :current-page="currentPage"
            :page-size="pageSize"
            :page-sizes="[8, 16, 24, 32]"
            :total="filteredBeforePagination.length"
            :background="true"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
          />
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && filteredBeforePagination.length === 0" class="empty-state">
        <el-empty description="No vouchers found" />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Refresh, Grid, Box, Check, Search, Tickets, Clock, Money } from '@element-plus/icons-vue'
import axios from 'axios'
import { useAuth } from '@/stores/auth'

// Data
const auth = useAuth()
const vouchers = ref([])
const events = ref([])
const expenses = ref([])
const expenseServices = ref([])
const loading = ref(false)
const viewMode = ref('table') // 'table' or 'cards'
const approvingIds = ref([])

// Enhanced filters and pagination
const filterEvent = ref('')
const filterType = ref('')
const filterStatus = ref('')
const filterRequestedBy = ref('')
const filterUserRole = ref('')
const filterApprovedBy = ref('')
const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('date-desc')

// API Base
const API_BASE = '/api'

// Unique filter options
const uniqueRequestedByUsers = computed(() => {
  const users = [...new Set(vouchers.value
    .map(v => v.requestedByUserName)
    .filter(name => name && name.trim() !== ''))]
  return users.sort()
})

const uniqueUserRoles = computed(() => {
  const roles = [...new Set(vouchers.value
    .map(v => v.requestedByUserRole)
    .filter(role => role && role.trim() !== ''))]
  return roles.sort()
})

const uniqueApprovedByUsers = computed(() => {
  const approvers = [...new Set(vouchers.value
    .map(v => v.approvedByUserName)
    .filter(name => name && name.trim() !== ''))]
  return approvers.sort()
})

// Enhanced Summary Statistics
const summaryStats = computed(() => {
  return {
    total: vouchers.value.length,
    pending: vouchers.value.filter(v => !v.isApproved).length,
    approved: vouchers.value.filter(v => v.isApproved).length,
    totalAmount: vouchers.value.reduce((sum, v) => sum + ((v.price || 0) * (v.quantity || 1)), 0)
  }
})

// Enhanced filtering and sorting
const filteredBeforePagination = computed(() => {
  let result = [...vouchers.value]

  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(v => {
      const expenseName = getExpenseName(v).toLowerCase()
      const eventName = getEventName(v.eventId).toLowerCase()
      const requestedBy = (v.requestedByUserName || '').toLowerCase()
      
      return expenseName.includes(query) || 
             eventName.includes(query) || 
             requestedBy.includes(query)
    })
  }

  // Apply event filter
  if (filterEvent.value) {
    result = result.filter(v => v.eventId === filterEvent.value)
  }

  // Apply type filter
  if (filterType.value) {
    result = result.filter(v => v.voucherType === filterType.value)
  }

  // Apply status filter
  if (filterStatus.value) {
    if (filterStatus.value === 'pending') {
      result = result.filter(v => !v.isApproved)
    } else if (filterStatus.value === 'approved') {
      result = result.filter(v => v.isApproved)
    }
  }

  // Apply requested by filter
  if (filterRequestedBy.value) {
    result = result.filter(v => v.requestedByUserName === filterRequestedBy.value)
  }

  // Apply user role filter
  if (filterUserRole.value) {
    result = result.filter(v => v.requestedByUserRole === filterUserRole.value)
  }

  // Apply approved by filter
  if (filterApprovedBy.value) {
    result = result.filter(v => v.approvedByUserName === filterApprovedBy.value)
  }

  // Apply sorting
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      
      switch(field) {
        case 'date':
          aVal = new Date(a.createdAt).getTime()
          bVal = new Date(b.createdAt).getTime()
          break
        case 'amount':
          aVal = (a.price || 0) * (a.quantity || 1)
          bVal = (b.price || 0) * (b.quantity || 1)
          break
        case 'name':
          aVal = getExpenseName(a).toLowerCase()
          bVal = getExpenseName(b).toLowerCase()
          break
        case 'userRole':
          aVal = (a.requestedByUserRole || '').toLowerCase()
          bVal = (b.requestedByUserRole || '').toLowerCase()
          break
        case 'approvedBy':
          aVal = (a.approvedByUserName || '').toLowerCase()
          bVal = (b.approvedByUserName || '').toLowerCase()
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
const paginatedVouchers = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

// Legacy computed for backward compatibility
const filteredVouchers = computed(() => filteredBeforePagination.value)

// Methods
const loadVouchers = async () => {
  loading.value = true
  try {
    const [vouchersRes, eventsRes, expensesRes, servicesRes] = await Promise.all([
      axios.get(`${API_BASE}/vouchers`),
      axios.get(`${API_BASE}/events`),
      axios.get(`${API_BASE}/expense-items`),
      axios.get(`${API_BASE}/expense-services`)
    ])
    
    vouchers.value = vouchersRes.data || []
    events.value = eventsRes.data || []
    expenses.value = expensesRes.data || []
    expenseServices.value = servicesRes.data || []
    
    // Enhance vouchers with additional data
    vouchers.value = await enhanceVoucherData(vouchers.value)
    
  } catch (error) {
    console.error('Failed to load vouchers:', error)
    ElMessage.error('Failed to load vouchers')
  } finally {
    loading.value = false
  }
}

const enhanceVoucherData = async (voucherList) => {
  // Get unique expense IDs
  const expenseIds = [...new Set(voucherList.map(v => v.id))]
  
  try {
    // Fetch expense details for each voucher
    const expensePromises = expenseIds.map(id => 
      axios.get(`${API_BASE}/event-expenses/${id}`).catch(() => null)
    )
    
    const expenseDetails = await Promise.all(expensePromises)
    const expenseMap = {}
    
    expenseDetails.forEach((res, index) => {
      if (res && res.data) {
        expenseMap[expenseIds[index]] = res.data
      }
    })
    
    // Enhance vouchers with expense details
    return voucherList.map(voucher => ({
      ...voucher,
      price: expenseMap[voucher.id]?.price || 0,
      quantity: expenseMap[voucher.id]?.quantity || 1,
      requestedByUserName: expenseMap[voucher.id]?.requestedByUserName || '',
      requestedByUserRole: expenseMap[voucher.id]?.requestedByUserRole || '',
      approvedByUserName: expenseMap[voucher.id]?.approvedByUserName || '',
      approvedOn: expenseMap[voucher.id]?.approvedOn || null
    }))
  } catch (error) {
    console.error('Error enhancing voucher data:', error)
    return voucherList
  }
}

const getEventName = (eventId) => {
  const event = events.value.find(e => e.id === eventId)
  return event ? event.name : 'Unknown Event'
}

const getExpenseName = (voucher) => {
  if (voucher.voucherType === 'Service') {
    const service = expenseServices.value.find(s => s.id === voucher.serviceId)
    return service ? service.name : 'Unknown Service'
  } else {
    const item = expenses.value.find(i => i.id === voucher.expenseId)
    return item ? item.name : 'Unknown Item'
  }
}

const formatPrice = (price) => {
  return (price || 0).toFixed(2)
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const canApprove = (voucher) => {
  // Check if user has permission to approve
  // This should be based on the approval role configuration
  return auth.hasPermission('ExpenseApproval') || auth.hasRole('Admin')
}

const approveVoucher = async (voucher) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to approve this ${voucher.voucherType === 'Service' ? 'expense' : 'item'} voucher for ${getExpenseName(voucher)}?`,
      'Confirm Approval',
      {
        confirmButtonText: 'Approve',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    approvingIds.value.push(voucher.id)
    
    // Call the expense approval endpoint
    await axios.put(`${API_BASE}/event-expenses/${voucher.id}/approve`)
    
    ElMessage.success('Voucher approved successfully')
    
    // Reload vouchers to get updated data
    await loadVouchers()
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to approve voucher:', error)
      ElMessage.error('Failed to approve voucher')
    }
  } finally {
    approvingIds.value = approvingIds.value.filter(id => id !== voucher.id)
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
    sortBy.value = 'date-desc'
  } else {
    let field = prop
    if (prop === 'createdAt') field = 'date'
    else if (prop === 'price') field = 'amount'
    else if (prop === 'expenseName') field = 'name'
    else if (prop === 'requestedByUserRole') field = 'userRole'
    else if (prop === 'approvedByUserName') field = 'approvedBy'
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

// Legacy method for backward compatibility
const filterVouchers = () => {
  // Filtering is handled by computed property
}

// Lifecycle
onMounted(() => {
  loadVouchers()
})
</script>

<style scoped>
.vouchers-container {
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

.summary-card.pending {
  border-left: 4px solid #e6a23c;
}

.summary-card.approved {
  border-left: 4px solid #67c23a;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.vouchers-card {
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

.header-actions {
  display: flex;
  gap: 10px;
}

.filters-section {
  margin-bottom: 20px;
}

.loading-container {
  padding: 20px;
}

/* Table View Styles */
.table-view {
  margin-top: 20px;
}

.text-muted {
  color: #909399;
  font-size: 12px;
}

/* Card View Styles */
.cards-view {
  margin-top: 20px;
}

.voucher-card {
  margin-bottom: 20px;
  transition: all 0.3s ease;
}

.voucher-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.voucher-card.approved {
  border-color: #67C23A;
}

.voucher-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.voucher-card-header h4 {
  margin: 0;
  font-size: 16px;
  color: #303133;
}

.voucher-details {
  margin-bottom: 15px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 10px;
  font-size: 14px;
}

.detail-item .label {
  color: #909399;
  font-weight: 500;
  min-width: 100px;
}

.detail-item .value {
  color: #303133;
  text-align: right;
  flex: 1;
}

.detail-item .value.price {
  color: #F56C6C;
  font-weight: 600;
}

.detail-item .value.total {
  color: #E6A23C;
  font-weight: 700;
  font-size: 16px;
}

.voucher-actions {
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #EBEEF5;
}

.empty-state {
  padding: 40px 0;
}

/* Responsive */
@media (max-width: 768px) {
  .header-actions {
    flex-direction: column;
    width: 100%;
  }
  
  .header-actions > * {
    width: 100%;
  }
  
  .filters-section .el-col {
    margin-bottom: 10px;
  }
}
</style>
