import { createRouter, createWebHistory } from 'vue-router';
import routes from './routes.js'

import { useUserStore } from '@/stores/user';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach((to, from, next) => {
  const userStore = useUserStore();

  // Redirect to the login page if not authenticated
  if (to.meta.requiresAuth && !userStore.isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } });
    return;
  }

  // Let the app access the static file
  if (to.path.startsWith('/api/uploads')) {   
    window.location.href = to.fullPath;
    return;
  }

  next();
});

export default router;
