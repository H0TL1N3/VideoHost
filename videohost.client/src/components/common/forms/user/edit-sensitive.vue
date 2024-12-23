<template>
  <div>
    <h3 class="mb-3">Edit your sensitive info</h3>
    <form @submit.prevent="onSubmit">

      <div>
        <label for="oldPassword" class="form-label">Old Password</label>
        <input v-model="oldPassword" type="password" placeholder="Enter your old password" class="form-control" :class="{ 'is-invalid': errors.oldPassword }" />
        <div v-if="errors.oldPassword" class="invalid-feedback">{{ errors.oldPassword }}</div>
      </div>

      <div>
        <label for="newEmail" class="form-label">New Email</label>
        <input v-model="newEmail" type="email" placeholder="Enter your new Email" class="form-control" :class="{ 'is-invalid': errors.newEmail }" />
        <div v-if="errors.newEmail" class="invalid-feedback">{{ errors.newEmail }}</div>
      </div>

      <div>
        <label for="newPassword" class="form-label">New Password</label>
        <input v-model="newPassword" type="password" placeholder="Enter your new password" class="form-control" :class="{ 'is-invalid': errors.newPassword }" />
        <div v-if="errors.newPassword" class="invalid-feedback">{{ errors.newPassword }}</div>
      </div>

      <div>
        <label for="newConfirmPassword" class="form-label">Confirm Password</label>
        <input v-model="newConfirmPassword" type="password" placeholder="Confirm your new password" class="form-control" :class="{ 'is-invalid': errors.newConfirmPassword }" />
        <div v-if="errors.newConfirmPassword" class="invalid-feedback">{{ errors.newConfirmPassword }}</div>
      </div>

      <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

    </form>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';

  import { useToast } from 'vue-toast-notification';
  import { useRouter } from 'vue-router';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  import { useUserStore } from '@/stores/user';

  const loading = ref(true);

  const router = useRouter();
  const toast = useToast();
  const userStore = useUserStore();

  const props = defineProps({
    user: {
      type: Object,
      required: true
    }
  });

  const schema = yup.object({
    oldPassword: yup
      .string()
      .required('Password is required')
      .min(6, 'Password must be at least 6 characters')
      .matches(/\d/, 'Password must contain at least one digit')
      .matches(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .matches(/[a-z]/, 'Password must contain at least one lowercase letter'),
    newEmail: yup.string().email('Email must be valid'),
    newPassword: yup
      .string()
      .min(6, 'Password must be at least 6 characters')
      .matches(/\d/, 'Password must contain at least one digit')
      .matches(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .matches(/[a-z]/, 'Password must contain at least one lowercase letter'),
    newConfirmPassword: yup
      .string()
      .oneOf([yup.ref('newPassword')], 'Passwords must match'),
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: oldPassword } = useField('oldPassword');
  const { value: newEmail } = useField('newEmail');
  const { value: newPassword } = useField('newPassword');
  const { value: newConfirmPassword } = useField('newConfirmPassword');

  onMounted(async () => {
    try {
      newEmail.value = props.user.email;
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
      id: props.user.id,
      oldPassword: oldPassword.value,
      newEmail: newEmail.value,
      newPassword: newPassword.value
    };

    try {
      const response = await axios.put('/api/User/update-sensitive-data', formData);

      userStore.logout();

      toast.success(response.data.message);

      router.push({ name: 'Home' });    
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating user. Please try again.');
    }
  };
</script>
