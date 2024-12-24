const routes = [
  // Always available routes
  { path: '/', name: 'Home', component: () => import('@/components/views/home.vue'), meta: { showInMenu: true } },
  { path: '/videos', name: 'Videos', component: () => import('@/components/views/videos.vue'), meta: { showInMenu: true } },
  { path: '/search', name: 'Search', component: () => import('@/components/views/search.vue'), meta: { showInMenu: true } },
  { path: '/tags', name: 'Tags', component: () => import('@/components/views/tags.vue'), meta: { showInMenu: true } },
  // User routes
  { path: '/login', name: 'Login', component: () => import('@/components/views/profile/login.vue'), meta: { showInMenu: false } },
  { path: '/register', name: 'Register', component: () => import('@/components/views/profile/register.vue'), meta: { showInMenu: false } },
  { path: '/dashboard', name: 'Dashboard', component: () => import('@/components/views/profile/dashboard.vue'), meta: { showInMenu: true, requiresAuth: true } },
  { path: '/user/:id', name: 'Profile', component: () => import('@/components/views/profile/profile.vue'), meta: { showInMenu: false } },
  { path: '/edit-user/:id', name: 'Edit User', component: () => import('@/components/views/profile/edit.vue'), meta: { showInMenu: false, requiresAuth: true } },
  // Video routes
  { path: '/upload-video', name: 'Upload Video', component: () => import('@/components/views/video/upload.vue'), meta: { showInMenu: true, requiresAuth: true } },
  { path: '/edit-video/:id', name: 'Edit Video', component: () => import('@/components/views/video/edit.vue'), meta: { showInMenu: false, requiresAuth: true } },
  { path: '/video/:id', name: 'Video', component: () => import('@/components/views/video/video.vue'), meta: { showInMenu: false } },
  // Admin routes
  { path: '/admin/dashboard', name: 'Admin Dashboard', component: () => import('@/components/views/admin/dashboard.vue'), meta: { showInMenu: true, requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/edit-tag/:id', name: 'Admin Edit Tag', component: () => import('@/components/views/admin/edit-tag.vue'), meta: { showInMenu: false, requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/edit-video/:id', name: 'Admin Edit Video', component: () => import('@/components/views/admin/edit-video.vue'), meta: { showInMenu: false, requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/edit-comment/:id', name: 'Admin Edit Comment', component: () => import('@/components/views/admin/edit-comment.vue'), meta: { showInMenu: false, requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/edit-user/:id', name: 'Admin Edit User', component: () => import('@/components/views/admin/edit-user.vue'), meta: { showInMenu: false, requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/edit-subscription/:id', name: 'Admin Edit Subscription', component: () => import('@/components/views/admin/edit-subscription.vue'), meta: { showInMenu: false, requiresAuth: true, requiresAdmin: true } },
  // Error routes
  { path: '/403-forbidden', name: '403 Forbidden', component: () => import('@/components/views/error/forbidden.vue'), meta: { showInMenu: false } },
  { path: '/:pathMatch(.*)*', name: '404 Not Found', component: () => import('@/components/views/error/page-not-found.vue'), meta: { showInMenu: false } }
]

export default routes;
