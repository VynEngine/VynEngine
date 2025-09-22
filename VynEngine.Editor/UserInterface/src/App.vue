<script setup lang="ts">
import Header from "@/components/layout/Header.vue";
import ResizeHandles from "@/components/layout/ResizeHandles.vue";
import ToastContainer from "@/components/container/ToastContainer.vue";
import DialogContainer from "@/components/container/DialogContainer.vue";
import ProgressContainer from "@/components/container/ProgressContainer.vue";

</script>

<template>
  <Header />

  <ProgressContainer />
  <DialogContainer />
  <ToastContainer />

  <ResizeHandles />
</template>

<style scoped>

</style>
