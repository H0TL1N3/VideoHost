<template>
  <div class="comment-section">
    <comment-input :video-id="videoId" @commentAdded="refreshComments" />
    <comment-list :video-id="videoId" :owner-id="ownerId" ref="comment-list"/>
  </div>
</template>

<script setup>
  import { useTemplateRef } from 'vue';

  import CommentInput from './comment-input.vue';
  import CommentList from './comment-list.vue';

  const props = defineProps({
    videoId: {
      type: Number,
      required: true,
    },
    ownerId: {
      type: Number,
      required: true
    }
  });

  const commentList = useTemplateRef('comment-list');

  const refreshComments = () => {
    commentList.value.refreshComments();
  };
</script>
