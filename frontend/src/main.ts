import './assets/main.css'
import './utils/errorHandler'
import './utils/mapHelpers'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth'

const app = createApp(App)

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

app.use(pinia)
app.use(router)

// Initialize auth from localStorage
const authStore = useAuthStore()
authStore.loadAuthFromStorage()

app.mount('#app')
