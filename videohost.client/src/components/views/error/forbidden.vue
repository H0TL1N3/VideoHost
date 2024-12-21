<template>
  <h1>Sorry!</h1>

  <div><b>Error 403:</b> access is forbidden.</div>
</template>

<script setup>
  import { useHead } from '@unhead/vue'

  import { DEFAULT_TITLE } from '@/assets/const.js'

  useHead({
    title: '403 Forbidden | ' + DEFAULT_TITLE
  })
</script>
