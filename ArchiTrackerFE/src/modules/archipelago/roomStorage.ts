import { ref } from 'vue'

const STORAGE_KEY = 'archiTracker:savedRooms'

export interface SavedRoom {
  code: string
  url: string
  savedAt: string
}

const rooms = ref<SavedRoom[]>(loadInitial())

function loadInitial(): SavedRoom[] {
  if (typeof window === 'undefined') {
    return []
  }

  try {
    const raw = window.localStorage.getItem(STORAGE_KEY)
    if (!raw) {
      return []
    }

    const parsed = JSON.parse(raw)
    if (Array.isArray(parsed)) {
      return parsed.filter((room) => typeof room?.code === 'string' && typeof room?.url === 'string')
    }
  } catch (error) {
    console.warn('Failed to parse saved rooms', error)
  }

  return []
}

function persist() {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(rooms.value))
}

function addRoom(room: SavedRoom) {
  const existingIndex = rooms.value.findIndex((entry) => entry.code === room.code)
  if (existingIndex >= 0) {
    rooms.value[existingIndex] = room
    rooms.value = [...rooms.value]
  } else {
    rooms.value = [room, ...rooms.value]
  }
  persist()
}

function removeRoom(code: string) {
  rooms.value = rooms.value.filter((entry) => entry.code !== code)
  persist()
}

export function useRoomStorage() {
  return {
    rooms,
    addRoom,
    removeRoom,
  }
}
