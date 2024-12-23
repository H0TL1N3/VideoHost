<template>

  <div v-if="!userStore.isAuthenticated">
    <h1>Hello!</h1>

    <div>Welcome to the <b>Home</b> page. To see updates from your subscriptions, please, log in.</div>
  </div>

  <div v-else>
    <h1>{{ userStore.user.displayName }}'s Home page</h1>

    <div class="row align-items-start">

      <div class="col">
        <div class="mb-3">Videos from your subscriptions:</div>
        <video-list :subscriber-id="userStore.user.id" :compact-style="true" />

      </div>

      <div class="col">
        <div class="mb-3">Comments from your subscriptions:</div>
        <comment-list :subscriber-id="userStore.user.id" />
      </div>

    </div>

  </div>

</template>

<script setup>
  import VideoList from '@/components/common/lists/video.vue';
  import CommentList from '@/components/common/lists/comment.vue';

  import { useUserStore } from '@/stores/user';

  const userStore = useUserStore();
</script>
