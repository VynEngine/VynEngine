<script setup lang="ts">
import type {DockNode, DockSplitNode, DockTabsNode} from "@/@types/dockspace.ts";
import DockSplit from "@/components/docking/DockSplit.vue";
import DockTabs from "@/components/docking/DockTabs.vue";
import DockTarget from "@/components/docking/DockTarget.vue";
import {useDockSpaceStore} from "@/stores/dockspace.ts";
import {ref} from "vue";

const store = useDockSpaceStore();

defineProps<{
  node: DockNode;
}>();

const selectHover = ref(false);
</script>

<template>
  <div class="dockspace" :class="{ hovered: selectHover }">
    <DockSplit v-if="node.kind === 'split'" :node="node as DockSplitNode" />
    <DockTabs v-else-if="node.kind === 'tabs'" :node="node as DockTabsNode" />

    <div class="dock-hover"/>
    <DockTarget v-if="store.dnd.dragging && store.dnd.draggingParent !== node" :node="node" v-model="selectHover"/>
  </div>
</template>