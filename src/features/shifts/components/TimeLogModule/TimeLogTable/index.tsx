"use client"

import React, { useMemo } from "react"
import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Button,
    Tooltip,
    Spinner
} from "@heroui/react"
import { Pencil } from "lucide-react"
import dayjs from "dayjs"

import {
    StaffInfoCell,
    TimeLogStatusBadge,
    TimeRangeDisplay,
    EmptyTimeLogState
} from "@/features/shifts/components"
import { TimeLogResponse } from "@/features/shifts/models/response"

interface Props {
    data: TimeLogResponse[]
    selectedDate: dayjs.Dayjs
    isLoading: boolean
    isManager: boolean
    onEditClick?: (log: TimeLogResponse) => void
}

export const TimeLogTable = ({ data, selectedDate, isLoading, isManager, onEditClick }: Props) => {
    // 1. Logic tự lọc: Chỉ lấy các bản ghi có ngày Check-in trùng với selectedDate
    const filteredLogs = useMemo(() => {
        return data.filter(
            (item) => item.checkInAt && dayjs(item.checkInAt).isSame(selectedDate, "day")
        )
    }, [data, selectedDate])

    return (
        <div className="flex flex-col gap-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* 2. Header hiển thị ngày (Style giống AgendaListView) */}
            {/* {!isLoading && filteredLogs.length > 0 && (
                <div className="flex items-center gap-3 mb-2">
                    <div className="h-10 w-1.5 bg-primary rounded-full" />
                    <div>
                        <h2 className="text-xl font-black capitalize">
                            {selectedDate.format("dddd")}
                        </h2>
                        <p className="text-xs font-bold text-default-400 uppercase tracking-widest">
                            Ngày {selectedDate.format("DD [Tháng] MM, YYYY")}
                        </p>
                    </div>
                </div>
            )} */}

            {/* 3. Bảng dữ liệu */}
            <Table
                aria-label="Time Log Table"
                selectionMode="none"
                layout="fixed"
                classNames={{
                    wrapper: "rounded-2xl shadow-sm border border-divider p-0 overflow-hidden",
                    table: "min-w-full"
                }}
            >
                <TableHeader>
                    <TableColumn className="w-[40%]">NHÂN VIÊN</TableColumn>
                    <TableColumn className="w-[25%]">THỜI GIAN THỰC TẾ</TableColumn>
                    <TableColumn align="center" className="w-[20%]">
                        TRẠNG THÁI
                    </TableColumn>

                    <TableColumn align="center" className={`${!isManager ? "hidden" : "w-[15%]"}`}>
                        THAO TÁC
                    </TableColumn>
                </TableHeader>
                <TableBody
                    items={filteredLogs} // Dùng dữ liệu đã lọc
                    isLoading={isLoading}
                    loadingContent={<Spinner label="Đang đối soát dữ liệu..." />}
                    emptyContent={
                        <EmptyTimeLogState
                            message={`Không có dữ liệu chấm công ngày ${selectedDate.format("DD/MM")}`}
                        />
                    }
                >
                    {(item) => (
                        <TableRow key={item.id} className="hover:bg-default-50 transition-colors">
                            <TableCell>
                                <StaffInfoCell staff={item.staff} />
                            </TableCell>
                            <TableCell>
                                <TimeRangeDisplay
                                    checkInAt={item.checkInAt}
                                    checkOutAt={item.checkOutAt}
                                    totalWorkMinutes={item.totalWorkMinutes}
                                />
                            </TableCell>
                            <TableCell>
                                <Tooltip
                                    content={item.managerNote || "Chưa có ghi chú"}
                                    isDisabled={!item.managerNote}
                                >
                                    <TimeLogStatusBadge status={item.status} />
                                </Tooltip>
                            </TableCell>
                            <TableCell className={`${!isManager && "hidden"}`}>
                                <Button
                                    isIconOnly
                                    size="sm"
                                    variant="light"
                                    color="primary"
                                    onPress={() => onEditClick?.(item)}
                                    className="mx-auto"
                                >
                                    <Pencil size={18} />
                                </Button>
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
        </div>
    )
}
