// Common types used across the application
export interface PaginationParams {
  page: number
  pageSize: number
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  hasMore: boolean
}

export interface LatLng {
  lat: number
  lng: number
}

export interface BoundingBox {
  north: number
  south: number
  east: number
  west: number
}

export type LoadingState = 'idle' | 'loading' | 'success' | 'error'

export interface ErrorResponse {
  success: false
  error: {
    errorCode: string
    message: string
    timestamp: string
  }
}