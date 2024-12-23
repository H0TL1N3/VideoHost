<template>
  <h3>{{ user.displayName }}'s Profile</h3> 

  <div class="row align-items-start">

    <div class="col">

      <div class="mb-3">{{ user.displayName }}'s info:</div>
      <div class="card container p-3 mb-3">
        <div class="row">

          <div class="col">
            <h5 class="card-title">User Name: {{ user.displayName }}</h5>
            <p class="card-text">Registration date: {{ new Date(user.registrationDate).toLocaleDateString() }}</p>
            <div v-if="userStore.isAuthenticated && props.userId !== userStore.user.id">
              <button v-if="!subscribed" class="btn btn-primary" @click="subscribe">
                Subscribe
              </button>
              <button v-else class="btn btn-danger" @click="unsubscribe">
                Unsubscribe
              </button>
            </div>
          </div>

          <div v-if="userStore.isAuthenticated && props.userId === userStore.user.id" class="col">
            <div class="float-end">
              <button @click="goToEditPage" class="btn btn-primary me-2">
                Edit Your Data
              </button>
              <button @click="deleteUser" class="btn btn-danger">
                Delete Your Profile
              </button>
            </div>
          </div>

        </div>
      </div>

    </div>

    <div class="col">
      <div class="mb-3">{{ user.displayName }}'s videos:</div>
      <video-list :user-id="props.userId" :compact-style="true"/>
    </div>

    <div class="col">
      <div class="mb-3">{{ user.displayName }}'s comments:</div>
      <comment-list :user-id="props.userId" />
    </div>

  </div>

</template>

<script setup>
  import { ref, watch, onMounted } from 'vue';

  import VideoList from '@/components/common/lists/video.vue';
  import CommentList from '@/components/common/lists/comment.vue';

  import { useHead } from '@unhead/vue';
  import { useRouter } from 'vue-router';
  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js'

  import { useUserStore } from '@/stores/user';

  const user = ref({});
  const subscribed = ref(false);

  const props = defineProps({
    userId: {
      type: Number,
      required: true
    }
  });

  const router = useRouter();
  const toast = useToast();
  const userStore = useUserStore();

  const pageTitle = ref('');

  watch(user, (newUser) => {
    pageTitle.value = newUser.displayName + '\'s Profile | ' + DEFAULT_TITLE;
  })

  useHead({
    title: pageTitle
  })

  onMounted(async () => {
  try {
    const response = await axios.get(`/api/User/get?id=${props.userId}`);

    user.value = response.data;

    if (!userStore.isAuthenticated)
      return;

    if (userStore.user.id === user.value.id)
      return;

    const subscription = await axios.get('/api/Subscription/get', {
      params: {
        subscriberId: userStore.user.id,
        subscribedToId: user.value.id
      }
    });

    if (subscription.data)
      subscribed.value = true;

  } catch (error) {
    let errorMessage = error.response?.data?.message;

    toast.error(errorMessage ?? 'An error occurred while loading the subscription data.');
  }

  const goToEditPage = () => {
    router.push({ name: 'Edit User', params: { id: props.userId } });
  }

  const deleteUser = async () => {
    if (confirm('Are you sure you want to delete your profile?')) {
      try {
        await axios.delete(`/api/Video/delete-user-videos?userId=${userStore.user.id}`)

        const response = await axios.delete('/api/User/delete');
        
        userStore.logout();

        toast.success(response.data.message);

        router.push({ name: 'Home' });       
      } catch (error) {
        let errorMessage = error.response?.data?.message;

        toast.error(errorMessage ?? 'An error occurred while deleting the profile.');
      }
    }
  }

  const subscribe = async () => {
    const formData = {
      subscriberId: userStore.user.id,
      subscribedToId: user.value.id
    }

    try {
      const response = await axios.post('/api/Subscription/add', formData);

      toast.success(response.data.message);

      subscribed.value = true;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while subscribing.');
    }
  }

  const unsubscribe = async () => {
    if (confirm('Are you sure you want to unsubscribe?')) {
      try {
        const response = await axios.delete('/api/Subscription/delete', {
          params: {
            subscriberId: userStore.user.id,
            subscribedToId: user.value.id
          }
        });

        toast.success(response.data.message);

        subscribed.value = false;
      } catch (error) {
        let errorMessage = error.response?.data?.message;

        toast.error(errorMessage ?? 'An error occurred while unsubscribing.');
      }
    }
  }
  });
</script>
