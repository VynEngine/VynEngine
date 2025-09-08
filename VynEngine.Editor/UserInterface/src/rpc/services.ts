import {service} from "@/rpc/index.ts";

export interface WindowService {
  isWindows(): Promise<boolean>;
  setTitle(title: string): Promise<void>;
  setMaximized(maximized: boolean): Promise<void>;
  setMinimized(minimized: boolean): Promise<void>;
  beginDrag(): Promise<void>;
  beginResize(direction: string): Promise<void>;
  close(): Promise<void>;
}

export function useWindowService(): WindowService {
  return service<WindowService>('window');
}