<template>
  <div class="filter-panel">
    <div class="panel-header">
      <h3>篩選條件</h3>
      <button
        @click="resetFilters"
        :disabled="!hasActiveFilters"
        class="btn-reset"
        title="清除所有篩選"
      >
        🔄 重置
      </button>
    </div>

    <!-- Category Filter -->
    <div class="filter-section">
      <h4>景點分類</h4>
      <div class="checkbox-group">
        <label v-for="category in categoryOptions" :key="category.value" class="checkbox-item">
          <input
            v-model="localFilters.categories"
            type="checkbox"
            :value="category.value"
            @change="updateFilters"
          />
          <span class="category-icon">{{ category.icon }}</span>
          <span>{{ category.label }}</span>
        </label>
      </div>
    </div>

    <!-- Age Group Filter -->
    <div class="filter-section">
      <h4>適合年齡</h4>
      <div class="checkbox-group">
        <label v-for="ageGroup in ageGroupOptions" :key="ageGroup.value" class="checkbox-item">
          <input
            v-model="localFilters.ageGroups"
            type="checkbox"
            :value="ageGroup.value"
            @change="updateFilters"
          />
          <span>{{ ageGroup.label }}</span>
        </label>
      </div>
    </div>

    <!-- Spot Type Filter -->
    <div class="filter-section">
      <h4>景點類型</h4>
      <div class="radio-group">
        <label class="radio-item">
          <input
            v-model="localFilters.spotType"
            type="radio"
            value="all"
            @change="updateFilters"
          />
          <span>全部</span>
        </label>
        <label class="radio-item">
          <input
            v-model="localFilters.spotType"
            type="radio"
            value="permanent"
            @change="updateFilters"
          />
          <span>永久景點</span>
        </label>
        <label class="radio-item">
          <input
            v-model="localFilters.spotType"
            type="radio"
            value="temporary"
            @change="updateFilters"
          />
          <span>期間限定</span>
        </label>
      </div>
    </div>

    <!-- Active Filters Summary -->
    <div v-if="hasActiveFilters" class="active-filters">
      <div class="active-filters-header">
        <h4>已選篩選</h4>
        <span class="filter-count">{{ activeFilterCount }} 項</span>
      </div>
      <div class="active-filters-list">
        <span
          v-for="(filter, index) in activeFiltersList"
          :key="index"
          class="active-filter-tag"
        >
          {{ filter }}
          <button @click="removeFilter(filter)" class="btn-remove-filter">×</button>
        </span>
      </div>
    </div>

    <!-- Results Info -->
    <div class="results-info">
      <span>顯示 {{ resultCount }} 個景點</span>
      <button
        @click="applyFilters"
        :disabled="loading"
        class="btn-apply"
      >
        <span v-if="!loading">套用篩選</span>
        <span v-else>更新中...</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import type { SpotCategory, AgeGroup } from '@/types'

interface Filters {
  categories: SpotCategory[]
  ageGroups: AgeGroup[]
  spotType: 'all' | 'permanent' | 'temporary'
}

interface Props {
  loading?: boolean
  resultCount?: number
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  resultCount: 0,
})

const emit = defineEmits<{
  filtersChange: [filters: Filters]
  applyFilters: []
}>()

const localFilters = reactive<Filters>({
  categories: [],
  ageGroups: [],
  spotType: 'all',
})

const categoryOptions = [
  { value: 'park' as SpotCategory, icon: '🌳', label: '公園' },
  { value: 'restaurant' as SpotCategory, icon: '🍽️', label: '親子餐廳' },
  { value: 'nursing-room' as SpotCategory, icon: '👶', label: '哺乳室/廁所' },
  { value: 'medical' as SpotCategory, icon: '🏥', label: '醫療機構' },
  { value: 'activity' as SpotCategory, icon: '🎪', label: '活動' },
  { value: 'other' as SpotCategory, icon: '📍', label: '其他' },
]

const ageGroupOptions = [
  { value: '0-3' as AgeGroup, label: '0-3 歲' },
  { value: '3-7' as AgeGroup, label: '3-7 歲' },
  { value: '7-12' as AgeGroup, label: '7-12 歲' },
  { value: '12+' as AgeGroup, label: '12 歲以上' },
]

const hasActiveFilters = computed(() => {
  return localFilters.categories.length > 0 ||
         localFilters.ageGroups.length > 0 ||
         localFilters.spotType !== 'all'
})

const activeFilterCount = computed(() => {
  let count = localFilters.categories.length + localFilters.ageGroups.length
  if (localFilters.spotType !== 'all') count++
  return count
})

const activeFiltersList = computed(() => {
  const filters: string[] = []

  // Add categories
  localFilters.categories.forEach(category => {
    const option = categoryOptions.find(opt => opt.value === category)
    if (option) filters.push(option.label)
  })

  // Add age groups
  localFilters.ageGroups.forEach(ageGroup => {
    const option = ageGroupOptions.find(opt => opt.value === ageGroup)
    if (option) filters.push(option.label)
  })

  // Add spot type
  if (localFilters.spotType !== 'all') {
    const typeLabel = localFilters.spotType === 'permanent' ? '永久' : '期間限定'
    filters.push(typeLabel)
  }

  return filters
})

const updateFilters = () => {
  emit('filtersChange', { ...localFilters })
}

const applyFilters = () => {
  emit('applyFilters')
}

const resetFilters = () => {
  localFilters.categories = []
  localFilters.ageGroups = []
  localFilters.spotType = 'all'
  updateFilters()
  applyFilters()
}

const removeFilter = (filterLabel: string) => {
  // Remove from categories
  const categoryOption = categoryOptions.find(opt => opt.label === filterLabel)
  if (categoryOption) {
    const index = localFilters.categories.indexOf(categoryOption.value)
    if (index > -1) {
      localFilters.categories.splice(index, 1)
    }
  }

  // Remove from age groups
  const ageGroupOption = ageGroupOptions.find(opt => opt.label === filterLabel)
  if (ageGroupOption) {
    const index = localFilters.ageGroups.indexOf(ageGroupOption.value)
    if (index > -1) {
      localFilters.ageGroups.splice(index, 1)
    }
  }

  // Remove spot type
  if (filterLabel === '永久' || filterLabel === '期間限定') {
    localFilters.spotType = 'all'
  }

  updateFilters()
  applyFilters()
}

// Watch for external filter changes
watch(() => props.resultCount, (newCount) => {
  // Result count updated externally
})
</script>

<style scoped>
.filter-panel {
  background: white;
  border-radius: 8px;
  padding: 20px;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #eee;
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  color: #333;
}

.btn-reset {
  background: none;
  border: none;
  color: #667eea;
  cursor: pointer;
  font-size: 14px;
  padding: 4px 8px;
  border-radius: 4px;
  transition: background-color 0.3s;
}

.btn-reset:hover:not(:disabled) {
  background: #f0f7ff;
}

.btn-reset:disabled {
  color: #ccc;
  cursor: not-allowed;
}

.filter-section {
  margin-bottom: 24px;
}

.filter-section h4 {
  margin: 0 0 12px 0;
  font-size: 16px;
  color: #333;
  font-weight: 500;
}

.checkbox-group,
.radio-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.checkbox-item,
.radio-item {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 8px;
  border-radius: 4px;
  transition: background-color 0.3s;
}

.checkbox-item:hover,
.radio-item:hover {
  background: #f9f9f9;
}

.checkbox-item input[type="checkbox"],
.radio-item input[type="radio"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.category-icon {
  font-size: 18px;
  line-height: 1;
}

.checkbox-item span,
.radio-item span {
  font-size: 15px;
  color: #333;
}

.active-filters {
  background: #f9f9f9;
  padding: 16px;
  border-radius: 6px;
  margin-bottom: 16px;
}

.active-filters-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.active-filters-header h4 {
  margin: 0;
  font-size: 14px;
  color: #666;
}

.filter-count {
  font-size: 13px;
  color: #999;
}

.active-filters-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.active-filter-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #667eea;
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 13px;
}

.btn-remove-filter {
  background: rgba(255, 255, 255, 0.2);
  border: none;
  color: white;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 16px;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-remove-filter:hover {
  background: rgba(255, 255, 255, 0.3);
}

.results-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #eee;
  font-size: 14px;
  color: #666;
}

.btn-apply {
  background: #667eea;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: opacity 0.3s;
}

.btn-apply:hover:not(:disabled) {
  opacity: 0.9;
}

.btn-apply:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>