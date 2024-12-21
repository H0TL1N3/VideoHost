const routes = [
  { path: '/', name: 'Home', component: () => import('@/components/views/home.vue'), meta: { showInMenu: true } },
  { path: '/videos', name: 'Videos', component: () => import('@/components/views/videos.vue'), meta: { showInMenu: true } },
  { path: '/search', name: 'Search', component: () => import('@/components/views/search.vue'), meta: { showInMenu: true } },
  { path: '/tags', name: 'Tags', component: () => import('@/components/views/tags.vue'), meta: { showInMenu: true } },
  { path: '/dashboard', name: 'Dashboard', component: () => import('@/components/views/profile/dashboard.vue'), meta: { showInMenu: true, requiresAuth: true }, },
  { path: '/upload-video', name: 'Upload Video', component: () => import('@/components/views/video/upload.vue'), meta: { showInMenu: true, requiresAuth: true }, },
  { path: '/edit-video/:id', name: 'Edit Video', component: () => import('@/components/views/video/edit.vue'), meta: { showInMenu: false, requiresAuth: true }, },
  { path: '/video/:id', name: 'Video', component: () => import('@/components/views/video/video.vue'), meta: { showInMenu: false } },
  { path: '/login', name: 'Login', component: () => import('@/components/views/profile/login.vue'), meta: { showInMenu: false } },
  { path: '/register', name: 'Register', component: () => import('@/components/views/profile/register.vue'), meta: { showInMenu: false } },
  { path: '/403-forbidden', name: '403 Forbidden', component: () => import('@/components/views/error/forbidden.vue'), meta: { showInMenu: false } },
  { path: '/:pathMatch(.*)*', name: '404 Not Found', component: () => import('@/components/views/error/page-not-found.vue'), meta: { showInMenu: false } }
]

export default routes;
