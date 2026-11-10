"use client"

import { useDisclosure, Spinner, Card, CardBody, Chip, addToast } from "@heroui/react"
import { Wallet, PlusCircle } from "lucide-react"
import { DashboardHeader } from "@/features/dashboard"
import { ButtonStyled } from "@/components"

import { useGetMyWallet } from "@/features/wallets/hooks"
import { WalletStatusLabels, WalletStatusColors } from "@/features/wallets/constants"
import { TopUpModal, WalletTransactionTable } from "@/features/wallets/components"
import { formatCurrency } from "@/utils/helpers/currency"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { useEffect, useRef } from "react"

export default function WalletPageView() {
    const { data: wallet, isLoading } = useGetMyWallet()

    const { isOpen, onOpen, onClose } = useDisclosure()

    const router = useRouter()
    const pathname = usePathname()
    const searchParams = useSearchParams()
    const isPaymentStatusToastShown = useRef(false)
    useEffect(() => {
        const paymentStatus = searchParams.get("payment_status")
        // const message = searchParams.get("message")

        if (paymentStatus && !isPaymentStatusToastShown.current) {
            isPaymentStatusToastShown.current = true

            if (paymentStatus === "success") {
                addToast({
                    title: "Thanh toán thành công",
                    description: "Yêu cầu nạp tiền của bạn đã được ghi nhận thanh toán.",
                    color: "success"
                })
            } else if (paymentStatus === "failed") {
                addToast({
                    title: "Thanh toán thất bại",
                    description: "Giao dịch đã bị hủy hoặc có lỗi xảy ra.",
                    color: "danger"
                })
            }

            // 3. XÓA DẤU VẾT: Thay thế URL hiện tại bằng URL gốc
            router.replace(pathname, { scroll: false })
        }
    }, [searchParams, pathname, router])

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-6">
            <DashboardHeader
                icon={<Wallet className="text-white" />}
                title="Ví Điện Tử"
                description="Quản lý số dư và theo dõi biến động tài khoản của bạn"
            />

            {isLoading ? (
                <div className="flex justify-center my-10">
                    <Spinner color="primary" />
                </div>
            ) : wallet ? (
                <>
                    {/* Thẻ Ví */}
                    <Card className="bg-linear-to-br from-[#8B4513] to-[#5D2E0A] text-white shadow-xl">
                        <CardBody className="p-8 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
                            <div>
                                <p className="text-white/80 text-sm font-medium mb-2 uppercase tracking-wider">
                                    Số dư khả dụng
                                </p>
                                <h2 className="text-4xl md:text-5xl font-black tabular-nums text-[#f3e5ab]">
                                    {formatCurrency(wallet.balance)}
                                </h2>
                                <div className="mt-4 flex items-center gap-3">
                                    <Chip
                                        size="sm"
                                        color={WalletStatusColors[wallet.status]}
                                        variant="flat"
                                        className="bg-white/20 text-white font-medium border-none"
                                    >
                                        {WalletStatusLabels[wallet.status]}
                                    </Chip>
                                </div>
                            </div>

                            <ButtonStyled
                                color="primary"
                                size="lg"
                                className="bg-white text-[#8B4513] font-bold px-8 shadow-md hover:scale-105 transition-transform"
                                onPress={onOpen}
                                startContent={<PlusCircle size={20} />}
                            >
                                Nạp Tiền
                            </ButtonStyled>
                        </CardBody>
                    </Card>

                    {/* Bảng giao dịch */}
                    <div className="bg-white rounded-2xl p-6 shadow-sm border border-default-100">
                        <h3 className="text-xl font-bold mb-4">Lịch sử giao dịch</h3>
                        <WalletTransactionTable />
                    </div>
                </>
            ) : (
                <div className="text-center text-default-500 py-10">
                    Không tìm thấy thông tin ví.
                </div>
            )}

            <TopUpModal isOpen={isOpen} onClose={onClose} />
        </div>
    )
}
