<template>
  <div>
    <label for="tagSelect" class="mb-2">Select Tags</label>
    <multiselect id="tagSelect"
                 v-model="selectedTagObjects"
                 :options="tags"
                 :multiple="true"
                 :close-on-select="false"
                 :track-by="'id'"
                 :label="'name'"
                 @update="updateTagIds" />
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';

  import Multiselect from 'vue-multiselect';
  import { useToast } from 'vue-toast-notification';

  import axios from 'axios';

  const tags = ref([]);
  const selectedTags = defineModel();
  const selectedTagObjects = ref([]); 

  const toast = useToast();

  onMounted(async () => {
    await loadTags();
    initializeSelectedTagObjects();
  });

  const loadTags = async () => {
    try {
      const response = await axios.get('/api/Tag/get');

      tags.value = response.data;
    } catch (error) {
      let errorMessage = error.response?.data?.message;

      toast.error(errorMessage ?? 'Error loading tags. Please try again.');
    }
  };

  // When passing to the component
  const initializeSelectedTagObjects = () => {
    selectedTagObjects.value = tags.value.filter((tag) => selectedTags.value.includes(tag.id));
  };

  // When passing from the component
  const updateTagIds = () => {
    selectedTags.value = selectedTagObjects.value.map((tag) => tag.id);
  };

  defineExpose({
    loadTags
  });
</script>
