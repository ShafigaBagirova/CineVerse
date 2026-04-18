"use client"

import { useEffect } from "react"
import { Bell } from "lucide-react"
import { cn } from "@/lib/utils"
import { useNotifications } from "@/components/providers/notification-provider"

export default function NotificationsPage() {
  const { notifications, unreadCount, loading, error, authRequired, refreshNotifications, markAsRead } = useNotifications()

  useEffect(() => {
    void refreshNotifications(50)
  }, [refreshNotifications])

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 lg:px-8">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold text-foreground">Notifications</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {unreadCount > 0 ? `${unreadCount} unread` : "All caught up"}
          </p>
        </div>
        <button
          onClick={() => void refreshNotifications(50)}
          className="rounded-lg border border-border/40 px-3 py-2 text-xs font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
        >
          Refresh
        </button>
      </div>

      <div className="rounded-2xl border border-border/50 bg-card">
        {loading ? (
          <div className="px-5 py-8 text-center text-sm text-muted-foreground">Loading notifications...</div>
        ) : authRequired ? (
          <div className="px-5 py-8 text-center text-sm text-muted-foreground">Sign in to view notifications.</div>
        ) : error ? (
          <div className="px-5 py-8 text-center text-sm text-muted-foreground">{error}</div>
        ) : notifications.length === 0 ? (
          <div className="px-5 py-8 text-center text-sm text-muted-foreground">No notifications yet.</div>
        ) : (
          <div className="divide-y divide-border/30">
            {notifications.map((item) => (
              <button
                key={item.id}
                onClick={() => void markAsRead(item.id)}
                className={cn(
                  "w-full px-5 py-4 text-left transition-colors hover:bg-surface",
                  !item.isRead && "bg-surface-hover/60"
                )}
              >
                <div className="flex items-start gap-3">
                  <div className="mt-0.5 rounded-full bg-primary/10 p-1.5">
                    <Bell className="h-3.5 w-3.5 text-primary" />
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between gap-3">
                      <p className="text-sm font-semibold text-foreground">{item.title}</p>
                      {!item.isRead && <span className="h-2 w-2 rounded-full bg-primary" />}
                    </div>
                    <p className="mt-1 text-sm text-muted-foreground">{item.message}</p>
                    <div className="mt-2 flex items-center gap-2 text-xs text-muted-foreground">
                      <span>{item.type}</span>
                      <span>-</span>
                      <span>{new Date(item.createdAt).toLocaleString()}</span>
                    </div>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
