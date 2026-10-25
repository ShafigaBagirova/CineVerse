"use client"

import { useState } from "react"
import { Smile } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { cn } from "@/lib/utils"

const EMOJI_GROUPS = [
  {
    name: "Smileys",
    icon: "😀",
    emojis: [
      "😀", "😃", "😄", "😁", "😆", "😅", "😂", "🤣", "😊", "😇",
      "🙂", "😉", "😍", "🥰", "😘", "😋", "😎", "🤩", "🥳", "😏",
      "😒", "😔", "😢", "😭", "😡", "😱", "😴", "🤔", "🙄", "😬",
    ],
  },
  {
    name: "Gestures",
    icon: "👍",
    emojis: ["👍", "👎", "👏", "🙌", "🙏", "💪", "👌", "✌️", "🤞", "🤝", "👋", "🤙", "🫶", "👀"],
  },
  {
    name: "Hearts",
    icon: "❤️",
    emojis: ["❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "💔", "💖", "💕", "💯", "🔥", "✨"],
  },
  {
    name: "Fun",
    icon: "🍿",
    emojis: [
      "🎬", "🍿", "📺", "🎥", "🎞️", "🎭", "🎉", "🎊", "🎵", "🎶",
      "😈", "👻", "💀", "🤯", "🙈", "🚀", "⭐", "🌟", "🏆", "☕",
    ],
  },
]

type EmojiPickerProps = {
  onSelect: (emoji: string) => void
  disabled?: boolean
}

export function EmojiPicker({ onSelect, disabled }: EmojiPickerProps) {
  const [open, setOpen] = useState(false)
  const [groupIndex, setGroupIndex] = useState(0)
  const group = EMOJI_GROUPS[groupIndex]

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button type="button" size="icon" variant="ghost" disabled={disabled} aria-label="Add emoji">
          <Smile className="h-5 w-5" />
        </Button>
      </PopoverTrigger>
      <PopoverContent align="start" side="top" className="w-72 p-2">
        <div className="mb-2 flex gap-1 border-b pb-2">
          {EMOJI_GROUPS.map((item, index) => (
            <button
              key={item.name}
              type="button"
              title={item.name}
              onClick={() => setGroupIndex(index)}
              className={cn(
                "flex h-8 w-8 items-center justify-center rounded text-lg transition hover:bg-muted",
                index === groupIndex && "bg-muted"
              )}
            >
              {item.icon}
            </button>
          ))}
        </div>
        <div className="grid grid-cols-8 gap-1">
          {group.emojis.map((emoji) => (
            <button
              key={emoji}
              type="button"
              onClick={() => onSelect(emoji)}
              className="flex h-8 w-8 items-center justify-center rounded text-xl transition hover:bg-muted"
            >
              {emoji}
            </button>
          ))}
        </div>
      </PopoverContent>
    </Popover>
  )
}
