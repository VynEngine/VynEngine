<script setup lang="ts">
import type {DockPane, DockTabsNode} from '@/@types/dockspace'
import {computed, ref} from 'vue'
import {useDockSpaceStore} from "@/stores/dockspace.ts";

const store = useDockSpaceStore();
const props = defineProps<{ node: DockTabsNode }>();

const showBar = computed(() => props.node.tabs.length > 1);
const active = computed(() => props.node.tabs.find(t => t.id === props.node.activeTab) ?? props.node.tabs[0]);

function onTabClick(id: string) {
  props.node.activeTab = id;
  store.save();
}

function onDragHandleDown(e: MouseEvent, pane: DockPane) {
  e.stopPropagation();

  store.dnd.dragging = pane;
  store.dnd.draggingParent = props.node;

  const startX = e.pageX;
  const startY = e.pageY;
  const cursorBefore = document.body.style.cursor;

  const dragElement = document.createElement('div');
  dragElement.classList.add('dock-dragghost');
  dragElement.classList.add('dock-tab');
  dragElement.classList.add('active');
  dragElement.style.position = 'absolute';
  dragElement.style.pointerEvents = 'none';
  dragElement.style.top = `${startY - 10}px`;
  dragElement.style.left = `${startX - 10}px`;
  dragElement.style.opacity = '0.8';
  dragElement.style.zIndex = '1000';
  dragElement.innerText = pane.title;

  document.body.appendChild(dragElement);
  document.body.style.cursor = 'grabbing';

  function onMouseMove(ev: MouseEvent) {
    dragElement.style.top = `${ev.pageY - 10}px`;
    dragElement.style.left = `${ev.pageX - 10}px`;
  }

  function onMouseUp() {
    if (store.dnd.draggingParent && store.dnd.dragging && store.dnd.dragOver && store.dnd.dragOverPos != '') {
      store.movePane(store.dnd.draggingParent, store.dnd.dragging, store.dnd.dragOver, store.dnd.dragOverPos as 'top' | 'bottom' | 'left' | 'right' | 'center');
      store.save();
    }

    store.dnd.draggingParent = null;
    store.dnd.dragging = null;
    store.dnd.dragOver = null;
    store.dnd.dragOverPos = '';

    document.body.removeChild(dragElement);
    document.body.style.cursor = cursorBefore;

    window.removeEventListener('mouseup', onMouseUp);
    window.removeEventListener('mousemove', onMouseMove);
  }

  window.addEventListener('mouseup', onMouseUp);
  window.addEventListener('mousemove', onMouseMove);
}
</script>

<template>
  <div class="dock-pane" :style="!showBar && store.dnd.dragging?.id === active?.id ? { display: 'none' } : {}">
    <div v-if="showBar" class="dock-tabbar">
      <div
        v-for="t in node.tabs"
        :key="t.id"
        class="dock-tab"
        :class="{ active: t.id === node.activeTab }"
        :style="store.dnd.dragging?.id === t.id ? { display: 'none' } : {}"
        @click="onTabClick(t.id)"
        @mousedown="onDragHandleDown($event, t)"
      >
        {{ t.title }}
      </div>
    </div>
    <div v-else class="dock-draghandle" @mousedown="onDragHandleDown($event, active)"></div>

    <div class="dock-content">
      <component v-if="active" :is="active.content" :style="store.dnd.dragging?.id === active?.id ? { display: 'none' } : {}" v-bind="{ pane: active }"/>
    </div>
  </div>
</template>
