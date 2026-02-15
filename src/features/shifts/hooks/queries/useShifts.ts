import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import {
    CreateShiftRequest,
    GetShiftsParams,
    GetTimeLogsParams,
    UpdateShiftRequest,
    UpdateTimeLogRequest
} from "@/features/shifts/models/request"
import { ShiftResponse, TimeLogResponse } from "@/features/shifts/models/response"
import { ShiftStatus, ShiftAssignmentStatus, TimeLogStatus } from "@/features/shifts/constants"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import dayjs from "dayjs"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

// --- MOCK DATA GENERATOR ---
const generateMockShifts = (): ShiftResponse[] => {
    // Lấy ngày thứ 2 của tuần hiện tại
    // day(1) sẽ trả về thứ 2 của tuần đó (Mặc định tuần bắt đầu từ CN)
    const monday = dayjs().day(1)
    const tuesday = monday.add(1, "day")
    const wednesday = monday.add(2, "day")
    // const thursday = monday.add(3, "day")

    // Helper format nhanh
    const fmtDate = (d: dayjs.Dayjs) => d.format(DEFAULT_DATE_FORMAT)
    // const fmtDateTime = (d: dayjs.Dayjs, timeStr: string) => {
    //     // timeStr dạng "08:00:00"
    //     return d.format(`YYYY-MM-DDT${timeStr}[Z]`)
    //     // Hoặc nếu muốn chuẩn format có Z thì dùng: d.format(`YYYY-MM-DDT${timeStr}[Z]`)
    //     // Nhưng ở đây mock đơn giản để test UI
    // }

    return [
        {
            id: "shift-1",
            storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd",
            shiftDate: fmtDate(monday), // Thứ 2
            startTime: "08:00:00",
            endTime: "16:00:00",
            requiredStaffCount: 3,
            breakDurationMinutes: 15,
            shiftTemplateId: "tpl-1",
            status: ShiftStatus.Published,
            note: "Ca sáng đầu tuần",
            assignedStaffCount: 2, // Thiếu 1 người
            assignments: [
                {
                    id: "assign-1",
                    shiftId: "shift-1",
                    userId: "user-1",
                    status: ShiftAssignmentStatus.Confirmed,
                    hourlyWage: 25000,
                    firstName: "A",
                    lastName: "Nguyễn Văn",
                    avatarUrl: "https://i.pravatar.cc/150?u=a042581f4e29026024d"
                },
                {
                    id: "assign-2",
                    shiftId: "shift-1",
                    userId: "user-2",
                    status: ShiftAssignmentStatus.Assigned,
                    hourlyWage: 22000,
                    firstName: "B",
                    lastName: "Trần Thị",
                    avatarUrl: "https://i.pravatar.cc/150?u=a042581f4e29026704d"
                }
            ]
        },
        {
            id: "shift-2",
            storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd",
            shiftDate: fmtDate(tuesday), // Thứ 3
            startTime: "13:00:00",
            endTime: "21:00:00",
            requiredStaffCount: 2,
            breakDurationMinutes: 60,
            shiftTemplateId: "tpl-2",
            status: ShiftStatus.Draft,
            assignedStaffCount: 0,
            assignments: []
        },
        {
            id: "shift-3",
            storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd",
            shiftDate: fmtDate(wednesday), // Thứ 4
            startTime: "18:00:00",
            endTime: "03:00:00", // Ca qua đêm sang thứ 5
            requiredStaffCount: 5,
            breakDurationMinutes: 20,
            shiftTemplateId: "tpl-3",
            status: ShiftStatus.Published,
            assignedStaffCount: 5, // Đủ người
            assignments: []
        }
    ]
}

export const fakeDelay = (ms = 500) => new Promise((resolve) => setTimeout(resolve, ms))
// Khởi tạo Mock DB
export let MOCK_SHIFTS_DB = generateMockShifts()

// --- 1. MOCK DATABASE (Dữ liệu mẫu cho Manager & Staff) ---
let MOCK_TIME_LOGS: TimeLogResponse[] = [
    {
        id: "log-001",
        shiftAssignmentId: "asg-c1",
        checkInAt: dayjs().subtract(1, "day").hour(8).minute(5).toISOString(),
        checkOutAt: dayjs().subtract(1, "day").hour(16).minute(0).toISOString(),
        totalWorkMinutes: 475,
        status: TimeLogStatus.Valid,
        createdAt: dayjs().subtract(1, "day").toISOString(),
        updatedAt: dayjs().subtract(1, "day").toISOString(),
        staff: { id: "user-01", firstName: "An", lastName: "Nguyễn" },
        shift: {
            id: "shift-01",
            shiftDate: dayjs().subtract(1, "day").format("YYYY-MM-DD"),
            startTime: "08:00:00",
            endTime: "16:00:00", // Ca thường trong ngày
            note: "Ca sáng Landmark 81"
        }
    },
    {
        id: "log-002",
        shiftAssignmentId: "asg-c2",
        // Case ca đêm: Check-in 22h tối hôm trước, Check-out 6h sáng hôm sau
        checkInAt: dayjs().subtract(1, "day").hour(22).minute(5).toISOString(),
        checkOutAt: dayjs().hour(6).minute(0).toISOString(),
        totalWorkMinutes: 475,
        status: TimeLogStatus.Valid,
        createdAt: dayjs().toISOString(),
        updatedAt: dayjs().toISOString(),
        staff: { id: "user-02", firstName: "Bình", lastName: "Trần" },
        shift: {
            id: "shift-02",
            shiftDate: dayjs().subtract(1, "day").format("YYYY-MM-DD"),
            startTime: "22:00:00",
            endTime: "06:00:00", // 🚩 CA ĐÊM: start > end -> Qua ngày hôm sau
            note: "Ca đêm Phố Đi Bộ"
        }
    },
    {
        id: "log-003",
        shiftAssignmentId: "asg-c3",
        checkInAt: dayjs().hour(14).minute(0).toISOString(),
        status: TimeLogStatus.Processing,
        createdAt: dayjs().toISOString(),
        updatedAt: dayjs().toISOString(),
        staff: { id: "user-03", firstName: "Cường", lastName: "Lê" },
        shift: {
            id: "shift-03",
            shiftDate: dayjs().format("YYYY-MM-DD"),
            startTime: "14:00:00",
            endTime: "22:00:00",
            note: "Ca chiều Bitexco"
        }
    },
    {
        id: "log-004",
        shiftAssignmentId: "asg-c4",
        // Case quên checkout ca đêm: Hết ca lúc 6h sáng nhưng giờ (8h) vẫn chưa thấy đâu
        checkInAt: dayjs().subtract(1, "day").hour(22).minute(0).toISOString(),
        status: TimeLogStatus.Processing,
        createdAt: dayjs().subtract(1, "day").toISOString(),
        updatedAt: dayjs().subtract(1, "day").toISOString(),
        staff: { id: "user-04", firstName: "Dũng", lastName: "Phạm" },
        shift: {
            id: "shift-04",
            shiftDate: dayjs().subtract(1, "day").format("YYYY-MM-DD"),
            startTime: "22:00:00",
            endTime: "06:00:00",
            note: "Ca đêm Landmark"
        }
    }
]

export const getMockTimeLogs = async (
    params: GetTimeLogsParams,
    pagination: PaginationParams
): Promise<PageResult<TimeLogResponse>> => {
    await fakeDelay(600)
    let filtered = [...MOCK_TIME_LOGS]
    console.log(pagination)

    // Lọc theo Status nếu có
    if (params.status !== undefined) {
        filtered = filtered.filter((l) => l.status === params.status)
    }

    // Lọc theo StaffId (dùng cho /me hoặc lọc manager)
    if (params.staffId) {
        filtered = filtered.filter((l) => l.staff.id === params.staffId)
    }

    // Lọc theo Ngày (check_in_at)
    if (params.fromDate && params.toDate) {
        filtered = filtered.filter((l) =>
            dayjs(l.checkInAt).isBetween(params.fromDate, params.toDate, "day", "[]")
        )
    }

    return {
        items: filtered,
        totalCount: filtered.length,
        totalPages: Math.ceil(filtered.length / (pagination.pageSize ?? 10))
    }
}

// --- QUERY HOOKS (GET DATA) ---

export const useGetShifts = ({
    params,
    enabled = true
}: {
    params: GetShiftsParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFTS, params],
        queryFn: async () => {
            await fakeDelay()
            let filtered = MOCK_SHIFTS_DB
            // So sánh string YYYY-MM-DD vẫn đúng
            if (params.fromDate) {
                filtered = filtered.filter((s) => s.shiftDate >= params.fromDate!)
            }
            if (params.toDate) {
                filtered = filtered.filter((s) => s.shiftDate <= params.toDate!)
            }

            return filtered
        },
        enabled
    })
}

export const useGetShiftById = ({ id, enabled = true }: { id: string; enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFTS, id],
        queryFn: async () => {
            await fakeDelay()
            const found = MOCK_SHIFTS_DB.find((s) => s.id === id)
            if (!found) throw new Error("Shift not found")
            return found
        },
        enabled
    })
}

// --- MUTATION HOOKS (ACTION DATA) ---

export const useCreateShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (req: CreateShiftRequest) => {
            await fakeDelay()
            const newShift: ShiftResponse = {
                id: `shift-${Date.now()}`,
                storeId: req.storeId,
                shiftDate: req.shiftDate,
                startTime: req.startTime,
                endTime: req.endTime,
                requiredStaffCount: req.requiredStaffCount,
                breakDurationMinutes: req.breakDurationMinutes,
                shiftTemplateId: req.shiftTemplateId,
                status: ShiftStatus.Draft,
                note: req.note,
                assignedStaffCount: 0,
                assignments: []
            }

            console.log(newShift)

            MOCK_SHIFTS_DB.push(newShift)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo ca làm việc thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({ title: STRINGS.COMMON.ERROR, description: "Lỗi Mock", color: "danger" })
        }
    })
}

export const useUpdateShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id, req }: { id: string; req: UpdateShiftRequest }) => {
            await fakeDelay()
            const index = MOCK_SHIFTS_DB.findIndex((s) => s.id === id)
            if (index !== -1) {
                MOCK_SHIFTS_DB[index] = { ...MOCK_SHIFTS_DB[index], ...req }
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật ca làm việc thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({ title: STRINGS.COMMON.ERROR, description: "Lỗi Mock", color: "danger" })
        }
    })
}

export const useDeleteShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id }: { id: string }) => {
            await fakeDelay()
            MOCK_SHIFTS_DB = MOCK_SHIFTS_DB.filter((s) => s.id !== id)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xoá ca làm việc (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({ title: STRINGS.COMMON.ERROR, description: "Lỗi Mock", color: "danger" })
        }
    })
}

export const usePublishShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id }: { id: string }) => {
            await fakeDelay()
            const index = MOCK_SHIFTS_DB.findIndex((s) => s.id === id)
            if (index !== -1) {
                MOCK_SHIFTS_DB[index].status = ShiftStatus.Published
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xuất bản ca làm việc (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({ title: STRINGS.COMMON.ERROR, description: "Lỗi Mock", color: "danger" })
        }
    })
}

/**
 * Manager xem toàn bộ logs
 */
export const useGetAllTimeLogs = ({
    query,
    pagination,
    enabled
}: {
    query: GetTimeLogsParams
    pagination: PaginationParams
    enabled: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFTS, QUERY_KEYS.TIME_LOGS, query, pagination],
        queryFn: () => getMockTimeLogs(query, pagination),
        // storeId sẽ được Gateway tự động xử lý dựa trên Token
        enabled: enabled,
        staleTime: 1000 * 30,
        placeholderData: (previousData) => previousData
    })
}

/**
 * Manager cập nhật log (Duyệt/Ghi chú)
 */
export const useUpdateTimeLog = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id, req }: { id: string; req: UpdateTimeLogRequest }) => {
            await fakeDelay(1000)
            const idx = MOCK_TIME_LOGS.findIndex((l) => l.id === id)
            if (idx !== -1) {
                MOCK_TIME_LOGS[idx] = {
                    ...MOCK_TIME_LOGS[idx],
                    status: req.status,
                    managerNote: req.managerNote,
                    updatedAt: dayjs().toISOString()
                }
            }
            return true
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.SHIFTS, QUERY_KEYS.TIME_LOGS],
                exact: false
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật bản ghi chấm công thành công!",
                color: "success"
            })
            onSuccess?.()
        }
    })
}
