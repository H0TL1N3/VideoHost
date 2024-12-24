<template>
  <div v-if="loading" class="text-center">Loading...</div>
  <div v-else-if="!comments.length" class="text-center">No comments found.</div>
  <div v-else class="comment-list">

    <div v-for="comment in comments" :key="comment.id" class="comment card mb-2">

      <div class="card-body">
        <h5 class="card-title">Under the video:
          <router-link :to="{ name: 'Video', params: { id: comment.video.id } }">
            {{ comment.video.name }}
          </router-link>     
        </h5>
        <p>{{ comment.content }}</p>
        <small class="text-muted">on {{ new Date(comment.creationDate).toLocaleString() }}</small>
      </div>

    </div>

    <div class="text-center">
      <button v-if="hasMoreComments" class="btn btn-outline-secondary" @click="loadComments">
        Load More
      </button>
    </div>

  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';

  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  const props = defineProps({
    subscriberId: {
      type: Number,
      required: false
    },
    userId: {
      type: Number,
      required: false
    }
  });

  const loading = ref(true);

  const comments = ref([]);
  const skip = ref(0);
  const take = 10;
  const hasMoreComments = ref(true);

  const toast = useToast();

  const loadComments = async () => {
    try {
      const response = await axios.get('/api/Comment/get', {
        params: {
          userId: props.userId,
          subscriberId: props.subscriberId,
          skip: skip.value,
          take: take
        },
      });

      if (response.data.length < take)
        hasMoreComments.value = false;

      comments.value.push(...response.data);

      skip.value += take;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the comments.');
    }
  };

  onMounted(async () => {
    await loadComments();
    loading.value = false;
  })
</script>
