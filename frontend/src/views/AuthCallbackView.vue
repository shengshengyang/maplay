<template>
  <div class="callback-container">
    <div class="callback-card">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>{{ $t('auth.callback.processing') || '處理中...' }}</p>
      </div>
      <div v-else-if="error" class="error-state">
        <p>{{ errorMessage }}</p>
        <button @click="goToLogin" class="back-button">
          {{ $t('auth.callback.backToLogin') || '返回登入' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import authService from '@/services/authService'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

onMounted(async () => {
  const code = route.query.code as string
  const provider = route.params.provider as string
  const redirect = (route.query.redirect as string) || '/'

  if (!code) {
    error.value = true
    errorMessage.value = 'OAuth 授權失敗：缺少授權碼'
    loading.value = false
    return
  }

  try {
    const response = await authService.oauthCallback({
      code,
      provider: provider as 'google' | 'line',
      redirectUri: `${window.location.origin}/login`
    })

    // Update auth store
    authStore.setAuth({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken
    })
    authStore.setUser(response.user)

    // Redirect to original destination
    router.push(redirect)
  } catch (err: any) {
    error.value = true
    errorMessage.value = err.message || 'OAuth 登入失敗'
    loading.value = false
  }
})

function goToLogin() {
  router.push('/login')
}
</script>

<style scoped>
.callback-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.callback-card {
  background: white;
  border-radius: 12px;
  padding: 40px;
  width: 100%;
  max-width: 400px;
  text-align: center;
}

.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-state p {
  color: #c33;
  margin: 0;
}

.back-button {
  padding: 12px 24px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: opacity 0.3s;
}

.back-button:hover {
  opacity: 0.9;
}
</style>
