<template>
  <div class="products-container">
    <el-card class="products-card">
      <template #header>
        <div class="card-header">
          <h2>Product Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            Add Product
          </el-button>
        </div>
      </template>

      <!-- Devotional Banner -->
      <div class="devotional-banner products-banner"></div>

      <!-- Search and Filters -->
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
              @change="handleCategoryFilter"
            >
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
              @change="handleStatusFilter"
            >
              <el-option label="Active" value="Active" />
              <el-option label="Inactive" value="Inactive" />
              <el-option label="Out of Stock" value="Out of Stock" />
            </el-select>
          </el-col>
          <el-col :xs="24" :sm="24" :md="10" :lg="10" :xl="10">
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

      <!-- Summary Cards -->
      <div class="summary-cards">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><Box /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ totalProducts }}</div>
                  <div class="summary-label">Total Products</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon active">
                  <el-icon><Check /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ activeProducts }}</div>
                  <div class="summary-label">Active</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon low-stock">
                  <el-icon><Warning /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ lowStockProducts }}</div>
                  <div class="summary-label">Low Stock</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="6" :md="6" :lg="6" :xl="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon out-of-stock">
                  <el-icon><Close /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ outOfStockProducts }}</div>
                  <div class="summary-label">Out of Stock</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Grid View -->
      <div v-if="showGridView" class="grid-view">
        <el-row :gutter="20">
          <el-col :xs="12" :sm="8" :md="6" :lg="6" :xl="6" v-for="product in products" :key="product.id">
            <el-card class="product-card" @click="viewProduct(product)">
              <div class="product-image">
                <el-icon class="product-icon"><Box /></el-icon>
              </div>
              <div class="product-info">
                <h4>{{ product.name }}</h4>
                <p class="product-category">{{ product.category || product.categoryNavigation?.name }}</p>
                <p class="product-price">₹{{ product.price.toLocaleString() }}</p>
                <el-tag :type="getStatusTagType(product.status)" size="small">
                  {{ product.status }}
                </el-tag>
                <p class="product-stock">Stock: {{ product.stockQuantity }}</p>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Table View -->
      <div v-else class="table-container">
        <el-table
          :data="products"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
        >
          <el-table-column prop="name" label="Product Name" min-width="200" show-overflow-tooltip />
          <el-table-column prop="category" label="Category" width="120" show-overflow-tooltip>
            <template #default="scope">
              {{ scope.row.category || scope.row.categoryNavigation?.name }}
            </template>
          </el-table-column>
          <el-table-column prop="price" label="Price" width="100">
            <template #default="scope">
              ₹{{ scope.row.price.toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="stockQuantity" label="Stock" width="80" />
          <el-table-column prop="minStockLevel" label="Min Stock" width="100" />
          <el-table-column prop="status" label="Status" width="100">
            <template #default="scope">
              <el-tag :type="getStatusTagType(scope.row.status)" size="small">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="Description" min-width="200" show-overflow-tooltip />
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="editProduct(scope.row)">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button
                  size="small"
                  type="success"
                  @click.stop="updateProductStatus(scope.row.id, 'Active')"
                  v-if="scope.row.status === 'Inactive'"
                >
                  <el-icon><Check /></el-icon>
                  <span class="btn-text">Activate</span>
                </el-button>
                <el-button
                  size="small"
                  type="warning"
                  @click.stop="updateProductStatus(scope.row.id, 'Inactive')"
                  v-if="scope.row.status === 'Active'"
                >
                  <el-icon><Close /></el-icon>
                  <span class="btn-text">Deactivate</span>
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="deleteProduct(scope.row.id)"
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
            :current-page="currentPage"
            :page-size="pageSize"
            :page-sizes="[10, 20, 50, 100]"
            :total="totalProducts"
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
      width="600px"
    >
      <el-form
        ref="productFormRef"
        :model="productForm"
        :rules="productRules"
        label-width="120px"
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
          <el-col :span="12">
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
          <el-col :span="12">
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
            <el-form-item label="Status" prop="status">
              <el-select v-model="productForm.status" placeholder="Select status" style="width: 100%">
                <el-option label="Active" value="Active" />
                <el-option label="Inactive" value="Inactive" />
                <el-option label="Out of Stock" value="Out of Stock" />
              </el-select>
            </el-form-item>
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
          <el-button type="primary" @click="saveProduct" :loading="saving">
            {{ editingProduct ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Product Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Product Details"
      width="600px"
    >
      <div v-if="selectedProduct" class="product-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Product Name">
            {{ selectedProduct.name }}
          </el-descriptions-item>
          <el-descriptions-item label="Status">
            <el-tag :type="getStatusTagType(selectedProduct.status)">
              {{ selectedProduct.status }}
            </el-tag>
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
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Grid, Box, Check, Warning, Close } from '@element-plus/icons-vue'
import axios from 'axios'

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
const totalProducts = ref(0)
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const showGridView = ref(false)
const editingProduct = ref(null)
const selectedProduct = ref(null)

// Form data
const productForm = reactive({
  name: '',
  category: '',
  categoryId: null,

  price: 0,
  stockQuantity: 0,
  minStockLevel: 0,
  status: 'Active',
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
  status: [{ required: true, message: 'Status is required', trigger: 'change' }],
  description: [{ required: true, message: 'Description is required', trigger: 'blur' }]
}

const productFormRef = ref()

// API base URL
const API_BASE = 'http://localhost:5051/api'

// Computed properties
const activeProducts = computed(() => {
  return products.value.filter(product => product.status === 'Active').length
})

const lowStockProducts = computed(() => {
  return products.value.filter(product => 
    product.status === 'Active' && product.stockQuantity <= product.minStockLevel
  ).length
})

const outOfStockProducts = computed(() => {
  return products.value.filter(product => product.status === 'Out of Stock').length
})

// Methods
const loadProducts = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/products`)
    products.value = response.data
    totalProducts.value = response.data.length
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
  if (searchTerm.value.trim()) {
    searchProducts(searchTerm.value)
  } else {
    loadProducts()
  }
}

const searchProducts = async (term) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/products/search/${term}`)
    products.value = response.data
    totalProducts.value = response.data.length
  } catch (error) {
    console.error('Error searching products:', error)
    ElMessage.error('Failed to search products')
  } finally {
    loading.value = false
  }
}

const handleCategoryFilter = () => {
  if (categoryFilter.value) {
    // Prefer filtering by CategoryId when available; fallback to Category name
    const selected = categoryFilter.value
    products.value = products.value.filter(product => {
      if (product.categoryId != null) return product.categoryId === selected
      if (product.category) {
        const match = categories.value.find(c => c.id === selected)
        return match ? product.category === match.name : false
      }
      return false
    })
    totalProducts.value = products.value.length
  } else {
    loadProducts()
  }
}

const handleStatusFilter = () => {
  if (statusFilter.value) {
    products.value = products.value.filter(product => product.status === statusFilter.value)
    totalProducts.value = products.value.length
  } else {
    loadProducts()
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
    status: product.status,
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
    status: 'Active',
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
  loadProducts()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadProducts()
}

// Lifecycle
onMounted(() => {
  loadProducts()
  loadCategories()
})
</script>

<style scoped>
.products-container {
  padding: 20px;
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
    flex-wrap: nowrap;
    overflow-x: auto;
    gap: 5px;
  }
  
  .action-buttons .el-button {
    width: 100%;
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
