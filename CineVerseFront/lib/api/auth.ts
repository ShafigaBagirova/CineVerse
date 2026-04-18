import { apiRequest, clearTokens, persistTokens } from "@/lib/api/http"

export interface LoginRequest {
  login: string
  password: string
}

/** Matches API `JsonStringEnumConverter` (camelCase names; integers disallowed). */
export type RegisterGender = "male" | "female"

export interface RegisterRequest {
  userName: string
  email: string
  password: string
  fullName: string
  dateOfBirth: string
  gender: RegisterGender
}

export interface ConfirmRegistrationRequest {
  email: string
  code: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  expiresAtUtc: string
}

export interface RegisterResponse {
  isCodeSent: boolean
  message: string
}

export interface JwtUserInfo {
  userId: string
  userName: string
  email: string
  roles: string[]
}

export async function login(request: LoginRequest) {
  const data = await apiRequest<TokenResponse>("/api/auth/login", {
    method: "POST",
    auth: false,
    body: request,
  })
  persistTokens(data)
  return data
}

export async function register(request: RegisterRequest) {
  return apiRequest<RegisterResponse>("/api/auth/register", {
    method: "POST",
    auth: false,
    body: request,
  })
}

export async function confirmRegistration(request: ConfirmRegistrationRequest) {
  return apiRequest<unknown>("/api/auth/confirm-registration", {
    method: "POST",
    auth: false,
    body: request,
  })
}

/** Resend email verification code (backend: POST /api/auth/resend-verification). */
export async function resendVerificationCode(email: string) {
  return apiRequest<unknown>("/api/auth/resend-verification", {
    method: "POST",
    auth: false,
    body: { email },
  })
}

/** Backend: POST /api/auth/google-login with { idToken }; returns AuthResponse in data envelope. */
export async function googleLogin(idToken: string) {
  const data = await apiRequest<{
    accessToken: string
    refreshToken: string
    accessTokenExpiresAtUtc: string
  }>("/api/auth/google-login", {
    method: "POST",
    auth: false,
    body: { idToken },
  })
  persistTokens({
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    expiresAtUtc: data.accessTokenExpiresAtUtc,
  })
  return data
}

export async function getCurrentUser() {
  return apiRequest<JwtUserInfo>("/api/auth/me", { method: "GET", auth: true, quiet: true })
}

export function logout() {
  clearTokens()
}
