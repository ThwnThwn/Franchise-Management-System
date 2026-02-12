import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import {
    GetAvailabilityParams,
    CreateShiftAvailabilityRequest,
    UpdateShiftAvailabilityRequest
} from "@/features/shifts/models/request"
import { PaginationParams } from "@/models/common/request"
import { ShiftAvailabilityResponse } from "@/features/shifts/models/response"
import { DayOfWeek } from "@/constants/enums"

// --- MOCK DATA STORAGE (In-memory) ---
let MOCK_DATABASE: ShiftAvailabilityResponse[] = [
    {
        id: "1",
        userId: "user-001",
        isAvailable: false,
        isRecurring: true,
        dayOfWeek: DayOfWeek.MONDAY,
        startTime: "08:00:00",
        endTime: "12:00:00",
        note: "Đi học quân sự (Thứ 2 hàng tuần)",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
    },
    {
        id: "2",
        userId: "user-001",
        isAvailable: true,
        isRecurring: false,
        specificDate: "2026-02-14",
        startTime: "18:00:00",
        endTime: "22:00:00",
        note: "Valentine rảnh đi làm (Ngày lẻ)",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
    },
    {
        id: "3",
        userId: "user-001",
        isAvailable: false,
        isRecurring: false,
        specificDate: "2026-02-20",
        startTime: "07:00:00",
        endTime: "11:00:00",
        note: "Bận việc gia đình",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
    }
]

// --- MOCK API FUNCTIONS ---
const mockApi = {
    getMyHistory: async (query: GetAvailabilityParams, pagination: PaginationParams) => {
        // Giả lập delay mạng
        await new Promise((r) => setTimeout(r, 600))

        // 1. Thực hiện FILTER logic
        let filteredData = [...MOCK_DATABASE]

        // Filter theo Rảnh/Bận
        if (query.isAvailable !== undefined) {
            filteredData = filteredData.filter((item) => item.isAvailable === query.isAvailable)
        }

        // Filter theo Lặp lại/Lẻ
        if (query.isRecurring !== undefined) {
            filteredData = filteredData.filter((item) => item.isRecurring === query.isRecurring)
        }

        // Filter theo Keyword (Note) - Không phân biệt hoa thường
        if (query.keyword) {
            const lowKey = query.keyword.toLowerCase()
            filteredData = filteredData.filter((item) => item.note?.toLowerCase().includes(lowKey))
        }

        // 2. Thực hiện PAGINATION logic
        const totalItems = filteredData.length
        const page = pagination.page || 1
        const pageSize = pagination.pageSize || 10
        const totalPages = Math.ceil(totalItems / pageSize)

        const pagedData = filteredData.slice((page - 1) * pageSize, page * pageSize)

        return {
            items: pagedData,
            totalItems,
            totalPages,
            page,
            pageSize
        }
    },

    create: async (req: CreateShiftAvailabilityRequest) => {
        await new Promise((r) => setTimeout(r, 500))
        const newItem: ShiftAvailabilityResponse = {
            ...req,
            id: Math.random().toString(36).substring(2, 9),
            userId: "user-001",
            startTime: req.startTime.length === 5 ? `${req.startTime}:00` : req.startTime,
            endTime: req.endTime.length === 5 ? `${req.endTime}:00` : req.endTime,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            specificDate: req.specificDate || undefined,
            dayOfWeek: req.dayOfWeek ?? undefined,
            note: req.note || ""
        }
        MOCK_DATABASE = [newItem, ...MOCK_DATABASE]
    },

    update: async (id: string, req: UpdateShiftAvailabilityRequest) => {
        await new Promise((r) => setTimeout(r, 500))
        MOCK_DATABASE = MOCK_DATABASE.map((item) =>
            item.id === id ? { ...item, ...req, updatedAt: new Date().toISOString() } : item
        )
    },

    delete: async (id: string) => {
        await new Promise((r) => setTimeout(r, 500))
        MOCK_DATABASE = MOCK_DATABASE.filter((item) => item.id !== id)
    }
}

// --- QUERY HOOKS ---

export const useGetMyAvailabilityHistory = ({
    query,
    pagination,
    enabled = true
}: {
    query: GetAvailabilityParams
    pagination: PaginationParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES, QUERY_KEYS.ME, query, pagination],
        queryFn: () => mockApi.getMyHistory(query, pagination), // Truyền thêm query vào đây
        enabled
    })
}

// --- MUTATION HOOKS (Giữ nguyên như cũ) ---
export const useCreateAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: mockApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Gửi yêu cầu thành công",
                color: "success"
            })
            onSuccess?.()
        }
    })
}

export const useUpdateAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateShiftAvailabilityRequest }) =>
            mockApi.update(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật thành công",
                color: "success"
            })
            onSuccess?.()
        }
    })
}

export const useDeleteAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: mockApi.delete,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xóa lịch",
                color: "success"
            })
            onSuccess?.()
        }
    })
}
