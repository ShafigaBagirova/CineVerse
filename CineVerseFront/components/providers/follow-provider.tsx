"use client"

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { useAuth } from "@/components/providers/auth-provider"
import { ApiError } from "@/lib/api/types"
import {
  followUser as followUserRequest,
  getFollowStatus,
  getSuggestedUsers,
  unfollowUser as unfollowUserRequest,
  type SuggestedUserItemDto,
} from "@/lib/api/follow"

interface FollowContextValue {
  followingByUserId: Record<string, boolean>
  loadingByUserId: Record<string, boolean>
  suggestedUsers: SuggestedUserItemDto[]
  suggestedLoading: boolean
  suggestedError: string | null
  suggestedAuthRequired: boolean
  suggestedVipRequired: boolean
  ensureFollowStatus: (userId: string) => Promise<void>
  followUser: (userId: string) => Promise<void>
  unfollowUser: (userId: string) => Promise<void>
  refreshSuggestedUsers: () => Promise<void>
}

const FollowContext = createContext<FollowContextValue | undefined>(undefined)

export function FollowProvider({ children }: { children: React.ReactNode }) {
  const { status, user } = useAuth()
  const [followingByUserId, setFollowingByUserId] = useState<Record<string, boolean>>({})
  const [loadingByUserId, setLoadingByUserId] = useState<Record<string, boolean>>({})
  const [suggestedUsers, setSuggestedUsers] = useState<SuggestedUserItemDto[]>([])
  const [suggestedLoading, setSuggestedLoading] = useState(false)
  const [suggestedError, setSuggestedError] = useState<string | null>(null)
  const [suggestedAuthRequired, setSuggestedAuthRequired] = useState(false)
  const [suggestedVipRequired, setSuggestedVipRequired] = useState(false)

  const ensureFollowStatus = useCallback(
    async (userId: string) => {
      if (!userId || status !== "authenticated" || user?.userId === userId || followingByUserId[userId] !== undefined) return

      try {
        setLoadingByUserId((prev) => ({ ...prev, [userId]: true }))
        const result = await getFollowStatus(userId)
        setFollowingByUserId((prev) => ({ ...prev, [userId]: result.isFollowing }))
      } catch {
        setFollowingByUserId((prev) => ({ ...prev, [userId]: false }))
      } finally {
        setLoadingByUserId((prev) => ({ ...prev, [userId]: false }))
      }
    },
    [followingByUserId, status, user?.userId]
  )

  const followUser = useCallback(
    async (userId: string) => {
      if (!userId || status !== "authenticated" || user?.userId === userId) return
      if (followingByUserId[userId]) return

      setLoadingByUserId((prev) => ({ ...prev, [userId]: true }))
      try {
        await followUserRequest(userId)
        setFollowingByUserId((prev) => ({ ...prev, [userId]: true }))
      } catch {
        // Preserve previous state when backend update fails.
      } finally {
        setLoadingByUserId((prev) => ({ ...prev, [userId]: false }))
      }
    },
    [followingByUserId, status, user?.userId]
  )

  const unfollowUser = useCallback(
    async (userId: string) => {
      if (!userId || status !== "authenticated" || user?.userId === userId) return
      if (!followingByUserId[userId]) return

      setLoadingByUserId((prev) => ({ ...prev, [userId]: true }))
      try {
        await unfollowUserRequest(userId)
        setFollowingByUserId((prev) => ({ ...prev, [userId]: false }))
      } catch {
        // Preserve previous state when backend update fails.
      } finally {
        setLoadingByUserId((prev) => ({ ...prev, [userId]: false }))
      }
    },
    [followingByUserId, status, user?.userId]
  )

  const refreshSuggestedUsers = useCallback(async () => {
    if (status !== "authenticated") {
      setSuggestedUsers([])
      setSuggestedError(null)
      setSuggestedAuthRequired(true)
      setSuggestedVipRequired(false)
      setSuggestedLoading(false)
      return
    }

    setSuggestedLoading(true)
    setSuggestedError(null)
    setSuggestedAuthRequired(false)
    setSuggestedVipRequired(false)

    try {
      const response = await getSuggestedUsers(1, 12)
      const raw = Array.isArray(response?.items) ? response.items : []
      setSuggestedUsers(raw.filter((item) => item.userId !== user?.userId))
    } catch (err) {
      setSuggestedUsers([])
      setSuggestedError(null)
      if (err instanceof ApiError) {
        if (err.status === 401) {
          setSuggestedAuthRequired(true)
          setSuggestedVipRequired(false)
        } else if (err.status === 403) {
          setSuggestedAuthRequired(false)
          setSuggestedVipRequired(true)
        } else {
          setSuggestedAuthRequired(false)
          setSuggestedVipRequired(false)
        }
      } else {
        setSuggestedAuthRequired(false)
        setSuggestedVipRequired(false)
      }
    } finally {
      setSuggestedLoading(false)
    }
  }, [status, user?.userId])

  useEffect(() => {
    void refreshSuggestedUsers()
  }, [refreshSuggestedUsers])

  const value = useMemo<FollowContextValue>(
    () => ({
      followingByUserId,
      loadingByUserId,
      suggestedUsers,
      suggestedLoading,
      suggestedError,
      suggestedAuthRequired,
      suggestedVipRequired,
      ensureFollowStatus,
      followUser,
      unfollowUser,
      refreshSuggestedUsers,
    }),
    [
      ensureFollowStatus,
      followUser,
      followingByUserId,
      loadingByUserId,
      refreshSuggestedUsers,
      suggestedAuthRequired,
      suggestedVipRequired,
      suggestedError,
      suggestedLoading,
      suggestedUsers,
      unfollowUser,
    ]
  )

  return <FollowContext.Provider value={value}>{children}</FollowContext.Provider>
}

export function useFollow() {
  const context = useContext(FollowContext)
  if (!context) {
    throw new Error("useFollow must be used inside FollowProvider")
  }
  return context
}
