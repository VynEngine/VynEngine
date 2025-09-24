<script setup lang="ts">
import type {DockNode, DockSplitNode, DockTabsNode} from "@/@types/dockspace.ts";
import DockSplit from "@/components/docking/DockSplit.vue";
import DockTabs from "@/components/docking/DockTabs.vue";

defineProps<{
  nodes: DockNode[];
}>();
</script>

<template>
  <div class="dockspace">
    <template v-for="(node, i) in nodes" :key="i">
      <DockSplit v-if="node.kind === 'split'" :node="node as DockSplitNode" />
      <DockTabs v-else-if="node.kind === 'tabs'" :node="node as DockTabsNode" />
    </template>
  </div>
</template>