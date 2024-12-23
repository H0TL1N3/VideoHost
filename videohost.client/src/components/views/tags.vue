<template>
  <form class="row row-cols-lg-auto align-items-center" @submit.prevent="onSubmit">

    <div class="col-8">
      <select-tags v-model="selectedTags" :show-label="showLabel"/>
    </div>

    <div class="col">
      <button type="submit" class="btn btn-primary">Search</button>
    </div>

  </form> 

  <video-list :page-title="'Tags'" :tag-ids="readyTags" />
</template>

<script setup>
  import { ref } from 'vue';

  import SelectTags from '@/components/common/forms/tag/select.vue';
  import VideoList from '@/components/common/lists/video.vue';

  import { useToast } from 'vue-toast-notification';

  const selectedTags = ref([]);
  const readyTags = ref([]);

  const showLabel = ref(false);

  const toast = useToast();

  const onSubmit = async () => {
    if (!selectedTags.value.length) {
      toast.error('Please select a single tag before proceeding.');
      return;
    }

    readyTags.value = selectedTags.value;
  };
</script>
