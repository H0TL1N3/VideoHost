<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <form @submit.prevent="onSubmit">

        <div class="mb-3 w-25">
          <label for="name" class="form-label">Video Name</label>
          <input v-model="name" type="text" placeholder="Enter video name" class="form-control" :class="{ 'is-invalid': errors.name }" />
          <div v-if="errors.name" class="invalid-feedback">{{ errors.name }}</div>
        </div>

        <div class="mb-3 w-25">
          <label for="description" class="form-label">Description</label>
          <textarea v-model="description" type="text" placeholder="Enter a description (optional)" class="form-control" :class="{ 'is-invalid': errors.description }" />
          <div v-if="errors.description" class="invalid-feedback">{{ errors.description }}</div>
        </div>

        <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

      </form>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useRouter, useRoute } from 'vue-router';
  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  import { useUserStore } from '@/stores/user';

  const video = ref(null);
  const loading = ref(true);

  const toast = useToast();
  const userStore = useUserStore();
  const router = useRouter();
  const route = useRoute();
  const videoId = route.params.id;

  const pageTitle = ref(DEFAULT_TITLE);

  useHead({ title: pageTitle })

  watch(video, async (newVideo) => {
    pageTitle.value = 'Edit ' + newVideo.name + ' | ' + DEFAULT_TITLE;
  })

  onMounted(async () => {
    try {
      const response = await axios.get(`/api/Video/get?videoId=${videoId}`);
      video.value = response.data; 

      // Check if the logged-in user owns the video
      if (video.value.user.id !== userStore.user.id) {
        router.push({ name: '403 Forbidden' });
        return;
      }

      name.value = video.value.name;
      description.value = video.value.description;
    } catch (error) {
      let errorMessage = error.response?.data?.message;
      toast.error(errorMessage ?? 'An error occurred while loading the video.');
    } finally {
      loading.value = false;
    }
  })

  const schema = yup.object({
    name: yup.string().required('Video name is required.').max(255, 'Name cannot exceed 255 characters.'),
    description: yup.string().max(500, 'Description cannot exceed 500 characters.'),
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: name } = useField('name');
  const { value: description } = useField('description');

  const onSubmit = async (values) => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      id: video.value.id,
      name: name.value,
      description: description.value,
    };

    try {
      const response = await axios.put('/api/Video/update', formData);

      toast.success(response.data.message);

      router.push({ name: 'Video', params: { id: video.value.id } });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating video. Please try again.');
    }
  };
</script>
