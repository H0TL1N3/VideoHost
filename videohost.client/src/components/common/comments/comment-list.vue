<template>
  <div class="comment-list">
    <div v-for="comment in comments" :key="comment.id" class="comment mb-2">
      <div class="d-flex justify-content-between">
        <p class="fw-bold">{{ comment.userName }}</p>
        <div v-if="userStore.isAuthenticated && canEditOrDelete(comment.userId)" class="actions">
          <button class="btn btn-link p-0" @click="editComment(comment.id)">Edit</button>
          <button class="btn btn-link p-0 text-danger" @click="deleteComment(comment.id)">Delete</button>
        </div>
        <div v-else-if="userStore.isAuthenticated && canDelete(comment.userId)">
          <button class="btn btn-link p-0 text-danger" @click="deleteComment(comment.id)">Delete</button>
        </div>
      </div>
      <p>{{ comment.content }}</p>
      <small class="text-muted">{{ new Date(comment.creationDate).toLocaleString() }}</small>
    </div>

    <div class="text-center">
      <button v-if="hasMoreComments" class="btn btn-outline-secondary" @click="loadComments">
        Load More
      </button>
    </div>
  </div>
</template>

<script setup>
  import { ref, defineExpose } from 'vue';

  import { useToast } from 'vue-toast-notification';
  import axios from 'axios';

  import { useUserStore } from '@/stores/user';

  const props = defineProps({
    videoId: {
      type: Number,
      required: true
    },
    ownerId: {
      type: Number,
      required: true
    }
  });

  const comments = ref([]);
  const skip = ref(0);
  const take = 10;
  const hasMoreComments = ref(true);

  const toast = useToast();
  const userStore = useUserStore();

  const refreshComments = async () => {
    // Reset state
    skip.value = 0;
    comments.value = [];
    hasMoreComments.value = true;

    loadComments();
  }

  const loadComments = async () => {
    try {
      const response = await axios.get("/api/Comment/get", {
        params: { videoId: props.videoId, skip: skip.value, take },
      });

      if (response.data.length < take)
        hasMoreComments.value = false;

      comments.value.push(...response.data);

      skip.value += take;
    } catch (error) {
      let errorMessage = error.response?.data?.message;
      toast.error(errorMessage ?? 'An error occurred while loading the comments.');
    }
  };

  loadComments();

  const canEditOrDelete = (commentUserId) => {
    return userStore.user.id === commentUserId;
  };

  const canDelete = (commentUserId) => {
    return userStore.user.id === commentUserId || userStore.user.id === props.ownerId;
  };

  const editComment = async (commentId) => {
    const newContent = prompt('Edit your comment:');

    if (!newContent) {
      toast.error('A comment cannot be empty');
      return;
    }

    const formData = {
      id: commentId,
      content: newContent
    }

    try {
      const response = await axios.put(`/api/Comment/update`, formData);

      const comment = comments.value.find((c) => c.id === commentId);
      if (comment) {
        comment.content = newContent;
      }

      toast.success(response.data.message);
    } catch (error) {
      let errorMessage = error.response?.data?.message;
      toast.error(errorMessage ?? 'An error occurred while editing the comment.');
    }
  };

  const deleteComment = async (id) => {
    if (confirm('Are you sure you want to delete this comment?')) {
      try {
        const response = await axios.delete(`/api/Comment/delete?id=${id}`);

        comments.value = comments.value.filter((c) => c.id !== id);

        toast.success(response.data.message);
      } catch (error) {
        let errorMessage = error.response?.data?.message;

        toast.error(errorMessage ?? 'An error occurred while deleting the comment.');
      }
    }
  };

  defineExpose({
    refreshComments
  })
</script>
