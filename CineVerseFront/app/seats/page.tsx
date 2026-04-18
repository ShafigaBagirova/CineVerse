import { Suspense } from "react"
import { SeatSelector } from "@/components/booking/seat-selector"

export default function SeatsPage() {
  return (
    <Suspense fallback={<div className="flex min-h-[60vh] items-center justify-center text-muted-foreground">Loading...</div>}>
      <SeatSelector />
    </Suspense>
  )
}
