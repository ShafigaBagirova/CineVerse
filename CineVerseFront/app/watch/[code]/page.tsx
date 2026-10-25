import { WatchRoom } from "@/components/watch/watch-room"

export default async function WatchRoomRoute({ params }: { params: Promise<{ code: string }> }) {
  const { code } = await params
  return <WatchRoom code={code.toUpperCase()} />
}
