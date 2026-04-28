"use client"

import { useEffect, useState } from "react"
import { Crown } from "lucide-react"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { CheckoutStripePayment } from "@/components/booking/checkout-stripe-payment"
import { getCurrentUser, refreshSession } from "@/lib/api/auth"
import type { JwtUserInfo } from "@/lib/api/auth"
import { createVipPaymentIntent } from "@/lib/api/payments"
import { ApiError } from "@/lib/api/types"
import { isVipUser } from "@/lib/roles"
import { Spinner } from "@/components/ui/spinner"

type Props = {
  open: boolean
  onOpenChange: (open: boolean) => void
  user: JwtUserInfo | null
  onRefreshUser: () => Promise<void>
}

function formatAzn(amount: number) {
  try {
    return new Intl.NumberFormat(undefined, { style: "currency", currency: "AZN" }).format(amount)
  } catch {
    return `${amount.toFixed(2)} AZN`
  }
}

export function VipUpgradeDialog({ open, onOpenChange, user, onRefreshUser }: Props) {
  const [clientSecret, setClientSecret] = useState<string | null>(null)
  const [amount, setAmount] = useState<number | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  /** After Stripe succeeds: poll until VIP role appears (webhook can lag). */
  const [postPayment, setPostPayment] = useState<"idle" | "activating" | "done">("idle")
  const [postPaymentMessage, setPostPaymentMessage] = useState<string | null>(null)

  useEffect(() => {
    if (!open) {
      setClientSecret(null)
      setAmount(null)
      setError(null)
      setPostPayment("idle")
      setPostPaymentMessage(null)
      return
    }
    if (isVipUser(user)) return
    let cancelled = false
    setLoading(true)
    setError(null)
    void (async () => {
      try {
        const data = await createVipPaymentIntent()
        if (cancelled) return
        setClientSecret(data.clientSecret)
        setAmount(data.amount)
      } catch (e) {
        if (!cancelled) setError(e instanceof ApiError ? e.message : "Could not start payment.")
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [open, user])

  async function handlePaymentComplete() {
    setError(null)
    setPostPayment("activating")
    setPostPaymentMessage("Activating VIP… This usually takes a few seconds.")

    const maxAttempts = 40
    const delayMs = 750
    const refreshAttempts = 5

    for (let i = 0; i < maxAttempts; i++) {
      if (i < refreshAttempts) {
        try {
          await refreshSession({ quiet: true })
        } catch {
          /* Session refresh can fail while webhook is still processing; keep polling /me. */
        }
      }

      try {
        const me = await getCurrentUser()
        if (isVipUser(me)) {
          try {
            await onRefreshUser()
          } catch {
            /* Context sync is best-effort; tokens and /me already show VIP. */
          }
          setPostPaymentMessage("Welcome! Your VIP membership is active.")
          setPostPayment("done")
          window.setTimeout(() => onOpenChange(false), 1400)
          return
        }
      } catch {
        /* Ignore transient /me errors while waiting for the VIP role. */
      }

      await new Promise((r) => setTimeout(r, delayMs))
    }

    setPostPaymentMessage(
      "Payment received. VIP may take a moment to appear — check back shortly or refresh the page."
    )
    setPostPayment("done")
    try {
      await onRefreshUser()
    } catch {
      /* ignore */
    }
    window.setTimeout(() => onOpenChange(false), 2800)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-md">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <Crown className="h-5 w-5 text-primary" />
            <DialogTitle>Become a VIP member</DialogTitle>
          </div>
          <DialogDescription>
            Monthly plan — same secure Stripe checkout as ticket purchases. After payment succeeds, your account is
            upgraded when the payment is confirmed (usually within seconds).
          </DialogDescription>
        </DialogHeader>
        {loading && (
          <div className="flex justify-center py-8">
            <Spinner className="h-8 w-8 text-primary" />
          </div>
        )}
        {error && !loading && postPayment === "idle" && <p className="text-sm text-destructive">{error}</p>}
        {(postPayment === "activating" || postPayment === "done") && (
          <div className="flex flex-col items-center gap-3 rounded-lg border border-border/50 bg-muted/20 px-4 py-6 text-center">
            {postPayment === "activating" && <Spinner className="h-8 w-8 text-primary" />}
            <p className="text-sm text-foreground">{postPaymentMessage}</p>
            {postPayment === "done" && (
              <p className="text-xs text-muted-foreground">You can close this dialog when ready.</p>
            )}
          </div>
        )}
        {!loading && !error && amount != null && postPayment === "idle" && (
          <p className="text-sm font-medium text-foreground">
            {formatAzn(amount)}
            <span className="text-muted-foreground"> / month</span>
          </p>
        )}
        {clientSecret && !loading && !error && postPayment === "idle" && (
          <CheckoutStripePayment
            clientSecret={clientSecret}
            onComplete={() => void handlePaymentComplete()}
            onError={(msg) => setError(msg)}
          />
        )}
      </DialogContent>
    </Dialog>
  )
}
