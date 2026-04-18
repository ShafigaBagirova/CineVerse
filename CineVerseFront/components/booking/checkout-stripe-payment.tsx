"use client"

import { useState } from "react"
import { PaymentElement, Elements, useElements, useStripe } from "@stripe/react-stripe-js"
import { isStripeConfigured, stripePromise } from "@/lib/stripe-client"
import { cn } from "@/lib/utils"

type Props = {
  /** PaymentIntent client secret from the backend — never shown in the DOM. */
  clientSecret: string
  disabled?: boolean
  onComplete: () => void | Promise<void>
  onError: (message: string) => void
}

function ConfirmPaymentForm({ disabled, onComplete, onError }: Props) {
  const stripe = useStripe()
  const elements = useElements()
  const [busy, setBusy] = useState(false)
  const [localError, setLocalError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!stripe || !elements || disabled || busy) return

    setBusy(true)
    setLocalError(null)
    try {
      const { error } = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: typeof window !== "undefined" ? window.location.href : "",
        },
        redirect: "if_required",
      })

      if (error) {
        const msg = error.message ?? "Payment could not be completed."
        setLocalError(msg)
        onError(msg)
        return
      }

      await onComplete()
    } finally {
      setBusy(false)
    }
  }

  return (
    <form onSubmit={(e) => void handleSubmit(e)} className="flex flex-col gap-3" aria-busy={busy}>
      <h4 className="text-sm font-semibold text-foreground">Card details</h4>
      <div className="rounded-lg border border-border/40 bg-background/50 p-3 sm:p-4">
        <PaymentElement
          options={{
            layout: "tabs",
          }}
        />
      </div>
      <p className="text-[10px] leading-relaxed text-muted-foreground">
        <span className="font-medium text-foreground/90">Test card:</span>{" "}
        <span className="font-mono text-foreground/90">4242 4242 4242 4242</span>
        <br />
        Expiry: any future date · CVC: any 3 digits · Postal code: any value if required
      </p>
      {localError && (
        <p className="text-xs text-red-400" role="alert">
          {localError}
        </p>
      )}
      <button
        type="submit"
        disabled={!stripe || disabled || busy}
        className={cn(
          "w-full rounded-lg border border-primary/40 bg-primary px-3 py-2.5 text-xs font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
        )}
      >
        {busy ? "Processing…" : "Confirm payment"}
      </button>
    </form>
  )
}

/**
 * Stripe Payment Element + confirmPayment. Remount when `clientSecret` changes (e.g. next seat hold).
 */
export function CheckoutStripePayment({ clientSecret, disabled, onComplete, onError }: Props) {
  if (!isStripeConfigured() || !stripePromise) {
    return (
      <div className="rounded-lg border border-border/40 bg-secondary/30 p-3 text-[10px] text-muted-foreground">
        <p className="font-medium text-foreground">Card payment not available</p>
        <p className="mt-1.5 leading-relaxed">
          Set <code className="rounded bg-secondary px-1 py-0.5 text-[9px]">NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY</code>{" "}
          (test key <code className="rounded bg-secondary px-1 py-0.5 text-[9px]">pk_test_…</code>) matching your backend
          Stripe account. Then reload and use a{" "}
          <a
            href="https://docs.stripe.com/testing#cards"
            target="_blank"
            rel="noreferrer"
            className="text-primary underline underline-offset-2"
          >
            Stripe test card
          </a>
          .
        </p>
      </div>
    )
  }

  return (
    <Elements
      key={clientSecret}
      stripe={stripePromise}
      options={{
        clientSecret,
        appearance: {
          theme: "night",
          variables: {
            colorPrimary: "#22c55e",
            colorBackground: "#0c0c0c",
            colorText: "#fafafa",
            colorDanger: "#f87171",
            borderRadius: "8px",
            fontFamily: "ui-sans-serif, system-ui, sans-serif",
          },
          rules: {
            ".Input": {
              border: "1px solid rgba(255,255,255,0.12)",
            },
          },
        },
      }}
    >
      <ConfirmPaymentForm
        clientSecret={clientSecret}
        disabled={disabled}
        onComplete={onComplete}
        onError={onError}
      />
    </Elements>
  )
}
