<script setup lang="ts">
import {useToastStore} from '@/stores/toast';

const store = useToastStore();
</script>

<template>
  <div class="toast-container">
    <div
      v-for="t in store.toasts"
      :key="t.id"
      class="toast"
      :class="t.type"
    >
      <i class="icon"
         :class="{
           'fa-solid fa-circle-check': t.type==='success',
           'fa-solid fa-circle-xmark': t.type==='error',
           'fa-solid fa-circle-info':  t.type==='info',
           'fa-solid fa-triangle-exclamation': t.type==='warn'
         }"/>
      <span class="message">{{ t.message }}</span>

      <i class="fa-solid fa-xmark" @click="store.remove(t.id)"/>
    </div>
  </div>
</template>

<style scoped lang="scss">
.toast-container {
  position: fixed;
  top: 72px;
  right: 1rem;
  display: flex;
  flex-direction: column;
  gap: .6rem;
  z-index: 2000;

  .toast {
    display: flex;
    align-items: center;
    gap: .6rem;
    padding: .6rem 1rem;
    min-width: 220px;
    border-radius: var(--radius-md);
    background: linear-gradient(
        135deg,
        rgba(255, 255, 255, 0.04) 0%,
        rgba(255, 255, 255, 0.02) 100%
    );
    border-left: 4px solid transparent;
    box-shadow: 0 4px 12px rgba(0, 0, 0, .4);
    color: var(--color-text);
    font-size: .9rem;
    animation: slide-in .25s ease, fade-out .25s ease forwards;

    .icon {
      flex-shrink: 0;
      font-size: 1rem;
    }

    .message {
      flex: 1;
    }

    .fa-xmark {
      cursor: pointer;
      opacity: .6;
      transition: opacity .2s;

      &:hover {
        opacity: 1;
      }
    }

    &.success {
      border-left-color: var(--color-success);

      .icon {
        color: var(--color-success);
      }
    }

    &.error {
      border-left-color: var(--color-error);

      .icon {
        color: var(--color-error);
      }
    }

    &.info {
      border-left-color: var(--color-info);

      .icon {
        color: var(--color-info);
      }
    }

    &.warn {
      border-left-color: var(--color-warning);

      .icon {
        color: var(--color-warning);
      }
    }
  }
}

@keyframes slide-in {
  from {
    transform: translateX(120%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
</style>
