"use client"

import { useState, useMemo } from "react"
import dayjs from "dayjs"
import { CalendarDate } from "@internationalized/date"
import { useDisclosure } from "@heroui/react"
import { RoleSlug } from "@/constants/enums"
import { useTokenStore } from "@/hooks"
import { TimeLogStatus } from "@/features/shifts/constants"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { GetTimeLogsParams } from "../../models/request"
import { PaginationParams } from "@/models/common/request"

export type TimeLogViewMode = "list" | "calendar"

export const useTimeLogLogic = () => {
    // 1. Phân quyền & Store từ Token
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const storeIdFromToken = useTokenStore((s) => s.storeId) ?? undefined
    const isManager = roleSlug === RoleSlug.Manager

    // 2. State cơ bản
    const [currentDate, setCurrentDate] = useState(dayjs())
    const [viewMode, setViewMode] = useState<TimeLogViewMode>("list")
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 50 })

    // 3. State lọc bổ sung
    const [status, setStatusState] = useState<TimeLogStatus | undefined>(undefined)
    const [staffName, setStaffNameState] = useState("")

    // --- HELPER: Reset về trang 1 ---
    const resetPagination = () => setPagination((prev) => ({ ...prev, page: 1 }))

    // --- WRAPPER HANDLERS CHO FILTERS ---
    // Mỗi khi filter thay đổi, tự động đưa page về 1
    const setStatus = (newStatus: TimeLogStatus | undefined) => {
        setStatusState(newStatus)
        resetPagination()
    }

    const setStaffName = (name: string) => {
        setStaffNameState(name)
        resetPagination()
    }

    // Khi đổi ngày hoặc đổi viewMode (tháng/tuần) cũng nên reset page
    const handleDateChange = (newDate: dayjs.Dayjs) => {
        setCurrentDate(newDate)
        resetPagination()
    }

    // 4. Modal State
    const { isOpen, onOpen, onClose } = useDisclosure()
    const [selectedLog, setSelectedLog] = useState<TimeLogResponse | null>(null)

    // 5. Tính toán Date Range
    const dateRange = useMemo(() => {
        return {
            fromDate: currentDate.startOf("week").format("YYYY-MM-DD"),
            toDate: currentDate.endOf("week").format("YYYY-MM-DD")
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentDate.format("YYYY-MM-DD")])

    // 6. Tổng hợp Params cho API
    const queryParams = useMemo<GetTimeLogsParams>(
        () => ({
            storeId: isManager ? storeIdFromToken : undefined,
            status,
            staffName: staffName || undefined,
            ...dateRange
        }),
        [isManager, storeIdFromToken, status, staffName, dateRange]
    )

    // --- CÁC HANDLER ĐIỀU HƯỚNG (Đã tích hợp reset page) ---
    const handleNext = () => {
        const unit = viewMode === "list" ? "day" : "week"
        handleDateChange(currentDate.add(1, unit))
    }

    const handlePrev = () => {
        const unit = viewMode === "list" ? "day" : "week"
        handleDateChange(currentDate.subtract(1, unit))
    }

    const handleToday = () => handleDateChange(dayjs())

    const handleJumpToDate = (date: CalendarDate) => handleDateChange(dayjs(date.toString()))

    const handleViewModeChange = (mode: TimeLogViewMode) => {
        setViewMode(mode)
        resetPagination()
    }

    // --- CÁC HANDLER TƯƠNG TÁC ---
    const handleEditClick = (log: TimeLogResponse) => {
        setSelectedLog(log)
        onOpen()
    }

    const handleCloseModal = () => {
        setSelectedLog(null)
        onClose()
    }

    return {
        isManager,
        currentDate,
        viewMode,
        setViewMode: handleViewModeChange,

        status,
        setStatus,
        staffName,
        setStaffName,
        pagination,
        setPagination,

        queryParams,

        handleNext,
        handlePrev,
        handleToday,
        handleJumpToDate,

        modal: {
            isOpen,
            onOpen: handleEditClick,
            onClose: handleCloseModal,
            selectedLog
        }
    }
}
