<template>
  <div v-if="loading" class="text-center">Loading</div>
  <div v-else>
    <h3 class="mb-3">Edit Tag with ID {{ tagId }}</h3>
    <form @submit.prevent="onSubmit">

      <div class="mb-3 w-25">
        <label for="name" class="form-label">Name</label>
        <input v-model="name" type="text" placeholder="Enter name" class="form-control" :class="{ 'is-invalid': errors.name }" />
        <div v-if="errors.name" class="invalid-feedback">{{ errors.name }}</div>
      </div>

      <button type="submit" class="btn btn-primary mt-3">Submit</button>

    </form>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useToast } from 'vue-toast-notification';
  import { useRouter, useRoute } from 'vue-router';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  useHead({
    title: 'Admin - Edit Tag | ' + DEFAULT_TITLE
  })

  const schema = yup.object({
    name: yup.string().required('Tag name is required.').max(20, 'Tag name cannot exceed 20 characters.')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: name } = useField('name');

  const loading = ref(true);

  const toast = useToast();
  const router = useRouter();
  const route = useRoute();
  const tagId = route.params.id;

  onMounted(async () => {
    try {
      const response = await axios.get('/api/Admin/get-entity', {
        params: {
          entityType: 'Tags',
          id: tagId
        }
      });

      name.value = response.data.name;

    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the tag data.');
    } finally {
      loading.value = false;
    }
  });

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      id: tagId,
      name: name.value
    };

    try {
      const response = await axios.put('/api/Admin/update-tag', formData);

      toast.success(response.data.message);

      router.push({ name: 'Admin Dashboard' });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating tag. Please try again.');
    }
  }
</script>
