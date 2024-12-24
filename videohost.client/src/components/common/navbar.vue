<template>
  <nav class="navbar navbar-expand-lg navbar-light bg-light">
    <div class="container-fluid">
      <ul class="navbar-nav mr-auto">
        <li class="nav-item" v-for="route in visibleRoutes" :key="route.name">
          <router-link :to="route.path" class="nav-link">{{ route.name }}</router-link>
        </li>
      </ul>
      <div v-if="!userStore.isAuthenticated">
        <router-link :to="{ name: 'Login' }" class="btn btn-primary me-2" role="button">
          Login
        </router-link>
        <router-link :to="{ name: 'Register' }" class="btn btn-secondary" role="button">
          Register
        </router-link>
      </div>
      <div v-if="userStore.isAuthenticated">
        <button class="btn btn-danger" @click="handleLogout">Logout</button>
      </div>
    </div>
  </nav>
</template>

<script setup>
  import { computed } from 'vue';

  import { useRouter } from 'vue-router';

  import routes from '@/router/routes.js';

  import { useUserStore } from '@/stores/user';

  const router = useRouter();

  const userStore = useUserStore();

  const visibleRoutes = computed(() => {
    return routes.filter(route => {
        if (!userStore.isAuthenticated && route.meta.requiresAuth)
           return false;

        if (userStore.isAuthenticated && userStore.user.role !== 'Admin' && route.meta.requiresAdmin)
          return false;

        if (route.meta.showInMenu)
           return true;

        return false;
    });
  })

  const handleLogout = async () => {
    router.push({ name: 'Home' });

    await userStore.logout();
  };
</script>
