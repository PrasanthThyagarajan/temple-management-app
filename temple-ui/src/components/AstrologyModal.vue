<template>
  <div class="astrology-modal">
    <!-- Service Selection -->
    <el-row :gutter="20" class="service-selection">
      <el-col :span="24">
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
      </el-col>
    </el-row>

    <!-- Input Form -->
    <el-row :gutter="20" class="input-form" v-if="serviceForm.serviceType">
      <el-col :span="24">
        <!-- Panchang Form -->
        <el-form v-if="serviceForm.serviceType === 'panchang'" :model="panchangForm" label-width="120px">
          <el-row :gutter="20">
            <el-col :xs="24" :sm="12" :md="8">
              <el-form-item label="Date & Time">
                <el-date-picker
                  v-model="panchangForm.dateTime"
                  type="datetime"
                  placeholder="Select date and time"
                  style="width: 100%"
                  format="YYYY-MM-DD HH:mm:ss"
                  value-format="YYYY-MM-DD HH:mm:ss"
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
              <el-form-item label="Date & Time">
                <el-date-picker
                  v-model="horoscopeForm.dateTime"
                  type="datetime"
                  placeholder="Select date and time"
                  style="width: 100%"
                  format="YYYY-MM-DD HH:mm:ss"
                  value-format="YYYY-MM-DD HH:mm:ss"
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
      </el-col>
    </el-row>

    <!-- Results Display -->
    <el-row :gutter="20" class="results" v-if="result">
      <el-col :span="24">
        <!-- Panchang Results -->
        <div v-if="serviceForm.serviceType === 'panchang' && result.data">
          <div class="panchang-results">
            <h3>Panchang for {{ formatDateTime(result.data.date) }}</h3>
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
      </el-col>
    </el-row>

    <!-- Modal Actions -->
    <div class="dialog-footer">
      <el-button @click="$emit('close')">Close</el-button>
      <el-button v-if="result" type="primary" @click="clearResults">Clear Results</el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import axios from 'axios'
import dayjs from 'dayjs'
import { ElMessage } from 'element-plus'

// Emits
const emit = defineEmits(['close'])

// Form data
const serviceForm = reactive({
  serviceType: ''
})

const panchangForm = reactive({
  dateTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
  latitude: 10.8505, // Default to Kerala coordinates
  longitude: 76.2711,
  timezone: 'Asia/Kolkata'
})

const horoscopeForm = reactive({
  dateTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
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

const formatDateTime = (dateTime) => {
  return dayjs(dateTime).format('MMMM DD, YYYY HH:mm')
}

const fetchPanchang = async () => {
  if (!panchangForm.dateTime || !panchangForm.latitude || !panchangForm.longitude) {
    ElMessage.warning('Please fill in all required fields')
    return
  }

  loading.value = true
  try {
    const requestData = {
      date: dayjs(panchangForm.dateTime).format('YYYY-MM-DD'),
      time: dayjs(panchangForm.dateTime).format('HH:mm:ss'),
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
  if (!horoscopeForm.dateTime || !horoscopeForm.zodiacSign) {
    ElMessage.warning('Please fill in all required fields')
    return
  }

  loading.value = true
  try {
    const requestData = {
      date: dayjs(horoscopeForm.dateTime).format('YYYY-MM-DD'),
      time: dayjs(horoscopeForm.dateTime).format('HH:mm:ss'),
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
.astrology-modal {
  max-width: 100%;
}

.service-selection,
.input-form,
.results {
  margin-bottom: 20px;
}

.info-card,
.prediction-card {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 8px;
  text-align: center;
  margin-bottom: 15px;
}

.info-card h4,
.prediction-card h4 {
  margin: 0 0 8px 0;
  color: #303133;
  font-size: 14px;
  font-weight: 600;
}

.info-card p,
.prediction-card p {
  margin: 0;
  color: #606266;
  font-size: 12px;
  line-height: 1.4;
}

.panchang-results h3,
.horoscope-results h3 {
  margin: 0 0 15px 0;
  color: #303133;
  font-size: 18px;
  font-weight: 600;
}

.panchang-results h4,
.horoscope-results h4 {
  margin: 15px 0 8px 0;
  color: #303133;
  font-size: 14px;
  font-weight: 600;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

/* Responsive Design */
@media (max-width: 768px) {
  .info-card,
  .prediction-card {
    padding: 12px;
    margin-bottom: 12px;
  }
  
  .info-card h4,
  .prediction-card h4 {
    font-size: 12px;
  }
  
  .info-card p,
  .prediction-card p {
    font-size: 11px;
  }
  
  .panchang-results h3,
  .horoscope-results h3 {
    font-size: 16px;
  }
}
</style>
