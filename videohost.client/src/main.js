import './assets/main.css'

// Import app
import { createApp } from 'vue'
import App from './App.vue'

// Pinia for retaining user store
import { createPinia } from 'pinia'
// TODO: Router - to implement! Create a routes file and use that for options.
//import { createRouter } from 'vue-router'

// Create Vue app 
const app = createApp(App)

// Create extra libraries
const pinia = createPinia()
// TODO: Router - to implement! Create a routes file and use that for options.
//const router = createRouter()

// Mount libraries to app
app.use(pinia)
// TODO: Router - to implement! Create a routes file and use that for options.
//app.use(router)

// Mount app to HTML
app.mount('#app')
