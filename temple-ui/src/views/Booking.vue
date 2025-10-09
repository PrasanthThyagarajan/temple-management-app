<template>
  <div class="booking-container">
    <el-card class="booking-card">
      <template #header>
        <div class="card-header">
          <h2>Prebooking</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            New Booking
          </el-button>
        </div>
      </template>

      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-input v-model="searchTerm" placeholder="Search bookings..." clearable @input="handleSearch">
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select v-model="statusFilter" placeholder="Filter by Status" clearable @change="handleFilterChange" style="width: 100%">
              <el-option label="Any Status" value="" />
              <el-option label="Pending" value="Pending" />
              <el-option label="Approved" value="Approved" />
              <el-option label="Rejected" value="Rejected" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select v-model="sortBy" placeholder="Sort by" @change="handleSortChange" style="width: 100%">
              <el-option label="Date (Newest)" value="date-desc" />
              <el-option label="Date (Oldest)" value="date-asc" />
              <el-option label="Recently Added" value="id-desc" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-date-picker v-model="dateFilter" type="date" placeholder="Filter by Date" format="YYYY-MM-DD" value-format="YYYY-MM-DD" @change="handleDateFilter" style="width: 100%" />
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-button @click="loadBookings" :loading="loading">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
          </el-col>
        </el-row>
      </div>

      <div class="table-container">
        <el-table :data="paginatedBookings" v-loading="loading" stripe style="width: 100%" @row-click="handleRowClick" @sort-change="handleTableSortChange" v-if="canRead">
          <el-table-column prop="bookingDate" label="Booking Date" width="120" sortable="custom">
            <template #default="scope">{{ formatDate(scope.row.bookingDate) }}</template>
          </el-table-column>
          <el-table-column prop="userId" label="User ID" width="100" />
          <el-table-column prop="productId" label="Product" min-width="150">
            <template #default="scope">{{ getProductName(scope.row.productId) }}</template>
          </el-table-column>
          <el-table-column prop="categoryId" label="Category" min-width="140">
            <template #default="scope">{{ getProductCategory(scope.row.productId) }}</template>
          </el-table-column>
          <el-table-column prop="status" label="Status" width="120">
            <template #default="scope">
              <el-tag :type="getStatusTag(scope.row.status)">{{ scope.row.status }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="notes" label="Notes" min-width="150" show-overflow-tooltip />
          <el-table-column label="Actions" width="280" fixed="right" v-if="canUpdate || canDelete || auth.hasPermission('BookingApproval') || auth.hasRole('Admin')">
            <template #default="scope">
              <template v-if="scope.row.status !== 'Rejected'">
                <el-button
                  v-if="scope.row.status === 'Pending' && canApprove(scope.row)"
                  size="small"
                  type="success"
                  @click.stop="approveBooking(scope.row)"
                  :loading="approvingIds.includes(scope.row.id)"
                >
                  <el-icon><Check /></el-icon>
                  Approve
                </el-button>
                <el-button
                  v-else-if="scope.row.status === 'Approved'"
                  size="small"
                  type="info"
                  disabled
                >
                  <el-icon><Check /></el-icon>
                  Approved
                </el-button>

                <el-button
                  v-if="scope.row.status === 'Pending' && canReject(scope.row)"
                  size="small"
                  type="warning"
                  @click.stop="rejectBooking(scope.row)"
                  :loading="rejectingIds.includes(scope.row.id)"
                >
                  <el-icon><Close /></el-icon>
                  Reject
                </el-button>

                <el-button
                  v-if="canDelete"
                  size="small"
                  type="danger"
                  plain
                  @click.stop="deleteBooking(scope.row)"
                  :loading="deletingIds.includes(scope.row.id)"
                >
                  <el-icon><Delete /></el-icon>
                  Delete
                </el-button>
              </template>
              <template v-else>
                <el-button size="small" @click.stop="viewBookingDetails(scope.row)">
                  <el-icon><View /></el-icon>
                  View
                </el-button>
              </template>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="pagination-container" v-if="canRead">
        <el-pagination :current-page="currentPage" :page-size="pageSize" :page-sizes="[10, 20, 50, 100]" :total="filteredBeforePagination.length" :background="true" layout="total, sizes, prev, pager, next, jumper" @size-change="handleSizeChange" @current-change="handleCurrentChange" />
      </div>
    </el-card>

    <!-- Create/Edit Booking Dialog -->
    <el-dialog v-model="showCreateDialog" :title="editingBooking ? 'Edit Booking' : 'New Booking'" :width="dialogWidth" :fullscreen="isMobileDialog">
      <el-form ref="bookingFormRef" :model="bookingForm" :rules="bookingRules" :label-width="formLabelWidth" :label-position="labelPosition">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12"><el-form-item label="Name" prop="customerName"><el-input v-model="bookingForm.customerName" /></el-form-item></el-col>
          <el-col :xs="24" :sm="12"><el-form-item label="Phone" prop="customerPhone"><el-input v-model="bookingForm.customerPhone" /></el-form-item></el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12"><el-form-item label="Booking Date" prop="saleDate"><el-date-picker v-model="bookingForm.saleDate" type="date" format="YYYY-MM-DD" value-format="YYYY-MM-DD" style="width: 100%" /></el-form-item></el-col>
          <el-col :xs="24" :sm="12"><el-form-item label="Payment Method" prop="paymentMethod"><el-select v-model="bookingForm.paymentMethod" style="width: 100%"><el-option label="Cash" value="Cash"/><el-option label="Card" value="Card"/><el-option label="UPI" value="UPI"/><el-option label="Online" value="Online"/></el-select></el-form-item></el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12"><el-form-item label="Category" prop="categoryId"><el-select v-model="bookingForm.categoryId" style="width: 100%" clearable @change="handleCategoryChange"><el-option v-for="category in categories" :key="category.id" :label="category.name" :value="category.id"/></el-select></el-form-item></el-col>
          <el-col :xs="24" :sm="12"><el-form-item label="Product" prop="productId"><el-select v-model="bookingForm.productId" style="width: 100%" :disabled="!bookingForm.categoryId" clearable><el-option v-for="product in prebookingProductsByCategory" :key="product.id" :label="product.name" :value="product.id"/></el-select></el-form-item></el-col>
        </el-row>
        <el-form-item label="Notes" prop="notes"><el-input v-model="bookingForm.notes" type="textarea" :rows="3" /></el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveBooking" :loading="saving" v-if="canCreate || canUpdate">{{ editingBooking ? 'Update' : 'Create' }}</el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Booking Details Dialog -->
    <el-dialog v-model="showDetailsDialog" title="Booking Details" width="600px">
      <div v-if="selectedBooking">
        <el-descriptions :column="1" border>
          <el-descriptions-item label="Booking Date">{{ formatDate(selectedBooking.bookingDate) }}</el-descriptions-item>
          <el-descriptions-item label="User ID">{{ selectedBooking.userId }}</el-descriptions-item>
          <el-descriptions-item label="Product">{{ getProductName(selectedBooking.productId) }}</el-descriptions-item>
          <el-descriptions-item label="Category">{{ getProductCategory(selectedBooking.productId) }}</el-descriptions-item>
          <el-descriptions-item label="Status">
            <el-tag :type="getStatusTag(selectedBooking.status)">{{ selectedBooking.status }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Payment Method">{{ selectedBooking.paymentMethod }}</el-descriptions-item>
          <el-descriptions-item label="Notes">{{ selectedBooking.notes || '—' }}</el-descriptions-item>
        </el-descriptions>
      </div>
      <template #footer>
        <span class="dialog-footer">
          <el-button type="primary" @click="showDetailsDialog = false">Close</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Check, Close, Delete, View } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'
import { useAuth } from '../stores/auth.js'

const auth = useAuth()

const canRead = ref(false)
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

const bookings = ref([])
const products = ref([])
const categories = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const dateFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('date-desc')
const showCreateDialog = ref(false)
const editingBooking = ref(null)
const selectedBooking = ref(null)
const showDetailsDialog = ref(false)
const approvingIds = ref([])
const rejectingIds = ref([])
const deletingIds = ref([])

const windowWidth = ref(window.innerWidth)
const isMobileDialog = computed(() => windowWidth.value < 768)
const labelPosition = computed(() => (windowWidth.value < 640 ? 'top' : 'right'))
const formLabelWidth = computed(() => (windowWidth.value < 640 ? '100px' : '120px'))
const dialogWidth = computed(() => {
  const width = windowWidth.value
  if (width < 576) return '100%'
  if (width < 768) return '95%'
  if (width < 992) return '90%'
  return Math.min(width * 0.75, 1200) + 'px'
})

const bookingForm = reactive({
  customerName: '',
  customerPhone: '',
  saleDate: dayjs().format('YYYY-MM-DD'),
  paymentMethod: 'Cash',
  notes: '',
  categoryId: null,
  productId: null,
  salesBookingStatusId: 2 // InProgress - set automatically
})

const bookingRules = {
  customerName: [{ required: true, message: 'Customer name is required', trigger: 'blur' }],
  customerPhone: [{ required: true, message: 'Customer phone is required', trigger: 'blur' }],
  saleDate: [{ required: true, message: 'Booking date is required', trigger: 'change' }],
  categoryId: [{ required: true, message: 'Category is required', trigger: 'change' }],
  productId: [{ required: true, message: 'Product is required', trigger: 'change' }]
}

const API_BASE = '/api'

const filteredBeforePagination = computed(() => {
  let result = [...bookings.value]
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(b => 
      b.userId?.toString().includes(term) || 
      getProductName(b.productId).toLowerCase().includes(term) ||
      b.notes?.toLowerCase().includes(term)
    )
  }
  if (statusFilter.value) {
    result = result.filter(b => b.status === statusFilter.value)
  }
  if (dateFilter.value) {
    const d = dayjs(dateFilter.value).format('YYYY-MM-DD')
    result = result.filter(b => dayjs(b.bookingDate).format('YYYY-MM-DD') === d)
  }
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      switch (field) {
        case 'date': aVal = new Date(a.bookingDate).getTime(); bVal = new Date(b.bookingDate).getTime(); break
        case 'id': aVal = a.id; bVal = b.id; break
        default: aVal = a[field]; bVal = b[field]
      }
      return order === 'asc' ? (aVal < bVal ? -1 : aVal > bVal ? 1 : 0) : (aVal > bVal ? -1 : aVal < bVal ? 1 : 0)
    })
  }
  return result
})

const paginatedBookings = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

const prebookingProducts = computed(() => {
  return products.value.filter(p => p.isPreBookingAvailable)
})

const prebookingProductsByCategory = computed(() => {
  if (!bookingForm.categoryId) return []
  return prebookingProducts.value.filter(p => 
    p.categoryId === bookingForm.categoryId || p.categoryNavigation?.id === bookingForm.categoryId
  )
})

const getProductName = (productId) => {
  if (!productId) return '—'
  const p = products.value.find(x => x.id === productId)
  return p?.name || '—'
}
const getProductCategory = (productId) => {
  if (!productId) return '—'
  const p = products.value.find(x => x.id === productId)
  return p?.categoryNavigation?.name || p?.category || '—'
}
const getBookingStatusText = (id) => ({1:'NoBooking',2:'InProgress',3:'Approved',4:'Failed'})[id] || '—'
const getBookingStatusTag = (id) => ({1:'',2:'warning',3:'success',4:'danger'})[id] || ''
const getStatusTag = (status) => {
  const statusMap = { 'Pending': 'warning', 'Approved': 'success', 'Rejected': 'danger' }
  return statusMap[status] || 'info'
}

const loadBookings = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/bookings`)
    console.log('Bookings loaded:', response.data)
    bookings.value = response.data || []
  } catch (e) {
    console.error('Error loading bookings:', e)
    console.error('Error response:', e.response?.data)
    // Don't show error message if it's just empty data
    if (e.response?.status !== 404) {
      ElMessage.error(`Failed to load bookings: ${e.response?.data?.title || e.message}`)
    }
    bookings.value = []
  } finally {
    loading.value = false
  }
}
const loadProducts = async () => { const r = await axios.get(`${API_BASE}/products`); products.value = r.data }
const loadCategories = async () => { const r = await axios.get(`${API_BASE}/categories/active`); categories.value = r.data }

const handleSearch = () => { currentPage.value = 1 }
const handleFilterChange = () => { currentPage.value = 1 }
const handleSortChange = () => {}
const handleTableSortChange = ({ prop, order }) => {
  if (!order) { sortBy.value = 'date-desc' } else {
    let field = prop; if (prop === 'saleDate') field = 'date'; sortBy.value = `${field}-${order === 'ascending' ? 'asc':'desc'}`
  }
}
const handleDateFilter = () => { currentPage.value = 1 }
const handleCategoryChange = () => { bookingForm.productId = null }

const bookingFormRef = ref()
const saveBooking = async () => {
  try {
    await bookingFormRef.value.validate()
    saving.value = true
    const payload = {
      userId: 1,
      staffId: 1,
      bookingDate: bookingForm.saleDate,
      productId: bookingForm.productId,
      categoryId: bookingForm.categoryId,
      estimatedAmount: 0,
      paymentMethod: bookingForm.paymentMethod,
      notes: bookingForm.notes
    }
    await axios.post(`${API_BASE}/bookings`, payload)
    ElMessage.success('Booking created successfully')
    showCreateDialog.value = false
    resetForm()
    await loadBookings()
  } catch (error) {
    console.error('Error saving booking:', error)
    const errorMsg = error.response?.data?.title || error.response?.data || error.message || 'Failed to save booking'
    ElMessage.error(errorMsg)
  } finally { saving.value = false }
}

const sendVerification = async (row) => {
  try {
    await axios.get(`${API_BASE}/sales/verify-booking`, { params: { token: row.bookingToken } })
    ElMessage.success('Booking verified successfully')
    loadBookings()
  } catch (e) {
    ElMessage.error('Failed to verify booking')
  }
}

const handleRowClick = (row) => { selectedBooking.value = row }
// Details view removed from Actions per new UX; keeping helper for possible future use
const viewBookingDetails = (row) => { selectedBooking.value = row; showDetailsDialog.value = true }

// Permissions refresh for this page
const refreshPagePermissions = async () => {
  try {
    if (window && window["templeAuth"]) {
      canRead.value = await window["templeAuth"].hasPageReadPermission('/bookings')
      canCreate.value = await window["templeAuth"].hasCreatePermission('/bookings')
      canUpdate.value = await window["templeAuth"].hasUpdatePermission('/bookings')
      canDelete.value = await window["templeAuth"].hasDeletePermission('/bookings')
    }
  } catch (_) { /* ignore */ }
}
const handleSizeChange = (val) => { pageSize.value = val; currentPage.value = 1 }
const handleCurrentChange = (val) => { currentPage.value = val }

const resetForm = () => {
  editingBooking.value = null
  Object.assign(bookingForm, {
    customerName: '',
    customerPhone: '',
    saleDate: dayjs().format('YYYY-MM-DD'),
    paymentMethod: 'Cash',
    notes: '',
    categoryId: null,
    productId: null,
    salesBookingStatusId: 2
  })
  bookingFormRef.value?.resetFields()
}

const formatDate = (date) => {
  return date ? dayjs(date).format('YYYY-MM-DD') : '—'
}

const canApprove = (booking) => {
  // Check if user has permission to approve
  // This should be based on the approval role configuration
  return canUpdate.value || auth.hasPermission('BookingApproval') || auth.hasRole('Admin')
}

const canReject = (booking) => {
  // Allow reject for users who can update or Admin; only when Pending (checked in template)
  return canUpdate.value || auth.hasRole('Admin')
}

const approveBooking = async (booking) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to approve this booking for ${getProductName(booking.productId)}?`,
      'Confirm Approval',
      {
        confirmButtonText: 'Approve',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    approvingIds.value.push(booking.id)
    
    // Get current user ID from auth (defaulting to 1 for now)
    const userId = auth.user.value?.userId || 1
    
    // Call the booking approval endpoint
    await axios.put(`${API_BASE}/bookings/${booking.id}/approve`, null, {
      params: { approvedBy: userId }
    })
    
    ElMessage.success('Booking approved successfully')
    
    // Reload bookings to get updated data
    await loadBookings()
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to approve booking:', error)
      ElMessage.error('Failed to approve booking')
    }
  } finally {
    approvingIds.value = approvingIds.value.filter(id => id !== booking.id)
  }
}

const rejectBooking = async (booking) => {
  try {
    await ElMessageBox.confirm(
      `Reject this booking for ${getProductName(booking.productId)}?`,
      'Confirm Rejection',
      { confirmButtonText: 'Reject', cancelButtonText: 'Cancel', type: 'warning' }
    )

    rejectingIds.value.push(booking.id)
    const userId = auth.user.value?.userId || 1
    await axios.put(`${API_BASE}/bookings/${booking.id}/reject`, null, { params: { approvedBy: userId } })
    ElMessage.success('Booking rejected')
    await loadBookings()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to reject booking:', error)
      ElMessage.error('Failed to reject booking')
    }
  } finally {
    rejectingIds.value = rejectingIds.value.filter(id => id !== booking.id)
  }
}

const deleteBooking = async (booking) => {
  try {
    await ElMessageBox.confirm(
      'This will permanently delete the booking. Continue?',
      'Delete Booking',
      { confirmButtonText: 'Delete', cancelButtonText: 'Cancel', type: 'error' }
    )
    deletingIds.value.push(booking.id)
    await axios.delete(`${API_BASE}/bookings/${booking.id}`)
    ElMessage.success('Booking deleted')
    await loadBookings()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to delete booking:', error)
      ElMessage.error('Failed to delete booking')
    }
  } finally {
    deletingIds.value = deletingIds.value.filter(id => id !== booking.id)
  }
}

onMounted(async () => {
  // Ensure latest permissions are loaded and applied for this page
  try { if (window && window["templeAuth"]) { await window["templeAuth"].refreshPermissions() } } catch (_) {}
  await refreshPagePermissions()
  await loadProducts(); await loadCategories(); await loadBookings();
})
</script>

<style scoped>
.booking-container { padding: 20px; }
.booking-card { margin-bottom: 20px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-filters { margin-bottom: 20px; }
.table-container { width: 100%; overflow-x: auto; }
.pagination-container { margin-top: 20px; text-align: right; }
</style>


