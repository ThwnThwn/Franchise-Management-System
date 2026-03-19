import { voucherApi } from "@/features/vouchers/services"
import VoucherMarketplacePageView from "@/features/vouchers/pages/VoucherMarketplacePageView"
import { getQueryClient } from "@/utils/helpers/getQueryClient"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { campaignApi } from "@/features/campaigns/services"
import { dehydrate, HydrationBoundary } from "@tanstack/react-query"

export const revalidate = 60

async function getMarketplaceVouchers() {
    try {
        return await voucherApi.getAll()
    } catch {
        return []
    }
}

export default async function VoucherMarketplacePage() {
    const response = await getMarketplaceVouchers()

    const queryClient = getQueryClient()

    await queryClient.prefetchQuery({
        queryKey: [QUERY_KEYS.CAMPAIGNS, "active"],
        queryFn: () => campaignApi.getActive()
    })

    return (
        <HydrationBoundary state={dehydrate(queryClient)}>
            <VoucherMarketplacePageView initialVouchers={response || []} />
        </HydrationBoundary>
    )
}
