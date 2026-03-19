"use client"

import { Card, CardBody, Progress } from "@heroui/react"
import clsx from "clsx"
import { ArrowRight, CalendarDays, Megaphone, PartyPopper, Sparkles } from "lucide-react"

import { DATE_TIME_VIEW_FORMAT } from "@/constants/constants"
import { CampaignResponse } from "@/features/campaigns/models/response"
import { useCountdown, useDay } from "@/hooks"
import dayjs from "dayjs"

// --- Định nghĩa các style Banner theo size ---
const SIZES = {
    sm: {
        card: "p-4 min-h-[140px]",
        title: "text-xl",
        desc: "text-xs line-clamp-1 mt-1",
        dates: "text-[11px] gap-1",
        iconBig: 48,
        iconSmall: 14,
        countdown: false
    },
    lg: {
        card: "p-8 min-h-[220px]",
        title: "text-3xl font-black uppercase tracking-tighter leading-none",
        desc: "text-sm mt-2 line-clamp-2 max-w-[70%]",
        dates: "text-xs gap-1.5 mt-4",
        iconBig: 100,
        iconSmall: 16,
        countdown: true
    }
}

const THEMES = {
    gold: {
        gradient: "from-amber-400 via-warning-500 to-amber-600",
        text: "text-white",
        desc: "text-amber-100",
        dates: "text-warning-100",
        icon: Megaphone
    },
    christmas: {
        gradient: "from-green-600 via-danger-600 to-green-700",
        text: "text-white",
        desc: "text-green-100",
        dates: "text-danger-100",
        icon: PartyPopper
    }
}

interface Props {
    campaign: CampaignResponse
    size?: keyof typeof SIZES
    theme?: keyof typeof THEMES
    onPress?: () => void
    className?: string
}

export const CampaignBanner = ({
    campaign,
    size = "lg",
    theme = "gold",
    onPress,
    className
}: Props) => {
    const { formatDateTime } = useDay({ defaultFormat: DATE_TIME_VIEW_FORMAT })
    const style = SIZES[size]
    const themeStyle = THEMES[theme]
    const ThemeIcon = themeStyle.icon

    // Hook tự động lo tính toán (không cần isMounted nữa)
    const { days, hours, minutes, seconds } = useCountdown(campaign.endDate)

    // Tính toán Progress Bar
    const totalDuration = dayjs(campaign.endDate).diff(dayjs(campaign.startDate))
    const elapsed = dayjs().diff(dayjs(campaign.startDate))
    const progressValue = Math.min(Math.max((elapsed / totalDuration) * 100, 0), 100)

    return (
        <Card
            shadow="sm"
            isPressable={!!onPress}
            onPress={onPress}
            className={clsx(
                `bg-linear-to-br ${themeStyle.gradient} border border-default-100/30`,
                "rounded-3xl shadow-lg transition-transform hover:scale-[1.01] active:scale-[0.99]",
                style.card,
                className
            )}
        >
            <CardBody className="p-0 grid grid-cols-12 gap-0 items-center overflow-hidden">
                {/* --- CỘT TRÁI (Nội dung chữ) --- */}
                <div className="col-span-8 flex flex-col h-full justify-between">
                    <div>
                        <h2
                            className={clsx(
                                "font-black text-default-900 leading-tight",
                                themeStyle.text,
                                style.title
                            )}
                        >
                            {campaign.name}
                        </h2>
                        <p className={clsx("font-medium", themeStyle.desc, style.desc)}>
                            {campaign.description || "Hàng ngàn voucher đang chờ!"}
                        </p>
                    </div>

                    <div
                        className={clsx(
                            "flex flex-wrap items-center font-bold",
                            themeStyle.dates,
                            style.dates
                        )}
                    >
                        <CalendarDays size={style.iconSmall} />
                        {campaign.startDate ? formatDateTime({ date: campaign.startDate }) : "---"}
                        <ArrowRight size={style.iconSmall} className="opacity-50" />
                        {campaign.endDate
                            ? formatDateTime({ date: campaign.endDate })
                            : "Vô thời hạn"}
                    </div>
                </div>

                {/* --- CỘT PHẢI (Visual & Countdown) --- */}
                <div className="col-span-4 h-full flex flex-col items-end justify-center relative pr-2">
                    {size === "lg" && (
                        <div className="bg-white/20 p-4 rounded-2xl backdrop-blur-md border border-white/30 w-full max-w-55 z-10">
                            <p className="text-[10px] font-black text-white/90 uppercase mb-2 flex items-center gap-1">
                                <Sparkles size={12} /> Kết thúc sau:
                            </p>

                            <div className="flex justify-between items-baseline text-white">
                                {/* Khối Ngày */}
                                <div className="text-center min-w-8">
                                    {/* THÊM suppressHydrationWarning VÀO ĐÂY */}
                                    <span suppressHydrationWarning className="text-2xl font-black">
                                        {days.toString().padStart(2, "0")}
                                    </span>
                                    <p className="text-[9px] uppercase font-bold opacity-70 mt-1">
                                        Ngày
                                    </p>
                                </div>

                                <span className="text-xl font-black mb-4 opacity-50">:</span>

                                {/* Khối Giờ */}
                                <div className="text-center min-w-8">
                                    <span suppressHydrationWarning className="text-2xl font-black">
                                        {hours.toString().padStart(2, "0")}
                                    </span>
                                    <p className="text-[9px] uppercase font-bold opacity-70 mt-1">
                                        Giờ
                                    </p>
                                </div>

                                <span className="text-xl font-black mb-4 opacity-50">:</span>

                                {/* Khối Phút */}
                                <div className="text-center min-w-8">
                                    <span suppressHydrationWarning className="text-2xl font-black">
                                        {minutes.toString().padStart(2, "0")}
                                    </span>
                                    <p className="text-[9px] uppercase font-bold opacity-70 mt-1">
                                        Phút
                                    </p>
                                </div>

                                <span className="text-xl font-black mb-4 opacity-50 animate-pulse">
                                    :
                                </span>

                                {/* Khối Giây */}
                                <div className="text-center min-w-8">
                                    <span
                                        suppressHydrationWarning
                                        className="text-2xl font-black text-danger-200"
                                    >
                                        {seconds.toString().padStart(2, "0")}
                                    </span>
                                    <p className="text-[9px] uppercase font-bold opacity-70 mt-1">
                                        Giây
                                    </p>
                                </div>
                            </div>

                            <Progress
                                size="sm"
                                radius="full"
                                value={progressValue}
                                className="mt-4"
                                classNames={{
                                    indicator: "bg-white",
                                    track: "bg-white/20"
                                }}
                            />
                        </div>
                    )}

                    {/* Icon nền */}
                    <ThemeIcon
                        size={style.iconBig}
                        strokeWidth={1.5}
                        className={clsx(
                            "absolute opacity-15 rotate-25",
                            "lg:-right-5 lg:-bottom-5 sm:-right-2.5 sm:-bottom-2.5",
                            themeStyle.text
                        )}
                    />
                </div>
            </CardBody>
        </Card>
    )
}
