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

export interface CreateFoodItemRequest {
  name: string
  description?: string
  price: number
  image?: File | null
  isAvailable: boolean
  foodCategoryId: number
}

export interface UpdateFoodItemRequest {
  name?: string
  description?: string
  price?: number
  isAvailable?: boolean
  isActive?: boolean
  foodCategoryId?: number
}

export interface FoodCategoryResponse {
  id: number
  name: string
  description?: string | null
  cinemaId: number
  isActive: boolean
}

export interface GetAllFoodCategoriesRequest {
  cinemaId?: number
  isActive?: boolean
  search?: string
  pageNumber?: number
  pageSize?: number
}

export interface CreateFoodCategoryRequest {
  name: string
  description?: string
  isActive?: boolean
  displayOrder: number
  cinemaId: number
}

export interface UpdateFoodCategoryRequest {
  name?: string
  description?: string
  displayOrder?: number
  isActive?: boolean
  cinemaId?: number
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

export interface GetAllFoodOrdersRequest {
  userId?: string
  seatHoldId?: number
  screeningId?: number
  seatId?: number
  cinemaId?: number
  status?: FoodOrderStatus
  deliveryType?: DeliveryType
  createdFrom?: string
  createdTo?: string
  pageNumber?: number
  pageSize?: number
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
const FOOD_ITEMS_PAGE_SIZE_DEFAULT = 10

function clampFoodItemsPagination(pageNumber: number | undefined, pageSize: number | undefined) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber ?? 1)) || 1)
  const raw = pageSize ?? FOOD_ITEMS_PAGE_SIZE_DEFAULT
  const ps = Math.min(FOOD_ITEMS_PAGE_SIZE_MAX, Math.max(1, Math.trunc(Number(raw)) || FOOD_ITEMS_PAGE_SIZE_DEFAULT))
  return { pageNumber: pn, pageSize: ps }
}

function clampFoodCategoriesPagination(pageNumber: number | undefined, pageSize: number | undefined) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber ?? 1)) || 1)
  const raw = pageSize ?? 10
  const ps = Math.min(50, Math.max(1, Math.trunc(Number(raw)) || 10))
  return { pageNumber: pn, pageSize: ps }
}

function clampFoodOrdersPagination(pageNumber: number | undefined, pageSize: number | undefined) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber ?? 1)) || 1)
  const raw = pageSize ?? 10
  const ps = Math.min(50, Math.max(1, Math.trunc(Number(raw)) || 10))
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

export async function createFoodItem(body: CreateFoodItemRequest) {
  const form = new FormData()
  form.set("name", body.name)
  if (body.description != null && body.description.trim() !== "") {
    form.set("description", body.description.trim())
  }
  form.set("price", String(body.price))
  form.set("isAvailable", body.isAvailable ? "true" : "false")
  form.set("foodCategoryId", String(body.foodCategoryId))
  if (body.image) {
    form.set("image", body.image)
  }
  return apiRequest<unknown>("/api/fooditem", {
    method: "POST",
    auth: true,
    body: form,
    headers: {},
  })
}

export async function updateFoodItem(id: number, body: UpdateFoodItemRequest) {
  return apiRequest<unknown>(`/api/fooditem/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteFoodItem(id: number) {
  return apiRequest<unknown>(`/api/fooditem/${id}`, {
    method: "DELETE",
    auth: true,
  })
}

export async function getAllFoodCategories(query: GetAllFoodCategoriesRequest = {}) {
  const { pageNumber, pageSize } = clampFoodCategoriesPagination(query.pageNumber, query.pageSize)
  const qs = buildQuery({
    cinemaId: query.cinemaId,
    isActive: query.isActive,
    search: query.search,
    pageNumber,
    pageSize,
  })
  return apiRequest<FoodCategoryResponse[]>(`/api/foodcategory${qs}`, {
    method: "GET",
    auth: true,
  })
}

export async function createFoodCategory(body: CreateFoodCategoryRequest) {
  return apiRequest<unknown>("/api/foodcategory", {
    method: "POST",
    auth: true,
    body,
  })
}

export async function updateFoodCategory(id: number, body: UpdateFoodCategoryRequest) {
  return apiRequest<unknown>(`/api/foodcategory/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteFoodCategory(id: number) {
  return apiRequest<unknown>(`/api/foodcategory/${id}`, {
    method: "DELETE",
    auth: true,
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

export async function getAllFoodOrders(query: GetAllFoodOrdersRequest = {}) {
  const { pageNumber, pageSize } = clampFoodOrdersPagination(query.pageNumber, query.pageSize)
  const qs = buildQuery({
    userId: query.userId,
    seatHoldId: query.seatHoldId,
    screeningId: query.screeningId,
    seatId: query.seatId,
    cinemaId: query.cinemaId,
    status: query.status,
    deliveryType: query.deliveryType,
    createdFrom: query.createdFrom,
    createdTo: query.createdTo,
    pageNumber,
    pageSize,
  })
  return apiRequest<FoodOrderResponse[]>(`/api/foodorder${qs}`, {
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
