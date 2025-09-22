import { ref } from 'vue';
import { service, on, off } from '@/rpc';
import { applyPatch } from 'fast-json-patch';

type Snapshot = { key: string; version: number; data: any };
type PatchMsg = { key: string; version: number; ops: any[] };

const syncSvc = service<{ requestSnapshot(key: string): void }>('sync');

export function useSynced<T = any>(key: string): Syncer<T> {
  const version = ref(0)
  const data = ref<T | null>(null);
  let dispose = () => {}

  async function init() {
    let resolver: () => void = () => {};

    const onSnapshot = (msg: Snapshot) => {
      if (msg.key !== key) return;
      version.value = msg.version;
      data.value = msg.data;
      off('sync', 'snapshot', onSnapshot);
      resolver();
    };

    const onPatch = (msg: PatchMsg) => {
      if (!data.value) return;
      const res = applyPatch(data.value as any, (msg as PatchMsg).ops, /*validate*/ false, /*mutate*/ true);
      data.value = res.newDocument;
      version.value = msg.version;
    };

    on('sync', 'snapshot', onSnapshot);
    on('sync', 'patch', onPatch);

    dispose = () => {
      off('sync', 'patch', onPatch);
    };

    await new Promise<void>(resolve => {
      resolver = resolve;
      syncSvc.requestSnapshot(key);
    });
  }

  init().then(() => {});
  return new Syncer<T>(data, version, dispose);
}

class Syncer<T> {
  private readonly _ref: any;
  private readonly _version: any;
  private readonly _dispose: () => void;

  constructor(ref: any, version: any, dispose: () => void) {
    this._ref = ref;
    this._version = version;
    this._dispose = dispose;
  }

  get value(): T | null {
    return this._ref.value as T | null;
  }

  get version(): number {
    return this._version.value as number;
  }

  dispose() {
    this._dispose();
  }
}