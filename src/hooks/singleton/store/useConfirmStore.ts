import { create } from "zustand"
import { ReactNode } from "react"
import { STRINGS } from "@/constants/strings"

// Định nghĩa Option nhận vào linh hoạt
export type ConfirmDialogOptions = {
    title?: string
    message?: ReactNode
    confirmText?: string
    cancelText?: string
    variant?: "danger" | "success" | "warning" | "info"
}

type ConfirmStore = {
    isOpen: boolean
    options: ConfirmDialogOptions
    resolveRef: ((value: boolean) => void) | null

    // Hàm gọi mở dialog
    confirm: (options: ConfirmDialogOptions) => Promise<boolean>

    // Internal actions
    close: () => void
    handleConfirm: () => void
    handleCancel: () => void
}

// Giá trị mặc định nếu người dùng không truyền
const DEFAULT_OPTIONS: ConfirmDialogOptions = {
    message: "Bạn có chắc chắn muốn thực hiện hành động này?",
    title: "Xác nhận",
    confirmText: STRINGS.COMMON.CONFIRM, // "Xác nhận"
    cancelText: STRINGS.COMMON.CANCEL, // "Hủy"
    variant: "danger"
}

export const useConfirmStore = create<ConfirmStore>((set, get) => ({
    isOpen: false,
    options: DEFAULT_OPTIONS,
    resolveRef: null,

    confirm: (userOptions) => {
        return new Promise((resolve) => {
            set({
                isOpen: true,
                // Merge option của user với mặc định
                options: { ...DEFAULT_OPTIONS, ...userOptions },
                resolveRef: resolve
            })
        })
    },

    close: () => set({ isOpen: false }),

    handleConfirm: () => {
        const { resolveRef } = get()
        if (resolveRef) resolveRef(true)
        set({ isOpen: false, resolveRef: null })
    },

    handleCancel: () => {
        const { resolveRef } = get()
        if (resolveRef) resolveRef(false)
        set({ isOpen: false, resolveRef: null })
    }
}))

// --- EXPORT HELPER FUNCTION ---
// Cách dùng mới: confirmDialog({ title: "Xóa?", message: "Chắc chưa?", variant: "warning" })
export const confirmDialog = (options: ConfirmDialogOptions) => {
    return useConfirmStore.getState().confirm(options)
}
