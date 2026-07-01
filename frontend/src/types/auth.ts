// Authentication related types
export interface User {
  id: number
  email: string
  username?: string
  role: 'user' | 'admin'
  createdAt: string
  updatedAt: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  user: User
  expiresIn: number // seconds
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  username?: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface OAuthCallbackRequest {
  code: string
  provider: 'google' | 'line'
  redirectUri: string
}

// API Error types
export interface ApiError {
  errorCode: string
  message: string
  details?: any
  timestamp: string
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: ApiError
}