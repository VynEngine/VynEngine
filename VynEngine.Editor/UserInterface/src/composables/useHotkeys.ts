import {onMounted, onUnmounted} from "vue";

type HotkeyHandler = (ev: KeyboardEvent) => void;
type NormalizedCombo = string;

const isMac = /Mac|iPod|iPhone|iPad/.test(navigator.platform);

const pressed = new Set<string>();
const registry = new Map<NormalizedCombo, HotkeyHandler>();
const parsedCache = new Map<string, NormalizedCombo>();

function normKeyName(k: string): string {
  const key = k.toLowerCase();
  if (key === ' ') return 'space';
  if (key === 'arrowup') return 'arrowup';
  if (key === 'arrowdown') return 'arrowdown';
  if (key === 'arrowleft') return 'arrowleft';
  if (key === 'arrowright') return 'arrowright';
  if (key === 'escape') return 'esc';
  if (key === 'return') return 'enter';
  return key.length === 1 ? key : key;
}

function isModifier(key: string) {
  return key === 'ctrl' || key === 'alt' || key === 'shift' || key === 'meta';
}

function normalizeCombo(raw: string): NormalizedCombo {
  const cached = parsedCache.get(raw);
  if (cached) return cached;

  const parts = raw.split('+').map(p => p.trim().toLowerCase()).filter(Boolean);

  const mapped = parts.map(p => {
    if (p === 'cmd' || p === 'command') return 'meta';
    if (p === 'option') return 'alt';
    if (p === 'control') return 'ctrl';
    if (p === 'escape') return 'esc';
    if (p === 'return') return 'enter';
    return p;
  });

  const modsOrder = ['ctrl', 'alt', 'shift', 'meta'];
  const mods = mapped.filter(m => isModifier(m)).sort((a, b) => modsOrder.indexOf(a) - modsOrder.indexOf(b));
  const nonMods = mapped.filter(m => !isModifier(m)).map(normKeyName);

  const uniq = Array.from(new Set([...mods, ...nonMods]));
  const result = uniq.join('+');
  parsedCache.set(raw, result);
  return result;
}

export function describeHotkey(raw: string) {
  const combo = normalizeCombo(raw).split('+');
  const pretty = combo.map(p => {
    if (p === 'ctrl') return isMac ? '⌃' : 'Ctrl';
    if (p === 'alt') return isMac ? '⌥' : 'Alt';
    if (p === 'shift') return isMac ? '⇧' : 'Shift';
    if (p === 'meta') return isMac ? '⌘' : 'Win';
    if (p === 'esc') return 'Esc';
    if (p === 'space') return 'Space';
    return p.length === 1 ? p.toUpperCase() : p[0].toUpperCase() + p.slice(1);
  });
  return pretty.join(isMac ? '' : '+');
}

export function registerHotkey(raw: string, handler: HotkeyHandler) {
  registry.set(normalizeCombo(raw), handler);
}

export function unregisterHotkey(raw: string) {
  registry.delete(normalizeCombo(raw));
}

export function clearHotkeys() {
  registry.clear();
}

function currentCombos(): NormalizedCombo[] {
  const mods = Array.from(pressed).filter(isModifier).sort();
  const mains = Array.from(pressed).filter(k => !isModifier(k));

  const combos: string[] = [];
  if (mains.length === 0) {
    if (mods.length) combos.push(mods.join('+'));
  } else {
    for (const m of mains) {
      combos.push([...mods, m].join('+'));
    }
  }
  return combos;
}

function bestMatch(): { combo: NormalizedCombo, handler: HotkeyHandler } | null {
  const combos = currentCombos();
  const candidates: Array<{ combo: NormalizedCombo, handler: HotkeyHandler }> = [];
  for (const c of combos) {
    const h = registry.get(c);
    if (h) candidates.push({combo: c, handler: h});
  }
  if (candidates.length === 0) return null;
  candidates.sort((a, b) => b.combo.split('+').length - a.combo.split('+').length);
  return candidates[0];
}

function isEditableTarget(el: EventTarget | null) {
  const t = el as HTMLElement | null;
  if (!t) return false;
  const tag = (t.tagName || '').toLowerCase();
  if (tag === 'input' || tag === 'textarea' || tag === 'select') return true;
  if (t.isContentEditable) return true;
  return !!t.closest?.('[contenteditable="true"]');
}

function onKeyDown(ev: KeyboardEvent) {
  if (isEditableTarget(ev.target)) return;

  const k = normKeyName(ev.key);

  if (ev.ctrlKey) pressed.add('ctrl'); else pressed.delete('ctrl');
  if (ev.altKey) pressed.add('alt'); else pressed.delete('alt');
  if (ev.shiftKey) pressed.add('shift'); else pressed.delete('shift');
  if (ev.metaKey) pressed.add('meta'); else pressed.delete('meta');

  pressed.add(k);

  const match = bestMatch();
  if (match) {
    ev.preventDefault();
    match.handler(ev);
  }
}

function onKeyUp(ev: KeyboardEvent) {
  const k = normKeyName(ev.key);
  pressed.delete(k);

  if (!ev.ctrlKey) pressed.delete('ctrl');
  if (!ev.altKey) pressed.delete('alt');
  if (!ev.shiftKey) pressed.delete('shift');
  if (!ev.metaKey) pressed.delete('meta');
}

export function useHotkeys() {
  onMounted(() => {
    window.addEventListener('keydown', onKeyDown, {capture: true});
    window.addEventListener('keyup', onKeyUp, {capture: true});
    window.addEventListener('blur', () => pressed.clear());
  });
  onUnmounted(() => {
    window.removeEventListener('keydown', onKeyDown, {capture: true});
    window.removeEventListener('keyup', onKeyUp, {capture: true});
    pressed.clear();
  });

  return {
    registerHotkey,
    unregisterHotkey,
    clearHotkeys,
    describeHotkey,
  };
}
