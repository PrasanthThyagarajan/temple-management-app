<template>
  <div class="dashboard">
    <el-row :gutter="20" class="dashboard-header">
      <el-col :span="24">
        <el-carousel
          :interval="5000"
          arrow="hover"
          indicator-position="outside"
          :height="carouselHeight"
          class="dashboard-carousel"
        >
          <el-carousel-item v-for="(slide, index) in slides" :key="index">
            <div class="carousel-slide" :style="{ backgroundImage: `url(${slide.image})` }">
              <div class="slide-overlay">
                <h1>{{ slide.title }}</h1>
                <h3 v-if="slide.subtitle">{{ slide.subtitle }}</h3>
              </div>
            </div>
          </el-carousel-item>
        </el-carousel>
      </el-col>
    </el-row>


    <el-row :gutter="20" class="dashboard-content">
      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card class="temple-info">
          <template #header>
            <div class="card-header">
              <span>Temple Information</span>
            </div>
          </template>
          <div class="temple-details">
            <h3>Pallipuram Pariyathara Shri Mutharamman Temple</h3>
            <p><strong>Location:</strong> Pallipuram, Kerala, India</p>
            <p><strong>Deity:</strong> Shri Mutharamman (Divine Mother)</p>
            <p><strong>Established:</strong> Ancient Temple</p>
            <p><strong>Speciality:</strong> A Sacred Abode of Divine Grace</p>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card class="upcoming-events">
          <template #header>
            <div class="card-header">
              <span>Upcoming Events</span>
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

    <el-row :gutter="20" class="dashboard-content">
      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card class="recent-products">
          <template #header>
            <div class="card-header">
              <span>Recent Products</span>
            </div>
          </template>
          <div v-if="recentProducts.length > 0">
            <div v-for="product in recentProducts" :key="product.id" class="product-item">
              <h4>{{ product.name }}</h4>
              <p>{{ product.category }} - ₹{{ product.price.toLocaleString() }}</p>
            </div>
          </div>
          <el-empty v-else description="No products found" />
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card class="recent-sales">
          <template #header>
            <div class="card-header">
              <span>Recent Sales</span>
            </div>
          </template>
          <div v-if="recentSales.length > 0">
            <div v-for="sale in recentSales" :key="sale.id" class="sale-item">
              <h4>{{ sale.customerName }}</h4>
              <p>{{ formatDate(sale.saleDate) }} - ₹{{ sale.finalAmount.toLocaleString() }}</p>
            </div>
          </div>
          <el-empty v-else description="No recent sales" />
        </el-card>
      </el-col>
    </el-row>

    <!-- Astrology Section -->
    <el-row :gutter="20" class="dashboard-content">
      <el-col :span="24">
        <div class="astrology-section">
          <!-- Astrology Header -->
          <div class="astrology-header">
            <div class="astrology-banner">
              <img src="/images/AstrologyHeader.jpg" alt="Astrology & Panchang" class="astrology-header-image" />
              <div class="astrology-overlay">
                <div class="astrology-content">
                  <h2 class="astrology-title">
                    <el-icon class="astrology-icon"><Star /></el-icon>
                    Astrology & Panchang
                  </h2>
                  <p class="astrology-subtitle">Discover your cosmic destiny with ancient wisdom</p>
                  <div class="astrology-features">
                    <span class="feature-tag">Daily Panchang</span>
                    <span class="feature-tag">Horoscope Predictions</span>
                    <span class="feature-tag">Auspicious Timings</span>
                    <span class="feature-tag">Planetary Positions</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Astrology Content -->
          <el-card class="astrology-card">
            <AstrologyModal />
          </el-card>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import axios from 'axios'
import dayjs from 'dayjs'
import { Star } from '@element-plus/icons-vue'
import AstrologyModal from '../components/AstrologyModal.vue'

const slides = ref([
  {
    title: 'Pallipuram Pariyathara Shri Mutharamman Temple',
    subtitle: 'A Sacred Abode of Divine Grace',
    image: '/images/image1.png'
  },
  {
    title: 'Shakti Peetham',
    subtitle: 'Where Devotion Meets Eternal Power',
    image: '/images/image2.jpg'
  },
  {
    title: 'Divine Mother Blessings',
    subtitle: 'Experience Peace and Prosperity',
    image: '/images/image3.jpg'
  },
  {
    title: 'Sri Mutharamman Temple',
    subtitle: 'A Place of Faith, Tradition, and Culture',
    image: '/images/image4.jpg'
  },
  {
    title: 'The Eternal Flame of Shakti',
    subtitle: 'Illuminating Hearts with Devotion',
    image: '/images/image5.jpg'
  },
  {
    title: 'Grace of Devi',
    subtitle: 'Protector, Mother, and Divine Energy',
    image: '/images/image6.jpg'
  },
  {
    title: 'Sacred Sanctum of Devi',
    subtitle: 'A Journey Towards Inner Peace',
    image: '/images/image7.jpg'
  },
  {
    title: 'Sri Devi Arul',
    subtitle: 'Where Prayers Blossom into Blessings',
    image: '/images/image8.jpg'
  },
  {
    title: 'Temple of Divine Energy',
    subtitle: 'Connecting Souls with the Mother Goddess',
    image: '/images/image9.jpg'
  }
])

const carouselHeight = ref('400px')

const updateCarouselHeight = () => {
  const width = window.innerWidth
  if (width < 480) {
    carouselHeight.value = '250px'
  } else if (width < 768) {
    carouselHeight.value = '300px'
  } else if (width < 1024) {
    carouselHeight.value = '350px'
  } else {
    carouselHeight.value = '400px'
  }
}


const upcomingEvents = ref([])
const recentProducts = ref([])
const recentSales = ref([])

const formatDate = (date) => {
  return dayjs(date).format('MMM DD, YYYY')
}

const fetchDashboardData = async () => {
  try {
    // Fetch upcoming events
    const eventsResponse = await axios.get('/api/events')
    const upcoming = eventsResponse.data.filter(event => 
      dayjs(event.startDate).isAfter(dayjs()) && event.status === 'Scheduled'
    )
    upcomingEvents.value = upcoming.slice(0, 5)

    // Fetch products
    const productsResponse = await axios.get('/api/products')
    recentProducts.value = productsResponse.data.slice(0, 5)

    // Fetch sales
    const salesResponse = await axios.get('/api/sales')
    recentSales.value = salesResponse.data.slice(0, 5)
  } catch (error) {
    console.error('Error fetching dashboard data:', error)
  }
}

onMounted(() => {
  updateCarouselHeight()
  window.addEventListener('resize', updateCarouselHeight)
  fetchDashboardData()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', updateCarouselHeight)
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

.devotional-hero {
  padding: 40px 20px;
}

.hero-inner {
  max-width: 900px;
  margin: 0 auto;
}

/* Dashboard carousel */
.dashboard-carousel {
  margin-bottom: 10px;
}
.dashboard-carousel :deep(.el-carousel__container) {
  border-radius: 12px;
  overflow: hidden;
}
.carousel-slide {
  position: relative;
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
}
.slide-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: #fff;
  background: linear-gradient(180deg, rgba(0,0,0,0.25) 0%, rgba(0,0,0,0.45) 100%);
  padding: 8px 16px;
}
.slide-overlay h1 {
  margin: 0 0 6px 0;
  font-size: 28px;
}
.slide-overlay h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 500;
}


.dashboard-content {
  margin-bottom: 30px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.temple-details h3 {
  margin: 0 0 15px 0;
  color: #303133;
  font-size: 20px;
  font-weight: 600;
}

.temple-details p {
  margin: 8px 0;
  color: #606266;
  font-size: 14px;
  line-height: 1.5;
}

.temple-details strong {
  color: #303133;
  font-weight: 600;
}

.event-item, .product-item, .sale-item {
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
}

.event-item:last-child, .product-item:last-child, .sale-item:last-child {
  border-bottom: none;
}

.event-item h4, .product-item h4, .sale-item h4 {
  margin: 0 0 5px 0;
  color: #303133;
}

.event-item p, .product-item p, .sale-item p {
  margin: 0;
  color: #606266;
  font-size: 14px;
}

/* Responsive Design */
@media (max-width: 768px) {
  .dashboard {
    padding: 0 5px;
  }
  
  .dashboard-header h2 {
    font-size: 24px;
  }
  
  .dashboard-header p {
    font-size: 14px;
  }
  
  
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 5px;
  }
  
  .temple-details h3 {
    font-size: 18px;
  }
  
  .temple-details p {
    font-size: 13px;
  }
  
  .event-item, .product-item, .sale-item {
    padding: 12px 0;
  }
  
  .event-item h4, .product-item h4, .sale-item h4 {
    font-size: 14px;
  }
  
  .event-item p, .product-item p, .sale-item p {
    font-size: 12px;
  }
}

@media (max-width: 480px) {
  .dashboard {
    padding: 0;
  }
  
  .dashboard-header {
    margin-bottom: 20px;
  }
  
  .dashboard-header h2 {
    font-size: 20px;
  }
  
  .dashboard-header p {
    font-size: 12px;
  }
  
  
  .temple-details h3 {
    font-size: 16px;
  }
  
  .temple-details p {
    font-size: 12px;
  }
  
  .event-item, .product-item, .sale-item {
    padding: 10px 0;
  }
}


/* Astrology Section */
.astrology-section {
  margin-top: 20px;
}

.astrology-header {
  margin-bottom: 20px;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

.astrology-banner {
  position: relative;
  height: 300px;
  overflow: hidden;
}

.astrology-header-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.astrology-banner:hover .astrology-header-image {
  transform: scale(1.05);
}

.astrology-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(
    135deg,
    rgba(102, 126, 234, 0.85) 0%,
    rgba(118, 75, 162, 0.85) 50%,
    rgba(255, 107, 107, 0.8) 100%
  );
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(2px);
}

.astrology-content {
  text-align: center;
  color: white;
  padding: 20px;
  max-width: 800px;
}

.astrology-title {
  font-size: 2.5rem;
  font-weight: 700;
  margin: 0 0 15px 0;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 15px;
}

.astrology-icon {
  font-size: 2.8rem;
  color: #FFD700;
  filter: drop-shadow(2px 2px 4px rgba(0, 0, 0, 0.3));
  animation: twinkle 2s ease-in-out infinite alternate;
}

@keyframes twinkle {
  0% { transform: scale(1) rotate(0deg); }
  100% { transform: scale(1.1) rotate(5deg); }
}

.astrology-subtitle {
  font-size: 1.2rem;
  margin: 0 0 25px 0;
  opacity: 0.95;
  font-weight: 300;
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.3);
}

.astrology-features {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 12px;
  margin-top: 20px;
}

.feature-tag {
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  padding: 8px 16px;
  border-radius: 25px;
  font-size: 0.9rem;
  font-weight: 500;
  color: white;
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.2);
  transition: all 0.3s ease;
  cursor: pointer;
}

.feature-tag:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.astrology-card {
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  border: 1px solid rgba(102, 126, 234, 0.1);
}

.astrology-card :deep(.el-card__body) {
  padding: 0;
}

/* Quick Links */
.quick-links .links-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
}

@media (max-width: 768px) {
  .quick-links .links-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  /* Astrology Header Responsive */
  .astrology-banner {
    height: 250px;
  }
  
  .astrology-title {
    font-size: 2rem;
    flex-direction: column;
    gap: 10px;
  }
  
  .astrology-icon {
    font-size: 2.2rem;
  }
  
  .astrology-subtitle {
    font-size: 1rem;
  }
  
  .astrology-features {
    gap: 8px;
  }
  
  .feature-tag {
    padding: 6px 12px;
    font-size: 0.8rem;
  }
}

@media (max-width: 480px) {
  .astrology-banner {
    height: 200px;
  }
  
  .astrology-title {
    font-size: 1.6rem;
  }
  
  .astrology-icon {
    font-size: 1.8rem;
  }
  
  .astrology-subtitle {
    font-size: 0.9rem;
  }
  
  .astrology-content {
    padding: 15px;
  }
  
  .astrology-features {
    flex-direction: column;
    align-items: center;
  }
  
  .feature-tag {
    width: 100%;
    max-width: 200px;
    text-align: center;
  }
}
</style>
