import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '@/stores/auth'
import Home from '../views/Home.vue'
import Dashboard from '../views/Dashboard.vue'
import Temples from '../views/Temples.vue'
import Devotees from '../views/Devotees.vue'
import Donations from '../views/Donations.vue'
import Events from '../views/Events.vue'
import Products from '../views/Products.vue'
import Categories from '../views/Categories.vue'
import Sales from '../views/Sales.vue'
import RolePermissions from '../views/RolePermissions.vue'


const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/dashboard',
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
  },
  {
    path: '/products',
    name: 'Products',
    component: Products
  },
  {
    path: '/categories',
    name: 'Categories',
    component: Categories
  },
  {
    path: '/sales',
    name: 'Sales',
    component: Sales
  }

  ,
  {
    path: '/admin/role-permissions',
    name: 'RolePermissions',
    component: RolePermissions,
    meta: { requiresAdmin: true }
  }

]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const { hasRole } = useAuth()
  if (to.meta?.requiresAdmin && !hasRole('Admin')) {
    return next('/dashboard')
  }
  next()
})

export default router
