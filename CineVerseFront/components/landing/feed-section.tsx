"use client"

import { useEffect, useMemo, useState } from "react"
import Image from "next/image"
import Link from "next/link"
import { Star, Crown, UserPlus, UserCheck } from "lucide-react"
import { getAllMovies, type GetAllMoviesResponse } from "@/lib/api/movies"
import { useAuth } from "@/components/providers/auth-provider"
import { useFollow } from "@/components/providers/follow-provider"

export function FeedSection() {
  const { status } = useAuth()
  const {
    suggestedUsers,
    suggestedLoading,
    suggestedError,
    suggestedAuthRequired,
    followingByUserId,
    loadingByUserId,
    ensureFollowStatus,
    toggleFollow,
  } = useFollow()
  const [movies, setMovies] = useState<GetAllMoviesResponse[]>([])

  useEffect(() => {
    let mounted = true
    const load = async () => {
      try {
        const response = await getAllMovies({ pageNumber: 1, pageSize: 8, sortBy: "userRating", desc: true })
        if (mounted) setMovies(response.items)
      } catch {
        if (mounted) setMovies([])
      }
    }
    void load()
    return () => {
      mounted = false
    }
  }, [])

  useEffect(() => {
    suggestedUsers.forEach((item) => {
      void ensureFollowStatus(item.userId)
    })
  }, [ensureFollowStatus, suggestedUsers])

  const friendActivity = useMemo(
    () => [
      { user: "Leyla H.", avatar: "LH", action: "watched", movie: movies[0], rating: 9, time: "2h ago" },
      { user: "Rashad M.", avatar: "RM", action: "reviewed", movie: movies[2], rating: 8, time: "5h ago" },
      { user: "Nigar A.", avatar: "NA", action: "added to watchlist", movie: movies[4], rating: null, time: "8h ago" },
    ].filter((item) => item.movie),
    [movies]
  )
  const trendingAZ = movies.slice(0, 4)
  const recommendations = movies.slice(2, 6)

  return (
    <section className="py-20">
      <div className="mx-auto max-w-7xl px-4 lg:px-8">
        {/* Friend Activity */}
        <div className="mb-14">
          <div className="mb-6 flex items-end justify-between">
            <div>
              <p className="mb-1 text-xs font-medium uppercase tracking-[0.2em] text-primary">Activity</p>
              <h2 className="font-serif text-2xl font-bold text-foreground">Friend Activity</h2>
            </div>
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            {friendActivity.map((item, i) => (
              <div key={i} className="rounded-2xl border border-border/50 bg-card p-4 transition-colors hover:border-border">
                <div className="flex items-center gap-3">
                  <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                    {item.avatar}
                  </div>
                  <div className="flex-1 text-sm">
                    <span className="font-semibold text-foreground">{item.user}</span>
                    <span className="text-muted-foreground"> {item.action}</span>
                  </div>
                  <span className="text-xs text-muted-foreground">{item.time}</span>
                </div>
                <Link href={`/movies/${item.movie.id}`} className="mt-3 flex gap-3">
                  <div className="relative aspect-[2/3] w-14 shrink-0 overflow-hidden rounded-lg">
                    <Image src={item.movie.posterUrl || "/images/movie-1.jpg"} alt={item.movie.title} fill className="object-cover" />
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold text-foreground hover:text-primary transition-colors">{item.movie.title}</h3>
                    <p className="text-xs text-muted-foreground">{item.movie.releaseYear ?? "N/A"}</p>
                    {item.rating && (
                      <div className="mt-1 flex items-center gap-1">
                        <Star className="h-3 w-3 fill-primary text-primary" />
                        <span className="text-xs font-semibold text-foreground">{item.rating}/10</span>
                      </div>
                    )}
                  </div>
                </Link>
              </div>
            ))}
          </div>
        </div>

        {/* Trending in Azerbaijan */}
        <div className="mb-14">
          <div className="mb-6 flex items-end justify-between">
            <div>
              <p className="mb-1 text-xs font-medium uppercase tracking-[0.2em] text-primary">Local</p>
              <h2 className="font-serif text-2xl font-bold text-foreground">Trending in Azerbaijan</h2>
            </div>
            <Link href="/movies" className="text-sm font-medium text-muted-foreground hover:text-primary transition-colors">
              View All
            </Link>
          </div>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            {trendingAZ.map((movie, idx) => (
              <Link key={movie.id} href={`/movies/${movie.id}`} className="group">
                <div className="relative aspect-[2/3] overflow-hidden rounded-xl border border-border/30">
                  <Image src={movie.posterUrl || "/images/movie-1.jpg"} alt={movie.title} fill className="object-cover transition-transform duration-500 group-hover:scale-105" />
                  <div className="absolute left-2 top-2 flex h-7 w-7 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground">
                    {idx + 1}
                  </div>
                </div>
                <h3 className="mt-2 text-sm font-semibold text-foreground group-hover:text-primary transition-colors">{movie.title}</h3>
                <div className="mt-0.5 flex items-center gap-1 text-xs text-muted-foreground">
                  <Star className="h-3 w-3 fill-primary text-primary" />
                  <span>{Number(movie.userAverageRating ?? movie.tmdbRating ?? 0).toFixed(1)}</span>
                </div>
              </Link>
            ))}
          </div>
        </div>

        {/* Personalized Recommendations */}
        <div className="mb-14">
          <div className="mb-6">
            <p className="mb-1 text-xs font-medium uppercase tracking-[0.2em] text-primary">For You</p>
            <h2 className="font-serif text-2xl font-bold text-foreground">Recommended</h2>
          </div>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            {recommendations.map((movie) => (
              <Link key={movie.id} href={`/movies/${movie.id}`} className="group">
                <div className="relative aspect-[2/3] overflow-hidden rounded-xl border border-border/30">
                  <Image src={movie.posterUrl || "/images/movie-1.jpg"} alt={movie.title} fill className="object-cover transition-transform duration-500 group-hover:scale-105" />
                </div>
                <h3 className="mt-2 text-sm font-semibold text-foreground group-hover:text-primary transition-colors">{movie.title}</h3>
                <p className="mt-0.5 text-xs text-muted-foreground">{movie.releaseYear ?? "N/A"}</p>
              </Link>
            ))}
          </div>
        </div>

        {/* People You Might Know */}
        <div>
          <div className="mb-6">
            <p className="mb-1 text-xs font-medium uppercase tracking-[0.2em] text-primary">Community</p>
            <h2 className="font-serif text-2xl font-bold text-foreground">People You Might Know</h2>
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            {suggestedLoading && <p className="text-sm text-muted-foreground md:col-span-3">Loading suggestions...</p>}
            {!suggestedLoading && suggestedAuthRequired && (
              <p className="text-sm text-muted-foreground md:col-span-3">
                {status === "authenticated"
                  ? "Suggested users are available for VIP accounts."
                  : "Sign in to see suggested users."}
              </p>
            )}
            {!suggestedLoading && suggestedError && !suggestedAuthRequired && (
              <p className="text-sm text-muted-foreground md:col-span-3">{suggestedError}</p>
            )}
            {!suggestedLoading && !suggestedError && !suggestedAuthRequired && suggestedUsers.length === 0 && (
              <p className="text-sm text-muted-foreground md:col-span-3">No suggestions available right now.</p>
            )}
            {!suggestedLoading && suggestedUsers.map((user) => (
              <div
                key={user.userId}
                className="rounded-xl border border-border/20 bg-card p-4 hover:bg-surface transition-colors"
              >
                <div className="flex items-start gap-3 mb-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground font-bold text-sm flex-shrink-0">
                    {(user.fullName || user.userName).slice(0, 2).toUpperCase()}
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center gap-1">
                      <h4 className="font-semibold text-foreground text-sm">{user.fullName || user.userName}</h4>
                      {user.tasteScore > 0 && (
                        <Crown className="h-3 w-3 text-primary" />
                      )}
                    </div>
                    <p className="text-xs text-muted-foreground">@{user.userName}</p>
                  </div>
                </div>

                <p className="text-xs text-muted-foreground mb-3 line-clamp-2">
                  Common movies: {user.commonMoviesCount} - Taste: {Math.round(user.tasteScore)}%
                </p>

                <button
                  onClick={() => void toggleFollow(user.userId)}
                  disabled={status !== "authenticated" || loadingByUserId[user.userId]}
                  className="w-full flex items-center justify-center gap-2 rounded-lg bg-primary px-3 py-2 text-xs font-medium text-primary-foreground hover:bg-primary/90 transition-colors disabled:opacity-60"
                >
                  {followingByUserId[user.userId] ? <UserCheck className="h-3.5 w-3.5" /> : <UserPlus className="h-3.5 w-3.5" />}
                  {status !== "authenticated"
                    ? "Sign in to follow"
                    : loadingByUserId[user.userId]
                      ? "Updating..."
                      : followingByUserId[user.userId]
                        ? "Following"
                        : "Follow"}
                </button>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
