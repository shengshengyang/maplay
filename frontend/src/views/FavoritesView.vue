<template>
  <div class="favorites-container">
    <div class="favorites-header">
      <h1>{{ $t('favorites.title') || '我��收藏' }}</h1>
      <p v-if="favoriteCount > 0" class="count">{{ $t('favorites.count', { count: favoriteCount }) || `共 ${favoriteCount} 個景點` }}</p>
    </div>

    <!-- Loading state -->
    <div v-if="loading && !favorites.length" class="loading-state">
      <div class="spinner"></div>
      <p>{{ $t('favorites.loading') || '載入中...' }}</p>
    </div>

    <!-- Error state -->
    <div v-else-if="error" class="error-state">
      <p>{{ errorMessage }}</p>
      <button @click="loadFavorites" class="retry-button">
        {{ $t('favorites.retry') || '重試' }}
      </button>
    </div>

    <!-- Empty state -->
    <div v-else-if="!favorites.length && !loading" class="empty-state">
      <div class="empty-icon">☆</div>
      <h3>{{ $t('favorites.empty.title') || '還沒有收藏任何景點' }}</h3>
      <p>{{ $t('favorites.empty.description') || '去探索更多有趣的親子景點吧！' }}</p>
      <router-link to="/" class="explore-link">
        {{ $t('favorites.explore') || '探索景點' }}
      </router-link>
    </div>

    <!-- Favorites list -->
    <div v-else class="favorites-list">
      <div
        v-for="spot in favorites"
        :key="spot.id"
        class="favorite-spot-card"
      >
        <div class="spot-image">
          <img
            v-if="spot.thumbnailUrl"
            :src="spot.thumbnailUrl"
            :alt="spot.name"
            @error="handleImageError"
          />
          <div v-else class="no-image">{{ spot.name[0] }}</div>
          <button
            @click="handleRemoveFavorite(spot.id, spot.name)"
            class="remove-btn"
            :title="$t('favorites.remove') || '取消收藏'"
          >
            ×
          </button>
        </div>

        <div class="spot-info">
          <h3>{{ spot.name }}</h3>
          <p class="spot-category">{{ $t(`categories.${spot.category}`) || spot.category }}</p>
          <p class="spot-date">{{ formatDate(spot.createdAt) }} 收藏</p>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination">
        <button
          @click="goToPage(currentPage - 1)"
          :disabled="currentPage === 1"
          class="page-btn"
        >
          ← {{ $t('pagination.previous') || '上一頁' }}
        </button>
        <span class="page-info">{{ currentPage }} / {{ totalPages }}</span>
        <button
          @click="goToPage(currentPage + 1)"
          :disabled="currentPage === totalPages"
          class="page-btn"
        >
          {{ $t('pagination.next') || '下一頁' }} →
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useFavoritesStore } from '@/stores/favorites'

const router = useRouter()
const authStore = useAuthStore()
const favoritesStore = useFavoritesStore()

const loading = ref(false)
const error = ref(false)
const errorMessage = ref('')

const currentPage = ref(1)
const pageSize = 20

const favorites = computed(() => favoritesStore.favoritedSpots)
const favoriteCount = computed(() => favoritesStore.favoriteCount)
const totalPages = computed(() =>
  favoritesStore.favoriteCount > 0
    ? Math.ceil(favoritesStore.favoriteCount / pageSize)
    : 0
)

onMounted(() => {
  if (authStore.isAuthenticated) {
    loadFavorites()
  }
})

async function loadFavorites() {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    await favoritesStore.fetchFavorites(currentPage.value, pageSize)
  } catch (err: any) {
    error.value = true
    errorMessage.value = err.response?.data?.message || '載入收藏列表失敗'
  } finally {
    loading.value = false
  }
}

async function handleRemoveFavorite(spotId: string, spotName: string) {
  if (!confirm(`確定要取消收藏「${spotName}」嗎？`)) {
    return
  }

  try {
    await favoritesStore.removeFavorite(spotId)
    // Refresh list if we're on the first page
    if (currentPage.value === 1) {
      await loadFavorites()
    }
  } catch (err: any) {
    console.error('Failed to remove favorite:', err)
    error.value = true
    errorMessage.value = err.response?.data?.message || '取消收藏失敗'
  }
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadFavorites()
}

function handleImageError(event: Event) {
  const img = event.target as HTMLImageElement
  img.style.display = 'none'
  const parent = img.parentElement
  if (parent) {
    const noImage = parent.querySelector('.no-image')
    if (noImage) {
      noImage.style.display = 'flex'
    }
  }
}

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-TW')
}
</script>

<style scoped>
.favorites-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.favorites-header {
  margin-bottom: 30px;
}

.favorites-header h1 {
  margin: 0 0 10px 0;
  font-size: 28px;
  color: #333;
}

.favorites-header .count {
  margin: 0;
  color: #666;
  font-size: 16px;
}

.loading-state,
.error-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  gap: 20px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.retry-button {
  padding: 12px 24px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.empty-icon {
  font-size: 64px;
  color: #ddd;
}

.empty-state h3 {
  margin: 0 0 10px 0;
  color: #666;
}

.empty-state p {
  margin: 0 0 20px 0;
  color: #999;
}

.explore-link {
  padding: 12px 24px;
  background: #667eea;
  color: white;
  text-decoration: none;
  border-radius: 6px;
  transition: opacity 0.3s;
}

.explore-link:hover {
  opacity: 0.9;
}

.favorites-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.favorite-spot-card {
  background: white;
  border: 1px solid #eee;
  border-radius: 12px;
  overflow: hidden;
  transition: box-shadow 0.3s;
}

.favorite-spot-card:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.spot-image {
  position: relative;
  height: 160px;
  background: #f5f5f5;
  overflow: hidden;
}

.spot-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-image {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  font-size: 48px;
  color: #ccc;
  font-weight: bold;
}

.remove-btn {
  position: absolute;
  top: 10px;
  right: 10px;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.9);
  border: none;
  cursor: pointer;
  font-size: 20px;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.remove-btn:hover {
  background: #e74c3c;
  color: white;
}

.spot-info {
  padding: 16px;
}

.spot-info h3 {
  margin: 0 0 8px 0;
  font-size: 16px;
  color: #333;
}

.spot-category {
  margin: 0 0 4px 0;
  font-size: 14px;
  color: #667eea;
}

.spot-date {
  margin: 0;
  font-size: 12px;
  color: #999;
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 20px;
  margin-top: 30px;
}

.page-btn {
  padding: 10px 20px;
  background: white;
  border: 1px solid #ddd;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s;
}

.page-btn:hover:not(:disabled) {
  border-color: #667eea;
  color: #667eea;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  font-size: 14px;
  color: #666;
}
</style>
