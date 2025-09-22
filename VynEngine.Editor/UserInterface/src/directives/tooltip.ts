import type {Directive, DirectiveBinding} from "vue";

type TooltipOptions = {
  text: string;
  offset?: number;
};

type BindingVal = string | TooltipOptions;

let tipEl: HTMLDivElement | null = null;
let hideTimer: number | null = null;

function ensureEl() {
  if (!tipEl) {
    tipEl = document.createElement("div");
    tipEl.className = "app-tooltip";
    document.body.appendChild(tipEl);
  }
  return tipEl;
}

function getTextAndOpts(binding: DirectiveBinding<BindingVal>) {
  const val = binding.value;
  const text = typeof val === "string" ? val : val?.text ?? "";
  const offset = typeof val === "string" ? 8 : (val?.offset ?? 8);
  return {text, offset};
}

function getPlacement(binding: DirectiveBinding) {
  const m = binding.modifiers;
  if (m.left) return "left";
  if (m.right) return "right";
  if (m.bottom) return "bottom";
  return "top";
}

function positionTip(target: HTMLElement, placement: string, offset: number) {
  const tip = ensureEl();
  const rect = target.getBoundingClientRect();

  tip.style.display = "block";
  const w = tip.offsetWidth;
  const h = tip.offsetHeight;

  let top: number, left: number;
  switch (placement) {
    case "bottom":
      top = rect.bottom + offset;
      left = rect.left + rect.width / 2 - w / 2;
      break;
    case "left":
      top = rect.top + rect.height / 2 - h / 2;
      left = rect.left - w - offset;
      break;
    case "right":
      top = rect.top + rect.height / 2 - h / 2;
      left = rect.right + offset;
      break;
    default: // top
      top = rect.top - h - offset;
      left = rect.left + rect.width / 2 - w / 2;
  }

  top = Math.max(4, Math.min(top, window.innerHeight - h - 4));
  left = Math.max(4, Math.min(left, window.innerWidth - w - 4));

  tip.style.top = `${top}px`;
  tip.style.left = `${left}px`;
}

function show(target: HTMLElement, binding: DirectiveBinding<BindingVal>) {
  const {text, offset} = getTextAndOpts(binding);
  if (!text) return;

  if (hideTimer) {
    clearTimeout(hideTimer);
    hideTimer = null;
  }

  const tip = ensureEl();
  tip.textContent = text;
  positionTip(target, getPlacement(binding), offset);
  tip.classList.add("visible");
}

function hide() {
  if (!tipEl) return;
  tipEl.classList.remove("visible");
  hideTimer = window.setTimeout(() => {
    if (tipEl) tipEl.style.display = "none";
  }, 150);
}

function bindEvents(el: HTMLElement, binding: DirectiveBinding<BindingVal>) {
  const onEnter = () => show(el, binding);
  const onLeave = () => hide();
  el.addEventListener("mouseenter", onEnter);
  el.addEventListener("mouseleave", onLeave);

  const onFocus = () => show(el, binding);
  const onBlur = () => hide();
  el.addEventListener("focus", onFocus);
  el.addEventListener("blur", onBlur);

  (el as any).__tooltipHandlers = {onEnter, onLeave, onFocus, onBlur};
}

const tooltip: Directive<HTMLElement, BindingVal> = {
  mounted(el, binding) {
    bindEvents(el, binding);
  },
  updated(el, binding) {
    if (tipEl?.classList.contains("visible")) {
      show(el, binding);
    }
  },
  unmounted(el) {
    const h = (el as any).__tooltipHandlers;
    if (h) {
      el.removeEventListener("mouseenter", h.onEnter);
      el.removeEventListener("mouseleave", h.onLeave);
      el.removeEventListener("focus", h.onFocus);
      el.removeEventListener("blur", h.onBlur);
      delete (el as any).__tooltipHandlers;
    }
    hide();
  }
};

export default tooltip;
