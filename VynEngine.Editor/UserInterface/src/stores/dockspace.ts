import {defineStore} from "pinia";
import {reactive, ref} from "vue";
import {
  type DockNode,
  DockNodeDirection,
  DockNodeKind,
  type DockSplitNode,
  type DockTabsNode,
  type DockPane
} from "@/@types/dockspace";

export type DockPath = number[];
export type DropZone = "center" | "north" | "south" | "west" | "east";

function isSplit(n: DockNode): n is DockSplitNode {
  return n.kind === DockNodeKind.Split;
}

function isTabs(n: DockNode): n is DockTabsNode {
  return n.kind === DockNodeKind.Tabs;
}

function clonePane(p: DockPane): DockPane {
  return {...p};
}

export const useDockSpaceStore = defineStore('dockspace', () => {
  // ---------- STATE ----------
  const nodes = ref<DockNode[]>([
    {
      kind: DockNodeKind.Split,
      direction: DockNodeDirection.Horizontal,
      sizes: [30, 70],
      children: [
        {
          kind: DockNodeKind.Tabs,
          tabs: [
            {id: 'sidebar', title: 'Sidebar', content: 'TestLayout'},
          ],
          activeTab: 'sidebar'
        } as DockTabsNode,
        {
          kind: DockNodeKind.Split,
          direction: DockNodeDirection.Vertical,
          sizes: [70, 30],
          children: [
            {
              kind: DockNodeKind.Tabs,
              tabs: [
                {id: 'main', title: 'Main', content: 'TestLayout'},
              ],
              activeTab: 'main'
            } as DockTabsNode,
            {
              kind: DockNodeKind.Tabs,
              tabs: [
                {id: 'console', title: 'Console', content: 'TestLayout'},
                {id: 'logs', title: 'Logs', content: 'TestLayout'}
              ],
              activeTab: 'console'
            } as DockTabsNode
          ]
        } as DockSplitNode
      ]
    } as DockSplitNode
  ]); //TODO: load

  nodes.value.forEach(n => normalizeSizes(n));

  const dnd = reactive({
    dragging: false,
    pane: null as DockPane | null,
    fromPath: [] as DockPath,
    previewX: 0,
    previewY: 0,
    overPath: null as DockPath | null,
    zone: null as DropZone | null,
  });

  function getRoot(): DockNode {
    return nodes.value[0];
  }

  function getNodeByPath(root: DockNode, path: DockPath): DockNode {
    let n: DockNode = root;
    for (const i of path) {
      if (!isSplit(n)) break;
      n = n.children[i];
    }
    return n;
  }

  function setNodeByPath(root: DockNode, path: DockPath, node: DockNode) {
    if (path.length === 0) {
      nodes.value[0] = node;
      return;
    }
    const parentPath = path.slice(0, -1);
    const idx = path[path.length - 1];
    const parent = getNodeByPath(root, parentPath) as DockSplitNode;
    parent.children.splice(idx, 1, node);
  }

  function tabIndexById(tabs: DockTabsNode, id: string) {
    return tabs.tabs.findIndex(t => t.id === id);
  }

  function removePane(root: DockNode, tabsPath: DockPath, tabIndex: number) {
    const tabs = getNodeByPath(root, tabsPath) as DockTabsNode;
    if (!isTabs(tabs)) return;

    tabs.tabs.splice(tabIndex, 1);
    if (!tabs.tabs.length) {
      tabs.activeTab = "";
      return;
    }
    if (!tabs.activeTab || tabIndexById(tabs, tabs.activeTab) === -1) {
      tabs.activeTab = tabs.tabs[Math.min(tabIndex, tabs.tabs.length - 1)].id;
    }
  }

  function insertIntoTabs(root: DockNode, tabsPath: DockPath, pane: DockPane, atIndex?: number) {
    const tabs = getNodeByPath(root, tabsPath) as DockTabsNode;
    if (!isTabs(tabs)) return;
    const idx = atIndex == null ? tabs.tabs.length : Math.max(0, Math.min(atIndex, tabs.tabs.length));
    tabs.tabs.splice(idx, 0, pane);
    tabs.activeTab = pane.id;
  }

  function splitAt(root: DockNode, childPath: DockPath, direction: DockNodeDirection, place: "before" | "after", pane: DockPane) {
    const parentPath = childPath.slice(0, -1);
    const idx = childPath[childPath.length - 1];
    const parent = parentPath.length ? (getNodeByPath(root, parentPath) as DockSplitNode) : null;
    const target = parent ? parent.children[idx] : root;

    const wrapTabs: DockTabsNode = isTabs(target)
      ? (target as DockTabsNode)
      : {kind: DockNodeKind.Tabs, tabs: [], activeTab: ""};

    const newTabs: DockTabsNode = {kind: DockNodeKind.Tabs, tabs: [pane], activeTab: pane.id};
    const split: DockSplitNode = {
      kind: DockNodeKind.Split,
      direction,
      children: place === "before" ? [newTabs, wrapTabs] : [wrapTabs, newTabs],
      sizes: [0.5, 0.5]
    };

    if (!parent) {
      setNodeByPath(root, [], split);
    } else {
      parent.children.splice(idx, 1, split);
      parent.sizes.splice(idx, 1, 0.5, 0.5);
      normalizeSizes(parent);
    }
  }

  function flatten(root: DockNode, path: DockPath) {
    const node = getNodeByPath(root, path);

    if (isTabs(node) && node.tabs.length === 0) {
      if (path.length === 0) return;
      const parentPath = path.slice(0, -1);
      const idx = path[path.length - 1];
      const parent = getNodeByPath(root, parentPath) as DockSplitNode;
      parent.children.splice(idx, 1);
      parent.sizes.splice(idx, 1);
      normalizeSizes(parent);
      if (parent.children.length === 1) {
        const only = parent.children[0];
        setNodeByPath(root, parentPath, only);
      }
    } else if (isSplit(node)) {
      node.children.forEach((_, i) => flatten(root, [...path, i]));
      if (node.children.length === 1) {
        setNodeByPath(root, path, node.children[0]);
      }
      normalizeSizes(node);
    }
  }

  function normalizeSizes(n: DockNode) {
    if (isSplit(n)) {
      const totalPxLike = n.sizes.reduce((a, b) => a + b, 0);
      if (totalPxLike <= 0) {
        const eq = 1 / Math.max(1, n.children.length);
        n.sizes = new Array(n.children.length).fill(eq);
      } else {
        n.sizes = n.sizes.map(s => s / totalPxLike);
      }
      n.children.forEach(c => normalizeSizes(c));
    }
  }

  function startTabDrag(pathToTabs: DockPath, tabIndex: number, clientX: number, clientY: number) {
    const tabs = getNodeByPath(getRoot(), pathToTabs) as DockTabsNode;
    if (!isTabs(tabs)) return;
    const pane = clonePane(tabs.tabs[tabIndex]);
    dnd.dragging = true;
    dnd.pane = pane;
    dnd.fromPath = [...pathToTabs, tabIndex];
    dnd.previewX = clientX;
    dnd.previewY = clientY;
  }

  function updatePointer(x: number, y: number) {
    if (!dnd.dragging) return;
    dnd.previewX = x;
    dnd.previewY = y;
  }

  function updateHover(path: DockPath | null, zone: DropZone | null) {
    if (!dnd.dragging) return;
    dnd.overPath = path;
    dnd.zone = zone;
  }

  function dropOrCancel() {
    if (!dnd.dragging || !dnd.pane) {
      cancelDrag();
      return;
    }
    const root = getRoot();

    if (dnd.overPath && dnd.zone) {
      const fromTabsPath = dnd.fromPath.slice(0, -1);
      const fromIndex = dnd.fromPath[dnd.fromPath.length - 1];
      removePane(root, fromTabsPath, fromIndex);
      flatten(root, fromTabsPath);

      if (dnd.zone === "center") {
        insertIntoTabs(root, dnd.overPath, dnd.pane);
      } else {
        const dir = (dnd.zone === "west" || dnd.zone === "east") ? DockNodeDirection.Horizontal : DockNodeDirection.Vertical;
        const place = (dnd.zone === "west" || dnd.zone === "north") ? "before" : "after";
        splitAt(root, dnd.overPath, dir, place, dnd.pane);
      }
    }

    cancelDrag();
  }

  function cancelDrag() {
    dnd.dragging = false;
    dnd.pane = null;
    dnd.fromPath = [];
    dnd.overPath = null;
    dnd.zone = null;
  }

  function save() {
    // TODO: save
  }

  return {
    // state
    nodes, dnd,
    // utils
    getNodeByPath, setNodeByPath, removePane, insertIntoTabs, splitAt, flatten, normalizeSizes,
    // dnd
    startTabDrag, updatePointer, updateHover, dropOrCancel, cancelDrag,
    // misc
    save
  };
});
