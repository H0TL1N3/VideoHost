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

  // Redirect to the forbidden page if already authenticated
  if (to.meta.authPage && userStore.isAuthenticated) {
    next({ name: '403 Forbidden' });
    return;
  }

  // Don't let regular users access the admin apges
  if (to.meta.requiresAdmin && userStore.isAuthenticated && userStore.user.role != 'Admin') {
    next({ name: '403 Forbidden' });
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
