<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ApiError } from '@/api'
import { apiClient } from '@/api/client'
import { useRoomStorage } from '@/modules/archipelago/roomStorage'

const router = useRouter()
const { t, locale } = useI18n()
const { rooms, addRoom, removeRoom } = useRoomStorage()

const roomUrl = ref('')
const fieldError = ref<string | null>(null)
const statusMessage = ref('')
const statusColor = ref<'success' | 'error'>('success')
const submitting = ref(false)
const archipelagoPattern = /^https:\/\/archipelago\.gg\/room\/[A-Za-z0-9_-]+$/i

const isValidUrl = computed(() => archipelagoPattern.test(roomUrl.value.trim()))
const hasSavedRooms = computed(() => rooms.value.length > 0)

const resetStatus = () => {
  if (statusMessage.value) {
    statusMessage.value = ''
  }
}

const validateInput = () => {
  const trimmed = roomUrl.value.trim()
  if (!archipelagoPattern.test(trimmed)) {
    fieldError.value = t('archipelago.error.invalid')
    return false
  }
  fieldError.value = null
  return true
}

const handleInput = () => {
  resetStatus()
  if (fieldError.value && isValidUrl.value) {
    fieldError.value = null
  }
}

const submit = async () => {
  statusMessage.value = ''
  statusColor.value = 'success'
  if (!validateInput()) {
    return
  }

  submitting.value = true
  try {
    const response = await apiClient.archipelagoRoom.postApiArchipelagoRoom({
      url: roomUrl.value.trim(),
    })
    statusMessage.value = t('archipelago.success')
    statusColor.value = 'success'
    if (response.link && response.url) {
      addRoom({
        code: response.link,
        url: response.url,
        savedAt: new Date().toISOString(),
      })
    }
  } catch (error: unknown) {
    statusColor.value = 'error'
    if (error instanceof ApiError && typeof error.body?.error === 'string') {
      statusMessage.value = error.body.error
    } else {
      statusMessage.value = t('archipelago.error.server')
    }
  } finally {
    submitting.value = false
  }
}

const goToRoom = (code: string) => {
  router.push({ name: 'room-details', params: { id: code } })
}

const formatSavedAt = (value: string) => {
  try {
    return new Intl.DateTimeFormat(locale.value, {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(value))
  } catch {
    return value
  }
}

const removeSavedRoom = (code: string) => {
  removeRoom(code)
}
</script>

<template>
  <v-container class="dashboard-blank py-12" fluid>
    <div class="hero text-center mx-auto mb-8">
      <p class="text-overline text-medium-emphasis mb-1">{{ t('dashboard.title') }}</p>
      <div class="text-h2 font-weight-bold mb-3">{{ t('dashboard.welcome') }}</div>
      <p class="text-body-1 text-medium-emphasis">{{ t('dashboard.subtitle') }}</p>
    </div>

    <v-card class="archipelago-card mx-auto px-4 py-4" max-width="560">
      <v-card-title class="text-h6 text-left">{{ t('archipelago.label') }}</v-card-title>
      <v-card-text>
        <v-form @submit.prevent="submit" class="form-stack">
          <v-text-field
            v-model="roomUrl"
            :label="t('archipelago.label')"
            :hint="t('archipelago.helper')"
            persistent-hint
            variant="outlined"
            color="primary"
            @input="handleInput"
            :error-messages="fieldError ? [fieldError] : []"
          ></v-text-field>
          <v-btn type="submit" color="primary" :loading="submitting" :disabled="submitting">
            {{ t('archipelago.submit') }}
          </v-btn>
        </v-form>

        <v-alert
          v-if="statusMessage"
          :type="statusColor"
          density="comfortable"
          class="mt-4"
          border="start"
        >
          {{ statusMessage }}
        </v-alert>
      </v-card-text>
    </v-card>

    <v-card class="saved-rooms-card mx-auto px-4 py-4 mt-8" max-width="720">
      <v-card-title class="text-h6 text-left">
        {{ t('dashboard.savedRooms.title') }}
      </v-card-title>
      <v-card-text>
        <div v-if="!hasSavedRooms" class="text-medium-emphasis">
          {{ t('dashboard.savedRooms.empty') }}
        </div>
        <v-list v-else lines="two">
          <v-list-item v-for="saved in rooms" :key="saved.code" class="px-0">
            <template #prepend>
              <v-avatar color="primary" variant="tonal" class="me-4">
                <span class="text-subtitle-1">{{ saved.code.slice(0, 2).toUpperCase() }}</span>
              </v-avatar>
            </template>
            <v-list-item-title class="font-weight-medium">{{ saved.code }}</v-list-item-title>
            <v-list-item-subtitle>
              <div>{{ saved.url }}</div>
              <div class="text-caption">
                {{ t('dashboard.savedRooms.savedAt', { date: formatSavedAt(saved.savedAt) }) }}
              </div>
            </v-list-item-subtitle>
            <template #append>
              <v-btn variant="tonal" color="secondary" class="me-2" @click="goToRoom(saved.code)">
                {{ t('dashboard.savedRooms.view') }}
              </v-btn>
              <v-btn icon variant="text" @click="removeSavedRoom(saved.code)">
                <v-icon icon="mdi-close"></v-icon>
              </v-btn>
            </template>
          </v-list-item>
        </v-list>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<style scoped>
.dashboard-blank {
  min-height: calc(100vh - 120px);
  background: radial-gradient(circle at top, rgba(255, 255, 255, 0.9), rgba(244, 246, 251, 0.8));
  display: flex;
  flex-direction: column;
  align-items: center;
}

.hero {
  max-width: 640px;
}

.archipelago-card {
  width: 100%;
  border-radius: 24px;
  box-shadow: 0 20px 60px rgba(15, 23, 42, 0.08);
  background-color: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.form-stack {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.saved-rooms-card {
  width: 100%;
  border-radius: 24px;
  box-shadow: 0 20px 60px rgba(15, 23, 42, 0.05);
  background-color: rgba(255, 255, 255, 0.95);
}
</style>
