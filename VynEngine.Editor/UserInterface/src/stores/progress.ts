import {defineStore} from 'pinia';
import {ref} from 'vue';

export type Progress = {
  id: number
  label: string
  value: number
};

let idCounter = 0;

export const useProgressStore = defineStore('progress', () => {
  const items = ref<Progress[]>([]);
  const expanded = ref(false);

  function start(label: string, indeterminate = true) {
    const p: Progress = {id: ++idCounter, label, value: indeterminate ? -1 : 0};
    items.value.push(p);
    return p.id;
  }

  function update(id: number, value: number) {
    const p = items.value.find(p => p.id === id);
    if (p) {
      p.value = value;
      if (value >= 100) {
        setTimeout(() => finish(id), 500);
      }
    }
  }

  function finish(id: number) {
    items.value = items.value.filter(p => p.id !== id);
  }

  return {items, expanded, start, update, finish};
});