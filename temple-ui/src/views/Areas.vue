<template>
  <div class="areas-container">
    <!-- Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Areas" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><OfficeBuilding /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Active Temples" :value="summaryStats.activeTemples">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><House /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Areas with Description" :value="summaryStats.withDescription">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><Document /></el-icon>
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

    <el-card class="areas-card">
      <template #header>
        <div class="card-header">
          <h2>Area Management</h2>
          <el-button type="primary" @click="openCreateDialog" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Area
          </el-button>
        </div>
      </template>

      <!-- Enhanced Filters -->
      <div class="filters-section">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="8">
            <el-input 
              v-model="searchTerm" 
              placeholder="Search areas..." 
              clearable 
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8">
            <el-select 
              v-model="selectedTempleId" 
              placeholder="Filter by temple" 
              clearable 
              @change="handleTempleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Temple" value="" />
              <el-option 
                v-for="t in temples" 
                :key="t.id" 
                :label="t.name" 
                :value="t.id" 
              />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8">
            <el-select 
              v-model="sortBy" 
              placeholder="Sort by" 
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
              <el-option label="Temple (A-Z)" value="temple-asc" />
              <el-option label="Temple (Z-A)" value="temple-desc" />
              <el-option label="Recently Added" value="id-desc" />
              <el-option label="Oldest First" value="id-asc" />
            </el-select>
          </el-col>
        </el-row>
      </div>

      <!-- Table with Sorting -->
      <el-table 
        :data="paginatedAreas" 
        v-loading="loading" 
        stripe 
        style="width: 100%"
        @sort-change="handleTableSortChange"
      >
        <el-table-column 
          prop="name" 
          label="Name" 
          min-width="200" 
          sortable="custom"
        />
        <el-table-column 
          prop="temple.name" 
          label="Temple" 
          min-width="200" 
          sortable="custom"
        />
        <el-table-column 
          prop="description" 
          label="Description" 
          min-width="250" 
          show-overflow-tooltip
        />
        <el-table-column label="Actions" width="220" fixed="right" v-if="canUpdate || canDelete">
          <template #default="scope">
            <el-button size="small" @click.stop="editArea(scope.row)" v-if="canUpdate">
              <el-icon><Edit /></el-icon>
              Edit
            </el-button>
            <el-button size="small" type="danger" @click.stop="deleteArea(scope.row.id)" v-if="canDelete">
              <el-icon><Delete /></el-icon>
              Delete
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Enhanced Pagination -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="filteredBeforePagination.length"
          :background="true"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>

    <el-dialog v-model="showDialog" :title="editingArea ? 'Edit Area' : 'Add Area'" width="600px">
      <el-form ref="areaFormRef" :model="areaForm" :rules="areaRules" label-width="120px">
        <el-form-item label="Temple" prop="templeId">
          <el-select v-model="areaForm.templeId" placeholder="Select temple" style="width: 100%">
            <el-option v-for="t in temples" :key="t.id" :label="t.name" :value="t.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="Name" prop="name">
          <el-input v-model="areaForm.name" placeholder="Enter area name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input v-model="areaForm.description" type="textarea" :rows="3" placeholder="Enter description" />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showDialog = false">Cancel</el-button>
          <el-button type="primary" :loading="saving" @click="saveArea" v-if="canCreate || canUpdate">{{ editingArea ? 'Update' : 'Create' }}</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Edit, Delete, OfficeBuilding, House, Document, Filter } from '@element-plus/icons-vue'
import axios from 'axios'

const API_BASE = '/api'

const areas = ref([])
const temples = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const showDialog = ref(false)
const editingArea = ref(null)
const areaFormRef = ref()
const selectedTempleId = ref('')
const sortBy = ref('name-asc')

// Permission states
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

const areaForm = reactive({
  templeId: '',
  name: '',
  description: ''
})

const areaRules = {
  templeId: [{ required: true, message: 'Temple is required', trigger: 'change' }],
  name: [{ required: true, message: 'Name is required', trigger: 'blur' }]
}

// Summary Statistics
const summaryStats = computed(() => {
  const filtered = filteredBeforePagination.value
  return {
    total: areas.value.length,
    activeTemples: [...new Set(areas.value.map(a => a.templeId))].length,
    withDescription: areas.value.filter(a => a.description && a.description.trim()).length,
    filtered: filtered.length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...areas.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(a =>
      a.name.toLowerCase().includes(term) || 
      (a.description || '').toLowerCase().includes(term) ||
      (a.temple?.name || '').toLowerCase().includes(term)
    )
  }
  
  // Apply temple filter
  if (selectedTempleId.value) {
    result = result.filter(a => a.templeId === selectedTempleId.value)
  }
  
  // Apply sorting
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      
      switch(field) {
        case 'name':
          aVal = a.name.toLowerCase()
          bVal = b.name.toLowerCase()
          break
        case 'temple':
          aVal = (a.temple?.name || '').toLowerCase()
          bVal = (b.temple?.name || '').toLowerCase()
          break
        case 'id':
          aVal = a.id
          bVal = b.id
          break
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
const paginatedAreas = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

const handleTempleFilterChange = () => {
  currentPage.value = 1
}

const handleSortChange = () => {
  currentPage.value = 1
}

const handleTableSortChange = ({ prop, order }) => {
  if (!order) {
    sortBy.value = 'name-asc'
  } else {
    const field = prop === 'temple.name' ? 'temple' : prop
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const loadAreas = async () => {
  try {
    loading.value = true
    const res = await axios.get(`${API_BASE}/areas`)
    areas.value = res.data
  } catch (e) {
    ElMessage.error('Failed to load areas')
  } finally {
    loading.value = false
  }
}

const loadTemples = async () => {
  try {
    const res = await axios.get(`${API_BASE}/temples`)
    temples.value = res.data
  } catch (e) {}
}

const openCreateDialog = () => {
  editingArea.value = null
  Object.assign(areaForm, { templeId: '', name: '', description: '' })
  showDialog.value = true
}

const editArea = (area) => {
  editingArea.value = area
  Object.assign(areaForm, { templeId: area.templeId, name: area.name, description: area.description })
  showDialog.value = true
}

const saveArea = async () => {
  try {
    await areaFormRef.value.validate()
    saving.value = true
    if (editingArea.value) {
      await axios.put(`${API_BASE}/areas/${editingArea.value.id}`, areaForm)
      ElMessage.success('Area updated successfully')
    } else {
      await axios.post(`${API_BASE}/areas`, areaForm)
      ElMessage.success('Area created successfully')
    }
    showDialog.value = false
    await loadAreas()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('Failed to save area')
  } finally {
    saving.value = false
  }
}

const deleteArea = async (id) => {
  try {
    await ElMessageBox.confirm('Delete this area?', 'Warning', { type: 'warning' })
    await axios.delete(`${API_BASE}/areas/${id}`)
    ElMessage.success('Area deleted successfully')
    await loadAreas()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('Failed to delete area')
  }
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

onMounted(async () => {
  loadAreas()
  loadTemples()
  
  // Check permissions
  if (window["templeAuth"]) {
    canCreate.value = await window["templeAuth"].hasCreatePermission('/areas')
    canUpdate.value = await window["templeAuth"].hasUpdatePermission('/areas')
    canDelete.value = await window["templeAuth"].hasDeletePermission('/areas')
  }
})
</script>

<style scoped>
.areas-container { 
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

.areas-card {
  margin-top: 20px;
}

.card-header { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
}

.filters-section {
  margin-bottom: 20px;
}

.pagination-container { 
  margin-top: 20px; 
  display: flex;
  justify-content: flex-end;
}

.el-table {
  font-size: 14px;
}

.el-table .el-button {
  padding: 5px 10px;
}
</style>


