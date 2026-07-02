import apiService from './apiService'
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RefreshTokenRequest,
  OAuthCallbackRequest,
  User
} from '@/types'

class AuthService {
  private readonly AUTH_ENDPOINTS = {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    REFRESH: '/api/auth/refresh-token',
    LOGOUT: '/api/auth/logout',
    VERIFY: '/api/auth/verify',
    OAUTH_CALLBACK: '/api/auth/oauth/callback',
  }

  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await apiService.post<AuthResponse>(
      this.AUTH_ENDPOINTS.LOGIN,
      credentials
    )
    return response.data
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await apiService.post<AuthResponse>(
      this.AUTH_ENDPOINTS.REGISTER,
      userData
    )
    return response.data
  }

  async refreshToken(refreshToken: string): Promise<{ accessToken: string }> {
    const request: RefreshTokenRequest = { refreshToken }
    const response = await apiService.post<{ accessToken: string }>(
      this.AUTH_ENDPOINTS.REFRESH,
      request
    )
    return response.data
  }

  async logout(): Promise<void> {
    try {
      await apiService.post(this.AUTH_ENDPOINTS.LOGOUT)
    } catch (error) {
      // Even if logout fails on server, clear local storage
      console.error('Logout failed on server, clearing local storage')
    } finally {
      this.clearLocalAuth()
    }
  }

  async verifyToken(): Promise<User> {
    const response = await apiService.get<User>(this.AUTH_ENDPOINTS.VERIFY)
    return response.data
  }

  async oauthCallback(callbackData: OAuthCallbackRequest): Promise<AuthResponse> {
    const response = await apiService.post<AuthResponse>(
      this.AUTH_ENDPOINTS.OAUTH_CALLBACK,
      callbackData
    )
    return response.data
  }

  getGoogleOAuthUrl(): string {
    const redirectUri = `${window.location.origin}/login`
    // This should match your backend OAuth configuration
    return `https://accounts.google.com/o/oauth2/v2/auth?client_id=${
      import.meta.env.VITE_GOOGLE_CLIENT_ID || ''
    }&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=code&scope=openid email profile`
  }

  getLineOAuthUrl(): string {
    const redirectUri = `${window.location.origin}/login`
    // This should match your backend OAuth configuration
    return `https://access.line.me/oauth2/v2.1/authorize?response_type=code&client_id=${
      import.meta.env.VITE_LINE_CLIENT_ID || ''
    }&redirect_uri=${encodeURIComponent(redirectUri)}&state=state&scope=openid email profile`
  }

  private clearLocalAuth(): void {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }
}

export default new AuthService()