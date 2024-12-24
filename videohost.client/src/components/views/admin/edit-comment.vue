<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <h3 class="mb-3">Edit Comment with ID {{ commentId }}</h3>
      <form @submit.prevent="onSubmit">

        <div class="mb-3 w-25">
          <label for="user" class="form-label">User</label>
          <multiselect v-model="user"
                       :options="availableUsers"
                       track-by="id"
                       label="displayName"
                       placeholder="Select a user"
                       :class="{ 'is-invalid': errors.user  }" />
          <div v-if="errors.user" class="invalid-feedback">{{ errors.user }}</div>
        </div>

        <div class="mb-3 w-25">
          <label for="video" class="form-label">Video</label>
          <multiselect v-model="video"
                       :options="availableVideos"
                       track-by="id"
                       label="name"
                       placeholder="Select a video"
                       :class="{ 'is-invalid': errors.video  }" />
          <div v-if="errors.video" class="invalid-feedback">{{ errors.video }}</div>
        </div>

        <div class="mb-3 w-25">
          <label for="content" class="form-label">Content</label>
          <textarea v-model="content" type="text" placeholder="Enter the comment's content" class="form-control" :class="{ 'is-invalid': errors.content }" />
          <div v-if="errors.content" class="invalid-feedback">{{ errors.content }}</div>
        </div>

        <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

      </form>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue';

  import Multiselect from 'vue-multiselect';

  import { useHead } from '@unhead/vue';
  import { useRouter, useRoute } from 'vue-router';
  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  useHead({
    title: 'Admin - Edit Comment | ' + DEFAULT_TITLE
  })

  const schema = yup.object({
    user: yup.object().required('User is required'),
    video: yup.object().required('Video is required'),
    content: yup.string().required('A comment is required.').max(500, 'The comment cannot exceed 500 characters.')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: user } = useField('user');
  const { value: video } = useField('video');
  const { value: content } = useField('content');

  const loading = ref(true);
  const availableUsers = ref([]);
  const availableVideos = ref([]);

  const toast = useToast();
  const router = useRouter();
  const route = useRoute();
  const commentId = route.params.id;

  onMounted(async () => {
    try {
      const response = await axios.get('/api/Admin/get-entity', {
        params: {
          entityType: 'Comments',
          id: commentId
        }
      });

      content.value = response.data.content;
      user.value = response.data.users.find(user => user.id === response.data.user.id);
      video.value = response.data.videos.find(video => video.id === response.data.video.id);

      availableUsers.value = response.data.users;
      availableVideos.value = response.data.videos;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the video.');
    } finally {
      loading.value = false;
    }
  })

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      id: commentId,
      userId: user.value.id,
      videoId: video.value.id,
      content: content.value
    };

    try {
      const response = await axios.put('/api/Admin/update-comment', formData);

      toast.success(response.data.message);

      router.push({ name: 'Admin Dashboard' });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating comment. Please try again.');
    }
  };
</script>
