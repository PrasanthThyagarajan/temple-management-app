<template>
  <div class="api-test">
    <el-card>
      <template #header>
        <h2>API Endpoint Test</h2>
      </template>
      
      <el-row :gutter="20">
        <el-col :span="12">
          <h3>Test Results</h3>
          <div v-for="(result, endpoint) in testResults" :key="endpoint" class="test-result">
            <el-tag :type="result.success ? 'success' : 'danger'">
              {{ endpoint }}: {{ result.success ? 'OK' : 'FAIL' }}
            </el-tag>
            <span v-if="result.count !== undefined"> ({{ result.count }} records)</span>
            <div v-if="result.error" class="error-message">{{ result.error }}</div>
          </div>
        </el-col>
        
        <el-col :span="12">
          <h3>Actions</h3>
          <el-button @click="testAllEndpoints" :loading="testing">Test All Endpoints</el-button>
          <el-button @click="clearResults">Clear Results</el-button>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'
import { ElMessage } from 'element-plus'

const testing = ref(false)
const testResults = ref({})

const endpoints = [
  '/api/areas',
  '/api/events', 
  '/api/event-types',
  '/api/products',
  '/api/sales',
  '/api/devotees',
  '/api/donations',
  '/api/temples',
  '/api/categories',
  '/api/roles',
  '/api/permissions'
]

const testEndpoint = async (endpoint) => {
  try {
    console.log(`🔍 Testing ${endpoint}...`)
    const response = await axios.get(endpoint)
    const count = Array.isArray(response.data) ? response.data.length : 'N/A'
    console.log(`✅ ${endpoint}: ${count} records`)
    return { success: true, count, data: response.data }
  } catch (error) {
    console.error(`❌ ${endpoint}:`, error)
    return { 
      success: false, 
      error: error.response?.data?.message || error.message,
      status: error.response?.status
    }
  }
}

const testAllEndpoints = async () => {
  testing.value = true
  testResults.value = {}
  
  console.log('🚀 Starting API endpoint tests...')
  
  for (const endpoint of endpoints) {
    const result = await testEndpoint(endpoint)
    testResults.value[endpoint] = result
  }
  
  const successCount = Object.values(testResults.value).filter(r => r.success).length
  const totalCount = endpoints.length
  
  console.log(`📊 Test Summary: ${successCount}/${totalCount} endpoints working`)
  
  if (successCount === totalCount) {
    ElMessage.success(`All ${totalCount} endpoints are working!`)
  } else {
    ElMessage.warning(`${successCount}/${totalCount} endpoints working`)
  }
  
  testing.value = false
}

const clearResults = () => {
  testResults.value = {}
}
</script>

<style scoped>
.api-test {
  padding: 20px;
}

.test-result {
  margin-bottom: 10px;
}

.error-message {
  color: #f56c6c;
  font-size: 12px;
  margin-left: 10px;
}
</style>
