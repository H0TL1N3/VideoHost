<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <h3 class="mb-3">Edit Video with ID {{ videoId }}</h3>
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

        <div class="mb-3 w-25">
          <label for="user" class="form-label">User</label>
          <multiselect v-model="user"
                       :options="availableUsers"
                       track-by="id"
                       label="displayName"
                       placeholder="Select a user"
                       :class="{ 'is-invalid': errors.user }" />
          <div v-if="errors.user" class="invalid-feedback">{{ errors.user }}</div>
        </div>

        <div class="mb-3 w-25">
          <select-tags v-model="selectedTags" />
        </div>

        <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

      </form>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue';

  import SelectTags from '@/components/common/forms/tag/select.vue';
  import Multiselect from 'vue-multiselect';

  import { useHead } from '@unhead/vue';
  import { useRouter, useRoute } from 'vue-router';
  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  useHead({
    title: 'Admin - Edit Video | ' + DEFAULT_TITLE
  })

  const schema = yup.object({
    name: yup.string().required('Video name is required.').max(255, 'Name cannot exceed 255 characters.'),
    description: yup.string().nullable().max(500, 'Description cannot exceed 500 characters.'),
    user: yup.object().required('User is required')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: name } = useField('name');
  const { value: description } = useField('description');
  const { value: user } = useField('user');

  const loading = ref(true);
  const selectedTags = ref([]);
  const availableUsers = ref([]);

  const toast = useToast();
  const router = useRouter();
  const route = useRoute();
  const videoId = route.params.id;  

  onMounted(async () => {
    try {
      const response = await axios.get('/api/Admin/get-entity', {
        params: {
          entityType: 'Videos',
          id: videoId
        }
      });

      name.value = response.data.name;
      description.value = response.data.description;
      user.value = response.data.users.find(user => user.id === response.data.userId);

      availableUsers.value = response.data.users;
      selectedTags.value = response.data.tags.map(tag => tag.id);      
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
      id: videoId,
      name: name.value,
      userId: user.value.id,
      description: description.value,
      tagIds: selectedTags.value
    };

    try {
      const response = await axios.put('/api/Admin/update-video', formData);

      toast.success(response.data.message);

      router.push({ name: 'Admin Dashboard' });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating video. Please try again.');
    }
  };
</script>
