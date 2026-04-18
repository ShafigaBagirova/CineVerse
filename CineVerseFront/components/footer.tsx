import Link from "next/link"
import { Film } from "lucide-react"

export function Footer() {
  return (
    <footer className="border-t border-border/50 bg-background">
      <div className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
        <div className="grid grid-cols-2 gap-8 md:grid-cols-4">
          <div className="col-span-2 md:col-span-1">
            <Link href="/" className="flex items-center gap-2">
              <Film className="h-6 w-6 text-primary" />
              <span className="text-lg font-bold text-foreground">CineVerse</span>
            </Link>
            <p className="mt-3 text-sm leading-relaxed text-muted-foreground">
              Discover, watch, and experience cinema like never before. Your all-in-one movie companion.
            </p>
          </div>
          <div>
            <h4 className="mb-3 text-sm font-semibold text-foreground">Explore</h4>
            <ul className="flex flex-col gap-2">
              <li><Link href="/movies" className="text-sm text-muted-foreground transition-colors hover:text-foreground">Movies</Link></li>
              <li><Link href="/showtimes" className="text-sm text-muted-foreground transition-colors hover:text-foreground">Showtimes</Link></li>
              <li><Link href="/watchlist" className="text-sm text-muted-foreground transition-colors hover:text-foreground">Watchlist</Link></li>
            </ul>
          </div>
          <div>
            <h4 className="mb-3 text-sm font-semibold text-foreground">Account</h4>
            <ul className="flex flex-col gap-2">
              <li><Link href="/profile" className="text-sm text-muted-foreground transition-colors hover:text-foreground">Profile</Link></li>
              <li><Link href="/tickets" className="text-sm text-muted-foreground transition-colors hover:text-foreground">My Tickets</Link></li>
              <li><Link href="/vip" className="text-sm text-muted-foreground transition-colors hover:text-foreground">VIP Analytics</Link></li>
            </ul>
          </div>
          <div>
            <h4 className="mb-3 text-sm font-semibold text-foreground">Company</h4>
            <ul className="flex flex-col gap-2">
              <li><span className="text-sm text-muted-foreground">About</span></li>
              <li><span className="text-sm text-muted-foreground">Contact</span></li>
              <li><span className="text-sm text-muted-foreground">Privacy Policy</span></li>
            </ul>
          </div>
        </div>
        <div className="mt-10 border-t border-border/50 pt-6">
          <p className="text-center text-xs text-muted-foreground">
            2026 CineVerse. All rights reserved. Made with passion for cinema.
          </p>
        </div>
      </div>
    </footer>
  )
}
