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
    Chip
} from "@heroui/react"
import { Plus, Edit, Trash2, Calendar, Repeat } from "lucide-react"
import { useGetMyAvailabilityHistory, useDeleteAvailability } from "@/features/shifts/hooks"
import { ShiftAvailabilityResponse } from "@/features/shifts/models/response"
import { confirmDialog } from "@/hooks"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ButtonStyled, PaginationStyled } from "@/components"
import { ShiftAvailabilityModal } from "@/features/shifts/components"
import { DayOfWeekLabel } from "@/constants/labels"

export default function ShiftAvailabilitiesPageView() {
    const [page, setPage] = useState(1)
    const { data, isLoading } = useGetMyAvailabilityHistory({
        query: {},
        pagination: { page, pageSize: 10 }
    })

    const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure()
    const [selectedItem, setSelectedItem] = useState<ShiftAvailabilityResponse | undefined>()

    const deleteMutation = useDeleteAvailability()

    const renderCell = (item: ShiftAvailabilityResponse, columnKey: React.Key) => {
        switch (columnKey) {
            case "type":
                return (
                    <Chip color={item.isAvailable ? "success" : "danger"} variant="flat" size="sm">
                        {item.isAvailable ? "RẢNH" : "BẬN"}
                    </Chip>
                )
            case "schedule":
                return (
                    <div className="flex flex-col">
                        <span className="font-medium">
                            {item.isRecurring ? (
                                <span className="flex items-center gap-1 text-primary">
                                    <Repeat size={14} /> {DayOfWeekLabel[item.dayOfWeek!]}
                                </span>
                            ) : (
                                <span className="flex items-center gap-1">
                                    <Calendar size={14} /> {item.specificDate}
                                </span>
                            )}
                        </span>
                        <span className="text-tiny text-default-500">
                            {item.startTime.slice(0, 5)} - {item.endTime.slice(0, 5)}
                        </span>
                    </div>
                )
            case "note":
                return (
                    <span className="text-default-600 italic text-small">{item.note || "---"}</span>
                )
            case "actions":
                return (
                    <div className="flex gap-2 justify-center">
                        <Button
                            isIconOnly
                            size="sm"
                            variant="light"
                            onPress={() => {
                                setSelectedItem(item)
                                onOpen()
                            }}
                        >
                            <Edit size={18} />
                        </Button>
                        <Button
                            isIconOnly
                            size="sm"
                            variant="light"
                            color="danger"
                            onPress={async () => {
                                if (
                                    await confirmDialog({
                                        title: "Xóa lịch",
                                        // message: "Bạn chắc chắn muốn xoá không?",
                                        variant: "danger"
                                    })
                                )
                                    deleteMutation.mutate(item.id)
                            }}
                        >
                            <Trash2 size={18} />
                        </Button>
                    </div>
                )
            default:
                return null
        }
    }

    return (
        <div className="max-w-7xl mx-auto p-4 md:p-8">
            <div className="flex justify-between mb-6">
                <DashboardHeader
                    icon={<Calendar className="text-white" />}
                    title="Lịch Cá Nhân"
                    description="Quản lý lịch rảnh/bận để Manager xếp ca"
                />
                <ButtonStyled
                    color="primary"
                    startContent={<Plus size={20} />}
                    onPress={() => {
                        setSelectedItem(undefined)
                        onOpen()
                    }}
                >
                    Thêm Lịch Mới
                </ButtonStyled>
            </div>

            <Table aria-label="Availability Table">
                <TableHeader>
                    <TableColumn key="type">TRẠNG THÁI</TableColumn>
                    <TableColumn key="schedule">THỜI GIAN</TableColumn>
                    <TableColumn key="note">GHI CHÚ</TableColumn>
                    <TableColumn key="actions" align="center">
                        THAO TÁC
                    </TableColumn>
                </TableHeader>
                <TableBody items={data?.items || []} isLoading={isLoading}>
                    {(item) => (
                        <TableRow key={item.id}>
                            {(key) => <TableCell>{renderCell(item, key)}</TableCell>}
                        </TableRow>
                    )}
                </TableBody>
            </Table>
            {(data?.items || []).length > 0 && !isLoading && (
                <PaginationStyled
                    page={page ?? 1}
                    total={data?.totalPages ?? 10}
                    onChange={(page: number) => setPage(page)}
                />
            )}

            <ShiftAvailabilityModal
                isOpen={isOpen}
                onOpenChange={onOpenChange}
                onClose={onClose}
                selectedItem={selectedItem}
            />
        </div>
    )
}
