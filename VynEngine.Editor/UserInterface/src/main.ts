import { createApp } from 'vue';
import { createPinia } from 'pinia';

import App from './App.vue';
import tooltip from "@/directives/tooltip.ts";

import '@fortawesome/fontawesome-free/css/all.min.css';
import '@/styles/index.scss';
import '@/rpc';
import TestLayout from "@/components/TestLayout.vue";

const app = createApp(App);

app.component('TestLayout', TestLayout);

app.directive('tooltip', tooltip);
app.use(createPinia());

app.mount('#app');
