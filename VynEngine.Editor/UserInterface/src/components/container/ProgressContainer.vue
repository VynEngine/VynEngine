<script setup lang="ts">
import {type Progress, useProgressStore} from '@/stores/progress';
import {computed} from "vue";

const store = useProgressStore();

const topMost = computed<Progress | null>(() => {
  if (store.items.length <= 0) {
    return null;
  }

  if (store.items.length === 1) {
    return store.items[0];
  }

  if (!store.items.some(x => x.value !== -1)) { // all indeterminate
    return {id: -1, label: '', value: -1}; // we return a dummy indeterminate progress
  }

  // we calculate a total progress based on all determinate progresses (ignore indeterminate ones)
  const determinateItems = store.items.filter(x => x.value !== -1);
  const totalValue = determinateItems.reduce((acc, item) => acc + item.value, 0);
  const averageValue = Math.round((totalValue / determinateItems.length));
  return {id: -1, label: 'Overall Progress', value: averageValue};
});
</script>

<template>
  <div v-if="store.items.length > 0" class="footer-progress" @click="store.expanded = !store.expanded">
    <div class="summary">
      <span style="width: 10rem">{{ store.items.length }} {{ store.items.length > 1 ? 'processes' : 'process' }} running</span>

      <template v-if="topMost && !store.expanded">
        <div v-if="topMost.value !== -1" class="bar">
          <div class="bar__fill" style="pointer-events: none" :style="{ width: topMost.value + '%' }"></div>
        </div>
        <div v-else class="indeterminate-bar" style="pointer-events: none"></div>
      </template>
    </div>
    <transition name="expand">
      <div v-if="store.expanded" class="details">
        <div v-for="p in store.items" :key="p.id" class="progress-item">
          <span>{{ p.label }}</span>
          <div v-if="p.value!==-1" class="bar">
            <div class="bar__fill" :style="{ width: p.value + '%' }"></div>
          </div>
          <div v-else class="indeterminate-bar"></div>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped lang="scss">
.footer-progress {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: var(--color-bg-darker);
  border-top: 1px solid rgba(255, 255, 255, .08);
  box-shadow: 0 -4px 12px rgba(0, 0, 0, .4);
  color: var(--color-text);
  font-size: .85rem;
  z-index: 900;
  cursor: pointer;

  .bar {
    width: 100%;
    height: 8px;
    border-radius: 999px;
    background: rgba(255, 255, 255, .08);
    border: 1px solid rgba(255, 255, 255, .12);
    overflow: hidden;

    &__fill {
      height: 100%;
      width: 0;
      border-radius: 999px;
      background: linear-gradient(90deg, var(--color-accent-green), var(--color-accent-cyan));
      transition: width .6s ease;
    }
  }

  .indeterminate-bar {
    position: relative;
    width: 100%;
    height: 8px;
    border-radius: 999px;
    background: rgba(255, 255, 255, .08);
    border: 1px solid rgba(255, 255, 255, .12);
    overflow: hidden;

    &::after {
      content: "";
      position: absolute;
      top: 0;
      bottom: 0;
      width: 40%;
      background: linear-gradient(90deg,
        var(--color-accent-green),
        var(--color-accent-cyan)
      );
      border-radius: 999px;
      animation: indeterminate-slide 2s infinite ease-in-out;
    }
  }

  .summary {
    padding: .4rem .8rem;
    display: flex;
    align-items: center;
    justify-content: flex-start;
    border-top-left-radius: var(--radius-md);
    border-top-right-radius: var(--radius-md);
    gap: 1rem;

    span {
      font-weight: 500;
      color: var(--color-text-muted);
    }
  }

  .details {
    padding: .6rem .8rem .8rem;
    border-top: 1px solid rgba(255, 255, 255, .06);
    display: flex;
    flex-direction: column;
    gap: .6rem;

    .progress-item {
      display: flex;
      flex-direction: column;
      gap: .3rem;

      span {
        font-size: .8rem;
        color: var(--color-text-muted);
      }
    }
  }
}

.expand-enter-active, .expand-leave-active {
  transition: max-height .25s ease, opacity .25s ease;
}

.expand-enter-from, .expand-leave-to {
  max-height: 0;
  opacity: 0;
}

.expand-enter-to, .expand-leave-from {
  max-height: 400px;
  opacity: 1;
}

@keyframes indeterminate-slide {
  0% {
    left: -40%;
    width: 20%;
  }
  50% {
    left: 100%;
    width: 50%;
  }
  100% {
    left: -40%;
    width: 20%;
  }
}
</style>