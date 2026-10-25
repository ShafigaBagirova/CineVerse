"use client"

import { useEffect, useState } from "react"
import { getNetflixSupport, type NetflixSupport } from "@/lib/browser-support"

/** "checking" only lasts until the browser is known (it cannot be read while the page is rendered on the server). */
export function useNetflixSupport(): NetflixSupport | "checking" {
  const [support, setSupport] = useState<NetflixSupport | "checking">("checking")

  useEffect(() => {
    setSupport(getNetflixSupport(navigator.userAgent, navigator.maxTouchPoints))
  }, [])

  return support
}
