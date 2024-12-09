<template>
    <NavBar></NavBar>

    <div class="container-fluid body-content m-4">
      <router-view></router-view>
    </div>

    <footer class="py-3 fixed-bottom bg-light">
      <div class="container-fluid">
        <p class="text-muted">&copy; VideoHost Qualification Project - 2024, ik22076</p>
      </div>    
    </footer>
</template>

<script setup>
  import { useHead } from '@unhead/vue'

  import { DEFAULT_TITLE } from '@/assets/const.js'

  import NavBar from './components/common/navbar.vue'

  useHead({
    title: DEFAULT_TITLE
  })
</script>
