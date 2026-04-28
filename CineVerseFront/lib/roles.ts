import type { JwtUserInfo } from "@/lib/api/auth"

/** Backend `Domain.Constants.RoleNames.Admin` */
export function isAdminUser(user: JwtUserInfo | null | undefined): boolean {
  const roles = user?.roles
  if (!roles?.length) return false
  return roles.some((r) => r === "Admin")
}

/** VIP is role-based (`RoleNames.Vip` = "Vip"); accept "VIP" if ever normalized differently. */
export function isVipUser(user: JwtUserInfo | null | undefined): boolean {
  const roles = user?.roles
  if (!roles?.length) return false
  return roles.includes("Vip") || roles.includes("VIP")
}
