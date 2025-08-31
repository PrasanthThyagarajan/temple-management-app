<template>
  <div class="events-container">
    <el-card class="events-card">
      <template #header>
        <div class="card-header">
          <h2>Event Management</h2>
          <el-button type="primary" @click="showCreateDialog = true">
            <el-icon><Plus /></el-icon>
            Add Event
          </el-button>
        </div>
      </template>

      <!-- Search and Filters -->
      <div class="search-filters">
        <el-row :gutter="20">
          <el-col :span="6">
            <el-input
              v-model="searchTerm"
              placeholder="Search events..."
              clearable
              @input="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </el-col>
          <el-col :span="4">
            <el-select
              v-model="templeFilter"
              placeholder="Filter by Temple"
              clearable
              @change="handleTempleFilter"
            >
              <el-option
                v-for="temple in temples"
                :key="temple.id"
                :label="temple.name"
                :value="temple.id"
              />
            </el-select>
          </el-col>
          <el-col :span="4">
            <el-select
              v-model="statusFilter"
              placeholder="Filter by Status"
              clearable
              @change="handleStatusFilter"
            >
              <el-option label="Upcoming" value="Upcoming" />
              <el-option label="Ongoing" value="Ongoing" />
              <el-option label="Completed" value="Completed" />
              <el-option label="Cancelled" value="Cancelled" />
            </el-select>
          </el-col>
          <el-col :span="4">
            <el-date-picker
              v-model="dateFilter"
              type="date"
              placeholder="Filter by Date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              @change="handleDateFilter"
            />
          </el-col>
          <el-col :span="6">
            <el-button @click="loadEvents" :loading="loading">
              <el-icon><Refresh /></el-icon>
              Refresh
            </el-button>
            <el-button type="success" @click="showCalendarView = !showCalendarView">
              <el-icon><Calendar /></el-icon>
              {{ showCalendarView ? 'List View' : 'Calendar View' }}
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- Summary Cards -->
      <div class="summary-cards">
        <el-row :gutter="20">
          <el-col :span="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon total">
                  <el-icon><Calendar /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ totalEvents }}</div>
                  <div class="summary-label">Total Events</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon upcoming">
                  <el-icon><Clock /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ upcomingEvents }}</div>
                  <div class="summary-label">Upcoming</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon ongoing">
                  <el-icon><VideoPlay /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ ongoingEvents }}</div>
                  <div class="summary-label">Ongoing</div>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="6">
            <el-card class="summary-card">
              <div class="summary-content">
                <div class="summary-icon completed">
                  <el-icon><Check /></el-icon>
                </div>
                <div class="summary-text">
                  <div class="summary-value">{{ completedEvents }}</div>
                  <div class="summary-label">Completed</div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- Calendar View -->
      <div v-if="showCalendarView" class="calendar-view">
        <el-calendar v-model="currentDate">
          <template #dateCell="{ data }">
            <div class="calendar-cell">
              <span>{{ data.day.split('-').slice(2).join('-') }}</span>
              <div class="event-indicators">
                <div
                  v-for="event in getEventsForDate(data.day)"
                  :key="event.id"
                  class="event-indicator"
                  :class="getEventStatusClass(event.status)"
                  @click="viewEvent(event)"
                >
                  {{ event.name }}
                </div>
              </div>
            </div>
          </template>
        </el-calendar>
      </div>

      <!-- Events Table -->
      <div v-else>
        <el-table
          :data="events"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
        >
          <el-table-column prop="name" label="Event Name" min-width="200" />
          <el-table-column prop="templeName" label="Temple" min-width="150" />
          <el-table-column prop="startDate" label="Start Date" width="120">
            <template #default="scope">
              {{ formatDate(scope.row.startDate) }}
            </template>
          </el-table-column>
          <el-table-column prop="endDate" label="End Date" width="120">
            <template #default="scope">
              {{ formatDate(scope.row.endDate) }}
            </template>
          </el-table-column>
          <el-table-column prop="startTime" label="Start Time" width="100" />
          <el-table-column prop="endTime" label="End Time" width="100" />
          <el-table-column prop="status" label="Status" width="100">
            <template #default="scope">
              <el-tag :type="getStatusTagType(scope.row.status)">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="type" label="Type" width="120">
            <template #default="scope">
              <el-tag :type="getTypeTagType(scope.row.type)">
                {{ scope.row.type }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="250" fixed="right">
            <template #default="scope">
              <el-button size="small" @click.stop="editEvent(scope.row)">
                <el-icon><Edit /></el-icon>
                Edit
              </el-button>
              <el-button
                size="small"
                type="success"
                @click.stop="updateEventStatus(scope.row.id, 'Completed')"
                v-if="scope.row.status === 'Ongoing'"
              >
                <el-icon><Check /></el-icon>
                Complete
              </el-button>
              <el-button
                size="small"
                type="warning"
                @click.stop="updateEventStatus(scope.row.id, 'Cancelled')"
                v-if="scope.row.status === 'Upcoming'"
              >
                <el-icon><Close /></el-icon>
                Cancel
              </el-button>
              <el-button
                size="small"
                type="danger"
                @click.stop="deleteEvent(scope.row.id)"
              >
                <el-icon><Delete /></el-icon>
                Delete
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Pagination -->
        <div class="pagination-container">
          <el-pagination
            :current-page="currentPage"
            :page-size="pageSize"
            :page-sizes="[10, 20, 50, 100]"
            :total="totalEvents"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
          />
        </div>
      </div>
    </el-card>

    <!-- Create/Edit Event Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingEvent ? 'Edit Event' : 'Add New Event'"
      width="800px"
    >
      <el-form
        ref="eventFormRef"
        :model="eventForm"
        :rules="eventRules"
        label-width="120px"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Event Name" prop="name">
              <el-input v-model="eventForm.name" placeholder="Enter event name" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Temple" prop="templeId">
              <el-select
                v-model="eventForm.templeId"
                placeholder="Select temple"
                style="width: 100%"
              >
                <el-option
                  v-for="temple in temples"
                  :key="temple.id"
                  :label="temple.name"
                  :value="temple.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Event Type" prop="type">
              <el-select v-model="eventForm.type" placeholder="Select event type">
                <el-option label="Puja" value="Puja" />
                <el-option label="Festival" value="Festival" />
                <el-option label="Ceremony" value="Ceremony" />
                <el-option label="Workshop" value="Workshop" />
                <el-option label="Other" value="Other" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Status" prop="status">
              <el-select v-model="eventForm.status" placeholder="Select status">
                <el-option label="Upcoming" value="Upcoming" />
                <el-option label="Ongoing" value="Ongoing" />
                <el-option label="Completed" value="Completed" />
                <el-option label="Cancelled" value="Cancelled" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Start Date" prop="startDate">
              <el-date-picker
                v-model="eventForm.startDate"
                type="date"
                placeholder="Select start date"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="End Date" prop="endDate">
              <el-date-picker
                v-model="eventForm.endDate"
                type="date"
                placeholder="Select end date"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Start Time" prop="startTime">
              <el-time-picker
                v-model="eventForm.startTime"
                placeholder="Select start time"
                format="HH:mm"
                value-format="HH:mm"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="End Time" prop="endTime">
              <el-time-picker
                v-model="eventForm.endTime"
                placeholder="Select end time"
                format="HH:mm"
                value-format="HH:mm"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="eventForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter event description"
          />
        </el-form-item>
        <el-form-item label="Location" prop="location">
          <el-input v-model="eventForm.location" placeholder="Enter event location" />
        </el-form-item>
        <el-form-item label="Organizer" prop="organizer">
          <el-input v-model="eventForm.organizer" placeholder="Enter organizer name" />
        </el-form-item>
        <el-form-item label="Contact" prop="contact">
          <el-input v-model="eventForm.contact" placeholder="Enter contact information" />
        </el-form-item>
        <el-form-item label="Notes" prop="notes">
          <el-input
            v-model="eventForm.notes"
            type="textarea"
            :rows="3"
            placeholder="Enter any additional notes"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">Cancel</el-button>
          <el-button type="primary" @click="saveEvent" :loading="saving">
            {{ editingEvent ? 'Update' : 'Create' }}
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Event Details Dialog -->
    <el-dialog
      v-model="showDetailsDialog"
      title="Event Details"
      width="700px"
    >
      <div v-if="selectedEvent" class="event-details">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="Event Name">
            {{ selectedEvent.name }}
          </el-descriptions-item>
          <el-descriptions-item label="Status">
            <el-tag :type="getStatusTagType(selectedEvent.status)">
              {{ selectedEvent.status }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Temple">{{ selectedEvent.templeName }}</el-descriptions-item>
          <el-descriptions-item label="Type">
            <el-tag :type="getTypeTagType(selectedEvent.type)">
              {{ selectedEvent.type }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Start Date">
            {{ formatDate(selectedEvent.startDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="End Date">
            {{ formatDate(selectedEvent.endDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="Start Time">{{ selectedEvent.startTime }}</el-descriptions-item>
          <el-descriptions-item label="End Time">{{ selectedEvent.endTime }}</el-descriptions-item>
          <el-descriptions-item label="Location">{{ selectedEvent.location }}</el-descriptions-item>
          <el-descriptions-item label="Organizer">{{ selectedEvent.organizer }}</el-descriptions-item>
          <el-descriptions-item label="Contact">{{ selectedEvent.contact }}</el-descriptions-item>
          <el-descriptions-item label="Description" :span="2">
            {{ selectedEvent.description }}
          </el-descriptions-item>
          <el-descriptions-item label="Notes" :span="2">
            {{ selectedEvent.notes || 'No notes available' }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Calendar, Clock, VideoPlay, Check, Close } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const events = ref([])
const temples = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const templeFilter = ref('')
const statusFilter = ref('')
const dateFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const totalEvents = ref(0)
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const showCalendarView = ref(false)
const editingEvent = ref(null)
const selectedEvent = ref(null)
const currentDate = ref(new Date())

// Form data
const eventForm = reactive({
  name: '',
  templeId: '',
  type: '',
  status: 'Upcoming',
  startDate: '',
  endDate: '',
  startTime: '',
  endTime: '',
  description: '',
  location: '',
  organizer: '',
  contact: '',
  notes: ''
})

// Form validation rules
const eventRules = {
  name: [{ required: true, message: 'Event name is required', trigger: 'blur' }],
  templeId: [{ required: true, message: 'Temple selection is required', trigger: 'change' }],
  type: [{ required: true, message: 'Event type is required', trigger: 'change' }],
  status: [{ required: true, message: 'Status is required', trigger: 'change' }],
  startDate: [{ required: true, message: 'Start date is required', trigger: 'change' }],
  endDate: [{ required: true, message: 'End date is required', trigger: 'change' }],
  startTime: [{ required: true, message: 'Start time is required', trigger: 'change' }],
  endTime: [{ required: true, message: 'End time is required', trigger: 'change' }],
  description: [{ required: true, message: 'Description is required', trigger: 'blur' }]
}

const eventFormRef = ref()

// API base URL
const API_BASE = 'http://localhost:5000/api'

// Computed properties
const upcomingEvents = computed(() => {
  return events.value.filter(event => event.status === 'Upcoming').length
})

const ongoingEvents = computed(() => {
  return events.value.filter(event => event.status === 'Ongoing').length
})

const completedEvents = computed(() => {
  return events.value.filter(event => event.status === 'Completed').length
})

// Methods
const loadEvents = async () => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/events`)
    events.value = response.data
    totalEvents.value = response.data.length
  } catch (error) {
    console.error('Error loading events:', error)
    ElMessage.error('Failed to load events')
  } finally {
    loading.value = false
  }
}

const loadTemples = async () => {
  try {
    const response = await axios.get(`${API_BASE}/temples`)
    temples.value = response.data
  } catch (error) {
    console.error('Error loading temples:', error)
  }
}

const handleSearch = () => {
  if (searchTerm.value.trim()) {
    searchEvents(searchTerm.value)
  } else {
    loadEvents()
  }
}

const searchEvents = async (term) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/events/search/${term}`)
    events.value = response.data
    totalEvents.value = response.data.length
  } catch (error) {
    console.error('Error searching events:', error)
    ElMessage.error('Failed to search events')
  } finally {
    loading.value = false
  }
}

const handleTempleFilter = () => {
  if (templeFilter.value) {
    filterByTemple(templeFilter.value)
  } else {
    loadEvents()
  }
}

const filterByTemple = async (templeId) => {
  try {
    loading.value = true
    const response = await axios.get(`${API_BASE}/temples/${templeId}/events`)
    events.value = response.data
    totalEvents.value = response.data.length
  } catch (error) {
    console.error('Error filtering by temple:', error)
    ElMessage.error('Failed to filter events by temple')
  } finally {
    loading.value = false
  }
}

const handleStatusFilter = () => {
  if (statusFilter.value) {
    events.value = events.value.filter(event => event.status === statusFilter.value)
    totalEvents.value = events.value.length
  } else {
    loadEvents()
  }
}

const handleDateFilter = () => {
  if (dateFilter.value) {
    const filterDate = dayjs(dateFilter.value)
    events.value = events.value.filter(event => {
      const eventDate = dayjs(event.startDate)
      return eventDate.isSame(filterDate, 'day')
    })
    totalEvents.value = events.value.length
  } else {
    loadEvents()
  }
}

const editEvent = (event) => {
  editingEvent.value = event
  Object.assign(eventForm, event)
  showCreateDialog.value = true
}

const saveEvent = async () => {
  try {
    await eventFormRef.value.validate()
    saving.value = true
    
    if (editingEvent.value) {
      // Update existing event
      await axios.put(`${API_BASE}/events/${editingEvent.value.id}`, eventForm)
      ElMessage.success('Event updated successfully')
    } else {
      // Create new event
      await axios.post(`${API_BASE}/events`, eventForm)
      ElMessage.success('Event created successfully')
    }
    
    showCreateDialog.value = false
    resetForm()
    loadEvents()
  } catch (error) {
    if (error.response?.data) {
      ElMessage.error(error.response.data)
    } else {
      ElMessage.error('Failed to save event')
    }
  } finally {
    saving.value = false
  }
}

const updateEventStatus = async (id, status) => {
  try {
    await axios.put(`${API_BASE}/events/${id}/status`, null, {
      params: { status }
    })
    ElMessage.success('Event status updated successfully')
    loadEvents()
  } catch (error) {
    console.error('Error updating event status:', error)
    ElMessage.error('Failed to update event status')
  }
}

const deleteEvent = async (id) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this event?',
      'Warning',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    await axios.delete(`${API_BASE}/events/${id}`)
    ElMessage.success('Event deleted successfully')
    loadEvents()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting event:', error)
      ElMessage.error('Failed to delete event')
    }
  }
}

const handleRowClick = (row) => {
  selectedEvent.value = row
  showDetailsDialog.value = true
}

const viewEvent = (event) => {
  selectedEvent.value = event
  showDetailsDialog.value = true
}

const resetForm = () => {
  editingEvent.value = null
  Object.assign(eventForm, {
    name: '',
    templeId: '',
    type: '',
    status: 'Upcoming',
    startDate: '',
    endDate: '',
    startTime: '',
    endTime: '',
    description: '',
    location: '',
    organizer: '',
    contact: '',
    notes: ''
  })
  eventFormRef.value?.resetFields()
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY')
}

const getStatusTagType = (status) => {
  const statusMap = {
    'Upcoming': 'primary',
    'Ongoing': 'success',
    'Completed': 'info',
    'Cancelled': 'danger'
  }
  return statusMap[status] || 'info'
}

const getTypeTagType = (type) => {
  const typeMap = {
    'Puja': 'success',
    'Festival': 'warning',
    'Ceremony': 'primary',
    'Workshop': 'info',
    'Other': 'default'
  }
  return typeMap[type] || 'default'
}

const getEventStatusClass = (status) => {
  const classMap = {
    'Upcoming': 'upcoming',
    'Ongoing': 'ongoing',
    'Completed': 'completed',
    'Cancelled': 'cancelled'
  }
  return classMap[status] || 'default'
}

const getEventsForDate = (date) => {
  return events.value.filter(event => {
    const eventDate = dayjs(event.startDate)
    const cellDate = dayjs(date)
    return eventDate.isSame(cellDate, 'day')
  })
}

const handleSizeChange = (val) => {
  pageSize.value = val
  loadEvents()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadEvents()
}

// Lifecycle
onMounted(() => {
  loadEvents()
  loadTemples()
})
</script>

<style scoped>
.events-container {
  padding: 20px;
}

.events-card {
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

.summary-icon.upcoming {
  background-color: #e6a23c;
}

.summary-icon.ongoing {
  background-color: #67c23a;
}

.summary-icon.completed {
  background-color: #909399;
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

.calendar-view {
  margin-bottom: 20px;
}

.calendar-cell {
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 4px;
}

.event-indicators {
  margin-top: 4px;
}

.event-indicator {
  font-size: 10px;
  padding: 2px 4px;
  margin: 1px 0;
  border-radius: 2px;
  cursor: pointer;
  color: white;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.event-indicator.upcoming {
  background-color: #e6a23c;
}

.event-indicator.ongoing {
  background-color: #67c23a;
}

.event-indicator.completed {
  background-color: #909399;
}

.event-indicator.cancelled {
  background-color: #f56c6c;
}

.event-indicator.default {
  background-color: #409eff;
}

.pagination-container {
  margin-top: 20px;
  text-align: right;
}

.event-details {
  padding: 20px 0;
}

.dialog-footer {
  text-align: right;
}
</style>
