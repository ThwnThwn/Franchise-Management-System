"use client"

import { Card, CardBody, Divider } from "@heroui/react"
import { Info, Landmark, ReceiptText, Percent, Hourglass } from "lucide-react"
import { FRANCHISE_POLICY } from "@/features/franchise/constants"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

export const FranchisePolicyCard = () => {
    return (
        <Card shadow="sm" className="border border-primary-100 bg-primary-50/20">
            <CardBody className="p-6 space-y-4">
                <h3 className="text-sm font-black uppercase text-primary-700 tracking-wider mb-2 flex items-center gap-2">
                    <Info size={18} />
                    Chính sách nhượng quyền chuẩn
                </h3>

                <p className="text-xs text-default-500 italic">
                    * Các con số dưới đây là giá trị mặc định cho một hợp đồng nhượng quyền tiêu
                    chuẩn.
                </p>

                <div className="space-y-3 mt-2">
                    {/* Phí nhượng quyền */}
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2 text-sm text-default-600">
                            <Landmark size={16} className="text-default-400" />
                            <span>Phí nhượng quyền:</span>
                        </div>
                        <span className="font-bold text-default-900">
                            {formatCurrencyWithSymbol(FRANCHISE_POLICY.DEFAULT_FRANCHISE_FEE)}
                        </span>
                    </div>

                    {/* Tiền cọc */}
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2 text-sm text-default-600">
                            <ReceiptText size={16} className="text-default-400" />
                            <span>Tiền cọc (Hoàn lại):</span>
                        </div>
                        <span className="font-bold text-default-900">
                            {formatCurrencyWithSymbol(FRANCHISE_POLICY.DEFAULT_DEPOSIT_AMOUNT)}
                        </span>
                    </div>

                    <Divider className="my-2" />

                    {/* Chia sẻ doanh thu */}
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2 text-sm text-default-600">
                            <Percent size={16} className="text-default-400" />
                            <span>Chia sẻ doanh thu:</span>
                        </div>
                        <span className="font-bold text-primary-600">
                            {FRANCHISE_POLICY.DEFAULT_REVENUE_SHARE}% / tháng
                        </span>
                    </div>

                    {/* Thời hạn */}
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2 text-sm text-default-600">
                            <Hourglass size={16} className="text-default-400" />
                            <span>Thời hạn hợp đồng:</span>
                        </div>
                        <span className="font-bold text-default-900">
                            {FRANCHISE_POLICY.DEFAULT_DURATION_YEARS} năm
                        </span>
                    </div>
                </div>

                <div className="p-3 bg-white rounded-xl border border-primary-100 mt-2">
                    <p className="text-[11px] text-primary-600 leading-relaxed">
                        <b>Lưu ý:</b> Sau khi hồ sơ được duyệt, bạn sẽ tiến hành ký kết hợp đồng
                        điện tử qua DocuSeal và thanh toán tiền cọc để kích hoạt dự án.
                    </p>
                </div>
            </CardBody>
        </Card>
    )
}
