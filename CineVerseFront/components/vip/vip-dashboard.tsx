"use client"

import { useEffect, useMemo, useState } from "react"
import { Crown, Film, Heart, Users, TrendingUp, Star } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { ApiError } from "@/lib/api/types"
import {
  getFollowInsights,
  getMutualFollowings,
  getSuggestedUsers,
  type FollowInsightsDto,
  type FollowUserItemDto,
  type SuggestedUserItemDto,
} from "@/lib/api/follow"

export function VipDashboard() {
  const { status, user } = useAuth()
  const isVip = !!user?.roles?.some((role) => role.toLowerCase() === "vip")
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [authRequired, setAuthRequired] = useState(false)
  const [vipRequired, setVipRequired] = useState(false)
  const [insights, setInsights] = useState<FollowInsightsDto | null>(null)
  const [suggested, setSuggested] = useState<SuggestedUserItemDto[]>([])
  const [mutuals, setMutuals] = useState<FollowUserItemDto[]>([])

  useEffect(() => {
    const load = async () => {
      if (status !== "authenticated" || !user?.userId) {
        setLoading(false)
        setAuthRequired(true)
        setVipRequired(false)
        setError(null)
        return
      }
      if (!isVip) {
        setLoading(false)
        setAuthRequired(false)
        setVipRequired(true)
        setError(null)
        return
      }

      try {
        setLoading(true)
        setError(null)
        setAuthRequired(false)
        setVipRequired(false)
        const [insightsData, suggestedData, mutualData] = await Promise.all([
          getFollowInsights(user.userId),
          getSuggestedUsers(1, 20),
          getMutualFollowings(user.userId, 1, 20),
        ])
        setInsights(insightsData)
        setSuggested(suggestedData.items)
        setMutuals(mutualData.items)
      } catch (err) {
        setInsights(null)
        setSuggested([])
        setMutuals([])
        if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
          setVipRequired(true)
        } else {
          setError(err instanceof Error ? err.message : "Failed to load VIP analytics.")
        }
      } finally {
        setLoading(false)
      }
    }
    void load()
  }, [isVip, status, user?.userId])

  const topSuggestions = useMemo(
    () => [...suggested].sort((a, b) => b.tasteScore - a.tasteScore).slice(0, 6),
    [suggested]
  )

  if (loading) {
    return <div className="mx-auto max-w-6xl px-4 py-10 lg:px-8 text-sm text-muted-foreground">Loading VIP analytics...</div>
  }

  if (authRequired) {
    return <div className="mx-auto max-w-6xl px-4 py-10 lg:px-8 text-sm text-muted-foreground">Sign in to view VIP analytics.</div>
  }

  if (vipRequired) {
    return <div className="mx-auto max-w-6xl px-4 py-10 lg:px-8 text-sm text-muted-foreground">VIP membership is required for this page.</div>
  }

  if (error) {
    return <div className="mx-auto max-w-6xl px-4 py-10 lg:px-8 text-sm text-muted-foreground">{error}</div>
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 lg:px-8">
      {/* Header */}
      <div className="mb-8 flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
          <Crown className="h-5 w-5 text-primary" />
        </div>
        <div>
          <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">VIP Analytics</p>
          <h1 className="font-serif text-3xl font-bold text-foreground">Taste Profile</h1>
        </div>
      </div>

      {/* Stats Row */}
      <div className="mb-8 grid grid-cols-2 gap-4 md:grid-cols-4">
        {[
          {
            icon: Users,
            label: "Taste Similarity",
            value: `${Math.round((insights?.tasteSimilarityScore ?? 0) * 100)}%`,
            sub: "from follow insights",
          },
          {
            icon: Film,
            label: "Suggested Users",
            value: String(insights?.suggestedUsersCount ?? 0),
            sub: "VIP suggestions available",
          },
          {
            icon: Heart,
            label: "Mutual Followings",
            value: String(insights?.mutualFollowingsCount ?? 0),
            sub: "shared following network",
          },
          {
            icon: TrendingUp,
            label: "Mutual Followers",
            value: String(insights?.mutualFollowersCount ?? 0),
            sub: "shared follower network",
          },
        ].map((stat) => {
          const Icon = stat.icon
          return (
            <div key={stat.label} className="rounded-2xl border border-border/50 bg-card p-5">
              <div className="mb-3 flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
                <Icon className="h-4.5 w-4.5 text-primary" />
              </div>
              <p className="text-2xl font-bold text-foreground">{stat.value}</p>
              <p className="text-sm font-medium text-foreground">{stat.label}</p>
              <p className="text-xs text-muted-foreground">{stat.sub}</p>
            </div>
          )
        })}
      </div>

      <div className="mb-8 rounded-2xl border border-border/50 bg-card p-6">
        <h2 className="mb-4 font-serif text-lg font-bold text-foreground">Suggested Similar Users</h2>
        {topSuggestions.length === 0 ? (
          <p className="text-sm text-muted-foreground">No suggested users available.</p>
        ) : (
          <div className="grid gap-3 md:grid-cols-2">
            {topSuggestions.map((item) => (
              <div key={item.userId} className="rounded-xl border border-border/40 bg-secondary/20 p-4">
                <p className="text-sm font-semibold text-foreground">{item.fullName || item.userName}</p>
                <p className="text-xs text-muted-foreground">@{item.userName}</p>
                <div className="mt-2 flex gap-3 text-xs text-muted-foreground">
                  <span className="inline-flex items-center gap-1"><Star className="h-3 w-3 text-primary" />Taste {Math.round(item.tasteScore)}%</span>
                  <span>Common Movies: {item.commonMoviesCount}</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="rounded-2xl border border-border/50 bg-card p-6">
        <h2 className="mb-4 font-serif text-lg font-bold text-foreground">Mutual Following Network</h2>
        {mutuals.length === 0 ? (
          <p className="text-sm text-muted-foreground">No mutual followings found.</p>
        ) : (
          <div className="grid gap-3 md:grid-cols-2">
            {mutuals.map((item) => (
              <div key={item.userId} className="rounded-xl border border-border/40 bg-secondary/20 p-4">
                <p className="text-sm font-semibold text-foreground">{item.fullName || item.userName}</p>
                <p className="text-xs text-muted-foreground">@{item.userName}</p>
              </div>
            ))}
          </div>
        )}
        <div className="mt-6 rounded-lg border border-border/40 bg-secondary/20 p-4">
          <p className="text-xs text-muted-foreground">
            Genre-level match, common watched titles, and rating-difference analytics are not exposed by current backend VIP DTOs.
            This page renders all available backend-supported VIP analytics fields.
          </p>
        </div>
      </div>
    </div>
  )
}
