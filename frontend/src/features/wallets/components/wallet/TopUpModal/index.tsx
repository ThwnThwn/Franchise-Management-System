"use client"

import { useFormik } from "formik"
import { ModalContent, ModalHeader, ModalBody, ModalFooter } from "@heroui/react"
import { CreditCard } from "lucide-react"

import { ModalStyled, ButtonStyled, InputStyled } from "@/components"
import { useTopUpWallet } from "@/features/wallets/hooks"
import { formatCurrency } from "@/utils/helpers/currency"
import { topUpSchema } from "./schema"
import { useFormikHelpers } from "@/hooks"
import { usePathname } from "next/navigation"

interface TopUpModalProps {
    isOpen: boolean
    onClose: () => void
    returnUrl?: string
}

export const TopUpModal = ({ isOpen, onClose, returnUrl: customReturnUrl }: TopUpModalProps) => {
    const pathname = usePathname()
    const finalReturnUrl = customReturnUrl || pathname || "/profile/wallet"

    const { mutate: topUp, isPending } = useTopUpWallet({
        onSuccess: (paymentUrl) => {
            window.location.href = paymentUrl
        }
    })

    const formik = useFormik({
        initialValues: {
            amount: ""
        },
        validationSchema: topUpSchema,
        onSubmit: (values) => {
            topUp({
                amount: Number(String(values.amount).replace(/[^0-9]/g, "")),
                returnUrl: finalReturnUrl
            })
        }
    })

    const { isInvalid, getErrorMessage, setNumberValue } = useFormikHelpers(formik)

    const handleClose = () => {
        formik.resetForm()
        onClose()
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={handleClose} size="sm">
            <ModalContent>
                <form onSubmit={formik.handleSubmit}>
                    <ModalHeader className="flex flex-col gap-1 pb-2">
                        <h2 className="text-xl font-bold flex items-center gap-2">
                            <CreditCard size={24} className="text-primary" />
                            Nạp tiền vào ví
                        </h2>
                    </ModalHeader>

                    <ModalBody>
                        <p className="text-sm text-default-500 mb-4">
                            Nhập số tiền bạn muốn nạp. Hệ thống sẽ chuyển hướng sang cổng thanh toán
                            VNPay.
                        </p>

                        <InputStyled
                            autoFocus
                            type="text"
                            inputMode="numeric"
                            label="Số tiền (VNĐ)"
                            placeholder="VD: 100.000"
                            variant="bordered"
                            value={formatCurrency(formik.values.amount)}
                            onValueChange={(v) => {
                                const rawValue = v.replace(/[^0-9]/g, "")
                                setNumberValue("amount", rawValue, "")
                            }}
                            isInvalid={isInvalid("amount")}
                            errorMessage={getErrorMessage("amount")}
                        />
                    </ModalBody>

                    <ModalFooter>
                        <ButtonStyled variant="light" onPress={handleClose} isDisabled={isPending}>
                            Hủy
                        </ButtonStyled>
                        <ButtonStyled color="primary" type="submit" isLoading={isPending}>
                            Tiếp tục
                        </ButtonStyled>
                    </ModalFooter>
                </form>
            </ModalContent>
        </ModalStyled>
    )
}
