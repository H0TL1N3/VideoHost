const routes = [
  { path: '/', name: 'Home', component: () => import('@/components/views/home.vue') },
  { path: '/video', name: 'Video', component: () => import('@/components/views/video.vue') },
  { path: '/search', name: 'Search', component: () => import('@/components/views/search.vue') },
  { path: '/tags', name: 'Tags', component: () => import('@/components/views/tags.vue') },
]

export default routes;
