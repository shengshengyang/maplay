<template>
  <div class="spot-popup">
    <div class="spot-popup-header">
      <div class="spot-icon">{{ getCategoryIcon(spot.category) }}</div>
      <div class="spot-title">
        <h3>{{ spot.name }}</h3>
        <span class="spot-category">{{ getCategoryLabel(spot.category) }}</span>
      </div>
    </div>

    <div class="spot-popup-content">
      <div v-if="spot.address" class="spot-info">
        <span class="info-icon">📍</span>
        <span>{{ spot.address }}</span>
      </div>

      <div v-if="spot.description" class="spot-description">
        {{ spot.description }}
      </div>

      <div v-if="spot.ageGroups && spot.ageGroups.length > 0" class="spot-info">
        <span class="info-icon">👶</span>
        <span>{{ getAgeGroupsLabel(spot.ageGroups) }}</span>
      </div>

      <div v-if="spot.facilities && spot.facilities.length > 0" class="spot-facilities">
        <div class="facilities-label">設施：</div>
        <div class="facilities-list">
          <span
            v-for="facility in spot.facilities"
            :key="facility"
            class="facility-tag"
          >
            {{ getFacilityLabel(facility) }}
          </span>
        </div>
      </div>

      <div v-if="spot.spotType === 'temporary'" class="spot-temporary">
        <span class="temp-icon">⏰</span>
        <span>期間限定：</span>
        <span>{{ formatDateRange(spot.startDate, spot.endDate) }}</span>
      </div>
    </div>

    <div class="spot-popup-actions">
      <button @click="viewDetails" class="btn-details">
        查看詳情
      </button>
      <button @click="openInGoogleMaps" class="btn-navigation">
        🧭 Google 導航
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { type PropType } from 'vue'
import { useRouter } from 'vue-router'
import type { Spot, SpotCategory, AgeGroup, Facility } from '@/types'

const props = defineProps({
  spot: {
    type: Object as PropType<Spot>,
    required: true,
  },
})

const router = useRouter()

const getCategoryIcon = (category: SpotCategory): string => {
  const iconMap: Record<SpotCategory, string> = {
    park: '🌳',
    restaurant: '🍽️',
    'nursing-room': '👶',
    medical: '🏥',
    activity: '🎪',
    other: '📍',
  }
  return iconMap[category] || '📍'
}

const getCategoryLabel = (category: SpotCategory): string => {
  const labelMap: Record<SpotCategory, string> = {
    park: '公園',
    restaurant: '親子餐廳',
    'nursing-room': '哺乳室/廁所',
    medical: '醫療機構',
    activity: '活動',
    other: '其他',
  }
  return labelMap[category] || '其他'
}

const getAgeGroupsLabel = (ageGroups: AgeGroup[]): string => {
  const labelMap: Record<AgeGroup, string> = {
    '0-3': '0-3歲',
    '3-7': '3-7歲',
    '7-12': '7-12歲',
    '12+': '12歲以上',
  }
  return ageGroups.map(age => labelMap[age]).join('、')
}

const getFacilityLabel = (facility: Facility): string => {
  const labelMap: Record<Facility, string> = {
    'parking': '停車場',
    'stroller-accessible': '嬰兒車��進',
    'diaper-changing-station': '尿布台',
    'playground': '遊樂設施',
    'rest-area': '休息區',
    'feeding-room': '哺乳室',
    'toilet': '廁所',
    'water-fountain': '飲水機',
    'shaded-area': '遮陽',
    'wifi': 'WiFi',
    'other': '其他',
  }
  return labelMap[facility] || facility
}

const formatDateRange = (startDate?: string, endDate?: string): string => {
  if (!startDate || !endDate) return ''

  const start = new Date(startDate)
  const end = new Date(endDate)

  const formatDate = (date: Date) => {
    return `${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`
  }

  return `${formatDate(start)} - ${formatDate(end)}`
}

const viewDetails = () => {
  router.push({ name: 'spot-detail', params: { id: props.spot.id } })
}

const openInGoogleMaps = () => {
  const { lat, lng, name } = props.spot
  const url = `https://maps.google.com/?q=${lat},${lng}&query=${encodeURIComponent(name)}`
  window.open(url, '_blank')
}
</script>

<style scoped>
.spot-popup {
  min-width: 300px;
  max-width: 400px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
}

.spot-popup-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #eee;
}

.spot-icon {
  font-size: 32px;
  line-height: 1;
}

.spot-title {
  flex: 1;
}

.spot-title h3 {
  margin: 0 0 4px 0;
  font-size: 18px;
  color: #333;
  line-height: 1.3;
}

.spot-category {
  font-size: 13px;
  color: #666;
  background: #f5f5f5;
  padding: 2px 8px;
  border-radius: 12px;
}

.spot-popup-content {
  margin-bottom: 16px;
}

.spot-info {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  font-size: 14px;
  color: #555;
}

.info-icon {
  font-size: 16px;
  line-height: 1;
}

.spot-description {
  margin-bottom: 12px;
  font-size: 14px;
  color: #666;
  line-height: 1.5;
}

.spot-facilities {
  margin-bottom: 12px;
}

.facilities-label {
  font-size: 13px;
  color: #666;
  margin-bottom: 8px;
}

.facilities-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.facility-tag {
  font-size: 12px;
  color: #555;
  background: #f0f0f0;
  padding: 4px 8px;
  border-radius: 4px;
}

.spot-temporary {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #856404;
  background: #fff3cd;
  padding: 8px;
  border-radius: 6px;
  border: 1px solid #ffc107;
}

.temp-icon {
  font-size: 14px;
}

.spot-popup-actions {
  display: flex;
  gap: 8px;
}

.spot-popup-actions button {
  flex: 1;
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-details {
  background: #667eea;
  color: white;
}

.btn-details:hover {
  background: #5568d3;
}

.btn-navigation {
  background: #4285f4;
  color: white;
}

.btn-navigation:hover {
  background: #3367d6;
}
</style>