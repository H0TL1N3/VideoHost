<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <h3 class="mb-3">Edit Subscription with ID {{ subscriptionId }}</h3>
      <form @submit.prevent="onSubmit">

        <div class="mb-3 w-25">
          <label for="subscriber" class="form-label">Subscriber User</label>
          <multiselect v-model="subscriber"
                       :options="availableUsers"
                       track-by="id"
                       label="displayName"
                       placeholder="Select a subscriber user"
                       :class="{ 'is-invalid': errors.subscriber  }" />
          <div v-if="errors.subscriber" class="invalid-feedback">{{ errors.subscriber }}</div>
        </div>

        <div class="mb-3 w-25">
          <label for="subscribedTo" class="form-label">SubscribedTo User</label>
          <multiselect v-model="subscribedTo"
                       :options="availableUsers"
                       track-by="id"
                       label="displayName"
                       placeholder="Select a subscribedTo user"
                       :class="{ 'is-invalid': errors.subscribedTo  }" />
          <div v-if="errors.subscribedTo" class="invalid-feedback">{{ errors.subscribedTo }}</div>
        </div>

        <button type="submit" class="btn btn-primary mt-3">Submit Changes</button>

      </form>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue';

  import Multiselect from 'vue-multiselect';

  import { useHead } from '@unhead/vue';
  import { useRouter, useRoute } from 'vue-router';
  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  useHead({
    title: 'Admin - Edit Subscription | ' + DEFAULT_TITLE
  })

  const schema = yup.object({
    subscriber: yup.object().required('Subscriber is required'),
    subscribedTo: yup.object().required('SubscribedTo is required')
      .test('not-equal', 'SubscribedTo must not be the same as Subscriber', function (value) {
        return value !== this.parent.subscriber;
      })
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: subscriber } = useField('subscriber');
  const { value: subscribedTo } = useField('subscribedTo');

  const loading = ref(true);
  const availableUsers = ref([]);

  const toast = useToast();
  const router = useRouter();
  const route = useRoute();
  const subscriptionId = route.params.id;

  onMounted(async () => {
    try {
      const response = await axios.get('/api/Admin/get-entity', {
        params: {
          entityType: 'Subscriptions',
          id: subscriptionId
        }
      });

      subscriber.value = response.data.users.find(user => user.id === response.data.subscriber.id);
      subscribedTo.value = response.data.users.find(user => user.id === response.data.subscribedTo.id);

      availableUsers.value = response.data.users;
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
      id: subscriptionId,
      subscriberId: subscriber.value.id,
      subscribedToId: subscribedTo.value.id
    };

    try {
      const response = await axios.put('/api/Admin/update-subscription', formData);

      toast.success(response.data.message);

      router.push({ name: 'Admin Dashboard' });
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error updating subscription. Please try again.');
    }
  };
</script>
