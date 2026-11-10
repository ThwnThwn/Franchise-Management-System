import React from "react"
export function FieldStyled({
    label,
    value,
    icon
}: {
    label: string
    value: string
    icon?: React.ReactNode
}) {
    return (
        <div className="flex items-start gap-3 rounded-xl border border-neutral-200 p-3">
            {icon && (
                <div className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600">
                    {icon}
                </div>
            )}
            <div>
                <p className="text-xs uppercase tracking-wide text-neutral-500">{label}</p>
                <p className="mt-1 font-medium">{value}</p>
            </div>
        </div>
    )
}
