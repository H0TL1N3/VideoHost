// Style importing
import './assets/main.css';
import 'bootstrap/dist/css/bootstrap.css';
import 'vue-toast-notification/dist/theme-bootstrap.css';
import 'vue-multiselect/dist/vue-multiselect.min.css';

// Import app
import { createApp } from 'vue';
import App from './App.vue';

// Unhead for managing meta info about the page
import { createHead } from '@unhead/vue';
// Pinia for retaining user store
import { createPinia } from 'pinia';
import { useUserStore } from './stores/user';
// Vue Router for enabling SPA functionality
import router from './router/router.js';
// Vue Toast Notification for toasts
import ToastPlugin from 'vue-toast-notification';

// Create Vue app 
const app = createApp(App)

// Create extra libraries
const head = createHead()
const pinia = createPinia()

// Mount libraries to app
app.use(head)
app.use(pinia)
app.use(router)
app.use(ToastPlugin)

// Reinitialize userStore
const userStore = useUserStore();
userStore.initializeStore()

// Mount app to HTML
app.mount('#app')
