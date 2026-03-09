"use client"

import React, { useState } from "react"
import { useGetAllVouchers, useUpdateVoucherStatus, useDeleteVoucher } from "@/features/orders/hooks/useVoucher"
import { VouchersTable } from "../../components/VouchersTable"
import { VoucherModal } from "../../components/VoucherModal"
import { Voucher } from "@/features/orders/models/voucher"
import { Button, Input } from "@heroui/react"
import { Plus, Search } from "lucide-react"

export const VouchersPageView = () => {
    const { data: vouchers, isLoading, refetch } = useGetAllVouchers()
    const { mutate: updateStatus } = useUpdateVoucherStatus()
    const { mutate: deleteVoucher } = useDeleteVoucher()

    const [isModalOpen, setIsModalOpen] = useState(false)
    const [selectedVoucher, setSelectedVoucher] = useState<Voucher | null>(null)
    const [searchQuery, setSearchQuery] = useState("")

    const handleCreate = () => {
        setSelectedVoucher(null)
        setIsModalOpen(true)
    }

    const handleEdit = (voucher: Voucher) => {
        setSelectedVoucher(voucher)
        setIsModalOpen(true)
    }

    const handleToggleStatus = (id: string, is_active: boolean) => {
        updateStatus({ id, req: { is_active: !is_active } }, {
            onSuccess: () => refetch()
        })
    }

    const handleDelete = (id: string) => {
        if (confirm("Bạn có chắc muốn xóa Voucher này?")) {
            deleteVoucher(id, {
                onSuccess: () => refetch()
            })
        }
    }

    const filteredVouchers = vouchers?.filter(v =>
        v.code.toLowerCase().includes(searchQuery.toLowerCase())
    ) || []

    return (
        <div className="p-6 space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold">Quản lý Voucher</h1>
                <Button color="primary" onPress={handleCreate} startContent={<Plus size={18} />}>
                    Tạo Voucher Mới
                </Button>
            </div>

            <div className="bg-default-50/50 p-4 rounded-2xl border border-default-100 flex items-center gap-4">
                <Input
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
                    onEdit={handleEdit}
                    onToggleStatus={handleToggleStatus}
                    onDelete={handleDelete}
                />
            </div>

            <VoucherModal
                isOpen={isModalOpen}
                onOpenChange={setIsModalOpen}
                voucher={selectedVoucher}
                onSuccess={refetch}
            />
        </div>
    )
}
