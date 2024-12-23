<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else class="container">
      <div class="row">

        <div class="col">
          <edit-basic-info :user="user"/>
        </div>

        <div class="col">
          <edit-sensitive-info :user="user"/>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue';

  import EditBasicInfo from '@/components/common/forms/user/edit-basic.vue';
  import EditSensitiveInfo from '@/components/common/forms/user/edit-sensitive.vue';

  import { useHead } from '@unhead/vue';
  import { useToast } from 'vue-toast-notification';
  import { useRouter, useRoute } from 'vue-router';
  import axios from 'axios';

  import { useUserStore } from '@/stores/user';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  const loading = ref(true);

  const toast = useToast();
  const userStore = useUserStore();
  const router = useRouter();
  const route = useRoute();
  const userId = route.params.id;
  const user = ref({});

  const pageTitle = ref(DEFAULT_TITLE);

  useHead({ title: pageTitle })

  watch(user, (newUser) => {
    pageTitle.value = 'Edit ' + user.username + ' | ' + DEFAULT_TITLE;
  })

  onMounted(async () => {
    try {
      const response = await axios.get(`/api/User/get?id=${userId}`);

      user.value = response.data;

      // Check if the logged-in user is the user that is being edited
      if (user.value.id !== userStore.user.id) {
        router.push({ name: '403 Forbidden' });
        return;
      }
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the user.');
    } finally {
      loading.value = false;
    }
  })
</script>
