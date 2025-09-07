<template>
  <div class="home">
    <!-- Hero Section -->
    <section class="home-hero">
      <div class="hero-overlay">
        <h1>Welcome to Devakaryam</h1>
        <p>Temple information, events, and services for devotees and visitors</p>
        <div class="hero-actions">
          <el-button type="primary" size="large" @click="$router.push('/temples')">
            <el-icon><OfficeBuilding /></el-icon>
            <span>Explore Temples</span>
          </el-button>
          <el-button size="large" @click="$router.push('/dashboard')">
            <el-icon><DataAnalysis /></el-icon>
            <span>Go to Dashboard</span>
          </el-button>
        </div>
      </div>
    </section>

    <!-- Info Grid -->
    <section class="home-section">
      <el-row :gutter="20">
        <el-col :xs="24" :sm="24" :md="8" :lg="8" :xl="8">
          <el-card shadow="hover" class="info-card">
            <template #header>
              <div class="card-header">
                <el-icon><Timer /></el-icon>
                <span>Darshan Timings</span>
              </div>
            </template>
            <ul class="list">
              <li><strong>Morning:</strong> 5:30 AM – 11:30 AM</li>
              <li><strong>Evening:</strong> 5:00 PM – 8:30 PM</li>
              <li><strong>Special Poojas:</strong> Fridays & Festival Days</li>
            </ul>
          </el-card>
        </el-col>

        <el-col :xs="24" :sm="24" :md="8" :lg="8" :xl="8">
          <el-card shadow="hover" class="info-card">
            <template #header>
              <div class="card-header">
                <el-icon><Calendar /></el-icon>
                <span>Festivals & Highlights</span>
              </div>
            </template>
            <ul class="list">
              <li>Navaratri – 9 days of devotion and music</li>
              <li>Thai Poosam – Kavadi & special abhishekam</li>
              <li>Monthly Pournami – Devi Alankaram</li>
            </ul>
          </el-card>
        </el-col>

        <el-col :xs="24" :sm="24" :md="8" :lg="8" :xl="8">
          <el-card shadow="hover" class="info-card">
            <template #header>
              <div class="card-header">
                <el-icon><Collection /></el-icon>
                <span>Services for Devotees</span>
              </div>
            </template>
            <div class="services">
              <el-button text @click="$router.push('/events')">View Events</el-button>
              <el-button text @click="$router.push('/donations')">Make a Donation</el-button>
              <el-button text @click="$router.push('/products')">Temple Store</el-button>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </section>

    <!-- Gallery Banner -->
    <section class="home-banner">
      <div class="banner-inner">
        <el-carousel :interval="4500" type="card" height="220px">
          <el-carousel-item v-for="(img, idx) in gallery" :key="idx">
            <div class="banner-slide" :style="{ backgroundImage: `url(${img})` }"></div>
          </el-carousel-item>
        </el-carousel>
      </div>
    </section>

    <!-- Visit & Contact -->
    <section class="home-section">
      <el-row :gutter="20">
        <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
          <el-card shadow="hover" class="visit-card">
            <template #header>
              <div class="card-header">
                <el-icon><Location /></el-icon>
                <span>Plan Your Visit</span>
              </div>
            </template>
            <p>Pallipuram Pariyathara Shri Mutharamman Temple, Kerala, India</p>
            <p>Peaceful ambience, traditional rituals, and cultural heritage</p>
            <el-button type="primary" @click="$router.push('/temples')">Find Temple Details</el-button>
          </el-card>
        </el-col>

        <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
          <el-card shadow="hover" class="cta-card">
            <template #header>
              <div class="card-header">
                <el-icon><DataAnalysis /></el-icon>
                <span>Administration</span>
              </div>
            </template>
            <p>Authorized staff can access the internal dashboard for managing devotees, events, donations and sales.</p>
            <el-button @click="$router.push('/dashboard')">Go to Dashboard</el-button>
          </el-card>
        </el-col>
      </el-row>
    </section>
  </div>
  
  
</template>

<script setup>
import { ref } from 'vue'

const gallery = ref([
  '/images/image2.jpg',
  '/images/image4.jpg',
  '/images/image6.jpg',
  '/images/image8.jpg',
])
</script>

<style scoped>
.home {
  max-width: 1200px;
  margin: 0 auto;
}

.home-hero {
  position: relative;
  background: linear-gradient(135deg, rgba(168,50,26,0.86), rgba(221,146,39,0.86)), var(--devotional-hero-bg, url('/images/temple-hero.jpg'));
  background-size: cover;
  background-position: center;
  border-radius: 14px;
  box-shadow: 0 12px 24px rgba(0,0,0,0.12);
  overflow: hidden;
}

.hero-overlay {
  padding: 60px 24px;
  text-align: center;
  color: #fff;
}

.hero-overlay h1 {
  margin: 0 0 10px 0;
  font-size: 34px;
  font-weight: 800;
  letter-spacing: 0.5px;
}

.hero-overlay p {
  margin: 0 0 18px 0;
  font-size: 16px;
  opacity: 0.95;
}

.hero-actions {
  display: inline-flex;
  gap: 12px;
}

.home-section {
  margin: 24px 0;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.info-card .list {
  padding-left: 18px;
  margin: 0;
  display: grid;
  gap: 8px;
}

.services {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.home-banner {
  margin: 8px 0 24px 0;
}

.banner-inner :deep(.el-carousel__container) {
  border-radius: 12px;
}

.banner-slide {
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  border-radius: 12px;
}

.visit-card p,
.cta-card p {
  margin: 6px 0 14px 0;
  color: #606266;
}

@media (max-width: 768px) {
  .hero-overlay {
    padding: 36px 16px;
  }
  .hero-overlay h1 {
    font-size: 26px;
  }
  .hero-overlay p {
    font-size: 14px;
  }
}
</style>


