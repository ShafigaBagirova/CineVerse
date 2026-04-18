import { ProfilePageClient } from "../page"

export default async function UserProfilePage({ params }: { params: Promise<{ userId: string }> }) {
  const { userId } = await params
  return <ProfilePageClient routeUserId={userId} />
}
