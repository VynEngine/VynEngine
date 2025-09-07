<script setup lang="ts">
import {useWindowService} from "@/rpc/services.ts";
import {computed, onMounted, onUnmounted, ref} from "vue";
import {off, on} from "@/rpc";

const fromYear = 2025;
const defaultSubtitle = "Visual Novel Engine for simple ease of use";

const windowService = useWindowService();

const showWindowButtons = ref(false);
const isMaximized = ref(false);
const currentSubtitle = ref(defaultSubtitle);

const copyrightYear = computed(() => {
  const currentYear = new Date().getFullYear();
  return currentYear > fromYear ? `${fromYear}-${currentYear}` : `${fromYear}`;
});

onMounted(async () => {
  on("window", "updateMaximized", onMaximizedChanged);
  on("window", "updateSubtitle", onChangeSubtitle);

  showWindowButtons.value = await windowService.isWindows();
});

onUnmounted(() => {
  off("window", "updateMaximized", onMaximizedChanged);
  off("window", "updateSubtitle", onChangeSubtitle);
});

function onMaximizedChanged(maximized: boolean) {
  isMaximized.value = maximized;
}

function onChangeSubtitle(newSubtitle: string) {
  currentSubtitle.value = newSubtitle.trim().length <= 0 ? defaultSubtitle : newSubtitle;
}
</script>

<template>
  <div class="app-header" @mousedown.self="windowService.beginDrag()">
    <div class="header-left">
      <img src="/favicon.ico" alt="logo" class="logo"/>
      <div class="menu">File</div>
      <div class="menu">Edit</div>
      <div class="menu">View</div>
    </div>

    <div class="header-center">
      <div class="title">VynEngine (c) DasDarki {{copyrightYear}}</div>
      <div class="subtitle">{{currentSubtitle}}</div>
    </div>

    <div v-show="showWindowButtons" class="header-right">
      <button @click="windowService.setMinimized(true)">
        <i class="fa-solid fa-minus"/>
      </button>
      <button @click="windowService.setMaximized(isMaximized = !isMaximized)">
        <i v-if="!isMaximized" class="fa-solid fa-up-right-and-down-left-from-center"/>
        <i v-else class="fa-solid fa-down-left-and-up-right-to-center"/>
      </button>
      <button class="close" @click="windowService.close()">
        <i class="fa-solid fa-xmark"/>
      </button>
    </div>
  </div>
</template>

<style scoped>

</style>