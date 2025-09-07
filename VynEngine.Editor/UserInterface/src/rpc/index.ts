type RpcRequest = {
  type: 'vyn.rpc';
  dir: 'js->cs';
  id: string;
  svc: string;
  method: string;
  args?: any[];
};

type RpcResponse = {
  type: 'vyn.rpc';
  dir: 'cs->js';
  id: string;
  ok: boolean;
  result?: any;
  error?: { code: string; message: string; data?: any };
};

type RpcEvent = {
  type: 'vyn.evt';
  dir: 'cs->js';
  svc: string;
  name: string;
  data?: any;
};

const pending = new Map<string, {
  resolve: (v: any) => void,
  reject: (e: any) => void,
  timer: number
}>();

function uuid() {
  return Math.random().toString(16).slice(2) + Date.now().toString(16);
}

(() => {
  // @ts-ignore
  if (window.__vynRpcInstalled) return;
  // @ts-ignore
  window.__vynRpcInstalled = true;

  // @ts-ignore
  window.external.receiveMessage((raw: string) => {
    try {
      const msg = JSON.parse(raw) as RpcResponse | RpcEvent;
      if (msg.type === 'vyn.rpc' && msg.dir === 'cs->js') {
        const p = pending.get((msg as RpcResponse).id);
        if (!p) return;
        clearTimeout(p.timer);
        pending.delete((msg as RpcResponse).id);
        (msg as RpcResponse).ok ? p.resolve((msg as RpcResponse).result)
          : p.reject((msg as RpcResponse).error);
        return;
      }
      if (msg.type === 'vyn.evt' && msg.dir === 'cs->js') {
        const key = `${msg.svc}:${msg.name}`;
        listeners.get(key)?.forEach(fn => fn(msg.data));
      }
    } catch { /* ignore */
    }
  });
})();

function sendRpc(svc: string, method: string, args: any[], timeoutMs = 10000): Promise<any> {
  const id = uuid();
  const packet: RpcRequest = {type: 'vyn.rpc', dir: 'js->cs', id, svc, method, args};
  const raw = JSON.stringify(packet);

  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => {
      pending.delete(id);
      reject({code: 'timeout', message: `RPC ${svc}.${method} timed out after ${timeoutMs}ms`});
    }, timeoutMs);

    pending.set(id, {resolve, reject, timer});

    // @ts-ignore
    window.external.sendMessage(raw);
  });
}

const listeners = new Map<string, Set<(d: any) => void>>();

export function on(svc: string, name: string, handler: (data: any) => void) {
  const key = `${svc}:${name}`;
  if (!listeners.has(key)) listeners.set(key, new Set());
  listeners.get(key)!.add(handler);
  return () => listeners.get(key)!.delete(handler);
}

export function off(svc: string, name: string, handler: (data: any) => void) {
  const key = `${svc}:${name}`;
  listeners.get(key)?.delete(handler);
}

export function service<T extends object = any>(svc: string, timeoutMs?: number): T {
  return new Proxy({}, {
    get(_t, prop: string) {
      return (...args: any[]) => sendRpc(svc, prop, args, timeoutMs);
    }
  }) as T;
}
