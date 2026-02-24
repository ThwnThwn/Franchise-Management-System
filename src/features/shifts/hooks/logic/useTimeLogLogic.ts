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

export type TimeLogViewMode = "list" | "calendar"

export const useTimeLogLogic = () => {
    // 1. Phân quyền & Store từ Token
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const storeIdFromToken = useTokenStore((s) => s.storeId) ?? undefined
    const isManager = roleSlug === RoleSlug.Manager

    // 2. State cơ bản (Thừa hưởng từ mẫu của bro)
    const [currentDate, setCurrentDate] = useState(dayjs())
    const [viewMode, setViewMode] = useState<TimeLogViewMode>("list")

    // 3. State lọc bổ sung (Dành cho Manager & Filter)
    const [status, setStatus] = useState<TimeLogStatus | undefined>(undefined)
    const [staffName, setStaffName] = useState("")

    // 4. Modal State (Dùng HeroUI Disclosure)
    const { isOpen, onOpen, onClose } = useDisclosure()
    const [selectedLog, setSelectedLog] = useState<TimeLogResponse | null>(null)

    // 5. Tính toán Date Range để gọi API
    // logic: Nếu xem List thì lấy theo ngày, xem Calendar thì lấy theo tháng/tuần
    const dateRange = useMemo(() => {
        return {
            fromDate: currentDate.startOf("month").format("YYYY-MM-DD"),
            toDate: currentDate.endOf("month").format("YYYY-MM-DD")
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentDate.format("YYYY-MM")])

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

    // --- CÁC HANDLER ĐIỀU HƯỚNG ---
    const handleNext = () => {
        const unit = viewMode === "list" ? "day" : "week"
        setCurrentDate((prev) => prev.add(1, unit))
    }

    const handlePrev = () => {
        const unit = viewMode === "list" ? "day" : "week"
        setCurrentDate((prev) => prev.subtract(1, unit))
    }

    const handleToday = () => setCurrentDate(dayjs())

    const handleJumpToDate = (date: CalendarDate) => setCurrentDate(dayjs(date.toString()))

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
        // Auth & Role
        isManager,

        // Date & View state
        currentDate,
        viewMode,
        setViewMode,

        // Filter state
        status,
        setStatus,
        staffName,
        setStaffName,

        // API Params
        queryParams,

        // Handlers
        handleNext,
        handlePrev,
        handleToday,
        handleJumpToDate,

        // Modal helpers
        modal: {
            isOpen: isOpen,
            onOpen: handleEditClick, // Mở kèm data
            onClose: handleCloseModal,
            selectedLog
        }
    }
}
