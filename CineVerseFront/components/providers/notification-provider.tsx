"use client"

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { useAuth } from "@/components/providers/auth-provider"
import { ApiError } from "@/lib/api/types"
import {
  getMyNotifications,
  getUnreadNotificationCount,
  markNotificationAsRead,
  type GetMyNotificationsResponse,
} from "@/lib/api/notifications"

interface NotificationContextValue {
  notifications: GetMyNotificationsResponse[]
  unreadCount: number
  loading: boolean
  error: string | null
  authRequired: boolean
  refreshNotifications: (pageSize?: number) => Promise<void>
  markAsRead: (id: number) => Promise<void>
}

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined)

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const { status } = useAuth()
  const [notifications, setNotifications] = useState<GetMyNotificationsResponse[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [authRequired, setAuthRequired] = useState(false)

  const refreshNotifications = useCallback(
    async (pageSize = 50) => {
      if (status !== "authenticated") {
        setNotifications([])
        setUnreadCount(0)
        setAuthRequired(true)
        setError(null)
        setLoading(false)
        return
      }

      try {
        setLoading(true)
        setError(null)
        setAuthRequired(false)
        const [list, count] = await Promise.all([getMyNotifications(1, pageSize), getUnreadNotificationCount()])
        setNotifications(list.items)
        setUnreadCount(count.unreadCount)
      } catch (err) {
        setNotifications([])
        setUnreadCount(0)
        if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
          setAuthRequired(true)
          setError("Sign in to view your notifications.")
        } else {
          setAuthRequired(false)
          setError(err instanceof Error ? err.message : "Failed to load notifications.")
        }
      } finally {
        setLoading(false)
      }
    },
    [status]
  )

  const markAsRead = useCallback(async (id: number) => {
    const target = notifications.find((item) => item.id === id)
    if (!target || target.isRead) return
    try {
      await markNotificationAsRead(id)
      setNotifications((prev) => prev.map((item) => (item.id === id ? { ...item, isRead: true } : item)))
      setUnreadCount((prev) => Math.max(0, prev - 1))
    } catch {
      // Keep existing state when backend update fails.
    }
  }, [notifications])

  useEffect(() => {
    void refreshNotifications()
  }, [refreshNotifications])

  const value = useMemo<NotificationContextValue>(
    () => ({
      notifications,
      unreadCount,
      loading,
      error,
      authRequired,
      refreshNotifications,
      markAsRead,
    }),
    [authRequired, error, loading, markAsRead, notifications, refreshNotifications, unreadCount]
  )

  return <NotificationContext.Provider value={value}>{children}</NotificationContext.Provider>
}

export function useNotifications() {
  const context = useContext(NotificationContext)
  if (!context) {
    throw new Error("useNotifications must be used inside NotificationProvider")
  }
  return context
}
