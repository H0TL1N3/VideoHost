<template>
  <h1>Hello!</h1>

  <div>Congratulations! You have reached a sample <b>Dashboard</b> page.</div>

  <div>Your info:</div>

  <div class="row align-items-start">
    <div class="card col-4">
      <div class="card-body">
        <h5 class="card-title">{{userStore.user.displayName}}</h5>
        <p class="card-text">Registration date: {{userStore.user.registrationDate}}</p>
      </div>
    </div>
  </div>

</template>

<script setup>
  import { ref, onBeforeMount } from 'vue'

  import { useHead } from '@unhead/vue'
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js'

  import { useUserStore } from '@/stores/user';

  const userStore = useUserStore();

  useHead({
    title: 'Dashboard | ' + DEFAULT_TITLE
  })
</script>
