"use client"

import { useState } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Spinner } from "@heroui/react"
import { Receipt } from "lucide-react"
import { ModalStyled, ButtonStyled } from "@/components"

import { useGetWithdrawalById, useCancelMyWithdrawal } from "@/features/wallets/hooks"
import { WithdrawalStatus } from "@/features/wallets/constants"

import {
    OverviewCard,
    BankInfoCard,
    TimelineCard,
    AlertCard,
    ReceiptCard
} from "../WithdrawalSharedCards"

interface Props {
    id: string | null
    isOpen: boolean
    onClose: () => void
}

export const WithdrawalDetailModal = ({ id, isOpen, onClose }: Props) => {
    const [showRawAccount, setShowRawAccount] = useState(false)

    const { data: detail, isLoading } = useGetWithdrawalById(id || "", isOpen && !!id)
    const { cancelWithConfirm, isPending: isCanceling } = useCancelMyWithdrawal({
        onSuccess: onClose
    })

    if (!id) return null

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} scrollBehavior="inside">
            <ModalContent>
                <ModalHeader className="flex flex-col gap-1 pb-2">
                    <h2 className="text-xl font-bold flex items-center gap-2">
                        <Receipt size={24} className="text-primary" /> Chi tiết lệnh rút tiền
                    </h2>
                </ModalHeader>

                <ModalBody className="py-4">
                    {isLoading || !detail ? (
                        <div className="flex justify-center py-10">
                            <Spinner color="primary" />
                        </div>
                    ) : (
                        <div className="flex flex-col gap-5">
                            <OverviewCard detail={detail} />

                            <BankInfoCard
                                detail={detail}
                                showRaw={showRawAccount}
                                onToggleRaw={() => setShowRawAccount(!showRawAccount)}
                            />

                            <TimelineCard detail={detail} />
                            <ReceiptCard detail={detail} />
                            <AlertCard detail={detail} />
                        </div>
                    )}
                </ModalBody>

                <ModalFooter className="border-t border-divider pt-4 flex-wrap justify-end gap-2">
                    <ButtonStyled variant="light" onPress={onClose} isDisabled={isCanceling}>
                        Đóng
                    </ButtonStyled>

                    {detail?.status === WithdrawalStatus.Pending && (
                        <ButtonStyled
                            color="danger"
                            onPress={() => cancelWithConfirm(detail.id)}
                            isLoading={isCanceling}
                            className="font-bold"
                        >
                            Hủy lệnh rút tiền
                        </ButtonStyled>
                    )}
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
