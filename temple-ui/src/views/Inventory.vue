<template>
  <div class="inventory-container">
    <!-- Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Items" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Box /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Active Items" :value="summaryStats.active">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><Check /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Value" :value="summaryStats.totalValue" :precision="2" prefix="₹">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><Money /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Precious Items" :value="summaryStats.preciousItems">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #e6a23c;"><Star /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="inventory-card">
      <template #header>
        <div class="card-header">
          <h2>Inventory Management</h2>
          <el-button type="primary" @click="openCreateDialog" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Item
          </el-button>
        </div>
      </template>

      <!-- Enhanced Filters -->
      <div class="filters-section">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6">
            <el-input 
              v-model="searchTerm" 
              placeholder="Search items..." 
              clearable 
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-select 
              v-model="selectedTempleId" 
              placeholder="Filter by temple" 
              clearable 
              @change="handleTempleFilterChange"
              style="width: 100%"
            >
              <el-option label="All Temples" value="" />
              <el-option 
                v-for="temple in temples" 
                :key="temple.id" 
                :label="temple.name" 
                :value="temple.id" 
              />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-select 
              v-model="selectedAreaId" 
              placeholder="Filter by area" 
              clearable 
              @change="handleAreaFilterChange"
              style="width: 100%"
              :disabled="!selectedTempleId"
            >
              <el-option label="All Areas" value="" />
              <el-option 
                v-for="area in filteredAreas" 
                :key="area.id" 
                :label="area.name" 
                :value="area.id" 
              />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-select 
              v-model="selectedItemWorth" 
              placeholder="Filter by worth" 
              clearable 
              @change="handleWorthFilterChange"
              style="width: 100%"
            >
              <el-option label="All Worth Levels" value="" />
              <el-option label="Low" :value="1" />
              <el-option label="Medium" :value="2" />
              <el-option label="High" :value="3" />
              <el-option label="Precious" :value="4" />
            </el-select>
          </el-col>
        </el-row>
        <el-row :gutter="20" style="margin-top: 15px;">
          <el-col :xs="24" :sm="12" :md="6">
            <el-select 
              v-model="sortBy" 
              placeholder="Sort by" 
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
              <el-option label="Value (Low to High)" value="value-asc" />
              <el-option label="Value (High to Low)" value="value-desc" />
              <el-option label="Quantity (Low to High)" value="quantity-asc" />
              <el-option label="Quantity (High to Low)" value="quantity-desc" />
              <el-option label="Recently Added" value="date-desc" />
              <el-option label="Oldest First" value="date-asc" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <el-checkbox v-model="showActiveOnly" @change="handleActiveFilterChange">
              Show Active Only
            </el-checkbox>
          </el-col>
        </el-row>
      </div>

      <!-- Inventory Table -->
      <el-table 
        :data="paginatedData" 
        style="width: 100%" 
        v-loading="loading"
        @sort-change="handleTableSortChange"
      >
        <el-table-column prop="itemName" label="Item Name" sortable="custom" min-width="180">
          <template #default="scope">
            <div class="item-name-cell">
              <span>{{ scope.row.itemName }}</span>
              <el-tag v-if="!scope.row.active" type="danger" size="small" style="margin-left: 8px;">Inactive</el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="templeName" label="Temple" min-width="150" />
        <el-table-column prop="areaName" label="Area" min-width="120" />
        <el-table-column prop="itemWorth" label="Worth" width="100">
          <template #default="scope">
            <el-tag :type="getWorthTagType(scope.row.itemWorth)">
              {{ getWorthDisplay(scope.row.itemWorth) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="approximatePrice" label="Price" width="120" align="right">
          <template #default="scope">
            ₹{{ formatCurrency(scope.row.approximatePrice) }}
          </template>
        </el-table-column>
        <el-table-column prop="quantity" label="Quantity" width="100" align="center" />
        <el-table-column prop="totalValue" label="Total Value" width="140" align="right">
          <template #default="scope">
            ₹{{ formatCurrency(scope.row.approximatePrice * scope.row.quantity) }}
          </template>
        </el-table-column>
        <el-table-column prop="createdDate" label="Created Date" width="120">
          <template #default="scope">
            {{ formatDate(scope.row.createdDate) }}
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="120" fixed="right">
          <template #default="scope">
            <el-button-group>
              <el-button size="small" @click="openEditDialog(scope.row)" v-if="canUpdate">
                <el-icon><Edit /></el-icon>
              </el-button>
              <el-button size="small" type="danger" @click="handleDelete(scope.row)" v-if="canDelete">
                <el-icon><Delete /></el-icon>
              </el-button>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div class="pagination-container">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[10, 25, 50, 100]"
          :total="filteredBeforePagination.length"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <!-- Create/Edit Dialog -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? 'Edit Item' : 'Add New Item'" width="600px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="140px">
        <el-form-item label="Temple" prop="templeId">
          <el-select v-model="form.templeId" placeholder="Select temple" style="width: 100%" @change="handleTempleChange">
            <el-option 
              v-for="temple in temples" 
              :key="temple.id" 
              :label="temple.name" 
              :value="temple.id" 
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Area" prop="areaId">
          <el-select v-model="form.areaId" placeholder="Select area" style="width: 100%" :disabled="!form.templeId">
            <el-option 
              v-for="area in formAreas" 
              :key="area.id" 
              :label="area.name" 
              :value="area.id" 
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Item Name" prop="itemName">
          <el-input v-model="form.itemName" placeholder="Enter item name" />
        </el-form-item>
        <el-form-item label="Item Worth" prop="itemWorth">
          <el-select v-model="form.itemWorth" placeholder="Select worth level" style="width: 100%">
            <el-option label="Low" :value="1" />
            <el-option label="Medium" :value="2" />
            <el-option label="High" :value="3" />
            <el-option label="Precious" :value="4" />
          </el-select>
        </el-form-item>
        <el-form-item label="Approximate Price" prop="approximatePrice">
          <el-input-number v-model="form.approximatePrice" :min="0" :precision="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Quantity" prop="quantity">
          <el-input-number v-model="form.quantity" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Created Date" prop="createdDate">
          <el-date-picker
            v-model="form.createdDate"
            type="datetime"
            placeholder="Select date and time"
            style="width: 100%"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss"
          />
        </el-form-item>
        <el-form-item label="Active" prop="active">
          <el-switch v-model="form.active" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="closeDialog">Cancel</el-button>
        <el-button type="primary" @click="handleSave" :loading="saving">
          {{ isEdit ? 'Update' : 'Create' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete, Search, Box, Check, Money, Star } from '@element-plus/icons-vue'
import axios from 'axios'
// Removed import - using global templeAuth instead

// Data
const inventory = ref([])
const temples = ref([])
const areas = ref([])
const loading = ref(false)
const saving = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const currentId = ref(null)
const formRef = ref()

// Search, filter and pagination
const searchTerm = ref('')
const selectedTempleId = ref('')
const selectedAreaId = ref('')
const selectedItemWorth = ref('')
const showActiveOnly = ref(true)
const sortBy = ref('date-desc')
const currentPage = ref(1)
const pageSize = ref(25)

// Form data
const form = reactive({
  templeId: null,
  areaId: null,
  itemName: '',
  itemWorth: 2,
  approximatePrice: 0,
  quantity: 1,
  createdDate: new Date().toISOString(),
  active: true
})

// Form validation rules
const rules = {
  templeId: [
    { required: true, message: 'Please select a temple', trigger: 'change' }
  ],
  areaId: [
    { required: true, message: 'Please select an area', trigger: 'change' }
  ],
  itemName: [
    { required: true, message: 'Please enter item name', trigger: 'blur' },
    { min: 2, max: 200, message: 'Length should be 2 to 200', trigger: 'blur' }
  ],
  itemWorth: [
    { required: true, message: 'Please select item worth', trigger: 'change' }
  ],
  approximatePrice: [
    { required: true, message: 'Please enter approximate price', trigger: 'blur' },
    { type: 'number', min: 0, message: 'Price must be positive', trigger: 'blur' }
  ],
  quantity: [
    { required: true, message: 'Please enter quantity', trigger: 'blur' },
    { type: 'number', min: 1, message: 'Quantity must be at least 1', trigger: 'blur' }
  ],
  createdDate: [
    { required: true, message: 'Please select created date', trigger: 'change' }
  ]
}

// Permissions
const canRead = ref(false)
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

// Computed
const filteredAreas = computed(() => {
  if (!selectedTempleId.value) return []
  return areas.value.filter(area => area.templeId === parseInt(selectedTempleId.value))
})

const formAreas = computed(() => {
  if (!form.templeId) return []
  return areas.value.filter(area => area.templeId === form.templeId)
})

const filteredData = computed(() => {
  let result = [...inventory.value]
  
  // Search filter
  if (searchTerm.value) {
    const search = searchTerm.value.toLowerCase()
    result = result.filter(item => 
      item.itemName.toLowerCase().includes(search) ||
      item.templeName?.toLowerCase().includes(search) ||
      item.areaName?.toLowerCase().includes(search)
    )
  }
  
  // Temple filter
  if (selectedTempleId.value) {
    result = result.filter(item => item.templeId === parseInt(selectedTempleId.value))
  }
  
  // Area filter
  if (selectedAreaId.value) {
    result = result.filter(item => item.areaId === parseInt(selectedAreaId.value))
  }
  
  // Worth filter
  if (selectedItemWorth.value) {
    result = result.filter(item => item.itemWorth === selectedItemWorth.value)
  }
  
  // Active filter
  if (showActiveOnly.value) {
    result = result.filter(item => item.active)
  }
  
  // Sorting
  const [field, order] = sortBy.value.split('-')
  result.sort((a, b) => {
    let aVal, bVal
    
    switch (field) {
      case 'name':
        aVal = a.itemName.toLowerCase()
        bVal = b.itemName.toLowerCase()
        break
      case 'value':
        aVal = a.approximatePrice
        bVal = b.approximatePrice
        break
      case 'quantity':
        aVal = a.quantity
        bVal = b.quantity
        break
      case 'date':
        aVal = new Date(a.createdDate)
        bVal = new Date(b.createdDate)
        break
      default:
        aVal = a.id
        bVal = b.id
    }
    
    if (order === 'asc') {
      return aVal > bVal ? 1 : -1
    } else {
      return aVal < bVal ? 1 : -1
    }
  })
  
  return result
})

const filteredBeforePagination = computed(() => filteredData.value)

const paginatedData = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredBeforePagination.value.slice(start, end)
})

const summaryStats = computed(() => {
  const all = inventory.value
  const active = all.filter(i => i.active)
  const totalValue = active.reduce((sum, i) => sum + (i.approximatePrice * i.quantity), 0)
  const precious = all.filter(i => i.itemWorth === 4).length
  
  return {
    total: all.length,
    active: active.length,
    totalValue: totalValue,
    preciousItems: precious
  }
})

// Methods
const fetchInventory = async () => {
  loading.value = true
  try {
    const response = await axios.get('/api/inventories')
    inventory.value = response.data
  } catch (error) {
    console.error('Error fetching inventory:', error)
    ElMessage.error('Failed to load inventory items')
  } finally {
    loading.value = false
  }
}

const fetchTemples = async () => {
  try {
    const response = await axios.get('/api/temples')
    temples.value = response.data
  } catch (error) {
    console.error('Error fetching temples:', error)
    ElMessage.error('Failed to load temples')
  }
}

const fetchAreas = async () => {
  try {
    const response = await axios.get('/api/areas')
    areas.value = response.data
  } catch (error) {
    console.error('Error fetching areas:', error)
    ElMessage.error('Failed to load areas')
  }
}

const openCreateDialog = () => {
  isEdit.value = false
  currentId.value = null
  resetForm()
  dialogVisible.value = true
}

const openEditDialog = (row) => {
  isEdit.value = true
  currentId.value = row.id
  Object.assign(form, {
    templeId: row.templeId,
    areaId: row.areaId,
    itemName: row.itemName,
    itemWorth: row.itemWorth,
    approximatePrice: row.approximatePrice,
    quantity: row.quantity,
    createdDate: row.createdDate,
    active: row.active
  })
  dialogVisible.value = true
}

const closeDialog = () => {
  dialogVisible.value = false
  resetForm()
}

const resetForm = () => {
  form.templeId = null
  form.areaId = null
  form.itemName = ''
  form.itemWorth = 2
  form.approximatePrice = 0
  form.quantity = 1
  form.createdDate = new Date().toISOString()
  form.active = true
  formRef.value?.resetFields()
}

const handleSave = async () => {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  
  saving.value = true
  try {
    if (isEdit.value) {
      await axios.put(`/api/inventories/${currentId.value}`, form)
      ElMessage.success('Item updated successfully')
    } else {
      await axios.post('/api/inventories', form)
      ElMessage.success('Item created successfully')
    }
    closeDialog()
    fetchInventory()
  } catch (error) {
    console.error('Error saving inventory item:', error)
    const message = error.response?.data || 'Failed to save item'
    ElMessage.error(message)
  } finally {
    saving.value = false
  }
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete "${row.itemName}"?`,
      'Confirm Delete',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    await axios.delete(`/api/inventories/${row.id}`)
    ElMessage.success('Item deleted successfully')
    fetchInventory()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting inventory item:', error)
      ElMessage.error('Failed to delete item')
    }
  }
}

const handleTempleChange = () => {
  form.areaId = null
}

const handleSearch = () => {
  currentPage.value = 1
}

const handleTempleFilterChange = () => {
  selectedAreaId.value = ''
  currentPage.value = 1
}

const handleAreaFilterChange = () => {
  currentPage.value = 1
}

const handleWorthFilterChange = () => {
  currentPage.value = 1
}

const handleActiveFilterChange = () => {
  currentPage.value = 1
}

const handleSortChange = () => {
  currentPage.value = 1
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'date-desc'
    return
  }
  
  const orderStr = order === 'ascending' ? 'asc' : 'desc'
  
  switch (prop) {
    case 'itemName':
      sortBy.value = `name-${orderStr}`
      break
    case 'approximatePrice':
      sortBy.value = `value-${orderStr}`
      break
    case 'quantity':
      sortBy.value = `quantity-${orderStr}`
      break
    case 'createdDate':
      sortBy.value = `date-${orderStr}`
      break
  }
}

const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

const getWorthTagType = (worth) => {
  switch (worth) {
    case 1: return 'info'
    case 2: return 'success'
    case 3: return 'warning'
    case 4: return 'danger'
    default: return 'info'
  }
}

const getWorthDisplay = (worth) => {
  switch (worth) {
    case 1: return 'Low'
    case 2: return 'Medium'
    case 3: return 'High'
    case 4: return 'Precious'
    default: return 'Unknown'
  }
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-IN', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value)
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString('en-IN')
}

// Initialize permissions
const initializePermissions = async () => {
  try {
    // Check permissions
    if (window["templeAuth"]) {
      canRead.value = await window["templeAuth"].hasPagePermission('/inventories', 'Read')
      canCreate.value = await window["templeAuth"].hasCreatePermission('/inventories')
      canUpdate.value = await window["templeAuth"].hasUpdatePermission('/inventories')
      canDelete.value = await window["templeAuth"].hasDeletePermission('/inventories')
    }
  } catch (error) {
    console.error('Error checking permissions:', error)
  }
}

// Lifecycle
onMounted(async () => {
  await initializePermissions()
  fetchInventory()
  fetchTemples()
  fetchAreas()
})
</script>

<style scoped>
.inventory-container {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.summary-cards {
  margin-bottom: 20px;
}

.summary-card {
  transition: all 0.3s ease;
}

.summary-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.inventory-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #303133;
}

.filters-section {
  margin-bottom: 20px;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.item-name-cell {
  display: flex;
  align-items: center;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 768px) {
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .card-header h2 {
    font-size: 1.25rem;
  }
  
  .filters-section {
    padding: 15px;
  }
}
</style>
