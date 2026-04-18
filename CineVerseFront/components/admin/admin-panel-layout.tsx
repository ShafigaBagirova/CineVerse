"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useEffect } from "react"
import {
  BarChart3,
  Calendar,
  Clapperboard,
  Film,
  LayoutDashboard,
  LogOut,
  Ticket,
  Users,
} from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { Button } from "@/components/ui/button"
import { Spinner } from "@/components/ui/spinner"
import { cn } from "@/lib/utils"
import { isAdminUser } from "@/lib/roles"

const links = [
  { href: "/admin/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/admin/movies", label: "Movies", icon: Film },
  { href: "/admin/halls", label: "Halls", icon: Clapperboard },
  { href: "/admin/showtimes", label: "Showtimes", icon: Calendar },
  { href: "/admin/users", label: "Users", icon: Users },
  { href: "/admin/bookings", label: "Bookings", icon: Ticket },
]

export function AdminPanelLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname()
  const router = useRouter()
  const { status, user, logout } = useAuth()

  useEffect(() => {
    if (status === "loading") return
    if (status === "guest") {
      router.replace("/auth")
      return
    }
    if (!isAdminUser(user)) {
      router.replace("/home")
    }
  }, [status, user, router])

  if (status === "loading") {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner className="h-10 w-10 text-primary" />
      </div>
    )
  }

  if (status === "guest" || !isAdminUser(user)) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner className="h-10 w-10 text-primary" />
      </div>
    )
  }

  return (
    <div className="flex min-h-screen w-full bg-background">
      <aside className="sticky top-0 hidden h-screen w-56 shrink-0 border-r border-border/60 bg-card/30 px-3 py-6 md:block">
        <div className="mb-6 flex items-center gap-2 px-2">
          <BarChart3 className="h-5 w-5 text-primary" />
          <span className="font-semibold tracking-tight">Admin</span>
        </div>
        <nav className="flex flex-col gap-1">
          {links.map(({ href, label, icon: Icon }) => {
            const active = pathname === href || (href !== "/admin/dashboard" && pathname.startsWith(href))
            return (
              <Link
                key={href}
                href={href}
                className={cn(
                  "flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  active ? "bg-primary/15 text-primary" : "text-muted-foreground hover:bg-muted/50 hover:text-foreground",
                )}
              >
                <Icon className="h-4 w-4 shrink-0" />
                {label}
              </Link>
            )
          })}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex flex-wrap items-center justify-between gap-3 border-b border-border/60 bg-background/90 px-4 py-3 backdrop-blur md:px-8">
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">CineVerse</p>
            <p className="text-sm font-semibold text-foreground">
              {links.find((l) => pathname === l.href || pathname.startsWith(l.href + "/"))?.label ?? "Admin"}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <span className="hidden text-sm text-muted-foreground sm:inline">{user?.userName}</span>
            <Button variant="outline" size="sm" asChild>
              <Link href="/">Site</Link>
            </Button>
            <Button
              variant="ghost"
              size="sm"
              className="gap-1"
              onClick={() => {
                logout()
                router.push("/auth")
              }}
            >
              <LogOut className="h-4 w-4" />
              Out
            </Button>
          </div>
        </header>

        <div className="flex flex-1 flex-col overflow-x-auto px-4 py-6 pb-24 md:px-8 md:pb-6">{children}</div>
      </div>

      <nav className="fixed bottom-0 left-0 right-0 z-40 flex border-t border-border/60 bg-background/95 px-2 py-2 backdrop-blur md:hidden">
        {links.map(({ href, label, icon: Icon }) => {
          const active = pathname === href || pathname.startsWith(href + "/")
          return (
            <Link
              key={href}
              href={href}
              className={cn(
                "flex flex-1 flex-col items-center gap-0.5 py-1 text-[10px] font-medium",
                active ? "text-primary" : "text-muted-foreground",
              )}
            >
              <Icon className="h-4 w-4" />
              {label}
            </Link>
          )
        })}
      </nav>
    </div>
  )
}
