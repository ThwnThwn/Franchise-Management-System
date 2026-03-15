import { voucherApi } from "@/features/vouchers/services"
import VoucherMarketplacePageView from "@/features/vouchers/pages/VoucherMarketplacePageView"

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

    return <VoucherMarketplacePageView initialVouchers={response || []} />
}
