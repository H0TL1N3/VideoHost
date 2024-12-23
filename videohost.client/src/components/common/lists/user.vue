<template>
  <div v-if="loading" class="mt-auto mb-auto">Loading...</div>
  <div v-else class="container">

    <div class="row mb-3">
      <div class="col-12 d-flex justify-content-end">
        <select v-model="orderOption" @change="orderUsers" class="form-select w-auto">
          <option value="recent">Most Recent</option>
          <option value="subscribers">Most Popular</option>
        </select>
      </div>
    </div>

      <div class="row d-flex justify-content-center">

        <div class="user card col-5 mb-3 me-3" v-for="user in users" :key="user.id">
          <div class="card-body d-flex flex-column justify-content-between p-3 mb-3">

            <div class="mb-auto">
              <h5 class="card-title">User name: {{ user.displayName }}</h5>
              <p class="card-text">Registration Date: {{ new Date(user.registrationDate).toLocaleDateString() }}</p>
            </div>

            <div class="d-flex justify-content-between align-items-end">

              <p class="mb-0"><strong>Subscriber count:</strong> {{ user.subscriberCount }}</p>

              <router-link :to="{ name: 'Profile', params: { id: user.id } }" class="btn btn-primary" role="button">
                Profile
              </router-link>
            </div>

          </div>
        </div>

        <div class="text-center">
          <button v-if="hasMoreUsers" class="btn btn-outline-secondary" @click="loadUsers">
            Load More
          </button>
        </div>

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
      required: true
    },
    searchTerm: {
      type: String,
      required: false,
      default: null
    }
  });

  watch(props, (newProps) => {
    resetState();
    loadUsers();
  })

  const resetState = () => {
    users.value = [];
    skip.value = 0;
    hasMoreUsers.value = true;
  }

  useHead({
    title: props.pageTitle + ' | ' + DEFAULT_TITLE
  })

  const loading = ref(true);

  const users = ref([]);
  const orderOption = ref('recent');
  const skip = ref(0);
  const take = 8;
  const hasMoreUsers = ref(true);

  const toast = useToast();

  const orderUsers = async () => {
    users.value = [];
    skip.value = 0;
    hasMoreUsers.value = true;
    await loadUsers();
  }

  const loadUsers = async () => {
    try {
      const response = await axios.get('/api/User/get-many', {
        params: {
          skip: skip.value,
          take: take,
          orderBy: orderOption.value,
          searchTerm: props.searchTerm
        }
      });

      if (response.data.length < take)
        hasMoreUsers.value = false;

      users.value.push(...response.data);

      skip.value += take;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the users.');

      hasMoreUsers.value = false;
    }
  }

  onMounted(async () => {
    await loadUsers();
    loading.value = false;
  })
</script>
