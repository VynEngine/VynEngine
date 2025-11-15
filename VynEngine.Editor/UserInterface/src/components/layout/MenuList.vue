<script setup lang="ts">
import {computed} from "vue";

export interface HeaderMenu {
  id: string;
  label?: string;
  hotkey?: string;
  icon?: string;
  iconColor?: string;
  disabled?: boolean;
  type?: 'item' | 'separator';
  children?: HeaderMenu[];
  open?: boolean;
}

const props = defineProps<{ items: HeaderMenu[] }>();
const emit = defineEmits<{ (e: 'select', id: string): void }>();

const hasOneIcon = computed(() => props.items.some(it => !!it.icon));

function onClick(it: HeaderMenu) {
  if (it.disabled) return;
  if (!it.children || it.children.length === 0) emit('select', it.id);
}

const closeTimers = new WeakMap<HeaderMenu, number>();
const CLOSE_DELAY = 120;

function cancelClose(it: HeaderMenu) {
  const t = closeTimers.get(it);
  if (t != null) {
    clearTimeout(t);
    closeTimers.delete(it);
  }
}

function scheduleClose(it: HeaderMenu) {
  cancelClose(it);
  const id = window.setTimeout(() => {
    closeItem(it);
    closeTimers.delete(it);
  }, CLOSE_DELAY);
  closeTimers.set(it, id);
}

function openItem(it: HeaderMenu) {
  for (const s of props.items) if (s !== it) {
    s.open = false;
    cancelClose(s);
  }
  cancelClose(it);
  it.open = true;
}

function closeAll(list: HeaderMenu[]) {
  for (const c of list) {
    c.open = false;
    cancelClose(c);
    if (c.children) closeAll(c.children);
  }
}

function closeItem(it: HeaderMenu) {
  it.open = false;
  if (it.children) closeAll(it.children);
}
</script>

<template>
  <ul class="menu-list" role="menu">
    <li
      v-for="it in items"
      :key="it.id"
      class="menu-item"
      role="none"
      @pointerenter="openItem(it); cancelClose(it)"
      @pointerleave="scheduleClose(it)"
    >
      <div v-if="it.type === 'separator'" class="mi-sep" role="separator"></div>

      <template v-else>
        <button
          class="menu-item-btn"
          type="button"
          role="menuitem"
          :disabled="it.disabled"
          @click="onClick(it)"
        >
          <span class="mi-left">
            <i v-if="it.icon" :class="it.icon" :style="{color: it.iconColor || ''}"></i>
            <i v-else-if="hasOneIcon" style="width: 1.3em; display: inline-block"></i>
            <span class="mi-label">{{ it.label || '' }}</span>
          </span>
          <span class="mi-right">
            <span v-if="it.hotkey" class="mi-hotkey">{{ it.hotkey }}</span>
            <i v-if="it.children?.length" class="fa-solid fa-chevron-right mi-caret"></i>
          </span>
        </button>

        <div
          v-if="it.children?.length"
          class="menu-sub"
          :class="{ open: it.open }"
          @pointerenter="cancelClose(it)"
          @pointerleave="scheduleClose(it)"
        >
          <MenuList :items="it.children" @select="emit('select', $event)"/>
        </div>
      </template>
    </li>
  </ul>
</template>

<style scoped lang="scss">
.menu-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.menu-item {
  position: relative;
  z-index: 1200;


  .menu-item-btn {
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: .75rem;
    padding: .35rem .5rem;
    border: 0;
    background: transparent;
    border-radius: var(--radius-md);
    color: var(--color-text);
    cursor: pointer;
    text-align: left;

    &:hover {
      background: rgba(255, 255, 255, .06);
    }

    &:disabled {
      opacity: .5;
      pointer-events: none;
    }
  }

  .mi-left {
    display: flex;
    align-items: center;
    gap: .5rem;
  }

  .mi-right {
    display: flex;
    align-items: center;
    gap: .35rem;
  }

  .mi-hotkey {
    color: var(--color-text-muted);
    font-size: .85em;
  }

  .mi-caret {
    color: var(--color-text-muted);
    font-size: .8em;
  }

  .mi-sep {
    height: 1px;
    margin: .3rem .2rem;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, .15), transparent);
  }

  .menu-sub {
    position: absolute;
    z-index: 1200;
    top: -4px;
    left: calc(100% + 6px);
    min-width: 220px;
    background: linear-gradient(180deg, rgba(255, 255, 255, .04), rgba(255, 255, 255, .02));
    border: 1px solid rgba(255, 255, 255, .08);
    border-radius: var(--radius-lg);
    box-shadow: 0 12px 32px rgba(0, 0, 0, .45);
    padding: .35rem;
    opacity: 0;
    transform: translateX(6px);
    transition: opacity .12s ease, transform .12s ease;
    pointer-events: none;
    backdrop-filter: blur(6px);

    &.open {
      opacity: 1;
      transform: translateX(0);
      pointer-events: auto;
    }
  }
}
</style>
