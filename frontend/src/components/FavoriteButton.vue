<template>
  <div class="favorite-button" :class="{ 'favorited': isFavorited, 'loading': loading }">
    <button
      v-if="showButton"
      @click="toggleFavorite"
      :disabled="loading"
      :title="isFavorited ? '取消收藏' : '收藏'"
      class="fav-btn"
    >
      <svg class="heart-icon" viewBox="0 0 24 24" fill="currentColor">
        <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.46 2 8.5c0-1.76.41-3.23.87-4.39-2.6-.63.69-1.05-1.48-1.48-2.4-.44-.83-.75-1.35-.87-1.35-.87-.86 0-1.65.58-2.48 1.37-.12.26-.26.51-.51-.77-.71-.98-.7-1.46-1.06-.71-1.06-.65 0-1.2.52-1.2 1.47 0 1.06.42 2.48 1.26 2.48 1.12 0 2.12-.9 2.12-2.12 0-.66-.24-1.28-.53-1.78-.74-.5-.83-1.06-1.06-.78-.78-.78-1.83 0-.65.47-1.22-1.22-1.22 0-.63.35-1.15.78-1.52 1.05-.42 1.87-1.5 3.12-1.5.85 0 1.63-.56 2.37-1.5 2.37-2.37 0-.63.24-1.2.42-1.78.74-.5.83-.75-1.35-.87-1.35-.87-.86 0-1.65.58-2.48 1.37-.12.26-.26.51-.51-.77-.71-.98-.7-1.46-1.06-.71-1.06-.65 0-1.2.52-1.2 1.47 0 1.06.42 2.48 1.26 2.48 1.12 0 2.12-.9 2.12-2.12 0-.66-.24-1.28-.53-1.78-.74-.5-.83-.75-1.35-.87-1.35-.87-.86 0-1.65.58-2.48 1.37-.12.26-.26.51-.51-.77-.71-.98-.7-1.46-1.06-.71-1.06-.65 0-1.2.52-1.2 1.47 0 1.06.42 2.48 1.26 2.48 1.12 0 2.12-.9 2.12-2.12 0-.66-.24-1.28-.53-1.78-.74-.5-.83-.75-1.35-.87-1.35-.87zm1.95 1.2c1.41 1.41 3.7 1.41 5.12 0 1.42-1.41 1.42-3.71 0-5.12-1.42-1.42-3.7 0-5.12-1.42z" />
      </svg>
      <span class="btn-text">{{ isFavorited ? '已收藏' : '收藏' }}</span>
    </button>
    <span v-else-if="showLoginHint" class="login-hint">
      <a href="/login" class="login-link">{{ $t('favorites.loginToFavorite') || '登入後收藏' }}</a>
    </span>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useFavoritesStore } from '@/stores/favorites'

interface Props {
  spotId: string
}

const props = defineProps<Props>()

const route = useRoute()
const authStore = useAuthStore()
const favoritesStore = useFavoritesStore()

const loading = ref(false)
const isFavorited = ref(false)

const showButton = computed(() => authStore.isAuthenticated)
const showLoginHint = computed(() => !authStore.isAuthenticated)

onMounted(async () => {
  if (authStore.isAuthenticated && props.spotId) {
    await checkFavoriteStatus()
  }
})

async function checkFavoriteStatus() {
  try {
    isFavorited.value = await favoritesStore.checkFavorited(props.spotId)
  } catch (err) {
    console.error('Failed to check favorite status:', err)
  }
}

async function toggleFavorite() {
  loading.value = true

  try {
    if (isFavorited.value) {
      await favoritesStore.removeFavorite(props.spotId)
      isFavorited.value = false
    } else {
      await favoritesStore.addFavorite(props.spotId)
      isFavorited.value = true
    }
  } catch (err) {
    console.error('Failed to toggle favorite:', err)
    // Error is handled in store with rollback
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.favorite-button {
  display: inline-block;
}

.fav-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: 2px solid #ddd;
  border-radius: 20px;
  background: white;
  cursor: pointer;
  transition: all 0.3s;
  font-size: 14px;
}

.fav-btn:hover:not(:disabled) {
  border-color: #667eea;
  color: #667eea;
}

.fav-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.favorite-button.favorited .fav-btn {
  border-color: #e74c3c;
  color: #e74c3c;
  background: #fff5f5;
}

.favoriteButton.favorited .fav-btn:hover:not(:disabled) {
  border-color: #c0392b;
  background: #fce8e6;
}

.favorite-button.loading .fav-btn {
  opacity: 0.7;
}

.heart-icon {
  width: 18px;
  height: 18px;
  transition: transform 0.2s;
}

.favorite-button.favorited .heart-icon {
  fill: #e74c3c;
  transform: scale(1.1);
}

.btn-text {
  font-weight: 500;
}

.login-hint {
  display: inline-block;
  font-size: 14px;
}

.login-link {
  color: #667eea;
  text-decoration: none;
  padding: 8px 16px;
  border: 1px solid #667eea;
  border-radius: 20px;
  transition: all 0.3s;
}

.login-link:hover {
  background: #667eea;
  color: white;
}
</style>
