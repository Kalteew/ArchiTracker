import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: () => import('@/views/DashboardView.vue'),
    },
    {
      path: '/room/:id',
      name: 'room-details',
      component: () => import('@/views/RoomDetailsView.vue'),
    },
  ],
})

export default router
