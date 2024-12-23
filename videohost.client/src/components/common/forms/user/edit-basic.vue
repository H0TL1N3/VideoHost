<template>
  <div>
    <h3 class="mb-3">Edit your info</h3>
    <form @submit.prevent="onSubmit">

      <div>
        <label for="username" class="form-label">Username</label>
        <input v-model="username" type="text" placeholder="Enter your username" class="form-control" :class="{ 'is-invalid': errors.username }" />
        <div v-if="errors.username" class="invalid-feedback">{{ errors.username }}</div>
      </div>

      <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

    </form>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';

  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  import { useUserStore } from '@/stores/user';

  const loading = ref(true);

  const toast = useToast();
  const userStore = useUserStore();

  const props = defineProps({
    user: {
      type: Object,
      required: true
    }
  });

  const schema = yup.object({
    username: yup.string().required('Username is required').max(255, 'Name cannot exceed 255 characters.')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: username } = useField('username');

  onMounted(async () => {
    try {
      username.value = props.user.displayName;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while loading the user.');
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
      username: username.value
    };

    try {
      const response = await axios.put('/api/User/update-display-name', formData);

      toast.success(response.data.message);
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating user. Please try again.');
    }
  };
</script>
