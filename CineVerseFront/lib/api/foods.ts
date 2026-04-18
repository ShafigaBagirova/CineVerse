import { apiRequest } from "@/lib/api/http"

/**
 * Backend DTOs (Application.FoodOrders.Dtos) — ASP.NET Core JSON uses camelCase by default.
 *
 * POST /api/foodorder/draft  → CreateFoodOrderDraftRequest
 *   seatHoldId: int
 *   items: FoodOrderItemRequest[]  → { foodItemId: int, quantity: int }
 *   deliveryType: DeliveryType enum (JSON string: "SeatDelivery" | "CounterPickup")
 *   note?: string (optional — omitted here to match minimal contract)
 *
 * PUT /api/foodorder/draft/{id} → UpdateFoodOrderDraftRequest
 *   items: FoodOrderItemRequest[]
 *   deliveryType: DeliveryType
 *   note?: string (optional — omitted)
 */

export type DeliveryType = "SeatDelivery" | "CounterPickup"
export type FoodOrderStatus = "Pending" | "Confirmed" | "Preparing" | "Ready" | "Delivered" | "Cancelled" | "Refunded"

export interface FoodItemResponse {
  id: number
  name: string
  description?: string | null
  price: number
  image?: string | null
  isAvailable: boolean
  foodCategoryId: number
  foodCategoryName: string
  cinemaId: number
}

export interface GetAllFoodItemsRequest {
  cinemaId?: number
  foodCategoryId?: number
  isAvailable?: boolean
  search?: string
  minPrice?: number
  maxPrice?: number
  pageNumber?: number
  pageSize?: number
}

export interface FoodOrderItemRequest {
  foodItemId: number
  quantity: number
}

export interface FoodOrderItemResponse {
  foodItemId: number
  foodItemName: string
  quantity: number
  unitPrice: number
  totalPrice: number
}

export interface FoodOrderResponse {
  id: number
  /** ASP.NET Identity user id (GUID string); matches backend FoodOrder.UserId. */
  userId: string
  ticketId?: number | null
  totalAmount: number
  status: FoodOrderStatus
  createdAt: string
  items: FoodOrderItemResponse[]
}

/** Exact JSON shape for POST /api/foodorder/draft (matches CreateFoodOrderDraftRequest, no note). */
export interface CreateFoodOrderDraftPayload {
  seatHoldId: number
  items: FoodOrderItemRequest[]
  deliveryType: DeliveryType
}

/** Exact JSON shape for PUT /api/foodorder/draft/{id} (matches UpdateFoodOrderDraftRequest, no note). */
export interface UpdateFoodOrderDraftPayload {
  items: FoodOrderItemRequest[]
  deliveryType: DeliveryType
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

/** Backend GetAllFoodItems validator: pageSize must be 1–50. */
const FOOD_ITEMS_PAGE_SIZE_MAX = 50
const FOOD_ITEMS_PAGE_SIZE_DEFAULT = 50

function clampFoodItemsPagination(pageNumber: number | undefined, pageSize: number | undefined) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber ?? 1)) || 1)
  const raw = pageSize ?? FOOD_ITEMS_PAGE_SIZE_DEFAULT
  const ps = Math.min(FOOD_ITEMS_PAGE_SIZE_MAX, Math.max(1, Math.trunc(Number(raw)) || FOOD_ITEMS_PAGE_SIZE_DEFAULT))
  return { pageNumber: pn, pageSize: ps }
}

/**
 * deliveryType MUST be exactly "SeatDelivery" or "CounterPickup" (JsonStringEnumConverter on API).
 */
export function toDeliveryTypeEnum(value: DeliveryType | string): DeliveryType {
  return value === "CounterPickup" ? "CounterPickup" : "SeatDelivery"
}

/**
 * Line items exactly as FoodOrderItemRequest: foodItemId + quantity (integers > 0).
 */
export function normalizeFoodOrderLineItems(
  items: Array<{ foodItemId: number; quantity: number }>
): FoodOrderItemRequest[] {
  return items
    .map((i) => ({
      foodItemId: Math.trunc(Number(i.foodItemId)),
      quantity: Math.trunc(Number(i.quantity)),
    }))
    .filter((i) => Number.isFinite(i.foodItemId) && i.foodItemId > 0 && Number.isFinite(i.quantity) && i.quantity > 0)
}

/** POST body — field order: seatHoldId, items, deliveryType (matches C# declaration order). */
export function buildCreateFoodOrderDraftPayload(
  seatHoldId: number,
  deliveryType: DeliveryType | string,
  items: Array<{ foodItemId: number; quantity: number }>
): CreateFoodOrderDraftPayload {
  const lineItems = normalizeFoodOrderLineItems(items)
  const dt = toDeliveryTypeEnum(deliveryType as DeliveryType)
  return {
    seatHoldId: Number(seatHoldId),
    items: lineItems,
    deliveryType: dt,
  }
}

/** PUT body — items, deliveryType */
export function buildUpdateFoodOrderDraftPayload(
  deliveryType: DeliveryType | string,
  items: Array<{ foodItemId: number; quantity: number }>
): UpdateFoodOrderDraftPayload {
  const lineItems = normalizeFoodOrderLineItems(items)
  const dt = toDeliveryTypeEnum(deliveryType as DeliveryType)
  return {
    items: lineItems,
    deliveryType: dt,
  }
}

/**
 * Draft create/update uses the same transport as the rest of the app (`apiRequest`):
 * same URL rules (`buildUrl` + Next rewrites), JSON handling, and 401 → refresh → retry.
 */
async function sendFoodOrderDraftRequest(
  path: string,
  method: "POST" | "PUT",
  payload: CreateFoodOrderDraftPayload | UpdateFoodOrderDraftPayload
): Promise<unknown> {
  return apiRequest<unknown>(path, {
    method,
    body: payload,
    auth: true,
    retryOn401: true,
  })
}

export async function getAllFoodItems(query: GetAllFoodItemsRequest = {}) {
  const { pageNumber, pageSize } = clampFoodItemsPagination(query.pageNumber, query.pageSize)
  const qs = buildQuery({
    cinemaId: query.cinemaId,
    foodCategoryId: query.foodCategoryId,
    isAvailable: query.isAvailable,
    search: query.search,
    minPrice: query.minPrice,
    maxPrice: query.maxPrice,
    pageNumber,
    pageSize,
  })
  return apiRequest<FoodItemResponse[]>(`/api/fooditem${qs}`, {
    method: "GET",
    auth: false,
    quiet: true,
  })
}

export async function getMyFoodOrders(seatHoldId?: number) {
  const qs = buildQuery({
    ...(typeof seatHoldId === "number" && Number.isFinite(seatHoldId) && seatHoldId > 0
      ? { seatHoldId }
      : {}),
    pageNumber: 1,
    pageSize: 20,
  })
  return apiRequest<FoodOrderResponse[]>(`/api/foodorder/my-orders${qs}`, {
    method: "GET",
    auth: true,
  })
}

export async function createFoodOrderDraft(
  seatHoldId: number,
  deliveryType: DeliveryType,
  items: Array<{ foodItemId: number; quantity: number }>
) {
  if (!seatHoldId || seatHoldId <= 0) {
    throw new Error("seatHoldId is required and must be greater than 0.")
  }
  const payload = buildCreateFoodOrderDraftPayload(seatHoldId, deliveryType, items)
  if (!payload.items.length) {
    throw new Error("At least one item is required.")
  }
  return sendFoodOrderDraftRequest("/api/foodorder/draft", "POST", payload)
}

export async function updateFoodOrderDraft(
  id: number,
  deliveryType: DeliveryType,
  items: Array<{ foodItemId: number; quantity: number }>
) {
  if (!id || id <= 0) {
    throw new Error("Food order id is required.")
  }
  const payload = buildUpdateFoodOrderDraftPayload(deliveryType, items)
  if (!payload.items.length) {
    throw new Error("At least one item is required.")
  }
  return sendFoodOrderDraftRequest(`/api/foodorder/draft/${id}`, "PUT", payload)
}

export async function cancelFoodOrder(id: number) {
  return apiRequest<unknown>(`/api/foodorder/${id}`, {
    method: "DELETE",
    auth: true,
  })
}
