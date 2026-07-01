<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <h1>{{ $t('auth.login.title') || '親子資源地圖' }}</h1>
        <p>{{ $t('auth.login.subtitle') || '登入以開始使用' }}</p>
      </div>

      <!-- Error message -->
      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>

      <!-- Login form -->
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">{{ $t('auth.login.email') || '電子郵件' }}</label>
          <input
            id="email"
            v-model="loginData.email"
            type="email"
            required
            :placeholder="$t('auth.login.emailPlaceholder') || '請輸入電子郵件'"
            :disabled="loading"
          />
        </div>

        <div class="form-group">
          <label for="password">{{ $t('auth.login.password') || '密碼' }}</label>
          <input
            id="password"
            v-model="loginData.password"
            type="password"
            required
            :placeholder="$t('auth.login.passwordPlaceholder') || '請輸入密碼'"
            :disabled="loading"
          />
        </div>

        <button type="submit" class="login-button" :disabled="loading">
          <span v-if="!loading">{{ $t('auth.login.submit') || '登入' }}</span>
          <span v-else class="loading-spinner">{{ $t('auth.login.loading') || '登入中...' }}</span>
        </button>
      </form>

      <!-- OAuth login options -->
      <div class="oauth-section">
        <div class="divider">
          <span>{{ $t('auth.login.or') || '或' }}</span>
        </div>

        <button @click="handleGoogleLogin" class="oauth-button google-button" :disabled="loading">
          <span class="google-icon">G</span>
          {{ $t('auth.login.google') || '使用 Google 登入' }}
        </button>

        <button @click="handleLineLogin" class="oauth-button line-button" :disabled="loading">
          <span class="line-icon">L</span>
          {{ $t('auth.login.line') || '使用 LINE 登入' }}
        </button>
      </div>

      <!-- Register link -->
      <div class="register-section">
        <p>
          {{ $t('auth.login.noAccount') || '還沒有帳號？' }}
          <router-link to="/register" class="register-link">
            {{ $t('auth.login.register') || '立即註冊' }}
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import authService from '@/services/authService'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const loginData = ref({
  email: '',
  password: ''
})

const loading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await authService.login(loginData.value)

    // Update auth store
    authStore.setAuth({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken
    })
    authStore.setUser(response.user)

    // Redirect to original destination or home
    const redirectTo = (route.query.redirect as string) || '/'
    router.push(redirectTo)
  } catch (error: any) {
    errorMessage.value = error.message || '登入失敗，請檢查您的帳號密碼'
  } finally {
    loading.value = false
  }
}

const handleGoogleLogin = () => {
  const oauthUrl = authService.getGoogleOAuthUrl()
  window.location.href = oauthUrl
}

const handleLineLogin = () => {
  const oauthUrl = authService.getLineOAuthUrl()
  window.location.href = oauthUrl
}

// Check for OAuth callback parameters
const checkOAuthCallback = () => {
  const code = route.query.code as string
  const provider = route.query.provider as string

  if (code && provider) {
    handleOAuthCallback(code, provider)
  }
}

const handleOAuthCallback = async (code: string, provider: string) => {
  loading.value = true
  errorMessage.value = ''

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

    // Redirect to home
    router.push('/')
  } catch (error: any) {
    errorMessage.value = error.message || 'OAuth 登入失敗'
  } finally {
    loading.value = false
  }
}

// Check for OAuth callback on mount
checkOAuthCallback()
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.login-card {
  background: white;
  border-radius: 12px;
  padding: 40px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
}

.login-header {
  text-align: center;
  margin-bottom: 30px;
}

.login-header h1 {
  color: #333;
  margin: 0 0 10px 0;
  font-size: 28px;
}

.login-header p {
  color: #666;
  margin: 0;
  font-size: 16px;
}

.error-message {
  background-color: #fee;
  color: #c33;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 20px;
  border: 1px solid #fcc;
  font-size: 14px;
}

.login-form {
  margin-bottom: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #333;
  font-weight: 500;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 16px;
  transition: border-color 0.3s;
}

.form-group input:focus {
  outline: none;
  border-color: #667eea;
}

.login-button {
  width: 100%;
  padding: 14px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.3s;
}

.login-button:hover:not(:disabled) {
  opacity: 0.9;
}

.login-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.oauth-section {
  margin-bottom: 20px;
}

.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 20px 0;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #eee;
}

.divider span {
  padding: 0 10px;
  color: #999;
  font-size: 14px;
}

.oauth-button {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  background: white;
  color: #333;
  font-size: 14px;
  cursor: pointer;
  margin-bottom: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  transition: background-color 0.3s;
}

.oauth-button:hover:not(:disabled) {
  background-color: #f5f5f5;
}

.oauth-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.google-icon {
  font-weight: bold;
  color: #4285f4;
}

.line-icon {
  font-weight: bold;
  color: #06c755;
}

.register-section {
  text-align: center;
  color: #666;
  font-size: 14px;
}

.register-link {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.register-link:hover {
  text-decoration: underline;
}

.loading-spinner {
  display: inline-block;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>