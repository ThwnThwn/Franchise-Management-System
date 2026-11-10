"use client"

import { cn } from "@heroui/react"
import { ChevronRight, LucideIcon } from "lucide-react"

interface SecurityRowProps {
    icon: LucideIcon
    title: string
    description: string
    onClick?: () => void
}

export const SecurityRow = ({ icon: Icon, title, description, onClick }: SecurityRowProps) => {
    return (
        <div
            onClick={onClick}
            className={cn(
                "group flex items-center gap-4 p-4 rounded-2xl cursor-pointer",
                "bg-transparent border border-transparent transition-all duration-300",
                "hover:bg-default-100/50 active:scale-[0.98]" // Chỉ tạo hiệu ứng hover nhẹ
            )}
        >
            {/* ICON - Không nền, chữ đen */}
            <div className="text-default-900">
                <Icon size={22} strokeWidth={2.5} />
            </div>

            {/* TEXT - Đen hết */}
            <div className="flex-1">
                <h4 className="text-sm font-black text-default-900 uppercase tracking-tight">
                    {title}
                </h4>
                <p className="text-xs text-default-600 font-medium leading-relaxed">
                    {description}
                </p>
            </div>

            {/* ARROW - Đen nhẹ, hover sẽ đậm */}
            <ChevronRight
                size={18}
                className="text-default-300 group-hover:text-default-900 transition-colors"
            />
        </div>
    )
}
