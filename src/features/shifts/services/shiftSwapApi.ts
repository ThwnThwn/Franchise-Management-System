import axiosInstance from "@/utils/axios"
import {
    ApproveSwapRequest,
    GetAllSwapsParams,
    RequestSwapRequest,
    RespondSwapRequest
} from "../models/request"
import { SwapRequestItem } from "../models/response"
import { buildQueryParams } from "@/utils/helpers/axiosHelper"

/**
 * SHIFT SWAP API
 * @title Điều phối đổi ca & Nhả ca
 * @description Resource-based: /shifts/{id}/swaps và /shift-swaps/{id}
 */
export const shiftSwapApi = {
    /**
     * Xem tất cả swap request
     * @param query - { status?: ShiftSwapStatus }
     */
    getAll: async (query: GetAllSwapsParams): Promise<SwapRequestItem[]> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/shifts/swaps", { params })
        return res.data
    },

    /**
     * Gửi yêu cầu đổi ca (A gửi cho B)
     * @param assignmentId - ID ca làm việc của người gửi
     * @param req - { targetUserId: string, reason?: string }
     */
    request: async ({
        assignmentId,
        req
    }: {
        assignmentId: string
        req: RequestSwapRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.post(`/shifts/${assignmentId}/swaps`, req)
        return res.data
    },

    /**
     * Phản hồi yêu cầu đổi ca (B phản hồi cho A)
     * @param swapId - ID của yêu cầu đổi ca (SwapRequest)
     * @param req - { accept: boolean, rejectReason?: string }
     */
    respond: async ({
        swapId,
        req
    }: {
        swapId: string
        req: RespondSwapRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.patch(`/shift-swaps/${swapId}/responses`, req)
        return res.data
    },

    /**
     * Manager duyệt cuối cùng để chính thức đổi ca
     */
    approve: async ({
        swapId,
        req
    }: {
        swapId: string
        req: ApproveSwapRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.patch(`/shift-swaps/${swapId}/approvals`, req)
        return res.data
    },

    /**
     * Tự hủy yêu cầu đổi ca đã gửi
     */
    cancel: async ({ swapId }: { swapId: string }): Promise<void> => {
        await axiosInstance.patch(`/shift-swaps/${swapId}/cancel`)
    },

    /**
     * Nhả ca đang nắm giữ về Chợ ca (Shift Pool)
     * @param assignmentId - ID ca làm việc cần nhả
     */
    release: async ({ assignmentId }: { assignmentId: string }): Promise<{ message?: string }> => {
        const res = await axiosInstance.patch(`/shifts/${assignmentId}/releases`)
        return res.data
    }
}
