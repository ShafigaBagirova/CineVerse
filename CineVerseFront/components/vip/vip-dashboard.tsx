"use client"

import { useEffect, useMemo, useRef, useState } from "react"
import { Crown, Film, Heart, Users, TrendingUp, Star, Sparkles } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { isVipUser } from "@/lib/roles"
import { cn } from "@/lib/utils"
import { ApiError } from "@/lib/api/types"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import {
  followInsightsTastePercent,
  getFollowInsights,
  getSuggestedUsers,
  suggestedUserTasteSimilarityPercent,
  type FollowInsightsDto,
  type SuggestedUserItemDto,
} from "@/lib/api/follow"

function initialsFromName(fullName: string | null | undefined, userName: string) {
  const t = (fullName ?? "").trim()
  if (t.length >= 2) return t.slice(0, 2).toUpperCase()
  return userName.slice(0, 2).toUpperCase() || "?"
}

export function VipDashboard() {
  const { status, user } = useAuth()
  const isVip = isVipUser(user)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [authRequired, setAuthRequired] = useState(false)
  const [vipRequired, setVipRequired] = useState(false)
  const [insights, setInsights] = useState<FollowInsightsDto | null>(null)
  const [suggested, setSuggested] = useState<SuggestedUserItemDto[]>([])
  /** Per–suggested-user follow insights (loaded once with suggestions; no refetch on selection). */
  const [insightsBySuggestedId, setInsightsBySuggestedId] = useState<Record<string, FollowInsightsDto>>({})

  const [selectedUserId, setSelectedUserId] = useState<string | null>(null)
  const detailSectionRef = useRef<HTMLElement | null>(null)

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
        const [insightsData, suggestedData] = await Promise.all([
          getFollowInsights(user.userId),
          getSuggestedUsers(1, 20),
        ])
        setInsights(insightsData)
        setSuggested(suggestedData.items)

        const items = suggestedData.items
        if (items.length > 0) {
          const pairs = await Promise.all(
            items.map(async (s) => {
              try {
                const d = await getFollowInsights(s.userId)
                return [s.userId, d] as const
              } catch {
                return [s.userId, null] as const
              }
            })
          )
          const next: Record<string, FollowInsightsDto> = {}
          for (const [id, dto] of pairs) {
            if (dto) next[id] = dto
          }
          setInsightsBySuggestedId(next)
        } else {
          setInsightsBySuggestedId({})
        }
      } catch (err) {
        setInsights(null)
        setSuggested([])
        setInsightsBySuggestedId({})
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

  const sortedSuggestions = useMemo(
    () => [...suggested].sort((a, b) => b.tasteScore - a.tasteScore || b.commonMoviesCount - a.commonMoviesCount),
    [suggested]
  )

  useEffect(() => {
    if (sortedSuggestions.length === 0) {
      setSelectedUserId(null)
      return
    }
    setSelectedUserId((prev) => {
      if (prev && sortedSuggestions.some((s) => s.userId === prev)) return prev
      return sortedSuggestions[0].userId
    })
  }, [sortedSuggestions])

  const selectedItem = useMemo(
    () => sortedSuggestions.find((s) => s.userId === selectedUserId) ?? null,
    [sortedSuggestions, selectedUserId]
  )

  const selectedPairInsights = selectedUserId ? insightsBySuggestedId[selectedUserId] : undefined
  const displayInsights = selectedPairInsights ?? insights

  function selectSuggestedUser(userId: string) {
    setSelectedUserId(userId)
    requestAnimationFrame(() => {
      detailSectionRef.current?.scrollIntoView({ behavior: "smooth", block: "start" })
    })
  }

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
            value: `${followInsightsTastePercent(insights?.tasteSimilarityScore)}%`,
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
            value: String(displayInsights?.mutualFollowingsCount ?? 0),
            sub: "shared following network",
          },
          {
            icon: TrendingUp,
            label: "Mutual Followers",
            value: String(displayInsights?.mutualFollowersCount ?? 0),
            sub: "shared follower network",
          },
        ].map((stat) => {
          const Icon = stat.icon
          return (
            <div key={stat.label} className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <div className="mb-3 flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
                <Icon className="h-4.5 w-4.5 text-primary" />
              </div>
              <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
              <p className="text-sm font-medium text-gray-900">{stat.label}</p>
              <p className="text-xs text-gray-500">{stat.sub}</p>
            </div>
          )
        })}
      </div>

      {/* Suggestions list — horizontal scroll */}
      <div className="mb-6">
        <div className="mb-4 flex flex-col gap-1 text-center sm:text-left">
          <h2 className="font-serif text-xl font-bold text-foreground md:text-2xl">Suggested similar users</h2>
          <p className="text-sm text-muted-foreground">
            Tap a profile to see how you compare — details appear below.
          </p>
          {sortedSuggestions.length > 0 && (
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground sm:text-center">
              {sortedSuggestions.length} suggestion{sortedSuggestions.length === 1 ? "" : "s"} — swipe on small screens
            </p>
          )}
        </div>

        {sortedSuggestions.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-gray-200 bg-white px-6 py-12 text-center shadow-sm">
            <Sparkles className="mx-auto mb-3 h-8 w-8 text-muted-foreground/50" />
            <p className="text-sm text-muted-foreground">No suggested users yet — add ratings or mark films as watched to find matches.</p>
          </div>
        ) : (
          <div className="relative -mx-4 sm:mx-0">
            <div className="flex snap-x snap-mandatory gap-3 overflow-x-auto px-4 pb-3 pt-1 sm:grid sm:grid-cols-2 sm:overflow-visible sm:px-0 sm:pb-0 lg:grid-cols-3">
              {sortedSuggestions.map((item) => {
                const selected = item.userId === selectedUserId
                const pct = suggestedUserTasteSimilarityPercent(item)
                return (
                  <button
                    key={item.userId}
                    type="button"
                    aria-pressed={selected}
                    onClick={() => selectSuggestedUser(item.userId)}
                    className={cn(
                      "flex w-[min(100%,280px)] shrink-0 snap-start flex-col rounded-2xl border p-4 text-left transition-all sm:w-auto sm:min-w-0 sm:snap-none",
                      "hover:border-primary/40 hover:bg-secondary/30 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/50",
                      selected
                        ? "border-primary/60 bg-gradient-to-br from-primary/10 via-card to-card shadow-md ring-2 ring-primary/30"
                        : "border-gray-200 bg-white shadow-sm"
                    )}
                  >
                    <div className="flex items-start gap-3">
                      <Avatar className="h-11 w-11 shrink-0 border border-border/40">
                        {item.avatarUrl ? (
                          <AvatarImage src={item.avatarUrl} alt="" className="object-cover" />
                        ) : null}
                        <AvatarFallback className="bg-primary/15 text-sm font-semibold text-primary">
                          {initialsFromName(item.fullName, item.userName)}
                        </AvatarFallback>
                      </Avatar>
                      <div className="min-w-0 flex-1">
                        <p className="truncate font-semibold text-foreground">{item.fullName || item.userName}</p>
                        <p className="truncate text-xs text-muted-foreground">@{item.userName}</p>
                      </div>
                    </div>
                    <div className="mt-3 flex flex-wrap gap-x-3 gap-y-1 border-t border-border/30 pt-3 text-xs text-muted-foreground">
                      <span className="inline-flex items-center gap-1 font-medium text-foreground">
                        <Star className="h-3.5 w-3.5 shrink-0 text-primary" />
                        {pct}% taste
                      </span>
                      <span>
                        <span className="font-medium text-foreground">{item.commonMoviesCount}</span> common
                      </span>
                    </div>
                  </button>
                )
              })}
            </div>
          </div>
        )}
      </div>

      {/* Inline detail — full width below list */}
      {selectedItem && (
        <section
          ref={detailSectionRef}
          tabIndex={-1}
          className="mb-10 scroll-mt-24 rounded-2xl border border-gray-200 bg-white shadow-sm outline-none"
          aria-labelledby="vip-compare-heading"
        >
          <div className="border-b border-gray-200 bg-white px-5 py-5 text-center sm:px-8 sm:text-left">
            <p className="text-xs font-medium uppercase tracking-wider text-primary">Comparison</p>
            <h3 id="vip-compare-heading" className="mt-1 font-serif text-xl font-bold text-gray-900">
              Comparing you with {selectedItem.fullName || selectedItem.userName}
            </h3>
            <p className="text-sm text-gray-600">@{selectedItem.userName}</p>
          </div>

          <div className="grid gap-4 p-5 sm:grid-cols-2 sm:p-8 lg:grid-cols-4">
            <div className="rounded-xl border border-gray-200 bg-white px-4 py-4 text-center shadow-sm sm:text-left">
              <p className="text-[10px] font-medium uppercase tracking-wider text-gray-500">Similarity</p>
              <p className="mt-1 text-3xl font-bold text-primary">{suggestedUserTasteSimilarityPercent(selectedItem)}%</p>
              <p className="text-xs text-gray-600">Movie taste overlap</p>
            </div>
            <div className="rounded-xl border border-gray-200 bg-white px-4 py-4 text-center shadow-sm sm:text-left">
              <p className="text-[10px] font-medium uppercase tracking-wider text-gray-500">Common movies</p>
              <p className="mt-1 text-3xl font-bold text-gray-900">{selectedItem.commonMoviesCount}</p>
              <p className="text-xs text-gray-600">Shared titles</p>
            </div>
            <div className="rounded-xl border border-gray-200 bg-white px-4 py-4 text-center shadow-sm sm:text-left">
              <p className="text-[10px] font-medium uppercase tracking-wider text-gray-500">Mutual followings</p>
              <p className="mt-1 text-3xl font-bold text-gray-900">
                {selectedPairInsights != null ? selectedPairInsights.mutualFollowingsCount : "—"}
              </p>
              <p className="text-xs text-gray-600">Accounts you both follow</p>
            </div>
            <div className="rounded-xl border border-gray-200 bg-white px-4 py-4 text-center shadow-sm sm:text-left">
              <p className="text-[10px] font-medium uppercase tracking-wider text-gray-500">Mutual followers</p>
              <p className="mt-1 text-3xl font-bold text-gray-900">
                {selectedPairInsights != null ? selectedPairInsights.mutualFollowersCount : "—"}
              </p>
              <p className="text-xs text-gray-600">Followers in common</p>
            </div>
          </div>

          {selectedPairInsights && (
            <div className="border-t border-gray-200 px-5 pb-6 pt-2 sm:px-8">
              <p className="text-center text-xs text-gray-600 sm:text-left">
                Network similarity score:{" "}
                <span className="font-semibold text-gray-900">
                  {followInsightsTastePercent(selectedPairInsights.tasteSimilarityScore)}%
                </span>{" "}
                (from mutual following overlap)
              </p>
            </div>
          )}
        </section>
      )}

    </div>
  )
}
