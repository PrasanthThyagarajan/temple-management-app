import { createRouter, createWebHistory } from 'vue-router';
import Home from '../views/Home.vue';
import Dashboard from '../views/Dashboard.vue';
import RoleManagement from '../components/RoleManagement.vue';
import UserRoleConfiguration from '../components/UserRoleConfiguration.vue';
import UserRegistration from '../components/UserRegistration.vue';
import AdminUserManagement from '../components/AdminUserManagement.vue';
import RolePermissions from '../components/RolePermissions.vue';
import Verify from '../views/Verify.vue';
import Devotees from '../views/Devotees.vue';
import Donations from '../views/Donations.vue';
import Events from '../views/Events.vue';
import Temples from '../views/Temples.vue';
import Products from '../views/Products.vue';
import Categories from '../views/Categories.vue';
import Sales from '../views/Sales.vue';

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/admin/role-permissions',
    name: 'RolePermissions',
    component: RolePermissions,
    meta: { requiresAdmin: true }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard
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
    path: '/temples',
    name: 'Temples',
    component: Temples
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
  },
  {
    path: '/verify',
    name: 'Verify',
    component: Verify
  },
  {
    path: '/roles',
    name: 'RoleManagement',
    component: RoleManagement,
    meta: { requiresPermission: 'UserRoleConfiguration' }
  },
  {
    path: '/user-roles',
    name: 'UserRoleConfiguration',
    component: UserRoleConfiguration,
    meta: { requiresPermission: 'UserRoleConfiguration' }
  },
  {
    path: '/register',
    name: 'UserRegistration',
    component: UserRegistration
  },
  {
    path: '/admin/users',
    name: 'AdminUserManagement',
    component: AdminUserManagement,
    meta: { requiresPermission: 'UserRoleConfiguration' }
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL || '/'),
  routes
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  const userRaw = localStorage.getItem('user');
  const permsRaw = localStorage.getItem('permissions');
  let roles = [];
  let perms = [];
  try {
    roles = userRaw ? (JSON.parse(userRaw).roles || []) : [];
    perms = permsRaw ? JSON.parse(permsRaw) : [];
  } catch (_) {}

  // Only allow Home for unauthenticated users; show login popup trigger via query
  if (!token && to.name !== 'Home') {
    return next({ name: 'Home', query: { login: '1', redirect: to.fullPath } });
  }

  const requiredPermission = to.matched.find(r => r.meta && r.meta.requiresPermission)?.meta.requiresPermission
  // Admin always allowed
  if (requiredPermission && !roles.includes('Admin') && !perms.includes(requiredPermission)) {
    return next({ name: 'Home' });
  }
  return next();
});

export default router;
