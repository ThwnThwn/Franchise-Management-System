"use client"

import { useState } from "react"
import { useGetAllVouchers, useUpdateVoucherStatus } from "@/features/vouchers/hooks"
import { VoucherModal, VoucherViewModal, VouchersTable } from "@/features/vouchers/components"
import { Voucher } from "@/features/vouchers/models/response"
import { Button } from "@heroui/react"
import { Plus, Search } from "lucide-react"
import { InputStyled } from "@/components"

export default function VouchersPageView() {
    const { data: vouchers, isLoading } = useGetAllVouchers()
    const { updateStatusWithConfirm } = useUpdateVoucherStatus()

    // Fetch products to resolve variant names for CSV export
    // const { data: productsData } = useGetAllProducts({
    //     params: {},
    //     pagination: { page: 1, pageSize: 100 },
    //     enabled: true
    // })

    // const variantMap = useMemo<Record<string, string>>(() => {
    //     const map: Record<string, string> = {}
    //     productsData?.items?.forEach((p) => {
    //         p.variants.forEach((v) => {
    //             map[v.sku] = `${p.name} - ${v.sku}${v.attributes ? ` (${v.attributes})` : ""}`
    //         })
    //     })
    //     return map
    // }, [productsData])

    const [isModalOpen, setIsModalOpen] = useState(false)
    const [viewingVoucher, setViewingVoucher] = useState<Voucher>()
    // const [isAssignModalOpen, setIsAssignModalOpen] = useState(false)
    // const [selectedVoucher, setSelectedVoucher] = useState<Voucher | undefined>(undefined)
    const [searchQuery, setSearchQuery] = useState("")

    const handleCreate = () => {
        // setSelectedVoucher(undefined)
        setIsModalOpen(true)
    }

    const handleToggleStatus = (id: string, isActive: boolean) => {
        updateStatusWithConfirm(id, isActive)
    }

    const filteredVouchers =
        vouchers?.filter((v) => v.code.toLowerCase().includes(searchQuery.toLowerCase())) || []

    return (
        <div className="p-6 space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold">Quản lý Voucher</h1>
                <Button color="primary" onPress={handleCreate} startContent={<Plus size={18} />}>
                    Tạo Voucher Mới
                </Button>
            </div>

            <div className="bg-default-50/50 p-4 rounded-2xl border border-default-100 flex items-center gap-4">
                <InputStyled
                    placeholder="Tìm kiếm theo mã code..."
                    value={searchQuery}
                    onValueChange={setSearchQuery}
                    startContent={<Search className="text-default-400" size={18} />}
                    className="max-w-xs"
                />
            </div>

            <div className="bg-background rounded-2xl border border-default-100 shadow-sm overflow-hidden p-0">
                <VouchersTable
                    vouchers={filteredVouchers}
                    isLoading={isLoading}
                    onToggleStatus={handleToggleStatus}
                    onRowAction={setViewingVoucher}
                />
            </div>

            <VoucherModal
                isOpen={isModalOpen}
                onOpenChange={setIsModalOpen}
                // voucher={selectedVoucher}
                // onSuccess={refetch}
            />
            <VoucherViewModal
                isOpen={!!viewingVoucher}
                onOpenChange={(isOpen) => {
                    if (!isOpen) setViewingVoucher(undefined)
                }}
                voucher={viewingVoucher}
            />
        </div>
    )
}
