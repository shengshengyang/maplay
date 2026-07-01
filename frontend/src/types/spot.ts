// Spot related types
export interface Spot {
  id: number
  name: string
  category: SpotCategory
  spotType: 'permanent' | 'temporary'
  address: string
  lat: number
  lng: number
  description?: string
  ageGroups: AgeGroup[]
  facilities: Facility[]
  startDate?: string // ISO 8601 date string
  endDate?: string // ISO 8601 date string
  status: 'pending' | 'approved' | 'rejected'
  submittedBy?: number
  createdAt: string // ISO 8601 datetime string
  updatedAt: string // ISO 8601 datetime string
  deletedAt?: string // ISO 8601 datetime string
  distanceMeters?: number // For nearby queries
  images?: SpotImage[]
  averageRating?: number
  averageCleanLevel?: number
  reviewCount?: number
  reviews?: Review[]
}

export interface SpotImage {
  id: number
  spotId: number
  url: string
  isCover: boolean
  uploadedBy: number
  createdAt: string
}

export interface Review {
  id: number
  spotId: number
  userId: number
  rating: number
  cleanLevel: number
  comment?: string
  createdAt: string
  updatedAt: string
}

export type SpotCategory =
  | 'park'
  | 'restaurant'
  | 'nursing-room'
  | 'medical'
  | 'activity'
  | 'other'

export type AgeGroup = '0-3' | '3-7' | '7-12' | '12+'

export type Facility =
  | 'parking'
  | 'stroller-accessible'
  | 'diaper-changing-station'
  | 'playground'
  | 'rest-area'
  | 'feeding-room'
  | 'toilet'
  | 'water-fountain'
  | 'shaded-area'
  | 'wifi'
  | 'other'

// API Request/Response types
export interface NearbySpotsQuery {
  lat: number
  lng: number
  radius?: number
  category?: SpotCategory
  age?: AgeGroup
  spotType?: 'permanent' | 'temporary'
  page?: number
  pageSize?: number
}

export interface CreateSpotRequest {
  name: string
  category: SpotCategory
  spotType: 'permanent' | 'temporary'
  address: string
  lat: number
  lng: number
  description?: string
  ageGroups: AgeGroup[]
  facilities: Facility[]
  startDate?: string
  endDate?: string
}

export interface NearbySpotsResponse {
  data: Spot[]
  total: number
  page: number
  pageSize: number
}