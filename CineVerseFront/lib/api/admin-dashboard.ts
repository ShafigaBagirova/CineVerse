import { apiRequest } from "@/lib/api/http"

/** GET /api/AdminDashboard/summary */
export interface AdminDashboardSummaryDto {
  totalUsers: number
  vipUsers: number
  totalMovies: number
  totalCinemas: number
  activeScreenings: number
  totalTicketsSold: number
  totalRevenue: number
  successfulPayments: number
  failedPayments: number
  pendingPayments: number
  totalReviews: number
  totalRatings: number
  totalWatchlistItems: number
}

export interface RevenueChartItemDto {
  date: string
  revenue: number
  ticketsSold: number
}

export interface TopMovieDto {
  movieId: number
  title: string
  averageRating: number
  reviewCount: number
  watchlistCount: number
  watchCount: number
  ticketCount: number
  revenue: number
}

export interface RecentPaymentDto {
  paymentId: number
  userId: string
  amount: number
  status: string
  createdAt?: string | null
}

export interface ScreeningOccupancyDto {
  screeningId: number
  movieTitle: string
  hallName: string
  startTime: string
  occupancyPercent: number
}

export async function getAdminDashboardSummary() {
  return apiRequest<AdminDashboardSummaryDto>("/api/AdminDashboard/summary", {
    method: "GET",
    auth: true,
    quiet: true,
  })
}

export async function getAdminRevenueChart(days = 7) {
  return apiRequest<RevenueChartItemDto[]>(`/api/AdminDashboard/revenue-chart?days=${days}`, {
    method: "GET",
    auth: true,
    quiet: true,
  })
}

export async function getAdminTopMovies(take = 5) {
  return apiRequest<TopMovieDto[]>(`/api/AdminDashboard/top-movies?take=${take}`, {
    method: "GET",
    auth: true,
    quiet: true,
  })
}

export async function getAdminRecentPayments(take = 10) {
  return apiRequest<RecentPaymentDto[]>(`/api/AdminDashboard/recent-payments?take=${take}`, {
    method: "GET",
    auth: true,
    quiet: true,
  })
}

export async function getAdminScreeningOccupancy(take = 10) {
  return apiRequest<ScreeningOccupancyDto[]>(`/api/AdminDashboard/screening-occupancy?take=${take}`, {
    method: "GET",
    auth: true,
    quiet: true,
  })
}
