import apiService from './apiService'

interface CreateFavoriteRequest {
  spotId: string
}

interface FavoriteSpot {
  id: string
  name: string
  category: string
  lat: number
  lng: number
  createdAt: string
  thumbnailUrl?: string
}

interface FavoriteListResponse {
  favorites: FavoriteSpot[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}

interface CheckFavoriteResponse {
  spotId: string
  isFavorited: boolean
}

class FavoritesService {
  private readonly FAVORITES_ENDPOINTS = {
    ADD: '/api/favorites',
    REMOVE: '/api/favorites',
    LIST: '/api/favorites',
    CHECK: '/api/favorites/check',
  }

  async addFavorite(spotId: string): Promise<void> {
    const request: CreateFavoriteRequest = { spotId }
    await apiService.post(this.FAVORITES_ENDPOINTS.ADD, request)
  }

  async removeFavorite(spotId: string): Promise<void> {
    await apiService.delete(`${this.FAVORITES_ENDPOINTS.REMOVE}/${spotId}`)
  }

  async getFavorites(page: number = 1, pageSize: number = 20): Promise<FavoriteListResponse> {
    const response = await apiService.get<FavoriteListResponse>(this.FAVORITES_ENDPOINTS.LIST, {
      page,
      pageSize
    })
    return response.data
  }

  async checkFavorited(spotId: string): Promise<CheckFavoriteResponse> {
    const response = await apiService.get<CheckFavoriteResponse>(`${this.FAVORITES_ENDPOINTS.CHECK}/${spotId}`)
    return response.data
  }
}

export default new FavoritesService()
