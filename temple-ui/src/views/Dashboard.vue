<template>
  <div class="dashboard">
    <el-row :gutter="20" class="dashboard-header">
      <el-col :span="24">
        <h2>Welcome to Devakaryam</h2>
        <p>Comprehensive Temple Management System</p>
      </el-col>
    </el-row>

    <el-row :gutter="20" class="stats-cards">
      <el-col :span="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <el-icon class="stat-icon temples"><HomeFilled /></el-icon>
            <div class="stat-info">
              <h3>{{ stats.temples }}</h3>
              <p>Total Temples</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :span="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <el-icon class="stat-icon devotees"><User /></el-icon>
            <div class="stat-info">
              <h3>{{ stats.devotees }}</h3>
              <p>Total Devotees</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :span="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <el-icon class="stat-icon donations"><Money /></el-icon>
            <div class="stat-info">
              <h3>₹{{ stats.totalDonations.toLocaleString() }}</h3>
              <p>Total Donations</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :span="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <el-icon class="stat-icon events"><Calendar /></el-icon>
            <div class="stat-info">
              <h3>{{ stats.events }}</h3>
              <p>Upcoming Events</p>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" class="dashboard-content">
      <el-col :span="12">
        <el-card class="recent-temples">
          <template #header>
            <div class="card-header">
              <span>Recent Temples</span>
              <el-button type="text" @click="$router.push('/temples')">View All</el-button>
            </div>
          </template>
          <div v-if="recentTemples.length > 0">
            <div v-for="temple in recentTemples" :key="temple.id" class="temple-item">
              <h4>{{ temple.name }}</h4>
              <p>{{ temple.city }}, {{ temple.state }}</p>
            </div>
          </div>
          <el-empty v-else description="No temples found" />
        </el-card>
      </el-col>
      
      <el-col :span="12">
        <el-card class="upcoming-events">
          <template #header>
            <div class="card-header">
              <span>Upcoming Events</span>
              <el-button type="text" @click="$router.push('/events')">View All</el-button>
            </div>
          </template>
          <div v-if="upcomingEvents.length > 0">
            <div v-for="event in upcomingEvents" :key="event.id" class="event-item">
              <h4>{{ event.name }}</h4>
              <p>{{ formatDate(event.startDate) }} - {{ event.eventType }}</p>
            </div>
          </div>
          <el-empty v-else description="No upcoming events" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { HomeFilled, User, Money, Calendar } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

const stats = ref({
  temples: 0,
  devotees: 0,
  totalDonations: 0,
  events: 0
})

const recentTemples = ref([])
const upcomingEvents = ref([])

const formatDate = (date) => {
  return dayjs(date).format('MMM DD, YYYY')
}

const fetchDashboardData = async () => {
  try {
    // Fetch temples
    const templesResponse = await axios.get('/api/temples')
    stats.value.temples = templesResponse.data.length
    recentTemples.value = templesResponse.data.slice(0, 5)

    // Fetch devotees
    const devoteesResponse = await axios.get('/api/devotees')
    stats.value.devotees = devoteesResponse.data.length

    // Fetch donations total
    const donationsResponse = await axios.get('/api/donations')
    stats.value.totalDonations = donationsResponse.data.reduce((sum, donation) => {
      return sum + (donation.status === 'Completed' ? donation.amount : 0)
    }, 0)

    // Fetch upcoming events
    const eventsResponse = await axios.get('/api/events')
    const upcoming = eventsResponse.data.filter(event => 
      dayjs(event.startDate).isAfter(dayjs()) && event.status === 'Scheduled'
    )
    stats.value.events = upcoming.length
    upcomingEvents.value = upcoming.slice(0, 5)
  } catch (error) {
    console.error('Error fetching dashboard data:', error)
  }
}

onMounted(() => {
  fetchDashboardData()
})
</script>

<style scoped>
.dashboard {
  max-width: 1200px;
  margin: 0 auto;
}

.dashboard-header {
  margin-bottom: 30px;
  text-align: center;
}

.dashboard-header h2 {
  color: #303133;
  margin-bottom: 10px;
}

.dashboard-header p {
  color: #606266;
  font-size: 16px;
}

.stats-cards {
  margin-bottom: 30px;
}

.stat-card {
  height: 120px;
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 20px;
}

.stat-icon {
  font-size: 40px;
  padding: 15px;
  border-radius: 10px;
}

.stat-icon.temples {
  background-color: #e3f2fd;
  color: #1976d2;
}

.stat-icon.devotees {
  background-color: #f3e5f5;
  color: #7b1fa2;
}

.stat-icon.donations {
  background-color: #e8f5e8;
  color: #388e3c;
}

.stat-icon.events {
  background-color: #fff3e0;
  color: #f57c00;
}

.stat-info h3 {
  margin: 0 0 5px 0;
  font-size: 28px;
  color: #303133;
}

.stat-info p {
  margin: 0;
  color: #606266;
  font-size: 14px;
}

.dashboard-content {
  margin-bottom: 30px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.temple-item, .event-item {
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
}

.temple-item:last-child, .event-item:last-child {
  border-bottom: none;
}

.temple-item h4, .event-item h4 {
  margin: 0 0 5px 0;
  color: #303133;
}

.temple-item p, .event-item p {
  margin: 0;
  color: #606266;
  font-size: 14px;
}
</style>
