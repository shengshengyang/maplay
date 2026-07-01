<template>
  <div class="home-view">
    <div class="home-header">
      <h1>親子資源地圖</h1>
      <p>探索附近的親子友善景點</p>
    </div>

    <div class="home-content">
      <!-- Map section -->
      <div class="map-section">
        <MapComponent
          ref="mapComponent"
          :center="mapCenter"
          :zoom="mapZoom"
          :loading="loading"
          @map-click="handleMapClick"
          @marker-click="handleMarkerClick"
          @map-ready="handleMapReady"
        />
      </div>

      <!-- Control panel -->
      <div class="control-panel">
        <!-- Search section -->
        <div class="panel-section">
          <h3>搜尋附近景點</h3>
          <button
            @click="searchNearbySpots"
            :disabled="loading"
            class="btn-primary"
          >
            <span v-if="!loading">🔍 搜尋附近</span>
            <span v-else>搜尋中...</span>
          </button>
        </div>

        <!-- Current location -->
        <div class="panel-section">
          <h3>目前位置</h3>
          <div class="location-info">
            <div v-if="currentLocation">
              <p>緯度: {{ currentLocation.lat.toFixed(6) }}</p>
              <p>經度: {{ currentLocation.lng.toFixed(6) }}</p>
            </div>
            <div v-else>
              <p>使用預設位置 (台中市)</p>
            </div>
          </div>
          <button
            @click="getCurrentLocation"
            :disabled="loading"
            class="btn-secondary"
          >
            📍 取得目前位置
          </button>
        </div>

        <!-- Results summary -->
        <div v-if="spots.length > 0" class="panel-section">
          <h3>搜尋結果</h3>
          <p>找到 {{ spots.length }} 個景點</p>
          <div class="spots-summary">
            <div v-for="spot in spots.slice(0, 5)" :key="spot.id" class="spot-item">
              <span class="spot-icon">{{ getSpotIcon(spot.category) }}</span>
              <div class="spot-info">
                <div class="spot-name">{{ spot.name }}</div>
                <div class="spot-address">{{ spot.address }}</div>
              </div>
              <button @click="viewSpotDetails(spot.id)" class="btn-view">查看</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import MapComponent from '@/components/MapComponent.vue'
import { useSpotsStore } from '@/stores/spots'
import spotService from '@/services/spotService'

const router = useRouter()
const spotsStore = useSpotsStore()

const mapComponent = ref<InstanceType<typeof MapComponent>>()
const loading = ref(false)
const currentLocation = ref<{ lat: number; lng: number } | null>(null)

const mapCenter = ref<[number, number]>([24.1477, 120.6736])
const mapZoom = ref(13)

const spots = computed(() => spotsStore.spots)

const handleMapReady = (map: any) => {
  console.log('Map ready:', map)
  // Auto-search nearby spots when map is ready
  searchNearbySpots()
}

const handleMapClick = (lat: number, lng: number) => {
  console.log('Map clicked:', lat, lng)
  // Could be used for adding new spots
}

const handleMarkerClick = (spot: any) => {
  console.log('Marker clicked:', spot)
  // Show spot details or perform other actions
}

const getCurrentLocation = () => {
  if ('geolocation' in navigator) {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        const { latitude, longitude } = position.coords
        currentLocation.value = { lat: latitude, lng: longitude }
        mapCenter.value = [latitude, longitude]
        mapZoom.value = 15

        // Update map center
        if (mapComponent.value) {
          mapComponent.value.setCurrentLocation(latitude, longitude)
        }

        // Search nearby spots with new location
        searchNearbySpots()
      },
      (error) => {
        console.error('Geolocation error:', error)
        alert('無法取得您的位置，使用預設位置')
      }
    )
  } else {
    alert('您的瀏覽器不支援地理位置功能')
  }
}

const searchNearbySpots = async () => {
  loading.value = true
  spotsStore.setLoading(true)
  spotsStore.setError(null)

  try {
    const center = currentLocation.value || { lat: mapCenter.value[0], lng: mapCenter.value[1] }

    const response = await spotService.getNearbySpots({
      lat: center.lat,
      lng: center.lng,
      radius: 2000,
      page: 1,
      pageSize: 50,
    })

    spotsStore.setSpots(response.data)

    // Update map markers
    if (mapComponent.value) {
      mapComponent.value.addMarkers(response.data)
    }
  } catch (error: any) {
    console.error('Failed to search nearby spots:', error)
    spotsStore.setError(error.message || '搜尋失敗')
  } finally {
    loading.value = false
    spotsStore.setLoading(false)
  }
}

const viewSpotDetails = (spotId: number) => {
  router.push({ name: 'spot-detail', params: { id: spotId } })
}

const getSpotIcon = (category: string): string => {
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

// Setup global function for popup buttons
if (typeof window !== 'undefined') {
  window.viewSpotDetails = viewSpotDetails
}

onMounted(() => {
  // Try to get user location on mount
  getCurrentLocation()
})
</script>

<style scoped>
.home-view {
  min-height: 100vh;
  background: #f5f5f5;
}

.home-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 20px;
  text-align: center;
}

.home-header h1 {
  margin: 0 0 8px 0;
  font-size: 28px;
}

.home-header p {
  margin: 0;
  font-size: 16px;
  opacity: 0.9;
}

.home-content {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 100px);
}

.map-section {
  flex: 1;
  padding: 20px;
  min-height: 400px;
}

.control-panel {
  background: white;
  border-top: 1px solid #ddd;
  padding: 20px;
  max-height: 400px;
  overflow-y: auto;
}

.panel-section {
  margin-bottom: 24px;
  padding-bottom: 24px;
  border-bottom: 1px solid #eee;
}

.panel-section:last-child {
  border-bottom: none;
}

.panel-section h3 {
  margin: 0 0 16px 0;
  font-size: 18px;
  color: #333;
}

.btn-primary,
.btn-secondary,
.btn-view {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.2s;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  opacity: 0.9;
}

.btn-secondary {
  background: #f0f0f0;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background: #e0e0e0;
}

.btn-view {
  padding: 8px 12px;
  background: #667eea;
  color: white;
  font-size: 14px;
}

.btn-view:hover {
  background: #5568d3;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.location-info {
  background: #f9f9f9;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 12px;
}

.location-info p {
  margin: 4px 0;
  font-size: 14px;
  color: #555;
}

.spots-summary {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.spot-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f9f9f9;
  border-radius: 6px;
}

.spot-icon {
  font-size: 24px;
  line-height: 1;
}

.spot-info {
  flex: 1;
}

.spot-name {
  font-weight: 500;
  color: #333;
  margin-bottom: 4px;
}

.spot-address {
  font-size: 13px;
  color: #666;
}

@media (min-width: 768px) {
  .home-content {
    flex-direction: row;
  }

  .map-section {
    flex: 2;
  }

  .control-panel {
    flex: 1;
    max-width: 400px;
    border-top: none;
    border-left: 1px solid #ddd;
  }
}
</style>
