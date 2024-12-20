// Style importing
import './assets/main.css'
import 'bootstrap/dist/css/bootstrap.css'
import 'vue-toast-notification/dist/theme-bootstrap.css';

// Import app
import { createApp } from 'vue'
import App from './App.vue'

// Unhead for managing meta info about the page
import { createHead } from '@unhead/vue'
// Pinia for retaining user store
import { createPinia } from 'pinia'
// Vue Router for enabling SPA functionality
import router from './router/router.js'
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
app.use(ToastPlugin);

// Mount app to HTML
app.mount('#app')
