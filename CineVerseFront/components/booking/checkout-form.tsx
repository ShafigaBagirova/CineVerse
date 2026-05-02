"use client"

import { useEffect, useState, useRef, useMemo } from "react"
import { CheckoutStripePayment } from "@/components/booking/checkout-stripe-payment"
import { isStripeConfigured } from "@/lib/stripe-client"
import { useSearchParams, useRouter } from "next/navigation"
import Image from "next/image"
import { CreditCard, MapPin, Clock, Armchair, Plus, Minus } from "lucide-react"
import { cn } from "@/lib/utils"
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { useAuth } from "@/components/providers/auth-provider"
import { getMovieById } from "@/lib/api/movies"
import { ApiError } from "@/lib/api/types"
import { createPaymentIntent, getPaymentStatusBySeatHoldId, retryPayment, type PaymentStatus } from "@/lib/api/payments"
import { getSeatHoldById } from "@/lib/api/seat-holds"
import { getMyTickets } from "@/lib/api/tickets"
import {
  createFoodOrderDraft,
  getAllFoodItems,
  getMyFoodOrders,
  updateFoodOrderDraft,
  type DeliveryType,
  type FoodItemResponse,
} from "@/lib/api/foods"

const API_BASE_URL = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "").replace(/\/$/, "")

function resolveSnackImageUrl(snack: FoodItemResponse) {
  const rawImage =
    (snack as unknown as Record<string, unknown>).imageUrl ??
    (snack as unknown as Record<string, unknown>).ImageUrl ??
    (snack as unknown as Record<string, unknown>).photoUrl ??
    (snack as unknown as Record<string, unknown>).PhotoUrl ??
    (snack as unknown as Record<string, unknown>).pictureUrl ??
    (snack as unknown as Record<string, unknown>).PictureUrl ??
    (snack as unknown as Record<string, unknown>).image ??
    (snack as unknown as Record<string, unknown>).Image

  if (typeof rawImage !== "string" || !rawImage.trim()) return "/placeholder-food.png"
  const value = rawImage.trim()
  return value.startsWith("http")
    ? value
    : `${API_BASE_URL}${value.startsWith("/") ? "" : "/"}${value}`
}

/** API returns enum strings in camelCase (`pending`); keep UI logic on canonical `PaymentStatus` labels. */
function normalizePaymentStatus(raw: unknown): PaymentStatus | "none" {
  if (raw === undefined || raw === null || raw === "") return "none"
  if (typeof raw === "number" && Number.isInteger(raw) && raw >= 1 && raw <= 5) {
    const byNum: PaymentStatus[] = ["Pending", "Succeeded", "Failed", "Cancelled", "Refunded"]
    return byNum[raw - 1] ?? "none"
  }
  const s = String(raw).toLowerCase()
  const map: Record<string, PaymentStatus> = {
    pending: "Pending",
    succeeded: "Succeeded",
    confirmed: "Succeeded",
    failed: "Failed",
    cancelled: "Cancelled",
    canceled: "Cancelled",
    refunded: "Refunded",
  }
  return map[s] ?? "none"
}

export function CheckoutForm() {
  const { status } = useAuth()
  const searchParams = useSearchParams()
  const router = useRouter()
  const movieId = Number(searchParams.get("movie")) || 1
  const seatHoldIdParam = Number(searchParams.get("seatHoldId")) || 0
  const cinema = searchParams.get("cinema") || ""
  const time = searchParams.get("time") || ""
  const seats =
    searchParams
      .get("seats")
      ?.split(",")
      .map((seat) => seat.trim())
      .filter(Boolean) || []
  const displaySeats = seats.map((seat) => {
    const compact = seat.trim()
    const parts = compact.split("-").map((p) => p.trim()).filter(Boolean)
    if (parts.length === 2) return `Row ${parts[0]}, Seat ${parts[1]}`
    return compact
  })
  const baseTotal = Number(searchParams.get("total")) || 0
  const screeningIdParam = Number(searchParams.get("screeningId")) || 0

  /** One payment intent per seat hold (backend); food draft uses the first hold only. */
  const seatHoldIds = useMemo(() => {
    const raw = searchParams.get("seatHoldIds")
    if (raw) {
      const ids = raw
        .split(",")
        .map((s) => Number(s.trim()))
        .filter((n) => Number.isFinite(n) && n > 0)
      if (ids.length > 0) return ids
    }
    return seatHoldIdParam > 0 ? [seatHoldIdParam] : []
  }, [searchParams, seatHoldIdParam])

  const primarySeatHoldId = seatHoldIds[0] ?? 0
  const [paymentModalOpen, setPaymentModalOpen] = useState(false)

  const [movie, setMovie] = useState<{ title: string; posterUrl?: string | null }>({
    title: "Movie",
    posterUrl: null,
  })
  const [movieLoaded, setMovieLoaded] = useState(false)
  const [foodItems, setFoodItems] = useState<FoodItemResponse[]>([])
  const [foodOrders, setFoodOrders] = useState<Record<number, number>>({})
  const [foodLoading, setFoodLoading] = useState(false)
  const [foodError, setFoodError] = useState<string | null>(null)
  const [foodDraftId, setFoodDraftId] = useState<number | null>(null)
  const foodDraftIdRef = useRef<number | null>(null)
  const [foodPersisting, setFoodPersisting] = useState(false)
  const [foodMessage, setFoodMessage] = useState<string | null>(null)
  const [deliveryType, setDeliveryType] = useState<DeliveryType>("SeatDelivery")
  const [foodNote, setFoodNote] = useState("")
  /** True only after the user changes food qty (+/-). Prevents autosave on mount / server draft hydrate. */
  const [hasTouchedFoodSelection, setHasTouchedFoodSelection] = useState(false)
  /** Skips the first effect run so no debounced draft request is scheduled on initial render. */
  const hasMountedRef = useRef(false)
  const [persistedFoodTotal, setPersistedFoodTotal] = useState<number | null>(null)
  const [processing, setProcessing] = useState(false)
  const [checkingStatus, setCheckingStatus] = useState(false)
  const [paymentState, setPaymentState] = useState<PaymentStatus | "none">("none")
  const [paymentMessage, setPaymentMessage] = useState<string | null>(null)
  const [paymentIntentMeta, setPaymentIntentMeta] = useState<{ clientSecret: string; providerPaymentIntentId: string } | null>(null)
  const [ticketId, setTicketId] = useState<number | null>(null)
  const [lastIntentTotal, setLastIntentTotal] = useState<number | null>(null)
  const [authRequired, setAuthRequired] = useState(false)
  const [checkoutError, setCheckoutError] = useState<string | null>(null)

  useEffect(() => {
    const loadMovie = async () => {
      setMovieLoaded(false)
      try {
        const data = await getMovieById(movieId)
        setMovie({ title: data.title, posterUrl: data.posterUrl })
      } catch {
        setMovie({ title: "Movie", posterUrl: null })
      } finally {
        setMovieLoaded(true)
      }
    }
    void loadMovie()
  }, [movieId])

  useEffect(() => {
    const loadFoodItems = async () => {
      try {
        setFoodLoading(true)
        setFoodError(null)
        const items = await getAllFoodItems({ isAvailable: true, pageNumber: 1, pageSize: 50 })
        console.log("SNACK DATA:", items)
        setFoodItems(items)
      } catch (err) {
        setFoodItems([])
          setFoodError(err instanceof Error ? err.message : "Failed to load food items.")
      } finally {
        setFoodLoading(false)
      }
    }
    void loadFoodItems()
  }, [])

  useEffect(() => {
    foodDraftIdRef.current = foodDraftId
  }, [foodDraftId])

  useEffect(() => {
    const loadExistingDraft = async () => {
      if (!primarySeatHoldId || status !== "authenticated") return
      try {
        const myOrders = await getMyFoodOrders(primarySeatHoldId)
        const pending = myOrders
          .filter((order) => order.status === "Pending")
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())[0]
        if (!pending) return
        foodDraftIdRef.current = pending.id
        setFoodDraftId(pending.id)
        setPersistedFoodTotal(Number(pending.totalAmount))
        const mapped: Record<number, number> = {}
        pending.items.forEach((item) => {
          mapped[item.foodItemId] = item.quantity
        })
        setFoodOrders(mapped)
      } catch {
        // keep local empty state
      }
    }
    void loadExistingDraft()
  }, [primarySeatHoldId, status])

  const foodTotal = Object.entries(foodOrders).reduce((sum, [id, qty]) => {
    const item = foodItems.find((f) => f.id === Number(id))
    return sum + (item?.price || 0) * qty
  }, 0)
  const effectiveFoodTotal = persistedFoodTotal ?? foodTotal
  const grandTotal = baseTotal + effectiveFoodTotal

  const updateFood = (id: number, delta: number) => {
    setHasTouchedFoodSelection(true)
    setFoodOrders((prev) => {
      const current = prev[id] || 0
      const next = Math.max(0, current + delta)
      if (next === 0) {
        const { [id]: _, ...rest } = prev
        return rest
      }
      return { ...prev, [id]: next }
    })
  }

  useEffect(() => {
    if (!hasMountedRef.current) {
      hasMountedRef.current = true
      return
    }

    if (!hasTouchedFoodSelection) return

    const selectedFoodItems = Object.entries(foodOrders).map(([id, qty]) => ({
      foodItemId: Number(id),
      quantity: Number(qty),
    }))

    const validItems = selectedFoodItems
      .filter((item) => Number(item.foodItemId) > 0 && Number(item.quantity) > 0)
      .map((item) => ({
        foodItemId: Number(item.foodItemId),
        quantity: Number(item.quantity),
      }))

    if (!primarySeatHoldId) return
    if (!["SeatDelivery", "CounterPickup"].includes(deliveryType)) return
    if (validItems.length === 0) return

    if (status !== "authenticated") {
      setFoodMessage("Sign in to persist food order.")
      return
    }

    const timeout = window.setTimeout(async () => {
      try {
        setFoodPersisting(true)
        setFoodError(null)

        const draftId = foodDraftIdRef.current

        if (draftId) {
          await updateFoodOrderDraft(draftId, deliveryType, validItems)
          const updatedOrders = await getMyFoodOrders(primarySeatHoldId)
          const current = updatedOrders.find((order) => order.id === draftId)
          if (current) {
            setPersistedFoodTotal(Number(current.totalAmount))
          }
        } else {
          await createFoodOrderDraft(primarySeatHoldId, deliveryType, validItems)
          const myOrders = await getMyFoodOrders(primarySeatHoldId)
          const pending = myOrders
            .filter((order) => order.status === "Pending")
            .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())[0]
          if (pending) {
            foodDraftIdRef.current = pending.id
            setFoodDraftId(pending.id)
            setPersistedFoodTotal(Number(pending.totalAmount))
          }
        }

        setFoodMessage("Food draft saved.")
      } catch (err) {
        if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
          setAuthRequired(true)
        }
        setFoodError(err instanceof Error ? err.message : "Failed to persist food order.")
      } finally {
        setFoodPersisting(false)
      }
    }, 500)

    return () => window.clearTimeout(timeout)
  }, [deliveryType, foodOrders, primarySeatHoldId, status, hasTouchedFoodSelection])

  async function resolveTicketForSeatHold() {
    try {
      const hold = await getSeatHoldById(primarySeatHoldId)
      const tickets = await getMyTickets({ pageNumber: 1, pageSize: 100 })
      const match = tickets.items
        .filter((ticket) => ticket.screeningId === hold.screeningId && ticket.seatId === hold.seatId)
        .sort((a, b) => new Date(b.purchasedAt).getTime() - new Date(a.purchasedAt).getTime())[0]
      setTicketId(match?.id ?? null)
      return match?.id ?? null
    } catch {
      setTicketId(null)
      return null
    }
  }

  async function checkPaymentStatusForHold(holdId: number) {
    if (!holdId) return
    try {
      setCheckingStatus(true)
      setCheckoutError(null)
      console.log("[checkout-status] checking seatHoldId", holdId)
      const statusResponse = await getPaymentStatusBySeatHoldId(holdId)
      if (!statusResponse.hasPayment) {
        setPaymentState("none")
        setPaymentMessage(null)
        return
      }
      const normalized = normalizePaymentStatus(statusResponse.status)
      setPaymentState(normalized)
      if (normalized === "Succeeded") {
        setPaymentMessage("Payment confirmed.")
        await resolveTicketForSeatHold()
      } else if (normalized === "Failed") {
        setPaymentMessage("Payment failed. You can retry.")
      } else if (normalized === "Cancelled") {
        setPaymentMessage("Payment was cancelled. Please select seats again.")
      } else if (normalized === "Pending") {
        setPaymentMessage("Payment is pending confirmation.")
      } else {
        setPaymentMessage(`Payment status: ${String(statusResponse.status)}`)
      }
    } catch (err) {
      setCheckoutError(err instanceof Error ? err.message : "Failed to check payment status.")
    } finally {
      setCheckingStatus(false)
    }
  }

  const currentPayHoldId = seatHoldIds[0] ?? 0
  /** After create-intent, show Payment Element + confirm flow (status normalized to `Pending`). */
  const awaitingCardConfirmation = Boolean(paymentIntentMeta) && paymentState === "Pending"
  const paymentStatusValue = String(paymentState ?? "none")
  const isCancelled = paymentStatusValue === "Cancelled"
  const isFailed = paymentStatusValue === "Failed"
  const isRefunded = paymentStatusValue === "Refunded"
  const isRestartable = isCancelled || isFailed || isRefunded
  const isPending = paymentStatusValue === "Pending"
  const isConfirmed = paymentStatusValue === "Confirmed" || paymentStatusValue === "Succeeded"

  const handlePayment = async () => {
    if (status !== "authenticated") {
      setAuthRequired(true)
      setCheckoutError("Sign in to complete payment.")
      return
    }

    if (seatHoldIds.length === 0 || !Number.isFinite(currentPayHoldId) || currentPayHoldId <= 0) {
      setCheckoutError(
        "No seat hold is linked to this checkout. Please go back and reselect seats."
      )
      return
    }

    try {
      setProcessing(true)
      setCheckoutError(null)
      setPaymentMessage(null)
      const created = await createPaymentIntent({ seatHoldId: currentPayHoldId })
      setLastIntentTotal(created.totalAmount)
      setPaymentIntentMeta({
        clientSecret: created.clientSecret,
        providerPaymentIntentId: created.providerPaymentIntentId,
      })
      setPaymentState(normalizePaymentStatus(created.status))
      setPaymentModalOpen(true)
      await checkPaymentStatusForHold(currentPayHoldId)
    } catch (err) {
      if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
        setAuthRequired(true)
      }
      setCheckoutError(err instanceof Error ? err.message : "Failed to start payment.")
    } finally {
      setProcessing(false)
    }
  }

  const handleRetry = async () => {
    if (!currentPayHoldId) return
    try {
      setProcessing(true)
      setCheckoutError(null)
      const retried = await retryPayment(currentPayHoldId)
      setLastIntentTotal(retried.totalAmount)
      setPaymentIntentMeta({
        clientSecret: retried.clientSecret,
        providerPaymentIntentId: retried.providerPaymentIntentId,
      })
      setPaymentState(normalizePaymentStatus(retried.status))
      setPaymentMessage("Payment retry started. Complete any steps with your bank if prompted, then check status below.")
    } catch (err) {
      setCheckoutError(err instanceof Error ? err.message : "Failed to retry payment.")
    } finally {
      setProcessing(false)
    }
  }

  useEffect(() => {
    if (!currentPayHoldId || status !== "authenticated") return
    void checkPaymentStatusForHold(currentPayHoldId)
  }, [currentPayHoldId, status])

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
          Checkout
        </p>
        <h1 className="font-serif text-3xl font-bold text-foreground">Complete Your Order</h1>
      </div>

      <div className="flex flex-col gap-6 lg:flex-row">
        <div className="flex-1 flex flex-col gap-6">
          {/* Movie Summary */}
          <div className="rounded-2xl border border-border/50 bg-card p-5">
            <h2 className="mb-4 text-sm font-semibold text-foreground">Booking Details</h2>
            <div className="flex gap-4">
              <div className="relative aspect-[2/3] w-20 shrink-0 overflow-hidden rounded-lg">
                {!movieLoaded ? (
                  <div className="h-full w-full animate-pulse bg-secondary/60" />
                ) : (
                  <Image
                    src={movie.posterUrl || "/images/movie-1.jpg"}
                    alt={movie.title}
                    fill
                    className="object-cover"
                  />
                )}
              </div>
              <div className="flex flex-col gap-1.5 text-sm">
                <h3 className="font-serif text-lg font-bold text-foreground">{movie.title}</h3>
                <span className="flex items-center gap-1.5 text-muted-foreground">
                  <MapPin className="h-3.5 w-3.5 text-primary" />{cinema}
                </span>
                <span className="flex items-center gap-1.5 text-muted-foreground">
                  <Clock className="h-3.5 w-3.5" />{time}
                </span>
                <span className="flex items-center gap-1.5 text-muted-foreground">
                  <Armchair className="h-3.5 w-3.5" />Seats: {displaySeats.join(", ") || "None"}
                </span>
              </div>
            </div>
          </div>

          {/* Food Add-ons */}
          <div className="rounded-2xl border border-border/50 bg-card p-5">
            <h2 className="mb-4 text-sm font-semibold text-foreground">Add Snacks & Drinks</h2>
            <div className="mb-4 flex gap-2">
              <button
                onClick={() => setDeliveryType("SeatDelivery")}
                className={cn(
                  "rounded-lg px-3 py-1.5 text-xs font-medium transition-colors",
                  deliveryType === "SeatDelivery"
                    ? "bg-primary text-primary-foreground"
                    : "bg-secondary text-muted-foreground hover:text-foreground"
                )}
              >
                Seat Delivery
              </button>
              <button
                onClick={() => setDeliveryType("CounterPickup")}
                className={cn(
                  "rounded-lg px-3 py-1.5 text-xs font-medium transition-colors",
                  deliveryType === "CounterPickup"
                    ? "bg-primary text-primary-foreground"
                    : "bg-secondary text-muted-foreground hover:text-foreground"
                )}
              >
                Counter Pickup
              </button>
            </div>
            <textarea
              value={foodNote}
              onChange={(e) => setFoodNote(e.target.value)}
              placeholder="Optional note for your food order"
              className="mb-4 min-h-16 w-full rounded-lg border border-border/50 bg-secondary/30 px-3 py-2 text-xs text-foreground"
            />
            {foodLoading && <p className="mb-3 text-xs text-muted-foreground">Loading food menu...</p>}
            {foodError && <p className="mb-3 text-xs text-muted-foreground">{foodError}</p>}
            {!foodLoading && !foodError && foodItems.length === 0 && (
              <p className="mb-3 text-xs text-muted-foreground">No food items available.</p>
            )}
            <div className="grid gap-3 sm:grid-cols-2">
              {foodItems.map((snack) => {
                const qty = foodOrders[snack.id] || 0
                const snackImageUrl = resolveSnackImageUrl(snack)
                return (
                  <div
                    key={snack.id}
                    className={cn(
                      "flex items-center gap-4 rounded-2xl border border-teal-100 p-4 transition-colors",
                      qty > 0 ? "border-primary/30 bg-primary/5" : "border-border/50 bg-secondary/30"
                    )}
                  >
                    <img
                      src={snackImageUrl}
                      alt={snack.name}
                      className="h-20 w-20 flex-shrink-0 rounded-xl border border-teal-100 bg-slate-100 object-cover"
                      onError={(e) => {
                        e.currentTarget.src = "/placeholder-food.png"
                      }}
                    />

                    <div className="flex-1">
                      <h3 className="font-semibold">{snack.name}</h3>
                      <p>{snack.price} AZN</p>
                    </div>

                    <div className="flex items-center gap-2">
                      {qty > 0 && (
                        <button
                          onClick={() => updateFood(snack.id, -1)}
                          className="flex h-7 w-7 items-center justify-center rounded-full bg-secondary text-foreground transition-colors hover:bg-secondary/80"
                        >
                          <Minus className="h-3.5 w-3.5" />
                        </button>
                      )}
                      {qty > 0 && (
                        <span className="w-5 text-center text-sm font-bold text-foreground">{qty}</span>
                      )}
                      <button
                        onClick={() => updateFood(snack.id, 1)}
                        className="flex h-7 w-7 items-center justify-center rounded-full bg-primary text-primary-foreground transition-colors hover:bg-primary/90"
                      >
                        <Plus className="h-3.5 w-3.5" />
                      </button>
                    </div>
                  </div>
                )
              })}
            </div>
            {foodPersisting && <p className="mt-3 text-xs text-muted-foreground">Saving food draft...</p>}
            {foodMessage && <p className="mt-2 text-xs text-primary">{foodMessage}</p>}
          </div>
        </div>

        {/* Payment Summary */}
        <div className="w-full lg:w-72">
          <div className="sticky top-24 rounded-2xl border border-border/50 bg-card p-6">
            <h3 className="mb-4 font-serif text-lg font-bold text-foreground">Payment</h3>

            <div className="flex flex-col gap-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tickets ({seats.length}x)</span>
                <span className="text-foreground">{baseTotal} AZN</span>
              </div>
              {effectiveFoodTotal > 0 && (
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Food & Drinks</span>
                  <span className="text-foreground">{effectiveFoodTotal} AZN</span>
                </div>
              )}
              <div className="border-t border-border/50 pt-2">
                <div className="flex justify-between">
                  <span className="font-semibold text-foreground">Total</span>
                  <span className="text-lg font-bold text-primary">{grandTotal} AZN</span>
                </div>
              </div>
            </div>

            {isRestartable ? (
              <button
                type="button"
                onClick={() => {
                  console.log("SELECT_SEATS_AGAIN_CLICKED")

                  // Clear checkout/payment state bound to old cancelled hold.
                  setPaymentModalOpen(false)
                  setPaymentIntentMeta(null)
                  setProcessing(false)
                  setCheckingStatus(false)
                  setPaymentState("none")
                  setPaymentMessage(null)
                  setCheckoutError(null)
                  setTicketId(null)
                  setLastIntentTotal(null)
                  setAuthRequired(false)

                  localStorage.removeItem("seatHoldId")
                  localStorage.removeItem("selectedSeats")
                  localStorage.removeItem("checkoutState")
                  localStorage.removeItem("bookingCheckout")
                  localStorage.removeItem("paymentState")
                  localStorage.removeItem("seatHoldIds")
                  localStorage.removeItem("bookingState")

                  sessionStorage.removeItem("seatHoldId")
                  sessionStorage.removeItem("selectedSeats")
                  sessionStorage.removeItem("checkoutState")
                  sessionStorage.removeItem("seatHoldIds")
                  sessionStorage.removeItem("bookingState")

                  const id = screeningIdParam || searchParams.get("screeningId")
                  if (!id) {
                    console.error("Missing screeningId for Select seats again redirect")
                    return
                  }

                  window.location.href = `/seats?movie=${movieId}&cinema=${encodeURIComponent(cinema)}&time=${encodeURIComponent(time)}&screeningId=${id}&resetCheckout=1`
                }}
                disabled={false}
                className="mt-5 flex w-full items-center justify-center gap-2 rounded-lg bg-primary py-3 text-sm font-semibold text-primary-foreground transition-all hover:bg-primary/90"
              >
                Select seats again
              </button>
            ) : (
              <button
                type="button"
                onClick={() => void handlePayment()}
                disabled={
                  processing ||
                  status !== "authenticated" ||
                  awaitingCardConfirmation ||
                  seatHoldIds.length === 0 ||
                  isPending ||
                  isConfirmed
                }
                className={cn(
                  "mt-5 flex w-full items-center justify-center gap-2 rounded-lg py-3 text-sm font-semibold transition-all",
                  isConfirmed && "cursor-not-allowed bg-green-500 text-white",
                  isPending && "cursor-not-allowed bg-primary/60 text-primary-foreground opacity-70",
                  processing
                    ? "bg-primary/70 text-primary-foreground"
                    : "bg-primary text-primary-foreground hover:bg-primary/90"
                )}
              >
                {processing ? (
                  <>
                    <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground/30 border-t-primary-foreground" />
                    Processing...
                  </>
                ) : (
                  <>
                    <CreditCard className="h-4 w-4" />
                    {isConfirmed
                      ? "Confirmed"
                      : isPending
                        ? "Processing..."
                        : status !== "authenticated"
                      ? "Sign in to Pay"
                      : awaitingCardConfirmation
                        ? "Payment session ready"
                        : `Pay ${grandTotal} AZN`}
                  </>
                )}
              </button>
            )}

            <button
              type="button"
              onClick={() => void checkPaymentStatusForHold(currentPayHoldId)}
              disabled={checkingStatus || !currentPayHoldId || status !== "authenticated"}
              className="mt-2 w-full rounded-lg border border-border/40 px-3 py-2 text-xs font-medium text-muted-foreground transition-colors hover:bg-secondary disabled:opacity-60"
            >
              {checkingStatus ? "Checking..." : "Check Payment Status"}
            </button>

            {paymentState === "Failed" && (
              <button
                type="button"
                onClick={() => void handleRetry()}
                disabled={processing || !currentPayHoldId || status !== "authenticated"}
                className="mt-2 w-full rounded-lg border border-primary/40 px-3 py-2 text-xs font-medium text-primary transition-colors hover:bg-primary/10 disabled:opacity-60"
              >
                Retry Payment
              </button>
            )}

            {paymentMessage && <p className="mt-3 text-xs text-primary">{paymentMessage}</p>}
            {checkoutError && <p className="mt-2 text-xs text-muted-foreground">{checkoutError}</p>}
            {authRequired && <p className="mt-2 text-xs text-muted-foreground">Sign in is required for payment and ticket creation.</p>}
            {paymentState === "Succeeded" && (
              <button
                type="button"
                onClick={() => router.push(ticketId ? `/tickets?ticketId=${ticketId}` : "/tickets")}
                className="mt-3 w-full rounded-lg bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground hover:bg-primary/90"
              >
                View Ticket{ticketId ? "" : "s"}
              </button>
            )}

            <p className="mt-3 text-center text-[10px] text-muted-foreground">
              Secure payment powered by CineVerse
            </p>
          </div>
        </div>
      </div>

      <Dialog open={paymentModalOpen} onOpenChange={setPaymentModalOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto border-border/50 bg-card p-5 pb-6 sm:max-w-xl sm:p-6 sm:pb-6">
          <DialogHeader>
            <DialogTitle className="font-serif text-xl text-foreground">Secure Checkout</DialogTitle>
            <DialogDescription className="text-xs text-muted-foreground">
              Review your order and confirm one payment for this booking.
            </DialogDescription>
          </DialogHeader>

          <div className="rounded-xl border border-border/50 bg-secondary/20 p-4 text-sm">
            <div className="mb-2 flex justify-between">
              <span className="text-muted-foreground">Tickets ({seats.length}x)</span>
              <span className="text-foreground">{baseTotal} AZN</span>
            </div>
            {effectiveFoodTotal > 0 && (
              <div className="mb-2 flex justify-between">
                <span className="text-muted-foreground">Food & Drinks</span>
                <span className="text-foreground">{effectiveFoodTotal} AZN</span>
              </div>
            )}
            <div className="flex justify-between border-t border-border/50 pt-2">
              <span className="font-semibold text-foreground">Total</span>
              <span className="text-base font-bold text-primary">{grandTotal} AZN</span>
            </div>
          </div>

          {awaitingCardConfirmation && paymentIntentMeta ? (
            <div className="text-foreground">
              <p className="mb-3 text-[10px] leading-relaxed text-muted-foreground">
                {isStripeConfigured()
                  ? "Enter your card details and confirm payment."
                  : "Add NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY to use card payment form."}
              </p>
              <CheckoutStripePayment
                clientSecret={paymentIntentMeta.clientSecret}
                disabled={processing}
                onComplete={async () => {
                  if (currentPayHoldId) await checkPaymentStatusForHold(currentPayHoldId)
                  setPaymentIntentMeta(null)
                  setPaymentMessage("Payment completed for this booking.")
                  setPaymentModalOpen(false)
                }}
                onError={(message) => setCheckoutError(message)}
              />
            </div>
          ) : (
            <p className="text-xs text-muted-foreground">
              Start payment from the summary panel to open a secure payment session here.
            </p>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}
