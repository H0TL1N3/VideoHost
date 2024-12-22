<template>
  <div v-if="loading" class="mt-auto mb-auto">Loading...</div>
  <div v-else class="container">

    <div class="row mb-3">
      <div class="col-12 d-flex justify-content-end">
        <select v-model="orderOption" @change="orderVideos" class="form-select w-auto">
          <option value="recent">Most Recent</option>
          <option value="views">Most Viewed</option>
        </select>
      </div>
    </div>

    <div class="row d-flex justify-content-center">
      <div class="video card col-5 mb-3 me-3" v-for="video in videos" :key="video.id">

        <router-link :to="{ name: 'Video', params: { id: video.id } }">
          <img :src="thumbnailPath(video)" class="video-preview card-img-top" :alt="video.name">
        </router-link>

        <div class="card-body container p-3 mb-3">
          <div class="row">
            <div class="col">
              <h5 class="card-title">Video name: {{ video.name }}</h5>
              <p class="card-text">Description: {{ video.description || "No description available." }}</p>
            </div>
            <div class="col">
              <div class="float-end">
                <p><strong>Uploaded by:</strong> {{ video.user.displayName }}</p>
                <p><strong>Uploaded on:</strong> {{ new Date(video.uploadDate).toLocaleDateString() }}</p>
              </div>
            </div>
          </div>
        </div>

      </div>

      <div class="text-center">
        <button v-if="hasMoreVideos" class="btn btn-outline-secondary" @click="loadVideos">
          Load More
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  const loading = ref(true);

  const videos = ref([]);
  const orderOption = ref('recent');
  const skip = ref(0);
  const take = 8;
  const hasMoreVideos = ref(true);

  const toast = useToast();

  const thumbnailPath = (video) => {
    return `/api/${video.thumbnailPath}`;
  }

  const orderVideos = async () => {
    videos.value = [];
    skip.value = 0;
    hasMoreVideos.value = true;
    await loadVideos();
  }

  const loadVideos = async () => {
    try {
      const response = await axios.get('/api/Video/get-videos', {
        params: { skip: skip.value, take: take, orderBy: orderOption.value },
      });

      if (response.data.length < take)
        hasMoreVideos.value = false;

      videos.value.push(...response.data);

      skip.value += take;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the videos.');

      hasMoreVideos.value = false;
    }
  }
  
  onMounted(async () => {
    await loadVideos();
    loading.value = false;
  })

  useHead({
    title: 'Videos | ' + DEFAULT_TITLE
  })
</script>
