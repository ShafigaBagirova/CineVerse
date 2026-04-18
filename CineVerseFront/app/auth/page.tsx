"use client"

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Film, Sparkles, Ticket, Users } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { GoogleSignInButton } from "@/components/auth/google-sign-in-button"
import { useAuth } from "@/components/providers/auth-provider"
import { getCurrentUser, register } from "@/lib/api/auth"
import { isAdminUser } from "@/lib/roles"
import { cn } from "@/lib/utils"

const PENDING_EMAIL_KEY = "cineverse.pendingVerificationEmail"

type Tab = "login" | "register"

const features = [
  { icon: Ticket, label: "Book seats & tickets" },
  { icon: Sparkles, label: "Rate & review what you watch" },
  { icon: Users, label: "Follow friends & discover picks" },
]

export default function AuthPage() {
  const router = useRouter()
  const { login, loginWithGoogle } = useAuth()
  const [tab, setTab] = useState<Tab>("login")
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const [loginForm, setLoginForm] = useState({ login: "", password: "" })
  const [registerForm, setRegisterForm] = useState({
    userName: "",
    email: "",
    password: "",
    fullName: "",
    dateOfBirth: "",
    gender: "0",
  })

  useEffect(() => {
    const params = new URLSearchParams(window.location.search)
    if (params.get("tab") === "register") setTab("register")
    const loginParam = params.get("login")
    if (loginParam) {
      setLoginForm((prev) => ({ ...prev, login: decodeURIComponent(loginParam) }))
    }
    if (window.location.search) {
      router.replace("/auth", { scroll: false })
    }
  }, [router])

  const title = useMemo(() => {
    if (tab === "register") return "Create your account"
    return "Welcome back"
  }, [tab])

  const subtitle = useMemo(() => {
    if (tab === "register") return "Join CineVerse to track films and book your next night out."
    return "Sign in to continue your watchlist, tickets, and reviews."
  }, [tab])

  function selectTab(next: Tab) {
    setTab(next)
    setError(null)
    setSuccess(null)
  }

  async function onLoginSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      await login(loginForm)
      setSuccess("Login successful.")
      const me = await getCurrentUser()
      router.push(isAdminUser(me) ? "/admin/dashboard" : "/home")
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed.")
    } finally {
      setBusy(false)
    }
  }

  async function onRegisterSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      await register({
        userName: registerForm.userName.trim(),
        email: registerForm.email.trim(),
        password: registerForm.password,
        fullName: registerForm.fullName.trim(),
        dateOfBirth: registerForm.dateOfBirth,
        gender: registerForm.gender === "1" ? "female" : "male",
      })
      const email = registerForm.email.trim()
      try {
        sessionStorage.setItem(PENDING_EMAIL_KEY, email)
      } catch {
        /* ignore */
      }
      router.push("/auth/verify?from=register")
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed.")
    } finally {
      setBusy(false)
    }
  }

  const onGoogleCredential = useCallback(
    async (credential: string) => {
      setBusy(true)
      setError(null)
      setSuccess(null)
      try {
        await loginWithGoogle(credential)
        setSuccess("Signed in with Google.")
        const me = await getCurrentUser()
        router.push(isAdminUser(me) ? "/admin/dashboard" : "/home")
      } catch (err) {
        setError(err instanceof Error ? err.message : "Google sign-in failed.")
      } finally {
        setBusy(false)
      }
    },
    [loginWithGoogle, router],
  )

  return (
    <div className="relative min-h-[calc(100vh-4rem)] overflow-hidden">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_120%_80%_at_50%_-20%,rgba(107,168,155,0.18),transparent_55%),radial-gradient(ellipse_80%_50%_at_100%_50%,rgba(126,187,176,0.06),transparent_50%),radial-gradient(ellipse_60%_40%_at_0%_80%,rgba(107,168,155,0.08),transparent_45%)]"
        aria-hidden
      />
      <div className="pointer-events-none absolute inset-0 bg-[linear-gradient(180deg,rgba(10,10,10,0.3)_0%,var(--background)_100%)]" aria-hidden />

      <div className="relative z-10 mx-auto flex max-w-6xl flex-col gap-10 px-4 py-12 lg:flex-row lg:items-center lg:gap-16 lg:py-16 lg:px-8">
        <div className="flex flex-1 flex-col gap-6 text-center lg:max-w-md lg:text-left">
          <div className="flex flex-col items-center gap-4 lg:items-start">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl border border-primary/25 bg-primary/10 shadow-[0_0_40px_-8px_rgba(107,168,155,0.4)]">
              <Film className="h-7 w-7 text-primary" strokeWidth={1.75} />
            </div>
            <div>
              <p className="font-serif text-3xl font-bold tracking-tight text-foreground md:text-4xl">CineVerse</p>
              <p className="mt-2 text-sm leading-relaxed text-muted-foreground md:text-base">
                Welcome back to CineVerse. Track, rate, and discover films—then book your seat in one place.
              </p>
            </div>
          </div>
          <ul className="mx-auto flex w-full max-w-sm flex-col gap-3 text-left text-sm text-muted-foreground lg:mx-0">
            {features.map(({ icon: Icon, label }) => (
              <li key={label} className="flex items-center gap-3 rounded-xl border border-border/40 bg-card/40 px-4 py-3 backdrop-blur-sm">
                <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                  <Icon className="h-4 w-4" />
                </span>
                {label}
              </li>
            ))}
          </ul>
        </div>

        <div className="flex w-full flex-1 justify-center lg:justify-end">
          <div className="w-full max-w-md">
            <div
              className={cn(
                "rounded-2xl border border-primary/20 bg-card/90 p-1 shadow-2xl shadow-black/40 backdrop-blur-xl",
                "ring-1 ring-primary/10",
              )}
            >
              <div className="rounded-[calc(var(--radius)+4px)] bg-gradient-to-b from-card to-card/95 p-6 sm:p-8">
                <div className="mb-6 flex gap-1 rounded-xl border border-border/50 bg-background/80 p-1">
                  {(["login", "register"] as Tab[]).map((item) => (
                    <button
                      key={item}
                      type="button"
                      className={cn(
                        "flex-1 rounded-lg py-2.5 text-xs font-semibold uppercase tracking-wide transition-all sm:text-sm",
                        tab === item
                          ? "bg-primary text-primary-foreground shadow-md shadow-primary/20"
                          : "text-muted-foreground hover:bg-muted/50 hover:text-foreground",
                      )}
                      onClick={() => selectTab(item)}
                    >
                      {item === "login" ? "Login" : "Register"}
                    </button>
                  ))}
                </div>

                <div className="mb-6 space-y-1">
                  <h1 className="font-serif text-2xl font-bold tracking-tight text-foreground">{title}</h1>
                  <p className="text-sm text-muted-foreground">{subtitle}</p>
                </div>

                {error && (
                  <Alert variant="destructive" className="mb-4 border-destructive/40 bg-destructive/5">
                    <AlertDescription>{error}</AlertDescription>
                  </Alert>
                )}
                {success && !error && (
                  <Alert className="mb-4 border-primary/30 bg-primary/5 text-primary">
                    <AlertDescription className="text-primary">{success}</AlertDescription>
                  </Alert>
                )}

                {tab === "login" && (
                  <div className="space-y-5">
                    <form className="space-y-4" onSubmit={onLoginSubmit}>
                      <div className="space-y-2">
                        <Label htmlFor="auth-login">Username or email</Label>
                        <Input
                          id="auth-login"
                          className="h-11 border-border/60 bg-background/50"
                          placeholder="you@example.com"
                          autoComplete="username"
                          value={loginForm.login}
                          onChange={(e) => setLoginForm((prev) => ({ ...prev, login: e.target.value }))}
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="auth-password">Password</Label>
                        <Input
                          id="auth-password"
                          type="password"
                          className="h-11 border-border/60 bg-background/50"
                          placeholder="••••••••"
                          autoComplete="current-password"
                          value={loginForm.password}
                          onChange={(e) => setLoginForm((prev) => ({ ...prev, password: e.target.value }))}
                          required
                        />
                      </div>
                      <Button disabled={busy} className="h-11 w-full text-base font-semibold shadow-lg shadow-primary/15" type="submit">
                        {busy ? "Signing in…" : "Sign in"}
                      </Button>
                    </form>

                    <div className="relative py-2">
                      <Separator className="bg-border/60" />
                      <span className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 bg-card px-3 text-[11px] font-medium uppercase tracking-wider text-muted-foreground">
                        or continue with
                      </span>
                    </div>

                    <GoogleSignInButton onCredential={onGoogleCredential} disabled={busy} />
                  </div>
                )}

                {tab === "register" && (
                  <form className="space-y-4" onSubmit={onRegisterSubmit}>
                    <div className="space-y-2">
                      <Label htmlFor="reg-user">Username</Label>
                      <Input
                        id="reg-user"
                        className="h-11 border-border/60 bg-background/50"
                        placeholder="cinephile42"
                        value={registerForm.userName}
                        onChange={(e) => setRegisterForm((prev) => ({ ...prev, userName: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="reg-email">Email</Label>
                      <Input
                        id="reg-email"
                        placeholder="you@example.com"
                        type="email"
                        className="h-11 border-border/60 bg-background/50"
                        value={registerForm.email}
                        onChange={(e) => setRegisterForm((prev) => ({ ...prev, email: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="reg-name">Full name</Label>
                      <Input
                        id="reg-name"
                        className="h-11 border-border/60 bg-background/50"
                        placeholder="Jane Doe"
                        value={registerForm.fullName}
                        onChange={(e) => setRegisterForm((prev) => ({ ...prev, fullName: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="reg-dob">Date of birth</Label>
                      <Input
                        id="reg-dob"
                        type="date"
                        className="h-11 border-border/60 bg-background/50"
                        value={registerForm.dateOfBirth}
                        onChange={(e) => setRegisterForm((prev) => ({ ...prev, dateOfBirth: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="reg-gender">Gender</Label>
                      <select
                        id="reg-gender"
                        className="border-input focus-visible:ring-ring/50 h-11 w-full rounded-md border border-border/60 bg-background/50 px-3 py-2 text-sm outline-none focus-visible:ring-[3px]"
                        value={registerForm.gender}
                        onChange={(e) => setRegisterForm((prev) => ({ ...prev, gender: e.target.value }))}
                        required
                      >
                        <option value="0">Male</option>
                        <option value="1">Female</option>
                      </select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="reg-password">Password</Label>
                      <Input
                        id="reg-password"
                        type="password"
                        className="h-11 border-border/60 bg-background/50"
                        placeholder="At least 8 characters"
                        minLength={8}
                        value={registerForm.password}
                        onChange={(e) => setRegisterForm((prev) => ({ ...prev, password: e.target.value }))}
                        required
                      />
                    </div>
                    <Button disabled={busy} className="h-11 w-full text-base font-semibold shadow-lg shadow-primary/15" type="submit">
                      {busy ? "Creating account…" : "Create account"}
                    </Button>
                  </form>
                )}

                <p className="mt-6 text-center text-xs text-muted-foreground">
                  <Link href="/movies" className="font-medium text-primary transition-colors hover:underline">
                    Browse movies
                  </Link>
                  <span className="mx-1.5 text-border">·</span>
                  <Link href="/" className="font-medium text-muted-foreground transition-colors hover:text-foreground">
                    Home
                  </Link>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
