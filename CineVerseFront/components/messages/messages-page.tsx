"use client"

import { useCallback, useEffect, useMemo, useRef, useState, type KeyboardEvent } from "react"
import { format } from "date-fns"
import { Search, Send, Wifi, WifiOff } from "lucide-react"
import { useSearchParams } from "next/navigation"
import { toast } from "sonner"
import { useAuth } from "@/components/providers/auth-provider"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Skeleton } from "@/components/ui/skeleton"
import { Textarea } from "@/components/ui/textarea"
import { useChatHub } from "@/hooks/use-chat-hub"
import {
  createPrivateChat,
  getChatMessages,
  getMyChats,
  markMessagesAsRead,
  sendMessage,
  type ChatDto,
  type ChatParticipantDto,
  type MessageDto,
} from "@/lib/api/chats"
import { searchUsers, type UserSearchDto } from "@/lib/api/user"
import { cn } from "@/lib/utils"

const CHAT_UNREAD_EVENT = "chat-unread-count-changed"
const MESSAGES_UNREAD_UPDATED_EVENT = "messages-unread-updated"
const MESSAGE_UNREAD_COUNT_CHANGE_EVENT = "message-unread-count-change"

function getChatDisplayName(chat: ChatDto, currentUserId: string) {
  const otherParticipants = chat.participants.filter((participant) => participant.userId !== currentUserId)
  const candidates = otherParticipants.length > 0 ? otherParticipants : chat.participants
  const names = candidates
    .map((participant) => participant.userName?.trim())
    .filter((name): name is string => Boolean(name))

  if (names.length > 0) return names.join(", ")
  return candidates.map((participant) => participant.userId).join(", ") || `Chat #${chat.id}`
}

function getParticipantInitials(participant: ChatParticipantDto | undefined) {
  const source = participant?.userName?.trim() || participant?.userId?.trim() || "C"
  return source
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part.charAt(0).toUpperCase())
    .join("")
}

function formatMessageTime(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ""
  return format(date, "HH:mm")
}

function formatChatPreviewTime(value?: string | null) {
  if (!value) return ""
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ""
  return format(date, "MMM d, HH:mm")
}

function sortMessages(messages: MessageDto[]) {
  return [...messages].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
}

function normalizeMessage(raw: MessageDto): MessageDto {
  const source = raw as unknown as Record<string, unknown>
  const chatId = Number(source.chatId ?? source.ChatId ?? 0)
  const id = Number(source.id ?? source.Id ?? 0)
  const senderId = String(source.senderId ?? source.SenderId ?? "")
  const content = String(source.content ?? source.Content ?? "")
  const createdAt = String(source.createdAt ?? source.CreatedAt ?? new Date().toISOString())
  const isReadRaw = source.isRead ?? source.IsRead
  const isRead = typeof isReadRaw === "boolean" ? isReadRaw : false
  return {
    ...raw,
    id,
    chatId,
    senderId,
    content,
    createdAt,
    isRead,
  }
}

export function MessagesPage() {
  const { status, user } = useAuth()
  const searchParams = useSearchParams()
  const currentUserId = user?.userId ?? ""
  const requestedChatId = Number(searchParams.get("chatId") ?? "")

  const [chatSearch, setChatSearch] = useState("")
  const [userSearchResults, setUserSearchResults] = useState<UserSearchDto[]>([])
  const [loadingUserSearch, setLoadingUserSearch] = useState(false)
  const [creatingPrivateChatFor, setCreatingPrivateChatFor] = useState<string | null>(null)
  const [messageInput, setMessageInput] = useState("")
  const [chats, setChats] = useState<ChatDto[]>([])
  const [messages, setMessages] = useState<MessageDto[]>([])
  const [activeChatId, setActiveChatId] = useState<number | null>(null)
  const [loadingChats, setLoadingChats] = useState(false)
  const [loadingMessages, setLoadingMessages] = useState(false)
  const [sending, setSending] = useState(false)
  const [pageError, setPageError] = useState<string | null>(null)
  const messagesEndRef = useRef<HTMLDivElement | null>(null)

  const emitGlobalUnread = useCallback((nextChats: ChatDto[]) => {
    if (typeof window === "undefined") return
    const totalUnread = nextChats.reduce((sum, chat) => sum + (chat.unreadCount ?? 0), 0)
    window.dispatchEvent(new CustomEvent(CHAT_UNREAD_EVENT, { detail: { totalUnread } }))
    window.dispatchEvent(new Event(MESSAGES_UNREAD_UPDATED_EVENT))
  }, [])

  const markActiveMessagesAsReadLocally = useCallback(() => {
    setMessages((prev) =>
      prev.map((message) =>
        message.senderId !== currentUserId ? { ...message, isRead: true } : message
      )
    )
  }, [currentUserId])

  const mergeIncomingMessage = useCallback((incomingRaw: MessageDto) => {
    const incoming = normalizeMessage(incomingRaw)
    console.log("received message:", incoming)
    if (incoming.chatId === activeChatId) {
      setMessages((prev) => {
        if (prev.some((message) => message.id === incoming.id)) return prev
        return sortMessages([...prev, incoming])
      })
    }

    setChats((prev) => {
      const existingTarget = prev.find((chat) => chat.id === incoming.chatId)
      const shouldIncrementUnread = incoming.senderId !== currentUserId && activeChatId !== incoming.chatId
      const updated = prev.map((chat) => {
        if (chat.id !== incoming.chatId) return chat
        return {
          ...chat,
          lastMessage: incoming,
          unreadCount: shouldIncrementUnread ? (chat.unreadCount ?? 0) + 1 : 0,
        }
      })

      if (!existingTarget) {
        updated.unshift({
          id: incoming.chatId,
          createdAt: incoming.createdAt,
          isGroup: false,
          participants: [],
          lastMessage: incoming,
          unreadCount: shouldIncrementUnread ? 1 : 0,
        })
      }

      const sorted = updated.sort((a, b) => {
        const aTime = new Date(a.lastMessage?.createdAt ?? a.createdAt).getTime()
        const bTime = new Date(b.lastMessage?.createdAt ?? b.createdAt).getTime()
        return bTime - aTime
      })
      emitGlobalUnread(sorted)
      return sorted
    })

    if (incoming.chatId === activeChatId && incoming.senderId !== currentUserId) {
      void markMessagesAsRead(incoming.chatId)
        .then(() => {
          if (typeof window !== "undefined") {
            window.dispatchEvent(
              new CustomEvent(MESSAGE_UNREAD_COUNT_CHANGE_EVENT, {
                detail: { reset: true },
              })
            )
          }
          markActiveMessagesAsReadLocally()
          setChats((prev) => {
            const updated = prev.map((chat) =>
              chat.id === incoming.chatId ? { ...chat, unreadCount: 0 } : chat
            )
            emitGlobalUnread(updated)
            return updated
          })
        })
        .catch(() => {})
    }

    if (incoming.senderId !== currentUserId && incoming.chatId !== activeChatId && typeof window !== "undefined") {
      window.dispatchEvent(
        new CustomEvent(MESSAGE_UNREAD_COUNT_CHANGE_EVENT, {
          detail: { increment: 1 },
        })
      )
    }
  }, [activeChatId, currentUserId, emitGlobalUnread, markActiveMessagesAsReadLocally])

  const { isConnected, connectionStatus, connectionError, sendMessageToChat } = useChatHub({
    enabled: status === "authenticated",
    activeChatId,
    onReceiveMessage: mergeIncomingMessage,
  })

  const loadChats = useCallback(async () => {
    setLoadingChats(true)
    setPageError(null)
    try {
      const response = await getMyChats()
      const sorted = [...response].sort((a, b) => {
        const aTime = new Date(a.lastMessage?.createdAt ?? a.createdAt).getTime()
        const bTime = new Date(b.lastMessage?.createdAt ?? b.createdAt).getTime()
        return bTime - aTime
      })
      const normalized = sorted.map((chat) => ({ ...chat, unreadCount: chat.unreadCount ?? 0 }))
      setChats(normalized)
      console.log("chats with unread:", normalized)
      emitGlobalUnread(normalized)
      setActiveChatId((previous) => {
        if (previous) return previous
        if (!Number.isNaN(requestedChatId) && requestedChatId > 0) {
          const exists = normalized.some((chat) => chat.id === requestedChatId)
          if (exists) return requestedChatId
        }
        return null
      })
    } catch (error) {
      console.error("Failed to load chats", error)
      setPageError("Failed to load your conversations.")
      toast.error("Failed to load your conversations.")
    } finally {
      setLoadingChats(false)
    }
  }, [emitGlobalUnread, requestedChatId])

  const loadMessages = useCallback(async (chatId: number) => {
    setLoadingMessages(true)
    try {
      const response = await getChatMessages(chatId)
      setMessages(sortMessages(response))
      await markMessagesAsRead(chatId)
      if (typeof window !== "undefined") {
        window.dispatchEvent(
          new CustomEvent(MESSAGE_UNREAD_COUNT_CHANGE_EVENT, {
            detail: { reset: true },
          })
        )
      }
      markActiveMessagesAsReadLocally()
      setChats((prev) => {
        const updated = prev.map((chat) => (chat.id === chatId ? { ...chat, unreadCount: 0 } : chat))
        emitGlobalUnread(updated)
        return updated
      })
    } catch (error) {
      console.error("Failed to load messages", error)
      toast.error("Failed to load messages.")
    } finally {
      setLoadingMessages(false)
    }
  }, [emitGlobalUnread, markActiveMessagesAsReadLocally])

  useEffect(() => {
    if (status === "authenticated") {
      if (typeof window !== "undefined") {
        window.dispatchEvent(
          new CustomEvent(MESSAGE_UNREAD_COUNT_CHANGE_EVENT, {
            detail: { reset: true },
          })
        )
      }
      void loadChats()
    }
  }, [status, loadChats])

  useEffect(() => {
    if (!Number.isNaN(requestedChatId) && requestedChatId > 0) {
      setActiveChatId(requestedChatId)
    }
  }, [requestedChatId])

  useEffect(() => {
    if (!activeChatId || status !== "authenticated") {
      setMessages([])
      return
    }
    void loadMessages(activeChatId)
  }, [activeChatId, status, loadMessages])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth", block: "end" })
  }, [messages])

  const filteredChats = useMemo(() => {
    const query = chatSearch.trim().toLowerCase()
    if (!query) return chats

    return chats.filter((chat) => {
      const name = getChatDisplayName(chat, currentUserId).toLowerCase()
      const lastMessage = chat.lastMessage?.content?.toLowerCase() ?? ""
      return name.includes(query) || lastMessage.includes(query)
    })
  }, [chatSearch, chats, currentUserId])

  const activeChat = useMemo(
    () => chats.find((chat) => chat.id === activeChatId) ?? null,
    [chats, activeChatId]
  )

  const activeChatName = useMemo(() => {
    if (!activeChat) return ""
    return getChatDisplayName(activeChat, currentUserId)
  }, [activeChat, currentUserId])

  const activeChatAvatar = useMemo(() => {
    if (!activeChat) return "CV"
    const other = activeChat.participants.find((participant) => participant.userId !== currentUserId)
    return getParticipantInitials(other ?? activeChat.participants[0])
  }, [activeChat, currentUserId])

  const lastOwnMessageId = useMemo(() => {
    for (let i = messages.length - 1; i >= 0; i -= 1) {
      if (messages[i].senderId === currentUserId) return messages[i].id
    }
    return null
  }, [messages, currentUserId])

  const handleSelectChat = useCallback((chatId: number) => {
    setActiveChatId(chatId)
    setPageError(null)
  }, [])

  useEffect(() => {
    const query = chatSearch.trim()
    if (!query) {
      setUserSearchResults([])
      setLoadingUserSearch(false)
      return
    }

    const timeoutId = window.setTimeout(() => {
      void (async () => {
        try {
          setLoadingUserSearch(true)
          const response = await searchUsers(query, 1, 6)
          setUserSearchResults(
            response.items.filter((item) => item.id && item.id !== currentUserId)
          )
        } catch {
          setUserSearchResults([])
        } finally {
          setLoadingUserSearch(false)
        }
      })()
    }, 300)

    return () => {
      window.clearTimeout(timeoutId)
    }
  }, [chatSearch, currentUserId])

  const handleStartPrivateChat = useCallback(async (targetUser: UserSearchDto) => {
    if (!targetUser.id) return
    if (targetUser.id === currentUserId) {
      toast.error("You cannot message yourself.")
      return
    }

    setCreatingPrivateChatFor(targetUser.id)
    try {
      const chat = await createPrivateChat(targetUser.id)
      setChats((prev) => {
        const exists = prev.some((item) => item.id === chat.id)
        const next = exists
          ? prev.map((item) => (item.id === chat.id ? { ...item, ...chat } : item))
          : [chat, ...prev]
        const sorted = next.sort((a, b) => {
          const aTime = new Date(a.lastMessage?.createdAt ?? a.createdAt).getTime()
          const bTime = new Date(b.lastMessage?.createdAt ?? b.createdAt).getTime()
          return bTime - aTime
        })
        emitGlobalUnread(sorted)
        return sorted
      })
      setActiveChatId(chat.id)
      setChatSearch("")
      setUserSearchResults([])
    } catch (error) {
      console.error("Failed to start private chat", error)
      toast.error("Failed to start chat.")
    } finally {
      setCreatingPrivateChatFor(null)
    }
  }, [currentUserId, emitGlobalUnread])

  const handleSend = useCallback(async () => {
    if (!activeChatId || sending) return
    const content = messageInput.trim()
    if (!content) return

    setSending(true)
    setMessageInput("")

    try {
      if (isConnected) {
        await sendMessageToChat(activeChatId, content)
      } else {
        const created = await sendMessage(activeChatId, content)
        mergeIncomingMessage(created)
      }
    } catch (error) {
      console.error("Failed to send message", error)
      setMessageInput(content)
      toast.error("Failed to send message.")
    } finally {
      setSending(false)
    }
  }, [activeChatId, isConnected, mergeIncomingMessage, messageInput, sendMessageToChat, sending])

  const handleComposerKeyDown = useCallback(
    (event: KeyboardEvent<HTMLTextAreaElement>) => {
      if (event.key !== "Enter") return
      if (event.shiftKey) return
      event.preventDefault()
      void handleSend()
    },
    [handleSend]
  )

  if (status === "loading") {
    return (
      <div className="mx-auto max-w-7xl px-4 py-6">
        <div className="grid min-h-[calc(100vh-140px)] grid-cols-1 gap-4 lg:grid-cols-[360px_minmax(0,1fr)]">
          <Card className="border-white/10 bg-[#0c1527]/95 p-4">
            <Skeleton className="h-10 w-full rounded-xl" />
            <div className="mt-4 space-y-3">
              <Skeleton className="h-20 w-full rounded-2xl" />
              <Skeleton className="h-20 w-full rounded-2xl" />
              <Skeleton className="h-20 w-full rounded-2xl" />
            </div>
          </Card>
          <Card className="border-white/10 bg-[#0c1527]/95 p-4">
            <Skeleton className="h-full min-h-[500px] w-full rounded-2xl" />
          </Card>
        </div>
      </div>
    )
  }

  if (status !== "authenticated") {
    return (
      <div className="mx-auto max-w-4xl px-4 py-12">
        <Card className="border-white/10 bg-[#0c1527]/95 p-8 text-center text-sm text-slate-300">
          Sign in to view your messages.
        </Card>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      <div className="mb-4 flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-white">Messages</h1>
          <p className="text-sm text-slate-400">Stay connected with other CineVerse users in real time.</p>
        </div>
        <div
          className={cn(
            "inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-xs font-medium",
            connectionStatus === "connected"
              ? "border-emerald-500/30 bg-emerald-500/10 text-emerald-300"
              : connectionStatus === "reconnecting"
              ? "border-amber-500/30 bg-amber-500/10 text-amber-200"
              : connectionStatus === "failed"
              ? "border-red-500/30 bg-red-500/10 text-red-200"
              : "border-amber-500/30 bg-amber-500/10 text-amber-200"
          )}
        >
          {connectionStatus === "connected" ? <Wifi className="size-3.5" /> : <WifiOff className="size-3.5" />}
          {connectionStatus === "connected"
            ? "Connected"
            : connectionStatus === "reconnecting"
            ? "Reconnecting"
            : connectionStatus === "failed"
            ? "Failed"
            : "Disconnected"}
        </div>
      </div>

      <div className="grid h-[calc(100vh-180px)] grid-cols-1 gap-4 lg:grid-cols-[360px_minmax(0,1fr)]">
        <Card className="overflow-hidden border-white/10 bg-[#09111f]/95 py-0 shadow-[0_18px_50px_rgba(0,0,0,0.35)]">
          <div className="border-b border-white/10 p-4">
            <div className="relative">
              <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-500" />
              <Input
                value={chatSearch}
                onChange={(event) => setChatSearch(event.target.value)}
                placeholder="Search users to start chat"
                className="h-11 rounded-xl border-white/10 bg-white/5 pl-10 text-slate-100 placeholder:text-slate-500"
              />
            </div>
            {chatSearch.trim() ? (
              <div className="mt-3 rounded-xl border border-white/10 bg-white/[0.03] p-2">
                {loadingUserSearch ? (
                  <div className="px-2 py-2 text-xs text-slate-400">Searching users...</div>
                ) : userSearchResults.length === 0 ? (
                  <div className="px-2 py-2 text-xs text-slate-400">No users found</div>
                ) : (
                  <div className="space-y-1">
                    {userSearchResults.map((item) => (
                      <button
                        key={item.id}
                        type="button"
                        onClick={() => void handleStartPrivateChat(item)}
                        disabled={creatingPrivateChatFor === item.id}
                        className="flex w-full items-center justify-between rounded-lg px-2 py-2 text-left transition hover:bg-white/10 disabled:opacity-60"
                      >
                        <div className="min-w-0">
                          <div className="truncate text-sm font-medium text-white">
                            {item.fullName?.trim() || item.userName}
                          </div>
                          <div className="truncate text-xs text-slate-400">
                            @{item.userName}{item.email ? ` • ${item.email}` : ""}
                          </div>
                        </div>
                        <div className="ml-2 rounded-md bg-[#81D8D0]/20 px-2 py-1 text-[11px] font-semibold text-[#81D8D0]">
                          {creatingPrivateChatFor === item.id ? "Opening..." : "Message"}
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            ) : null}
          </div>

          <ScrollArea className="h-[calc(100vh-270px)]">
            <div className="space-y-2 p-3">
              {loadingChats ? (
                <>
                  <Skeleton className="h-20 rounded-2xl bg-white/10" />
                  <Skeleton className="h-20 rounded-2xl bg-white/10" />
                  <Skeleton className="h-20 rounded-2xl bg-white/10" />
                </>
              ) : filteredChats.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-white/10 bg-white/[0.03] p-6 text-center text-sm text-slate-400">
                  {chats.length === 0 ? "No conversations yet. Search for a user above to start chatting." : "No chats match your search."}
                </div>
              ) : (
                filteredChats.map((chat) => {
                  const displayName = getChatDisplayName(chat, currentUserId)
                  const otherParticipant =
                    chat.participants.find((participant) => participant.userId !== currentUserId) ??
                    chat.participants[0]
                  const unreadCount = Number(
                    (chat as unknown as Record<string, unknown>).unreadCount ??
                      (chat as unknown as Record<string, unknown>).UnreadCount ??
                      0
                  )
                  console.log("chat unread:", chat.id, unreadCount)

                  return (
                    <button
                      key={chat.id}
                      type="button"
                      onClick={() => handleSelectChat(chat.id)}
                      className={cn(
                        "w-full rounded-2xl border p-4 text-left transition",
                        activeChatId === chat.id
                          ? "border-[#81D8D0]/60 bg-[#81D8D0]/12 shadow-[0_0_0_1px_rgba(129,216,208,0.18)]"
                          : (chat.unreadCount ?? 0) > 0
                          ? "border-white/20 bg-white/[0.08] hover:border-white/25 hover:bg-white/[0.1]"
                          : "border-white/8 bg-white/[0.03] hover:border-white/15 hover:bg-white/[0.05]"
                      )}
                    >
                      <div className="flex items-start gap-3">
                        <div className="flex size-11 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-[#81D8D0] to-[#5bbab2] text-sm font-semibold text-slate-950">
                          {getParticipantInitials(otherParticipant)}
                        </div>

                        <div className="min-w-0 flex-1">
                          <div className="flex items-center justify-between gap-3">
                            <div className={cn("truncate text-white", (chat.unreadCount ?? 0) > 0 ? "font-semibold" : "font-medium")}>
                              {displayName}
                            </div>
                            <div className="flex shrink-0 items-center gap-2">
                              <span className="text-[11px] text-slate-500">
                                {formatChatPreviewTime(chat.lastMessage?.createdAt ?? chat.createdAt)}
                              </span>
                              {unreadCount > 0 ? (
                                <span className="ml-2 flex h-5 min-w-[20px] items-center justify-center rounded-full bg-red-500 px-1.5 text-xs font-bold text-white">
                                  {unreadCount > 9 ? "9+" : unreadCount}
                                </span>
                              ) : null}
                            </div>
                          </div>
                          <div className="mt-1 truncate text-sm text-slate-400">
                            <span className={cn((chat.unreadCount ?? 0) > 0 && "font-semibold text-slate-200")}>
                              {chat.lastMessage?.content ?? "No messages yet"}
                            </span>
                          </div>
                        </div>

                      </div>
                    </button>
                  )
                })
              )}
            </div>
          </ScrollArea>
        </Card>

        <Card className="flex h-full min-h-0 flex-col overflow-hidden border-white/10 bg-[#0c1527]/95 py-0 shadow-[0_18px_50px_rgba(0,0,0,0.35)]">
          <div className="flex items-center justify-between gap-3 border-b border-white/10 px-5 py-4">
            {activeChat ? (
              <>
                <div className="flex min-w-0 items-center gap-3">
                  <div className="flex size-12 items-center justify-center rounded-full bg-gradient-to-br from-[#81D8D0] to-[#5bbab2] text-sm font-semibold text-slate-950">
                    {activeChatAvatar}
                  </div>
                  <div className="min-w-0">
                    <div className="truncate font-medium text-white">{activeChatName}</div>
                    <div className="text-xs text-slate-400">
                      {isConnected ? "Connected to chat" : "Messages will sync when connection returns"}
                    </div>
                  </div>
                </div>
                <div className="text-xs text-slate-500">Chat #{activeChat.id}</div>
              </>
            ) : (
              <div>
                <div className="font-medium text-white">Select a conversation</div>
                <div className="text-xs text-slate-400">Choose a chat from the left sidebar to start messaging.</div>
              </div>
            )}
          </div>

          <div className="relative flex min-h-0 flex-1 flex-col">
            <ScrollArea className="min-h-0 flex-1">
              <div className="space-y-4 p-5">
                {pageError ? (
                  <div className="rounded-2xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
                    {pageError}
                  </div>
                ) : null}
                {connectionError ? (
                  <div className="rounded-2xl border border-amber-500/20 bg-amber-500/10 px-4 py-3 text-sm text-amber-200">
                    {connectionError}
                  </div>
                ) : null}

                {!activeChat ? (
                  <div className="flex min-h-[420px] items-center justify-center">
                    <div className="max-w-sm text-center">
                      <div className="text-lg font-medium text-white">No chat selected</div>
                      <p className="mt-2 text-sm text-slate-400">
                        Pick a conversation to view messages and chat in real time.
                      </p>
                    </div>
                  </div>
                ) : loadingMessages ? (
                  <div className="space-y-3">
                    <Skeleton className="h-16 w-2/3 rounded-2xl bg-white/10" />
                    <Skeleton className="ml-auto h-16 w-1/2 rounded-2xl bg-white/10" />
                    <Skeleton className="h-16 w-3/4 rounded-2xl bg-white/10" />
                  </div>
                ) : messages.length === 0 ? (
                  <div className="flex min-h-[420px] items-center justify-center">
                    <div className="max-w-sm text-center">
                      <div className="text-lg font-medium text-white">No messages yet</div>
                      <p className="mt-2 text-sm text-slate-400">
                        Start the conversation by sending your first message.
                      </p>
                    </div>
                  </div>
                ) : (
                  messages.map((message) => {
                    const isMine =
                      message.senderId === currentUserId ||
                      (message as unknown as Record<string, unknown>).SenderId === currentUserId
                    const isRead = Boolean(
                      (message as unknown as Record<string, unknown>).isRead ??
                        (message as unknown as Record<string, unknown>).IsRead ??
                        false
                    )
                    console.log(
                      "message read:",
                      message.id,
                      (message as unknown as Record<string, unknown>).isRead,
                      (message as unknown as Record<string, unknown>).IsRead
                    )

                    return (
                      <div key={message.id} className={cn("flex", isMine ? "justify-end" : "justify-start")}>
                        <div
                          className={cn(
                            "max-w-[85%] rounded-[22px] px-4 py-3 sm:max-w-[70%]",
                            isMine
                              ? "bg-[#81D8D0] text-slate-950"
                              : "border border-white/10 bg-white/[0.05] text-slate-100"
                          )}
                        >
                          {!isMine && message.senderName ? (
                            <div className="mb-1 text-[11px] font-medium uppercase tracking-wide text-[#81D8D0]">
                              {message.senderName}
                            </div>
                          ) : null}
                          <div className="whitespace-pre-wrap break-words text-sm leading-6">{message.content}</div>
                          <div
                            className={cn(
                              "mt-2 text-right text-[11px]",
                              isMine ? "text-xs text-slate-700/80" : "text-slate-500"
                            )}
                          >
                            {formatMessageTime(message.createdAt)}
                          </div>
                          {isMine ? (
                            <span
                              className={cn(
                                "mt-1 block text-right text-[11px] font-semibold",
                                isRead ? "text-emerald-900" : "text-slate-700"
                              )}
                            >
                              {isRead ? "Seen ✓✓" : "Sent ✓"}
                            </span>
                          ) : null}
                        </div>
                      </div>
                    )
                  })
                )}
                <div ref={messagesEndRef} />
              </div>
            </ScrollArea>

            {activeChat ? (
              <div className="sticky bottom-0 z-10 border-t border-white/10 bg-[#0a1222]/95 px-4 py-3 backdrop-blur">
                <form
                  onSubmit={(event) => {
                    event.preventDefault()
                    void handleSend()
                  }}
                  className="flex items-end gap-2"
                >
                  <Textarea
                    value={messageInput}
                    onChange={(event) => setMessageInput(event.target.value)}
                    onKeyDown={handleComposerKeyDown}
                    placeholder="Type your message..."
                    disabled={sending}
                    rows={1}
                    className="max-h-32 min-h-12 resize-y rounded-2xl border-white/10 bg-white/5 text-slate-100 placeholder:text-slate-500"
                  />
                  <Button
                    type="submit"
                    disabled={sending || !messageInput.trim()}
                    className="mb-1 h-10 rounded-xl bg-[#81D8D0] px-4 text-slate-950 hover:bg-[#6fd0c7]"
                  >
                    <Send className="size-4" />
                    {sending ? "Sending..." : "Send"}
                  </Button>
                </form>
              </div>
            ) : null}
          </div>
        </Card>
      </div>
    </div>
  )
}
