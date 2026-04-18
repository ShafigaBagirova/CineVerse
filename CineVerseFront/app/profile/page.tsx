"use client"

import { useCallback, useEffect, useMemo, useState } from "react"
import Image from "next/image"
import Link from "next/link"
import { Star, Film, Heart, Crown, Bookmark, Eye, Users, UserCheck, UserPlus } from "lucide-react"
import { cn } from "@/lib/utils"
import { useAuth } from "@/components/providers/auth-provider"
import {
  getUserProfile,
  getUserRatings,
  getUserReviews,
  type UserPublicProfileDto,
} from "@/lib/api/user"
import {
  getMyWatchedMovies,
  getMyWatchlist,
  removeFromWatched,
  removeFromWatchlist,
  type WatchedMovieDto,
  type WatchlistMovieDto,
} from "@/lib/api/watch"
import { resolveMoviePosterUrl } from "@/lib/movie-poster"
import {
  getFollowers,
  getFollowings,
  getFollowInsights,
  getFollowStats,
  type FollowUserItemDto,
} from "@/lib/api/follow"
import { useFollow } from "@/components/providers/follow-provider"

const allTabs = ["Watchlist", "Watched", "Reviews", "Following"] as const
const publicTabs = ["Reviews", "Following"] as const

const userProfile = {
  name: "CineVerse User",
  bio: "Film enthusiast from Baku. Lover of sci-fi, thrillers, and anything by Nolan and Villeneuve.",
  avatar: "AM",
  isVip: true,
  stats: {
    watched: 247,
    avgRating: 7.8,
    favoriteGenres: ["Sci-Fi", "Thriller", "Drama"],
  },
  tasteCompatibility: 87,
  followers: 0,
  following: 0,
}

export type ProfilePageClientProps = { routeUserId?: string }

export function ProfilePageClient({ routeUserId }: ProfilePageClientProps) {
  const { user, status } = useAuth()
  const profileUserId = routeUserId ?? user?.userId ?? null
  const isOwnProfile = !!(user?.userId && profileUserId && user.userId === profileUserId)
  const [profileInfo, setProfileInfo] = useState<UserPublicProfileDto | null | undefined>(undefined)
  const [avgRatingStat, setAvgRatingStat] = useState<number | null>(null)
  const [activeTab, setActiveTab] = useState<string>("Watchlist")
  const tabs = isOwnProfile ? allTabs : publicTabs
  const [watchlistMovies, setWatchlistMovies] = useState<WatchlistMovieDto[]>([])
  const [watchedMovies, setWatchedMovies] = useState<WatchedMovieDto[]>([])
  const [watchLoading, setWatchLoading] = useState(false)
  const [watchError, setWatchError] = useState<string | null>(null)
  const [reviewsLoading, setReviewsLoading] = useState(false)
  const [reviewsError, setReviewsError] = useState<string | null>(null)
  const [userReviews, setUserReviews] = useState<
    Array<{
      id: number
      movieId: number
      movie: string
      posterPath: string | null
      rating: number | null
      text: string
      date: string
    }>
  >([])
  const [followStats, setFollowStats] = useState({ followersCount: 0, followingsCount: 0 })
  const [followListMode, setFollowListMode] = useState<"following" | "followers">("following")
  const [followItems, setFollowItems] = useState<FollowUserItemDto[]>([])
  const [followSearch, setFollowSearch] = useState("")
  const [followLoading, setFollowLoading] = useState(false)
  const [followError, setFollowError] = useState<string | null>(null)
  const [tasteCompatibility, setTasteCompatibility] = useState<number | null>(null)
  const { followingByUserId, loadingByUserId, ensureFollowStatus, toggleFollow } = useFollow()
  const isVip = !!user?.roles?.some((role) => role.toLowerCase() === "vip")

  useEffect(() => {
    if (routeUserId) setActiveTab("Reviews")
  }, [routeUserId])

  useEffect(() => {
    if (!profileUserId) {
      setProfileInfo(undefined)
      return
    }
    let cancelled = false
    setProfileInfo(undefined)
    void (async () => {
      const p = await getUserProfile(profileUserId)
      if (cancelled) return
      if (p) {
        setProfileInfo(p)
        return
      }
      if (!routeUserId && user?.userId === profileUserId) {
        setProfileInfo({
          id: user.userId,
          userId: user.userId,
          userName: user.userName ?? "",
          fullName: null,
          avatarUrl: null,
        })
        return
      }
      setProfileInfo(null)
    })()
    return () => {
      cancelled = true
    }
  }, [profileUserId, routeUserId, user?.userId, user?.userName])

  useEffect(() => {
    const load = async () => {
      if (!profileUserId) return
      try {
        setReviewsLoading(true)
        setReviewsError(null)
        const [reviewsResponse, ratingsResponse] = await Promise.all([
          getUserReviews(profileUserId, 1, 20),
          getUserRatings(profileUserId, 1, 50),
        ])

        const ratings = ratingsResponse.items
        if (ratings.length > 0) {
          const sum = ratings.reduce((acc, r) => acc + r.rating, 0)
          setAvgRatingStat(sum / ratings.length)
        } else {
          setAvgRatingStat(null)
        }

        const ratingsByMovie = new Map<number, number>()
        ratings.forEach((item) => ratingsByMovie.set(item.movieId, item.rating))

        const mapped = reviewsResponse.items.map((review) => {
          const movieId = review.movieId
          const movie =
            review.movieTitle?.trim() || (movieId ? `Movie #${movieId}` : "Movie")
          return {
            id: review.id,
            movieId,
            movie,
            posterPath: review.posterPath ?? null,
            rating: movieId ? ratingsByMovie.get(movieId) ?? null : null,
            text: review.content,
            date: new Date(review.createdAt).toLocaleDateString(),
          }
        })

        setUserReviews(mapped)
      } catch (err) {
        setUserReviews([])
        setAvgRatingStat(null)
        setReviewsError(err instanceof Error ? err.message : "Failed to load user reviews.")
      } finally {
        setReviewsLoading(false)
      }
    }
    void load()
  }, [profileUserId])

  const loadWatchData = async () => {
    if (!user || !isOwnProfile) return
    try {
      setWatchLoading(true)
      setWatchError(null)
      const [watchlist, watched] = await Promise.all([
        getMyWatchlist(1, 50),
        getMyWatchedMovies(1, 50),
      ])
      setWatchlistMovies(watchlist.items)
      setWatchedMovies(watched.items)
    } catch (err) {
      setWatchlistMovies([])
      setWatchedMovies([])
      setWatchError(err instanceof Error ? err.message : "Failed to load watchlist/watched data.")
    } finally {
      setWatchLoading(false)
    }
  }

  useEffect(() => {
    void loadWatchData()
  }, [user?.userId, isOwnProfile])

  useEffect(() => {
    const loadFollowStats = async () => {
      if (!profileUserId) return
      try {
        const stats = await getFollowStats(profileUserId)
        setFollowStats(stats)
      } catch {
        setFollowStats({ followersCount: 0, followingsCount: 0 })
      }
    }
    void loadFollowStats()
  }, [profileUserId])

  useEffect(() => {
    if (!profileUserId || status !== "authenticated" || isOwnProfile) return
    void ensureFollowStatus(profileUserId)
  }, [profileUserId, status, isOwnProfile, ensureFollowStatus])

  const handleProfileFollow = useCallback(async () => {
    if (!profileUserId) return
    await toggleFollow(profileUserId)
    try {
      const stats = await getFollowStats(profileUserId)
      setFollowStats(stats)
    } catch {
      /* ignore */
    }
  }, [profileUserId, toggleFollow])

  useEffect(() => {
    const loadFollowList = async () => {
      if (!profileUserId) return
      try {
        setFollowLoading(true)
        setFollowError(null)
        const response =
          followListMode === "following"
            ? await getFollowings(profileUserId, 1, 50)
            : await getFollowers(profileUserId, 1, 50)
        setFollowItems(response.items)
      } catch (err) {
        setFollowItems([])
        setFollowError(err instanceof Error ? err.message : "Failed to load follow users.")
      } finally {
        setFollowLoading(false)
      }
    }
    void loadFollowList()
  }, [followListMode, profileUserId])

  useEffect(() => {
    const loadInsights = async () => {
      if (!profileUserId || status !== "authenticated" || !isVip || !isOwnProfile) {
        setTasteCompatibility(null)
        return
      }
      try {
        const data = await getFollowInsights(profileUserId)
        setTasteCompatibility(Math.round((data.tasteSimilarityScore ?? 0) * 100))
      } catch {
        setTasteCompatibility(null)
      }
    }
    void loadInsights()
  }, [isVip, status, profileUserId, isOwnProfile])

  useEffect(() => {
    followItems.forEach((item) => {
      void ensureFollowStatus(item.userId)
    })
  }, [ensureFollowStatus, followItems])

  const filteredFollowItems = useMemo(() => {
    const term = followSearch.trim().toLowerCase()
    if (!term) return followItems
    const tokens = term.split(/\s+/).filter(Boolean)
    return followItems.filter((item) => {
      const fullName = (item.fullName ?? "").trim().toLowerCase()
      const userName = (item.userName ?? "").trim().toLowerCase()
      const combined = `${fullName} ${userName}`.trim()
      return tokens.every((token) => combined.includes(token))
    })
  }, [followItems, followSearch])

  const moveToWatched = async (movieId: number) => {
    // This action will be fully implemented in Movie detail/module flow.
    await removeFromWatchlist(movieId)
    await loadWatchData()
  }

  const moveToWatchlist = async (movieId: number) => {
    await removeFromWatched(movieId)
    await loadWatchData()
  }

  if (!profileUserId) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8 text-center">
        <p className="text-muted-foreground">Sign in to view your profile.</p>
        <Link href="/auth" className="mt-4 inline-block text-sm font-medium text-primary hover:underline">
          Sign in
        </Link>
      </div>
    )
  }

  if (profileInfo === undefined) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
        <p className="text-sm text-muted-foreground">Loading profile...</p>
      </div>
    )
  }

  if (profileInfo === null) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
        <p className="text-sm text-muted-foreground">User not found.</p>
      </div>
    )
  }

  const displayName =
    profileInfo.fullName?.trim() || profileInfo.userName || user?.userName || userProfile.name
  const initialsSource = profileInfo.fullName || profileInfo.userName || user?.userName || userProfile.avatar
  const watchedDisplay = isOwnProfile ? watchedMovies.length : "—"
  const avgDisplay =
    avgRatingStat !== null ? Number(avgRatingStat).toFixed(1) : isOwnProfile ? String(userProfile.stats.avgRating) : "—"

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      {/* Profile Header */}
      <div className="flex flex-col items-center gap-6 rounded-2xl border border-border/50 bg-card p-8 text-center md:flex-row md:text-left">
        <div className="relative">
          <div className="flex h-24 w-24 items-center justify-center rounded-full bg-primary/10 text-2xl font-bold text-primary">
            {initialsSource.slice(0, 2).toUpperCase()}
          </div>
          {isOwnProfile && isVip && (
            <div className="absolute -right-1 -top-1 rounded-full bg-primary p-1.5">
              <Crown className="h-4 w-4 text-primary-foreground" />
            </div>
          )}
        </div>
        <div className="flex-1">
          <div className="flex items-center justify-center gap-3 md:justify-start">
            <h1 className="font-serif text-2xl font-bold text-foreground">{displayName}</h1>
            {isOwnProfile && isVip && (
              <span className="rounded-full bg-primary/10 px-3 py-0.5 text-xs font-bold text-primary">VIP</span>
            )}
          </div>
          <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
            {isOwnProfile ? userProfile.bio : `@${profileInfo.userName}`}
          </p>
          <div className="mt-4 flex justify-center gap-6 md:justify-start">
            <div className="text-center">
              <div className="flex items-center gap-1.5">
                <Film className="h-4 w-4 text-primary" />
                <span className="text-lg font-bold text-foreground">{watchedDisplay}</span>
              </div>
              <p className="text-xs text-muted-foreground">Watched</p>
            </div>
            <div className="text-center">
              <div className="flex items-center gap-1.5">
                <Star className="h-4 w-4 fill-primary text-primary" />
                <span className="text-lg font-bold text-foreground">{avgDisplay}</span>
              </div>
              <p className="text-xs text-muted-foreground">Avg Rating</p>
            </div>
            <div className="text-center">
              <div className="flex items-center gap-1.5">
                <Users className="h-4 w-4 text-primary" />
                <span className="text-lg font-bold text-foreground">{followStats.followersCount}</span>
              </div>
              <p className="text-xs text-muted-foreground">Followers</p>
            </div>
            <div className="text-center">
              <div className="flex items-center gap-1.5">
                <Heart className="h-4 w-4 text-primary" />
                <span className="text-lg font-bold text-foreground">{followStats.followingsCount}</span>
              </div>
              <p className="text-xs text-muted-foreground">Following</p>
            </div>
          </div>
          {isOwnProfile && (
            <div className="mt-3 flex flex-wrap justify-center gap-1.5 md:justify-start">
              {userProfile.stats.favoriteGenres.map((g) => (
                <span key={g} className="rounded-full bg-secondary px-3 py-1 text-xs font-medium text-muted-foreground">{g}</span>
              ))}
            </div>
          )}
        </div>
        {isOwnProfile ? (
          <button
            type="button"
            disabled
            className="rounded-lg bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground opacity-90"
          >
            Your Profile
          </button>
        ) : status !== "authenticated" ? (
          <Link
            href="/auth"
            className="inline-flex shrink-0 items-center justify-center rounded-lg bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90"
          >
            Sign in to follow
          </Link>
        ) : (
          <button
            type="button"
            onClick={() => void handleProfileFollow()}
            disabled={!!profileUserId && !!loadingByUserId[profileUserId]}
            className="shrink-0 rounded-lg bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-70"
          >
            {profileUserId && loadingByUserId[profileUserId]
              ? "Updating…"
              : profileUserId && followingByUserId[profileUserId]
                ? "Following"
                : "Follow"}
          </button>
        )}
      </div>

      {/* VIP Taste Compatibility */}
      {isOwnProfile && isVip && (
        <div className="mt-6 rounded-2xl border border-primary/20 bg-primary/5 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-primary">Taste Compatibility</p>
              <p className="mt-1 text-sm text-muted-foreground">How closely your movie taste aligns</p>
            </div>
            <div className="relative flex h-20 w-20 items-center justify-center">
              <svg className="h-20 w-20 -rotate-90" viewBox="0 0 100 100">
                <circle cx="50" cy="50" r="40" fill="none" stroke="currentColor" strokeWidth="6" className="text-secondary" />
                <circle
                  cx="50" cy="50" r="40" fill="none" stroke="currentColor" strokeWidth="6"
                  className="text-primary"
                  strokeDasharray={`${(tasteCompatibility ?? userProfile.tasteCompatibility) * 2.51} 251`}
                  strokeLinecap="round"
                />
              </svg>
              <span className="absolute text-sm font-bold text-primary">{tasteCompatibility ?? userProfile.tasteCompatibility}%</span>
            </div>
          </div>
          <Link href="/vip" className="mt-3 inline-flex text-xs font-medium text-primary hover:underline">
            View Full Analytics
          </Link>
        </div>
      )}

      {/* Tabs */}
      <div className="mt-8 flex gap-1 rounded-xl border border-border/50 bg-card p-1">
        {tabs.map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={cn(
              "flex-1 rounded-lg py-2.5 text-sm font-medium transition-colors",
              activeTab === tab
                ? "bg-primary text-primary-foreground"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <div className="mt-6">
        {activeTab === "Watchlist" && !isOwnProfile && (
          <p className="text-sm text-muted-foreground">Watchlist is only visible on your own profile.</p>
        )}
        {activeTab === "Watchlist" && isOwnProfile && (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
            {watchLoading && <p className="text-sm text-muted-foreground col-span-full">Loading watchlist...</p>}
            {watchError && <p className="text-sm text-muted-foreground col-span-full">{watchError}</p>}
            {!watchLoading && !watchError && watchlistMovies.map((movie) => (
              <div key={movie.movieId} className="group">
              <Link href={`/movies/${movie.movieId}`} className="group">
                <div className="relative aspect-[2/3] overflow-hidden rounded-xl border border-border/30">
                  <Image
                    src={resolveMoviePosterUrl(movie.posterUrl)}
                    alt={movie.title}
                    fill
                    className="object-cover transition-transform duration-500 group-hover:scale-105"
                  />
                  <div className="absolute right-2 top-2">
                    <Bookmark className="h-4 w-4 fill-primary text-primary" />
                  </div>
                </div>
                <h3 className="mt-2 text-sm font-semibold text-foreground group-hover:text-primary">{movie.title}</h3>
                </Link>
                <button onClick={() => moveToWatched(movie.movieId)} className="mt-1 text-xs text-muted-foreground hover:text-foreground">Remove</button>
              </div>
            ))}
          </div>
        )}

        {activeTab === "Watched" && !isOwnProfile && (
          <p className="text-sm text-muted-foreground">Watched list is only visible on your own profile.</p>
        )}
        {activeTab === "Watched" && isOwnProfile && (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
            {watchLoading && <p className="text-sm text-muted-foreground col-span-full">Loading watched list...</p>}
            {watchError && <p className="text-sm text-muted-foreground col-span-full">{watchError}</p>}
            {!watchLoading && !watchError && watchedMovies.map((movie) => (
              <div key={movie.movieId} className="group">
              <Link href={`/movies/${movie.movieId}`} className="group">
                <div className="relative aspect-[2/3] overflow-hidden rounded-xl border border-border/30">
                  <Image
                    src={resolveMoviePosterUrl(movie.posterPath)}
                    alt={movie.title}
                    fill
                    className="object-cover transition-transform duration-500 group-hover:scale-105"
                  />
                  <div className="absolute right-2 top-2 rounded-full bg-primary/90 p-1">
                    <Eye className="h-3.5 w-3.5 text-primary-foreground" />
                  </div>
                </div>
                <h3 className="mt-2 text-sm font-semibold text-foreground group-hover:text-primary">{movie.title}</h3>
                </Link>
                <button onClick={() => moveToWatchlist(movie.movieId)} className="mt-1 text-xs text-muted-foreground hover:text-foreground">Remove</button>
              </div>
            ))}
          </div>
        )}

        {activeTab === "Reviews" && (
          <div className="flex flex-col gap-4">
            {reviewsLoading && (
              <div className="rounded-2xl border border-border/50 bg-card p-5">
                <p className="text-sm text-muted-foreground">Loading reviews...</p>
              </div>
            )}
            {reviewsError && (
              <div className="rounded-2xl border border-border/50 bg-card p-5">
                <p className="text-sm text-muted-foreground">{reviewsError}</p>
              </div>
            )}
            {!reviewsLoading && !reviewsError && userReviews.length === 0 && (
              <div className="rounded-2xl border border-border/50 bg-card p-5">
                <p className="text-sm text-muted-foreground">No reviews yet.</p>
              </div>
            )}
            {!reviewsLoading && !reviewsError && userReviews.map((review) => (
              <div
                key={review.id}
                className="flex gap-4 rounded-2xl border border-border/50 bg-card p-4 sm:p-5"
              >
                <Link
                  href={review.movieId ? `/movies/${review.movieId}` : "#"}
                  className="relative h-32 w-[5.25rem] shrink-0 overflow-hidden rounded-xl border border-border/40 bg-muted/30 sm:h-36 sm:w-24"
                >
                  <Image
                    src={resolveMoviePosterUrl(review.posterPath)}
                    alt={review.movie}
                    fill
                    className="object-cover"
                    sizes="96px"
                  />
                </Link>
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <Link
                      href={review.movieId ? `/movies/${review.movieId}` : "#"}
                      className="font-semibold text-foreground hover:text-primary hover:underline"
                    >
                      {review.movie}
                    </Link>
                    {review.rating !== null && (
                      <div className="flex shrink-0 items-center gap-1 rounded-full bg-primary/10 px-2.5 py-1">
                        <Star className="h-3.5 w-3.5 fill-primary text-primary" />
                        <span className="text-xs font-bold text-primary">{Number(review.rating).toFixed(1)}/10</span>
                      </div>
                    )}
                  </div>
                  <p className="mt-2 text-sm leading-relaxed text-muted-foreground">{review.text}</p>
                  <p className="mt-2 text-xs text-muted-foreground">{review.date}</p>
                </div>
              </div>
            ))}
          </div>
        )}

        {activeTab === "Following" && (
          <div>
            <div className="mb-4 inline-flex rounded-lg border border-border/40 bg-card p-1">
              <button
                onClick={() => setFollowListMode("following")}
                className={cn(
                  "rounded-md px-3 py-1.5 text-xs font-medium transition-colors",
                  followListMode === "following" ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"
                )}
              >
                Following
              </button>
              <button
                onClick={() => setFollowListMode("followers")}
                className={cn(
                  "rounded-md px-3 py-1.5 text-xs font-medium transition-colors",
                  followListMode === "followers" ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"
                )}
              >
                Followers
              </button>
            </div>
            <div className="mb-4">
              <input
                type="text"
                value={followSearch}
                onChange={(e) => setFollowSearch(e.target.value)}
                placeholder="Search users by first name, last name, full name, or username"
                className="w-full rounded-lg border border-border/50 bg-card px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground"
              />
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              {followLoading && <p className="text-sm text-muted-foreground md:col-span-2">Loading users...</p>}
              {followError && <p className="text-sm text-muted-foreground md:col-span-2">{followError}</p>}
              {!followLoading && !followError && filteredFollowItems.length === 0 && (
                <p className="text-sm text-muted-foreground md:col-span-2">No users yet.</p>
              )}
              {!followLoading && !followError && filteredFollowItems.map((item) => {
                const isFollowing = followingByUserId[item.userId] ?? false
                const isMe = item.userId === user?.userId
                return (
                  <div
                    key={item.userId}
                    className="rounded-lg border border-border/20 bg-card p-4 hover:bg-surface transition-colors"
                  >
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground font-bold text-sm">
                          {(item.fullName || item.userName).slice(0, 2).toUpperCase()}
                        </div>
                        <div>
                          <h4 className="font-semibold text-foreground text-sm">{item.fullName || item.userName}</h4>
                          <p className="text-xs text-muted-foreground">@{item.userName}</p>
                        </div>
                      </div>
                      <button
                        onClick={() => void toggleFollow(item.userId)}
                        disabled={isMe || status !== "authenticated" || loadingByUserId[item.userId]}
                        className={cn(
                          "rounded-lg px-2 py-1 text-xs font-medium transition-colors border",
                          isFollowing
                            ? "bg-primary/20 text-primary border-primary/30 hover:bg-primary/30"
                            : "bg-primary text-primary-foreground border-primary hover:bg-primary/90",
                          "disabled:opacity-60"
                        )}
                      >
                        {isMe ? (
                          "You"
                        ) : status !== "authenticated" ? (
                          "Sign in"
                        ) : loadingByUserId[item.userId] ? (
                          "Updating..."
                        ) : isFollowing ? (
                          <span className="inline-flex items-center gap-1"><UserCheck className="h-3 w-3" />Following</span>
                        ) : (
                          <span className="inline-flex items-center gap-1"><UserPlus className="h-3 w-3" />Follow</span>
                        )}
                      </button>
                    </div>
                  </div>
                )
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

export default function ProfilePage() {
  return <ProfilePageClient />
}
