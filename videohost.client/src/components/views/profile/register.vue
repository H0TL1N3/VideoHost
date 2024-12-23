<template>
  <form @submit.prevent="onSubmit">

    <div class="mb-3 w-25">
      <label for="username" class="form-label">Username</label>
      <input v-model="username" type="text" placeholder="Username" class="form-control" :class="{ 'is-invalid': errors.username }" />
      <div v-if="errors.username" class="invalid-feedback">{{ errors.username }}</div>
    </div>

    <div class="mb-3 w-25">
      <label for="email" class="form-label">Email</label>
      <input v-model="email" type="email" placeholder="Email" class="form-control" :class="{ 'is-invalid': errors.email }" />
      <div v-if="errors.email" class="invalid-feedback">{{ errors.email }}</div>
    </div>

    <div class="mb-3 w-25">
      <label for="password" class="form-label">Password</label>
      <input v-model="password" type="password" placeholder="Password" class="form-control" :class="{ 'is-invalid': errors.password }" />
      <div v-if="errors.password" class="invalid-feedback">{{ errors.password }}</div>
    </div>

    <div class="mb-3 w-25">
      <label for="confirmPassword" class="form-label">Confirm Password</label>
      <input v-model="confirmPassword" type="password" placeholder="Confirm Password" class="form-control" :class="{ 'is-invalid': errors.confirmPassword }" />
      <div v-if="errors.confirmPassword" class="invalid-feedback">{{ errors.confirmPassword }}</div>
    </div>

    <button type="submit" class="btn btn-primary">Register</button>

  </form>
</template>

<script setup>
  import { useHead } from '@unhead/vue';
  import * as yup from 'yup';
  import { useField, useForm } from 'vee-validate';
  import { useToast } from 'vue-toast-notification';

  import { DEFAULT_TITLE } from '@/assets/const.js'

  import { useUserStore } from '@/stores/user';

  useHead({
    title: 'Register | ' + DEFAULT_TITLE
  })

  const schema = yup.object({
    username: yup.string().required('Username is required').max(255, 'Name cannot exceed 255 characters.'),
    email: yup.string().required('Email is required').email('Email must be valid'),
    password: yup
      .string()
      .required('Password is required')
      .min(6, 'Password must be at least 6 characters')
      .matches(/\d/, 'Password must contain at least one digit')
      .matches(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .matches(/[a-z]/, 'Password must contain at least one lowercase letter'),
    confirmPassword: yup
      .string()
      .oneOf([yup.ref('password')], 'Passwords must match')
      .required('Confirm Password is required'),
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: username } = useField('username');
  const { value: email } = useField('email');
  const { value: password } = useField('password');
  const { value: confirmPassword } = useField('confirmPassword');

  const toast = useToast();
  const userStore = useUserStore();

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      username: username.value,
      email: email.value,
      password: password.value
    };

    try {
      const response = await userStore.register(formData);

      toast.success(response.message);

      router.push({ name: 'Login' }); 
    } catch (error) {
      let errorMessage = '';

      // Extract error messages from the response (if any)
      error.response?.data?.forEach(error => {
        errorMessage += error.description + ' ';
      })

      toast.error(errorMessage ?? 'An error occurred during registration.');
    }
  };
</script>
