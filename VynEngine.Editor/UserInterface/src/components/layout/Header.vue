<script setup lang="ts">
import {useWindowService} from "@/rpc/services.ts";
import {onMounted, onUnmounted, ref} from "vue";
import {off, on} from "@/rpc";
import MenuList, {type HeaderMenu} from "@/components/layout/MenuList.vue";
import {registerHotkey, unregisterHotkey, useHotkeys} from "@/composables/useHotkeys.ts";

const defaultSubtitle = "Visual Novel Engine for ease of use";
const windowService = useWindowService();
const { } = useHotkeys();

const showWindowButtons = ref(false);
const isMaximized = ref(false);
const currentSubtitle = ref(defaultSubtitle);

const menus = ref<HeaderMenu[]>([
  { id: 'file', label: 'File' },
  { id: 'edit', label: 'Edit' },
  {
    id: 'view',
    label: 'View',
    children: [
      { id: 'toggle-devtools', label: 'Toggle Developer Tools', hotkey: 'F12', icon: 'fa-solid fa-code' },
      { id: 'reload', label: 'Reload', hotkey: 'Ctrl+R', icon: 'fa-solid fa-arrows-rotate' },
      { id: 'force-reload', label: 'Force Reload', hotkey: 'Ctrl+Shift+R', icon: 'fa-solid fa-arrows-rotate' },
      { id: 'separator-1', type: 'separator' },
      { id: 'toggle-fullscreen', label: 'Toggle Fullscreen', hotkey: 'F11', icon: 'fa-solid fa-expand' },
      {
        id: 'submenu-view',
        label: 'Submenu',
        children: [
          { id: 'submenu-item-1', label: 'Item 1' },
          { id: 'submenu-item-2', label: 'Item 2' },
          { id: 'submenu-item-3', label: 'Item 3' }
        ]
      }
    ]
  },
  { id: 'help', label: 'Help' }
]);

onMounted(async () => {
  on("window", "updateMaximized", onMaximizedChanged);
  on("window", "updateSubtitle", onChangeSubtitle);
  showWindowButtons.value = await windowService.isWindows();

  document.addEventListener("mousedown", onDocumentClick);

  refreshMenuHotkeys();
});

onUnmounted(() => {
  off("window", "updateMaximized", onMaximizedChanged);
  off("window", "updateSubtitle", onChangeSubtitle);

  document.removeEventListener("mousedown", onDocumentClick);

  unregisterMenuHotkeys();
});

function toggleMaximize() {
  windowService.setMaximized(!isMaximized.value);
}
function onMaximizedChanged(maximized: boolean) {
  isMaximized.value = maximized;
}
function onChangeSubtitle(newSubtitle: string) {
  currentSubtitle.value = newSubtitle.trim().length <= 0 ? defaultSubtitle : newSubtitle;
}

function closeAllMenus() {
  menus.value.forEach(m => m.open = false);
}

function toggleMenu(it: HeaderMenu) {
  if (it.open) {
    it.open = false;
    closeAllMenus();
  } else {
    closeAllMenus();
    it.open = true;
  }
}

function flattenMenus(m: HeaderMenu[]): HeaderMenu[] {
  const out: HeaderMenu[] = [];
  const stack: HeaderMenu[] = [...m];
  while (stack.length) {
    const it = stack.shift()!;
    out.push(it);
    if (it.children) stack.unshift(...it.children);
  }
  return out;
}

function refreshMenuHotkeys() {
  unregisterMenuHotkeys();
  registerMenuHotkeys();
}

function registerMenuHotkeys() {
  for (const it of flattenMenus(menus.value)) {
    if (it.hotkey && !it.disabled) {
      registerHotkey(it.hotkey, () => onMenuItem(it.id));
    }
  }
}

function unregisterMenuHotkeys() {
  for (const it of flattenMenus(menus.value)) {
    if (it.hotkey) {
      unregisterHotkey(it.hotkey);
    }
  }
}

function onDocumentClick(e: MouseEvent) {
  const target = e.target as HTMLElement;
  if (!target.closest(".menu-bar") && !target.closest(".menu-dropdown")) {
    closeAllMenus();
  }
}

function onMenuItem(id: string) {
  console.log("menu:", id);
  closeAllMenus();
}
</script>

<template>
  <div class="app-header" @mousedown.self="windowService.beginDrag()">
    <div class="header-left">
      <img src="/favicon.ico" alt="logo" class="logo"/>

      <ul class="menu-bar" role="menubar">
        <li v-for="m in menus" :key="m.id" class="menu-root" role="none">
          <button
            class="menu-root-btn"
            role="menuitem"
            type="button"
            :disabled="m.disabled"
            @click="toggleMenu(m)"
          >
            <i v-if="m.icon" :class="m.icon" :style="{color: m.iconColor || ''}"></i>
            <span>{{ m.label }}</span>
          </button>

          <div v-if="m.children?.length"
               class="menu-dropdown"
               :class="{ 'is-open': m.open }">
            <MenuList :items="m.children" @select="onMenuItem" />
          </div>
        </li>
      </ul>
    </div>

    <div class="header-center">
      <div class="title">VynEngine</div>
      <div class="subtitle">{{currentSubtitle}}</div>
    </div>

    <div v-show="showWindowButtons" class="header-right">
      <button @click="windowService.setMinimized(true)">
        <i class="fa-solid fa-minus"/>
      </button>
      <button @click="toggleMaximize">
        <i v-if="!isMaximized" class="fa-solid fa-up-right-and-down-left-from-center"/>
        <i v-else class="fa-solid fa-down-left-and-up-right-to-center"/>
      </button>
      <button class="close" @click="windowService.close()">
        <i class="fa-solid fa-xmark"/>
      </button>
    </div>
  </div>
</template>
