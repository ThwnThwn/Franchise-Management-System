"use client"

import { useState } from "react"
import { addToast } from "@heroui/react"
import { ButtonStyled, EnumPicker, InputStyled } from "@/components"
import {
    ORDER_TEXT,
    OrderStatus,
    OrderStatusLabels,
    PaymentMethod,
    PaymentMethodLabels
} from "@/features/orders/constants"
import { confirmDialog, useTokenStore } from "@/hooks"
import { usePayment, useUpdateOrderStatus } from "@/features/orders/hooks"
import { STRINGS } from "@/constants/strings"
import { useRouter } from "next/navigation"
import { omitRecord } from "@/utils/helpers/uiHelper"
import { RoleSlug } from "@/constants/enums"
import { OrderStatusActions } from "@/features/orders/components/OrderStatusActions"
import { canCancelFrom } from "@/features/orders/utils"
import { Voucher } from "@/features/vouchers/models/response"
import { Order } from "@/features/orders/models/response"
import { WalletResponse } from "@/features/wallets/models/response"
import { formatCurrencyWithSymbol, normalizeCurrency } from "@/utils/helpers/currency"
import { Wallet as WalletIcon } from "lucide-react"

export function OrderDetailFooter({
    order,
    wallet, // Nhận thêm thông tin Ví từ cha
    totalAfterTax, //  Tổng tiền bill (để in ra báo khách)
    finalAmount, //  Tiền mặt còn lại phải trả
    appliedWallet,
    setAppliedWallet, //  Cần hàm này để update state khi khách nhập
    appliedPoint,
    shippingVoucher,
    discountVoucher
}: {
    order: Order
    wallet?: WalletResponse
    totalAfterTax: number
    finalAmount: number
    appliedWallet: number
    setAppliedWallet: (val: number) => void
    appliedPoint: number
    shippingVoucher?: Voucher
    discountVoucher?: Voucher
}) {
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isStaff = roleSlug === RoleSlug.Staff
    const router = useRouter()

    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | undefined>(undefined)

    const actualWalletBalance = normalizeCurrency(wallet?.balance) || 0

    //  Hàm xử lý khi khách gõ tiền Ví
    const handleWalletChange = (val: string) => {
        const amount = Math.max(0, normalizeCurrency(val) || 0)
        // Chỉ cho phép nhập tối đa bằng số tiền Sau Thuế hoặc Số dư ví thực tế
        setAppliedWallet(Math.min(amount, actualWalletBalance, totalAfterTax))
    }

    const { mutate: payment, isPending: isPaymentPending } = usePayment({
        id: order.id,
        onSuccess: (data: { paymentUrl: string }) => {
            if (data.paymentUrl) {
                router.push(data.paymentUrl)
            } else {
                addToast({
                    title: STRINGS.COMMON.SUCCESS,
                    description: "Thành công! Đang chờ nhà hàng xác nhận đơn...",
                    color: "success"
                })
            }
        }
    })

    const handlePayment = () => {
        if (paymentMethod === undefined) return

        //  Dùng state được truyền từ cha xuống để submit
        payment({
            paymentMethod,
            returnUrl: `/orders/${order.id}`,
            point: appliedPoint > 0 ? appliedPoint : undefined,
            appliedWalletAmount: appliedWallet > 0 ? appliedWallet : undefined,
            voucherShipCode: shippingVoucher?.code || undefined,
            voucherDiscountCode: discountVoucher?.code || undefined
        })
    }

    const updateStatus = useUpdateOrderStatus({ id: order.id })
    const handleUpdate = async (status: OrderStatus) => {
        const isConfirmed = await confirmDialog({
            title: "Xác nhận chuyển trạng thái",
            message: (
                <div className="text-slate-600">Chuyển sang [{OrderStatusLabels[status]}]</div>
            ),
            confirmText: "Xác nhận",
            variant: "info"
        })
        if (isConfirmed) updateStatus.mutate(status)
    }

    const handleCancel = async () => {
        const isConfirmed = await confirmDialog({
            title: "Xác nhận huỷ",
            message: (
                <div className="text-slate-600">
                    Bạn có chắc chắn muốn huỷ đơn hàng <br />
                    <span className="font-bold text-slate-900 text-lg">{order.code}</span> không?
                </div>
            ),
            confirmText: "Đồng ý huỷ",
            variant: "danger"
        })
        if (isConfirmed) updateStatus.mutate(OrderStatus.Cancelled)
    }

    // NẾU ĐƠN KHÔNG PHẢI TRẠNG THÁI PENDING
    if (order.status !== OrderStatus.PaymentPending) {
        return (
            <>
                {/* Hủy đơn */}
                {canCancelFrom(order.status) && (
                    <center>
                        <ButtonStyled
                            variant="light"
                            color="danger"
                            size="sm"
                            className="font-bold opacity-40 hover:opacity-100 transition-all underline"
                            onPress={handleCancel}
                            isLoading={updateStatus.isPending}
                        >
                            Tôi muốn huỷ đơn hàng này
                        </ButtonStyled>
                    </center>
                )}
                <div className="w-full text-center space-y-2 py-4">
                    <p className="text-default-400 font-medium italic">
                        Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi.
                    </p>
                    <p className="text-[10px] text-default-300 uppercase tracking-widest">
                        Bản sao hóa đơn điện tử • {order.id}
                    </p>
                    {isStaff && (
                        <OrderStatusActions currentStatus={order.status} onUpdate={handleUpdate} />
                    )}
                </div>
            </>
        )
    }

    // NẾU ĐƠN ĐANG PENDING
    return (
        <div className="w-full space-y-6 pb-10">
            {/* Thanh điều hướng Form */}
            <div className="flex flex-col items-center gap-2">
                <p className="text-[11px] uppercase tracking-[0.3em] text-primary font-black">
                    Hoàn tất thanh toán
                </p>
                <div className="h-1 w-12 bg-primary/20 rounded-full" />
            </div>

            <div className="max-w-3xl mx-auto w-full space-y-6 px-4">
                {/*  1. KHỐI NHẬP VÍ & TỔNG KẾT NHANH */}
                {order.status === OrderStatus.PaymentPending && (
                    <div className="bg-default-50 p-5 rounded-3xl border border-default-200 space-y-4 shadow-inner">
                        <InputStyled
                            type="number"
                            label="Dùng số dư Ví CFMS"
                            placeholder="0"
                            value={appliedWallet > 0 ? appliedWallet.toString() : ""}
                            onValueChange={handleWalletChange}
                            startContent={<WalletIcon size={18} className="text-success-500" />}
                            endContent={
                                <span className="text-xs font-bold text-default-400">VND</span>
                            }
                            description={`Số dư khả dụng: ${formatCurrencyWithSymbol(actualWalletBalance)}`}
                        />

                        {/* Summary nhỏ của dòng tiền thanh toán */}
                        <div className="pt-2 border-t border-dashed border-default-200 space-y-2">
                            <div className="flex justify-between text-sm">
                                <span className="text-default-500">Tổng hóa đơn:</span>
                                <span className="font-bold">
                                    {formatCurrencyWithSymbol(totalAfterTax)}
                                </span>
                            </div>
                            {appliedWallet > 0 && (
                                <div className="flex justify-between text-sm font-bold text-success-600">
                                    <span>Trừ từ Ví:</span>
                                    <span>-{formatCurrencyWithSymbol(appliedWallet)}</span>
                                </div>
                            )}
                            <div className="flex justify-between items-end pt-2">
                                <span className="text-xs font-black uppercase text-default-500 tracking-widest">
                                    Tiền cần trả:
                                </span>
                                <span className="text-2xl font-black text-primary leading-none">
                                    {formatCurrencyWithSymbol(finalAmount)}
                                </span>
                            </div>
                        </div>
                    </div>
                )}

                {/*  2. Form chọn phương thức */}
                <div className="flex flex-col md:flex-row justify-center items-center gap-4 w-full pt-2">
                    <EnumPicker
                        placeholder="Phương thức thanh toán"
                        labels={omitRecord(PaymentMethodLabels, [PaymentMethod.Unknown])}
                        value={paymentMethod}
                        onChange={(val) => setPaymentMethod(val as PaymentMethod)}
                        isClearable={false}
                        className="w-full md:w-80"
                    />
                    <ButtonStyled
                        color="primary"
                        size="lg"
                        className="w-full md:w-auto px-12 font-black shadow-xl shadow-primary/30"
                        onPress={handlePayment}
                        isLoading={isPaymentPending}
                        isDisabled={paymentMethod === undefined}
                    >
                        {ORDER_TEXT.labels.PAY}
                    </ButtonStyled>
                </div>

                {/* Hủy đơn */}
                {canCancelFrom(order.status) && (
                    <center>
                        <ButtonStyled
                            variant="light"
                            color="danger"
                            size="sm"
                            className="font-bold opacity-40 hover:opacity-100 transition-all underline"
                            onPress={handleCancel}
                            isLoading={updateStatus.isPending}
                        >
                            Tôi muốn huỷ đơn hàng này
                        </ButtonStyled>
                    </center>
                )}
            </div>
        </div>
    )
}
