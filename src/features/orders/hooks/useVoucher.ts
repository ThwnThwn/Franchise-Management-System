import { useQuery, useMutation } from "@tanstack/react-query"
import { voucherApi } from "../services/voucherApi"
import { CreateVoucherRequest, UpdateVoucherRequest, UpdateVoucherStatusRequest } from "../models/voucher"

import { QUERY_KEYS } from "@/constants/queryKeys"

export const useGetAllVouchers = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.VOUCHERS],
        queryFn: () => voucherApi.getAll()
    })
}

export const useCreateVoucher = () => {
    return useMutation({
        mutationFn: (req: CreateVoucherRequest) => voucherApi.create(req)
    })
}

export const useUpdateVoucher = () => {
    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateVoucherRequest }) =>
            voucherApi.update(id, req)
    })
}

export const useUpdateVoucherStatus = () => {
    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateVoucherStatusRequest }) =>
            voucherApi.updateStatus(id, req)
    })
}

export const useDeleteVoucher = () => {
    return useMutation({
        mutationFn: (id: string) => voucherApi.delete(id)
    })
}

export const useGetMyVouchers = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.MY_VOUCHERS],
        queryFn: () => voucherApi.getMyVouchers()
    })
}

export const useRedeemVoucher = () => {
    return useMutation({
        mutationFn: (id: string) => voucherApi.redeemVoucher(id)
    })
}

export const useCollectVoucher = () => {
    return useMutation({
        mutationFn: (id: string) => voucherApi.collectVoucher(id)
    })
}

export const useAssignVoucher = () => {
    return useMutation({
        mutationFn: ({ id, userIds }: { id: string; userIds: string[] }) => voucherApi.assignVoucher(id, userIds)
    })
}
