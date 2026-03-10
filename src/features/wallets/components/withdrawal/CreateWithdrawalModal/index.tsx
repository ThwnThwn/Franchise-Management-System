/* eslint-disable @next/next/no-img-element */
"use client"

import { useEffect, useState } from "react"
import { useFormik } from "formik"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Autocomplete,
    AutocompleteItem
} from "@heroui/react"
import { Landmark, Wallet } from "lucide-react"

import { ModalStyled, ButtonStyled, InputStyled } from "@/components"
import { useCreateWithdrawal, useGetMyWallet } from "@/features/wallets/hooks"
import { formatCurrency, formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { useFormikHelpers } from "@/hooks" // Import bảo bối của ní vào đây
import { getWithdrawalSchema } from "./schema"
import { VIETQR_API_URL } from "@/constants/env"

interface Props {
    isOpen: boolean
    onClose: () => void
}

export const CreateWithdrawalModal = ({ isOpen, onClose }: Props) => {
    const { data: wallet } = useGetMyWallet(isOpen)
    const maxBalance = wallet?.balance || "0"

    const [banks, setBanks] = useState<any[]>([])

    const { createWithConfirm, isPending } = useCreateWithdrawal({
        onSuccess: () => {
            formik.resetForm()
            onClose()
        }
    })

    const formik = useFormik({
        initialValues: {
            amount: "",
            bankCode: "",
            bankName: "",
            accountName: "",
            accountNumber: "",
            notes: "",
            isManual: false
        },
        enableReinitialize: true,
        validationSchema: getWithdrawalSchema(maxBalance),
        onSubmit: async (values) => {
            await createWithConfirm({
                amount: String(values.amount).replace(/[^0-9]/g, ""),
                bankCode: values.isManual ? undefined : values.bankCode,
                bankName: values.bankName,
                accountName: values.accountName,
                accountNumber: values.accountNumber,
                notes: values.notes
            })
        }
    })

    const { isInvalid, getErrorMessage, setNumberValue } = useFormikHelpers(formik)

    useEffect(() => {
        const fetchBanks = async () => {
            try {
                const res = await fetch(VIETQR_API_URL)
                const data = await res.json()
                setBanks(data.data || [])
            } catch {
                formik.setFieldValue("isManual", true)
            }
        }

        if (isOpen && banks.length === 0) {
            fetchBanks()
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [banks.length, isOpen])

    const handleClose = () => {
        formik.resetForm()
        onClose()
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={handleClose} size="lg">
            <ModalContent>
                <form onSubmit={formik.handleSubmit}>
                    <ModalHeader className="flex flex-col gap-1 pb-2">
                        <h2 className="text-xl font-bold flex items-center gap-2">
                            <Landmark size={24} className="text-primary" />
                            Tạo lệnh rút tiền
                        </h2>
                    </ModalHeader>

                    <ModalBody className="flex flex-col gap-4">
                        <div className="flex items-center gap-2 bg-primary-50 text-primary-700 p-3 rounded-xl border border-primary-100">
                            <Wallet size={18} />
                            <span className="text-sm font-semibold">
                                Số dư khả dụng: {formatCurrencyWithSymbol(maxBalance)}
                            </span>
                        </div>

                        <InputStyled
                            label="Số tiền rút (VNĐ)"
                            type="text"
                            inputMode="numeric"
                            value={formatCurrency(formik.values.amount)}
                            onValueChange={(v) => {
                                const rawValue = v.replace(/[^0-9]/g, "")
                                setNumberValue("amount", rawValue, "")
                            }}
                            isInvalid={isInvalid("amount")}
                            errorMessage={getErrorMessage("amount")}
                            isRequired
                        />

                        {!formik.values.isManual ? (
                            <div className="flex flex-col gap-1">
                                <Autocomplete
                                    label="Ngân hàng thụ hưởng"
                                    defaultItems={banks}
                                    isRequired
                                    selectedKey={formik.values.bankCode}
                                    onSelectionChange={(key) => {
                                        const bank = banks.find((b) => b.code === key)
                                        if (bank) {
                                            formik.setFieldValue("bankCode", bank.code)
                                            formik.setFieldValue("bankName", bank.shortName)
                                        }
                                    }}
                                    isInvalid={isInvalid("bankCode")}
                                    errorMessage={getErrorMessage("bankCode")}
                                >
                                    {(bank) => (
                                        <AutocompleteItem
                                            key={bank.code}
                                            // HeroUI cần textValue để biết đường Search khi dùng custom UI bên trong
                                            textValue={`${bank.shortName} ${bank.name}`}
                                        >
                                            <div className="flex items-center gap-3">
                                                {/* Ốp logo ngân hàng vào đây */}
                                                <div className="w-8 h-8 shrink-0 bg-white border border-default-200 rounded-md p-0.5 flex items-center justify-center">
                                                    <img
                                                        src={bank.logo}
                                                        alt={bank.shortName}
                                                        className="max-w-full max-h-full object-contain"
                                                    />
                                                </div>
                                                <div className="flex flex-col">
                                                    <span className="text-sm font-bold">
                                                        {bank.shortName}
                                                    </span>
                                                    <span className="text-xs text-default-500 line-clamp-1">
                                                        {bank.name}
                                                    </span>
                                                </div>
                                            </div>
                                        </AutocompleteItem>
                                    )}
                                </Autocomplete>
                                <span
                                    className="text-xs text-primary cursor-pointer self-end hover:underline"
                                    onClick={() => {
                                        formik.setFieldValue("isManual", true)
                                        formik.setFieldValue("bankCode", "")
                                        formik.setFieldValue("bankName", "")
                                    }}
                                >
                                    Không tìm thấy ngân hàng? Nhập thủ công
                                </span>
                            </div>
                        ) : (
                            <div className="flex flex-col gap-1">
                                <InputStyled
                                    label="Tên Ngân hàng"
                                    value={formik.values.bankName}
                                    onValueChange={(v) => formik.setFieldValue("bankName", v)}
                                    isInvalid={isInvalid("bankName")}
                                    errorMessage={getErrorMessage("bankName")}
                                    isRequired
                                />
                                <span
                                    className="text-xs text-primary cursor-pointer self-end hover:underline"
                                    onClick={() => {
                                        formik.setFieldValue("isManual", false)
                                        formik.setFieldValue("bankCode", "")
                                        formik.setFieldValue("bankName", "")
                                    }}
                                >
                                    Chọn từ danh sách
                                </span>
                            </div>
                        )}

                        <InputStyled
                            label="Số tài khoản"
                            value={formik.values.accountNumber}
                            onValueChange={(v) =>
                                formik.setFieldValue("accountNumber", v.replace(/[^0-9]/g, ""))
                            }
                            isInvalid={isInvalid("accountNumber")}
                            errorMessage={getErrorMessage("accountNumber")}
                            isRequired
                        />
                        <InputStyled
                            label="Tên chủ tài khoản"
                            placeholder="VD: NGUYEN VAN A"
                            value={formik.values.accountName}
                            onValueChange={(v) =>
                                formik.setFieldValue("accountName", v.toUpperCase())
                            }
                            isInvalid={isInvalid("accountName")}
                            errorMessage={getErrorMessage("accountName")}
                            isRequired
                        />
                        <InputStyled
                            label="Ghi chú (Tùy chọn)"
                            value={formik.values.notes}
                            onValueChange={(v) => formik.setFieldValue("notes", v)}
                            isInvalid={isInvalid("notes")}
                            errorMessage={getErrorMessage("notes")}
                        />
                    </ModalBody>

                    <ModalFooter>
                        <ButtonStyled variant="light" onPress={handleClose} isDisabled={isPending}>
                            Hủy
                        </ButtonStyled>
                        <ButtonStyled color="primary" type="submit" isLoading={isPending}>
                            Xác nhận rút
                        </ButtonStyled>
                    </ModalFooter>
                </form>
            </ModalContent>
        </ModalStyled>
    )
}
