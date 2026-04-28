"use client"

import { createContext, useContext, useEffect, useMemo, useState } from "react"
import * as authApi from "@/lib/api/auth"
import { getAccessToken } from "@/lib/api/http"
import { ApiError } from "@/lib/api/types"

type AuthStatus = "loading" | "authenticated" | "guest"

interface AuthContextValue {
  status: AuthStatus
  user: authApi.JwtUserInfo | null
  login: (request: authApi.LoginRequest) => Promise<void>
  loginWithGoogle: (idToken: string) => Promise<void>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>("loading")
  const [user, setUser] = useState<authApi.JwtUserInfo | null>(null)

  const refreshUser = async () => {
    const token = getAccessToken()
    if (!token) {
      setUser(null)
      setStatus("guest")
      return
    }

    try {
      const me = await authApi.getCurrentUser()
      setUser(me)
      setStatus("authenticated")
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) {
        authApi.logout()
        setUser(null)
        setStatus("guest")
        return
      }
      // Transient /me failures should not wipe the session (e.g. VIP webhook delay, network blips).
    }
  }

  useEffect(() => {
    void refreshUser()
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      user,
      login: async (request) => {
        await authApi.login(request)
        await refreshUser()
      },
      loginWithGoogle: async (idToken: string) => {
        await authApi.googleLogin(idToken)
        await refreshUser()
      },
      logout: () => {
        authApi.logout()
        setUser(null)
        setStatus("guest")
      },
      refreshUser,
    }),
    [status, user]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider")
  }
  return context
}
