import { voucherApi } from "@/features/vouchers/services"
import VoucherMarketplacePageView from "@/features/vouchers/pages/VoucherMarketplacePageView"

export default async function VoucherMarketplacePage() {
    const response = await voucherApi.getAll()

    return <VoucherMarketplacePageView initialVouchers={response || []} />
}
