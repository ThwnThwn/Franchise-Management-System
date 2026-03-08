import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { shiftApplicationApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { promptDialog } from "@/hooks"

/**
 * Lấy danh sách nhân viên đang chờ nhận một ca cụ thể
 * Thường dùng cho Manager khi bấm vào một ca ở Chợ Ca
 */
export const useGetApplicationsByShiftId = ({
    shiftId,
    enabled = true
}: {
    shiftId: string
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_APPLICATIONS, shiftId],
        queryFn: () => shiftApplicationApi.getByShiftId({ shiftId }),
        enabled: !!shiftId && enabled
    })
}

// --- [Story: Staff nộp đơn nhận ca] ---
export const useApplyForShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: shiftApplicationApi.apply, // API lõi vẫn giữ nguyên, rất trong sáng
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_APPLICATIONS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã gửi yêu cầu nhận ca thành công!",
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

    // BỔ SUNG: Helper function bọc sẵn luồng UI (Dialog) + Logic (Mutation)
    const applyWithConfirm = async (shiftId: string) => {
        const warningText = (
            <div className="text-left text-sm">
                <p className="font-bold text-danger mb-1">⚠️ LƯU Ý QUAN TRỌNG:</p>
                <p className="text-default-600">
                    Một khi đã đăng ký nhận ca, bạn sẽ <b>KHÔNG THỂ</b> tự ý hủy bỏ. Vui lòng đảm
                    bảo bạn có thể sắp xếp thời gian làm việc trước khi xác nhận.
                </p>
            </div>
        )

        const noteOrCancel = await promptDialog({
            title: "Xác nhận nhận ca",
            message: warningText,
            variant: "warning",
            confirmText: "Đồng ý nhận ca",
            inputPlaceholder: "Ghi chú thêm cho Quản lý (Không bắt buộc)...",
            isInputRequired: false // Không ép nhập
        })

        // Ném lỗi ngầm nếu user bấm Hủy (để UI ngoài có catch thì catch, không thì thôi)
        if (noteOrCancel === false) return Promise.reject(new Error("USER_CANCELLED"))

        // Nếu xác nhận -> Kích hoạt lõi mutation
        return mutation.mutateAsync({
            shiftId,
            req: {
                note: noteOrCancel as string
            }
        })
    }

    // Trả ra cả mutation gốc (nếu sau này cần dùng trần) và hàm bọc
    return {
        ...mutation,
        applyWithConfirm
    }
}

// --- [Story: Manager duyệt/từ chối đơn] ---
export const useReviewShiftApplication = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: shiftApplicationApi.review,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_APPLICATIONS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã xử lý đơn đăng ký thành công",
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

    // BỔ SUNG: Hàm Helper để gọi duyệt/từ chối kèm UI Dialog
    const reviewWithConfirm = async (applicationId: string, isApproved: boolean) => {
        // 1. Dựng nội dung UI linh hoạt dựa trên hành động
        const actionText = isApproved ? "Phê duyệt" : "Từ chối"
        const variantType = isApproved ? "success" : "danger"
        const placeholder = isApproved
            ? "Lời nhắn cho nhân viên (Không bắt buộc)..."
            : "Vui lòng nhập lý do từ chối (Bắt buộc)..."

        const warningText = (
            <div className="text-left text-sm">
                <p>
                    Bạn có chắc chắn muốn{" "}
                    <b className={isApproved ? "text-success" : "text-danger"}>{actionText}</b> đơn
                    nhận ca này?
                </p>
                {!isApproved && (
                    <p className="mt-1 text-xs text-danger italic">
                        *Lý do từ chối sẽ được gửi trực tiếp đến nhân viên.
                    </p>
                )}
            </div>
        )

        // 2. Bật Dialog (Lưu ý cờ isInputRequired)
        const noteOrCancel = await promptDialog({
            title: `Xác nhận ${actionText}`,
            message: warningText,
            variant: variantType,
            confirmText: `Đồng ý ${actionText}`,
            inputPlaceholder: placeholder,
            isInputRequired: !isApproved
        })

        // 3. User bấm Hủy -> Bỏ qua
        if (noteOrCancel === false) return Promise.reject(new Error("USER_CANCELLED"))

        // 4. User bấm Xác nhận -> Bắn Mutation gốc
        return mutation.mutateAsync({
            id: applicationId,
            req: {
                isApproved: isApproved,
                reviewNote: noteOrCancel as string
            }
        })
    }

    return {
        ...mutation,
        reviewWithConfirm
    }
}
