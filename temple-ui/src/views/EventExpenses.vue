<template>
  <div class="expenses-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Expenses" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Money /></el-icon>
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
        <el-card class="summary-card">
          <el-statistic 
            title="Total Amount" 
            :value="summaryStats.amount" 
            prefix="₹"
            :precision="2"
          >
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
    </el-row>

    <el-card class="expenses-card">
      <template #header>
        <div class="card-header">
          <h2>Event Expenses</h2>
          <div class="header-actions">
            <el-button type="info" @click="loadEventExpenses" title="Refresh data">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
            <el-button type="primary" @click="showCreateDialog = true">
              <el-icon><Plus /></el-icon>
              Add {{ activeTab === 'service' ? 'Service' : 'Item' }} Expense
            </el-button>
          </div>
        </div>
      </template>

      <!-- Enhanced Filters -->
      <div class="filters-section">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search expenses..."
              :prefix-icon="Search"
              @input="handleSearch"
              clearable
            />
          </el-col>
          <el-col :xs="12" :sm="6" :md="4">
            <el-select
              v-model="statusFilter"
              placeholder="Filter by Status"
              @change="handleFilterChange"
              clearable
              style="width: 100%"
            >
              <el-option label="Any Status" value="" />
              <el-option label="Pending" value="pending" />
              <el-option label="Approved" value="approved" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4">
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
        </el-row>
      </div>

      <!-- Tabs for Service and Item Event Expenses -->
      <el-tabs v-model="activeTab" @tab-click="handleTabClick">
        <el-tab-pane label="Services" name="service">
          <!-- Service Expenses Content -->
          <div class="service-expenses">
            <!-- Debug info -->
            <div v-if="serviceExpenses.length === 0" style="padding: 20px; text-align: center; color: #666;">
              <p>No service expenses found.</p>
            </div>
            
            <!-- Pending Approval Service Expenses -->
            <div v-if="filteredPendingServiceExpenses.length > 0" style="margin-bottom: 30px;">
              <h3 style="margin-bottom: 15px; color: #E6A23C;">Pending Approval</h3>
              <el-table 
                :data="paginatedPendingServiceExpenses" 
                stripe 
                style="width: 100%"
                @sort-change="handleTableSortChange"
              >
                <el-table-column 
                  label="Service Name" 
                  min-width="200"
                  sortable="custom"
                  prop="serviceName"
                >
                  <template #default="scope">
                    {{ getServiceName(scope.row.expenseServiceId) }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="eventId" 
                  label="Event" 
                  width="120" 
                  sortable="custom"
                />
                <el-table-column 
                  prop="price" 
                  label="Price" 
                  width="120"
                  sortable="custom"
                >
                  <template #default="scope">
                    ₹{{ scope.row.price?.toFixed(2) || '0.00' }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="createdAt" 
                  label="Created" 
                  width="180"
                  sortable="custom"
                />
                <el-table-column label="Requested By" width="200">
                  <template #default="scope">
                    <div v-if="scope.row.requestedByUserName">
                      <strong>{{ scope.row.requestedByUserName }}</strong>
                      <br>
                      <small v-if="scope.row.requestedByUserRole" class="text-muted">({{ scope.row.requestedByUserRole }})</small>
                    </div>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Actions" width="200" fixed="right">
                  <template #default="scope">
                    <el-button size="small" type="success" @click.stop="approveServiceExpense(scope.row)">
                      <el-icon><Check /></el-icon>
                      Approve
                    </el-button>
                    <el-button size="small" @click.stop="editServiceExpense(scope.row)">
                      <el-icon><Edit /></el-icon>
                      Edit
                    </el-button>
                    <el-button size="small" type="danger" @click.stop="deleteServiceExpense(scope.row.id)">
                      <el-icon><Delete /></el-icon>
                      Delete
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
              
              <!-- Pagination for Pending Service Expenses -->
              <div class="pagination-container">
                <el-pagination
                  :current-page="currentPagePendingService"
                  :page-size="pageSizePendingService"
                  :page-sizes="[5, 10, 20]"
                  :total="filteredPendingServiceExpenses.length"
                  :background="true"
                  layout="total, sizes, prev, pager, next"
                  @size-change="handleSizeChange"
                  @current-change="handleCurrentChange"
                />
              </div>
            </div>

            <!-- Standard Service Expenses -->
            <div>
              <h3 style="margin-bottom: 15px; color: #67C23A;">Standard Expenses</h3>
              <el-table 
                :data="paginatedStandardServiceExpenses" 
                stripe 
                style="width: 100%"
                @sort-change="handleTableSortChange"
              >
                <el-table-column 
                  label="Service Name" 
                  min-width="200"
                  sortable="custom"
                  prop="serviceName"
                >
                  <template #default="scope">
                    {{ getServiceName(scope.row.expenseServiceId) }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="eventId" 
                  label="Event" 
                  width="120"
                  sortable="custom"
                />
                <el-table-column 
                  prop="price" 
                  label="Price" 
                  width="120"
                  sortable="custom"
                >
                  <template #default="scope">
                    ₹{{ scope.row.price?.toFixed(2) || '0.00' }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="createdAt" 
                  label="Created" 
                  width="180"
                  sortable="custom"
                />
                <el-table-column label="Requested By" width="200">
                  <template #default="scope">
                    <div v-if="scope.row.requestedByUserName">
                      <strong>{{ scope.row.requestedByUserName }}</strong>
                      <br>
                      <small v-if="scope.row.requestedByUserRole" class="text-muted">({{ scope.row.requestedByUserRole }})</small>
                    </div>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Approved By" width="200">
                  <template #default="scope">
                    <div v-if="scope.row.approvedByUserName">
                      <strong>{{ scope.row.approvedByUserName }}</strong>
                      <br>
                      <small v-if="scope.row.approvedByUserRole" class="text-muted">({{ scope.row.approvedByUserRole }})</small>
                    </div>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Approved On" width="180">
                  <template #default="scope">
                    <span v-if="scope.row.approvedOn">{{ formatDate(scope.row.approvedOn) }}</span>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Actions" width="150" fixed="right">
                  <template #default="scope">
                    <el-button size="small" @click.stop="editServiceExpense(scope.row)">
                      <el-icon><Edit /></el-icon>
                      Edit
                    </el-button>
                    <el-button size="small" type="danger" @click.stop="deleteServiceExpense(scope.row.id)">
                      <el-icon><Delete /></el-icon>
                      Delete
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
              
              <!-- Pagination for Standard Service Expenses -->
              <div class="pagination-container">
                <el-pagination
                  :current-page="currentPageStandardService"
                  :page-size="pageSizeStandardService"
                  :page-sizes="[5, 10, 20]"
                  :total="filteredStandardServiceExpenses.length"
                  :background="true"
                  layout="total, sizes, prev, pager, next"
                  @size-change="handleSizeChange"
                  @current-change="handleCurrentChange"
                />
              </div>
            </div>
          </div>
        </el-tab-pane>
        <el-tab-pane label="Items" name="item">
          <!-- Item Expenses Content -->
          <div class="item-expenses">
            <!-- Debug info -->
            <div v-if="itemExpenses.length === 0" style="padding: 20px; text-align: center; color: #666;">
              <p>No item expenses found.</p>
            </div>
            
            <!-- Pending Approval Item Expenses -->
            <div v-if="filteredPendingItemExpenses.length > 0" style="margin-bottom: 30px;">
              <h3 style="margin-bottom: 15px; color: #E6A23C;">Pending Approval</h3>
              <el-table 
                :data="paginatedPendingItemExpenses" 
                stripe 
                style="width: 100%"
                @sort-change="handleTableSortChange"
              >
                <el-table-column 
                  label="Item Name" 
                  min-width="200"
                  sortable="custom"
                  prop="itemName"
                >
                  <template #default="scope">
                    {{ getItemName(scope.row.eventExpenseId || scope.row.EventExpenseId) }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="eventId" 
                  label="Event" 
                  width="120"
                  sortable="custom"
                />
                <el-table-column 
                  prop="price" 
                  label="Price" 
                  width="120"
                  sortable="custom"
                >
                  <template #default="scope">
                    ₹{{ scope.row.price?.toFixed(2) || '0.00' }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="createdAt" 
                  label="Created" 
                  width="180"
                  sortable="custom"
                />
                <el-table-column label="Requested By" width="200">
                  <template #default="scope">
                    <div v-if="scope.row.requestedByUserName">
                      <strong>{{ scope.row.requestedByUserName }}</strong>
                      <br>
                      <small v-if="scope.row.requestedByUserRole" class="text-muted">({{ scope.row.requestedByUserRole }})</small>
                    </div>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Actions" width="200" fixed="right">
                  <template #default="scope">
                    <el-button size="small" type="success" @click.stop="approveItemExpense(scope.row)">
                      <el-icon><Check /></el-icon>
                      Approve
                    </el-button>
                    <el-button size="small" @click.stop="editItemExpense(scope.row)">
                      <el-icon><Edit /></el-icon>
                      Edit
                    </el-button>
                    <el-button size="small" type="danger" @click.stop="deleteItemExpense(scope.row.id)">
                      <el-icon><Delete /></el-icon>
                      Delete
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
              
              <!-- Pagination for Pending Item Expenses -->
              <div class="pagination-container">
                <el-pagination
                  :current-page="currentPagePendingItem"
                  :page-size="pageSizePendingItem"
                  :page-sizes="[5, 10, 20]"
                  :total="filteredPendingItemExpenses.length"
                  :background="true"
                  layout="total, sizes, prev, pager, next"
                  @size-change="handleSizeChange"
                  @current-change="handleCurrentChange"
                />
              </div>
            </div>

            <!-- Standard Item Expenses -->
            <div>
              <h3 style="margin-bottom: 15px; color: #67C23A;">Standard Expenses</h3>
              <el-table 
                :data="paginatedStandardItemExpenses" 
                stripe 
                style="width: 100%"
                @sort-change="handleTableSortChange"
              >
                <el-table-column 
                  label="Item Name" 
                  min-width="200"
                  sortable="custom"
                  prop="itemName"
                >
                  <template #default="scope">
                    {{ getItemName(scope.row.eventExpenseId || scope.row.EventExpenseId) }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="eventId" 
                  label="Event" 
                  width="120"
                  sortable="custom"
                />
                <el-table-column 
                  prop="price" 
                  label="Price" 
                  width="120"
                  sortable="custom"
                >
                  <template #default="scope">
                    ₹{{ scope.row.price?.toFixed(2) || '0.00' }}
                  </template>
                </el-table-column>
                <el-table-column 
                  prop="createdAt" 
                  label="Created" 
                  width="180"
                  sortable="custom"
                />
                <el-table-column label="Requested By" width="200">
                  <template #default="scope">
                    <div v-if="scope.row.requestedByUserName">
                      <strong>{{ scope.row.requestedByUserName }}</strong>
                      <br>
                      <small v-if="scope.row.requestedByUserRole" class="text-muted">({{ scope.row.requestedByUserRole }})</small>
                    </div>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Approved By" width="200">
                  <template #default="scope">
                    <div v-if="scope.row.approvedByUserName">
                      <strong>{{ scope.row.approvedByUserName }}</strong>
                      <br>
                      <small v-if="scope.row.approvedByUserRole" class="text-muted">({{ scope.row.approvedByUserRole }})</small>
                    </div>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Approved On" width="180">
                  <template #default="scope">
                    <span v-if="scope.row.approvedOn">{{ formatDate(scope.row.approvedOn) }}</span>
                    <span v-else class="text-muted">—</span>
                  </template>
                </el-table-column>
                <el-table-column label="Actions" width="150" fixed="right">
                  <template #default="scope">
                    <el-button size="small" @click.stop="editItemExpense(scope.row)">
                      <el-icon><Edit /></el-icon>
                      Edit
                    </el-button>
                    <el-button size="small" type="danger" @click.stop="deleteItemExpense(scope.row.id)">
                      <el-icon><Delete /></el-icon>
                      Delete
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
              
              <!-- Pagination for Standard Item Expenses -->
              <div class="pagination-container">
                <el-pagination
                  :current-page="currentPageStandardItem"
                  :page-size="pageSizeStandardItem"
                  :page-sizes="[5, 10, 20]"
                  :total="filteredStandardItemExpenses.length"
                  :background="true"
                  layout="total, sizes, prev, pager, next"
                  @size-change="handleSizeChange"
                  @current-change="handleCurrentChange"
                />
              </div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>

      <!-- Create Event Expense Dialog -->
      <el-dialog
        v-model="showCreateDialog"
        :title="`Add ${activeTab === 'service' ? 'Service' : 'Item'} Expense`"
        :width="dialogWidth"
        :fullscreen="isMobileDialog"
      >
        <el-form ref="expenseFormRef" :model="expenseForm" :rules="expenseRules" :label-width="isMobileDialog ? '100px' : '140px'">
          <el-form-item 
            :label="`${activeTab === 'service' ? 'Service' : 'Item'}`" 
            :prop="activeTab === 'item' ? 'EventExpenseId' : 'ExpenseServiceId'"
          >
            <el-select
              v-if="activeTab === 'item'"
              v-model="expenseForm.EventExpenseId"
              placeholder="Select expense item"
              style="width: 100%"
            >
              <el-option v-for="item in EventExpenses" :key="item.id" :label="item.name" :value="item.id" />
            </el-select>
            <el-select
              v-else
              v-model="expenseForm.ExpenseServiceId"
              placeholder="Select expense service"
              style="width: 100%"
            >
              <el-option v-for="svc in expenseServices" :key="svc.id" :label="svc.name" :value="svc.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="Event" prop="eventId">
            <el-select v-model="expenseForm.eventId" placeholder="Select event" style="width: 100%">
              <el-option v-for="ev in events" :key="ev.id" :label="ev.name" :value="ev.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="Price" prop="price">
            <el-input-number
              v-model="expenseForm.price"
              :min="0"
              :precision="2"
              placeholder="Enter price"
              style="width: 100%"
            />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="onCancelCreate">Cancel</el-button>
          <el-button type="primary" :loading="saving" @click="saveExpense">Create</el-button>
        </template>
      </el-dialog>


      <!-- Edit Event Expense Dialog -->
      <el-dialog
        v-model="showEditDialog"
        :title="`Edit ${activeTab === 'service' ? 'Service' : 'Item'} Expense`"
        :width="dialogWidth"
        :fullscreen="isMobileDialog"
      >
        <el-form ref="editExpenseFormRef" :model="editExpenseForm" :rules="expenseRules" :label-width="isMobileDialog ? '100px' : '140px'">
          <el-form-item 
            :label="`${activeTab === 'service' ? 'Service' : 'Item'}`" 
            :prop="activeTab === 'item' ? 'EventExpenseId' : 'ExpenseServiceId'"
          >
            <el-select
              v-if="activeTab === 'item'"
              v-model="editExpenseForm.EventExpenseId"
              placeholder="Select expense item"
              style="width: 100%"
            >
              <el-option v-for="item in EventExpenses" :key="item.id" :label="item.name" :value="item.id" />
            </el-select>
            <el-select
              v-else
              v-model="editExpenseForm.ExpenseServiceId"
              placeholder="Select expense service"
              style="width: 100%"
            >
              <el-option v-for="svc in expenseServices" :key="svc.id" :label="svc.name" :value="svc.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="Event" prop="eventId">
            <el-select v-model="editExpenseForm.eventId" placeholder="Select event" style="width: 100%">
              <el-option v-for="ev in events" :key="ev.id" :label="ev.name" :value="ev.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="Price" prop="price">
            <el-input-number
              v-model="editExpenseForm.price"
              :min="0"
              :precision="2"
              placeholder="Enter price"
              style="width: 100%"
            />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="onCancelEdit">Cancel</el-button>
          <el-button type="primary" :loading="updating" @click="updateExpense">Update</el-button>
        </template>
      </el-dialog>

      <!-- Manage Event Expense Popup -->
      <el-dialog v-model="showExpenseDialog" title="Event Expense Details" :width="dialogWidth" :fullscreen="isMobileDialog">
        <el-table :data="selectedEventExpenses" stripe style="width: 100%">
          <el-table-column prop="name" label="Name" min-width="200" />
          <el-table-column prop="eventId" label="Event" width="120" />
          <el-table-column prop="price" label="Price" width="120">
            <template #default="scope">
              ₹{{ scope.row.price?.toFixed(2) || '0.00' }}
            </template>
          </el-table-column>
          <el-table-column prop="createdAt" label="Created" width="180" />
        </el-table>
        <template #footer>
          <el-button @click="showExpenseDialog = false">Close</el-button>
        </template>
      </el-dialog>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Plus, Edit, Delete, Refresh, Check, Money, Clock, CreditCard, Calendar, Search } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const activeTab = ref('service')
const showCreateDialog = ref(false)
const showEditDialog = ref(false)
const serviceExpenses = ref([])
const itemExpenses = ref([])
const selectedEventExpenses = ref([])
const showExpenseDialog = ref(false)
const saving = ref(false)
const updating = ref(false)
const EventExpenses = ref([])
const expenseServices = ref([])
const events = ref([])
const expenseFormRef = ref()
const editExpenseFormRef = ref()

// Responsive properties
const windowWidth = ref(window.innerWidth)
const isMobileDialog = computed(() => windowWidth.value < 768)
const dialogWidth = computed(() => {
  if (windowWidth.value < 576) return '100%'
  if (windowWidth.value < 768) return '95%'
  if (windowWidth.value < 992) return '90%'
  return '600px'
})

// Enhanced filters and pagination
const searchTerm = ref('')
const statusFilter = ref('')
const sortBy = ref('date-desc')

// Pagination for each table
const currentPagePendingService = ref(1)
const pageSizePendingService = ref(10)
const currentPageStandardService = ref(1)
const pageSizeStandardService = ref(10)
const currentPagePendingItem = ref(1)
const pageSizePendingItem = ref(10)
const currentPageStandardItem = ref(1)
const pageSizeStandardItem = ref(10)
const expenseForm = reactive({
  EventExpenseId: null,
  ExpenseServiceId: null,
  eventId: null,
  price: 0
})
const editExpenseForm = reactive({
  id: null,
  EventExpenseId: null,
  ExpenseServiceId: null,
  eventId: null,
  price: 0
})
const expenseRules = computed(() => ({
  // Item/service required will be validated based on selected tab
  EventExpenseId: activeTab.value === 'item' ? [{ required: true, message: 'Item is required', trigger: 'change' }] : [],
  ExpenseServiceId: activeTab.value === 'service' ? [{ required: true, message: 'Service is required', trigger: 'change' }] : [],
  eventId: [{ required: true, message: 'Event is required', trigger: 'change' }],
  price: [
    { required: true, message: 'Price is required', trigger: 'blur' },
    { type: 'number', min: 0, message: 'Price must be greater than or equal to 0', trigger: 'blur' }
  ]
}))


const API_BASE = '/api'
// const canApprove = ref(false) // Commented out - unused variable

// Computed properties for separating item expenses into two lists
const pendingApprovalItemExpenses = computed(() => {
  const pending = itemExpenses.value.filter(expense => {
    const needsApproval = expense.isApprovalNeed === true
    const isNotApproved = !expense.isApproved
    console.log('🔍 Item expense:', expense.id, 'isApprovalNeed:', expense.isApprovalNeed, 'isApproved:', expense.isApproved, 'shouldShowInPending:', needsApproval && isNotApproved)
    return needsApproval && isNotApproved
  })
  console.log('🔍 Pending approval item expenses:', pending.length)
  return pending
})

const standardItemExpenses = computed(() => {
  return itemExpenses.value.filter(expense => {
    const needsApproval = expense.isApprovalNeed === true
    const isApproved = expense.isApproved
    // Show in standard if: doesn't need approval OR is already approved
    return !needsApproval || isApproved
  })
})

// Computed properties for separating service expenses into two lists
const pendingApprovalServiceExpenses = computed(() => {
  const pending = serviceExpenses.value.filter(expense => {
    const needsApproval = expense.isApprovalNeed === true
    const isNotApproved = !expense.isApproved
    console.log('🔍 Service expense:', expense.id, 'isApprovalNeed:', expense.isApprovalNeed, 'isApproved:', expense.isApproved, 'shouldShowInPending:', needsApproval && isNotApproved)
    return needsApproval && isNotApproved
  })
  console.log('🔍 Pending approval service expenses:', pending.length)
  return pending
})

const standardServiceExpenses = computed(() => {
  return serviceExpenses.value.filter(expense => {
    const needsApproval = expense.isApprovalNeed === true
    const isApproved = expense.isApproved
    // Show in standard if: doesn't need approval OR is already approved
    return !needsApproval || isApproved
  })
})

// Enhanced Summary Statistics
const summaryStats = computed(() => {
  const allExpenses = [...serviceExpenses.value, ...itemExpenses.value]
  const thisMonth = dayjs().format('YYYY-MM')
  
  return {
    total: allExpenses.length,
    pending: allExpenses.filter(e => e.isApprovalNeed && !e.isApproved).length,
    amount: allExpenses.reduce((sum, e) => sum + (e.price || 0), 0),
    thisMonth: allExpenses.filter(e => 
      dayjs(e.createdAt).format('YYYY-MM') === thisMonth
    ).length
  }
})

// Enhanced filtering and pagination for each table
const filterAndSortExpenses = (expenses) => {
  let result = [...expenses]

  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(e =>
      getServiceName(e.expenseServiceId)?.toLowerCase().includes(term) ||
      getItemName(e.eventExpenseId || e.EventExpenseId)?.toLowerCase().includes(term) ||
      e.requestedByUserName?.toLowerCase().includes(term) ||
      e.approvedByUserName?.toLowerCase().includes(term)
    )
  }

  // Apply status filter
  if (statusFilter.value) {
    if (statusFilter.value === 'pending') {
      result = result.filter(e => e.isApprovalNeed && !e.isApproved)
    } else if (statusFilter.value === 'approved') {
      result = result.filter(e => !e.isApprovalNeed || e.isApproved)
    }
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
          aVal = a.price || 0
          bVal = b.price || 0
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
}

// Filtered data for each table
const filteredPendingServiceExpenses = computed(() => 
  filterAndSortExpenses(pendingApprovalServiceExpenses.value)
)
const filteredStandardServiceExpenses = computed(() => 
  filterAndSortExpenses(standardServiceExpenses.value)
)
const filteredPendingItemExpenses = computed(() => 
  filterAndSortExpenses(pendingApprovalItemExpenses.value)
)
const filteredStandardItemExpenses = computed(() => 
  filterAndSortExpenses(standardItemExpenses.value)
)

// Paginated data for each table
const paginatedPendingServiceExpenses = computed(() => {
  const start = (currentPagePendingService.value - 1) * pageSizePendingService.value
  return filteredPendingServiceExpenses.value.slice(start, start + pageSizePendingService.value)
})
const paginatedStandardServiceExpenses = computed(() => {
  const start = (currentPageStandardService.value - 1) * pageSizeStandardService.value
  return filteredStandardServiceExpenses.value.slice(start, start + pageSizeStandardService.value)
})
const paginatedPendingItemExpenses = computed(() => {
  const start = (currentPagePendingItem.value - 1) * pageSizePendingItem.value
  return filteredPendingItemExpenses.value.slice(start, start + pageSizePendingItem.value)
})
const paginatedStandardItemExpenses = computed(() => {
  const start = (currentPageStandardItem.value - 1) * pageSizeStandardItem.value
  return filteredStandardItemExpenses.value.slice(start, start + pageSizeStandardItem.value)
})

// Methods
const handleTabClick = (tab) => {
  console.log('Tab clicked:', tab.name)
}

const handleSearch = () => {
  // Reset all pagination when searching
  currentPagePendingService.value = 1
  currentPageStandardService.value = 1
  currentPagePendingItem.value = 1
  currentPageStandardItem.value = 1
}

const handleFilterChange = () => {
  // Reset all pagination when filtering
  currentPagePendingService.value = 1
  currentPageStandardService.value = 1
  currentPagePendingItem.value = 1
  currentPageStandardItem.value = 1
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
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const handleSizeChange = (val) => {
  // Note: This is a simplified handler - you may want to implement
  // specific logic for each table's page size change
  console.log('Page size changed:', val)
}

const handleCurrentChange = (val) => {
  // Note: This is a simplified handler - you may want to implement
  // specific logic for each table's current page change
  console.log('Current page changed:', val)
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

const getItemName = (eventExpenseId) => {
  const item = EventExpenses.value.find(i => i.id === eventExpenseId)
  return item ? item.name : '—'
}

const getServiceName = (ExpenseServiceId) => {
  const svc = expenseServices.value.find(s => s.id === ExpenseServiceId)
  return svc ? svc.name : '—'
}

const editServiceExpense = (expense) => {
  editExpenseForm.id = expense.id
  editExpenseForm.EventExpenseId = expense.EventExpenseId
  editExpenseForm.ExpenseServiceId = expense.expenseServiceId
  editExpenseForm.eventId = expense.eventId
  editExpenseForm.price = expense.price
  activeTab.value = 'service'
  showEditDialog.value = true
}

const deleteServiceExpense = async (id) => {
  try {
    await axios.delete(`${API_BASE}/event-expenses/${id}`)
    ElMessage.success('Expense deleted successfully')
    await loadEventExpenses()
  } catch (error) {
    ElMessage.error('Failed to delete expense')
  }
}

const editItemExpense = (expense) => {
  editExpenseForm.id = expense.id
  editExpenseForm.EventExpenseId = expense.eventExpenseId || expense.EventExpenseId
  editExpenseForm.ExpenseServiceId = expense.expenseServiceId
  editExpenseForm.eventId = expense.eventId
  editExpenseForm.price = expense.price
  activeTab.value = 'item'
  showEditDialog.value = true
}

const deleteItemExpense = async (id) => {
  try {
    await axios.delete(`${API_BASE}/event-expenses/${id}`)
    ElMessage.success('Expense deleted successfully')
    await loadEventExpenses()
  } catch (error) {
    ElMessage.error('Failed to delete expense')
  }
}

const approveServiceExpense = async (expense) => {
  try {
    await axios.put(`${API_BASE}/event-expenses/${expense.id}/approve`)
    ElMessage.success('Service expense approved successfully')
    await loadEventExpenses()
  } catch (error) {
    console.error('Failed to approve service expense:', error)
    ElMessage.error('Failed to approve service expense')
  }
}

const approveItemExpense = async (expense) => {
  try {
    await axios.put(`${API_BASE}/event-expenses/${expense.id}/approve`)
    ElMessage.success('Item expense approved successfully')
    await loadEventExpenses()
  } catch (error) {
    console.error('Failed to approve item expense:', error)
    ElMessage.error('Failed to approve item expense')
  }
}

// Create expense handlers
const onCancelCreate = () => {
  showCreateDialog.value = false
  resetForm()
}


const onCancelEdit = () => {
  showEditDialog.value = false
  resetEditForm()
}

const resetForm = () => {
  expenseForm.EventExpenseId = null
  expenseForm.ExpenseServiceId = null
  expenseForm.eventId = null
  expenseForm.price = 0
  expenseFormRef.value?.resetFields?.()
}


const resetEditForm = () => {
  editExpenseForm.id = null
  editExpenseForm.EventExpenseId = null
  editExpenseForm.ExpenseServiceId = null
  editExpenseForm.eventId = null
  editExpenseForm.price = 0
  editExpenseFormRef.value?.resetFields?.()
}

const loadExpenseItems = async () => {
  try {
    const res = await axios.get(`${API_BASE}/expense-items`)
    EventExpenses.value = res.data
    console.log('🔍 DEBUG: Loaded expense items:', EventExpenses.value)
  } catch (e) {
    console.error('Failed to load expense items', e)
  }
}

const loadExpenseServices = async () => {
  try {
    const res = await axios.get(`${API_BASE}/expense-services`)
    expenseServices.value = res.data
  } catch (e) {
    console.error('Failed to load expense services', e)
  }
}

const loadEvents = async () => {
  try {
    const res = await axios.get(`${API_BASE}/events`)
    events.value = res.data
  } catch (e) {
    console.error('Failed to load events', e)
  }
}

const loadEventExpenses = async () => {
  try {
    console.log('🔍 Loading event expenses from API...')
    const res = await axios.get(`${API_BASE}/event-expenses`)
    // debugger;
    const all = (res.data || []).map(x => ({
      ...x, // Keep all original fields
      // normalize casings and types to be robust to API variations
      expenseServiceId: x.expenseServiceId ?? x.ExpenseServiceId ?? null,
      eventExpenseId: x.eventExpenseId ?? x.EventExpenseId ?? null,
      isApprovalNeed: x.isApprovalNeed === true || x.isApprovalNeed === 1 || x.isApprovalNeed === 'true' || x.isApprovalNeed === 'True',
      isApproved: x.isApproved === true || x.isApproved === 1 || x.isApproved === 'true' || x.isApproved === 'True'
    }))
    // debugger;
    
    // Debug logging to see what we're getting from the API
    console.log('🔍 DEBUG: All expenses from API:', all)
    console.log('🔍 DEBUG: Sample expense structure:', all[0])
    console.log('🔍 DEBUG: Looking for expense ID 6...')
    const expense6 = all.find(x => x.id === 6)
    if (expense6) {
      console.log('🔍 DEBUG: Found expense 6:', expense6)
      console.log('🔍 DEBUG: Expense 6 isApprovalNeed:', expense6.isApprovalNeed, typeof expense6.isApprovalNeed)
      console.log('🔍 DEBUG: Expense 6 isApproved:', expense6.isApproved, typeof expense6.isApproved)
    } else {
      console.log('🔍 DEBUG: Expense 6 NOT found in API response!')
    }
    
    // Split into service and item arrays for tabs with more robust filtering
    serviceExpenses.value = all.filter(x => {
      // API returns camelCase properties
      const hasServiceId = x.expenseServiceId !== null && Number(x.expenseServiceId) > 0
      console.log(`🔍 Expense ${x.id}: expenseServiceId=${x.expenseServiceId}, hasServiceId=${hasServiceId}`)
      return hasServiceId
    })
    
    itemExpenses.value = all.filter(x => {
      const hasItemId = x.eventExpenseId !== null && Number(x.eventExpenseId) > 0
      console.log(`🔍 Expense ${x.id}: eventExpenseId=${x.eventExpenseId}, hasItemId=${hasItemId}, price=${x.price}`)
      return hasItemId
    })
    
    console.log('🔍 DEBUG: Service expenses:', serviceExpenses.value)
    console.log('🔍 DEBUG: Item expenses:', itemExpenses.value)
    console.log('🔍 DEBUG: Service expenses count:', serviceExpenses.value.length)
    console.log('🔍 DEBUG: Item expenses count:', itemExpenses.value.length)
    
    // Force reactivity update
    console.log('🔍 DEBUG: Forcing reactivity update...')
    setTimeout(() => {
      console.log('🔍 DEBUG: After timeout - checking computed properties...')
      console.log('🔍 DEBUG: pendingApprovalServiceExpenses.length:', pendingApprovalServiceExpenses.value?.length)
      console.log('🔍 DEBUG: pendingApprovalItemExpenses.length:', pendingApprovalItemExpenses.value?.length)
    }, 100)
  } catch (e) {
    console.error('Failed to load event expenses', e)
  }
}

const saveExpense = async () => {
  try {
    await expenseFormRef.value.validate()
    saving.value = true
    
    // Clear the opposite field based on selected tab
    if (activeTab.value === 'item') {
      expenseForm.ExpenseServiceId = null
    } else {
      expenseForm.EventExpenseId = null
    }

    const payload = {
      EventExpenseId: activeTab.value === 'item' ? expenseForm.EventExpenseId : null,
      ExpenseServiceId: activeTab.value === 'service' ? expenseForm.ExpenseServiceId : null,
      eventId: expenseForm.eventId,
      price: expenseForm.price,
    }
    
    await axios.post(`${API_BASE}/event-expenses`, payload)
    ElMessage.success(`${activeTab.value === 'service' ? 'Service' : 'Item'} expense created successfully`)
    showCreateDialog.value = false
    resetForm()
    await loadEventExpenses()
  } catch (error) {
    if (error !== 'cancel') {
      const msg = error?.response?.data || 'Failed to create expense'
      ElMessage.error(msg)
    }
  } finally {
    saving.value = false
  }
}


const updateExpense = async () => {
  try {
    await editExpenseFormRef.value.validate()
    updating.value = true
    
    // Clear the opposite field based on selected tab
    if (activeTab.value === 'item') {
      editExpenseForm.ExpenseServiceId = null
    } else {
      editExpenseForm.EventExpenseId = null
    }

    const payload = {
      EventExpenseId: activeTab.value === 'item' ? editExpenseForm.EventExpenseId : null,
      ExpenseServiceId: activeTab.value === 'service' ? editExpenseForm.ExpenseServiceId : null,
      eventId: editExpenseForm.eventId,
      price: editExpenseForm.price,
    }
    
    await axios.put(`${API_BASE}/event-expenses/${editExpenseForm.id}`, payload)
    ElMessage.success(`${activeTab.value === 'service' ? 'Service' : 'Item'} expense updated successfully`)
    showEditDialog.value = false
    resetEditForm()
    await loadEventExpenses()
  } catch (error) {
    if (error !== 'cancel') {
      const msg = error?.response?.data || 'Failed to update expense'
      ElMessage.error(msg)
    }
  } finally {
    updating.value = false
  }
}

// Window resize handler
const handleResize = () => {
  windowWidth.value = window.innerWidth
}

onMounted(() => {
  loadExpenseItems()
  loadExpenseServices()
  loadEvents()
  loadEventExpenses()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.expenses-container {
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

.filters-section {
  padding: 20px 0;
  border-bottom: 1px solid #ebeef5;
  margin-bottom: 20px;
}

.pagination-container {
  margin-top: 15px;
  text-align: right;
}

.expenses-card {
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

/* Responsive Styles */
@media (max-width: 768px) {
  .expenses-container {
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
  
  .search-filters .el-row {
    row-gap: 10px;
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

  /* Adjust form label width for mobile */
  .el-dialog .el-form .el-form-item__label {
    width: 100px !important;
  }

  /* Make tables responsive */
  .table-container {
    overflow-x: auto;
  }

  .pagination-container {
    text-align: center;
  }
  
  .pagination-container .el-pagination {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .expenses-container {
    padding: 5px;
  }
  
  .card-header h2 {
    font-size: 18px;
  }
  
  .search-filters {
    margin-bottom: 10px;
  }

  /* Extra small screen dialog adjustments */
  .el-dialog .el-form .el-form-item__label {
    width: 80px !important;
    font-size: 12px;
    text-align: left !important;
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
  
  .pagination-container .el-pagination {
    font-size: 12px;
  }
  
  .pagination-container .el-pagination .el-pager li {
    min-width: 28px;
    height: 28px;
    line-height: 28px;
  }
}
</style>
