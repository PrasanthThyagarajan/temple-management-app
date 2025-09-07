<template>
  <div class="sales-container">
    <el-card class="sales-card">
      <template #header>
        <div class="card-header">
          <h2>Sales Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            New Sale
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner sales-banner"></div>

      <!-- Search and Filters -->
      <div class="search-filters">
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
            <el-date-picker
              v-model="dateFilter"
              type="date"
              placeholder="Filter by Date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              @change="handleDateFilter"
            />
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select
              v-model="statusFilter"
              placeholder="Filter by Status"
              clearable
              @change="handleStatusFilter"
            >
              <el-option label="Completed" value="Completed" />
              <el-option label="Pending" value="Pending" />
              <el-option label="Cancelled" value="Cancelled" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="24" :md="6" :lg="6" :xl="6">
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

      <!-- Summary Cards -->
      <div class="summary-cards">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><ShoppingCart /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ totalSales }}</div>
                  <div class="summary-label">Total Sales</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon revenue">
                  <el-icon><Money /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">₹{{ totalRevenue.toLocaleString() }}</div>
                  <div class="summary-label">Total Revenue</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon completed">
                  <el-icon><Check /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ completedSales }}</div>
                  <div class="summary-label">Completed</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon pending">
                  <el-icon><Clock /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ pendingSales }}</div>
                  <div class="summary-label">Pending</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Sales Table -->
      <div class="table-container">
        <el-table
          :data="sales"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
        >
        <el-table-column prop="saleDate" label="Sale Date" width="120">
          <template #default="scope">
            {{ formatDate(scope.row.saleDate) }}
          </template>
        </el-table-column>
        <el-table-column prop="customerName" label="Customer" min-width="150" />
        <el-table-column prop="customerPhone" label="Phone" width="120" />
        <el-table-column prop="totalAmount" label="Total Amount" width="120">
          <template #default="scope">
            ₹{{ scope.row.totalAmount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column prop="discountAmount" label="Discount" width="100">
          <template #default="scope">
            ₹{{ scope.row.discountAmount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column prop="finalAmount" label="Final Amount" width="120">
          <template #default="scope">
            ₹{{ scope.row.finalAmount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column prop="paymentMethod" label="Payment" width="100" />
        <el-table-column prop="status" label="Status" width="100">
          <template #default="scope">
            <el-tag :type="getStatusTagType(scope.row.status)">
              {{ scope.row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="notes" label="Notes" min-width="150" show-overflow-tooltip />
        <el-table-column label="Actions" width="200" fixed="right">
          <template #default="scope">
            <el-button size="small" @click.stop="viewSaleDetails(scope.row)">
              <el-icon><View /></el-icon>
              View
            </el-button>
            <el-button size="small" @click.stop="editSale(scope.row)">
              <el-icon><Edit /></el-icon>
              Edit
            </el-button>
            <el-button
              size="small"
              type="success"
              @click.stop="updateSaleStatus(scope.row.id, 'Completed')"
              v-if="scope.row.status === 'Pending'"
            >
              <el-icon><Check /></el-icon>
              Complete
            </el-button>
            <el-button
              size="small"
              type="danger"
              @click.stop="deleteSale(scope.row.id)"
            >
              <el-icon><Delete /></el-icon>
              Delete
            </el-button>
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
          :total="totalSales"
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
      width="800px"
    >
      <el-form
        ref="saleFormRef"
        :model="saleForm"
        :rules="saleRules"
        label-width="120px"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Customer Name" prop="customerName">
              <el-input v-model="saleForm.customerName" placeholder="Enter customer name" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Customer Phone" prop="customerPhone">
              <el-input v-model="saleForm.customerPhone" placeholder="Enter phone number" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
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
          <el-col :span="12">
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
          <el-col :span="12">
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
          <el-col :span="12">
            <el-form-item label="Status" prop="status">
              <el-select v-model="saleForm.status" placeholder="Select status" style="width: 100%">
                <el-option label="Completed" value="Completed" />
                <el-option label="Pending" value="Pending" />
                <el-option label="Cancelled" value="Cancelled" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        
        <!-- Sale Items Section -->
        <el-divider content-position="left">Sale Items</el-divider>
        <div class="sale-items">
          <div v-for="(item, index) in saleForm.saleItems" :key="index" class="sale-item">
            <el-row :gutter="20">
              <el-col :span="8">
                <el-form-item :label="`Product ${index + 1}`" :prop="`saleItems.${index}.productId`">
                  <el-select
                    v-model="item.productId"
                    placeholder="Select product"
                    style="width: 100%"
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
              <el-col :span="4">
                <el-form-item :label="`Quantity`" :prop="`saleItems.${index}.quantity`">
                  <el-input-number
                    v-model="item.quantity"
                    :min="1"
                    style="width: 100%"
                    @change="calculateItemTotal(index)"
                  />
                </el-form-item>
              </el-col>
              <el-col :span="4">
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
              <el-col :span="4">
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
              <el-col :span="4">
                <el-form-item label="Action">
                  <el-button
                    type="danger"
                    size="small"
                    @click="removeSaleItem(index)"
                    :disabled="saleForm.saleItems.length === 1"
                  >
                    <el-icon><Delete /></el-icon>
                  </el-button>
                </el-form-item>
              </el-col>
            </el-row>
          </div>
          <el-button type="dashed" @click="addSaleItem" style="width: 100%">
            <el-icon><Plus /></el-icon>
            Add Item
          </el-button>
        </div>

        <el-row :gutter="20">
          <el-col :span="12">
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
          <el-col :span="12">
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
        
        <el-form-item label="Notes" prop="notes">
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
          <el-button type="primary" @click="saveSale" :loading="saving">
            {{ editingSale ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Sale Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Sale Details"
      width="700px"
    >
      <div v-if="selectedSale" class="sale-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Sale Date">
            {{ formatDate(selectedSale.saleDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="Status">
            <el-tag :type="getStatusTagType(selectedSale.status)">
              {{ selectedSale.status }}
            </el-tag>
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
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Download, ShoppingCart, Money, Check, Clock, View } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const sales = ref([])
const products = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const dateFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const totalSales = ref(0)
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const editingSale = ref(null)
const selectedSale = ref(null)

// Form data
const saleForm = reactive({
  customerName: '',
  customerPhone: '',
  saleDate: dayjs().format('YYYY-MM-DD'),
  paymentMethod: 'Cash',
  discountAmount: 0,
  status: 'Completed',
  totalAmount: 0,
  finalAmount: 0,
  notes: '',
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
  status: [{ required: true, message: 'Status is required', trigger: 'change' }]
}

const saleFormRef = ref()

// API base URL
const API_BASE = 'http://localhost:5051/api'

// Computed properties
const totalRevenue = computed(() => {
  return sales.value
    .filter(sale => sale.status === 'Completed')
    .reduce((sum, sale) => sum + sale.finalAmount, 0)
})

const completedSales = computed(() => {
  return sales.value.filter(sale => sale.status === 'Completed').length
})

const pendingSales = computed(() => {
  return sales.value.filter(sale => sale.status === 'Pending').length
})

// Methods
const loadSales = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/sales`)
    sales.value = response.data
    totalSales.value = response.data.length
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

const handleSearch = () => {
  if (searchTerm.value.trim()) {
    searchSales(searchTerm.value)
  } else {
    loadSales()
  }
}

const searchSales = async (term) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/sales/search/${term}`)
    sales.value = response.data
    totalSales.value = response.data.length
  } catch (error) {
    console.error('Error searching sales:', error)
    ElMessage.error('Failed to search sales')
  } finally {
    loading.value = false
  }
}

const handleDateFilter = () => {
  if (dateFilter.value) {
    const filterDate = dayjs(dateFilter.value)
    sales.value = sales.value.filter(sale => {
      const saleDate = dayjs(sale.saleDate)
      return saleDate.isSame(filterDate, 'day')
    })
    totalSales.value = sales.value.length
  } else {
    loadSales()
  }
}

const handleStatusFilter = () => {
  if (statusFilter.value) {
    sales.value = sales.value.filter(sale => sale.status === statusFilter.value)
    totalSales.value = sales.value.length
  } else {
    loadSales()
  }
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
    status: 'Completed',
    totalAmount: 0,
    finalAmount: 0,
    notes: '',
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
  loadSales()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadSales()
}

// Lifecycle
onMounted(() => {
  loadSales()
  loadProducts()
})
</script>

<style scoped>
.sales-container {
  padding: 20px;
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

.sale-item {
  margin-bottom: 15px;
  padding: 15px;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  background-color: #fafafa;
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
