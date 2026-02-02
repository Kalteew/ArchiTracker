<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ApiError, type ArchipelagoRoomVisualizationResponse } from '@/api'
import { apiClient } from '@/api/client'

const route = useRoute()
const router = useRouter()
const { t, locale } = useI18n()

const roomData = ref<ArchipelagoRoomVisualizationResponse | null>(null)
const loading = ref(true)
const errorMessage = ref('')
const roomCode = computed(() => route.params.id as string)

const fetchRoom = async () => {
  loading.value = true
  errorMessage.value = ''
  try {
    roomData.value = await apiClient.archipelagoRoom.getApiArchipelagoRoom(roomCode.value)
  } catch (error: unknown) {
    if (error instanceof ApiError && typeof error.body?.error === 'string') {
      errorMessage.value = error.body.error
    } else if (error instanceof ApiError && error.status === 404) {
      errorMessage.value = t('roomView.errors.notFound')
    } else {
      errorMessage.value = t('roomView.errors.generic')
    }
  } finally {
    loading.value = false
  }
}

watch(
  () => roomCode.value,
  () => {
    fetchRoom()
  },
  { immediate: true },
)

const formatDate = (value?: string | null) => {
  if (!value) {
    return '—'
  }
  try {
    return new Intl.DateTimeFormat(locale.value, {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(value))
  } catch {
    return value
  }
}

const openTracker = () => {
  if (roomData.value?.trackerUrl) {
    window.open(roomData.value.trackerUrl, '_blank', 'noopener')
  }
}

const goBack = () => {
  router.push({ name: 'dashboard' })
}
</script>

<template>
  <v-container class="room-view py-8" fluid>
    <div class="d-flex align-center mb-4">
      <v-btn variant="text" prepend-icon="mdi-arrow-left" @click="goBack">
        {{ t('roomView.actions.back') }}
      </v-btn>
    </div>

    <v-card class="mb-6">
      <v-card-title class="d-flex align-center justify-space-between">
        <div>
          <div class="text-h5 font-weight-bold">{{ t('roomView.title', { code: roomCode }) }}</div>
          <p class="text-body-2 text-medium-emphasis mb-0">
            {{ t('roomView.subtitle') }}
          </p>
        </div>
        <v-btn color="primary" prepend-icon="mdi-open-in-new" :disabled="!roomData?.trackerUrl" @click="openTracker">
          {{ t('roomView.actions.openTracker') }}
        </v-btn>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text>
        <v-progress-linear v-if="loading" indeterminate color="primary"></v-progress-linear>
        <v-alert v-else-if="errorMessage" type="error" border="start" density="comfortable">
          {{ errorMessage }}
        </v-alert>
        <div v-else>
          <div class="status-grid mb-6">
            <div>
              <p class="text-caption text-medium-emphasis mb-1">
                {{ t('roomView.status.lastActivity') }}
              </p>
              <p class="text-subtitle-1">{{ formatDate(roomData?.roomStatus?.lastActivity) }}</p>
            </div>
            <div>
              <p class="text-caption text-medium-emphasis mb-1">{{ t('roomView.status.port') }}</p>
              <p class="text-subtitle-1">{{ roomData?.roomStatus?.lastPort ?? '—' }}</p>
            </div>
            <div>
              <p class="text-caption text-medium-emphasis mb-1">{{ t('roomView.status.timeout') }}</p>
              <p class="text-subtitle-1">{{ roomData?.roomStatus?.timeoutSeconds ?? '—' }}</p>
            </div>
          </div>

          <v-card flat class="mb-6" variant="tonal">
            <v-card-title class="text-subtitle-1 font-weight-bold">
              {{ t('roomView.checks.title') }}
            </v-card-title>
            <v-card-text>
              <v-table density="comfortable">
                <thead>
                  <tr>
                    <th>{{ t('roomView.checks.slot') }}</th>
                    <th>{{ t('roomView.checks.player') }}</th>
                    <th>{{ t('roomView.checks.state') }}</th>
                    <th>{{ t('roomView.checks.checks') }}</th>
                    <th>{{ t('roomView.checks.activity') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="player in roomData?.players" :key="player.slot + player.player">
                    <td>{{ player.slot }}</td>
                    <td>{{ player.player }}</td>
                    <td>{{ player.state }}</td>
                    <td>{{ player.checks }}</td>
                    <td>{{ player.lastActivity }}</td>
                  </tr>
                  <tr v-if="!roomData?.players?.length">
                    <td colspan="5" class="text-center text-medium-emphasis">
                      {{ t('roomView.checks.empty') }}
                    </td>
                  </tr>
                </tbody>
              </v-table>
            </v-card-text>
          </v-card>

          <v-card flat variant="tonal">
            <v-card-title class="text-subtitle-1 font-weight-bold">
              {{ t('roomView.hints.title') }}
            </v-card-title>
            <v-card-text>
              <v-table density="comfortable">
                <thead>
                  <tr>
                    <th>{{ t('roomView.hints.sender') }}</th>
                    <th>{{ t('roomView.hints.receiver') }}</th>
                    <th>{{ t('roomView.hints.item') }}</th>
                    <th>{{ t('roomView.hints.location') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="hint in roomData?.hints" :key="hint.sender + hint.receiver + hint.item">
                    <td>{{ hint.sender }}</td>
                    <td>{{ hint.receiver }}</td>
                    <td>{{ hint.item }}</td>
                    <td>{{ hint.location }}</td>
                  </tr>
                  <tr v-if="!roomData?.hints?.length">
                    <td colspan="4" class="text-center text-medium-emphasis">
                      {{ t('roomView.hints.empty') }}
                    </td>
                  </tr>
                </tbody>
              </v-table>
            </v-card-text>
          </v-card>
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<style scoped>
.room-view {
  min-height: calc(100vh - 120px);
  background: radial-gradient(circle at top, rgba(255, 255, 255, 0.9), rgba(244, 246, 251, 0.8));
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
}
</style>
