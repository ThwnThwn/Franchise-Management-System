import { Order } from "@/features/orders/models/response"
import { Voucher } from "@/features/vouchers/models/response"
import { checkVoucherEligibility } from "@/features/vouchers/utils"
import { VoucherSelectionItem } from "../VoucherSelectionItem"

interface Props {
    vouchers: Voucher[]
    order?: Order
    selectedVoucher?: Voucher
    searchQuery: string
    onSelect: (voucher: Voucher) => void
}

export function VoucherList({ vouchers, order, selectedVoucher, searchQuery, onSelect }: Props) {
    if (vouchers.length === 0) {
        return (
            <div className="text-center py-10 text-default-400 italic">
                {searchQuery ? "Không tìm thấy mã phù hợp." : "Bạn không có mã nào trong mục này."}
            </div>
        )
    }

    // Đưa các voucher khả dụng (Eligible) lên trên đầu
    const sortedList = [...vouchers].sort((a, b) => {
        const aEligible = checkVoucherEligibility(a, order).isEligible ? 1 : 0
        const bEligible = checkVoucherEligibility(b, order).isEligible ? 1 : 0
        return bEligible - aEligible
    })

    return (
        <div className="flex flex-col gap-3 pb-4">
            {sortedList.map((voucher) => {
                const { isEligible, reason } = checkVoucherEligibility(voucher, order)
                return (
                    <VoucherSelectionItem
                        key={voucher.id}
                        voucher={voucher}
                        isEligible={isEligible}
                        reason={reason}
                        isSelected={selectedVoucher?.id === voucher.id}
                        onSelect={onSelect}
                    />
                )
            })}
        </div>
    )
}
