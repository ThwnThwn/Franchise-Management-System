"use client"

import React, { useState } from "react"
import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Button,
    Chip,
    Tooltip,
    Input
} from "@heroui/react"
import { Search, Plus, Edit, Trash2, Building2, User, Phone, Mail, MapPin } from "lucide-react"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ButtonStyled, InputStyled } from "@/components"
import {
    useGetSuppliers,
    useCreateSupplier,
    useUpdateSupplier,
    useDeleteSupplier,
    CreateSupplierModal,
    EditSupplierModal
} from "@/features/suppliers"
import { SupplierItem } from "../models/response"

export function SuppliersPage() {
    // ── State ──
    const [search, setSearch] = useState("")
    const [page, setPage] = useState(1)
    const [pageSize] = useState(20)

    const [isCreateOpen, setIsCreateOpen] = useState(false)
    const [isEditOpen, setIsEditOpen] = useState(false)
    const [selectedSupplier, setSelectedSupplier] = useState<SupplierItem | null>(null)

    // ── Queries & Mutations ──
    const { data, isLoading } = useGetSuppliers({
        params: {
            name: search || undefined,
            page,
            pageSize
        }
    })

    const createMutation = useCreateSupplier({
        onSuccess: () => setIsCreateOpen(false)
    })

    const updateMutation = useUpdateSupplier({
        onSuccess: () => {
            setIsEditOpen(false)
            setSelectedSupplier(null)
        }
    })

    const deleteMutation = useDeleteSupplier()

    // ── Handlers ──
    const handleEdit = (supplier: SupplierItem) => {
        setSelectedSupplier(supplier)
        setIsEditOpen(true)
    }

    const handleDelete = (supplier: SupplierItem) => {
        if (confirm(`Bạn có chắc muốn xóa nhà cung cấp "${supplier.name}"?`)) {
            deleteMutation.mutate(supplier.id)
        }
    }

    const suppliers = data?.items ?? []
    const totalCount = data?.totalCount ?? 0

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            <DashboardHeader
                icon={<Building2 className="w-6 h-6 text-white" />}
                title="Quản lý nhà cung cấp"
                description="Danh sách các đối tác cung cấp nguyên liệu cho hệ thống"
            />

            {/* ── TOOLBAR ── */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <div className="w-full sm:w-80">
                    <InputStyled
                        placeholder="Tìm kiếm nhà cung cấp..."
                        startContent={<Search size={18} className="text-slate-400" />}
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>

                <ButtonStyled
                    color="primary"
                    startContent={<Plus size={18} />}
                    onPress={() => setIsCreateOpen(true)}
                >
                    Thêm nhà cung cấp
                </ButtonStyled>
            </div>

            {/* ── TABLE ── */}
            <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
                <Table
                    aria-label="Danh sách nhà cung cấp"
                    classNames={{
                        base: "max-h-[70vh] overflow-auto",
                        table: "min-w-full",
                        th: "bg-slate-50 text-slate-600 font-semibold py-4 border-b border-slate-100",
                        td: "py-4 border-b border-slate-50"
                    }}
                >
                    <TableHeader>
                        <TableColumn>NHÀ CUNG CẤP</TableColumn>
                        <TableColumn>LIÊN HỆ</TableColumn>
                        <TableColumn>ĐỊA CHỈ</TableColumn>
                        <TableColumn>TRẠNG THÁI</TableColumn>
                        <TableColumn align="center">HÀNH ĐỘNG</TableColumn>
                    </TableHeader>
                    <TableBody isLoading={isLoading} emptyContent="Không tìm thấy nhà cung cấp nào">
                        {suppliers.map((supplier: SupplierItem) => (
                            <TableRow key={supplier.id}>
                                <TableCell>
                                    <div className="flex flex-col">
                                        <span className="font-bold text-slate-900">
                                            {supplier.name}
                                        </span>
                                        <span className="text-xs text-slate-400">
                                            #{supplier.code}
                                        </span>
                                    </div>
                                </TableCell>
                                <TableCell>
                                    <div className="flex flex-col gap-1">
                                        {supplier.contactPerson && (
                                            <div className="flex items-center gap-2 text-sm text-slate-600">
                                                <User size={14} className="text-slate-400" />
                                                {supplier.contactPerson}
                                            </div>
                                        )}
                                        {supplier.phone && (
                                            <div className="flex items-center gap-2 text-sm text-slate-600">
                                                <Phone size={14} className="text-slate-400" />
                                                {supplier.phone}
                                            </div>
                                        )}
                                        {supplier.email && (
                                            <div className="flex items-center gap-2 text-sm text-slate-600">
                                                <Mail size={14} className="text-slate-400" />
                                                {supplier.email}
                                            </div>
                                        )}
                                    </div>
                                </TableCell>
                                <TableCell>
                                    {supplier.address && (
                                        <div className="flex items-start gap-2 max-w-xs text-sm text-slate-600">
                                            <MapPin size={14} className="text-slate-400 mt-1" />
                                            <span className="line-clamp-2">{supplier.address}</span>
                                        </div>
                                    )}
                                </TableCell>
                                <TableCell>
                                    <Chip
                                        color={supplier.isActive ? "success" : "default"}
                                        variant="flat"
                                        size="sm"
                                    >
                                        {supplier.isActive ? "Đang hoạt động" : "Ngừng hợp tác"}
                                    </Chip>
                                </TableCell>
                                <TableCell>
                                    <div className="flex justify-center items-center gap-2">
                                        <Tooltip content="Chỉnh sửa">
                                            <Button
                                                isIconOnly
                                                variant="light"
                                                onPress={() => handleEdit(supplier)}
                                            >
                                                <Edit size={18} className="text-slate-400" />
                                            </Button>
                                        </Tooltip>
                                        <Tooltip content="Xóa" color="danger">
                                            <Button
                                                isIconOnly
                                                variant="light"
                                                color="danger"
                                                onPress={() => handleDelete(supplier)}
                                            >
                                                <Trash2 size={18} />
                                            </Button>
                                        </Tooltip>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </div>

            {/* ── MODALS ── */}
            <CreateSupplierModal
                isOpen={isCreateOpen}
                onClose={() => setIsCreateOpen(false)}
                onSubmit={(req) => createMutation.mutate(req)}
                isLoading={createMutation.isPending}
            />

            <EditSupplierModal
                isOpen={isEditOpen}
                onClose={() => setIsEditOpen(false)}
                supplier={selectedSupplier}
                onSubmit={(id, req) => updateMutation.mutate({ id, req })}
                isLoading={updateMutation.isPending}
            />
        </div>
    )
}
