import { createApp } from 'vue';
import { createPinia } from 'pinia';

import App from './App.vue';
import tooltip from "@/directives/tooltip.ts";

import '@fortawesome/fontawesome-free/css/all.min.css';
import '@/styles/index.scss';
import '@/rpc';

const app = createApp(App);

app.directive('tooltip', tooltip);
app.use(createPinia());

app.mount('#app');
