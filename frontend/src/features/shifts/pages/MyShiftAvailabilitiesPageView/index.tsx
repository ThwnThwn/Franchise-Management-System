"use client"

import React, { useState, useMemo, useCallback } from "react"
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
    Switch,
    cn
} from "@heroui/react"
import { Plus, Edit, Trash2, Calendar, Repeat, Search } from "lucide-react"
import dayjs from "dayjs"
import "dayjs/locale/vi"

import { useGetMyAvailabilityHistory, useDeleteAvailability } from "@/features/shifts/hooks"
import { ShiftAvailabilityResponse } from "@/features/shifts/models/response"
import { GetAvailabilityParams } from "@/features/shifts/models/request"
import { confirmDialog } from "@/hooks"
import { DashboardHeader } from "@/features/dashboard/components"
import { ButtonStyled, InputStyled, PaginationStyled } from "@/components"
import { ShiftAvailabilityModal } from "@/features/shifts/components"
import { DayOfWeekLabel } from "@/constants/labels"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { DayOfWeek } from "@/constants/enums"

export default function MyShiftAvailabilitiesPageView() {
    // --- STATE ---
    const [page, setPage] = useState(1)
    const [searchTerm, setSearchTerm] = useState("")
    const [filters, setFilters] = useState<GetAvailabilityParams>({
        isAvailable: undefined,
        isRecurring: undefined,
        keyword: ""
    })

    const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure()
    const [selectedItem, setSelectedItem] = useState<ShiftAvailabilityResponse | undefined>()

    // --- HOOKS ---
    const { data, isLoading } = useGetMyAvailabilityHistory({
        query: filters,
        pagination: { page, pageSize: 10 }
    })
    const deleteMutation = useDeleteAvailability()

    // --- LOGIC CENTRALIZED UPDATE (Thay thế cho useEffect) ---
    // Hàm này giúp cập nhật filter và reset page về 1 trong cùng một đợt render
    const handleFilterChange = useCallback((newFilters: Partial<GetAvailabilityParams>) => {
        setFilters((prev) => ({ ...prev, ...newFilters }))
        setPage(1)
    }, [])

    // Logic Debounce cho Keyword
    const debouncedSearch = useMemo(
        () => debouncedWrapper(async (val: string) => handleFilterChange({ keyword: val }), 600),
        [handleFilterChange]
    )

    // --- LOGIC PERMISSION ---
    const canEditOrDelete = (item: ShiftAvailabilityResponse) => {
        if (item.isRecurring) return true
        if (!item.specificDate) return false
        return dayjs(item.specificDate).isAfter(dayjs().subtract(1, "day"), "day")
    }

    // --- RENDER CELL ---
    const renderCell = (item: ShiftAvailabilityResponse, columnKey: React.Key) => {
        switch (columnKey) {
            case "type":
                return (
                    <Chip color={item.isAvailable ? "success" : "danger"} variant="flat" size="sm">
                        {item.isAvailable ? "RẢNH" : "BẬN"}
                    </Chip>
                )
            case "schedule": {
                const isPast =
                    !item.isRecurring && dayjs(item.specificDate).isBefore(dayjs(), "day")

                // Logic lấy Label:
                // Nếu là Recurring -> Lấy trực tiếp từ dayOfWeek (0-6)
                // Nếu là Specific -> Dùng dayjs().day() để lấy index (0-6) rồi tra bảng Label
                const getDayLabel = () => {
                    if (item.isRecurring && item.dayOfWeek !== undefined) {
                        return DayOfWeekLabel[item.dayOfWeek as DayOfWeek]
                    }
                    if (item.specificDate) {
                        const dayIndex = dayjs(item.specificDate).day() as DayOfWeek
                        return DayOfWeekLabel[dayIndex]
                    }
                    return ""
                }

                return (
                    <div className={cn("flex flex-col", isPast && "opacity-50")}>
                        <span className="font-medium">
                            {item.isRecurring ? (
                                <span className="flex items-center gap-1 text-primary">
                                    <Repeat size={14} /> {getDayLabel()} hàng tuần
                                </span>
                            ) : (
                                <span className="flex items-center gap-1 text-default-700">
                                    <Calendar size={14} />
                                    {getDayLabel()}, {dayjs(item.specificDate).format("DD/MM/YYYY")}
                                </span>
                            )}
                        </span>
                        <span className="text-tiny text-default-500 font-mono">
                            {item.startTime.slice(0, 5)} - {item.endTime.slice(0, 5)}
                            {isPast && (
                                <span className="ml-2 italic text-danger-400">(Đã qua)</span>
                            )}
                        </span>
                    </div>
                )
            }
            case "note":
                return (
                    <span className="text-default-600 italic text-small line-clamp-1 max-w-50">
                        {item.note || "---"}
                    </span>
                )
            case "actions": {
                const isActionable = canEditOrDelete(item)
                return (
                    <div className="flex gap-1 justify-center">
                        <Button
                            isIconOnly
                            size="sm"
                            variant="light"
                            isDisabled={!isActionable}
                            onPress={() => {
                                setSelectedItem(item)
                                onOpen()
                            }}
                        >
                            <Edit
                                size={18}
                                className={isActionable ? "text-primary" : "text-default-300"}
                            />
                        </Button>
                        <Button
                            isIconOnly
                            size="sm"
                            variant="light"
                            color="danger"
                            isDisabled={!isActionable}
                            onPress={async () => {
                                if (
                                    await confirmDialog({
                                        title: "Xóa lịch đăng ký",
                                        message: `Bạn chắc chắn muốn xóa lịch ${item.isAvailable ? "Rảnh" : "Bận"} này không?`,
                                        variant: "danger"
                                    })
                                )
                                    deleteMutation.mutate(item.id)
                            }}
                        >
                            <Trash2
                                size={18}
                                className={isActionable ? "text-danger" : "text-default-300"}
                            />
                        </Button>
                    </div>
                )
            }
            default:
                return null
        }
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-3">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <DashboardHeader
                    icon={<Calendar className="text-white" />}
                    title="Lịch Cá Nhân"
                    description="Đăng ký và theo dõi lịch rảnh/bận để được sắp ca tối ưu nhất"
                />
                <ButtonStyled
                    color="primary"
                    variant="shadow"
                    startContent={<Plus size={20} />}
                    onPress={() => {
                        setSelectedItem(undefined)
                        onOpen()
                    }}
                >
                    Đăng ký lịch mới
                </ButtonStyled>
            </div>

            {/* Filter Section - Logic tập trung tại handleFilterChange */}
            <div className="flex flex-col lg:flex-row gap-4 items-end bg-white/60 backdrop-blur-md p-4 rounded-2xl border border-divider shadow-sm">
                <div className="w-full lg:max-w-xs">
                    <InputStyled
                        label="Tìm kiếm"
                        placeholder="Tìm theo ghi chú..."
                        startContent={<Search size={18} className="text-default-400" />}
                        value={searchTerm}
                        onChange={(e) => {
                            setSearchTerm(e.target.value)
                            debouncedSearch(e.target.value)
                        }}
                        isClearable
                        onClear={() => {
                            setSearchTerm("")
                            handleFilterChange({ keyword: "" })
                        }}
                    />
                </div>

                <div className="flex flex-col gap-2">
                    <span className="text-tiny font-bold text-default-500 ml-1">TRẠNG THÁI</span>
                    <div className="flex bg-default-100 p-1 rounded-xl border border-default-200">
                        {[
                            { label: "Tất cả", value: undefined },
                            { label: "Rảnh", value: true },
                            { label: "Bận", value: false }
                        ].map((opt) => (
                            <Button
                                key={String(opt.value)}
                                size="sm"
                                radius="lg"
                                variant={filters.isAvailable === opt.value ? "solid" : "light"}
                                color={filters.isAvailable === opt.value ? "primary" : "default"}
                                className={cn(filters.isAvailable === opt.value && "shadow-sm")}
                                onPress={() => handleFilterChange({ isAvailable: opt.value })}
                            >
                                {opt.label}
                            </Button>
                        ))}
                    </div>
                </div>

                <div className="flex items-center h-10 px-4 gap-2 bg-default-100 rounded-xl border border-default-200">
                    <Switch
                        size="sm"
                        isSelected={filters.isRecurring}
                        onValueChange={(v) => handleFilterChange({ isRecurring: v || undefined })}
                    />
                    <span className="text-small font-medium text-default-600">Lịch lặp lại</span>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-divider overflow-hidden">
                <Table aria-label="Staff Availability Table" removeWrapper className="min-h-100">
                    <TableHeader>
                        <TableColumn key="type" width={150}>
                            TRẠNG THÁI
                        </TableColumn>
                        <TableColumn key="schedule">THỜI GIAN ĐĂNG KÝ</TableColumn>
                        <TableColumn key="note">GHI CHÚ</TableColumn>
                        <TableColumn key="actions" align="center" width={100}>
                            THAO TÁC
                        </TableColumn>
                    </TableHeader>
                    <TableBody
                        items={data?.items || []}
                        isLoading={isLoading}
                        emptyContent={!isLoading && "Không tìm thấy dữ liệu đăng ký lịch."}
                    >
                        {(item) => (
                            <TableRow
                                key={item.id}
                                className="hover:bg-default-50 transition-colors"
                            >
                                {(key) => <TableCell>{renderCell(item, key)}</TableCell>}
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>

            {(data?.items || []).length > 0 && !isLoading && (
                <PaginationStyled
                    page={page}
                    total={data?.totalPages ?? 1}
                    onChange={(p) => setPage(p)}
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
