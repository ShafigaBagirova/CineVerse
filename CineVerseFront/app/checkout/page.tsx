import { Suspense } from "react"
import { CheckoutForm } from "@/components/booking/checkout-form"

export default function CheckoutPage() {
  return (
    <Suspense fallback={<div className="flex min-h-[60vh] items-center justify-center text-muted-foreground">Loading...</div>}>
      <CheckoutForm />
    </Suspense>
  )
}
