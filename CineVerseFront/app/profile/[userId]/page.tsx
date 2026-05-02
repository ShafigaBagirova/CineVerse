import { ProfilePageClient } from "../page"
import { sanitizeProfileRouteUserId } from "@/lib/api/user"

export default async function UserProfilePage({ params }: { params: Promise<{ userId: string }> }) {
  const { userId: raw } = await params
  const routeUserId = sanitizeProfileRouteUserId(raw)
  return <ProfilePageClient routeUserId={routeUserId} />
}
