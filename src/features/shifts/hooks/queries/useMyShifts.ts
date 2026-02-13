import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import dayjs from "dayjs"
import { AttendancePayload, GetAgendaParams } from "@/features/shifts/models/request"
import { StaffShiftAgenda, UserCurrentShift } from "@/features/shifts/models/response"
import isBetween from "dayjs/plugin/isBetween"

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
    startTime: "14:00:00",
    endTime: "21:00:00",
    canCheckIn: true,
    canCheckOut: false,
    isOvertime: false,
    checkInAt: undefined,
    checkOutAt: undefined,
    serverTime: dayjs().toISOString()
}

dayjs.extend(isBetween)

const FIXED_MOCK_AGENDA: StaffShiftAgenda[] = [
    // --- TUẦN TRƯỚC (QUÁ KHỨ) ---
    {
        id: "asg-p1",
        date: "2026-02-05", // Thứ 5 tuần trước
        startTime: "08:00:00",
        endTime: "16:00:00",
        store: { id: "st-81", name: "Landmark 81", address: "Bình Thạnh", isOpenNow: true },
        coWorkers: [
            {
                id: "w1",
                firstName: "An",
                lastName: "Nguyễn",
                avatar: "https://i.pravatar.cc/150?u=1"
            }
        ],
        checkInAt: "2026-02-05T08:05:00Z",
        checkOutAt: "2026-02-05T16:00:00Z"
    },

    // --- TUẦN NÀY (HIỆN TẠI) ---
    {
        id: "asg-c1",
        date: "2026-02-10", // Thứ 3
        startTime: "08:00:00",
        endTime: "16:00:00",
        store: { id: "st-81", name: "Landmark 81", address: "Bình Thạnh", isOpenNow: true },
        coWorkers: [
            {
                id: "w1",
                firstName: "An",
                lastName: "Nguyễn",
                avatar: "https://i.pravatar.cc/150?u=1"
            }
        ]
    },
    {
        id: "asg-c2",
        date: "2026-02-13", // Thứ 6 - HÔM NAY
        startTime: "14:00:00",
        endTime: "22:00:00",
        store: { id: "st-pdbo", name: "Phố Đi Bộ", address: "Quận 1", isOpenNow: true },
        coWorkers: [
            {
                id: "w3",
                firstName: "Cường",
                lastName: "Lê",
                avatar: "https://i.pravatar.cc/150?u=3"
            }
        ]
    },
    {
        id: "asg-c3",
        date: "2026-02-14", // Thứ 7 - Valentine (Tăng cường)
        startTime: "18:00:00",
        endTime: "02:00:00", // Xuyên đêm
        store: { id: "st-pdbo", name: "Phố Đi Bộ", address: "Quận 1", isOpenNow: true },
        coWorkers: [
            {
                id: "w1",
                firstName: "An",
                lastName: "Nguyễn",
                avatar: "https://i.pravatar.cc/150?u=1"
            },
            {
                id: "w2",
                firstName: "Bình",
                lastName: "Trần",
                avatar: "https://i.pravatar.cc/150?u=2"
            }
        ]
    },

    // --- TUẦN SAU (TƯƠNG LAI) ---
    {
        id: "asg-f1",
        date: "2026-02-18", // Thứ 4 tuần sau
        startTime: "14:00:00",
        endTime: "22:00:00",
        store: { id: "st-pdbo", name: "Phố Đi Bộ", address: "Quận 1", isOpenNow: true },
        coWorkers: []
    },
    {
        id: "asg-f2",
        date: "2026-02-21", // Thứ 7 tuần sau
        startTime: "22:00:00",
        endTime: "06:00:00", // Xuyên đêm
        store: { id: "st-81", name: "Landmark 81", address: "Bình Thạnh", isOpenNow: false },
        coWorkers: [
            { id: "w2", firstName: "Bình", lastName: "Lê", avatar: "https://i.pravatar.cc/150?u=2" }
        ]
    },
    {
        id: "asg-f3",
        date: "2026-02-23", // Thứ 2 tuần sau nữa
        startTime: "07:00:00",
        endTime: "15:00:00",
        store: { id: "st-81", name: "Landmark 81", address: "Bình Thạnh", isOpenNow: true },
        coWorkers: []
    },

    // --- THÁNG SAU (TEST CALENDAR VIEW) ---
    {
        id: "asg-m1",
        date: "2026-03-02",
        startTime: "08:00:00",
        endTime: "16:00:00",
        store: { id: "st-81", name: "Landmark 81", address: "Bình Thạnh", isOpenNow: true },
        coWorkers: []
    }
]

/**
 * Hàm hỗ trợ tạo dữ liệu giả lập (Mock) cho Agenda.
 * Giúp FE phát triển độc lập không phụ thuộc BE.
 */
export const getMockAgendaData = async (params: GetAgendaParams): Promise<StaffShiftAgenda[]> => {
    // Giả lập độ trễ mạng để thấy Skeleton/Spinner
    await new Promise((resolve) => setTimeout(resolve, 600))

    const { startDate, endDate } = params

    // 1. Thực hiện lọc
    const filteredData = FIXED_MOCK_AGENDA.filter((item) => {
        const itemDate = dayjs(item.date)
        // Kiểm tra ngày có nằm trong khoảng [startDate, endDate]
        return itemDate.isBetween(startDate, endDate, "day", "[]")
    })

    // 2. Sắp xếp tăng dần theo thời gian để hiển thị Timeline đúng thứ tự
    return filteredData.sort((a, b) => {
        const dateA = dayjs(`${a.date} ${a.startTime}`).unix()
        const dateB = dayjs(`${b.date} ${b.startTime}`).unix()
        return dateA - dateB
    })
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

/**
 * Hook lấy danh sách lịch làm việc của nhân viên theo khoảng ngày.
 * Tự động kích hoạt lại khi startDate hoặc endDate thay đổi.
 * @param params - Đối tượng chứa ngày bắt đầu và kết thúc (YYYY-MM-DD)
 * @param enabled - Flag cho phép/ngăn chặn việc tự động fetch (VD: Chỉ fetch khi đã có Role Staff)
 */
export const useGetMyAgenda = (params: GetAgendaParams, enabled = true) => {
    return useQuery({
        // Key bao gồm params để TanStack Query tự động phân tách cache theo từng khoảng ngày khác nhau
        queryKey: [
            QUERY_KEYS.ME,
            QUERY_KEYS.SHIFTS,
            QUERY_KEYS.AGENDA,
            params.startDate,
            params.endDate
        ],

        queryFn: async () => {
            const res = await getMockAgendaData(params)
            return res
        },

        // Chỉ chạy khi thỏa mãn: Role hợp lệ, có đủ ngày bắt đầu và kết thúc
        enabled: enabled && !!params.startDate && !!params.endDate,

        /**
         * Lịch làm việc thường cố định, ít thay đổi đột ngột.
         * Để staleTime 5 phút giúp tiết kiệm tài nguyên hệ thống, tránh fetch lại khi nhân viên chuyển tab.
         */
        staleTime: 1000 * 60 * 5,

        // Giữ lại dữ liệu cũ trong lúc fetch dữ liệu mới để UI không bị giật (Loading skeleton)
        placeholderData: (previousData) => previousData
    })
}
