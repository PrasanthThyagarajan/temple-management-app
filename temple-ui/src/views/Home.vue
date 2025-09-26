<template>
  <div class="home">
    <!-- Hero Section -->
    <section class="home-hero">
      <div class="hero-overlay">
        <h1>Welcome to Devakaryam</h1>
        <p>Temple information, events, and services for devotees and visitors</p>
        <div class="hero-actions">
          <!-- Show Dashboard button when user is authenticated -->
          <template v-if="isAuthenticated">
            <el-button type="primary" size="large" @click="$router.push('/dashboard')">
              <el-icon><DataBoard /></el-icon>
              Dashboard
            </el-button>
            <el-button type="success" size="large" @click="$router.push('/profile')">
              <el-icon><User /></el-icon>
              My Profile
            </el-button>
          </template>
          
          <!-- Show Login and Register buttons when user is not authenticated -->
          <template v-else>
            <el-button type="primary" size="large" @click="showLogin = true">
              <el-icon><User /></el-icon>
              Login
            </el-button>
            <el-button type="success" size="large" @click="$router.push('/register')">
              <el-icon><EditPen /></el-icon>
              Register
            </el-button>
          </template>
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
              <li>Day 1 (Monday) – Ganapathi Homam, Navagraha Pooja & Special Alankaram</li>
              <li>Day 2 (Tuesday) – Khosha Yatra (Uravalam) with Cheriya Paduka & Valiya Paduka Seva</li>
              <li>Day 2 Evening – Vahana Seva & Cultural Programs</li>
              <li>Day 3 (Wednesday) – Ucha Koda Mahotsavam with Manja Neerattu</li>
              <li>Day 3 Special – Pongala Mahotsavam, Maha Deeparadhana & Annadhanam</li>
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
              <el-button text @click="$router.push('/dashboard')">
                <el-icon><Star /></el-icon>
                Astrology & Panchang
              </el-button>
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

    <!-- Daily Horoscope Display -->
    <section v-if="dailyHoroscope" class="home-section">
      <el-card shadow="hover" class="horoscope-card">
        <template #header>
          <div class="card-header">
            <el-icon><Star /></el-icon>
            <span>ഇന്നത്തെ ഭവിഷ്യവാക്യം - {{ dailyHoroscope.sign }}</span>
          </div>
        </template>
        <div class="horoscope-content">
          <div class="horoscope-text">
            <p><strong>ശുഭാകാംക്ഷകൾ:</strong> {{ dailyHoroscope.prediction }}</p>
            <p><strong>അനുശാസനം:</strong> {{ dailyHoroscope.advice }}</p>
            <p><strong>ഭാഗ്യസംഖ്യ:</strong> {{ dailyHoroscope.luckyNumber }}</p>
            <p><strong>ഭാഗ്യനിറം:</strong> {{ dailyHoroscope.luckyColor }}</p>
          </div>
        </div>
      </el-card>
    </section>

    <!-- Location Map -->
    <section class="home-section">
      <el-card shadow="hover" class="map-card">
        <template #header>
          <div class="card-header">
            <el-icon><Location /></el-icon>
            <span>Location Map · {{ templeName }}</span>
          </div>
        </template>
        <div class="map-embed">
          <iframe
            :src="mapsEmbedUrl"
            width="100%"
            height="320"
            style="border:0;"
            allowFullScreen
            loading="lazy"
            referrerpolicy="no-referrer-when-downgrade"
          ></iframe>
        </div>
        <div class="map-actions">
          <el-button type="primary" @click="() => window.open(mapsLink, '_blank')">
            <el-icon><Location /></el-icon>
            <span>Open in Google Maps</span>
          </el-button>
        </div>
      </el-card>
    </section>

    <!-- Login Modal for unauthorized access -->
    <LoginModal v-model="showLogin" @login-success="onLoginSuccess" />

  </div>
  
  
</template>

<script setup>
import { ref, onMounted } from 'vue'
import dayjs from 'dayjs'
import { ElMessage } from 'element-plus'
import { Star, User, EditPen, Location, DataBoard } from '@element-plus/icons-vue'
import { useRoute, useRouter } from 'vue-router'
import LoginModal from '../components/LoginModal.vue'
import { useAuth } from '../stores/auth.js'

const gallery = ref([
  '/images/image2.jpg',
  '/images/image4.jpg',
  '/images/image6.jpg',
  '/images/image8.jpg',
])


// Compute today's Rahu Kalam and Gulika Kalam using standard weekday slots
const weekday = dayjs().day() // 0=Sun ... 6=Sat
const rahuMap = {
  0: '4:30 PM – 6:00 PM',   // Sunday
  1: '7:30 AM – 9:00 AM',   // Monday
  2: '3:00 PM – 4:30 PM',   // Tuesday
  3: '12:00 PM – 1:30 PM',  // Wednesday
  4: '1:30 PM – 3:00 PM',   // Thursday
  5: '10:30 AM – 12:00 PM', // Friday
  6: '9:00 AM – 10:30 AM'   // Saturday
}
const gulikaMap = {
  0: '3:00 PM – 4:30 PM',   // Sunday
  1: '1:30 PM – 3:00 PM',   // Monday
  2: '12:00 PM – 1:30 PM',  // Tuesday
  3: '10:30 AM – 12:00 PM', // Wednesday
  4: '9:00 AM – 10:30 AM',  // Thursday
  5: '7:30 AM – 9:00 AM',   // Friday
  6: '6:00 AM – 7:30 AM'    // Saturday
}
const todayRahu = rahuMap[weekday]
const todayGulika = gulikaMap[weekday]

// Map/Location
const templeName = 'Sri Mutharamman Devi Temple'
const address = 'Sri Mutharamman Devi Temple Trust, HVM6+R62, St Andrews Rd, Kaniyapuram, Thiruvananthapuram, Kerala 695301'
const mapsQuery = encodeURIComponent(address)
const mapsEmbedUrl = `https://www.google.com/maps?q=${mapsQuery}&z=16&output=embed`
const mapsLink = `https://www.google.com/maps?q=${mapsQuery}`
// Known coordinates for accurate solar calculations (derived from address)
const templeCoords = { lat: 8.584543647772376, lng: 76.8604863098345 }

// Good Muhurtham (Abhijit) calculation
const abhijitStart = ref('')
const abhijitEnd = ref('')

// Astrology API integration
const dailyHoroscope = ref(null)
const horoscopeLoading = ref(false)

// Login modal control for unauthorized redirect
const showLogin = ref(false)
const route = useRoute()
const router = useRouter()
const { isAuthenticated, user, refreshAuthState } = useAuth()

// Malayalam translations for astrology content
const malayalamTranslations = {
  signs: {
    'Aries': 'മേഷം',
    'Taurus': 'വൃഷഭം',
    'Gemini': 'മിഥുനം',
    'Cancer': 'കർക്കടകം',
    'Leo': 'സിംഹം',
    'Virgo': 'കന്യ',
    'Libra': 'തുലാം',
    'Scorpio': 'വൃശ്ചികം',
    'Sagittarius': 'ധനു',
    'Capricorn': 'മകരം',
    'Aquarius': 'കുംഭം',
    'Pisces': 'മീനം'
  },
  predictions: {
    'excellent': 'മികച്ച',
    'very good': 'വളരെ നല്ല',
    'good': 'നല്ല',
    'average': 'ശരാശരി',
    'fair': 'ശരാശരി',
    'poor': 'മോശം',
    'very poor': 'വളരെ മോശം'
  }
}

const computeAbhijitMuhurtam = async () => {
  try {
    // Use known temple coordinates (avoid browser geocoding restrictions)
    const lat = templeCoords.lat
    const lng = templeCoords.lng

    // Build date string for IST to avoid UTC day rollover issues
    const nowUtc = Date.now()
    const istNow = new Date(nowUtc + 5.5 * 60 * 60 * 1000)
    const yyyy = istNow.getUTCFullYear()
    const mm = String(istNow.getUTCMonth() + 1).padStart(2, '0')
    const dd = String(istNow.getUTCDate()).padStart(2, '0')
    const dateStr = `${yyyy}-${mm}-${dd}`

    // Fetch sunrise/sunset in UTC
    const sunUrl = `https://api.sunrise-sunset.org/json?lat=${lat}&lng=${lng}&date=${dateStr}&formatted=0`
    const sunRes = await fetch(sunUrl)
    const sun = await sunRes.json()
    if (!sun || !sun.results) return

    const sunriseUTC = new Date(sun.results.sunrise).getTime()
    const sunsetUTC = new Date(sun.results.sunset).getTime()
    if (!sunriseUTC || !sunsetUTC) return

    // Convert to IST (fixed +05:30 since temple is in Kerala)
    const offsetMs = 5.5 * 60 * 60 * 1000
    const sunriseIST = sunriseUTC + offsetMs
    const sunsetIST = sunsetUTC + offsetMs

    const dayLengthMs = sunsetIST - sunriseIST
    if (dayLengthMs <= 0) return

    const muhurtamMs = dayLengthMs / 15
    const solarNoonMs = sunriseIST + dayLengthMs / 2
    const startMs = solarNoonMs - muhurtamMs / 2
    const endMs = solarNoonMs + muhurtamMs / 2

    const fmt = new Intl.DateTimeFormat('en-IN', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
      timeZone: 'Asia/Kolkata'
    })
    abhijitStart.value = fmt.format(new Date(startMs))
    abhijitEnd.value = fmt.format(new Date(endMs))
  } catch (e) {
    console.error('Failed to compute Abhijit Muhurtham', e)
  }
}

// Astrology API functions
const loadDailyHoroscope = async () => {
  horoscopeLoading.value = true
  try {
    // For demo purposes, we'll create a sample horoscope
    // In production, you would call the actual FreeAstrologyAPI.com API
    const signs = ['മേഷം', 'വൃഷഭം', 'മിഥുനം', 'കർക്കടകം', 'സിംഹം', 'കന്യ', 'തുലാം', 'വൃശ്ചികം', 'ധനു', 'മകരം', 'കുംഭം', 'മീനം']
    const colors = ['ചുവപ്പ്', 'നീല', 'പച്ച', 'മഞ്ഞ', 'നാരങ്ങ', 'വെള്ള', 'കറുപ്പ്', 'സ്വർണ്ണം', 'വെള്ളി', 'ഗുലാബി']
    const predictions = [
      'ഇന്ന് നിങ്ങൾക്ക് ഒരു മികച്ച ദിവസമാണ്. പുതിയ അവസരങ്ങൾ വരികയും നിങ്ങളുടെ പ്രവർത്തനങ്ങൾ ഫലപ്രദമാകുകയും ചെയ്യും.',
      'ഇന്ന് ശാന്തതയും സമാധാനവും നിങ്ങളുടെ ജീവിതത്തിൽ പ്രധാനമാണ്. ആഴത്തിൽ ചിന്തിക്കുകയും ശരിയായ തീരുമാനങ്ങൾ എടുക്കുകയും ചെയ്യുക.',
      'ഇന്ന് നിങ്ങളുടെ സാമൂഹിക ജീവിതത്തിൽ പ്രധാനമാണ്. സുഹൃത്തുക്കളുമായി സമയം ചെലവഴിക്കുകയും പുതിയ ബന്ധങ്ങൾ സൃഷ്ടിക്കുകയും ചെയ്യുക.',
      'ഇന്ന് നിങ്ങളുടെ കുടുംബത്തിന് പ്രാധാന്യം കൂടുതലാണ്. പ്രിയപ്പെട്ടവരുമായി സമയം ചെലവഴിക്കുകയും അവരുടെ ആവശ്യങ്ങൾ കാര്യമാക്കുകയും ചെയ്യുക.'
    ]
    const advices = [
      'ധൈര്യത്തോടെ പുതിയ ചുമതലകൾ സ്വീകരിക്കുക. ജനങ്ങളുമായി സഹകരണത്തോടെ പ്രവർത്തിക്കുക.',
      'ക്ഷമയോടെ പ്രവർത്തിക്കുക. എല്ലാവരുമായും സൗഹൃദത്തോടെ പെരുമാറുക.',
      'നിങ്ങളുടെ സ്വന്തം ആശയങ്ങൾ പ്രകടിപ്പിക്കുക. മറ്റുള്ളവരുടെ അഭിപ്രായങ്ങളും കാര്യമാക്കുക.',
      'കുടുംബത്തിന് സമയം നൽകുക. പ്രിയപ്പെട്ടവരുമായി ആഴത്തിലുള്ള ബന്ധം സൃഷ്ടിക്കുക.'
    ]
    
    const randomSign = signs[Math.floor(Math.random() * signs.length)]
    const randomPrediction = predictions[Math.floor(Math.random() * predictions.length)]
    const randomAdvice = advices[Math.floor(Math.random() * advices.length)]
    const randomColor = colors[Math.floor(Math.random() * colors.length)]
    
    const sampleHoroscope = {
      sign: randomSign,
      prediction: randomPrediction,
      advice: randomAdvice,
      luckyNumber: Math.floor(Math.random() * 100) + 1,
      luckyColor: randomColor
    }
    
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    dailyHoroscope.value = sampleHoroscope
    ElMessage.success('ഭവിഷ്യവാക്യം വിജയകരമായി ലോഡ് ചെയ്തു!')
  } catch (error) {
    console.error('Error loading horoscope:', error)
    ElMessage.error('ഭവിഷ്യവാക്യം ലോഡ് ചെയ്യാൻ കഴിഞ്ഞില്ല. ദയവായി വീണ്ടും ശ്രമിക്കുക.')
  } finally {
    horoscopeLoading.value = false
  }
}

// Function to call actual FreeAstrologyAPI.com (when API key is available)
// To get your API key, visit: https://freeastrologyapi.com/api-reference
const callAstrologyAPI = async (endpoint, data) => {
  const API_BASE_URL = 'https://json.freeastrologyapi.com'
  const API_KEY = 'YOUR_API_KEY_HERE' // Replace with actual API key from FreeAstrologyAPI.com
  
  try {
    const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-api-key': API_KEY
      },
      body: JSON.stringify(data)
    })
    
    if (!response.ok) {
      throw new Error(`API Error: ${response.status}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Astrology API Error:', error)
    throw error
  }
}

onMounted(() => {
  computeAbhijitMuhurtam()
  if (route.query.login === '1') {
    showLogin.value = true
  }
})

const onLoginSuccess = () => {
  showLogin.value = false
  // Refresh auth state to update the buttons immediately
  refreshAuthState()
  const redirect = route.query.redirect
  if (typeof redirect === 'string' && redirect.startsWith('/')) {
    router.replace(redirect)
  }
}
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

.auspicious-list {
  padding-left: 18px;
  margin: 0 0 8px 0;
  display: grid;
  gap: 6px;
}

.timings-note {
  color: #909399;
  font-size: 12px;
}

.map-card {
  overflow: hidden;
}

.map-embed iframe {
  border-radius: 12px;
}

.map-actions {
  margin-top: 12px;
  display: flex;
  justify-content: flex-end;
}

/* Astrology Section Styles */
.astrology-card {
  overflow: hidden;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.astrology-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.astrology-image {
  text-align: center;
}

.astrology-banner-img {
  width: 100%;
  height: 120px;
  object-fit: cover;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}

.astrology-banner-placeholder {
  width: 100%;
  height: 120px;
  background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}

.astrology-banner-placeholder p {
  margin: 8px 0 0 0;
  font-size: 14px;
  font-weight: 500;
}

.astrology-info h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
  font-weight: 600;
  color: #fff;
}

.astrology-info p {
  margin: 0 0 16px 0;
  font-size: 14px;
  line-height: 1.5;
  opacity: 0.9;
}

.astrology-features {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 16px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  opacity: 0.9;
}

.feature-item .el-icon {
  font-size: 16px;
  color: #ffd700;
}

.horoscope-card {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  color: white;
}

.horoscope-content {
  padding: 16px 0;
}

.horoscope-text p {
  margin: 0 0 12px 0;
  font-size: 14px;
  line-height: 1.6;
}

.horoscope-text p:last-child {
  margin-bottom: 0;
}

.horoscope-text strong {
  color: #ffd700;
  font-weight: 600;
}

/* Responsive Design for Astrology Section */
@media (max-width: 768px) {
  .astrology-content {
    gap: 12px;
  }
  
  .astrology-banner-img {
    height: 100px;
  }
  
  .astrology-info h3 {
    font-size: 16px;
  }
  
  .astrology-info p {
    font-size: 13px;
  }
  
  .feature-item {
    font-size: 12px;
  }
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


