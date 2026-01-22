import { cn } from "@heroui/react"
import { FileText } from "lucide-react"
import React from "react"

export function SectionStyled({
    title,
    children,
    sectionClassName = "",
    childrenClassName = "",
    icon: Icon = FileText
}: {
    title: string
    children: React.ReactNode
    sectionClassName?: string
    childrenClassName?: string
    icon?: React.ElementType
}) {
    return (
        <div className={cn("mb-10", sectionClassName)}>
            <h3 className="w-fit text-xl font-semibold text-gray-800 dark:text-white mb-3 flex items-center gap-2">
                <Icon size={20} className="text-primary" /> {title}
            </h3>
            <div className={cn(childrenClassName)}>{children}</div>
        </div>
    )
}
