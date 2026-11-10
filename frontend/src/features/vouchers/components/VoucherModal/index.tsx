"use client"

import React, { useEffect, useState } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Button } from "@heroui/react"
import { useFormik, FormikProvider } from "formik"
import { Tag, Save } from "lucide-react"

import { CreateVoucherRequest } from "@/features/vouchers/models/request"
import { useCreateVoucher, useUpdateVoucher } from "@/features/vouchers/hooks/useVoucher"
import { VoucherType } from "@/features/vouchers/constants"
import { VoucherSchema } from "./schema"
import { Voucher } from "@/features/vouchers/models/response"
import { ModalStyled } from "@/components"

// Import 2 component con vừa tách
import { VoucherBasicForm } from "./VoucherBasicForm"
import { VoucherGiftManager } from "./VoucherGiftManager"
import { normalizeCurrency } from "@/utils/helpers/currency"

interface Props {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    voucher?: Voucher

    onSuccessCallback?: (newVoucherId: string) => void
}

export type VoucherTabType = "discount" | "shipping" | "gift"

export const VoucherModal = ({ isOpen, onOpenChange, voucher, onSuccessCallback }: Props) => {
    const isEditing = !!voucher

    // API Mutations
    const { mutate: createVoucher, isPending: isCreating } = useCreateVoucher({
        onSuccess: (res) => {
            onOpenChange(false)
            if (onSuccessCallback && res?.id) {
                onSuccessCallback(res.id)
            }
        }
    })
    const { mutate: updateVoucher, isPending: isUpdating } = useUpdateVoucher({
        onSuccess: () => onOpenChange(false)
    })

    // Local UI State cho việc điều hướng Tab
    const [uiTab, setUiTab] = useState<VoucherTabType>("discount")
    const [isPercent, setIsPercent] = useState(true)

    // Formik Setup
    const formik = useFormik<CreateVoucherRequest>({
        initialValues: {
            code: "",
            type: VoucherType.DISCOUNT_PERCENTAGE,
            discountValue: "",
            maxDiscountValue: "",
            minOrderValue: "",
            pointCost: "",
            initQuantity: 100,
            maxUsagePerUser: 1,
            endDate: "",
            voucherItems: []
        },
        validationSchema: VoucherSchema,
        onSubmit: (values) => {
            const safeInt = (val: any) => {
                const num = normalizeCurrency(val)
                return num > 0 ? num : undefined // BE cần *int32
            }

            const safeStr = (val: any) => {
                const num = normalizeCurrency(val)
                return num > 0 ? String(num) : undefined // BE cần *string, gỡ sạch phẩy/chấm
            }

            const payload: any = {
                ...values,

                discountValue: safeStr(values.discountValue),
                maxDiscountValue: safeStr(values.maxDiscountValue),
                minOrderValue: safeStr(values.minOrderValue),
                pointCost: safeStr(values.pointCost),

                initQuantity: safeInt(values.initQuantity),
                maxUsagePerUser: safeInt(values.maxUsagePerUser)
            }

            // Nếu là quà tặng thì không gửi discountValue
            if (payload.type === VoucherType.GIFT) {
                delete payload.discountValue
                delete payload.maxDiscountValue
            }

            // Nếu là giảm tiền mặt thì không gửi maxDiscount (vì chỉ % mới có tối đa)
            if (!isPercent) {
                delete payload.maxDiscountValue
            }

            // Nếu update thì không gửi field code
            if (isEditing) {
                updateVoucher({ id: voucher.id, req: payload })
            } else {
                createVoucher(payload as CreateVoucherRequest)
            }
        }
    })

    // Load data khi mở modal (Edit Mode) hoặc Reset khi đóng (Create Mode)
    useEffect(() => {
        if (!isOpen) return

        if (voucher) {
            formik.setValues({
                ...voucher,
                endDate: voucher.endDate ? voucher.endDate.split("T")[0] : "",
                voucherItems:
                    voucher.voucherItems?.map((i) => ({
                        sku: i.sku,
                        quantity: i.quantity,
                        type: i.type
                    })) || []
            })

            // Sync UI Tabs dựa trên voucher.type
            if (voucher.type <= 1) setUiTab("discount")
            else if (voucher.type <= 3) setUiTab("shipping")
            else setUiTab("gift")

            // Check xem có phải loại % không
            setIsPercent(
                [VoucherType.DISCOUNT_PERCENTAGE, VoucherType.SHIPPING_PERCENTAGE].includes(
                    voucher.type
                )
            )
        } else {
            formik.resetForm()
            setUiTab("discount")
            setIsPercent(true)
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen, voucher])

    // Lắng nghe sự thay đổi của UI Tabs để tự động map sang VoucherType cho Formik
    useEffect(() => {
        let actualType = VoucherType.DISCOUNT_PERCENTAGE

        if (uiTab === "discount") {
            actualType = isPercent
                ? VoucherType.DISCOUNT_PERCENTAGE
                : VoucherType.DISCOUNT_FIXED_AMOUNT
        } else if (uiTab === "shipping") {
            actualType = isPercent
                ? VoucherType.SHIPPING_PERCENTAGE
                : VoucherType.SHIPPING_FIXED_AMOUNT
        } else {
            actualType = VoucherType.GIFT
        }

        formik.setFieldValue("type", actualType)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [uiTab, isPercent])

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} size="5xl" backdrop="blur">
            <ModalContent>
                {(onClose) => (
                    // Cung cấp context cho các component con
                    <FormikProvider value={formik}>
                        {/* --- HEADER --- */}
                        <ModalHeader className="flex items-center gap-3 border-b border-default-100 py-4 px-8">
                            <div className="p-2 bg-warning-100 text-warning-700 rounded-xl shadow-sm">
                                <Tag size={20} />
                            </div>
                            <div className="flex flex-col">
                                <span className="text-xl font-black text-default-900 leading-none">
                                    {isEditing ? "Hiệu chỉnh Voucher" : "Thiết kế Voucher mới"}
                                </span>
                                <span className="text-tiny text-default-400 font-medium">
                                    Cấu hình ưu đãi dành cho khách hàng
                                </span>
                            </div>
                        </ModalHeader>

                        {/* --- BODY --- */}
                        <ModalBody className="py-6 px-8 bg-[#FCFAFA]">
                            <VoucherBasicForm
                                uiTab={uiTab}
                                setUiTab={setUiTab}
                                isPercent={isPercent}
                                setIsPercent={setIsPercent}
                                isEditing={isEditing}
                            />

                            {/* Chỉ mount GiftManager khi tab là gift để tối ưu performance */}
                            {uiTab === "gift" && <VoucherGiftManager />}
                        </ModalBody>

                        {/* --- FOOTER --- */}
                        <ModalFooter className="bg-white border-t border-default-100 py-4 px-8">
                            <Button variant="light" className="font-bold" onPress={onClose}>
                                Thoát
                            </Button>
                            <Button
                                color="warning"
                                className="px-12 font-black shadow-lg shadow-warning-200"
                                isLoading={isCreating || isUpdating}
                                onPress={() => formik.handleSubmit()}
                            >
                                <Save size={18} className="mr-2" />
                                {isEditing ? "CẬP NHẬT" : "PHÁT HÀNH NGAY"}
                            </Button>
                        </ModalFooter>
                    </FormikProvider>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
