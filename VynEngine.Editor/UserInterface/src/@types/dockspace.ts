export enum DockNodeKind {
  Split = 'split',
  Tabs = 'tabs',
}

export enum DockNodeDirection {
  Horizontal = 'horizontal',
  Vertical = 'vertical',
}

export interface DockNode {
  kind: DockNodeKind;
}

export interface DockSplitNode extends DockNode {
  kind: DockNodeKind.Split;
  direction: DockNodeDirection;
  children: DockNode[];
  sizes: number[]; // relative sizes of children
}

export interface DockTabsNode extends DockNode {
  kind: DockNodeKind.Tabs;
  tabs: DockPane[]; // identifiers for tabs
  activeTab: string; // identifier for the active tab
}

export interface DockPane {
  id: string; // unique identifier for the pane
  title: string; // title of the pane
  content: string; // content or component name for the pane
}