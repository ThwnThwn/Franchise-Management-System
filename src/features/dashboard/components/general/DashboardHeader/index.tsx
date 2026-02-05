import { Coffee } from "lucide-react"
import React from "react"

export function DashboardHeader({
    icon = <Coffee className="w-6 h-6 text-white" />,
    title,
    description
}: {
    icon?: React.ReactNode
    title: string
    description: string
}) {
    return (
        <div className="flex items-center gap-3 mb-6">
            <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                {icon}
            </div>
            <div>
                <h1 className="text-2xl font-black text-slate-800">{title}</h1>
                <p className="text-slate-500 text-sm">{description}</p>
            </div>
        </div>
    )
}
