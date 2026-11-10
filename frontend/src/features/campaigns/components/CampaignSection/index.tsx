"use client"

import { useGetActiveCampaigns } from "@/features/campaigns/hooks/useCampaigns"
import { CampaignCarousel } from "../CampaignCarousel"
import { Spinner } from "@heroui/react"

export default function CampaignSection({ className }: { className?: string }) {
    const { data: campaigns = [], isLoading } = useGetActiveCampaigns()

    if (isLoading) {
        return (
            <div className="w-full py-20 flex justify-center">
                <Spinner label="Đang quét..." color="primary" />
            </div>
        )
    }

    if (campaigns.length === 0) return null

    return (
        <section className={className}>
            <div className="container mx-auto">
                <div className="px-6 mb-2 flex items-baseline gap-3">
                    <h2 className="text-3xl font-black text-default-900 tracking-tighter uppercase">
                        Sự kiện
                    </h2>
                    <span className="text-sm font-bold text-primary-500 animate-pulse">
                        • LIVE NOW
                    </span>
                </div>

                <CampaignCarousel campaigns={campaigns} autoPlayInterval={6000} />
            </div>
        </section>
    )
}
