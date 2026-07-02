import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import favoritesService from '@/services/favoritesService'

interface FavoriteSpot {
  id: string
  name: string
  category: string
  lat: number
  lng: number
  createdAt: string
  thumbnailUrl?: string
}

export const useFavoritesStore = defineStore('favorites', () => {
  // State
  const favorites = ref<FavoriteSpot[]>([])
  const isFavoritedMap = ref<Record<string, boolean>>({})
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const favoritedSpots = computed(() => favorites.value)
  const favoriteCount = computed(() => favorites.value.length)

  // Helper to check if a spot is favorited
  function isFavorited(spotId: string): boolean {
    return !!isFavoritedMap.value[spotId]
  }

  // Actions
  async fetchFavorites(page: number = 1, pageSize: number = 20) {
    loading.value = true
    error.value = null

    try {
      const response = await favoritesService.getFavorites(page, pageSize)
      favorites.value = response.favorites

      // Update isFavoritedMap
      const newMap: Record<string, boolean> = {}
      response.favorites.forEach(fav => {
        newMap[fav.id] = true
      })
      isFavoritedMap.value = { ...isFavoritedMap.value, ...newMap }
    } catch (err: any) {
      error.value = err.response?.data?.message || '載入收藏列表失敗'
      throw err
    } finally {
      loading.value = false
    }
  }

  async addFavorite(spotId: string) {
    loading.value = true
    error.value = null

    try {
      // Optimistic update
      isFavoritedMap.value[spotId] = true

      await favoritesService.addFavorite(spotId)
    } catch (err: any) {
      // Rollback on error
      delete isFavoritedMap.value[spotId]
      error.value = err.response?.data?.message || '收藏失敗'
      throw err
    } finally {
      loading.value = false
    }
  }

  async removeFavorite(spotId: string) {
    loading.value = true
    error.value = null

    try {
      // Optimistic update
      delete isFavoritedMap.value[spotId]
      favorites.value = favorites.value.filter(f => f.id !== spotId)

      await favoritesService.removeFavorite(spotId)
    } catch (err: any) {
      // Rollback on error
      isFavoritedMap.value[spotId] = true
      // Need to re-fetch to restore correct list
      await fetchFavorites()
      error.value = err.response?.data?.message || '取消收藏失敗'
      throw err
    } finally {
      loading.value = false
    }
  }

  async checkFavorited(spotId: string) {
    // Check cache first
    if (isFavoritedMap.value[spotId] !== undefined) {
      return isFavoritedMap.value[spotId]
    }

    try {
      const result = await favoritesService.checkFavorited(spotId)
      isFavoritedMap.value[spotId] = result.isFavorited
      return result.isFavorited
    } catch (err) {
      console.error('Failed to check favorite status:', err)
      return false
    }
  }

  function clearFavorites() {
    favorites.value = []
    isFavoritedMap.value = {}
    error.value = null
  }

  return {
    // State
    favorites,
    isFavoritedMap,
    loading,
    error,
    // Getters
    favoritedSpots,
    favoriteCount,
    // Actions
    fetchFavorites,
    addFavorite,
    removeFavorite,
    checkFavorited,
    isFavorited,
    clearFavorites
  }
}, {
  persist: {
    key: 'favorites',
    storage: localStorage,
    pick: ['isFavoritedMap']
  }
})
