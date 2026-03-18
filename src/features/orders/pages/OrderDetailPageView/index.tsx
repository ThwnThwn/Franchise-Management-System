"use client"

import { useEffect, useRef, useState } from "react"
import "./index.css"
import { Spinner, addToast, Card, CardHeader, CardBody, CardFooter } from "@heroui/react"
import { BackLink, ButtonStyled } from "@/components"
import { useTokenStore } from "@/hooks"
import { useGetOrderById } from "@/features/orders/hooks"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { OrderDetail, OrderDetailFooter, OrderDetailHeader } from "@/features/orders/components"
import { Printer } from "lucide-react"
import { RoleSlug } from "@/constants/enums"
import { useGetRecipes } from "@/features/products/hooks/useRecipe"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherSelector } from "@/features/vouchers/components"

import { useGetMe } from "@/features/profile/hooks"
import { useGetUserById } from "@/features/users/hooks"
import { useGetWalletByUserId } from "@/features/wallets/hooks"
import { useOrderDisplayData } from "@/features/orders/hooks" // Nhớ import đúng đường dẫn
import { normalizeCurrency } from "@/utils/helpers/currency"

export default function OrderDetailPageView({ orderId }: { orderId: string }) {
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const isStaff = useTokenStore((s) => s.roleSlug === RoleSlug.Staff)

    const router = useRouter()
    const pathname = usePathname()
    const searchParams = useSearchParams()

    // --- 1. STATE QUẢN LÝ KHẤU TRỪ (Ví & Điểm) ---
    const [appliedWallet, setAppliedWallet] = useState<number>(0)
    const [appliedPoint, setAppliedPoint] = useState<number>(0)

    // --- 2. STATE VOUCHER ---
    const [isVoucherOpen, setIsVoucherOpen] = useState(false)
    const [shippingVoucher, setShippingVoucher] = useState<Voucher | undefined>(undefined)
    const [discountVoucher, setDiscountVoucher] = useState<Voucher | undefined>(undefined)

    // --- 3. FETCH DATA ---
    const { data: order, isLoading } = useGetOrderById({ id: orderId, enabled: !!orderId })

    const { data: userMe } = useGetMe({ enabled: isCustomer })
    const { data: userId } = useGetUserById({ id: order?.customerId ?? "", enabled: !isCustomer })
    const user = userMe || userId

    const { data: wallet } = useGetWalletByUserId({
        userId: user?.id,
        enabled: !!user?.id
    })

    const { data: recipes, isLoading: isRecipeLoading } = useGetRecipes({
        req: { variantIds: order?.items.map((item) => item.variantId) || [] },
        enabled: !isCustomer && !!order
    })

    // --- 4. GỌI HOOK TÍNH TOÁN BẰNG OBJECT ---
    const actualWalletBalance = normalizeCurrency(wallet?.balance)
    const actualPointBalance = normalizeCurrency(user?.loyalty?.availablePoints)
    const valuePerPoint = normalizeCurrency(user?.loyalty?.currentTier?.valuePerPoint) || 1000

    const displayData = useOrderDisplayData({
        order: order,
        appliedDiscountVoucher: discountVoucher,
        appliedShippingVoucher: shippingVoucher,
        discountRate: user?.loyalty?.currentTier?.discountRate,
        requestedPoints: appliedPoint,
        requestedWallet: appliedWallet,
        availablePointBalance: actualPointBalance,
        availableWalletBalance: actualWalletBalance,
        valuePerPoint: valuePerPoint
    })

    // --- 5. AUTO-ADJUST (Ép ngược state nếu nhập lố) ---
    if (displayData.isPending) {
        if (appliedPoint !== displayData.safePoints) {
            setAppliedPoint(displayData.safePoints)
        }
        if (appliedWallet !== displayData.safeWallet) {
            setAppliedWallet(displayData.safeWallet)
        }
    }

    // --- 6. HANDLERS ---
    const handleApplyVouchers = (ship?: Voucher, discount?: Voucher) => {
        setShippingVoucher(ship)
        setDiscountVoucher(discount)
    }

    const isPaymentStatusToastShown = useRef(false)
    useEffect(() => {
        const paymentStatus = searchParams.get("payment_status")
        if (paymentStatus && !isPaymentStatusToastShown.current) {
            isPaymentStatusToastShown.current = true
            if (paymentStatus === "success") {
                addToast({ title: "Thành công", description: "Đã thanh toán.", color: "success" })
            } else if (paymentStatus === "failed") {
                addToast({ title: "Thất bại", description: "Giao dịch lỗi.", color: "danger" })
            }
            router.replace(pathname, { scroll: false })
        }
    }, [searchParams, pathname, router])

    if (isLoading || isRecipeLoading || !order) {
        return (
            <div className="flex h-[80vh] items-center justify-center">
                <Spinner label="Đang tải hoá đơn..." />
            </div>
        )
    }

    return (
        <div className="w-full md:w-3xl max-w-5xl mx-auto p-4 md:p-10 space-y-8">
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
                <CardHeader className="flex flex-col p-8 md:p-12 pb-0! space-y-8">
                    <OrderDetailHeader order={order} />
                </CardHeader>

                <div className="px-8 md:px-12">
                    <div className="my-4 border-t-2 border-dashed border-default-500 w-full relative">
                        <div className="absolute -left-15 -top-3 w-6 h-6 rounded-full bg-background shadow-inner md:block hidden" />
                        <div className="absolute -right-15 -top-3 w-6 h-6 rounded-full bg-background shadow-inner md:block hidden" />
                    </div>
                </div>

                <CardBody className="px-8 py-5 md:p-12">
                    <div className="min-h-50">
                        {/* TRUYỀN DATA ĐÃ TÍNH XUỐNG ORDER DETAIL */}
                        <OrderDetail
                            key={order.id}
                            order={order}
                            user={user}
                            wallet={wallet}
                            recipes={recipes}
                            isCustomer={isCustomer}
                            appliedShippingVoucher={shippingVoucher}
                            appliedDiscountVoucher={discountVoucher}
                            onOpenVoucherSelector={() => setIsVoucherOpen(true)}
                            // State Point
                            appliedPoint={appliedPoint}
                            setAppliedPoint={setAppliedPoint}
                            // State Ví (Để phòng hờ OrderDetail cần truyền tiếp xuống Footer bên trong, nếu có)
                            appliedWallet={appliedWallet}
                            setAppliedWallet={setAppliedWallet}
                            // Truyền các con số đã được Hook tính
                            giftItems={displayData.giftItems}
                            purchasedItems={displayData.purchasedItems}
                            discounts={displayData.discounts}
                            calculatedTax={displayData.calculatedTax}
                            pointValueDeduction={displayData.pointValueDeduction}
                            totalAfterTax={displayData.totalAfterTax}
                            finalAmount={displayData.finalAmount}
                            isPending={displayData.isPending}
                        />
                    </div>
                </CardBody>

                {(isStaff || isCustomer) && (
                    <>
                        <div className="print-hidden px-8 md:px-12">
                            <div className="my-4 border-t-2 border-dashed border-default-500 w-full relative"></div>
                        </div>
                        <CardFooter className="print-hidden flex flex-col p-8 md:p-12 gap-8 bg-slate-50/30">
                            {/* TRUYỀN DATA XUỐNG FOOTER */}
                            <OrderDetailFooter
                                order={order}
                                wallet={wallet}
                                totalAfterTax={displayData.totalAfterTax}
                                finalAmount={displayData.finalAmount}
                                appliedWallet={appliedWallet}
                                setAppliedWallet={setAppliedWallet}
                                appliedPoint={appliedPoint}
                                shippingVoucher={shippingVoucher}
                                discountVoucher={discountVoucher}
                            />
                        </CardFooter>
                    </>
                )}
            </Card>

            {isCustomer && (
                <VoucherSelector
                    order={order}
                    isOpen={isVoucherOpen}
                    onOpenChange={setIsVoucherOpen}
                    onApplyVouchers={handleApplyVouchers}
                    appliedShippingVoucher={shippingVoucher}
                    appliedDiscountVoucher={discountVoucher}
                />
            )}
        </div>
    )
}
