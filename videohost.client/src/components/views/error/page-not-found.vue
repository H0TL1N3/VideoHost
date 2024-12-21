<template>
  <h1>Sorry!</h1>

  <div><b>Error 404:</b> this page does not exist.</div>
</template>

<script setup>
  import { useHead } from '@unhead/vue'

  import { DEFAULT_TITLE } from '@/assets/const.js'

  useHead({
    title: '404 Not Found | ' + DEFAULT_TITLE
  })
</script>
