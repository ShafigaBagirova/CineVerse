"use client"

import Link from "next/link"
import { ExternalLink, Laptop, Puzzle, RefreshCw } from "lucide-react"
import { Button } from "@/components/ui/button"
import type { NetflixSupport } from "@/lib/browser-support"
import { EXTENSION_STORE_URL } from "@/lib/netflix-extension-link"
import { cn } from "@/lib/utils"

type Tone = "dark" | "light"

const mutedClass = (tone: Tone) => (tone === "dark" ? "text-zinc-400" : "text-muted-foreground")

/** Shown instead of a Netflix room where the extension cannot exist: phones, Firefox, Safari. */
export function NetflixUnsupportedNotice({
  support,
  tone,
}: {
  support: Exclude<NetflixSupport, "supported">
  tone: Tone
}) {
  return (
    <>
      <Laptop className={cn("h-8 w-8", mutedClass(tone))} />
      <p className="text-base font-medium">Netflix rooms need Chrome or Edge on a computer</p>
      <p className={cn("max-w-md text-sm", mutedClass(tone))}>
        {support === "mobile" ? "Netflix rooms are not available on phones yet." : "This browser is not supported."} Open
        CineVerse in Chrome or Edge on a computer to use a Netflix room. YouTube rooms work in any browser.
      </p>
      <Button asChild variant={tone === "dark" ? "secondary" : "outline"} size="sm">
        <Link href="/watch">Back to watch rooms</Link>
      </Button>
    </>
  )
}

/** Shown where the extension should be but did not answer. The page keeps asking, so it connects by itself. */
export function NetflixExtensionMissingNotice({ tone }: { tone: Tone }) {
  const hasStoreLink = EXTENSION_STORE_URL !== ""

  return (
    <>
      <Puzzle className={cn("h-8 w-8", mutedClass(tone))} />
      <p className="text-base font-medium">CineVerse extension not found</p>
      <p className={cn("max-w-md text-sm", mutedClass(tone))}>
        Netflix plays in your own browser window, so everyone in the room needs the CineVerse extension in their own
        browser (Chrome or Edge).{" "}
        {hasStoreLink
          ? "Add it and this page connects by itself."
          : "Open chrome://extensions (or edge://extensions), turn on Developer mode and use Load unpacked on the CineVerseExtension folder. Allow it in private windows if you use one. This page then connects by itself."}
      </p>
      <div className="flex flex-wrap justify-center gap-2">
        {hasStoreLink ? (
          <Button asChild size="sm">
            <a href={EXTENSION_STORE_URL} target="_blank" rel="noopener noreferrer">
              <ExternalLink className="mr-1 h-4 w-4" />
              Add the extension
            </a>
          </Button>
        ) : null}
        <Button variant={tone === "dark" ? "secondary" : "outline"} size="sm" onClick={() => window.location.reload()}>
          <RefreshCw className="mr-1 h-4 w-4" />
          Reload page
        </Button>
      </div>
      <p className={cn("max-w-md text-xs", mutedClass(tone))}>If nothing changes after a few seconds, reload the page.</p>
    </>
  )
}
