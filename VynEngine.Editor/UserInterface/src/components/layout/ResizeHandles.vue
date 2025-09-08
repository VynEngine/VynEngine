<script setup lang="ts">
import {useWindowService} from "@/rpc/services.ts";

const windowService = useWindowService();

function start(direction: string) {
  windowService.beginResize(direction);
}
</script>

<template>
  <div class="resize-grip grip-left" @mousedown="start('left')"></div>
  <div class="resize-grip grip-right" @mousedown="start('right')"></div>
  <div class="resize-grip grip-top" @mousedown="start('top')"></div>
  <div class="resize-grip grip-bottom" @mousedown="start('bottom')"></div>

  <div class="resize-grip corner-tl" @mousedown="start('topleft')"></div>
  <div class="resize-grip corner-tr" @mousedown="start('topright')"></div>
  <div class="resize-grip corner-bl" @mousedown="start('bottomleft')"></div>
  <div class="resize-grip corner-br" @mousedown="start('bottomright')"></div>
</template>

<style scoped lang="scss">
$grip: 8px;
$corner: 12px;

.resize-grip {
  position: fixed;
  z-index: 99999;
  pointer-events: auto;
}

.grip-left {
  left: 0;
  top: 0;
  width: $grip;
  height: 100%;
  cursor: ew-resize;
}

.grip-right {
  right: 0;
  top: 0;
  width: $grip;
  height: 100%;
  cursor: ew-resize;
}

.grip-top {
  left: 0;
  top: 0;
  width: 100%;
  height: $grip;
  cursor: ns-resize;
}

.grip-bottom {
  left: 0;
  bottom: 0;
  width: 100%;
  height: $grip;
  cursor: ns-resize;
}

.corner-tl {
  left: 0;
  top: 0;
  width: $corner;
  height: $corner;
  cursor: nwse-resize;
}

.corner-tr {
  right: 0;
  top: 0;
  width: $corner;
  height: $corner;
  cursor: nesw-resize;
}

.corner-bl {
  left: 0;
  bottom: 0;
  width: $corner;
  height: $corner;
  cursor: nesw-resize;
}

.corner-br {
  right: 0;
  bottom: 0;
  width: $corner;
  height: $corner;
  cursor: nwse-resize;
}

.resize-grip {
  background: transparent;
}
</style>