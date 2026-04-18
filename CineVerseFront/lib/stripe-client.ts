import { loadStripe } from "@stripe/stripe-js"

const publishableKey = process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY ?? ""

/** Same promise for every render; null if key missing (see `isStripeConfigured`). */
export const stripePromise = publishableKey.trim() ? loadStripe(publishableKey) : null

export function isStripeConfigured(): boolean {
  return Boolean(publishableKey.trim())
}
