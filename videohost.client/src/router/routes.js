const routes = [
  { path: '/', name: 'Home', component: () => import('@/components/views/home.vue'), meta: { showInMenu: true } },
  { path: '/video', name: 'Video', component: () => import('@/components/views/video.vue'), meta: { showInMenu: true } },
  { path: '/search', name: 'Search', component: () => import('@/components/views/search.vue'), meta: { showInMenu: true } },
  { path: '/tags', name: 'Tags', component: () => import('@/components/views/tags.vue'), meta: { showInMenu: true } },
  { path: '/dashboard', name: 'Dashboard', component: () => import('@/components/views/profile/dashboard.vue'), meta: { showInMenu: true,  requiresAuth: true }, },
  { path: '/login', name: 'Login', component: () => import('@/components/views/profile/login.vue'), meta: { showInMenu: false } },
  { path: '/register', name: 'Register', component: () => import('@/components/views/profile/register.vue'), meta: { showInMenu: false } },
]

export default routes;
