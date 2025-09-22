import { defineStore } from 'pinia';
import { ref } from 'vue';

export type Toast = {
  id: number
  type: 'info' | 'success' | 'error' | 'warn'
  message: string
  timeout?: number
};

let idCounter = 0;

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([]);

  function push(t: Omit<Toast, 'id'>) {
    const toast: Toast = { id: idCounter++, ...t };
    toasts.value.push(toast);
    if (toast.timeout && toast.timeout > 0) {
      setTimeout(() => {
        remove(toast.id);
      }, toast.timeout);
    }
  }

  function remove(t: number|Toast) {
    const id = typeof t === 'number' ? t : t.id;
    toasts.value = toasts.value.filter(toast => toast.id !== id);
  }

  return { toasts, push, remove };
});