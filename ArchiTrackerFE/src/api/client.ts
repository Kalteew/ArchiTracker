import { ApiClient } from '@/api'

export const apiClient = new ApiClient({
  BASE: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5099',
})
