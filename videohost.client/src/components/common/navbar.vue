<template>
  <nav class="navbar navbar-expand-lg navbar-light bg-light">
    <div class="container-fluid">
      <ul class="navbar-nav mr-auto">
        <li class="nav-item" v-for="route in routes" :key="route.name">
          <router-link :to="route.path" class="nav-link">{{ route.name }}</router-link>
        </li>
      </ul>
    </div>
  </nav>
</template>

<script setup>
  import routes from '@/routes.js'
</script>
