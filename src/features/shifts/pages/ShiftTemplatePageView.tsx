// src/features/manager/pages/ShiftTemplatePage.tsx

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
import { Plus, Edit, Trash2, Clock } from "lucide-react"
import { useGetShiftTemplates, useDeleteShiftTemplate } from "@/features/shifts/hooks"
import { ShiftTemplateResponse } from "../models/response"
import { confirmDialog } from "@/hooks"
import { ShiftTemplateModal } from "../components/ShiftTemplateModal"

export const ShiftTemplatePageView = ({ storeId }: { storeId?: string }) => {
    const { data, isLoading } = useGetShiftTemplates({
        params: { storeId: storeId },
        enabled: !!storeId
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
                return <span>{template.breakDurationMinutes} phút</span>
            case "actions":
                // Không cho sửa/xóa Template của hệ thống (storeId === null)
                if (!template.storeId) {
                    return (
                        <Chip size="sm" variant="dot" color="default">
                            Read Only
                        </Chip>
                    )
                }
                return (
                    <div className="flex items-center justify-center gap-2">
                        <Tooltip content="Chỉnh sửa">
                            <span
                                className="text-lg text-default-400 cursor-pointer active:opacity-50 hover:text-primary"
                                onClick={() => handleEdit(template)}
                            >
                                <Edit size={18} />
                            </span>
                        </Tooltip>
                        <Tooltip content="Xóa" color="danger">
                            <span
                                className="text-lg text-danger cursor-pointer active:opacity-50 hover:text-danger-400"
                                onClick={() => handleDelete(template.id)}
                            >
                                <Trash2 size={18} />
                            </span>
                        </Tooltip>
                    </div>
                )
            default:
                return null
        }
    }

    if (!storeId) return null

    return (
        <div className="p-6">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-2xl font-bold">Mẫu Ca Làm Việc</h1>
                    <p className="text-gray-500 text-sm">
                        Quản lý các khung giờ làm việc thường dùng
                    </p>
                </div>
                <Button color="primary" startContent={<Plus size={20} />} onPress={handleCreate}>
                    Tạo Mẫu Mới
                </Button>
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
                storeId={storeId}
            />
        </div>
    )
}
