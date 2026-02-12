import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import dayjs from "dayjs"
import { AttendancePayload } from "@/features/shifts/models/request"
import { UserCurrentShift } from "@/features/shifts/models/response"

// --- MOCK DATABASE (Sống tạm trong RAM) ---
let MOCK_SHIFT_DATA: UserCurrentShift = {
    hasShiftToday: true,
    assignmentId: "asg-001",
    shiftId: "shift-morning",
    currentLogId: undefined, // Sẽ có khi check-in
    storeId: "store-01",
    storeName: "Coffee House Quận 1",
    storeLat: 10.7769,
    storeLong: 106.7009,
    startTime: "20:00:00",
    endTime: "04:00:00",
    canCheckIn: true,
    canCheckOut: false,
    isOvertime: false,
    checkInAt: undefined,
    checkOutAt: undefined,
    serverTime: dayjs().toISOString()
}

/**
 * HELPER: Giả lập delay mạng
 */
const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

// --- 1. QUERY HOOK: Lấy trạng thái ca làm hiện tại ---
export const useGetCurrentShift = (enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.CURRENT],
        queryFn: async () => {
            await sleep(800) // Giả lập mạng load 0.8s
            return { ...MOCK_SHIFT_DATA, serverTime: dayjs().toISOString() }
        },
        enabled,
        refetchOnWindowFocus: true,
        staleTime: 1000 * 60
    })
}

// --- 2. MUTATION HOOK: Check-in (Vào ca) ---
export const useCheckIn = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({
            assignmentId,
            payload
        }: {
            assignmentId: string
            payload: AttendancePayload
        }) => {
            await sleep(1500) // Giả lập xử lý 1.5s
            console.log(assignmentId, payload)

            // Cập nhật Mock Data
            MOCK_SHIFT_DATA = {
                ...MOCK_SHIFT_DATA,
                currentLogId: "log-999",
                checkInAt: dayjs().toISOString(),
                canCheckIn: false,
                canCheckOut: true
            }
            return true
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.CURRENT]
            })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Vào ca làm việc thành công. Chúc bạn một ngày vui vẻ!",
                color: "success"
            })
            onSuccess?.()
        }
    })
}

// --- 3. MUTATION HOOK: Check-out (Tan ca) ---
export const useCheckOut = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ logId, payload }: { logId: string; payload: AttendancePayload }) => {
            await sleep(1500)
            console.log(logId, payload)

            // Cập nhật Mock Data
            MOCK_SHIFT_DATA = {
                ...MOCK_SHIFT_DATA,
                checkOutAt: dayjs().toISOString(),
                canCheckOut: false,
                hasShiftToday: true // Vẫn hiện ca nhưng ở trạng thái "Done"
            }
            return true
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.CURRENT]
            })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tan ca thành công. Hẹn gặp lại bạn!",
                color: "success"
            })
            onSuccess?.()
        }
    })
}
