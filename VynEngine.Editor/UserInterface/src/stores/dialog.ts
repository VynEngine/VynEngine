import {defineStore} from 'pinia';
import {type Component, ref} from 'vue';

export type Dialog =
  | { kind: 'confirm', message: string, resolve: (v: boolean) => void }
  | { kind: 'input', message: string, value: string, resolve: (v: string | null) => void }
  | { kind:'custom'; component: Component; props?:Record<string,any>; resolve:(v:any)=>void };

export const useDialogStore = defineStore('dialog', () => {
  const current = ref<Dialog | null>(null);

  function confirm(message: string) {
    return new Promise<boolean>(resolve => {
      current.value = {kind: 'confirm', message, resolve};
    })
  }

  function input(message: string, initial = "") {
    return new Promise<string | null>(resolve => {
      current.value = {kind: 'input', message, value: initial, resolve};
    })
  }

  function custom(component: Component, props?:Record<string,any>) {
    return new Promise<any>(resolve => {
      current.value = {kind: 'custom', component, props, resolve};
    })
  }

  function close(result: any) {
    (current.value as Dialog)?.resolve(result as never);
    current.value = null;
  }

  return {current, confirm, input, close};
});