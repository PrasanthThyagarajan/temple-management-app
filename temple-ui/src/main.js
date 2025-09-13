import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import './styles/responsive.css'
import './styles/devotional-theme.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router from './router'
import RoleManagement from './components/RoleManagement.vue';
import UserRegistration from './components/UserRegistration.vue';
import UserRoleConfiguration from './components/UserRoleConfiguration.vue';

const app = createApp(App)

// Register Element Plus icons
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.use(createPinia())
app.use(router)
app.use(ElementPlus)
app.component('RoleManagement', RoleManagement)
app.component('UserRegistration', UserRegistration)
app.component('UserRoleConfiguration', UserRoleConfiguration)

app.mount('#app')
