<template>
  <div class="map-component">
    <div ref="mapContainer" class="map-container"></div>
    <div v-if="loading" class="map-loading">
      <div class="loading-spinner"></div>
      <p>載入地圖中...</p>
    </div>
    <div v-if="error" class="map-error">
      <p>地圖載入失敗</p>
      <button @click="initMap" class="retry-button">重試</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

interface Props {
  center?: [number, number]
  zoom?: number
  loading?: boolean
}

// Props definition
const props = withDefaults(defineProps<Props>(), {
  center: () => [24.1477, 120.6736] as [number, number],
  zoom: 13,
  loading: false,
})

// Emit events
const emit = defineEmits<{
  mapClick: [lat: number, lng: number]
  mapReady: [map: L.Map]
  markerClick: [spot: any]
}>()

// Component state
const mapContainer = ref<HTMLDivElement>()
const map = ref<L.Map>()
const error = ref(false)

// Marker management
const spots = ref<any[]>([])
const markers = ref<L.Marker[]>([])
const currentLocationMarker = ref<L.Marker>()

// Marker icon creation
const createSpotIcon = (spot: any) => {
  const categoryIcons: Record<string, string> = {
    park: '🌳',
    restaurant: '🍽️',
    'nursing-room': '👶',
    medical: '🏥',
    activity: '🎪',
    other: '📍',
  }

  const icon = categoryIcons[spot.category] || '📍'
  const isTemporary = spot.spotType === 'temporary'

  // Create custom HTML icon
  return L.divIcon({
    html: `
      <div class="custom-marker ${isTemporary ? 'temporary' : ''}">
        <span class="marker-icon">${icon}</span>
        ${isTemporary ? '<span class="temp-indicator">⏰</span>' : ''}
      </div>
    `,
    className: 'custom-marker-container',
    iconSize: [40, 40],
    iconAnchor: [20, 40],
    popupAnchor: [0, -40],
  })
}

const clearMarkers = () => {
  markers.value.forEach(marker => marker.remove())
  markers.value = []
}

const addMarkers = (spotsData: any[]) => {
  if (!map.value) return

  clearMarkers()

  spotsData.forEach(spot => {
    const marker = L.marker([spot.lat, spot.lng], {
      icon: createSpotIcon(spot),
    })

    // Create popup content using Vue component
    const popupContent = document.createElement('div')
    const popupApp = h(SpotPopup, { spot })
    // Note: For proper Vue component rendering in Leaflet popups,
    // we would need to use a mounting approach. For now, use basic HTML
    popupContent.innerHTML = `
      <div class="simple-spot-popup">
        <h3>${spot.name}</h3>
        <p class="category">${getCategoryLabel(spot.category)}</p>
        <p class="address">📍 ${spot.address || ''}</p>
        <button onclick="window.viewSpotDetails(${spot.id})">查看詳情</button>
      </div>
    `

    const popup = L.popup({
      maxWidth: 400,
      className: 'custom-popup'
    }).setContent(popupContent)

    marker.bindPopup(popup)

    marker.on('click', () => {
      emit('markerClick', spot)
    })

    marker.addTo(map.value!)
    markers.value.push(marker)
  })
}

// Helper function for category labels
const getCategoryLabel = (category: string): string => {
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

const updateMarkers = () => {
  addMarkers(spots.value)
}

const setCurrentLocation = (lat: number, lng: number) => {
  if (!map.value) return

  // Remove existing location marker
  if (currentLocationMarker.value) {
    currentLocationMarker.value.remove()
  }

  // Add new location marker
  const locationIcon = L.divIcon({
    html: '<div class="current-location-marker"></div>',
    className: 'current-location-container',
    iconSize: [20, 20],
    iconAnchor: [10, 10],
  })

  currentLocationMarker.value = L.marker([lat, lng], {
    icon: locationIcon,
  }).addTo(map.value!)

  map.value.setView([lat, lng], 15)
}

// Watch for prop changes

// Fix for default marker icons in Leaflet with webpack
const initIconDefaults = () => {
  delete (L.Icon.Default.prototype as any)._getIconUrl
  L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
    iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  })
}

const initMap = () => {
  if (!mapContainer.value || map.value) return

  try {
    error.value = false

    // Initialize icon defaults
    initIconDefaults()

    // Create map instance
    map.value = L.map(mapContainer.value, {
      center: props.center,
      zoom: props.zoom,
      zoomControl: true,
    })

    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19,
    }).addTo(map.value)

    // Add click event listener
    map.value.on('click', (e: L.LeafletMouseEvent) => {
      const { lat, lng } = e.latlng
      emit('mapClick', lat, lng)
    })

    // Emit map ready event
    emit('mapReady', map.value)

    // Try to get user location
    getUserLocation()
  } catch (err) {
    console.error('Failed to initialize map:', err)
    error.value = true
  }
}

const getUserLocation = () => {
  if (!map.value) return

  if ('geolocation' in navigator) {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        const { latitude, longitude } = position.coords
        map.value?.setView([latitude, longitude], 15)
      },
      (err) => {
        console.warn('Geolocation failed, using default location:', err)
        // Fall back to default location (already set)
      }
    )
  }
}

// Watch for prop changes
watch(() => props.center, (newCenter) => {
  if (map.value && newCenter) {
    map.value.setView(newCenter)
  }
})

watch(() => props.zoom, (newZoom) => {
  if (map.value) {
    map.value.setZoom(newZoom)
  }
})

// Lifecycle hooks
onMounted(() => {
  // Delay map initialization to ensure DOM is ready
  setTimeout(() => {
    initMap()
  }, 100)
})

onUnmounted(() => {
  if (map.value) {
    map.value.remove()
    map.value = undefined
  }
})

// Expose map instance and methods for parent components
defineExpose({
  map,
  initMap,
  getUserLocation,
  clearMarkers,
  addMarkers,
  updateMarkers,
  setCurrentLocation,
  spots,
})
</script>

<style scoped>
.map-component {
  position: relative;
  width: 100%;
  height: 100%;
}

.map-container {
  width: 100%;
  height: 100%;
  min-height: 400px;
  border-radius: 8px;
  overflow: hidden;
}

.map-loading,
.map-error {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  background: rgba(255, 255, 255, 0.9);
  z-index: 1000;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.map-error p {
  color: #c33;
  margin-bottom: 16px;
}

.retry-button {
  padding: 10px 20px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}

.retry-button:hover {
  background: #5568d3;
}

/* Custom marker styles */
:deep(.custom-marker-container) {
  background: transparent;
  border: none;
}

.custom-marker {
  position: relative;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  background: white;
  border-radius: 50%;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  transition: transform 0.2s;
}

.custom-marker:hover {
  transform: scale(1.1);
}

.custom-marker.temporary {
  background: #fff3cd;
  border: 2px solid #ffc107;
}

.marker-icon {
  line-height: 1;
}

.temp-indicator {
  position: absolute;
  top: -5px;
  right: -5px;
  font-size: 14px;
  background: #ffc107;
  border-radius: 50%;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid white;
}

/* Current location marker */
:deep(.current-location-container) {
  background: transparent;
  border: none;
}

.current-location-marker {
  width: 20px;
  height: 20px;
  background: #4285f4;
  border: 3px solid white;
  border-radius: 50%;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.2);
    opacity: 0.8;
  }
}

/* Simple popup styling for Leaflet */
:deep(.simple-spot-popup) {
  min-width: 250px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.simple-spot-popup h3 {
  margin: 0 0 8px 0;
  font-size: 16px;
  color: #333;
}

.simple-spot-popup .category {
  font-size: 13px;
  color: #666;
  background: #f5f5f5;
  padding: 2px 8px;
  border-radius: 12px;
  display: inline-block;
  margin-bottom: 8px;
}

.simple-spot-popup .address {
  font-size: 14px;
  color: #555;
  margin: 8px 0;
}

.simple-spot-popup button {
  width: 100%;
  padding: 8px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-top: 8px;
}

.simple-spot-popup button:hover {
  background: #5568d3;
}
</style>