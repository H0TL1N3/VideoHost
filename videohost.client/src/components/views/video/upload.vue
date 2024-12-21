<template>
  <div>
    <form @submit.prevent="onSubmit">

      <div class="mb-3 w-25">
        <label for="name" class="form-label">Video Name</label>
        <input v-model="name" type="text" placeholder="Enter video name" class="form-control" :class="{ 'is-invalid': errors.name }" />
        <div v-if="errors.name" class="invalid-feedback">{{ errors.name }}</div>
      </div>

      <div class="mb-3 w-25">
        <label for="description" class="form-label">Description</label>
        <textarea v-model="description" type="text" placeholder="Enter a description (optional)" class="form-control" :class="{ 'is-invalid': errors.description }"/>
        <div v-if="errors.description" class="invalid-feedback">{{ errors.description }}</div>
      </div>

      <div class="mb-3 w-25">
        <label for="videoFile" class="form-label">Upload Video</label>
        <input type="file" class="form-control" @change="handleFileChange" ref="fileInput" />
        <small class="text-muted">Maximum size: 500MB</small>
      </div>

      <button type="submit" class="btn btn-primary mt-3" :disabled="isUploading">
        {{ isUploading ? 'Uploading...' : 'Upload' }}
      </button>

    </form>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useRouter } from 'vue-router';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  import { useUserStore } from '@/stores/user';

  useHead({
    title: 'Upload Video | ' + DEFAULT_TITLE
  })

  const uploadStatus = ref('');
  const isUploading = ref(false);

  const videoFile = ref(null);

  const toast = useToast();
  const router = useRouter();
  const userStore = useUserStore();

  const schema = yup.object({
    name: yup.string().required('Video name is required.').max(255, 'Name cannot exceed 255 characters.'),
    description: yup.string().max(500, 'Description cannot exceed 500 characters.'),
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: name } = useField('name');
  const { value: description } = useField('description');

  const fileInput = ref(null);

  const clearFormFile = () => {
    videoFile.value = null;
    if (fileInput.value)
      fileInput.value.value = '';
  }

  const handleFileChange = (event) => {
    const file = event.target.files[0];

    if (file) {
      const allowedTypes = ['video/mp4'];

      if (!allowedTypes.includes(file.type)) {
        toast.error('Only MP4 video files are allowed.');
        clearFormFile();
      } else if (file.size > 500 * 1024 * 1024) {
        toast.error('File size exceeds the 500MB limit.');
        clearFormFile();
      } else {
        videoFile.value = file;
      }
    } else {
      toast.error('Please select a video file.');
      clearFormFile();
    }
  };

  const onSubmit = async (values) => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    if (!videoFile.value) {
      toast.error('Please upload a video file.');
      return;
    }

    const formData = {
      name: name.value,
      description: description.value,
      videoFile: videoFile.value,
      userId: userStore.user.id
    };

    try {
      isUploading.value = true;

      const response = await axios.post('/api/Video/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        }
      });

      toast.success(response.data.message);

      isUploading.value = false;

      router.push({ name: 'Video', params: { id: response.data.videoId } });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error uploading video. Please try again.');

      isUploading.value = false;
    }
  };
</script>
