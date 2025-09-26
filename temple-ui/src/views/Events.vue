<template>
  <div class="events-container">
    <!-- Enhanced Summary Cards -->
    <el-row :gutter="20" class="summary-cards">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card">
          <el-statistic title="Total Events" :value="summaryStats.total">
            <template #prefix>
              <el-icon style="vertical-align: middle;"><Calendar /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card upcoming">
          <el-statistic title="Upcoming Events" :value="summaryStats.upcoming">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #409eff;"><Clock /></el-icon>
            </template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="summary-card ongoing">
          <el-statistic title="Ongoing Events" :value="summaryStats.ongoing">
            <template #prefix>
              <el-icon style="vertical-align: middle; color: #67c23a;"><VideoPlay /></el-icon>
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

      <!-- Enhanced Search and Filters -->
      <div class="search-filters">
        <div class="devotional-banner events-banner"></div>
        <el-row :gutter="20">
          <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6">
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
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select
              v-model="areaFilter"
              placeholder="Filter by Area"
              clearable
              @change="handleFilterChange"
              style="width: 100%"
            >
              <el-option label="Any Area" value="" />
              <el-option
                v-for="area in areas"
                :key="area.id"
                :label="area.name"
                :value="area.id"
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
              <el-option label="Upcoming" value="Upcoming" />
              <el-option label="Ongoing" value="Ongoing" />
              <el-option label="Completed" value="Completed" />
              <el-option label="Cancelled" value="Cancelled" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-select
              v-model="sortBy"
              placeholder="Sort by"
              @change="handleSortChange"
              style="width: 100%"
            >
              <el-option label="Start Date (Newest)" value="startDate-desc" />
              <el-option label="Start Date (Oldest)" value="startDate-asc" />
              <el-option label="Name (A-Z)" value="name-asc" />
              <el-option label="Name (Z-A)" value="name-desc" />
              <el-option label="Recently Added" value="id-desc" />
            </el-select>
          </el-col>
          <el-col :xs="12" :sm="6" :md="4" :lg="4" :xl="4">
            <el-date-picker
              v-model="dateFilter"
              type="date"
              placeholder="Filter by Date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              @change="handleDateFilter"
              style="width: 100%"
            />
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
      <div v-else class="table-container">
        <el-table
          :data="paginatedEvents"
          v-loading="loading"
          stripe
          style="width: 100%"
          @row-click="handleRowClick"
          @sort-change="handleTableSortChange"
        >
          <el-table-column 
            prop="name" 
            label="Event Name" 
            min-width="200" 
            sortable="custom"
          />
          <el-table-column 
            label="Area" 
            min-width="150"
            prop="area.name"
            sortable="custom"
          >
            <template #default="scope">
              {{ scope.row.area?.name || '—' }}
            </template>
          </el-table-column>
          <el-table-column 
            prop="startDate" 
            label="Start Date" 
            width="120"
            sortable="custom"
          >
            <template #default="scope">
              {{ formatDate(scope.row.startDate) }}
            </template>
          </el-table-column>
          <el-table-column prop="endDate" label="End Date" width="120">
            <template #default="scope">
              {{ formatDate(scope.row.endDate) }}
            </template>
          </el-table-column>
          
          <el-table-column prop="status" label="Status" width="100">
            <template #default="scope">
              <el-tag :type="getStatusTagType(scope.row.status)">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Type" width="160">
            <template #default="scope">
              <el-tag :type="getTypeTagType(scope.row.eventType?.name)">
                {{ scope.row.eventType?.name || '—' }}
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
            <el-form-item label="Area" prop="areaId">
              <el-select
                v-model="eventForm.areaId"
                placeholder="Select area"
                style="width: 100%"
              >
                <el-option
                  v-for="area in areas"
                  :key="area.id"
                  :label="area.name"
                  :value="area.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Event Type" prop="eventTypeId">
              <el-select v-model="eventForm.eventTypeId" placeholder="Select event type">
                <el-option
                  v-for="t in eventTypes"
                  :key="t.id"
                  :label="t.name"
                  :value="t.id"
                />
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
            <el-form-item label="Start" prop="startDate">
              <el-date-picker
                v-model="eventForm.startDate"
                type="datetime"
                placeholder="Select start date & time"
                format="YYYY-MM-DD HH:mm"
                value-format="YYYY-MM-DDTHH:mm:ss"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="End" prop="endDate">
              <el-date-picker
                v-model="eventForm.endDate"
                type="datetime"
                placeholder="Select end date & time"
                format="YYYY-MM-DD HH:mm"
                value-format="YYYY-MM-DDTHH:mm:ss"
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
        <el-form-item label="Approval Needed" prop="isApprovalNeeded">
          <el-checkbox v-model="eventForm.isApprovalNeeded">Is Approval Needed</el-checkbox>
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
          <el-descriptions-item label="Area">{{ selectedEvent.area?.name }}</el-descriptions-item>
          <el-descriptions-item label="Type">
            <el-tag :type="getTypeTagType(selectedEvent.eventType?.name)">
              {{ selectedEvent.eventType?.name }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="Start Date">
            {{ formatDate(selectedEvent.startDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="End Date">
            {{ formatDate(selectedEvent.endDate) }}
          </el-descriptions-item>
          <el-descriptions-item label="Start">{{ formatDateTime(selectedEvent.startDate) }}</el-descriptions-item>
          <el-descriptions-item label="End">{{ formatDateTime(selectedEvent.endDate) }}</el-descriptions-item>
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
import { Plus, Search, Refresh, Edit, Delete, Calendar, Clock, VideoPlay, Check, Close, Filter } from '@element-plus/icons-vue'
import axios from 'axios'
import dayjs from 'dayjs'

// Reactive data
const events = ref([])
const areas = ref([])
const eventTypes = ref([])
const loading = ref(false)
const saving = ref(false)
const searchTerm = ref('')
const areaFilter = ref('')
const statusFilter = ref('')
const dateFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const sortBy = ref('startDate-desc')
const showCreateDialog = ref(false)
const showDetailsDialog = ref(false)
const showCalendarView = ref(false)
const editingEvent = ref(null)
const selectedEvent = ref(null)
const currentDate = ref(new Date())

// Form data
const eventForm = reactive({
  name: '',
  areaId: '',
  eventTypeId: '',
  status: '',
  startDate: '',
  endDate: '',
  description: '',
  location: '',
  organizer: '',
  contact: '',
  notes: '',
  isApprovalNeeded: false
})

// Form validation rules
const eventRules = {
  name: [{ required: true, message: 'Event name is required', trigger: 'blur' }],
  areaId: [{ required: true, message: 'Area is required', trigger: 'change' }],
  eventTypeId: [{ required: true, message: 'Event type is required', trigger: 'change' }],
  status: [{ required: true, message: 'Status is required', trigger: 'change' }],
  startDate: [{ required: true, message: 'Start date is required', trigger: 'change' }],
  endDate: [{ required: true, message: 'End date is required', trigger: 'change' }],
  description: [{ required: true, message: 'Description is required', trigger: 'blur' }]
}

const eventFormRef = ref()

// API base URL (use Vite proxy)
const API_BASE = '/api'

// Summary Statistics
const summaryStats = computed(() => {
  return {
    total: events.value.length,
    upcoming: events.value.filter(e => e.status === 'Upcoming').length,
    ongoing: events.value.filter(e => e.status === 'Ongoing').length,
    completed: events.value.filter(e => e.status === 'Completed').length
  }
})

// Filtering and Sorting
const filteredBeforePagination = computed(() => {
  let result = [...events.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(e =>
      e.name?.toLowerCase().includes(term) ||
      e.description?.toLowerCase().includes(term) ||
      e.location?.toLowerCase().includes(term) ||
      e.area?.name?.toLowerCase().includes(term)
    )
  }
  
  // Apply area filter
  if (areaFilter.value) {
    result = result.filter(e => e.areaId === parseInt(areaFilter.value))
  }
  
  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(e => e.status === statusFilter.value)
  }
  
  // Apply date filter
  if (dateFilter.value) {
    const filterDate = dayjs(dateFilter.value)
    result = result.filter(e => {
      const startDate = dayjs(e.startDate)
      const endDate = dayjs(e.endDate)
      return filterDate.isBetween(startDate, endDate, 'day', '[]')
    })
  }
  
  // Apply sorting
  if (sortBy.value) {
    const [field, order] = sortBy.value.split('-')
    result.sort((a, b) => {
      let aVal, bVal
      
      switch(field) {
        case 'startDate':
          aVal = new Date(a.startDate).getTime()
          bVal = new Date(b.startDate).getTime()
          break
        case 'name':
          aVal = (a.name || '').toLowerCase()
          bVal = (b.name || '').toLowerCase()
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
const paginatedEvents = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredBeforePagination.value.slice(start, start + pageSize.value)
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

const loadAreas = async () => {
  try {
    console.log('🔍 Events: Loading areas...')
    const response = await axios.get(`${API_BASE}/areas`)
    console.log('🔍 Events: Areas response:', response.data)
    areas.value = response.data
    console.log('🔍 Events: Areas loaded:', areas.value.length)
  } catch (error) {
    console.error('🚨 Events: Error loading areas:', error)
    ElMessage.error('Failed to load areas')
  }
}

const loadEventTypes = async () => {
  try {
    console.log('🔍 Events: Loading event types...')
    const response = await axios.get(`${API_BASE}/event-types`)
    console.log('🔍 Events: Event types response:', response.data)
    eventTypes.value = response.data
    console.log('🔍 Events: Event types loaded:', eventTypes.value.length)
  } catch (error) {
    console.error('🚨 Events: Error loading event types:', error)
    ElMessage.error('Failed to load event types')
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
    sortBy.value = 'startDate-desc'
  } else {
    let field = prop
    if (prop === 'startDate') field = 'startDate'
    else if (prop === 'name') field = 'name'
    else if (prop === 'area.name') field = 'area'
    
    sortBy.value = `${field}-${order === 'ascending' ? 'asc' : 'desc'}`
  }
}

const handleDateFilter = () => {
  currentPage.value = 1
}

const editEvent = (event) => {
  editingEvent.value = event
  Object.assign(eventForm, {
    name: event.name,
    areaId: event.area?.id || '',
    eventTypeId: event.eventType?.id || '',
    status: event.status || '',
    startDate: event.startDate,
    endDate: event.endDate,
    description: event.description,
    location: event.location,
    organizer: event.organizer,
    contact: event.contact,
    notes: event.notes,
    isApprovalNeeded: event.isApprovalNeeded
  })
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
    areaId: '',
    eventTypeId: '',
    status: '',
    startDate: '',
    endDate: '',
    description: '',
    location: '',
    organizer: '',
    contact: '',
    notes: '',
    isApprovalNeeded: false
  })
  eventFormRef.value?.resetFields()
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY')
}

const formatDateTime = (dateString) => {
  if (!dateString) return 'N/A'
  return dayjs(dateString).format('MMM DD, YYYY HH:mm')
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
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

// Lifecycle
onMounted(async () => {
  await loadAreas()
  await loadEventTypes()
  await loadEvents()
})
</script>

<style scoped>
.events-container {
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

.summary-card.upcoming {
  border-left: 4px solid #409eff;
}

.summary-card.ongoing {
  border-left: 4px solid #67c23a;
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

.table-container {
  width: 100%;
  overflow-x: auto;
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

  .el-button .el-icon + span,
  .el-button .el-icon + .btn-text {
    margin-left: 6px;
  }
}

@media (max-width: 480px) {
  .summary-content {
    gap: 10px;
  }

  .el-button span:not(.el-icon) {
    display: none;
  }
}
</style>
