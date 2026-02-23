"use client"

import React, { useEffect, useState } from "react"
import "./index.css"
import { Chip, Spinner, addToast, Card, CardHeader, CardBody, CardFooter } from "@heroui/react"
import { BackLink, ButtonStyled, EnumPicker } from "@/components"
import {
    ORDER_TEXT,
    OrderStatus,
    OrderStatusColors,
    OrderStatusLabels,
    OrderStatusNote,
    PaymentMethod,
    PaymentMethodLabels
} from "@/features/orders/constants"
import { confirmDialog, useDay, useTokenStore } from "@/hooks"
import { CHB, DATE_TIME_VIEW_FORMAT } from "@/constants/constants"
import { useGetOrderById, usePayment, useUpdateOrderStatus } from "@/features/orders/hooks"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { useRouter } from "next/navigation"
import { omitRecord } from "@/utils/helpers/uiHelper"
import { OrderDetail } from "@/features/orders/components"
import { Printer } from "lucide-react"
import { RoleSlug } from "@/constants/enums"
import { useGetRecipes } from "@/features/products/hooks/useRecipe"

export default function OrderDetailPageView({ orderId }: { orderId: string }) {
    // const isCustomer = false
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)

    const router = useRouter()

    const { data: order, isLoading, error } = useGetOrderById({ id: orderId, enabled: !!orderId })
    const {
        data: recipes,
        isLoading: isRecipeLoading,
        error: recipeError
    } = useGetRecipes({
        req: {
            variantIds: order?.items.map((item) => item.variantId) || []
        },
        enabled: !isCustomer && !!order
    })

    // const recipes = React.useMemo(() => {
    //     if (!order) return []
    //     const ids = order.items.map((item) => item.variantId)
    //     return generateMockRecipes(ids)
    // }, [order])

    // const isRecipeLoading = false
    // const recipeError = null

    const { formatDateTime } = useDay({ defaultFormat: DATE_TIME_VIEW_FORMAT })
    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | undefined>(undefined)

    const { mutate: payment, isPending: isPaymentPending } = usePayment({
        id: orderId,
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
        payment({ paymentMethod, returnUrl: `/orders/${orderId}` })
    }

    const updateStatus = useUpdateOrderStatus({ id: orderId })

    const handleCancel = async () => {
        if (!order) return
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

    useEffect(() => {
        if (error || recipeError) {
            const backendErr = (error || recipeError) as BackendError
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: backendErr.detail,
                color: "danger"
            })
        }
    }, [error, recipeError])

    if (isLoading || isRecipeLoading || !order) {
        return (
            <div className="flex h-[80vh] items-center justify-center">
                <Spinner label="Đang tải hoá đơn..." />
            </div>
        )
    }

    return (
        // Đã đẩy rộng ra max-w-5xl
        <div className="w-full md:w-3xl max-w-5xl mx-auto p-4 md:p-10 space-y-8">
            {/* Header ngoài Card: Điều hướng & Hành động nhanh */}
            <div className="flex justify-between items-center px-2">
                <BackLink
                    href={isCustomer ? "/orders" : "/dashboard/orders"}
                    label="Danh sách đơn hàng"
                    className="text-default-500 hover:text-primary"
                />
                <ButtonStyled isIconOnly variant="flat" size="sm" onPress={() => window.print()}>
                    <Printer size={18} />
                </ButtonStyled>
            </div>

            <Card
                id="invoice-card"
                className="shadow-none border-none rounded-[2.5rem] overflow-hidden bg-white"
            >
                {/* Phần đầu Hóa đơn: Thông tin định danh */}
                <CardHeader className="flex flex-col p-8 md:p-12 pb-0! space-y-8">
                    {/* Phần hiển thị khi IN (CHB Coffee Shop) */}
                    <div className="hidden print:flex flex-col items-center text-center">
                        <h2 className="text-xl font-black uppercase">{CHB} COFFEE SHOP</h2>
                        <div className="border-b border-black w-full my-4" />
                    </div>

                    <div className="flex flex-col gap-1 w-full mb-0">
                        <div className="space-y-4">
                            {" "}
                            {/* Tăng gap lên 4 để thoáng hơn */}
                            <div className="flex justify-between items-start gap-3">
                                <div className="space-y-1">
                                    <h1 className="text-3xl md:text-4xl font-black tracking-tighter text-slate-900 leading-none">
                                        {ORDER_TEXT.labels.ORDER_DETAIL_TITLE}
                                    </h1>
                                </div>

                                <div className="flex flex-col items-end gap-2">
                                    <div className="flex items-center gap-2">
                                        <p className="hidden print:block text-[10px] font-mono text-default-400 tracking-widest uppercase font-bold">
                                            Trạng thái:
                                        </p>
                                        <Chip
                                            color={OrderStatusColors[order.status]}
                                            variant="flat"
                                            size="lg"
                                            className="font-black border-none px-4"
                                        >
                                            {OrderStatusLabels[order.status]}
                                        </Chip>
                                    </div>
                                </div>
                            </div>
                            {/* Mã đơn hàng nằm gọn dưới tiêu đề */}
                            <p className="text-sm mb-1 font-mono text-default-400 tracking-widest uppercase">
                                Mã đơn:{" "}
                                <span className="text-slate-900 font-bold">#{order.code}</span>
                            </p>
                            {/* Thông tin thời gian */}
                            <p className="text-[10px] font-mono text-default-400 tracking-[0.2em] uppercase">
                                {ORDER_TEXT.labels.ORDER_DATE}:{" "}
                                <span className="text-slate-900 font-bold ml-1">
                                    {formatDateTime({ date: order.createdAt })}
                                </span>
                            </p>
                            {/* Dòng Note cụ thể cho từng Status - Điểm nhấn mới */}
                            <div className="bg-default-50 p-4 rounded-2xl border-l-4 border-default-200 print:bg-white print:p-0 print:border-none">
                                <p className="text-sm text-slate-600 font-medium italic">
                                    <span className="font-bold not-italic">Ghi chú: </span>
                                    {OrderStatusNote[order.status]}
                                </p>
                            </div>
                        </div>
                    </div>
                </CardHeader>

                {/* Đường phân cách nét đứt (Dashed Divider) */}
                <div className="px-8 md:px-12">
                    <div className="my-4 border-t-2 border-dashed border-default-500 w-full relative">
                        {/* Hiệu ứng đục lỗ hai bên mép hóa đơn */}
                        <div className="absolute -left-15 -top-3 w-6 h-6 rounded-full bg-background florage-background shadow-inner md:block hidden" />
                        <div className="absolute -right-15 -top-3 w-6 h-6 rounded-full bg-background florage-background shadow-inner md:block hidden" />
                    </div>
                </div>

                <CardBody className="p-8 md:p-12">
                    {/* Chỗ này sẽ hiển thị chi tiết món ăn, số lượng, giá tiền */}
                    <div className="min-h-50">
                        <OrderDetail
                            key={order.id}
                            order={order}
                            recipes={recipes}
                            isCustomer={isCustomer}
                        />
                    </div>
                </CardBody>

                {/* Footer Hóa đơn */}
                <div className="print:hidden px-8 md:px-12">
                    <div className="my-4 border-t-2 border-dashed border-default-500 w-full relative"></div>
                </div>

                <CardFooter className="print:hidden flex flex-col p-8 md:p-12 gap-8 bg-slate-50/30">
                    {order.status === OrderStatus.PaymentPending ? (
                        <div className="w-full space-y-6">
                            <div className="flex flex-col items-center gap-2">
                                <p className="text-[11px] uppercase tracking-[0.3em] text-primary font-black">
                                    Hoàn tất thanh toán đơn hàng
                                </p>
                                <div className="h-1 w-12 bg-primary/20 rounded-full" />
                            </div>

                            <div className="flex flex-col md:flex-row justify-center items-center gap-4 max-w-2xl mx-auto w-full">
                                <EnumPicker
                                    placeholder="Phương thức thanh toán"
                                    labels={omitRecord(PaymentMethodLabels, [
                                        PaymentMethod.Unknown
                                    ])}
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

                            {isCustomer && (
                                <center>
                                    <ButtonStyled
                                        variant="light"
                                        color="danger"
                                        size="sm"
                                        className="font-bold opacity-40 hover:opacity-100 transition-all"
                                        onPress={handleCancel}
                                        isLoading={updateStatus.isPending}
                                    >
                                        Tôi muốn huỷ đơn hàng này
                                    </ButtonStyled>
                                </center>
                            )}
                        </div>
                    ) : (
                        <div className="w-full text-center space-y-2 py-4">
                            <p className="text-default-400 font-medium italic">
                                Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi.
                            </p>
                            <p className="text-[10px] text-default-300 uppercase tracking-widest">
                                Bản sao hóa đơn điện tử • {order.id}
                            </p>
                        </div>
                    )}
                </CardFooter>
            </Card>
        </div>
    )
}
