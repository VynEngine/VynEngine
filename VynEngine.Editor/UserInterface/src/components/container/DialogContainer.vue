<script setup lang="ts">
import {useDialogStore} from '@/stores/dialog';

const store = useDialogStore();
</script>

<template>
  <div v-if="store.current" class="dialog-overlay">
    <div class="dialog-box card">
      <div class="card__body">
        <template v-if="store.current.kind === 'confirm'">
          <p class="message">{{ store.current.message }}</p>
          <div class="actions">
            <button class="btn btn--neutral is-outline" @click="store.close(false)">No</button>
            <button class="btn btn--primary is-filled" @click="store.close(true)">Yes</button>
          </div>
        </template>

        <template v-else-if="store.current.kind === 'input'">
          <p class="message">{{ store.current.message }}</p>
          <div class="form-field">
            <input class="input" v-model="store.current.value"/>
          </div>
          <div class="actions">
            <button class="btn btn--neutral is-outline" @click="store.close(null)">Cancel</button>
            <button class="btn btn--primary is-filled" @click="store.close(store.current.value)">OK</button>
          </div>
        </template>

        <template v-else-if="store.current.kind === 'custom'">
          <component
            :is="store.current.component"
            v-bind="store.current.props"
            @close="store.close"
          />
        </template>

      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.dialog-overlay {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, .6);
  backdrop-filter: blur(4px);
  z-index: 9999;
}

.dialog-box {
  width: 360px;
  max-width: 90%;
  border-radius: var(--radius-lg);

  .message {
    margin-bottom: 1rem;
    font-size: .95rem;
    line-height: 1.4;
  }

  .actions {
    display: flex;
    justify-content: flex-end;
    gap: .5rem;
    margin-top: 1rem;
  }
}
</style>
