"use client"

import { useMemo, useState } from "react"
import { Order } from "@/features/orders/models/response"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Input,
    Tabs,
    Tab,
    ScrollShadow
} from "@heroui/react"
import { Search, Ticket } from "lucide-react"
import { Voucher } from "@/features/vouchers/models/response"
import { useGetMyVouchers, VoucherType } from "@/features/vouchers"

import { ModalStyled } from "@/components"
import { VoucherDetailPanel } from "./VoucherDetailPanel"
import { VoucherList } from "./VoucherList"

interface VoucherSelectorProps {
    order?: Order
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    onApplyVouchers: (shippingVoucher?: Voucher, discountVoucher?: Voucher) => void
    appliedShippingVoucher?: Voucher
    appliedDiscountVoucher?: Voucher
}

export function VoucherSelector({
    order,
    isOpen,
    onOpenChange,
    onApplyVouchers,
    appliedShippingVoucher,
    appliedDiscountVoucher
}: VoucherSelectorProps) {
    const { data: vouchers, isLoading } = useGetMyVouchers()

    // State
    const [selectedShipping, setSelectedShipping] = useState<Voucher | undefined>(undefined)
    const [selectedDiscount, setSelectedDiscount] = useState<Voucher | undefined>(undefined)
    const [searchQuery, setSearchQuery] = useState("")
    const [activeTab, setActiveTab] = useState<"shipping" | "discount">("discount")

    // Sync state khi modal mở
    const [prevIsOpen, setPrevIsOpen] = useState(isOpen)
    if (isOpen !== prevIsOpen) {
        setPrevIsOpen(isOpen)
        if (isOpen) {
            setSelectedShipping(appliedShippingVoucher)
            setSelectedDiscount(appliedDiscountVoucher)
            setSearchQuery("")
            setActiveTab(!appliedShippingVoucher ? "shipping" : "discount")
        }
    }

    const filteredVouchers = useMemo(() => {
        if (!searchQuery.trim()) return vouchers || []
        return (vouchers || []).filter((v) =>
            v.code.toLowerCase().includes(searchQuery.trim().toLowerCase())
        )
    }, [vouchers, searchQuery])

    const shippingVouchers = useMemo(() => {
        return filteredVouchers.filter(
            (v) =>
                v.type === VoucherType.SHIPPING_FIXED_AMOUNT ||
                v.type === VoucherType.SHIPPING_PERCENTAGE
        )
    }, [filteredVouchers])

    const discountVouchers = useMemo(() => {
        return filteredVouchers.filter(
            (v) =>
                v.type === VoucherType.DISCOUNT_FIXED_AMOUNT ||
                v.type === VoucherType.DISCOUNT_PERCENTAGE ||
                v.type === VoucherType.GIFT
        )
    }, [filteredVouchers])

    // Hàm chọn (Toggle: bấm lại vào cái đã chọn thì bỏ chọn)
    const handleSelect = (voucher: Voucher, type: "shipping" | "discount") => {
        if (type === "shipping") {
            setSelectedShipping((prev) => (prev?.id === voucher.id ? undefined : voucher))
        } else {
            setSelectedDiscount((prev) => (prev?.id === voucher.id ? undefined : voucher))
        }
    }

    const handleApply = () => {
        onApplyVouchers(selectedShipping, selectedDiscount)
        onOpenChange(false)
    }

    const activeSelectedVoucher = activeTab === "shipping" ? selectedShipping : selectedDiscount

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} size="4xl">
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-3 border-b pb-4 bg-default-50">
                            <h2 className="text-xl font-black flex items-center gap-2">
                                <Ticket className="text-warning-600" /> Chọn Voucher
                            </h2>
                        </ModalHeader>

                        <ModalBody className="p-0 bg-default-50/50 overflow-hidden">
                            <div className="flex flex-col md:flex-row h-full">
                                {/* IMPORT CỘT TRÁI (CHI TIẾT) */}
                                <div className="hidden md:block md:w-5/12 p-5 bg-white border-r border-default-100 h-120">
                                    <VoucherDetailPanel voucher={activeSelectedVoucher} />
                                </div>

                                {/* Cột Phải: Danh sách & Tabs */}
                                <div className="w-full md:w-7/12 flex flex-col h-120">
                                    <div className="px-5 pt-4 pb-2 bg-white">
                                        <Input
                                            placeholder="Tìm mã voucher nhanh..."
                                            value={searchQuery}
                                            onValueChange={setSearchQuery}
                                            startContent={
                                                <Search size={16} className="text-default-400" />
                                            }
                                            variant="bordered"
                                            className="w-full"
                                            isClearable
                                        />
                                    </div>

                                    {isLoading ? (
                                        <div className="flex justify-center items-center flex-1">
                                            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-warning"></div>
                                        </div>
                                    ) : (
                                        <Tabs
                                            fullWidth
                                            size="md"
                                            color="warning"
                                            variant="underlined"
                                            selectedKey={activeTab}
                                            onSelectionChange={(k) => setActiveTab(k as any)}
                                            classNames={{
                                                tabList: "bg-white px-4",
                                                cursor: "w-full"
                                            }}
                                        >
                                            <Tab
                                                key="shipping"
                                                title={
                                                    <div className="font-bold">
                                                        Giao hàng{" "}
                                                        {shippingVouchers.length > 0 &&
                                                            `(${shippingVouchers.length})`}
                                                    </div>
                                                }
                                            >
                                                <ScrollShadow className="px-5 pt-4 h-87.5">
                                                    {/* IMPORT LIST VẬN CHUYỂN */}
                                                    <VoucherList
                                                        vouchers={shippingVouchers}
                                                        order={order}
                                                        selectedVoucher={selectedShipping}
                                                        searchQuery={searchQuery}
                                                        onSelect={(v) =>
                                                            handleSelect(v, "shipping")
                                                        }
                                                    />
                                                </ScrollShadow>
                                            </Tab>

                                            <Tab
                                                key="discount"
                                                title={
                                                    <div className="font-bold">
                                                        Giảm Giá & Quà{" "}
                                                        {discountVouchers.length > 0 &&
                                                            `(${discountVouchers.length})`}
                                                    </div>
                                                }
                                            >
                                                <ScrollShadow className="px-5 pt-4 h-87.5">
                                                    {/* IMPORT LIST GIẢM GIÁ */}
                                                    <VoucherList
                                                        vouchers={discountVouchers}
                                                        order={order}
                                                        selectedVoucher={selectedDiscount}
                                                        searchQuery={searchQuery}
                                                        onSelect={(v) =>
                                                            handleSelect(v, "discount")
                                                        }
                                                    />
                                                </ScrollShadow>
                                            </Tab>
                                        </Tabs>
                                    )}
                                </div>
                            </div>
                        </ModalBody>

                        <ModalFooter className="border-t bg-white shadow-[0_-10px_15px_-3px_rgba(0,0,0,0.05)]">
                            <div className="flex justify-between items-center w-full">
                                <div className="text-xs font-medium text-default-500">
                                    Đã chọn:{" "}
                                    <span className="text-warning-600 font-bold">
                                        {
                                            [selectedShipping, selectedDiscount].filter(Boolean)
                                                .length
                                        }
                                        /2
                                    </span>
                                </div>
                                <div className="flex gap-2">
                                    <Button
                                        color="default"
                                        variant="light"
                                        onPress={onClose}
                                        className="font-bold"
                                    >
                                        Trở lại
                                    </Button>
                                    <Button
                                        color="warning"
                                        className="font-bold px-8 shadow-md text-white"
                                        onPress={handleApply}
                                    >
                                        Đồng ý
                                    </Button>
                                </div>
                            </div>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
