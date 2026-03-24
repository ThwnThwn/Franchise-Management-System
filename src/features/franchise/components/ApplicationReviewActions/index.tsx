"use client"

import { DatePickerStyled, TextareaStyled } from "@/components/styled"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { Button, Card, CardBody } from "@heroui/react"
import dayjs from "dayjs"
import { CheckCircle, XCircle } from "lucide-react"
import { useState } from "react"
import { FRANCHISE_POLICY, FranchiseApplicationStatus } from "@/features/franchise/constants"
import { useReviewApplication } from "@/features/franchise/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import { useDay } from "@/hooks"
import { FranchiseApplicationResponse } from "../../models/response"

export const ApplicationReviewActions = ({
    application
}: {
    application: FranchiseApplicationResponse
}) => {
    const [note, setNote] = useState("")
    const [validFrom, setValidFrom] = useState<string>(dayjs().format())
    const { toDate, formatDate } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })

    const { mutate: review, isPending } = useReviewApplication()

    const handleAction = (isApprove: boolean) => {
        review({
            id: application.id,
            req: {
                action: isApprove
                    ? FranchiseApplicationStatus.Approved
                    : FranchiseApplicationStatus.Rejected,
                note: note,
                validFrom: isApprove ? validFrom : undefined,
                depositAmount: FRANCHISE_POLICY.DEFAULT_DEPOSIT_AMOUNT,
                franchiseFee: FRANCHISE_POLICY.DEFAULT_FRANCHISE_FEE,
                revenueSharePercentage: FRANCHISE_POLICY.DEFAULT_REVENUE_SHARE,
                validUntil: isApprove
                    ? dayjs(validFrom).add(FRANCHISE_POLICY.DEFAULT_DURATION_YEARS, "year").format()
                    : undefined
            }
        })
    }

    return (
        <Card className="border-none shadow-sm bg-default-50/50">
            <CardBody className="p-6 space-y-6">
                <div className="flex items-center gap-2 text-primary-700 font-black uppercase text-xs tracking-wider">
                    <CheckCircle size={16} />
                    Thẩm định hồ sơ
                </div>

                <div className="space-y-4">
                    <DatePickerStyled
                        label="Ngày bắt đầu hợp đồng (Dự kiến)"
                        labelPlacement="outside"
                        value={toDate(validFrom)}
                        onChange={(val) => setValidFrom(formatDate(val))}
                    />

                    <TextareaStyled
                        label="Ghi chú thẩm định"
                        placeholder="Nhập lý do duyệt hoặc từ chối..."
                        labelPlacement="outside"
                        variant="bordered"
                        value={note}
                        onValueChange={setNote}
                    />
                </div>

                <div className="p-4 rounded-xl bg-white border border-default-200 space-y-3">
                    <p className="text-xs font-bold text-default-500 uppercase">
                        Thông số hợp đồng sẽ tạo:
                    </p>
                    <div className="flex justify-between text-sm">
                        <span className="text-default-500">Phí:</span>
                        <span className="font-bold">
                            {formatCurrencyWithSymbol(FRANCHISE_POLICY.DEFAULT_FRANCHISE_FEE)}
                        </span>
                    </div>
                    <div className="flex justify-between text-sm">
                        <span className="text-default-500">Cọc:</span>
                        <span className="font-bold">
                            {formatCurrencyWithSymbol(FRANCHISE_POLICY.DEFAULT_DEPOSIT_AMOUNT)}
                        </span>
                    </div>
                    <div className="flex justify-between text-sm">
                        <span className="text-default-500">Thời hạn:</span>
                        <span className="font-bold">
                            {FRANCHISE_POLICY.DEFAULT_DURATION_YEARS} năm
                        </span>
                    </div>
                </div>

                {application.status === FranchiseApplicationStatus.Pending && (
                    <div className="grid grid-cols-2 gap-3">
                        <Button
                            variant="flat"
                            color="danger"
                            className="font-bold"
                            startContent={<XCircle size={18} />}
                            onPress={() => handleAction(false)}
                            isLoading={isPending}
                        >
                            TỪ CHỐI
                        </Button>
                        <Button
                            color="primary"
                            className="font-bold"
                            startContent={<CheckCircle size={18} />}
                            onPress={() => handleAction(true)}
                            isLoading={isPending}
                        >
                            PHÊ DUYỆT
                        </Button>
                    </div>
                )}
            </CardBody>
        </Card>
    )
}
