<template>
  <div>
    <h3>Create a New Tag</h3>
    <form @submit.prevent="onSubmit">

      <div class="mb-3">
        <label for="name" class="form-label">Name</label>
        <input v-model="name" type="text" placeholder="Enter name" class="form-control" :class="{ 'is-invalid': errors.name }" />
        <div v-if="errors.name" class="invalid-feedback">{{ errors.name }}</div>
      </div>

      <button type="submit" class="btn btn-primary mt-3">Create</button>
    </form>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  const schema = yup.object({
    name: yup.string().required('Tag name is required.').max(20, 'Tag name cannot exceed 20 characters.')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: name } = useField('name');
  const { value: description } = useField('description');

  const toast = useToast();

  const emit = defineEmits(['tag-created']);

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      name: name.value
    };

    try {
      const response = await axios.post('/api/Tag/add', formData);

      emit('tag-created');

      toast.success(response.data.message);

      name.value = '';
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error creating tag. Please try again.');
    }
  };
</script>
