<template>
  <div class="sales-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Sales" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><ShoppingCart /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic 
            title="Total Revenue" 
            :value="summaryStats.revenue" 
            prefix="₹"
            :precision="2"
          >
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><Money /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Today's Sales" :value="summaryStats.today">
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

    <el-card class="sales-card">
      <template #header>
        <div class="card-header">
          <h2>Sales Management</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            New Sale
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner sales-banner"></div>

      <!-- Enhanced Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Category" prop="categoryId">
              <el-select v-model="saleForm.categoryId" placeholder="Select category" style="width: 100%" clearable @change="handleCategoryChange">
                <el-option
                  v-for="category in categories"
                  :key="category.id"
                  :label="category.name"
                  :value="category.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Product" prop="productId">
              <el-select v-model="saleForm.productId" placeholder="Select product" style="width: 100%" :disabled="!saleForm.categoryId" clearable>
                <el-option
                  v-for="product in productsBySelectedCategory"
                  :key="product.id"
                  :label="product.name"
                  :value="product.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Booking Status" prop="salesBookingStatusId">
              <el-select v-model="saleForm.salesBookingStatusId" placeholder="Select status" style="width: 100%">
                <el-option label="NoBooking" :value="1" />
                <el-option label="InProgress" :value="2" />
                <el-option label="Approved" :value="3" />
                <el-option label="Failed" :value="4" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-input
              v-model="searchTerm"
              placeholder="Search sales..."
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
              v-model="statusFilter"
              placeholder="Filter by Status"
              clearable
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Status" value="" />
              <el-option label="Completed" value="Completed" />
              <el-option label="Pending" value="Pending" />
              <el-option label="Cancelled" value="Cancelled" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
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
              <el-option label="Recently Added" value="id-desc" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-date-picker
              v-model="dateFilter"
              type="date"
              placeholder="Filter by Date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              @change="handleDateFilter"
              style="width: 100%"
            />
          </el-col>
          <el-col :xs="24" :sm="24" :md="8" :lg="8" :xl="8">
            <el-button @click="loadSales" :loading="loading">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
            <el-button type="success" @click="exportSales">
              <el-icon><Download /></el-icon>
              Export
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Sales Table -->
      <div class="table-container">
        <el-table
          :data="paginatedSales"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
          @sort-change="handleTableSortChange"
        >
        <el-table-column 
          prop="productId" 
          label="Product" 
          min-width="150"
        >
          <template #default="scope">
            {{ getProductName(scope.row.productId) }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="categoryName" 
          label="Category" 
          min-width="140"
        >
          <template #default="scope">
            {{ getProductCategory(scope.row.productId) }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="saleDate" 
          label="Sale Date" 
          width="120"
          sortable="custom"
        >
          <template #default="scope">
            {{ formatDate(scope.row.saleDate) }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="customerName" 
          label="Customer" 
          min-width="150" 
          sortable="custom"
        />
        <el-table-column prop="customerPhone" label="Phone" width="120" />
        <el-table-column 
          prop="totalAmount" 
          label="Total Amount" 
          width="120"
          sortable="custom"
        >
          <template #default="scope">
            ₹{{ scope.row.totalAmount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column prop="discountAmount" label="Discount" width="100">
          <template #default="scope">
            ₹{{ scope.row.discountAmount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="finalAmount" 
          label="Final Amount" 
          width="120"
          sortable="custom"
        >
          <template #default="scope">
            ₹{{ scope.row.finalAmount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="salesBookingStatusId" 
          label="Booking Status" 
          width="140"
        >
          <template #default="scope">
            <el-tag :type="getBookingStatusTag(scope.row.salesBookingStatusId)">
              {{ getBookingStatusText(scope.row.salesBookingStatusId) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="paymentMethod" label="Payment" width="100" />
        <el-table-column prop="notes" label="Notes" min-width="150" show-overflow-tooltip />
        <el-table-column label="Actions" width="200" fixed="right" v-if="canUpdate || canDelete">
          <template #default="scope">
            <el-button size="small" @click.stop="viewSaleDetails(scope.row)">
              <el-icon><View /></el-icon>
              View
            </el-button>
            <el-button size="small" @click.stop="editSale(scope.row)" v-if="canUpdate">
              <el-icon><Edit /></el-icon>
              Edit
            </el-button>
            <el-button
              size="small"
              type="success"
              @click.stop="updateSaleStatus(scope.row.id, 'Completed')"
              v-if="scope.row.status === 'Pending' && canUpdate"
            >
              <el-icon><Check /></el-icon>
              Complete
            </el-button>
            <el-button
              size="small"
              type="danger"
              @click.stop="deleteSale(scope.row.id)"
              v-if="canDelete"
            >
              <el-icon><Delete /></el-icon>
              Delete
            </el-button>
          </template>
        </el-table-column>
        </el-table>
      </div>

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
    </el-card>

    <!-- Create/Edit Sale Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingSale ? 'Edit Sale' : 'New Sale'"
      :width="isMobileDialog ? '100%' : '85%'"
      :fullscreen="isMobileDialog"
      class="sale-dialog"
    >
      <el-form
        ref="saleFormRef"
        :model="saleForm"
        :rules="saleRules"
        :label-width="formLabelWidth"
        :label-position="labelPosition"
      >
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Customer Name" prop="customerName">
              <el-input v-model="saleForm.customerName" placeholder="Enter customer name" />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Customer Phone" prop="customerPhone">
              <el-input v-model="saleForm.customerPhone" placeholder="Enter phone number" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Sale Date" prop="saleDate">
              <el-date-picker
                v-model="saleForm.saleDate"
                type="date"
                placeholder="Select sale date"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Payment Method" prop="paymentMethod">
              <el-select v-model="saleForm.paymentMethod" placeholder="Select payment method" style="width: 100%">
                <el-option label="Cash" value="Cash" />
                <el-option label="Card" value="Card" />
                <el-option label="UPI" value="UPI" />
                <el-option label="Online" value="Online" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Discount Amount" prop="discountAmount">
              <el-input-number
                v-model="saleForm.discountAmount"
                :min="0"
                :precision="2"
                style="width: 100%"
                placeholder="Enter discount amount"
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <!-- Sale Items Section -->
        <el-divider content-position="left">Sale Items</el-divider>
        <div class="sale-items">
          <div v-for="(item, index) in saleForm.saleItems" :key="index" class="sale-item">
            <el-row :gutter="0" class="sale-item-row">
              <el-col :xs="24" :sm="24" :md="8" :lg="8">
                <el-form-item :label="`Product ${index + 1}`" :prop="`saleItems.${index}.productId`">
              <el-select
                    v-model="item.productId"
                    placeholder="Select product"
                    style="width: 100%"
                filterable
                clearable
                    @change="updateItemDetails(index)"
                  >
                    <el-option
                      v-for="product in products"
                      :key="product.id"
                      :label="product.name"
                      :value="product.id"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :xs="12" :sm="12" :md="4" :lg="4">
                <el-form-item :label="`Quantity`" :prop="`saleItems.${index}.quantity`">
                  <el-input-number
                    v-model="item.quantity"
                    :min="1"
                    style="width: 100%"
                    @change="calculateItemTotal(index)"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="12" :sm="12" :md="5" :lg="5">
                <el-form-item :label="`Price`">
                  <el-input-number
                    v-model="item.unitPrice"
                    :min="0"
                    :precision="2"
                    style="width: 100%"
                    @change="calculateItemTotal(index)"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="12" :sm="12" :md="5" :lg="5">
                <el-form-item :label="`Total`">
              <el-input-number
                    v-model="item.totalPrice"
                    :min="0"
                    :precision="2"
                style="width: 100%"
                    disabled
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="2" :lg="2" class="action-col">
                <div class="action-wrap">
                  <el-button
                    type="danger"
                    size="small"
                    @click="removeSaleItem(index)"
                    :disabled="saleForm.saleItems.length === 1"
                  >
                    <el-icon><Delete /></el-icon>
                  </el-button>
                </div>
              </el-col>
            </el-row>
          </div>
          <el-button class="add-item-btn" type="dashed" @click="addSaleItem">
            <el-icon><Plus /></el-icon>
            Add Item
          </el-button>
        </div>

        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Event" prop="eventId">
              <el-select v-model="saleForm.eventId" placeholder="Select event" style="width: 100%">
                <el-option
                  v-for="event in events"
                  :key="event.id"
                  :label="event.name"
                  :value="event.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Total Amount">
              <el-input-number
                v-model="saleForm.totalAmount"
                :min="0"
                :precision="2"
                style="width: 100%"
                disabled
              />
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Final Amount">
              <el-input-number
                v-model="saleForm.finalAmount"
                :min="0"
                :precision="2"
                style="width: 100%"
                disabled
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-form-item label="Notes" prop="notes" :label-width="isMobileDialog ? '100px' : '120px'">
          <el-input
            v-model="saleForm.notes"
            type="textarea"
            :rows="3"
            placeholder="Enter any additional notes"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveSale" :loading="saving" v-if="canCreate || canUpdate">
            {{ editingSale ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Sale Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Sale Details"
      :width="detailsDialogWidth"
      :fullscreen="isMobileDialog"
    >
      <div v-if="selectedSale" class="sale-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Product">
            {{ getProductName(selectedSale.productId) }}
          </el-descriptions-item>
          <el-descriptions-item label="Category">
            {{ getProductCategory(selectedSale.productId) }}
          </el-descriptions-item>
          <el-descriptions-item label="Sale Date">
            {{ formatDate(selectedSale.saleDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="Customer Name">{{ selectedSale.customerName }}</el-descriptions-item>
          <el-descriptions-item label="Customer Phone">{{ selectedSale.customerPhone }}</el-descriptions-item>
          <el-descriptions-item label="Payment Method">{{ selectedSale.paymentMethod }}</el-descriptions-item>
          <el-descriptions-item label="Total Amount">
            ₹{{ selectedSale.totalAmount.toLocaleString() }}
          </el-descriptions-item>
          <el-descriptions-item label="Discount Amount">
            ₹{{ selectedSale.discountAmount.toLocaleString() }}
          </el-descriptions-item>
          <el-descriptions-item label="Final Amount">
            ₹{{ selectedSale.finalAmount.toLocaleString() }}
          </el-descriptions-item>
          <el-descriptions-item label="Booking Status">
            {{ getBookingStatusText(selectedSale.salesBookingStatusId) }}
          </el-descriptions-item>
          <el-descriptions-item label="Notes" :span="2">
            {{ selectedSale.notes || 'No notes available' }}
          </el-descriptions-item>
        </el-descriptions>

        <!-- Sale Items -->
        <el-divider content-position="left">Sale Items</el-divider>
        <el-table :data="selectedSale.saleItems" stripe>
          <el-table-column prop="productName" label="Product" />
          <el-table-column prop="quantity" label="Quantity" width="80" />
          <el-table-column prop="unitPrice" label="Unit Price" width="100">
            <template #default="scope">
              ₹{{ scope.row.unitPrice.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="totalPrice" label="Total" width="100">
            <template #default="scope">
              ₹{{ scope.row.totalPrice.toLocaleString() }}
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Download, ShoppingCart, Money, Check, Clock, View, Calendar, Filter } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Permission states
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

// Reactive data
const sales = ref([])
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
const showDetailsDialog = ref(false)
const editingSale = ref(null)
const selectedSale = ref(null)
const events = ref([])

// Responsive properties
const windowWidth = ref(window.innerWidth)
const isMobileDialog = computed(() => windowWidth.value < 768)
const labelPosition = computed(() => (windowWidth.value < 640 ? 'top' : 'right'))
const formLabelWidth = computed(() => {
  if (windowWidth.value < 480) return '85px'
  if (windowWidth.value < 640) return '100px'
  return '120px'
})
const dialogWidth = computed(() => {
  const width = windowWidth.value
  if (width < 576) return '100%'
  if (width < 768) return '95%'
  if (width < 992) return '90%'
  // For desktop, calculate 75% of screen width
  return Math.min(width * 0.75, 1400) + 'px'
})
const detailsDialogWidth = computed(() => {
  if (windowWidth.value < 576) return '100%'
  if (windowWidth.value < 768) return '95%'
  if (windowWidth.value < 992) return '85%'
  return '700px'
})

// Form data
const saleForm = reactive({
  customerName: '',
  customerPhone: '',
  saleDate: dayjs().format('YYYY-MM-DD'),
  paymentMethod: 'Cash',
  discountAmount: 0,
  totalAmount: 0,
  finalAmount: 0,
  notes: '',
  eventId: null,
  categoryId: null,
  productId: null,
  salesBookingStatusId: 1,
  saleItems: [
    {
      productId: '',
      productName: '',
      quantity: 1,
      unitPrice: 0,
      totalPrice: 0
    }
  ]
})

// Form validation rules
const saleRules = {
  customerName: [{ required: true, message: 'Customer name is required', trigger: 'blur' }],
  customerPhone: [{ required: true, message: 'Customer phone is required', trigger: 'blur' }],
  saleDate: [{ required: true, message: 'Sale date is required', trigger: 'change' }],
  paymentMethod: [{ required: true, message: 'Payment method is required', trigger: 'change' }],
}

const saleFormRef = ref()

// API base URL
const API_BASE = '/api'
const productsBySelectedCategory = computed(() => {
  if (!saleForm.categoryId) return []
  // Sales page: exclude prebooking-only products
  return products.value
    .filter(p => (p.categoryId === saleForm.categoryId || p.categoryNavigation?.id === saleForm.categoryId))
    .filter(p => !p.isPreBookingAvailable)
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

const getBookingStatusText = (id) => {
  const map = { 1: 'NoBooking', 2: 'InProgress', 3: 'EmailFailed', 4: 'Awaiting', 5: 'Success', 6: 'Failed' }
  return map[id] || '—'
}

const getBookingStatusTag = (id) => {
  const map = { 5: 'success', 2: 'warning', 4: 'info', 3: 'danger', 6: 'danger', 1: '' }
  return map[id] || ''
}


// Summary Statistics
const summaryStats = computed(() => {
  const today = dayjs().format('YYYY-MM-DD')
  return {
    total: sales.value.length,
    revenue: sales.value
      .filter(s => s.status === 'Completed')
      .reduce((sum, s) => sum + (s.finalAmount || 0), 0),
    today: sales.value.filter(s => 
      dayjs(s.saleDate).format('YYYY-MM-DD') === today
    ).length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...sales.value]

  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(s =>
      s.customerName?.toLowerCase().includes(term) ||
      s.customerPhone?.includes(term) ||
      s.notes?.toLowerCase().includes(term)
    )
  }

  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(s => s.status === statusFilter.value)
  }

  // Apply date filter
  if (dateFilter.value) {
    const filterDate = dayjs(dateFilter.value).format('YYYY-MM-DD')
    result = result.filter(s => 
      dayjs(s.saleDate).format('YYYY-MM-DD') === filterDate
    )
  }

  // Apply sorting
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      
      switch(field) {
        case 'date':
          aVal = new Date(a.saleDate).getTime()
          bVal = new Date(b.saleDate).getTime()
          break
        case 'amount':
          aVal = a.finalAmount || 0
          bVal = b.finalAmount || 0
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
const paginatedSales = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

// Methods
const loadSales = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/sales`)
    // Sales page: show only non-prebooking or completed sales
    const all = response.data
    sales.value = all.filter(s => {
      const p = products.value.find(pp => pp.id === s.productId)
      return !p?.isPreBookingAvailable
    })
  } catch (error) {
    console.error('Error loading sales:', error)
    ElMessage.error('Failed to load sales')
  } finally {
    loading.value = false
  }
}

const loadProducts = async () => {
  try {
    const response = await axios.get(`${API_BASE}/products`)
    products.value = response.data
  } catch (error) {
    console.error('Error loading products:', error)
  }
}

const loadCategories = async () => {
  try {
    const response = await axios.get(`${API_BASE}/categories/active`)
    categories.value = response.data
  } catch (error) {
    console.error('Error loading categories:', error)
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

const handleSortChange = () => {
  // Sorting doesn't require resetting pagination
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'date-desc'
  } else {
    let field = prop
    if (prop === 'saleDate') field = 'date'
    else if (prop === 'totalAmount' || prop === 'finalAmount') field = 'amount'
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const handleDateFilter = () => {
  currentPage.value = 1
}

const addSaleItem = () => {
  saleForm.saleItems.push({
    productId: '',
    productName: '',
    quantity: 1,
    unitPrice: 0,
    totalPrice: 0
  })
}

const removeSaleItem = (index) => {
  if (saleForm.saleItems.length > 1) {
    saleForm.saleItems.splice(index, 1)
    calculateTotals()
  }
}

const updateItemDetails = (index) => {
  const product = products.value.find(p => p.id === saleForm.saleItems[index].productId)
  if (product) {
    saleForm.saleItems[index].productName = product.name
    saleForm.saleItems[index].unitPrice = product.price
    calculateItemTotal(index)
  }
}

const handleCategoryChange = () => {
  saleForm.productId = null
}

const calculateItemTotal = (index) => {
  const item = saleForm.saleItems[index]
  item.totalPrice = item.quantity * item.unitPrice
  calculateTotals()
}

const calculateTotals = () => {
  saleForm.totalAmount = saleForm.saleItems.reduce((sum, item) => sum + item.totalPrice, 0)
  saleForm.finalAmount = saleForm.totalAmount - saleForm.discountAmount
}

const editSale = (sale) => {
  editingSale.value = sale
  Object.assign(saleForm, sale)
  showCreateDialog.value = true
}

const saveSale = async () => {
  try {
    await saleFormRef.value.validate()
    saving.value = true
    
    if (editingSale.value) {
      // Update existing sale
      await axios.put(`${API_BASE}/sales/${editingSale.value.id}`, saleForm)
      ElMessage.success('Sale updated successfully')
    } else {
      // Create new sale
      await axios.post(`${API_BASE}/sales`, saleForm)
      ElMessage.success('Sale created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadSales()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save sale')
    }
  } finally {
    saving.value = false
  }
}

const updateSaleStatus = async (id, status) => {
  try {
    await axios.put(`${API_BASE}/sales/${id}/status`, null, {
      params: { status }
    })
    ElMessage.success('Sale status updated successfully')
    loadSales()
  } catch (error) {
    console.error('Error updating sale status:', error)
    ElMessage.error('Failed to update sale status')
  }
}

const deleteSale = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this sale?',
      'Warning',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/sales/${id}`)
    ElMessage.success('Sale deleted successfully')
    loadSales()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting sale:', error)
      ElMessage.error('Failed to delete sale')
    }
  }
}

const handleRowClick = (row) => {
  selectedSale.value = row
  showDetailsDialog.value = true
}

const viewSaleDetails = (sale) => {
  selectedSale.value = sale
  showDetailsDialog.value = true
}

const exportSales = () => {
  ElMessage.info('Export functionality will be implemented')
}

const resetForm = () => {
  editingSale.value = null
  Object.assign(saleForm, {
    customerName: '',
    customerPhone: '',
    saleDate: dayjs().format('YYYY-MM-DD'),
    paymentMethod: 'Cash',
    discountAmount: 0,
    totalAmount: 0,
    finalAmount: 0,
    notes: '',
    eventId: null,
    categoryId: null,
    productId: null,
    salesBookingStatusId: 1,
    saleItems: [
      {
        productId: '',
        productName: '',
        quantity: 1,
        unitPrice: 0,
        totalPrice: 0
      }
    ]
  })
  saleFormRef.value?.resetFields()
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY')
}

const getStatusTagType = (status) => {
  const statusMap = {
    'Completed': 'success',
    'Pending': 'warning',
    'Cancelled': 'danger'
  }
  return statusMap[status] || 'info'
}

const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

// Window resize handler
const handleResize = () => {
  windowWidth.value = window.innerWidth
}

// Lifecycle
onMounted(async () => {
  loadSales()
  loadProducts()
  loadCategories()
  loadEvents()
  window.addEventListener('resize', handleResize)
  
  // Check permissions
  if (window["templeAuth"]) {
    canCreate.value = await window["templeAuth"].hasCreatePermission('/sales')
    canUpdate.value = await window["templeAuth"].hasUpdatePermission('/sales')
    canDelete.value = await window["templeAuth"].hasDeletePermission('/sales')
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>

.sales-container {
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

.sales-card {
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

/* Allow wrapping/spacing for filters on narrow screens */
.search-filters .el-row {
  row-gap: 10px;
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
  background-color: #409eff;
}

.summary-icon.revenue {
  background-color: #67c23a;
}

.summary-icon.completed {
  background-color: #67c23a;
}

.summary-icon.pending {
  background-color: #e6a23c;
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

.sale-items {
  margin-bottom: 20px;
}

.sale-items .add-item-btn {
  width: 100%;
  margin-top: 10px;
}

.sale-item {
  margin-bottom: 15px;
  padding: 15px;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  background-color: #fafafa;
  overflow: hidden;
}

/* Ensure row stays inside the sale-item panel borders */
.sale-item-row {
  margin-left: 0 !important;
  margin-right: 0 !important;
}

.sale-item .action-col {
  display: flex;
  align-items: center;
  height: 100%;
}
.sale-item .action-col .el-form-item__content,
.sale-item .action-col .action-wrap {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 0 !important;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.sale-details {
  padding: 20px 0;
}

.dialog-footer {
  text-align: right;
}

/* Align InputNumber visuals across the form */
.sale-dialog .el-input-number {
  width: 100%;
}
.sale-dialog .el-input-number .el-input__wrapper,
.sale-dialog .el-input .el-input__wrapper {
  background-color: #fff;
}
.sale-dialog .el-input-number .el-input__inner,
.sale-dialog .el-input .el-input__inner {
  text-align: left;
}
.sale-dialog .el-input-number.is-disabled { opacity: 1; }
.sale-dialog .el-input-number.is-disabled .el-input__wrapper,
.sale-dialog .el-input.is-disabled .el-input__wrapper {
  background-color: #fff !important;
  color: var(--el-text-color-regular) !important;
  box-shadow: 0 0 0 1px var(--el-border-color) inset !important;
}

/* Force sale dialog width */
.sale-dialog {
  width: 75% !important;
  max-width: 1400px !important;
}

.sale-dialog .el-dialog__body {
  padding: 20px;
}

@media (max-width: 768px) {
  .sale-dialog {
    width: 100% !important;
    max-width: 100% !important;
  }
}

.table-container {
  width: 100%;
  overflow-x: auto;
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

/* Responsive */
@media (max-width: 768px) {
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .search-filters .el-row {
    row-gap: 10px;
  }

  .summary-value {
    font-size: 20px;
  }

  .summary-label {
    font-size: 12px;
  }

  .el-button .el-icon + span,
  .el-button .el-icon + .btn-text {
    margin-left: 6px;
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

  .sale-items {
    margin-bottom: 15px;
  }

  .sale-item {
    padding: 10px;
    margin-bottom: 10px;
  }

  .sale-item-row {
    row-gap: 10px;
  }

  /* Adjust form label width for mobile */
  .el-dialog .el-form .el-form-item__label {
    width: 100px !important;
  }

  /* Make descriptions responsive */
  .el-descriptions .el-descriptions__label {
    font-size: 12px;
  }
  
  .el-descriptions .el-descriptions__content {
    font-size: 12px;
  }
}

@media (max-width: 576px) {
  /* Extra small screen adjustments */
  .el-dialog .el-form .el-form-item__label {
    width: auto !important;
    min-width: 85px;
    font-size: 12px;
    text-align: left !important;
    padding-right: 8px;
    line-height: 1.2;
    white-space: normal;
  }

  .sale-item {
    padding: 12px;
  }

  /* Stack form items vertically on very small screens */
  .sale-item-row { row-gap: 10px; }
  .sale-item-row .el-form-item { margin-bottom: 10px; }

  /* Adjust button sizes in dialog */
  .el-dialog .el-button {
    padding: 8px 12px;
    font-size: 13px;
  }

  /* Responsive form layout */
  .el-dialog .el-form-item {
    margin-bottom: 15px;
  }

  /* Make date picker responsive */
  .el-date-editor {
    width: 100% !important;
  }

  /* Sale items mobile adjustments */
  .sale-item-row { padding: 0; }

  .sale-item .el-form-item__label {
    font-size: 11px;
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

@media (max-width: 480px) {
  .summary-content {
    gap: 10px;
  }

  .el-button span:not(.el-icon) {
    display: none;
  }
}
</style>

<style>
/* Global styles for sale dialog - not scoped */
.el-dialog.sale-dialog {
  width: 85% !important;
  max-width: 1400px !important;
}

@media (max-width: 768px) {
  .el-dialog.sale-dialog {
    width: 100% !important;
    max-width: 100% !important;
  }
}
</style>
