import {defineStore} from "pinia";
import {reactive, ref} from "vue";
import {
  type DockNode,
  DockNodeDirection,
  DockNodeKind,
  type DockPane,
  type DockSplitNode,
  type DockTabsNode,
} from "@/@types/dockspace";
import {useDataService} from "@/rpc/services.ts";
import {parse, stringify} from 'smol-toml';
import {useToastStore} from "@/stores/toast.ts";

const defaultLayout = () => [
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
] as DockNode[];

export const useDockSpaceStore = defineStore('dockspace', () => {
  const nodes = ref<DockNode[]>(defaultLayout());
  const layoutFile = ref<string>("");

  const service = useDataService();
  (async () => {
    layoutFile.value = await service.getFile(["editor", "layout.toml"]);
    const layoutData = await service.read(layoutFile.value);
    if (layoutData && layoutData.length > 0) {
      try {
        const parsed = parse(layoutData);
        const parsedNodes = (parsed as any)["nodes"];
        if (Array.isArray(parsedNodes)) {
          nodes.value = parsedNodes as DockNode[];
        } else {
          console.warn("Invalid layout data, using default");
        }
      } catch (e) {
        console.error("Failed to parse layout data:", e);
      }
    }
  })();

  function save() {
    if (layoutFile.value === "") {
      console.warn("Layout file path is empty, cannot save layout");
      return;
    }

    service.write(layoutFile.value, stringify({nodes: nodes.value})).then(success => {
      if (!success) {
        console.warn("Failed to save layout data");
      }
    }).catch((e) => {
      console.error("Failed to save layout data:", e);
    });
  }

  const dnd = reactive({
    draggingParent: null as DockTabsNode | null,
    dragging: null as DockPane | null,
    dragOver: null as DockNode | null,
    dragOverPos: '' as 'top' | 'bottom' | 'left' | 'right' | 'center' | '',
  });

  function getParentNode(node: DockNode): DockNode | null {
    function findParent(current: DockNode, target: DockNode): DockNode | null {
      if (current.kind === DockNodeKind.Split) {
        const splitNode = current as DockSplitNode;
        for (const child of splitNode.children) {
          if (child === target) {
            return current;
          }
          const found = findParent(child, target);
          if (found) {
            return found;
          }
        }
      }

      return null;
    }

    for (const rootNode of nodes.value) {
      if (rootNode === node) {
        return { kind: DockNodeKind.Root };
      }

      const parent = findParent(rootNode, node);
      if (parent) {
        return parent;
      }
    }

    return null;
  }

  function removeNode(node: DockNode) {
    const parent = getParentNode(node);
    if (!parent) {
      const notify = useToastStore();
      notify.push({type: "warn", message: "Failed to remove node: parent not found!", timeout: 5000});
      return;
    }

    if (parent.kind === DockNodeKind.Root) {
      nodes.value = nodes.value.filter(n => n !== node);
    } else if (parent.kind === DockNodeKind.Split) {
      const splitParent = parent as DockSplitNode;
      const index = splitParent.children.indexOf(node);
      if (index !== -1) {
        splitParent.children.splice(index, 1);
        splitParent.sizes.splice(index, 1);
        if (splitParent.children.length === 0) {
          removeNode(splitParent);
        }
      }
    }
  }

  function removePane(pane: DockPane, parent: DockTabsNode) {
    const index = parent.tabs.findIndex(t => t.id === pane.id);
    if (index !== -1) {
      parent.tabs.splice(index, 1);
      if (parent.activeTab === pane.id) {
        if (parent.tabs.length > 0) {
          parent.activeTab = parent.tabs[Math.max(0, index - 1)].id;
        } else {
          parent.activeTab = '';
        }
      }

      if (parent.tabs.length === 0) {
        removeNode(parent);
      }
    }
  }

  function movePane(oldParent: DockTabsNode, pane: DockPane, newParent: DockNode, position: 'top' | 'bottom' | 'left' | 'right' | 'center') {
    removePane(pane, oldParent);

    if (newParent.kind === DockNodeKind.Tabs) {
      const tabsParent = newParent as DockTabsNode;
      if (position === 'center') {
        tabsParent.tabs.push(pane);
        tabsParent.activeTab = pane.id;
      } else {
        const parentOfTabs = getParentNode(tabsParent);
        if (!parentOfTabs || parentOfTabs.kind !== DockNodeKind.Split) {
          const notify = useToastStore();
          notify.push({type: "warn", message: "Failed to move node: invalid parent!", timeout: 5000});
          return;
        }

        const splitParent = parentOfTabs as DockSplitNode;
        const index = splitParent.children.indexOf(tabsParent);
        if (index === -1) {
          const notify = useToastStore();
          notify.push({type: "warn", message: "Failed to move node: tabs parent not found in its parent!", timeout: 5000});
          return;
        }

        let newSplit: DockSplitNode;
        if (position === 'left' || position === 'right') {
          newSplit = {
            kind: DockNodeKind.Split,
            direction: DockNodeDirection.Horizontal,
            sizes: [50, 50],
            children: []
          };
        } else {
          newSplit = {
            kind: DockNodeKind.Split,
            direction: DockNodeDirection.Vertical,
            sizes: [50, 50],
            children: []
          };
        }

        if (position === 'left' || position === 'top') {
          newSplit.children.push({
            kind: DockNodeKind.Tabs,
            tabs: [pane],
            activeTab: pane.id
          } as DockTabsNode);
          newSplit.children.push(tabsParent);
        } else {
          newSplit.children.push(tabsParent);
          newSplit.children.push({
            kind: DockNodeKind.Tabs,
            tabs: [pane],
            activeTab: pane.id
          } as DockTabsNode);
        }

        splitParent.children.splice(index, 1, newSplit);
      }
    } else if (newParent.kind === DockNodeKind.Split) {
      const splitParent = newParent as DockSplitNode;
      let insertIndex = splitParent.children.length;
      if (position === 'top' || position === 'left') {
        insertIndex = 0;
      }
      splitParent.children.splice(insertIndex, 0, {
        kind: DockNodeKind.Tabs,
        tabs: [pane],
        activeTab: pane.id
      } as DockTabsNode);
      splitParent.sizes.push(100 / splitParent.children.length);
      splitParent.sizes = splitParent.sizes.map(() => 100 / splitParent.children.length);
    } else {
      const notify = useToastStore();
      notify.push({type: "warn", message: "Failed to move node: invalid target!", timeout: 5000});
    }
  }

  return {
    nodes, dnd,
    save, movePane
  };
});
