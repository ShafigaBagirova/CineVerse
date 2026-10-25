import { Suspense } from "react"
import { NetflixPicker } from "@/components/watch/netflix-picker"

export default function WatchNetflixRoute() {
  return (
    <Suspense fallback={null}>
      <NetflixPicker />
    </Suspense>
  )
}
