// Style importing
import './assets/main.css'
import 'bootstrap/dist/css/bootstrap.css'

// Import app
import { createApp } from 'vue'
import App from './App.vue'

// Unhead for managing meta info about the page
import { createHead } from '@unhead/vue'
// Pinia for retaining user store
import { createPinia } from 'pinia'
// Vue Router for enabling SPA functionality
import { createRouter, createWebHistory } from "vue-router";
import routes from './routes.js'

// Create Vue app 
const app = createApp(App)

// Create extra libraries
const head = createHead()
const pinia = createPinia()
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

// Mount libraries to app
app.use(head)
app.use(pinia)
app.use(router)

// Mount app to HTML
app.mount('#app')
