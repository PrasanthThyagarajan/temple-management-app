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
import Areas from '../views/Areas.vue';
import Categories from '../views/Categories.vue';
import Sales from '../views/Sales.vue';
import EventExpenses from '../views/EventExpenses.vue';
import EventExpenseItems from '../views/EventExpenseItems.vue';
import EventExpenseServices from '../views/EventExpenseServices.vue';
import Vouchers from '../views/Vouchers.vue';
import ContributionSettings from '../views/ContributionSettings.vue';
import Contributions from '../views/Contributions.vue';
import ApiTest from '../views/ApiTest.vue';
import UserProfile from '../views/UserProfile.vue';

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/areas',
    name: 'Areas',
    component: Areas
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
  },
  {
    path: '/event-expenses',
    name: 'EventExpenses',
    component: EventExpenses
  },
  {
    path: '/event-expense-items',
    name: 'EventExpenseItems',
    component: EventExpenseItems
  },
  {
    path: '/event-expense-services',
    name: 'EventExpenseServices',
    component: EventExpenseServices
  },
  {
    path: '/vouchers',
    name: 'Vouchers',
    component: Vouchers,
    meta: { requiresPermission: 'ExpenseApproval' }
  },
  {
    path: '/contribution-settings',
    name: 'ContributionSettings',
    component: ContributionSettings
  },
  {
    path: '/contributions',
    name: 'Contributions',
    component: Contributions
  },
  {
    path: '/manage-Expenses',
    redirect: '/event-expenses'
  },
  {
    path: '/create-manage-event',
    name: 'CreateManageEvent',
    component: Events
  },
  {
    path: '/add-events',
    name: 'AddEvents',
    component: Events
  },
  {
    path: '/api-test',
    name: 'ApiTest',
    component: ApiTest
  },
  {
    path: '/profile',
    name: 'Profile',
    component: UserProfile
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL || '/'),
  routes
});

// Router guard is now handled in main.js before router initialization
// This ensures authentication verification happens before any routing

export default router;
