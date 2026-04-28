"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useState } from "react"
import { Film, Search, Menu, X, Ticket, User, Clock, BarChart3, Bell, Bookmark } from "lucide-react"
import { cn } from "@/lib/utils"
import { useAuth } from "@/components/providers/auth-provider"
import { useNotifications } from "@/components/providers/notification-provider"
import { formatNotificationType } from "@/lib/api/notifications"
import { searchUsers, type UserSearchDto } from "@/lib/api/user"

const navLinks = [
  { href: "/", label: "Home" },
  { href: "/movies", label: "Movies" },
  { href: "/suggestions", label: "Suggestions" },
  { href: "/showtimes", label: "Showtimes" },
  { href: "/tickets", label: "Tickets" },
  { href: "/profile", label: "Profile" },
  { href: "/vip", label: "VIP" },
]

const mobileNavLinks = [
  { href: "/", label: "Home", icon: Film },
  { href: "/movies", label: "Movies", icon: Search },
  { href: "/suggestions", label: "Suggestions", icon: Bookmark },
  { href: "/showtimes", label: "Showtimes", icon: Clock },
  { href: "/tickets", label: "Tickets", icon: Ticket },
  { href: "/profile", label: "Profile", icon: User },
  { href: "/vip", label: "VIP", icon: BarChart3 },
]

export function Navbar() {
  const pathname = usePathname()
  const router = useRouter()
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

  const handleLogout = () => {
    logout()
    setMobileOpen(false)
    router.replace("/auth")
  }

  return (
    <>
      <nav className="fixed top-0 left-0 right-0 z-50 border-b border-[#8FD8D2] bg-[#CBEDEA]/75 backdrop-blur-md shadow-sm">
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
                  "rounded-xl px-3 py-2 text-sm font-medium transition-all duration-200",
                  pathname === link.href
                    ? "border border-[#7ACCC5] bg-[#D7F3F0]/80 text-[#2C7A7B]"
                    : "text-gray-600 hover:bg-gray-100 hover:text-gray-900"
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
                className="flex items-center gap-1 rounded-xl border border-[#8FD8D2] bg-[#D7F3F0]/80 px-2 py-1"
              >
                <Search className="h-4 w-4 text-muted-foreground" />
                <input
                  type="text"
                  value={searchText}
                  onChange={(e) => setSearchText(e.target.value)}
                  placeholder="Search users..."
                  className="w-36 rounded-xl bg-[#CBEDEA]/75 text-sm text-slate-900 placeholder:text-[#4B6664] focus:outline-none focus:border-[#7ACCC5] focus:ring-2 focus:ring-[#81D8D0]"
                />
                <button
                  type="submit"
                  className="rounded-xl px-2 py-1 text-xs font-medium text-gray-600 transition-all duration-200 hover:bg-gray-100 hover:text-gray-900"
                >
                  Go
                </button>
              </form>
              {userSearchOpen && (
                <div className="absolute right-0 top-12 w-72 rounded-2xl border border-[#8FD8D2] bg-[#CBEDEA]/75 backdrop-blur-md shadow-md">
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
                              className="flex items-center gap-3 border-b border-[#8FD8D2] px-4 py-3 transition-all duration-200 hover:scale-[1.01] hover:bg-[#D7F3F0]/80"
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
              className="rounded-xl p-2 text-gray-600 transition-all duration-200 hover:bg-gray-100 hover:text-gray-900 md:hidden"
            >
              <Search className="h-5 w-5" />
              <span className="sr-only">Search users</span>
            </button>
            <div className="relative">
              <button 
                onClick={() => setNotificationOpen(!notificationOpen)}
                className="relative rounded-xl p-2 text-gray-600 transition-all duration-200 hover:bg-gray-100 hover:text-gray-900"
              >
                <Bell className="h-5 w-5 text-[#2C7A7B]" />
                {unreadCount > 0 && (
                  <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground">
                    {unreadCount > 9 ? '9+' : unreadCount}
                  </span>
                )}
                <span className="sr-only">Notifications</span>
              </button>
              {notificationOpen && (
                <div className="absolute right-0 top-12 w-80 rounded-2xl border border-[#8FD8D2] bg-[#CBEDEA]/75 backdrop-blur-md shadow-md">
                  <div className="border-b border-[#8FD8D2] px-4 py-3">
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
                      notifications.map((notif) => {
                        const title = (notif.title ?? "").trim() || formatNotificationType(notif.type) || "Notification"
                        const message = (notif.message ?? "").trim() || "Open notifications to view details."
                        const type = (notif.type ?? "").trim()
                        return (
                        <Link
                          key={notif.id}
                          href="/notifications"
                          className={cn(
                            "flex gap-3 border-b border-[#8FD8D2] px-4 py-3 transition-all duration-200 hover:scale-[1.01] hover:bg-[#D7F3F0]/80",
                            !notif.isRead && "bg-[#D7F3F0]/80"
                          )}
                          onClick={() => {
                            void markAsRead(notif.id)
                            setNotificationOpen(false)
                          }}
                        >
                          <div className="flex-1">
                            <p className="text-sm font-medium text-foreground">{title}</p>
                            <p className="text-xs text-muted-foreground">{message}</p>
                            <p className="mt-1 text-[10px] text-muted-foreground/90">
                              {formatNotificationType(type)} · {new Date(notif.createdAt).toLocaleString()}
                            </p>
                          </div>
                          {!notif.isRead && <div className="h-2 w-2 rounded-full bg-primary flex-shrink-0 mt-1" />}
                        </Link>
                      )})
                    ) : (
                      <div className="px-4 py-6 text-center text-sm text-muted-foreground">
                        No notifications
                      </div>
                    )}
                  </div>
                  <div className="border-t border-[#8FD8D2] px-4 py-2">
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
                  className="rounded-xl bg-[#6ECFC7] px-4 py-2 text-sm font-medium text-white transition-all duration-200 hover:bg-[#5BC3BA]"
                >
                  {user?.userName ?? "Profile"}
                </Link>
                <button
                  onClick={handleLogout}
                  className="rounded-xl border border-[#7ACCC5] bg-[#D7F3F0]/80 px-4 py-2 text-sm font-medium text-[#2C7A7B] transition-all duration-200 hover:bg-[#CBEDEA]/75"
                >
                  Logout
                </button>
              </div>
            ) : (
              <Link
                href="/auth"
                className="hidden rounded-xl bg-[#6ECFC7] px-4 py-2 text-sm font-medium text-white transition-all duration-200 hover:bg-[#5BC3BA] md:block"
              >
                {status === "loading" ? "Loading..." : "Sign In"}
              </Link>
            )}
            <button
              className="rounded-xl p-2 text-gray-600 transition-all duration-200 hover:bg-gray-100 hover:text-gray-900 md:hidden"
              onClick={() => setMobileOpen(!mobileOpen)}
            >
              {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
              <span className="sr-only">Toggle menu</span>
            </button>
          </div>
        </div>

        {mobileOpen && (
          <div className="border-t border-[#8FD8D2] bg-[#CBEDEA]/75 backdrop-blur-md md:hidden">
            <div className="flex flex-col px-4 py-3">
              {mobileNavLinks.map((link) => {
                const Icon = link.icon
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    onClick={() => setMobileOpen(false)}
                    className={cn(
                      "flex items-center gap-3 rounded-xl px-3 py-3 text-sm font-medium transition-all duration-200",
                      pathname === link.href
                        ? "border border-[#7ACCC5] bg-[#D7F3F0]/80 text-[#2C7A7B]"
                        : "text-gray-600 hover:bg-gray-100 hover:text-gray-900"
                    )}
                  >
                    <Icon className="h-4 w-4" />
                    {link.label}
                  </Link>
                )
              })}
              <div className="mt-2 border-t border-[#8FD8D2] pt-3">
                <Link
                  href={status === "authenticated" ? "/profile" : "/auth"}
                  onClick={() => setMobileOpen(false)}
                  className="block rounded-xl bg-[#6ECFC7] px-4 py-2.5 text-center text-sm font-medium text-white"
                >
                  {status === "authenticated" ? "Profile" : status === "loading" ? "Loading..." : "Sign In"}
                </Link>
                {status === "authenticated" && (
                  <button
                    onClick={handleLogout}
                    className="mt-2 block w-full rounded-xl border border-[#7ACCC5] bg-[#D7F3F0]/80 px-4 py-2.5 text-center text-sm font-medium text-[#2C7A7B] transition-all duration-200 hover:bg-[#CBEDEA]/75"
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
