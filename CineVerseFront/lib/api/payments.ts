import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"
import { ApiError } from "@/lib/api/types"

export type PaymentStatus = "Pending" | "Succeeded" | "Failed" | "Cancelled" | "Refunded"
export type PaymentProvider = "Stripe" | string

export interface CreatePaymentIntentRequest {
  seatHoldId: number
}

export interface CreatePaymentIntentResponse {
  paymentId: number
  seatHoldId: number
  ticketAmount: number
  foodAmount: number
  totalAmount: number
  clientSecret: string
  providerPaymentIntentId: string
  status: PaymentStatus
}

export interface RetryPaymentResponse {
  paymentId: number
  seatHoldId: number
  provider: PaymentProvider
  providerPaymentIntentId: string
  clientSecret: string
  totalAmount: number
  ticketAmount: number
  foodAmount: number
  currency: string
  status: PaymentStatus
}

export interface GetPaymentStatusBySeatHoldIdResponse {
  seatHoldId: number
  hasPayment: boolean
  status: PaymentStatus
  amount?: number | null
  currency?: string | null
  paymentIntentId?: string | null
  paidAtUtc?: string | null
}

export interface GetMyPaymentsRequest {
  pageNumber?: number
  pageSize?: number
  status?: PaymentStatus
}

export interface GetMyPaymentsResponse {
  id: number
  seatHoldId: number
  screeningId: number
  seatId: number
  amount: number
  currency: string
  provider: PaymentProvider
  status: PaymentStatus
  providerPaymentIntentId: string
  createdAtUtc: string
  paidAtUtc?: string | null
}

function buildQuery(query: Record<string, string | number | boolean | undefined>) {
  const params = new URLSearchParams()
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === "") return
    params.set(key, String(value))
  })
  const queryString = params.toString()
  return queryString ? `?${queryString}` : ""
}

export async function createPaymentIntent(request: CreatePaymentIntentRequest) {
  const seatHoldId = Number(request.seatHoldId)
  if (!Number.isFinite(seatHoldId) || seatHoldId <= 0) {
    throw new ApiError("seatHoldId must be a positive number", 400)
  }
  return apiRequest<CreatePaymentIntentResponse>("/api/payment/create-intent", {
    method: "POST",
    auth: true,
    body: { seatHoldId },
  })
}

export async function retryPayment(seatHoldId: number) {
  return apiRequest<RetryPaymentResponse>(`/api/payment/retry/${seatHoldId}`, {
    method: "POST",
    auth: true,
  })
}

export async function getPaymentStatusBySeatHoldId(seatHoldId: number) {
  return apiRequest<GetPaymentStatusBySeatHoldIdResponse>(`/api/payment/seat-hold/${seatHoldId}/status`, {
    method: "GET",
    auth: true,
  })
}

export async function getMyPayments(query: GetMyPaymentsRequest = {}) {
  const qs = buildQuery({
    pageNumber: query.pageNumber ?? 1,
    pageSize: query.pageSize ?? 10,
    status: query.status,
  })
  return apiRequest<PaginatedResponse<GetMyPaymentsResponse>>(`/api/payment/my${qs}`, {
    method: "GET",
    auth: true,
  })
}
