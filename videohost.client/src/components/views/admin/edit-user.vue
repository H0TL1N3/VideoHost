<template>
  <div v-if="loading" class="text-center">Loading</div>
  <div v-else>
    <h3 class="mb-3">Edit User with ID {{ userId }}</h3>
    <form @submit.prevent="onSubmit">

      <div class="mb-3 w-25">
        <label for="username" class="form-label">Username</label>
        <input v-model="username" type="text" placeholder="Enter username" class="form-control" :class="{ 'is-invalid': errors.username }" />
        <div v-if="errors.username" class="invalid-feedback">{{ errors.username }}</div>
      </div>

      <div class="mb-3 w-25">
        <label for="role" class="form-label">Role</label>
        <select v-model="role" class="form-select">
          <option v-for="role in availableRoles" :key="role" :value="role">{{ role }}</option>
        </select>
        <div v-if="errors.role" class="invalid-feedback">{{ errors.role }}</div>
      </div>

      <div class="mb-3 w-25">
        <label for="email" class="form-label">Email</label>
        <input v-model="email" type="email" placeholder="Enter Email" class="form-control" :class="{ 'is-invalid': errors.email }" />
        <div v-if="errors.email" class="invalid-feedback">{{ errors.email }}</div>
      </div>

      <div class="mb-3 w-25">
        <label for="newPassword" class="form-label">New Password</label>
        <input v-model="newPassword" type="password" placeholder="Enter the new password" class="form-control" :class="{ 'is-invalid': errors.newPassword }" />
        <div v-if="errors.newPassword" class="invalid-feedback">{{ errors.newPassword }}</div>
      </div>

      <div class="mb-3 w-25">
        <label for="newConfirmPassword" class="form-label">Confirm Password</label>
        <input v-model="newConfirmPassword" type="password" placeholder="Confirm the new password" class="form-control" :class="{ 'is-invalid': errors.newConfirmPassword }" />
        <div v-if="errors.newConfirmPassword" class="invalid-feedback">{{ errors.newConfirmPassword }}</div>
      </div>

      <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

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
    title: 'Admin - Edit User | ' + DEFAULT_TITLE
  })

  const schema = yup.object({
    username: yup.string().required('Username is required').max(255, 'Name cannot exceed 255 characters.'),
    email: yup.string().email('Email must be valid'),
    newPassword: yup
      .string()
      .min(6, 'Password must be at least 6 characters')
      .matches(/\d/, 'Password must contain at least one digit')
      .matches(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .matches(/[a-z]/, 'Password must contain at least one lowercase letter'),
    newConfirmPassword: yup
      .string()
      .oneOf([yup.ref('newPassword')], 'Passwords must match'),
    role: yup
      .string()
      .required('Role is required')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: username } = useField('username');
  const { value: email } = useField('newEmail');
  const { value: newPassword } = useField('newPassword');
  const { value: newConfirmPassword } = useField('newConfirmPassword');
  const { value: role } = useField('role');

  const loading = ref(true);
  const availableRoles = ref([]);

  const toast = useToast();
  const router = useRouter();
  const route = useRoute();
  const userId = route.params.id;

  onMounted(async () => {
    try {
      const response = await axios.get('/api/Admin/get-entity', {
        params: {
          entityType: 'Users',
          id: userId
        }
      });

      username.value = response.data.displayName;
      email.value = response.data.email;
      role.value = response.data.role;

      availableRoles.value = response.data.roles;

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
      id: userId,
      displayName: username.value,
      role: role.value,
      email: email.value,
      newPassword: newPassword.value,
    };

    try {
      const response = await axios.put('/api/Admin/update-user', formData);

      toast.success(response.data.message);

      router.push({ name: 'Admin Dashboard' });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating user. Please try again.');
    }
  }
</script>
