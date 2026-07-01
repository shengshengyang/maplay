<template>
  <div class="spot-detail-view">
    <div v-if="loading" class="loading">
      <div class="loading-spinner"></div>
      <p>載入中...</p>
    </div>

    <div v-else-if="error" class="error">
      <p>{{ error }}</p>
      <button @click="goBack" class="btn-back">返回</button>
    </div>

    <div v-else-if="spot" class="spot-detail">
      <!-- Header section -->
      <div class="detail-header">
        <button @click="goBack" class="btn-back">← 返回</button>
        <div class="header-content">
          <div class="spot-icon">{{ getCategoryIcon(spot.category) }}</div>
          <div class="header-text">
            <h1>{{ spot.name }}</h1>
            <span class="category-badge">{{ getCategoryLabel(spot.category) }}</span>
          </div>
        </div>
        <button
          @click="openInGoogleMaps"
          class="btn-navigation"
        >
          🧭 Google 導航
        </button>
      </div>

      <!-- Main content -->
      <div class="detail-content">
        <!-- Images section -->
        <div v-if="spot.images && spot.images.length > 0" class="images-section">
          <div class="images-gallery">
            <div
              v-for="image in spot.images"
              :key="image.id"
              class="image-item"
              :class="{ 'is-cover': image.isCover }"
            >
              <img :src="image.url" :alt="spot.name" />
              <span v-if="image.isCover" class="cover-badge">封面</span>
            </div>
          </div>
        </div>

        <!-- Basic information -->
        <div class="info-section">
          <h2>基本資訊</h2>
          <div class="info-grid">
            <div v-if="spot.address" class="info-item">
              <span class="info-label">地址：</span>
              <span>{{ spot.address }}</span>
            </div>

            <div v-if="spot.description" class="info-item full-width">
              <span class="info-label">描述：</span>
              <p>{{ spot.description }}</p>
            </div>

            <div class="info-item">
              <span class="info-label">類型：</span>
              <span>{{ spot.spotType === 'permanent' ? '永久' : '期間限定' }}</span>
            </div>

            <div v-if="spot.spotType === 'temporary' && spot.startDate && spot.endDate" class="info-item full-width">
              <span class="info-label">期間：</span>
              <span>{{ formatDateRange(spot.startDate, spot.endDate) }}</span>
            </div>

            <div v-if="spot.ageGroups && spot.ageGroups.length > 0" class="info-item">
              <span class="info-label">適合年齡：</span>
              <span>{{ getAgeGroupsLabel(spot.ageGroups) }}</span>
            </div>

            <div v-if="spot.distanceMeters" class="info-item">
              <span class="info-label">距離：</span>
              <span>{{ formatDistance(spot.distanceMeters) }}</span>
            </div>
          </div>
        </div>

        <!-- Facilities section -->
        <div v-if="spot.facilities && spot.facilities.length > 0" class="facilities-section">
          <h2>設施</h2>
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

        <!-- Reviews section -->
        <div v-if="spot.reviews && spot.reviews.length > 0" class="reviews-section">
          <h2>最新評價</h2>
          <div class="reviews-summary">
            <div class="rating-summary">
              <span class="rating-score">{{ spot.averageRating?.toFixed(1) || 'N/A' }}</span>
              <span class="rating-label">平均評分</span>
            </div>
            <div class="review-count">
              {{ spot.reviewCount || 0 }} 則評價
            </div>
          </div>
          <div class="reviews-list">
            <div
              v-for="review in spot.reviews.slice(0, 3)"
              :key="review.id"
              class="review-item"
            >
              <div class="review-rating">
                評分：{{ review.rating }}/5
                <span v-if="review.cleanLevel"> | 清潔度：{{ review.cleanLevel }}/5</span>
              </div>
              <p v-if="review.comment" class="review-comment">{{ review.comment }}</p>
              <div class="review-date">{{ formatDate(review.createdAt) }}</div>
            </div>
          </div>
        </div>

        <!-- Actions section -->
        <div class="actions-section">
          <button
            @click="reportIssue"
            class="btn-action btn-report"
          >
            🚩 回報問題
          </button>
          <button
            @click="shareSpot"
            class="btn-action btn-share"
          >
            📤 分享景點
          </button>
        </div>
      </div>
    </div>

    <div v-else class="not-found">
      <p>找不到該景點</p>
      <button @click="goBack" class="btn-back">返回</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import spotService from '@/services/spotService'
import type { Spot, SpotCategory, AgeGroup, Facility } from '@/types'

const route = useRoute()
const router = useRouter()

const spot = ref<Spot | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const loadSpotDetail = async () => {
  const spotId = Number(route.params.id)
  if (!spotId) {
    error.value = '無效的景點 ID'
    loading.value = false
    return
  }

  loading.value = true
  error.value = null

  try {
    const spotData = await spotService.getSpotDetail(spotId)
    spot.value = spotData
  } catch (err: any) {
    error.value = err.message || '載入景點詳情失敗'
  } finally {
    loading.value = false
  }
}

const goBack = () => {
  router.back()
}

const openInGoogleMaps = () => {
  if (!spot.value) return

  const { lat, lng, name } = spot.value
  const url = `https://maps.google.com/?q=${lat},${lng}&query=${encodeURIComponent(name)}`
  window.open(url, '_blank')
}

const reportIssue = () => {
  // TODO: Implement issue reporting
  alert('問題回報功能開發中')
}

const shareSpot = () => {
  if (!spot.value) return

  if (navigator.share) {
    navigator.share({
      title: spot.value.name,
      text: `${spot.value.name} - ${getCategoryLabel(spot.value.category)}`,
      url: window.location.href,
    }).catch(console.error)
  } else {
    // Fallback: copy URL to clipboard
    navigator.clipboard.writeText(window.location.href)
    alert('連結已複製到剪貼簿')
  }
}

// Helper functions
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
    'stroller-accessible': '嬰兒車可進',
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

const formatDateRange = (startDate: string, endDate: string): string => {
  const start = new Date(startDate)
  const end = new Date(endDate)

  const formatDate = (date: Date) => {
    return `${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`
  }

  return `${formatDate(start)} - ${formatDate(end)}`
}

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return `${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`
}

const formatDistance = (meters: number): string => {
  if (meters < 1000) {
    return `${Math.round(meters)} 公尺`
  } else {
    return `${(meters / 1000).toFixed(1)} 公里`
  }
}

onMounted(() => {
  loadSpotDetail()
})
</script>

<style scoped>
.spot-detail-view {
  min-height: 100vh;
  background: #f5f5f5;
}

.loading,
.error,
.not-found {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  padding: 20px;
  text-align: center;
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

.spot-detail {
  max-width: 800px;
  margin: 0 auto;
  background: white;
  min-height: 100vh;
}

.detail-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.btn-back {
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  align-self: flex-start;
}

.btn-back:hover {
  background: rgba(255, 255, 255, 0.3);
}

.header-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.spot-icon {
  font-size: 48px;
  line-height: 1;
}

.header-text h1 {
  margin: 0 0 8px 0;
  font-size: 24px;
  line-height: 1.3;
}

.category-badge {
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 14px;
}

.btn-navigation {
  background: white;
  color: #667eea;
  border: none;
  padding: 12px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 500;
  align-self: flex-start;
}

.btn-navigation:hover {
  background: #f0f0f0;
}

.detail-content {
  padding: 20px;
}

.info-section,
.facilities-section,
.reviews-section,
.actions-section {
  margin-bottom: 32px;
}

.info-section h2,
.facilities-section h2,
.reviews-section h2 {
  margin: 0 0 16px 0;
  font-size: 20px;
  color: #333;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-item.full-width {
  grid-column: 1 / -1;
}

.info-label {
  font-weight: 500;
  color: #666;
  font-size: 14px;
}

.facilities-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.facility-tag {
  background: #f0f0f0;
  color: #555;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 14px;
}

.images-section {
  margin-bottom: 32px;
}

.images-gallery {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 16px;
}

.image-item {
  position: relative;
  aspect-ratio: 4 / 3;
  overflow: hidden;
  border-radius: 8px;
}

.image-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cover-badge {
  position: absolute;
  bottom: 8px;
  right: 8px;
  background: rgba(102, 126, 234, 0.9);
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.reviews-summary {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
  padding: 16px;
  background: #f9f9f9;
  border-radius: 8px;
}

.rating-summary {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.rating-score {
  font-size: 32px;
  font-weight: bold;
  color: #667eea;
}

.rating-label {
  font-size: 14px;
  color: #666;
}

.review-count {
  color: #666;
  font-size: 14px;
}

.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.review-item {
  padding: 16px;
  background: #f9f9f9;
  border-radius: 8px;
}

.review-rating {
  font-weight: 500;
  color: #333;
  margin-bottom: 8px;
}

.review-comment {
  color: #555;
  margin: 8px 0;
  line-height: 1.5;
}

.review-date {
  color: #999;
  font-size: 13px;
}

.actions-section {
  display: flex;
  gap: 12px;
}

.btn-action {
  flex: 1;
  padding: 12px;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  cursor: pointer;
  transition: opacity 0.2s;
}

.btn-action:hover {
  opacity: 0.9;
}

.btn-report {
  background: #ffc107;
  color: #333;
}

.btn-share {
  background: #4285f4;
  color: white;
}

@media (max-width: 768px) {
  .info-grid {
    grid-template-columns: 1fr;
  }

  .actions-section {
    flex-direction: column;
  }
}
</style>