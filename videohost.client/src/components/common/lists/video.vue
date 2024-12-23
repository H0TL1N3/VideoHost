<template>
  <div v-if="loading" class="text-center">Loading...</div>
  <div v-if="!videos.length" class="text-center">No videos found.</div>
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

      <div class="video card col-5 mb-3 me-3" v-for="video in videos" :key="video.id" :class="props.compactStyle ? 'col-12' : 'col-5'">

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
                <p>
                  <strong>Uploaded by: </strong>
                  <router-link :to="{ name: 'Profile', params: { id: video.user.id } }">
                    {{ video.user.displayName }}
                  </router-link>
                </p>
                <p><strong>Uploaded on: </strong>{{ new Date(video.uploadDate).toLocaleDateString() }}</p>
              </div>
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
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  const props = defineProps({
    pageTitle: {
      type: String,
      required: false
    },
    searchTerm: {
      type: String,
      required: false,
      default: null
    },
    tagIds: {
      type: Array,
      required: false,
      default: null
    },
    subscriberId: {
      type: Number,
      required: false
    },
    userId: {
      type: Number,
      required: false
    },
    compactStyle: {
      type: Boolean,
      required: false,
      default: false
    }
  });

  watch(props, (newProps) => {
    resetState();
    loadVideos();
  })

  const resetState = () => {
    videos.value = [];
    skip.value = 0;
    hasMoreVideos.value = true;
  }

  if (props.pageTitle) {
    useHead({
      title: props.pageTitle + ' | ' + DEFAULT_TITLE
    })
  }
  
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
      let tagIdsToPass = null;

      if (props.tagIds !== null) {
        tagIdsToPass = props.tagIds.length ? JSON.stringify(props.tagIds) : null;
      }   

      const response = await axios.get('/api/Video/get-many', {
        params: {
          searchTerm: props.searchTerm,
          tagIds: tagIdsToPass,
          userId: props.userId,
          subscriberId: props.subscriberId,
          orderBy: orderOption.value,
          skip: skip.value,
          take: take,
        }
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
</script>
