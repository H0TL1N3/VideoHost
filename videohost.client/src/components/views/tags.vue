<template>
  <h1>Hello!</h1>

  <div>This is a sample <b>Tags</b> page.</div>
</template>

<script setup>
  import { useHead } from '@unhead/vue'

  import { DEFAULT_TITLE } from '@/assets/const.js'

  useHead({
    title: 'Tags | ' + DEFAULT_TITLE
  })
</script>
