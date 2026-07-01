import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const user = ref<any>(null)
  const isAuthenticated = computed(() => !!token.value)

  // Actions
  function setAuth(tokens: { accessToken: string; refreshToken: string }) {
    token.value = tokens.accessToken
    refreshToken.value = tokens.refreshToken
    // Persist to localStorage
    localStorage.setItem('accessToken', tokens.accessToken)
    localStorage.setItem('refreshToken', tokens.refreshToken)
  }

  function setUser(userData: any) {
    user.value = userData
    localStorage.setItem('user', JSON.stringify(userData))
  }

  function clearAuth() {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  function loadAuthFromStorage() {
    const storedToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')
    const storedUser = localStorage.getItem('user')

    if (storedToken) {
      token.value = storedToken
      refreshToken.value = storedRefreshToken
      if (storedUser) {
        user.value = JSON.parse(storedUser)
      }
    }
  }

  return {
    token,
    refreshToken,
    user,
    isAuthenticated,
    setAuth,
    setUser,
    clearAuth,
    loadAuthFromStorage
  }
})