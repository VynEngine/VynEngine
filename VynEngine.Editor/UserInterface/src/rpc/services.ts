import {service} from "@/rpc/index.ts";

export interface WindowService {
  isWindows(): Promise<boolean>;
  setTitle(title: string): Promise<void>;
  setMaximized(maximized: boolean): Promise<void>;
  setMinimized(minimized: boolean): Promise<void>;
  beginDrag(): Promise<void>;
  beginResize(direction: string): Promise<void>;
  close(): Promise<void>;

  showTaskbarProgress(indeterminate: boolean): Promise<void>;
  hideTaskbarProgress(): Promise<void>;
  setTaskbarProgressErrored(): Promise<void>;
  setTaskbarProgressPaused(): Promise<void>;
  updateTaskbarProgress(percent: number): Promise<void>;
}

export function useWindowService(): WindowService {
  return service<WindowService>('window');
}

export interface DataService {
  getFile(parts: string[]): Promise<string>;
  exists(path: string): Promise<boolean>;
  read(path: string): Promise<string|null>;
  write(path: string, content: string): Promise<boolean>;
}

export function useDataService(): DataService {
  return service<DataService>('data');
}