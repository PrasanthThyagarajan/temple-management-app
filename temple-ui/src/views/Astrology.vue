<template>
  <div class="astrology">
    <el-row :gutter="20" class="page-header">
      <el-col :span="24">
        <h1>Astrology & Panchang</h1>
        <p>Get daily Panchang, weekly and monthly horoscope predictions</p>
      </el-col>
    </el-row>

    <!-- Service Selection -->
    <el-row :gutter="20" class="service-selection">
      <el-col :span="24">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>Select Service</span>
            </div>
          </template>
          <el-form :model="serviceForm" label-width="120px">
            <el-form-item label="Service Type">
              <el-select v-model="serviceForm.serviceType" placeholder="Select a service" @change="onServiceChange">
                <el-option label="Daily Panchang" value="panchang" />
                <el-option label="Daily Horoscope" value="daily-horoscope" />
                <el-option label="Weekly Horoscope" value="weekly-horoscope" />
                <el-option label="Monthly Horoscope" value="monthly-horoscope" />
              </el-select>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>
    </el-row>

    <!-- Input Form -->
    <el-row :gutter="20" class="input-form" v-if="serviceForm.serviceType">
      <el-col :span="24">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>Input Parameters</span>
            </div>
          </template>
          
          <!-- Panchang Form -->
          <el-form v-if="serviceForm.serviceType === 'panchang'" :model="panchangForm" label-width="120px">
            <el-row :gutter="20">
              <el-col :xs="24" :sm="12" :md="8">
                <el-form-item label="Date">
                  <el-date-picker
                    v-model="panchangForm.date"
                    type="date"
                    placeholder="Select date"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8">
                <el-form-item label="Latitude">
                  <el-input-number
                    v-model="panchangForm.latitude"
                    :precision="6"
                    :step="0.000001"
                    :min="-90"
                    :max="90"
                    placeholder="Enter latitude"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8">
                <el-form-item label="Longitude">
                  <el-input-number
                    v-model="panchangForm.longitude"
                    :precision="6"
                    :step="0.000001"
                    :min="-180"
                    :max="180"
                    placeholder="Enter longitude"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
            </el-row>
            <el-row>
              <el-col :span="24">
                <el-form-item label="Timezone">
                  <el-select v-model="panchangForm.timezone" placeholder="Select timezone" style="width: 100%">
                    <el-option label="Asia/Kolkata" value="Asia/Kolkata" />
                    <el-option label="Asia/Dubai" value="Asia/Dubai" />
                    <el-option label="America/New_York" value="America/New_York" />
                    <el-option label="Europe/London" value="Europe/London" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item>
              <el-button type="primary" @click="fetchPanchang" :loading="loading">
                Get Panchang
              </el-button>
            </el-form-item>
          </el-form>

          <!-- Horoscope Form -->
          <el-form v-else :model="horoscopeForm" label-width="120px">
            <el-row :gutter="20">
              <el-col :xs="24" :sm="12" :md="8">
                <el-form-item label="Date">
                  <el-date-picker
                    v-model="horoscopeForm.date"
                    type="date"
                    placeholder="Select date"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8">
                <el-form-item label="Zodiac Sign">
                  <el-select v-model="horoscopeForm.zodiacSign" placeholder="Select zodiac sign" style="width: 100%">
                    <el-option label="Aries" value="aries" />
                    <el-option label="Taurus" value="taurus" />
                    <el-option label="Gemini" value="gemini" />
                    <el-option label="Cancer" value="cancer" />
                    <el-option label="Leo" value="leo" />
                    <el-option label="Virgo" value="virgo" />
                    <el-option label="Libra" value="libra" />
                    <el-option label="Scorpio" value="scorpio" />
                    <el-option label="Sagittarius" value="sagittarius" />
                    <el-option label="Capricorn" value="capricorn" />
                    <el-option label="Aquarius" value="aquarius" />
                    <el-option label="Pisces" value="pisces" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12" :md="8">
                <el-form-item label="Language">
                  <el-select v-model="horoscopeForm.language" placeholder="Select language" style="width: 100%">
                    <el-option label="English" value="en" />
                    <el-option label="Hindi" value="hi" />
                    <el-option label="Tamil" value="ta" />
                    <el-option label="Malayalam" value="ml" />
                    <el-option label="Telugu" value="te" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item>
              <el-button type="primary" @click="fetchHoroscope" :loading="loading">
                Get {{ getHoroscopeTypeText() }}
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>
    </el-row>

    <!-- Results Display -->
    <el-row :gutter="20" class="results" v-if="result">
      <el-col :span="24">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>Results</span>
              <el-button type="text" @click="clearResults">Clear</el-button>
            </div>
          </template>
          
          <!-- Panchang Results -->
          <div v-if="serviceForm.serviceType === 'panchang' && result.data">
            <div class="panchang-results">
              <h3>Panchang for {{ formatDate(result.data.date) }}</h3>
              <el-row :gutter="20">
                <el-col :xs="24" :sm="12" :md="6">
                  <div class="info-card">
                    <h4>Tithi</h4>
                    <p>{{ result.data.tithi }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="6">
                  <div class="info-card">
                    <h4>Nakshatra</h4>
                    <p>{{ result.data.nakshatra }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="6">
                  <div class="info-card">
                    <h4>Day</h4>
                    <p>{{ result.data.day }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="6">
                  <div class="info-card">
                    <h4>Sunrise</h4>
                    <p>{{ result.data.sunrise }}</p>
                  </div>
                </el-col>
              </el-row>
              
              <el-row :gutter="20" v-if="result.data.auspiciousTimings && result.data.auspiciousTimings.length > 0">
                <el-col :span="24">
                  <h4>Auspicious Timings</h4>
                  <el-table :data="result.data.auspiciousTimings" stripe>
                    <el-table-column prop="name" label="Name" />
                    <el-table-column prop="startTime" label="Start Time" />
                    <el-table-column prop="endTime" label="End Time" />
                    <el-table-column prop="description" label="Description" />
                  </el-table>
                </el-col>
              </el-row>
            </div>
          </div>

          <!-- Horoscope Results -->
          <div v-else-if="result.data">
            <div class="horoscope-results">
              <h3>{{ getHoroscopeTypeText() }} for {{ result.data.zodiacSign }}</h3>
              <el-row :gutter="20">
                <el-col :xs="24" :sm="12" :md="8">
                  <div class="prediction-card">
                    <h4>General Prediction</h4>
                    <p>{{ result.data.prediction }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="8">
                  <div class="prediction-card">
                    <h4>Love</h4>
                    <p>{{ result.data.love }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="8">
                  <div class="prediction-card">
                    <h4>Career</h4>
                    <p>{{ result.data.career }}</p>
                  </div>
                </el-col>
              </el-row>
              
              <el-row :gutter="20">
                <el-col :xs="24" :sm="12" :md="8">
                  <div class="prediction-card">
                    <h4>Health</h4>
                    <p>{{ result.data.health }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="8">
                  <div class="prediction-card">
                    <h4>Finance</h4>
                    <p>{{ result.data.finance }}</p>
                  </div>
                </el-col>
                <el-col :xs="24" :sm="12" :md="8">
                  <div class="prediction-card">
                    <h4>Lucky Number</h4>
                    <p>{{ result.data.luckyNumber }}</p>
                  </div>
                </el-col>
              </el-row>
            </div>
          </div>

          <!-- Error Display -->
          <div v-else-if="!result.success">
            <el-alert
              :title="result.message"
              type="error"
              show-icon
              :closable="false"
            />
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import axios from 'axios'
import dayjs from 'dayjs'
import { ElMessage } from 'element-plus'

// Form data
const serviceForm = reactive({
  serviceType: ''
})

const panchangForm = reactive({
  date: new Date(),
  latitude: 10.8505, // Default to Kerala coordinates
  longitude: 76.2711,
  timezone: 'Asia/Kolkata'
})

const horoscopeForm = reactive({
  date: new Date(),
  zodiacSign: '',
  language: 'en'
})

// State
const loading = ref(false)
const result = ref(null)

// Methods
const onServiceChange = () => {
  result.value = null
}

const getHoroscopeTypeText = () => {
  switch (serviceForm.serviceType) {
    case 'daily-horoscope':
      return 'Daily Horoscope'
    case 'weekly-horoscope':
      return 'Weekly Horoscope'
    case 'monthly-horoscope':
      return 'Monthly Horoscope'
    default:
      return 'Horoscope'
  }
}

const formatDate = (date) => {
  return dayjs(date).format('MMMM DD, YYYY')
}

const fetchPanchang = async () => {
  if (!panchangForm.date || !panchangForm.latitude || !panchangForm.longitude) {
    ElMessage.warning('Please fill in all required fields')
    return
  }

  loading.value = true
  try {
    const requestData = {
      date: dayjs(panchangForm.date).format('YYYY-MM-DD'),
      latitude: panchangForm.latitude,
      longitude: panchangForm.longitude,
      timezone: panchangForm.timezone
    }

    const response = await axios.post('/api/panchang', requestData)
    result.value = response.data
    
    if (response.data.success) {
      ElMessage.success('Panchang data fetched successfully')
    } else {
      ElMessage.error(response.data.message || 'Failed to fetch Panchang data')
    }
  } catch (error) {
    console.error('Error fetching Panchang:', error)
    ElMessage.error('Error fetching Panchang data')
    result.value = {
      success: false,
      message: 'Error fetching Panchang data'
    }
  } finally {
    loading.value = false
  }
}

const fetchHoroscope = async () => {
  if (!horoscopeForm.date || !horoscopeForm.zodiacSign) {
    ElMessage.warning('Please fill in all required fields')
    return
  }

  loading.value = true
  try {
    const requestData = {
      date: dayjs(horoscopeForm.date).format('YYYY-MM-DD'),
      zodiacSign: horoscopeForm.zodiacSign,
      language: horoscopeForm.language
    }

    let endpoint = ''
    switch (serviceForm.serviceType) {
      case 'daily-horoscope':
        endpoint = '/api/horoscope/daily'
        break
      case 'weekly-horoscope':
        endpoint = '/api/horoscope/weekly'
        break
      case 'monthly-horoscope':
        endpoint = '/api/horoscope/monthly'
        break
    }

    const response = await axios.post(endpoint, requestData)
    result.value = response.data
    
    if (response.data.success) {
      ElMessage.success(`${getHoroscopeTypeText()} fetched successfully`)
    } else {
      ElMessage.error(response.data.message || `Failed to fetch ${getHoroscopeTypeText()}`)
    }
  } catch (error) {
    console.error('Error fetching horoscope:', error)
    ElMessage.error(`Error fetching ${getHoroscopeTypeText()}`)
    result.value = {
      success: false,
      message: `Error fetching ${getHoroscopeTypeText()}`
    }
  } finally {
    loading.value = false
  }
}

const clearResults = () => {
  result.value = null
}
</script>

<style scoped>
.astrology {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  margin-bottom: 30px;
  text-align: center;
}

.page-header h1 {
  color: #303133;
  margin-bottom: 10px;
  font-size: 32px;
}

.page-header p {
  color: #606266;
  font-size: 16px;
}

.service-selection,
.input-form,
.results {
  margin-bottom: 30px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info-card,
.prediction-card {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
  margin-bottom: 20px;
}

.info-card h4,
.prediction-card h4 {
  margin: 0 0 10px 0;
  color: #303133;
  font-size: 16px;
  font-weight: 600;
}

.info-card p,
.prediction-card p {
  margin: 0;
  color: #606266;
  font-size: 14px;
  line-height: 1.5;
}

.panchang-results h3,
.horoscope-results h3 {
  margin: 0 0 20px 0;
  color: #303133;
  font-size: 20px;
  font-weight: 600;
}

.panchang-results h4,
.horoscope-results h4 {
  margin: 20px 0 10px 0;
  color: #303133;
  font-size: 16px;
  font-weight: 600;
}

/* Responsive Design */
@media (max-width: 768px) {
  .astrology {
    padding: 10px;
  }
  
  .page-header h1 {
    font-size: 24px;
  }
  
  .page-header p {
    font-size: 14px;
  }
  
  .info-card,
  .prediction-card {
    padding: 15px;
    margin-bottom: 15px;
  }
  
  .info-card h4,
  .prediction-card h4 {
    font-size: 14px;
  }
  
  .info-card p,
  .prediction-card p {
    font-size: 12px;
  }
}
</style>

