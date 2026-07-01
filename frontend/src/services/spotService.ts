import apiService from './apiService'
import type {
  Spot,
  NearbySpotsQuery,
  CreateSpotRequest,
  NearbySpotsResponse,
  SpotImage
} from '@/types'

class SpotService {
  private readonly SPOT_ENDPOINTS = {
    NEARBY: '/spots/nearby',
    DETAIL: '/spots',
    ACTIVE_TEMP: '/spots/active-temp',
    IMAGES: '/spots', // Will be combined with spot ID
  }

  async getNearbySpots(query: NearbySpotsQuery): Promise<NearbySpotsResponse> {
    const params = {
      lat: query.lat,
      lng: query.lng,
      radius: query.radius || 2000,
      ...(query.category && { category: query.category }),
      ...(query.age && { age: query.age }),
      ...(query.spotType && { spotType: query.spotType }),
      page: query.page || 1,
      pageSize: query.pageSize || 20,
    }

    const response = await apiService.get<NearbySpotsResponse>(
      this.SPOT_ENDPOINTS.NEARBY,
      params
    )
    return response.data
  }

  async getSpotDetail(id: number): Promise<Spot> {
    const response = await apiService.get<Spot>(
      `${this.SPOT_ENDPOINTS.DETAIL}/${id}`
    )
    return response.data
  }

  async getActiveTempSpots(page = 1, pageSize = 20): Promise<NearbySpotsResponse> {
    const response = await apiService.get<NearbySpotsResponse>(
      this.SPOT_ENDPOINTS.ACTIVE_TEMP,
      { page, pageSize }
    )
    return response.data
  }

  async createSpot(spotData: CreateSpotRequest): Promise<Spot> {
    const response = await apiService.post<Spot>(
      this.SPOT_ENDPOINTS.DETAIL,
      spotData
    )
    return response.data
  }

  async updateSpot(id: number, spotData: Partial<CreateSpotRequest>): Promise<Spot> {
    const response = await apiService.put<Spot>(
      `${this.SPOT_ENDPOINTS.DETAIL}/${id}`,
      spotData
    )
    return response.data
  }

  async deleteSpot(id: number): Promise<void> {
    await apiService.delete(`${this.SPOT_ENDPOINTS.DETAIL}/${id}`)
  }

  // Image operations
  async uploadSpotImage(
    spotId: number,
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<SpotImage> {
    const formData = new FormData()
    formData.append('image', file)

    const response = await apiService.upload<SpotImage>(
      `${this.SPOT_ENDPOINTS.IMAGES}/${spotId}/images`,
      formData,
      onProgress
    )
    return response.data
  }

  async uploadMultipleImages(
    spotId: number,
    files: File[],
    onProgress?: (progress: number) => void
  ): Promise<SpotImage[]> {
    const uploadPromises = files.map(file =>
      this.uploadSpotImage(spotId, file, onProgress)
    )

    return Promise.all(uploadPromises)
  }

  async deleteSpotImage(spotId: number, imageId: number): Promise<void> {
    await apiService.delete(`${this.SPOT_ENDPOINTS.IMAGES}/${spotId}/images/${imageId}`)
  }

  async setSpotImageCover(spotId: number, imageId: number): Promise<void> {
    await apiService.put(`${this.SPOT_ENDPOINTS.IMAGES}/${spotId}/images/${imageId}/cover`)
  }

  // Helper methods for spot categorization
  getSpotCategoryIcon(category: string): string {
    const iconMap: Record<string, string> = {
      park: '🌳',
      restaurant: '🍽️',
      'nursing-room': '👶',
      medical: '🏥',
      activity: '🎪',
      other: '📍',
    }
    return iconMap[category] || '📍'
  }

  getSpotCategoryLabel(category: string): string {
    const labelMap: Record<string, string> = {
      park: '公園',
      restaurant: '親子餐廳',
      'nursing-room': '哺乳室/廁所',
      medical: '醫療機構',
      activity: '活動',
      other: '其他',
    }
    return labelMap[category] || '其他'
  }

  getAgeGroupLabel(ageGroup: string): string {
    const labelMap: Record<string, string> = {
      '0-3': '0-3歲',
      '3-7': '3-7歲',
      '7-12': '7-12歲',
      '12+': '12歲以上',
    }
    return labelMap[ageGroup] || ageGroup
  }

  getFacilityLabel(facility: string): string {
    const labelMap: Record<string, string> = {
      'parking': '停車場',
      'stroller-accessible': '嬰兒車可進入',
      'diaper-changing-station': '尿布台',
      'playground': '遊樂設施',
      'rest-area': '休息區',
      'feeding-room': '哺乳室',
      'toilet': '廁所',
      'water-fountain': '飲水機',
      'shaded-area': '遮陽處',
      'wifi': '免費WiFi',
      'other': '其他設施',
    }
    return labelMap[facility] || facility
  }

  isTemporarySpotActive(spot: Spot): boolean {
    if (spot.spotType !== 'temporary' || !spot.startDate || !spot.endDate) {
      return false
    }

    const now = new Date()
    const startDate = new Date(spot.startDate)
    const endDate = new Date(spot.endDate)

    return now >= startDate && now <= endDate
  }
}

export default new SpotService()