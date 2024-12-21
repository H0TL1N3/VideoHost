<template>
  <div class="container d-flex flex-column align-items-center">
    <div v-if="loading" class="mt-auto mb-auto">Loading...</div>
    <div v-else class="w-75">
      <h1>{{ video.name }}</h1>
      <div class="d-flex justify-content-center mb-4">
        <video controls :src="videoPath" class="object-fit-contain mt-3"></video>
      </div>
      <div class="card container p-3 mb-3">
        <div class="row">
          <div class="col">
            <p><strong>Description:</strong> {{ video.description || "No description available." }}</p>
            <p><strong>Uploaded by:</strong> {{ video.user.displayName }}</p>
            <p><strong>Uploaded on:</strong> {{ new Date(video.uploadDate).toLocaleDateString() }}</p>
            <p><strong>Views:</strong> {{ video.viewCount }}</p>
          </div>
          <div v-if="userStore.isAuthenticated && video.user.id === userStore.user.id" class="col">
            <div class="float-end">
              <button @click="goToEditPage" class="btn btn-primary me-2">
                Edit Video
              </button>
              <button @click="deleteVideo" class="btn btn-danger">
                Delete Video
              </button>
            </div>       
          </div>
        </div>
      </div>
      <comment-section :video-id="video.id" :owner-id="video.user.id"></comment-section>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, computed, watch } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useRouter, useRoute } from 'vue-router';
  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  import CommentSection from '@/components/common/comments/comment-section.vue';
  import { useUserStore } from '@/stores/user';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  const video = ref(null);
  const loading = ref(true);

  const userStore = useUserStore();
  const toast = useToast();
  const router = useRouter();
  const route = useRoute();
  const videoId = route.params.id;

  const pageTitle = ref('');

  useHead({ title: pageTitle })

  watch(video, async (newVideo) => {
    pageTitle.value = newVideo.name + ' | ' + DEFAULT_TITLE;
  })

  const goToEditPage = () => {
    router.push({ name: 'Edit Video', params: { id: videoId } });
  };

  const deleteVideo = async() => {
    if (confirm('Are you sure you want to delete this video?')) {
      try {
        const response = await axios.delete(`/api/Video/delete?id=${video.value.id}`);

        router.push({ name: 'Home' });

        toast.success(response.data.message);
      } catch (error) {
        let errorMessage = error.response?.data?.message;

        toast.error(errorMessage ?? 'An error occurred while deleting the video.');
      }
    }
  }

  const videoPath = computed(() =>
    video.value ? `/api/${video.value.uploadPath}` : null
  );

  onMounted(async () => {
    try {
      const response = await axios.get(`/api/Video/get?videoId=${videoId}`);

      video.value = response.data;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the video.');
    } finally {
      loading.value = false;
    }
  });
</script>
