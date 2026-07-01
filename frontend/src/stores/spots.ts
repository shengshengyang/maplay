import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useSpotsStore = defineStore('spots', () => {
  // State
  const spots = ref<any[]>([])
  const currentSpot = ref<any>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Filter state
  const filters = ref({
    categories: [] as string[],
    ageGroups: [] as string[],
    spotType: 'all' // 'all', 'permanent', 'temporary'
  })

  // Computed
  const filteredSpots = computed(() => {
    return spots.value.filter(spot => {
      // Category filter
      if (filters.value.categories.length > 0) {
        if (!filters.value.categories.includes(spot.category)) {
          return false
        }
      }

      // Age group filter
      if (filters.value.ageGroups.length > 0) {
        const hasAgeGroup = filters.value.ageGroups.some(age =>
          spot.ageGroups?.includes(age)
        )
        if (!hasAgeGroup) {
          return false
        }
      }

      // Spot type filter
      if (filters.value.spotType !== 'all') {
        if (filters.value.spotType === 'permanent' && spot.spotType !== 'permanent') {
          return false
        }
        if (filters.value.spotType === 'temporary' && spot.spotType !== 'temporary') {
          return false
        }
      }

      return true
    })
  })

  // Actions
  function setSpots(data: any[]) {
    spots.value = data
  }

  function setCurrentSpot(spot: any) {
    currentSpot.value = spot
  }

  function setLoading(status: boolean) {
    loading.value = status
  }

  function setError(err: string | null) {
    error.value = err
  }

  function updateFilters(newFilters: Partial<typeof filters.value>) {
    filters.value = { ...filters.value, ...newFilters }
  }

  function resetFilters() {
    filters.value = {
      categories: [],
      ageGroups: [],
      spotType: 'all'
    }
  }

  return {
    spots,
    currentSpot,
    loading,
    error,
    filters,
    filteredSpots,
    setSpots,
    setCurrentSpot,
    setLoading,
    setError,
    updateFilters,
    resetFilters
  }
})