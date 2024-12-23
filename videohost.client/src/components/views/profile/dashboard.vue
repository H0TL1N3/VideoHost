<template>
  <user-profile :user-id="userStore.user.id" />
</template>

<script setup>
  import UserProfile from '@/components/common/profile/user.vue';

  import { useUserStore } from '@/stores/user';

  const userStore = useUserStore();
</script>
