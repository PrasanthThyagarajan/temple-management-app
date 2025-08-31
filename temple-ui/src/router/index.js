import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import Temples from '../views/Temples.vue'
import Devotees from '../views/Devotees.vue'
import Donations from '../views/Donations.vue'
import Events from '../views/Events.vue'

const routes = [
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard
  },
  {
    path: '/temples',
    name: 'Temples',
    component: Temples
  },
  {
    path: '/devotees',
    name: 'Devotees',
    component: Devotees
  },
  {
    path: '/donations',
    name: 'Donations',
    component: Donations
  },
  {
    path: '/events',
    name: 'Events',
    component: Events
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
