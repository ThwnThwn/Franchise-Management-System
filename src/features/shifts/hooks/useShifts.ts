import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import {
    CreateShiftRequest,
    GetShiftsParams,
    UpdateShiftRequest
} from "@/features/shifts/models/request"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus, ShiftAssignmentStatus } from "@/features/shifts/constants"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import dayjs from "dayjs"

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
    const fmtDateTime = (d: dayjs.Dayjs, timeStr: string) => {
        // timeStr dạng "08:00:00"
        return d.format(`YYYY-MM-DDT${timeStr}[Z]`)
        // Hoặc nếu muốn chuẩn format có Z thì dùng: d.format(`YYYY-MM-DDT${timeStr}[Z]`)
        // Nhưng ở đây mock đơn giản để test UI
    }

    return [
        {
            id: "shift-1",
            storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd",
            shiftDate: fmtDate(monday), // Thứ 2
            startTime: fmtDateTime(monday, "01:00:00"),
            endTime: fmtDateTime(monday, "09:00:00"),
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
                    staffName: "Nguyễn Văn A",
                    staffAvatarUrl: "https://i.pravatar.cc/150?u=a042581f4e29026024d"
                },
                {
                    id: "assign-2",
                    shiftId: "shift-1",
                    userId: "user-2",
                    status: ShiftAssignmentStatus.Assigned,
                    hourlyWage: 22000,
                    staffName: "Trần Thị B",
                    staffAvatarUrl: "https://i.pravatar.cc/150?u=a042581f4e29026704d"
                }
            ]
        },
        {
            id: "shift-2",
            storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd",
            shiftDate: fmtDate(tuesday), // Thứ 3
            startTime: fmtDateTime(tuesday, "06:00:00"),
            endTime: fmtDateTime(tuesday, "14:00:00"),
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
            startTime: fmtDateTime(wednesday, "11:00:00"),
            endTime: fmtDateTime(wednesday, "20:00:00"), // Ca qua đêm sang thứ 5
            requiredStaffCount: 5,
            breakDurationMinutes: 20,
            shiftTemplateId: "tpl-3",
            status: ShiftStatus.Published,
            assignedStaffCount: 5, // Đủ người
            assignments: []
        }
    ]
}

// Khởi tạo Mock DB
let MOCK_SHIFTS_DB = generateMockShifts()

const fakeDelay = (ms = 500) => new Promise((resolve) => setTimeout(resolve, ms))

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
            console.log("Mock API Get Shifts:", params)

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

export const useAssignStaffToShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id, userIds }: { id: string; userIds: string[] }) => {
            await fakeDelay()
            const index = MOCK_SHIFTS_DB.findIndex((s) => s.id === id)
            if (index !== -1) {
                MOCK_SHIFTS_DB[index].assignedStaffCount += userIds.length
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Phân công nhân viên thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({ title: STRINGS.COMMON.ERROR, description: "Lỗi Mock", color: "danger" })
        }
    })
}

export const useRemoveStaffFromShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id }: { id: string }) => {
            await fakeDelay()
            const index = MOCK_SHIFTS_DB.findIndex((s) => s.id === id)
            if (index !== -1 && MOCK_SHIFTS_DB[index].assignedStaffCount > 0) {
                MOCK_SHIFTS_DB[index].assignedStaffCount -= 1
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xóa nhân viên khỏi ca (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({ title: STRINGS.COMMON.ERROR, description: "Lỗi Mock", color: "danger" })
        }
    })
}
