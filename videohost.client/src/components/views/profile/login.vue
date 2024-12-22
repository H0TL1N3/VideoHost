<template>
  <form @submit.prevent="onSubmit">

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

    <button type="submit" class="btn btn-primary">Login</button>

  </form>
</template>

<script setup>
  import { useHead } from '@unhead/vue';
  import * as yup from 'yup';
  import { useField, useForm } from 'vee-validate';
  import { useToast } from 'vue-toast-notification';
  import { useRouter } from 'vue-router';

  import { DEFAULT_TITLE } from '@/assets/const.js'

  import { useUserStore } from '@/stores/user';

  useHead({
    title: 'Login | ' + DEFAULT_TITLE
  }) 

  const schema = yup.object({
    email: yup.string().required('Email is required').email('Email must be valid'),
    password: yup
      .string()
      .required('Password is required')
      .min(6, 'Password must be at least 6 characters')
      .matches(/\d/, 'Password must contain at least one digit')
      .matches(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .matches(/[a-z]/, 'Password must contain at least one lowercase letter'),
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: email } = useField('email');
  const { value: password } = useField('password');

  const toast = useToast();
  const router = useRouter();
  const userStore = useUserStore();

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      email: email.value,
      password: password.value,
    };

    try {
      const response = await userStore.login(formData);

      toast.success(response.message);

      router.push({ name: 'Home' });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred during login.');
    }
  };
</script>
