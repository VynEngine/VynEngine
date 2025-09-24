<script setup lang="ts">
import type {DockTabsNode} from '@/@types/dockspace'
import {computed} from 'vue'

const props = defineProps<{ node: DockTabsNode }>();
const showBar = computed(() => props.node.tabs.length > 1);
const active = computed(() => props.node.tabs.find(t => t.id === props.node.activeTab) ?? props.node.tabs[0]);

function onTabClick(id: string) {
  props.node.activeTab = id;
}

function onDragHandleDown(e: MouseEvent) {
  e.stopPropagation();
}
</script>

<template>
  <div class="dock-pane">
    <div v-if="showBar" class="dock-tabbar">
      <div
        v-for="t in node.tabs"
        :key="t.id"
        class="dock-tab"
        :class="{ active: t.id === node.activeTab }"
        @click="onTabClick(t.id)"
      >
        {{ t.title }}
      </div>
    </div>

    <div class="dock-content">
      <component v-if="active" :is="active.content"/>
    </div>
  </div>
</template>
