import FeaturedSection from "@/components/home/FeaturedSection"
import HeroSection from "@/components/home/HeroSection"
import StoreHighlight from "@/components/home/StoreHighlight"
import CampaignSection from "@/features/campaigns/components/CampaignSection"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { campaignApi } from "@/features/campaigns/services"
import { getQueryClient } from "@/utils/helpers/getQueryClient"
import { dehydrate, HydrationBoundary } from "@tanstack/react-query"

export default async function Home() {
    const queryClient = getQueryClient()

    await queryClient.prefetchQuery({
        queryKey: [QUERY_KEYS.CAMPAIGNS, "active"],
        queryFn: () => campaignApi.getActive()
    })

    return (
        <HydrationBoundary state={dehydrate(queryClient)}>
            <div className="flex flex-col w-full">
                <HeroSection />
                <CampaignSection className="py-8" />
                <FeaturedSection />
                <StoreHighlight />
            </div>
        </HydrationBoundary>
    )
}
