<template>
  <div class="event-expense-items-container">
    <el-card class="event-expense-items-card">
      <template #header>
        <div class="card-header">
          <h2>Expense Items Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            Add Event Expense Item
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner events-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
            <el-input
              v-model="searchTerm"
              placeholder="Search expense items..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-button type="info" @click="loadExpenseItems" title="Refresh data">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Summary Cards -->
      <div class="summary-cards">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="12" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><Document /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ filteredExpenseItems.length }}</div>
                  <div class="summary-label">Total Items</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="12" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon active">
                  <el-icon><Check /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ activeItemsCount }}</div>
                  <div class="summary-label">Active Items</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Table -->
      <div class="table-container">
        <el-table 
          :data="paginatedExpenseItems" 
          stripe 
          style="width: 100%"
          :loading="loading"
          v-loading="loading"
        >
          <el-table-column prop="name" label="Item Name" min-width="200" sortable>
            <template #default="scope">
              <strong>{{ scope.row.name }}</strong>
            </template>
          </el-table-column>
          <el-table-column prop="isApprovalNeeded" label="Approval Needed" width="160">
            <template #default="scope">
              <el-tag :type="scope.row.isApprovalNeeded ? 'warning' : 'info'">
                {{ scope.row.isApprovalNeeded ? 'Yes' : 'No' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="Description" min-width="300">
            <template #default="scope">
              <span v-if="scope.row.description">{{ scope.row.description }}</span>
              <span v-else class="text-muted">No description</span>
            </template>
          </el-table-column>
          <el-table-column prop="createdAt" label="Created Date" width="180" sortable>
            <template #default="scope">
              {{ formatDate(scope.row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="Status" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.isActive ? 'success' : 'danger'">
                {{ scope.row.isActive ? 'Active' : 'Inactive' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <el-button size="small" @click="editExpenseItem(scope.row)">
                <el-icon><Edit /></el-icon>
                Edit
              </el-button>
              <el-button 
                size="small" 
                type="danger" 
                @click="deleteExpenseItem(scope.row)"
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
          :total="filteredExpenseItems.length"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingExpenseItem ? 'Edit Event Expense Item' : 'Create Event Expense Item'"
      width="600px"
    >
      <el-form 
        ref="expenseItemFormRef" 
        :model="expenseItemForm" 
        :rules="expenseItemRules" 
        label-width="140px"
      >
        <el-form-item label="Item Name" prop="name">
          <el-input 
            v-model="expenseItemForm.name" 
            placeholder="Enter item name"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input 
            v-model="expenseItemForm.description" 
            placeholder="Enter description"
            type="textarea"
            :rows="3"
            maxlength="500"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="Approval Needed" prop="isApprovalNeeded">
          <el-switch v-model="expenseItemForm.isApprovalNeeded" active-text="Yes" inactive-text="No" />
        </el-form-item>
        <el-form-item label="Status" prop="isActive">
          <el-radio-group v-model="expenseItemForm.isActive">
            <el-radio :label="true">Active</el-radio>
            <el-radio :label="false">Inactive</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="onCancel">Cancel</el-button>
          <el-button 
            type="primary" 
            :loading="saving" 
            @click="saveExpenseItem"
          >
            {{ editingExpenseItem ? 'Update' : 'Create' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Document, Check } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const expenseItems = ref([])
const roles = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const showCreateDialog = ref(false)
const editingExpenseItem = ref(null)
const expenseItemFormRef = ref()

// Form data
const expenseItemForm = reactive({
  name: '',
  description: '',
  isApprovalNeeded: false,
  isActive: true,
})

// Form validation rules
const expenseItemRules = {
  name: [
    { required: true, message: 'Item name is required', trigger: 'blur' },
    { min: 1, max: 200, message: 'Length should be 1 to 200 characters', trigger: 'blur' }
  ],
  description: [
    { max: 500, message: 'Length should not exceed 500 characters', trigger: 'blur' }
  ],
  isApprovalNeeded: []
}

const API_BASE = '/api'

// Computed properties
const filteredExpenseItems = computed(() => {
  if (!searchTerm.value) return expenseItems.value
  
  const search = searchTerm.value.toLowerCase()
  return expenseItems.value.filter(item => 
    item.name?.toLowerCase().includes(search) ||
    item.description?.toLowerCase().includes(search)
  )
})

const paginatedExpenseItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredExpenseItems.value.slice(start, end)
})

const activeItemsCount = computed(() => {
  return expenseItems.value.filter(item => item.isActive).length
})

// Methods
const formatDate = (dateString) => {
  if (!dateString) return '—'
  return dayjs(dateString).format('MMM DD, YYYY HH:mm')
}

const handleSearch = () => {
  currentPage.value = 1
}

const handleSizeChange = (val) => {
  pageSize.value = val
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

const loadExpenseItems = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/expense-items`)
    expenseItems.value = response.data || []
  } catch (error) {
    console.error('Failed to load expense items:', error)
    ElMessage.error('Failed to load expense items')
    expenseItems.value = []
  } finally {
    loading.value = false
  }
}

const loadRoles = async () => {
  try {
    const response = await axios.get(`${API_BASE}/roles`)
    roles.value = response.data || []
  } catch (error) {
    console.error('Failed to load roles:', error)
    ElMessage.error('Failed to load roles')
    roles.value = []
  }
}

const editExpenseItem = (item) => {
  editingExpenseItem.value = item
  expenseItemForm.name = item.name
  expenseItemForm.description = item.description || ''
  expenseItemForm.isApprovalNeeded = !!item.isApprovalNeeded
  expenseItemForm.isActive = item.isActive !== undefined ? item.isActive : true
  showCreateDialog.value = true
}

const deleteExpenseItem = async (item) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete "${item.name}"?`,
      'Confirm Delete',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/expense-items/${item.id}`)
    ElMessage.success('Expense item deleted successfully')
    await loadExpenseItems()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to delete expense item:', error)
      ElMessage.error('Failed to delete expense item')
    }
  }
}

const onCancel = () => {
  showCreateDialog.value = false
  resetForm()
}

const resetForm = () => {
  editingExpenseItem.value = null
  expenseItemForm.name = ''
  expenseItemForm.description = ''
  expenseItemForm.isApprovalNeeded = false
  expenseItemForm.isActive = true
  expenseItemFormRef.value?.resetFields()
}

const saveExpenseItem = async () => {
  try {
    await expenseItemFormRef.value.validate()
    saving.value = true
    
    const payload = {
      name: expenseItemForm.name.trim(),
      description: expenseItemForm.description.trim(),
      isApprovalNeeded: !!expenseItemForm.isApprovalNeeded,
      isActive: expenseItemForm.isActive,
    }
    
    if (editingExpenseItem.value) {
      // Update existing item
      await axios.put(`${API_BASE}/expense-items/${editingExpenseItem.value.id}`, payload)
      ElMessage.success('Expense item updated successfully')
    } else {
      // Create new item
      await axios.post(`${API_BASE}/expense-items`, payload)
      ElMessage.success('Expense item created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    await loadExpenseItems()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to save expense item:', error)
      const message = error?.response?.data?.message || 'Failed to save expense item'
      ElMessage.error(message)
    }
  } finally {
    saving.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadExpenseItems()
  loadRoles()
})
</script>

<style scoped>
.event-expense-items-container {
  padding: 20px;
}

.event-expense-items-card {
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

.summary-icon.active {
  background-color: #67c23a;
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

.table-container {
  width: 100%;
  overflow-x: auto;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.dialog-footer {
  text-align: right;
}

.text-muted {
  color: #909399;
  font-style: italic;
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
}
</style>
