import { useMutation, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { ShiftStatus } from "@/features/shifts/constants"
import { ShiftResponse } from "@/features/shifts/models/response"
import {
    BulkCreateManualRequest,
    BulkCreateFromTemplateRequest
} from "@/features/shifts/models/request"
import dayjs from "dayjs"
import { fakeDelay, MOCK_SHIFTS_DB } from "./useShifts"
import { MOCK_TEMPLATES_DB } from "./useShiftTemplates"

// --- IMPORT MOCK DB TỪ FILE TRƯỚC CỦA BRO ---
// Giả định bro để chung hoặc export chúng ra

// --- BULK MUTATION MOCK HOOKS ---

/**
 * [MOCK] Hook tạo hàng loạt ca làm việc thủ công
 */
export const useBulkCreateManualShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (req: BulkCreateManualRequest) => {
            await fakeDelay(1000)

            const start = dayjs(req.startDate)
            const end = dayjs(req.endDate)
            const diffDays = end.diff(start, "day")

            const newShifts: ShiftResponse[] = []

            for (let i = 0; i <= diffDays; i++) {
                const currentDay = start.add(i, "day")
                // Kiểm tra xem ngày hiện tại có nằm trong mảng daysOfWeek không
                if (req.daysOfWeek.includes(currentDay.day())) {
                    newShifts.push({
                        id: `shift-bulk-manual-${Date.now()}-${i}`,
                        storeId: req.storeId,
                        shiftDate: currentDay.format("YYYY-MM-DD"),
                        startTime: req.startTime,
                        endTime: req.endTime,
                        requiredStaffCount: req.requiredStaffCount,
                        breakDurationMinutes: req.breakDurationMinutes,
                        status: ShiftStatus.Draft,
                        note: req.note || "Ca tạo hàng loạt (Manual)",
                        assignedStaffCount: 0,
                        assignments: []
                    })
                }
            }

            MOCK_SHIFTS_DB.push(...newShifts)
            return newShifts
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo hàng loạt ca làm việc thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        }
    })
}

/**
 * [MOCK] Hook tạo hàng loạt ca làm việc từ Template
 */
export const useBulkCreateFromTemplateShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (req: BulkCreateFromTemplateRequest) => {
            await fakeDelay(1000)

            // Tìm template trong Mock DB của Template
            const template = MOCK_TEMPLATES_DB.find((t) => t.id === req.templateId)
            if (!template) throw new Error("Template không tồn tại")

            const start = dayjs(req.startDate)
            const end = dayjs(req.endDate)
            const diffDays = end.diff(start, "day")

            const newShifts: ShiftResponse[] = []

            for (let i = 0; i <= diffDays; i++) {
                const currentDay = start.add(i, "day")
                // Đối với template, giả định tạo cho tất cả các ngày trong khoảng
                newShifts.push({
                    id: `shift-bulk-tpl-${Date.now()}-${i}`,
                    storeId: req.storeId,
                    shiftDate: currentDay.format("YYYY-MM-DD"),
                    // Lấy giờ từ template
                    startTime: template.startTime,
                    endTime: template.endTime,
                    requiredStaffCount: req.requiredStaffCount,
                    breakDurationMinutes: template.breakDurationMinutes,
                    shiftTemplateId: template.id,
                    status: ShiftStatus.Draft,
                    note: `Tạo từ mẫu: ${template.name}`,
                    assignedStaffCount: 0,
                    assignments: []
                })
            }

            MOCK_SHIFTS_DB.push(...newShifts)
            return newShifts
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã đổ ca làm việc từ mẫu thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        }
    })
}

/**
 * [MOCK] Hook công khai hàng loạt ca làm việc
 */
export const useBulkPublishShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (ids: string[]) => {
            await fakeDelay(800)
            MOCK_SHIFTS_DB.forEach((shift) => {
                if (ids.includes(shift.id)) {
                    shift.status = ShiftStatus.Published
                }
            })
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã chốt và công khai các ca làm việc thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        }
    })
}

/**
 * [MOCK] Hook xóa hàng loạt ca làm việc nháp
 */
export const useBulkDeleteShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (ids: string[]) => {
            await fakeDelay(800)
            // Lọc bỏ những ca nằm trong danh sách ids
            const updatedDB = MOCK_SHIFTS_DB.filter((shift) => !ids.includes(shift.id))
            // Cập nhật lại Mock DB (Vì dùng let/export nên cần gán lại cẩn thận)
            MOCK_SHIFTS_DB.length = 0
            MOCK_SHIFTS_DB.push(...updatedDB)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã dọn dẹp các ca làm việc nháp (MOCK)",
                color: "success"
            })
            onSuccess?.()
        }
    })
}
