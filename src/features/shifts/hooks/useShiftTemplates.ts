import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import {
    CreateShiftTemplateRequest,
    GetShiftTemplatesParams,
    UpdateShiftTemplateRequest
} from "../models/request"
import { ShiftTemplateResponse } from "../models/response"

// --- MOCK DATABASE (Lưu trong RAM để test logic thêm/sửa/xóa) ---
let MOCK_DB: ShiftTemplateResponse[] = [
    {
        id: "tpl-1",
        storeId: null, // Global Template (Admin tạo)
        code: "CA-SANG-HANH-CHINH",
        name: "Ca Hành Chính",
        startTime: "08:00:00",
        endTime: "17:00:00",
        breakDurationMinutes: 60,
        isActive: true,
        createdAt: new Date().toISOString()
    },
    {
        id: "tpl-2",
        storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd", // Template của cửa hàng hiện tại
        code: "CA-CHIEU",
        name: "Ca Chiều (Part-time)",
        startTime: "13:00:00",
        endTime: "18:00:00",
        breakDurationMinutes: 0,
        isActive: true,
        createdAt: new Date().toISOString()
    },
    {
        id: "tpl-3",
        storeId: "b7ac8328-2ca3-470d-80ca-15444eeab2dd",
        code: "CA-TOI",
        name: "Ca Tối (Đông khách)",
        startTime: "18:00:00",
        endTime: "23:00:00",
        breakDurationMinutes: 15,
        isActive: true,
        createdAt: new Date().toISOString()
    }
]

// Hàm delay giả lập mạng lag (500ms)
const fakeDelay = (ms = 500) => new Promise((resolve) => setTimeout(resolve, ms))

// --- QUERY HOOKS ---

export const useGetShiftTemplates = ({
    params,
    enabled = true
}: {
    params: GetShiftTemplatesParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_TEMPLATES, params],
        queryFn: async () => {
            await fakeDelay()

            // Giả lập lọc dữ liệu theo storeId và Global
            const filtered = MOCK_DB.filter(
                (t) => t.storeId === params.storeId || t.storeId === null
            )

            // Trả về đúng format PageResult
            return filtered
        },
        enabled
    })
}

export const useGetShiftTemplateById = ({
    id,
    enabled = true
}: {
    id: string
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_TEMPLATES, id],
        queryFn: async () => {
            await fakeDelay()
            const found = MOCK_DB.find((t) => t.id === id)
            if (!found) throw new Error("Template not found")
            return found
        },
        enabled
    })
}

// --- MUTATION HOOKS ---

export const useCreateShiftTemplate = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (req: CreateShiftTemplateRequest) => {
            await fakeDelay(800) // Giả vờ xử lý lâu tí

            const newItem: ShiftTemplateResponse = {
                id: `tpl-${Date.now()}`, // Fake ID
                storeId: req.storeId,
                code: req.code,
                name: req.name,
                startTime: req.startTime,
                endTime: req.endTime,
                breakDurationMinutes: req.breakDurationMinutes,
                isActive: true,
                createdAt: new Date().toISOString()
            }

            MOCK_DB.push(newItem) // Lưu vào Mock DB
            return newItem
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_TEMPLATES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo mẫu ca làm việc thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: "Lỗi giả lập: Không thể tạo",
                color: "danger"
            })
        }
    })
}

export const useUpdateShiftTemplate = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id, req }: { id: string; req: UpdateShiftTemplateRequest }) => {
            await fakeDelay(600)

            const index = MOCK_DB.findIndex((t) => t.id === id)
            if (index === -1) throw new Error("Not found")

            // Merge data cũ với data mới
            MOCK_DB[index] = {
                ...MOCK_DB[index],
                ...req,
                // Giữ lại các field không có trong req
                code: req.code || MOCK_DB[index].code,
                name: req.name || MOCK_DB[index].name,
                startTime: req.startTime || MOCK_DB[index].startTime,
                endTime: req.endTime || MOCK_DB[index].endTime,
                breakDurationMinutes:
                    req.breakDurationMinutes ?? MOCK_DB[index].breakDurationMinutes
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_TEMPLATES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật mẫu ca thành công (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: "Lỗi giả lập: Không thể cập nhật",
                color: "danger"
            })
        }
    })
}

export const useDeleteShiftTemplate = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id }: { id: string }) => {
            await fakeDelay(600)

            // Lọc bỏ item có id tương ứng
            MOCK_DB = MOCK_DB.filter((t) => t.id !== id)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_TEMPLATES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xoá mẫu ca làm việc (MOCK)",
                color: "success"
            })
            onSuccess?.()
        },
        onError: () => {
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: "Lỗi giả lập: Không thể xóa",
                color: "danger"
            })
        }
    })
}
