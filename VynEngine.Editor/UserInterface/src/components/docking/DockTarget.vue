<script setup lang="ts">
import {useDockSpaceStore} from "@/stores/dockspace.ts";
import type {DockNode} from "@/@types/dockspace.ts";
import {onUnmounted} from "vue";

const store = useDockSpaceStore();

const props = defineProps<{
  node: DockNode;
}>();

const model = defineModel<boolean>();

onUnmounted(() => {
  model.value = false;
});

function onMouseEnter(e: MouseEvent, pos: 'top' | 'bottom' | 'left' | 'right' | 'center') {
  e.preventDefault();
  e.stopPropagation();

  model.value = true;
  store.dnd.dragOver = props.node;
  store.dnd.dragOverPos = pos;

  function onMouseLeave() {
    if (store.dnd.dragOver === props.node) {
      model.value = false;

      store.dnd.dragOver = null;
      store.dnd.dragOverPos = "";
    }

    e.currentTarget?.removeEventListener('mouseleave', onMouseLeave);
  }

  e.currentTarget?.addEventListener('mouseleave', onMouseLeave);
}
</script>

<template>
  <div class="dock-target">
    <div class="row">
      <div class="target" @mouseenter="onMouseEnter($event, 'top')"/>
    </div>
    <div class="row">
      <div class="target" @mouseenter="onMouseEnter($event, 'left')"/>
      <div class="target" @mouseenter="onMouseEnter($event, 'center')"/>
      <div class="target" @mouseenter="onMouseEnter($event, 'right')"/>
    </div>
    <div class="row">
      <div class="target" @mouseenter="onMouseEnter($event, 'bottom')"/>
    </div>
  </div>
</template>