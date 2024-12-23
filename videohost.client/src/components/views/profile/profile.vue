<template>
  <user-profile :user-id="userId" />
</template>

<script setup>
  import UserProfile from '@/components/common/profile/user.vue';

  import { useRoute } from 'vue-router';

  const route = useRoute();

  const userId = parseInt(route.params.id);
</script>
