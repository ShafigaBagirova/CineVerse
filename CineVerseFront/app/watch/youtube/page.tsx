import { Suspense } from "react"
import { YouTubePicker } from "@/components/watch/youtube-picker"

export default function WatchYouTubeRoute() {
  return (
    <Suspense fallback={null}>
      <YouTubePicker />
    </Suspense>
  )
}
