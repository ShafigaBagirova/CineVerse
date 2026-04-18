"use client"

import { AuthProvider } from "@/components/providers/auth-provider"
import { FollowProvider } from "@/components/providers/follow-provider"
import { NotificationProvider } from "@/components/providers/notification-provider"

export function AppProviders({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <FollowProvider>
        <NotificationProvider>{children}</NotificationProvider>
      </FollowProvider>
    </AuthProvider>
  )
}
