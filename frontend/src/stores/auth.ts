import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/services/authService'

interface User {
  id: string
  email: string
  displayName: string
  avatarUrl?: string
  role: string
}

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const user = ref<User | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const userDisplayName = computed(() => user.value?.displayName || '')
  const userId = computed(() => user.value?.id || '')

  // Actions
  function setAuth(tokens: { accessToken: string; refreshToken: string }) {
    token.value = tokens.accessToken
    refreshToken.value = tokens.refreshToken
  }

  function setUser(userData: User) {
    user.value = userData
  }

  function clearAuth() {
    token.value = null
    refreshToken.value = null
    user.value = null
    error.value = null
  }

  function loadAuthFromStorage() {
    const storedToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')
    const storedUser = localStorage.getItem('user')

    if (storedToken) {
      token.value = storedToken
      refreshToken.value = storedRefreshToken
      if (storedUser) {
        try {
          user.value = JSON.parse(storedUser)
        } catch {
          // Invalid user data, clear everything
          clearAuth()
        }
      }
    }
  }

  async login(credentials: { email: string; password: string }) {
    loading.value = true
    error.value = null

    try {
      const response = await authService.login(credentials)
      setAuth({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken
      })
      setUser(response.user)
      return true
    } catch (err: any) {
      error.value = err.message || '登入失敗'
      throw err
    } finally {
      loading.value = false
    }
  }

  async logout() {
    loading.value = true
    error.value = null

    try {
      await authService.logout()
    } catch (err) {
      // Ignore logout errors, always clear local state
      console.error('Logout error:', err)
    } finally {
      clearAuth()
      loading.value = false
    }
  }

  async refreshTokens() {
    if (!refreshToken.value) {
      throw new Error('No refresh token available')
    }

    try {
      const response = await authService.refreshToken(refreshToken.value)
      token.value = response.accessToken
      localStorage.setItem('accessToken', response.accessToken)
      return true
    } catch (err) {
      // Refresh failed, clear auth
      clearAuth()
      throw err
    }
  }

  return {
    // State
    token,
    refreshToken,
    user,
    loading,
    error,
    // Getters
    isAuthenticated,
    userDisplayName,
    userId,
    // Actions
    setAuth,
    setUser,
    clearAuth,
    loadAuthFromStorage,
    login,
    logout,
    refreshTokens
  }
}, {
  persist: {
    key: 'auth',
    storage: localStorage,
    pick: ['token', 'refreshToken', 'user']
  }
})