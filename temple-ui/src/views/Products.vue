<template>
  <div class="products-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Products" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Box /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card active">
          <el-statistic title="Active Products" :value="summaryStats.active">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><Check /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Out of Stock" :value="summaryStats.outOfStock">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #f56c6c;"><Warning /></el-icon>
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

    <el-card class="products-card">
      <template #header>
        <div class="card-header">
          <h2>Product Management</h2>
          <el-button type="primary" @click="showCreateDialog = true" v-if="canCreate">
            <el-icon><Plus /></el-icon>
            Add Product
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner products-banner"></div>

      <!-- Enhanced Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
            <el-input
              v-model="searchTerm"
              placeholder="Search products..."
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
              v-model="categoryFilter"
              placeholder="Filter by Category"
              clearable
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Category" value="" />
              <el-option
                v-for="cat in categories"
                :key="cat.id"
                :label="cat.name"
                :value="cat.id"
              />
            </el-select>
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
              <el-option label="Active" value="Active" />
              <el-option label="Inactive" value="Inactive" />
              <el-option label="Out of Stock" value="Out of Stock" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select
              v-model="sortBy"
              placeholder="Sort by"
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
              <el-option label="Price (Low to High)" value="price-asc" />
              <el-option label="Price (High to Low)" value="price-desc" />
              <el-option label="Recently Added" value="id-desc" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="24" :md="8" :lg="8" :xl="8">
            <div class="action-buttons">
              <el-button @click="loadProducts" :loading="loading" class="refresh-btn">
                <el-icon><Refresh /></el-icon>
                <span class="btn-text">Refresh</span>
              </el-button>
              <el-button type="success" @click="showGridView = !showGridView" class="view-toggle-btn">
                <el-icon><Grid /></el-icon>
                <span class="btn-text">{{ showGridView ? 'List View' : 'Grid View' }}</span>
              </el-button>
            </div>
          </el-col>
        </el-row>
      </div>


      <!-- Grid View -->
      <div v-if="showGridView" class="grid-view">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="8" :md="6" :lg="6" :xl="6" v-for="product in paginatedProducts" :key="product.id">
            <el-card class="product-card" @click="viewProduct(product)">
              <div class="product-image">
                <el-icon class="product-icon"><Box /></el-icon>
              </div>
              <div class="product-info">
                <h4>{{ product.name }}</h4>
                <p class="product-category">{{ product.category || product.categoryNavigation?.name }}</p>
                <p class="product-price">₹{{ product.price.toLocaleString() }}</p>
                <p class="product-stock">Stock: {{ product.stockQuantity }}</p>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Table View -->
      <div v-else class="table-container">
        <el-table
          :data="paginatedProducts"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
          @sort-change="handleTableSortChange"
        >
          <el-table-column 
            prop="name" 
            label="Product Name" 
            min-width="200" 
            show-overflow-tooltip 
            sortable="custom"
          />
          <el-table-column 
            prop="category" 
            label="Category" 
            width="120" 
            show-overflow-tooltip
            sortable="custom"
          >
            <template #default="scope">
              {{ scope.row.category || scope.row.categoryNavigation?.name }}
            </template>
          </el-table-column>
          <el-table-column 
            prop="price" 
            label="Price" 
            width="100"
            sortable="custom"
          >
            <template #default="scope">
              ₹{{ scope.row.price.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="stockQuantity" label="Stock" width="80" />
          <el-table-column prop="minStockLevel" label="Min Stock" width="100" />
          <el-table-column prop="description" label="Description" min-width="200" show-overflow-tooltip />
          <el-table-column label="Actions" width="200" fixed="right" v-if="canUpdate || canDelete">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editProduct(scope.row)" v-if="canUpdate">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="success"
                  @click.stop="updateProductStatus(scope.row.id, 'Active')"
                  v-if="scope.row.status === 'Inactive' && canUpdate"
                >
                  <el-icon><Check /></el-icon>
                  <span class="btn-text">Activate</span>
                </el-button>
                <el-button
                  size="small"
                  type="warning"
                  @click.stop="updateProductStatus(scope.row.id, 'Inactive')"
                  v-if="scope.row.status === 'Active' && canUpdate"
                >
                  <el-icon><Close /></el-icon>
                  <span class="btn-text">Deactivate</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteProduct(scope.row.id)"
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

    <!-- Create/Edit Product Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingProduct ? 'Edit Product' : 'Add New Product'"
      :width="dialogWidth"
      :fullscreen="isMobileDialog"
    >
      <el-form
        ref="productFormRef"
        :model="productForm"
        :rules="productRules"
        :label-width="isMobileDialog ? '100px' : '120px'"
      >
        <el-form-item label="Product Name" prop="name">
          <el-input v-model="productForm.name" placeholder="Enter product name" />
        </el-form-item>
        <el-form-item label="Category" prop="categoryId">
          <el-select v-model="productForm.categoryId" placeholder="Select category" style="width: 100%" clearable>
            <el-option
              v-for="category in categories"
              :key="category.id"
              :label="category.name"
              :value="category.id"
            />
          </el-select>
        </el-form-item>

        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Price" prop="price">
              <el-input-number
                v-model="productForm.price"
                :min="0"
                :precision="2"
                style="width: 100%"
                placeholder="Enter price"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Stock Quantity" prop="stockQuantity">
              <el-input-number
                v-model="productForm.stockQuantity"
                :min="0"
                style="width: 100%"
                placeholder="Enter stock quantity"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="12" :lg="12">
            <el-form-item label="Min Stock Level" prop="minStockLevel">
              <el-input-number
                v-model="productForm.minStockLevel"
                :min="0"
                style="width: 100%"
                placeholder="Enter minimum stock level"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
          </el-col>
        </el-row>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="productForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter product description"
          />
        </el-form-item>
        <el-form-item label="Notes" prop="notes">
          <el-input
            v-model="productForm.notes"
            type="textarea"
            :rows="2"
            placeholder="Enter any additional notes"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveProduct" :loading="saving" v-if="canCreate || canUpdate">
            {{ editingProduct ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Product Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Product Details"
      :width="dialogWidth"
      :fullscreen="isMobileDialog"
    >
      <div v-if="selectedProduct" class="product-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Product Name">
            {{ selectedProduct.name }}
          </el-descriptions-item>
          <el-descriptions-item label="Category">{{ selectedProduct.category }}</el-descriptions-item>

          <el-descriptions-item label="Price">
            ₹{{ selectedProduct.price.toLocaleString() }}
          </el-descriptions-item>
          <el-descriptions-item label="Stock Quantity">{{ selectedProduct.stockQuantity }}</el-descriptions-item>
          <el-descriptions-item label="Min Stock Level">{{ selectedProduct.minStockLevel }}</el-descriptions-item>
          <el-descriptions-item label="Description" :span="2">
            {{ selectedProduct.description }}
          </el-descriptions-item>
          <el-descriptions-item label="Notes" :span="2">
            {{ selectedProduct.notes || 'No notes available' }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Grid, Box, Check, Warning, Close, Filter } from '@element-plus/icons-vue'
import axios from 'axios'

// Permission states
const canCreate = ref(false)
const canUpdate = ref(false)
const canDelete = ref(false)

// Reactive data
const products = ref([])
const categories = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const categoryFilter = ref(null)
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('name-asc')
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const showGridView = ref(false)
const editingProduct = ref(null)
const selectedProduct = ref(null)

// Responsive properties
const windowWidth = ref(window.innerWidth)
const isMobileDialog = computed(() => windowWidth.value < 768)
const dialogWidth = computed(() => {
  if (windowWidth.value < 576) return '100%'
  if (windowWidth.value < 768) return '95%'
  if (windowWidth.value < 992) return '90%'
  return '600px'
})

// Form data
const productForm = reactive({
  name: '',
  category: '',
  categoryId: null,

  price: 0,
  stockQuantity: 0,
  minStockLevel: 0,
  description: '',
  notes: ''
})

// Form validation rules
const productRules = {
  name: [{ required: true, message: 'Product name is required', trigger: 'blur' }],
  category: [{ required: true, message: 'Category is required', trigger: 'change' }],
  price: [{ required: true, message: 'Price is required', trigger: 'blur' }],
  stockQuantity: [{ required: true, message: 'Stock quantity is required', trigger: 'blur' }],
  minStockLevel: [{ required: true, message: 'Minimum stock level is required', trigger: 'blur' }],
  description: [{ required: true, message: 'Description is required', trigger: 'blur' }]
}

const productFormRef = ref()

// API base URL
const API_BASE = '/api'

// Summary Statistics
const summaryStats = computed(() => {
  return {
    total: products.value.length,
    lowStock: products.value.filter(p => 
      p.stockQuantity <= p.minStockLevel
    ).length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...products.value]

  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(p =>
      p.name?.toLowerCase().includes(term) ||
      p.description?.toLowerCase().includes(term) ||
      p.category?.toLowerCase().includes(term) ||
      p.categoryNavigation?.name?.toLowerCase().includes(term)
    )
  }

  // Apply category filter
  if (categoryFilter.value) {
    result = result.filter(p => {
      if (p.categoryId != null) return p.categoryId === categoryFilter.value
      if (p.category) {
        const match = categories.value.find(c => c.id === categoryFilter.value)
        return match ? p.category === match.name : false
      }
      return false
    })
  }

  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(p => p.status === statusFilter.value)
  }

  // Apply sorting
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      
      switch(field) {
        case 'name':
          aVal = (a.name || '').toLowerCase()
          bVal = (b.name || '').toLowerCase()
          break
        case 'price':
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
})

// Paginated data
const paginatedProducts = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
})

// Methods
const loadProducts = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/products`)
    products.value = response.data
  } catch (error) {
    console.error('Error loading products:', error)
    ElMessage.error('Failed to load products')
  } finally {
    loading.value = false
  }
}

const loadCategories = async () => {
  try {
    const response = await axios.get(`${API_BASE}/categories/active`)
    categories.value = response.data
  } catch (error) {
    console.error('Error loading categories:', error)
    ElMessage.error('Failed to load categories')
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
    sortBy.value = 'name-asc'
  } else {
    let field = prop
    if (prop === 'price') field = 'price'
    else if (prop === 'name') field = 'name'
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const editProduct = (product) => {
  editingProduct.value = product
  Object.assign(productForm, {
    name: product.name,
    category: product.category,
    categoryId: product.categoryId,
    price: product.price,
    stockQuantity: product.stockQuantity,
    minStockLevel: product.minStockLevel,
    description: product.description,
    notes: product.notes
  })
  showCreateDialog.value = true
}

const saveProduct = async () => {
  try {
    await productFormRef.value.validate()
    saving.value = true
    
    if (editingProduct.value) {
      // Update existing product
      await axios.put(`${API_BASE}/products/${editingProduct.value.id}`, productForm)
      ElMessage.success('Product updated successfully')
    } else {
      // Create new product
      await axios.post(`${API_BASE}/products`, productForm)
      ElMessage.success('Product created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadProducts()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save product')
    }
  } finally {
    saving.value = false
  }
}

const updateProductStatus = async (id, status) => {
  try {
    await axios.put(`${API_BASE}/products/${id}/status`, null, {
      params: { status }
    })
    ElMessage.success('Product status updated successfully')
    loadProducts()
  } catch (error) {
    console.error('Error updating product status:', error)
    ElMessage.error('Failed to update product status')
  }
}

const deleteProduct = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this product?',
      'Warning',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/products/${id}`)
    ElMessage.success('Product deleted successfully')
    loadProducts()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting product:', error)
      ElMessage.error('Failed to delete product')
    }
  }
}

const handleRowClick = (row) => {
  selectedProduct.value = row
  showDetailsDialog.value = true
}

const viewProduct = (product) => {
  selectedProduct.value = product
  showDetailsDialog.value = true
}

const resetForm = () => {
  editingProduct.value = null
  Object.assign(productForm, {
    name: '',
    category: '',
    categoryId: null,
    price: 0,
    stockQuantity: 0,
    minStockLevel: 0,
    description: '',
    notes: ''
  })
  productFormRef.value?.resetFields()
}

const getStatusTagType = (status) => {
  const statusMap = {
    'Active': 'success',
    'Inactive': 'info',
    'Out of Stock': 'danger'
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
  loadProducts()
  loadCategories()
  window.addEventListener('resize', handleResize)
  
  // Check permissions
  if (window["templeAuth"]) {
    canCreate.value = await window["templeAuth"].hasCreatePermission('/products')
    canUpdate.value = await window["templeAuth"].hasUpdatePermission('/products')
    canDelete.value = await window["templeAuth"].hasDeletePermission('/products')
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.products-container {
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

.products-card {
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

/* Ensure filter rows wrap nicely on smaller widths */
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

.summary-icon.active {
  background-color: #67c23a;
}

.summary-icon.low-stock {
  background-color: #e6a23c;
}

.summary-icon.out-of-stock {
  background-color: #f56c6c;
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

.grid-view {
  margin-bottom: 20px;
}

.product-card {
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 20px;
}

.product-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.product-image {
  text-align: center;
  padding: 20px;
  background-color: #f5f7fa;
  margin-bottom: 15px;
}

.product-icon {
  font-size: 48px;
  color: #409eff;
}

.product-info h4 {
  margin: 0 0 10px 0;
  color: #303133;
  font-size: 16px;
}

.product-category {
  color: #909399;
  font-size: 14px;
  margin: 0 0 10px 0;
}

.product-price {
  color: #67c23a;
  font-size: 18px;
  font-weight: bold;
  margin: 0 0 10px 0;
}

.product-stock {
  color: #606266;
  font-size: 14px;
  margin: 10px 0 0 0;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.product-details {
  padding: 20px 0;
}

.dialog-footer {
  text-align: right;
}

.service-category-type {
  color: #909399;
  font-size: 12px;
  font-style: italic;
}

/* Responsive Design */
.table-container {
  overflow-x: auto;
  margin-bottom: 20px;
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

.action-buttons {
  display: flex;
  gap: 5px;
  flex-wrap: nowrap;
  align-items: center;
}

.btn-text {
  display: inline;
}

.refresh-btn,
.view-toggle-btn {
  flex: 1;
  min-width: 0;
}

@media (max-width: 768px) {
  .products-container {
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
  
  .action-buttons {
    flex-direction: row;
    flex-wrap: wrap;
    gap: 8px;
  }
  
  .action-buttons .el-button {
    flex: 1 1 48%;
    min-width: 140px;
    font-size: 12px;
  }
  
  .btn-text {
    display: none;
  }
  
  .refresh-btn .btn-text,
  .view-toggle-btn .btn-text {
    display: inline;
  }
  
  .summary-cards {
    margin-bottom: 15px;
  }
  
  .summary-card {
    margin-bottom: 10px;
  }
  
  .summary-content {
    gap: 10px;
  }
  
  .summary-icon {
    width: 40px;
    height: 40px;
    font-size: 20px;
  }
  
  .summary-value {
    font-size: 20px;
  }
  
  .summary-label {
    font-size: 12px;
  }
  
  .product-card {
    margin-bottom: 15px;
  }
  
  .product-info h4 {
    font-size: 14px;
  }
  
  .product-category,
  .product-price,
  .product-stock {
    font-size: 12px;
  }
  
  .pagination-container {
    text-align: center;
  }
  
  .pagination-container .el-pagination {
    justify-content: center;
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

  /* Make descriptions responsive */
  .el-descriptions .el-descriptions__label {
    font-size: 12px;
  }
  
  .el-descriptions .el-descriptions__content {
    font-size: 12px;
  }
}

@media (max-width: 480px) {
  .products-container {
    padding: 5px;
  }
  
  .card-header h2 {
    font-size: 18px;
  }
  
  .search-filters {
    margin-bottom: 10px;
  }
  
  .action-buttons .el-button {
    font-size: 11px;
    padding: 5px 8px;
  }
  
  .summary-icon {
    width: 35px;
    height: 35px;
    font-size: 18px;
  }
  
  .summary-value {
    font-size: 18px;
  }
  
  .summary-label {
    font-size: 11px;
  }
  
  .product-card {
    margin-bottom: 10px;
  }
  
  .product-image {
    padding: 15px;
  }
  
  .product-icon {
    font-size: 36px;
  }
  
  .product-info h4 {
    font-size: 13px;
  }
  
  .product-category,
  .product-price,
  .product-stock {
    font-size: 11px;
  }
  
  .pagination-container .el-pagination {
    font-size: 12px;
  }
  
  .pagination-container .el-pagination .el-pager li {
    min-width: 28px;
    height: 28px;
    line-height: 28px;
  }

  /* Extra small screen dialog adjustments */
  .el-dialog .el-form .el-form-item__label {
    width: 80px !important;
    font-size: 12px;
    text-align: left !important;
  }

  /* Responsive form layout */
  .el-dialog .el-form-item {
    margin-bottom: 15px;
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

/* Tablet Styles */
@media (min-width: 769px) and (max-width: 1024px) {
  .products-container {
    padding: 15px;
  }
  
  .action-buttons .el-button {
    font-size: 13px;
  }
  
  .summary-icon {
    width: 45px;
    height: 45px;
    font-size: 22px;
  }
  
  .summary-value {
    font-size: 22px;
  }
}
</style>
