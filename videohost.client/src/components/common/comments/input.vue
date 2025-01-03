<template>
  <div>
    <div v-if="userStore.isAuthenticated" class="comment-input mb-3">
      <form @submit.prevent="onSubmit">

        <label for="content" class="form-label">Your Comment</label>
        <textarea v-model="content"
                  class="form-control mb-2"
                  rows="3"
                  placeholder="Add a comment"
                  :class="{ 'is-invalid': errors.content }"></textarea>
        <div v-if="errors.content" class="invalid-feedback">{{ errors.content }}</div>

        <button class="btn btn-primary" :disabled="!content">
          Post Comment
        </button>

      </form>
    </div>
    <div v-else>
      <p>Please, login to leave comments under this video.</p>
    </div>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  import { useField, useForm } from 'vee-validate';
  import { useToast } from 'vue-toast-notification';
  import * as yup from 'yup';
  import axios from 'axios';

  import { useUserStore } from '@/stores/user';

  const props = defineProps({
    videoId: {
      type: Number,
      required: true,
    },
  });

  const schema = yup.object({
    content: yup.string().required('A comment is required.').max(500, 'The comment cannot exceed 500 characters.')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: content } = useField('content');

  const emit = defineEmits(['commentAdded']);
  const toast = useToast();
  const userStore = useUserStore();

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix the errors in the form before submitting.');
      return;
    }

    const formData = {
      content: content.value,
      videoId: props.videoId
    };

    try {
      const response = await axios.post('/api/Comment/add', formData);

      toast.success(response.data.message);

      content.value = '';

      emit('commentAdded');
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'An error occurred while adding the comment.');
    }
  };
</script>
