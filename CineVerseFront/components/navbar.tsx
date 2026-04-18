"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useState } from "react"
import { Film, Search, Menu, X, Ticket, User, Heart, Clock, BarChart3, Bell, Bookmark } from "lucide-react"
import { cn } from "@/lib/utils"
import { useAuth } from "@/components/providers/auth-provider"
import { useNotifications } from "@/components/providers/notification-provider"
import { searchUsers, type UserSearchDto } from "@/lib/api/user"

const navLinks = [
  { href: "/", label: "Home" },
  { href: "/movies", label: "Movies" },
  { href: "/showtimes", label: "Showtimes" },
  { href: "/watchlist", label: "Watchlist" },
  { href: "/tickets", label: "Tickets" },
  { href: "/profile", label: "Profile" },
  { href: "/vip", label: "VIP" },
]

const mobileNavLinks = [
  { href: "/", label: "Home", icon: Film },
  { href: "/movies", label: "Movies", icon: Search },
  { href: "/showtimes", label: "Showtimes", icon: Clock },
  { href: "/watchlist", label: "Watchlist", icon: Heart },
  { href: "/tickets", label: "Tickets", icon: Ticket },
  { href: "/profile", label: "Profile", icon: User },
  { href: "/vip", label: "VIP", icon: BarChart3 },
]

export function Navbar() {
  const pathname = usePathname()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [notificationOpen, setNotificationOpen] = useState(false)
  const [searchText, setSearchText] = useState("")
  const [userSearchResults, setUserSearchResults] = useState<UserSearchDto[]>([])
  const [userSearchOpen, setUserSearchOpen] = useState(false)
  const [userSearchLoading, setUserSearchLoading] = useState(false)
  const [userSearchError, setUserSearchError] = useState<string | null>(null)
  const { status, user, logout } = useAuth()
  const { notifications, unreadCount, loading, error, authRequired, markAsRead } = useNotifications()

  if (pathname.startsWith("/admin")) {
    return null
  }

  const submitUserSearch = async (value: string) => {
    const q = value.trim()
    if (!q) {
      setUserSearchResults([])
      setUserSearchError(null)
      setUserSearchOpen(false)
      return
    }
    try {
      setUserSearchLoading(true)
      setUserSearchError(null)
      const response = await searchUsers(q, 1, 8)
      setUserSearchResults(response.items)
      setUserSearchOpen(true)
    } catch (err) {
      setUserSearchResults([])
      setUserSearchError(err instanceof Error ? err.message : "Failed to search users.")
      setUserSearchOpen(true)
    } finally {
      setUserSearchLoading(false)
    }
  }

  return (
    <>
      <nav className="fixed top-0 left-0 right-0 z-50 border-b border-border/20 bg-background/80 backdrop-blur-xl">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 lg:px-8">
          <Link href="/" className="flex items-center gap-2">
            <Film className="h-6 w-6 text-primary" />
            <span className="text-lg font-bold tracking-tight text-foreground">
              CineVerse
            </span>
          </Link>

          <div className="hidden items-center gap-1 md:flex">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className={cn(
                  "rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  pathname === link.href
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                )}
              >
                {link.label}
              </Link>
            ))}
          </div>

          <div className="flex items-center gap-3">
            <div className="relative hidden md:block">
              <form
                onSubmit={(e) => {
                  e.preventDefault()
                  void submitUserSearch(searchText)
                }}
                className="flex items-center gap-1 rounded-lg border border-border/40 bg-card px-2 py-1"
              >
                <Search className="h-4 w-4 text-muted-foreground" />
                <input
                  type="text"
                  value={searchText}
                  onChange={(e) => setSearchText(e.target.value)}
                  placeholder="Search users..."
                  className="w-36 bg-transparent text-sm text-foreground placeholder:text-muted-foreground focus:outline-none"
                />
                <button
                  type="submit"
                  className="rounded-md px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                >
                  Go
                </button>
              </form>
              {userSearchOpen && (
                <div className="absolute right-0 top-12 w-72 rounded-lg border border-border/20 bg-surface shadow-lg">
                  <div className="max-h-80 overflow-y-auto">
                    {userSearchLoading ? (
                      <div className="px-4 py-4 text-sm text-muted-foreground">Searching users...</div>
                    ) : userSearchError ? (
                      <div className="px-4 py-4 text-sm text-muted-foreground">{userSearchError}</div>
                    ) : userSearchResults.length === 0 ? (
                      <div className="px-4 py-4 text-sm text-muted-foreground">No users found</div>
                    ) : (
                      userSearchResults.map((u, index) => {
                        if (!u.id) return null
                        return (
                          <div key={`${u.id}-${index}`}>
                            <Link
                              href={`/profile/${u.id}`}
                              onClick={() => setUserSearchOpen(false)}
                              className="flex items-center gap-3 border-b border-border/20 px-4 py-3 transition-colors hover:bg-surface-hover"
                            >
                              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                                {(u.fullName || u.userName).slice(0, 2).toUpperCase()}
                              </div>
                              <div>
                                <p className="text-sm font-medium text-foreground">{u.fullName || u.userName}</p>
                                <p className="text-xs text-muted-foreground">@{u.userName}</p>
                              </div>
                            </Link>
                          </div>
                        )
                      })
                    )}
                  </div>
                </div>
              )}
            </div>
            <button
              type="button"
              onClick={() => void submitUserSearch(searchText)}
              className="rounded-lg p-2 text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground md:hidden"
            >
              <Search className="h-5 w-5" />
              <span className="sr-only">Search users</span>
            </button>
            <div className="relative">
              <button 
                onClick={() => setNotificationOpen(!notificationOpen)}
                className="relative rounded-lg p-2 text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
              >
                <Bell className="h-5 w-5" />
                {unreadCount > 0 && (
                  <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground">
                    {unreadCount > 9 ? '9+' : unreadCount}
                  </span>
                )}
                <span className="sr-only">Notifications</span>
              </button>
              {notificationOpen && (
                <div className="absolute right-0 top-12 w-80 rounded-lg border border-border/20 bg-surface shadow-lg">
                  <div className="border-b border-border/20 px-4 py-3">
                    <h3 className="text-sm font-semibold text-foreground">Notifications</h3>
                  </div>
                  <div className="max-h-96 overflow-y-auto">
                    {loading ? (
                      <div className="px-4 py-6 text-center text-sm text-muted-foreground">Loading notifications...</div>
                    ) : authRequired ? (
                      <div className="px-4 py-6 text-center text-sm text-muted-foreground">Sign in to view notifications.</div>
                    ) : error ? (
                      <div className="px-4 py-6 text-center text-sm text-muted-foreground">{error}</div>
                    ) : notifications.length > 0 ? (
                      notifications.map((notif) => (
                        <Link
                          key={notif.id}
                          href="/notifications"
                          className={cn(
                            "flex gap-3 border-b border-border/20 px-4 py-3 transition-colors hover:bg-surface-hover",
                            !notif.isRead && "bg-surface-hover"
                          )}
                          onClick={() => {
                            void markAsRead(notif.id)
                            setNotificationOpen(false)
                          }}
                        >
                          <div className="flex-1">
                            <p className="text-sm font-medium text-foreground">{notif.title}</p>
                            <p className="text-xs text-muted-foreground">{notif.message}</p>
                            <p className="text-xs text-muted-foreground mt-1">{new Date(notif.createdAt).toLocaleString()}</p>
                          </div>
                          {!notif.isRead && <div className="h-2 w-2 rounded-full bg-primary flex-shrink-0 mt-1" />}
                        </Link>
                      ))
                    ) : (
                      <div className="px-4 py-6 text-center text-sm text-muted-foreground">
                        No notifications
                      </div>
                    )}
                  </div>
                  <div className="border-t border-border/20 px-4 py-2">
                    <Link
                      href="/notifications"
                      className="text-xs font-medium text-primary hover:underline"
                      onClick={() => setNotificationOpen(false)}
                    >
                      View all notifications
                    </Link>
                  </div>
                </div>
              )}
            </div>
            {status === "authenticated" ? (
              <div className="hidden items-center gap-2 md:flex">
                <Link
                  href="/profile"
                  className="rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
                >
                  {user?.userName ?? "Profile"}
                </Link>
                <button
                  onClick={logout}
                  className="rounded-lg border border-border/40 px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                >
                  Logout
                </button>
              </div>
            ) : (
              <Link
                href="/auth"
                className="hidden rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 md:block"
              >
                {status === "loading" ? "Loading..." : "Sign In"}
              </Link>
            )}
            <button
              className="rounded-lg p-2 text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground md:hidden"
              onClick={() => setMobileOpen(!mobileOpen)}
            >
              {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
              <span className="sr-only">Toggle menu</span>
            </button>
          </div>
        </div>

        {mobileOpen && (
          <div className="border-t border-border/50 bg-background/95 backdrop-blur-xl md:hidden">
            <div className="flex flex-col px-4 py-3">
              {mobileNavLinks.map((link) => {
                const Icon = link.icon
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    onClick={() => setMobileOpen(false)}
                    className={cn(
                      "flex items-center gap-3 rounded-lg px-3 py-3 text-sm font-medium transition-colors",
                      pathname === link.href
                        ? "bg-primary/10 text-primary"
                        : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                    )}
                  >
                    <Icon className="h-4 w-4" />
                    {link.label}
                  </Link>
                )
              })}
              <div className="mt-2 border-t border-border/50 pt-3">
                <Link
                  href={status === "authenticated" ? "/profile" : "/auth"}
                  onClick={() => setMobileOpen(false)}
                  className="block rounded-lg bg-primary px-4 py-2.5 text-center text-sm font-medium text-primary-foreground"
                >
                  {status === "authenticated" ? "Profile" : status === "loading" ? "Loading..." : "Sign In"}
                </Link>
                {status === "authenticated" && (
                  <button
                    onClick={() => {
                      logout()
                      setMobileOpen(false)
                    }}
                    className="mt-2 block w-full rounded-lg border border-border/40 px-4 py-2.5 text-center text-sm font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                  >
                    Logout
                  </button>
                )}
              </div>
            </div>
          </div>
        )}
      </nav>
      <div className="h-16" />
    </>
  )
}
