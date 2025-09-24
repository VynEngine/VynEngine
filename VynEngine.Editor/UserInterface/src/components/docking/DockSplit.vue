<script setup lang="ts">
import type {DockSplitNode, DockNode} from '@/@types/dockspace'
import DockSpace from './DockSpace.vue'
import {useDockSpaceStore} from "@/stores/dockspace.ts";

const store = useDockSpaceStore();
const props = defineProps<{ node: DockSplitNode }>();

function onSplitterDown(index: number, e: MouseEvent) {
  e.preventDefault();
  e.stopPropagation();

  const startX = e.clientX;
  const startY = e.clientY;
  const startSizes = [...props.node.sizes];
  const totalSize = props.node.direction === 'horizontal'
    ? (e.currentTarget as HTMLElement).parentElement!.clientWidth
    : (e.currentTarget as HTMLElement).parentElement!.clientHeight;

  function onMouseMove(e: MouseEvent) {
    const delta = props.node.direction === 'horizontal'
      ? e.clientX - startX
      : e.clientY - startY;
    const deltaRatio = delta / totalSize;
    const newSizes = [...startSizes];
    newSizes[index] = Math.max(0.1, startSizes[index] + deltaRatio);
    newSizes[index + 1] = Math.max(0.1, startSizes[index + 1] - deltaRatio);
    const sizeSum = newSizes.reduce((a, b) => a + b, 0);
    props.node.sizes = newSizes.map(s => s / sizeSum);
  }

  function onMouseUp() {
    window.removeEventListener('mousemove', onMouseMove);
    window.removeEventListener('mouseup', onMouseUp);

    store.save();
  }

  window.addEventListener('mousemove', onMouseMove);
  window.addEventListener('mouseup', onMouseUp);
}
</script>

<template>
  <div class="dock-split" :class="node.direction">
    <template v-for="(child, i) in node.children" :key="i">
      <div class="dock-child" :style="{ flex: node.sizes[i] }">
        <DockSpace :nodes="[child as DockNode]"/>
      </div>

      <div
        v-if="i < node.children.length - 1"
        class="splitter"
        :class="node.direction"
        @mousedown="onSplitterDown(i, $event)"
      >
        <div class="thumb"></div>
      </div>
    </template>
  </div>
</template>
