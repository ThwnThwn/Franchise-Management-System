import React from "react"
import { CardStyled } from "@/components"
import { Chip } from "@heroui/react"
import { ShieldUser, Calendar, Fingerprint } from "lucide-react"
import { RoleResponse } from "../../models/respone"
import { useDay } from "@/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"

interface RoleCardProps {
    role: RoleResponse
    // onPress?: (role: RoleResponse) => void
}

export function RoleCard({ role }: RoleCardProps) {
    const { formatDateTime } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })

    return (
        <CardStyled
            className="p-5 hover:border-primary/50 transition-all cursor-pointer group border-2 border-transparent"
            // onClick={() => onPress?.(role)}
        >
            <div className="flex flex-col h-full gap-4">
                {/* Header: Name & Icon */}
                <div className="flex justify-between items-start">
                    <div className="flex items-center gap-3">
                        <div className="p-2.5 bg-primary-50 rounded-xl text-primary group-hover:bg-primary group-hover:text-white transition-colors">
                            <ShieldUser size={24} />
                        </div>
                        <div>
                            <h3 className="font-bold text-lg text-slate-800 leading-tight">
                                {role.name}
                            </h3>
                            <div className="flex items-center gap-1 text-slate-400 mt-0.5">
                                <Fingerprint size={12} />
                                <span className="text-[10px] font-mono uppercase tracking-wider">
                                    {role.slug}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* <Tooltip content="Chỉnh sửa vai trò">
                        <div className="opacity-0 group-hover:opacity-100 transition-opacity">
                            <ButtonIconStyled
                                isIconOnly
                                size="sm"
                                variant="light"
                                onPress={() => onPress?.(role)}
                            >
                                <Edit3 size={18} />
                            </ButtonIconStyled>
                        </div>
                    </Tooltip> */}
                </div>

                {/* Body: Description */}
                <p className="text-sm text-slate-500 line-clamp-2 min-h-10">
                    {role.description || "Chưa có mô tả cho vai trò này."}
                </p>

                {/* Footer: Date Info */}
                <div className="mt-auto pt-4 border-t border-slate-100 flex items-center justify-between">
                    <div className="flex items-center gap-1.5 text-slate-400">
                        <Calendar size={14} />
                        <span className="text-[11px]">
                            {role.createdAt
                                ? formatDateTime({
                                      date: role.createdAt
                                  })
                                : "---"}
                        </span>
                    </div>

                    <Chip
                        size="sm"
                        variant="dot"
                        color="primary"
                        className="border-none bg-primary-50/50 text-[10px] h-6"
                    >
                        Active
                    </Chip>
                </div>
            </div>
        </CardStyled>
    )
}
