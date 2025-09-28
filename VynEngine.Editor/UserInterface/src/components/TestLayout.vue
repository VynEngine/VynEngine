<script setup lang="ts">
import type {DockPane} from "@/@types/dockspace.ts";

defineProps<{
  pane: DockPane
}>();
</script>

<template>
  <div style="width: 100%; height: 100%">
    <h1>Test: {{pane.title}}</h1>
  </div>
</template>

<style scoped lang="scss">

</style>