<template>
  <div class="spot-form">
    <div class="form-header">
      <h2>回報新景點</h2>
      <p>分享您發現的親子友善景點</p>
    </div>

    <form @submit.prevent="handleSubmit" class="form-content">
      <!-- Basic Information -->
      <div class="form-section">
        <h3>基本資訊</h3>

        <div class="form-group">
          <label for="name">景點名稱 *</label>
          <input
            id="name"
            v-model="formData.name"
            type="text"
            required
            maxlength="200"
            placeholder="例如：台中兒童藝術館"
          />
          <span class="char-count">{{ formData.name.length }}/200</span>
        </div>

        <div class="form-group">
          <label for="category">分類 *</label>
          <select id="category" v-model="formData.category" required>
            <option value="">請選擇分類</option>
            <option value="park">公園</option>
            <option value="restaurant">親子餐廳</option>
            <option value="nursing-room">哺乳室/廁所</option>
            <option value="medical">醫療機構</option>
            <option value="activity">活動</option>
            <option value="other">其他</option>
          </select>
        </div>

        <div class="form-group">
          <label for="spotType">景點類型 *</label>
          <select id="spotType" v-model="formData.spotType" required>
            <option value="permanent">永久景點</option>
            <option value="temporary">期間限定</option>
          </select>
        </div>

        <div class="form-group">
          <label for="address">地址 *</label>
          <input
            id="address"
            v-model="formData.address"
            type="text"
            required
            placeholder="例如：台中市西區英才路600號"
          />
        </div>

        <div class="form-group">
          <label for="description">描述</label>
          <textarea
            id="description"
            v-model="formData.description"
            rows="3"
            placeholder="簡單描述這��景點的特色..."
          ></textarea>
        </div>
      </div>

      <!-- Location Information -->
      <div class="form-section">
        <h3>位置資訊</h3>

        <div class="location-picker">
          <p class="location-hint">💡 在地圖上點擊以設定景點位置</p>
          <div v-if="!formData.lat || !formData.lng" class="location-placeholder">
            <span>尚未設定位置</span>
            <small>請在地圖上點擊選擇位置</small>
          </div>
          <div v-else class="location-info">
            <span>📍 緯度: {{ formData.lat.toFixed(6) }}</span>
            <span>📍 經度: {{ formData.lng.toFixed(6) }}</span>
          </div>
        </div>

        <button type="button" @click="openMapPicker" class="btn-map">
          🗺️ 在地圖上選擇位置
        </button>
      </div>

      <!-- Target Age Groups -->
      <div class="form-section">
        <h3>適合年齡層</h3>
        <div class="checkbox-group">
          <label v-for="ageGroup in ageGroupOptions" :key="ageGroup.value" class="checkbox-item">
            <input
              v-model="formData.ageGroups"
              type="checkbox"
              :value="ageGroup.value"
            />
            <span>{{ ageGroup.label }}</span>
          </label>
        </div>
      </div>

      <!-- Facilities -->
      <div class="form-section">
        <h3>設施 (可複選)</h3>
        <div class="checkbox-group">
          <label v-for="facility in facilityOptions" :key="facility.value" class="checkbox-item">
            <input
              v-model="formData.facilities"
              type="checkbox"
              :value="facility.value"
            />
            <span>{{ facility.label }}</span>
          </label>
        </div>
      </div>

      <!-- Temporary Spot Date Range -->
      <div v-if="formData.spotType === 'temporary'" class="form-section">
        <h3>活動期間</h3>
        <div class="date-group">
          <div class="form-group">
            <label for="startDate">開始日期 *</label>
            <input
              id="startDate"
              v-model="formData.startDate"
              type="date"
              required
            />
          </div>
          <div class="form-group">
            <label for="endDate">結束日期 *</label>
            <input
              id="endDate"
              v-model="formData.endDate"
              type="date"
              :min="formData.startDate"
              required
            />
          </div>
        </div>
      </div>

      <!-- Image Upload -->
      <div class="form-section">
        <h3>圖片 (選填)</h3>
        <div class="image-upload">
          <input
            ref="fileInput"
            type="file"
            accept="image/jpeg,image/png,image/webp"
            multiple
            @change="handleImageSelect"
            class="file-input"
          />
          <button type="button" @click="selectImages" class="btn-upload">
            📷 選擇圖片
          </button>
          <span class="upload-hint">支援 JPG, PNG, WEBP 格式，單檔最大 5MB</span>
        </div>

        <div v-if="imagePreviews.length > 0" class="image-previews">
          <div v-for="(image, index) in imagePreviews" :key="index" class="image-preview-item">
            <img :src="image.preview" :alt="`Preview ${index + 1}`" />
            <button type="button" @click="removeImage(index)" class="btn-remove">×</button>
          </div>
        </div>
      </div>

      <!-- Submit Actions -->
      <div class="form-actions">
        <button type="button" @click="handleCancel" class="btn-cancel">
          取消
        </button>
        <button type="submit" :disabled="submitting" class="btn-submit">
          <span v-if="!submitting">提交回報</span>
          <span v-else>提交中...</span>
        </button>
      </div>

      <!-- Error Message -->
      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>

      <!-- Success Message -->
      <div v-if="successMessage" class="success-message">
        {{ successMessage }}
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import type { CreateSpotRequest, SpotCategory, AgeGroup, Facility } from '@/types'
import spotService from '@/services/spotService'

const router = useRouter()

const formData = reactive<CreateSpotRequest & { startDate?: string; endDate?: string }>({
  name: '',
  category: 'other' as SpotCategory,
  spotType: 'permanent',
  address: '',
  description: '',
  lat: 0,
  lng: 0,
  ageGroups: [],
  facilities: [],
  startDate: '',
  endDate: '',
})

const submitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const fileInput = ref<HTMLInputElement>()
const selectedFiles = ref<File[]>([])
const imagePreviews = ref<Array<{ file: File; preview: string }>>([])

const ageGroupOptions = [
  { value: '0-3' as AgeGroup, label: '0-3 歲' },
  { value: '3-7' as AgeGroup, label: '3-7 歲' },
  { value: '7-12' as AgeGroup, label: '7-12 歲' },
  { value: '12+' as AgeGroup, label: '12 歲以上' },
]

const facilityOptions = [
  { value: 'parking' as Facility, label: '停車場' },
  { value: 'stroller-accessible' as Facility, label: '嬰兒車可進入' },
  { value: 'diaper-changing-station' as Facility, label: '尿布台' },
  { value: 'playground' as Facility, label: '遊樂設施' },
  { value: 'rest-area' as Facility, label: '休息區' },
  { value: 'feeding-room' as Facility, label: '哺乳室' },
  { value: 'toilet' as Facility, label: '廁所' },
  { value: 'water-fountain' as Facility, label: '飲水機' },
  { value: 'shaded-area' as Facility, label: '遮陽處' },
  { value: 'wifi' as Facility, label: '免費 WiFi' },
  { value: 'other' as Facility, label: '其他' },
]

const selectImages = () => {
  fileInput.value?.click()
}

const handleImageSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = Array.from(target.files || [])

  // Validate files
  const validFiles = files.filter(file => {
    if (!file.type.match(/image\/(jpeg|png|webp)/)) {
      alert(`檔案 ${file.name} 格式不符，請選擇 JPG, PNG, WEBP 格式`)
      return false
    }
    if (file.size > 5 * 1024 * 1024) {
      alert(`檔案 ${file.name} ��過 5MB 限制`)
      return false
    }
    return true
  })

  // Add previews
  validFiles.forEach(file => {
    const reader = new FileReader()
    reader.onload = (e) => {
      imagePreviews.value.push({
        file,
        preview: e.target?.result as string,
      })
    }
    reader.readAsDataURL(file)
  })

  selectedFiles.value.push(...validFiles)
}

const removeImage = (index: number) => {
  imagePreviews.value.splice(index, 1)
  selectedFiles.value.splice(index, 1)
}

const openMapPicker = () => {
  // Navigate to map with coordinate picking mode
  router.push({
    name: 'home',
    query: { mode: 'add-spot' },
  })
}

const setLocation = (lat: number, lng: number) => {
  formData.lat = lat
  formData.lng = lng
}

const validateForm = (): boolean => {
  if (!formData.name || formData.name.trim().length === 0) {
    errorMessage.value = '請輸入景點名稱'
    return false
  }

  if (!formData.category) {
    errorMessage.value = '請選擇景點分類'
    return false
  }

  if (!formData.address || formData.address.trim().length === 0) {
    errorMessage.value = '請輸入地址'
    return false
  }

  if (formData.lat === 0 || formData.lng === 0) {
    errorMessage.value = '請在地圖上選擇景點位置'
    return false
  }

  if (formData.spotType === 'temporary') {
    if (!formData.startDate || !formData.endDate) {
      errorMessage.value = '期間限定景點需要設定開始和結束日期'
      return false
    }

    const startDate = new Date(formData.startDate)
    const endDate = new Date(formData.endDate)
    if (endDate < startDate) {
      errorMessage.value = '結束日期不能早於開始日期'
      return false
    }
  }

  return true
}

const handleSubmit = async () => {
  errorMessage.value = ''
  successMessage.value = ''

  if (!validateForm()) {
    return
  }

  submitting.value = true

  try {
    // Create spot request
    const spotRequest: CreateSpotRequest = {
      name: formData.name.trim(),
      category: formData.category,
      spotType: formData.spotType,
      address: formData.address.trim(),
      lat: formData.lat,
      lng: formData.lng,
      description: formData.description?.trim(),
      ageGroups: formData.ageGroups,
      facilities: formData.facilities,
      ...(formData.spotType === 'temporary' && {
        startDate: formData.startDate,
        endDate: formData.endDate,
      }),
    }

    // Create spot
    const spot = await spotService.createSpot(spotRequest)

    // Upload images if any
    if (selectedFiles.value.length > 0) {
      const uploadPromises = selectedFiles.value.map(file =>
        spotService.uploadSpotImage(spot.id, file)
      )
      await Promise.all(uploadPromises)
    }

    successMessage.value = '景點回報成功！您的回報將在審核通過後顯示在地圖上。'

    // Reset form
    resetForm()

    // Navigate back after 3 seconds
    setTimeout(() => {
      router.push({ name: 'home' })
    }, 3000)

  } catch (error: any) {
    errorMessage.value = error.message || '提交失敗，請稍後再試'
  } finally {
    submitting.value = false
  }
}

const handleCancel = () => {
  router.back()
}

const resetForm = () => {
  formData.name = ''
  formData.category = 'other' as SpotCategory
  formData.spotType = 'permanent'
  formData.address = ''
  formData.description = ''
  formData.lat = 0
  formData.lng = 0
  formData.ageGroups = []
  formData.facilities = []
  formData.startDate = ''
  formData.endDate = ''

  selectedFiles.value = []
  imagePreviews.value = []
}

// Expose methods for parent components
defineExpose({
  setLocation,
})
</script>

<style scoped>
.spot-form {
  max-width: 600px;
  margin: 0 auto;
  background: white;
  border-radius: 8px;
  padding: 24px;
}

.form-header {
  text-align: center;
  margin-bottom: 32px;
}

.form-header h2 {
  margin: 0 0 8px 0;
  color: #333;
  font-size: 24px;
}

.form-header p {
  margin: 0;
  color: #666;
  font-size: 16px;
}

.form-section {
  margin-bottom: 32px;
  padding-bottom: 24px;
  border-bottom: 1px solid #eee;
}

.form-section:last-child {
  border-bottom: none;
}

.form-section h3 {
  margin: 0 0 16px 0;
  color: #333;
  font-size: 18px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #333;
  font-weight: 500;
  font-size: 14px;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 16px;
  transition: border-color 0.3s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #667eea;
}

.char-count {
  display: block;
  text-align: right;
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

.location-picker {
  margin-bottom: 16px;
}

.location-hint {
  background: #f0f7ff;
  border-left: 4px solid #667eea;
  padding: 12px;
  margin: 0 0 16px 0;
  font-size: 14px;
  color: #333;
}

.location-placeholder {
  text-align: center;
  padding: 24px;
  background: #f9f9f9;
  border: 2px dashed #ddd;
  border-radius: 6px;
  color: #999;
}

.location-placeholder span {
  display: block;
  font-size: 16px;
  margin-bottom: 4px;
}

.location-placeholder small {
  font-size: 13px;
}

.location-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
  background: #f0f7ff;
  border-radius: 6px;
  font-size: 14px;
  color: #333;
}

.btn-map {
  width: 100%;
  padding: 12px;
  background: #f0f7ff;
  color: #667eea;
  border: 2px dashed #667eea;
  border-radius: 6px;
  cursor: pointer;
  font-size: 16px;
  transition: background-color 0.3s;
}

.btn-map:hover {
  background: #e6f0ff;
}

.checkbox-group {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 12px;
}

.checkbox-item {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.checkbox-item input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.checkbox-item span {
  font-size: 15px;
  color: #333;
}

.date-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.image-upload {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 16px;
}

.file-input {
  display: none;
}

.btn-upload {
  padding: 12px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 16px;
  transition: opacity 0.3s;
}

.btn-upload:hover {
  opacity: 0.9;
}

.upload-hint {
  font-size: 13px;
  color: #666;
}

.image-previews {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 12px;
}

.image-preview-item {
  position: relative;
  aspect-ratio: 1;
  overflow: hidden;
  border-radius: 6px;
}

.image-preview-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.btn-remove {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 24px;
  height: 24px;
  background: rgba(0, 0, 0, 0.6);
  color: white;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  font-size: 18px;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-remove:hover {
  background: rgba(0, 0, 0, 0.8);
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 32px;
}

.btn-cancel,
.btn-submit {
  flex: 1;
  padding: 14px;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.3s;
}

.btn-cancel {
  background: #f0f0f0;
  color: #333;
}

.btn-cancel:hover {
  background: #e0e0e0;
}

.btn-submit {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-submit:hover:not(:disabled) {
  opacity: 0.9;
}

.btn-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error-message {
  background: #fee;
  color: #c33;
  padding: 12px;
  border-radius: 6px;
  margin-top: 16px;
  border: 1px solid #fcc;
  font-size: 14px;
}

.success-message {
  background: #e8f5e8;
  color: #2d8b2d;
  padding: 12px;
  border-radius: 6px;
  margin-top: 16px;
  border: 1px solid #c8e6c8;
  font-size: 14px;
}

@media (max-width: 768px) {
  .spot-form {
    padding: 16px;
  }

  .checkbox-group {
    grid-template-columns: 1fr;
  }

  .date-group {
    grid-template-columns: 1fr;
  }
}
</style>