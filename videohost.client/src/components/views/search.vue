<template>
  <form class="row row-cols-lg-auto align-items-center" @submit.prevent="onSubmit">

    <div class="col-12">
      <input v-model="searchTerm" type="text" placeholder="Enter your search term:" class="form-control" :class="{ 'is-invalid': errors.searchTerm }" />
      <div v-if="errors.searchTerm" class="invalid-feedback">{{ errors.searchTerm }}</div>
    </div>

    <div class="col-12">
      <select v-model="searchCategory" class="form-select" :class="{ 'is-invalid': errors.searchCategory }">
        <option value="video">Search Videos</option>
        <option value="user">Search Users</option>
      </select>
      <div v-if="errors.searchCategory" class="invalid-feedback">{{ errors.searchCategory }}</div>
    </div>

    <div class="col-12">
      <button type="submit" class="btn btn-primary">Search</button>
    </div>

  </form>

  <div v-if="validatedSearchCategory === 'video'">
    <video-list :page-title="'Search Videos'" :search-term="validatedSearchTerm" />
  </div>
  <div v-else-if="validatedSearchCategory === 'user'">
    <user-list :page-title="'Search Users'" :search-term="validatedSearchTerm" />
  </div>


</template>

<script setup>
  import { ref } from 'vue';

  import VideoList from '@/components/common/lists/video.vue';
  import UserList from '@/components/common/lists/user.vue';

  import { useToast } from 'vue-toast-notification';
  import { useField, useForm } from 'vee-validate';
  import * as yup from 'yup';

  const schema = yup.object({
    searchTerm: yup.string().max(255,'A search term may not exceed 255 characters'),
    searchCategory: yup.string().required('Select one of the types to search')
  });

  const { errors, validate } = useForm({
    validationSchema: schema,
  });

  const { value: searchTerm } = useField('searchTerm');
  const { value: searchCategory } = useField('searchCategory');

  const toast = useToast();

  const validatedSearchTerm = ref('');
  const validatedSearchCategory = ref('');

  searchCategory.value = 'video';

  const onSubmit = async () => {
    const formCheck = await validate();

    if (!formCheck.valid) {
      toast.error('Please fix your form before submitting.');
      return;
    }

    validatedSearchTerm.value = searchTerm.value;
    validatedSearchCategory.value = searchCategory.value;
  };
</script>
