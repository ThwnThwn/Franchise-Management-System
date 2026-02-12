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
    useDisclosure,
    Chip,
    Tooltip
} from "@heroui/react"
import { Plus, Edit, Trash2, Clock, Lock, FileClock } from "lucide-react"
import { useGetShiftTemplates, useDeleteShiftTemplate } from "@/features/shifts/hooks"
import { ShiftTemplateResponse } from "@/features/shifts/models/response"
import { confirmDialog, useTokenStore } from "@/hooks"
import { ShiftTemplateModal } from "@/features/shifts/components"
import { RoleSlug } from "@/constants/enums"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ButtonStyled } from "@/components"
import { formatCurrency } from "@/utils/helpers/currency"

export default function ShiftTemplatesPageView() {
    const storeId = useTokenStore((s) => s.storeId)
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isAdmin = roleSlug === RoleSlug.Admin

    const { data, isLoading } = useGetShiftTemplates({
        params: { storeId: storeId || undefined }
    })

    // 2. Modal Logic
    const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure()
    const [selectedTemplate, setSelectedTemplate] = useState<ShiftTemplateResponse | undefined>(
        undefined
    )

    const handleCreate = () => {
        setSelectedTemplate(undefined) // Clear selection -> Create mode
        onOpen()
    }

    const handleEdit = (template: ShiftTemplateResponse) => {
        // Chỉ cho sửa template của Store mình (StoreId != null)
        if (!template.storeId) return
        setSelectedTemplate(template)
        onOpen()
    }

    // 3. Delete Logic
    const deleteMutation = useDeleteShiftTemplate()
    const handleDelete = async (id: string) => {
        const isConfirmed = await confirmDialog({
            title: "Xóa Mẫu Ca",
            message: "Hành động này không thể hoàn tác. Bạn chắc chứ?",
            variant: "danger"
        })

        if (isConfirmed) {
            deleteMutation.mutate({ id })
        }
    }

    // 4. Render Columns
    const renderCell = (template: ShiftTemplateResponse, columnKey: React.Key) => {
        switch (columnKey) {
            case "code":
                return <span className="font-semibold">{template.code}</span>
            case "name":
                return (
                    <div className="flex flex-col">
                        <span className="text-small">{template.name}</span>
                        {/* Check Global vs Custom */}
                        {!template.storeId && (
                            <span className="text-tiny text-default-400">System Default</span>
                        )}
                    </div>
                )
            case "time":
                return (
                    <Chip startContent={<Clock size={14} />} variant="flat" size="sm">
                        {template.startTime.slice(0, 5)} - {template.endTime.slice(0, 5)}
                    </Chip>
                )
            case "break":
                return <span>{formatCurrency(template.breakDurationMinutes)} phút</span>
            case "actions": {
                const isSystemTemplate = !template.storeId
                const isReadOnly = !isAdmin && isSystemTemplate

                if (isReadOnly) {
                    return (
                        <div className="flex justify-center">
                            <Chip
                                size="sm"
                                variant="flat"
                                color="warning"
                                startContent={<Lock size={12} />}
                                className="text-xs"
                            >
                                Mẫu hệ thống
                            </Chip>
                        </div>
                    )
                }

                return (
                    <div className="flex items-center justify-center gap-2">
                        <Tooltip content="Chỉnh sửa">
                            <Button
                                isIconOnly
                                size="sm"
                                variant="light"
                                color="primary"
                                onPress={() => handleEdit(template)}
                            >
                                <Edit size={18} />
                            </Button>
                        </Tooltip>

                        <Tooltip content="Xóa" color="danger">
                            <Button
                                isIconOnly
                                size="sm"
                                variant="light"
                                color="danger"
                                onPress={() => handleDelete(template.id)}
                            >
                                <Trash2 size={18} />
                            </Button>
                        </Tooltip>
                    </div>
                )
            }
            default:
                return null
        }
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8">
            <div className="flex justify-between">
                <DashboardHeader
                    icon={<FileClock className="w-6 h-6 text-white" />}
                    title="Mẫu Ca Làm Việc"
                    description="Quản lý các khung giờ làm việc thường dùng"
                />
                <ButtonStyled
                    color="primary"
                    startContent={<Plus size={20} />}
                    onPress={handleCreate}
                >
                    Tạo Mẫu Mới
                </ButtonStyled>
            </div>

            <Table aria-label="Shift Templates Table">
                <TableHeader>
                    <TableColumn key="code">MÃ</TableColumn>
                    <TableColumn key="name">TÊN CA</TableColumn>
                    <TableColumn key="time">KHUNG GIỜ</TableColumn>
                    <TableColumn key="break">NGHỈ (PHÚT)</TableColumn>
                    <TableColumn key="actions" align="center">
                        THAO TÁC
                    </TableColumn>
                </TableHeader>
                <TableBody
                    items={data || []}
                    isLoading={isLoading}
                    emptyContent={"Chưa có mẫu ca nào. Hãy tạo mới!"}
                >
                    {(item) => (
                        <TableRow key={item.id}>
                            {(columnKey) => <TableCell>{renderCell(item, columnKey)}</TableCell>}
                        </TableRow>
                    )}
                </TableBody>
            </Table>

            {/* Modal Form */}
            <ShiftTemplateModal
                isOpen={isOpen}
                onOpenChange={onOpenChange}
                onClose={onClose}
                selectedTemplate={selectedTemplate}
                storeId={storeId || undefined}
            />
        </div>
    )
}
