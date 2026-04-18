import type { JwtUserInfo } from "@/lib/api/auth"

/** Backend `Domain.Constants.RoleNames.Admin` */
export function isAdminUser(user: JwtUserInfo | null | undefined): boolean {
  const roles = user?.roles
  if (!roles?.length) return false
  return roles.some((r) => r === "Admin")
}
