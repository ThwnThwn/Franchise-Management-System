import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
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
        note: "Đi học quân sự (Mock)",
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
        note: "Valentine rảnh đi làm (Mock)",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
    }
]

// --- MOCK API FUNCTIONS ---
const mockApi = {
    getMyHistory: async (pagination: PaginationParams) => {
        await new Promise((r) => setTimeout(r, 800))
        return {
            items: MOCK_DATABASE,
            totalItems: MOCK_DATABASE.length,
            totalPages: 1,
            page: pagination.page || 1,
            pageSize: pagination.pageSize || 10
        }
    },
    create: async (req: CreateShiftAvailabilityRequest) => {
        await new Promise((r) => setTimeout(r, 500))
        const newItem: ShiftAvailabilityResponse = {
            ...req,
            id: Math.random().toString(36).substr(2, 9),
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
        queryFn: () => mockApi.getMyHistory(pagination),
        enabled
    })
}

// --- MUTATION HOOKS ---

export const useCreateAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: mockApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Gửi yêu cầu lịch làm việc thành công",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
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
                description: "Cập nhật lịch thành công",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
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
                description: "Đã xóa lịch đăng ký",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}
