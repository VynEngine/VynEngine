<script setup lang="ts">
import Header from "@/components/layout/Header.vue";
import ResizeHandles from "@/components/layout/ResizeHandles.vue";
import ToastContainer from "@/components/container/ToastContainer.vue";
import DialogContainer from "@/components/container/DialogContainer.vue";
import ProgressContainer from "@/components/container/ProgressContainer.vue";
import DockSpace from "@/components/docking/DockSpace.vue";
import {useDockSpaceStore} from "@/stores/dockspace.ts";

const dockSpaceStore = useDockSpaceStore();
</script>

<template>
  <Header />

  <main>
    <DockSpace v-for="node in dockSpaceStore.nodes" :node="node" />
  </main>

  <ProgressContainer />
  <DialogContainer />
  <ToastContainer />

  <ResizeHandles />
</template>