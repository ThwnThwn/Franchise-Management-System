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
    withInput?: boolean
    inputPlaceholder?: string
    isInputRequired?: boolean
}

type ConfirmStore = {
    isOpen: boolean
    options: ConfirmDialogOptions
    resolveRef: ((value: boolean | string) => void) | null

    // Hàm gọi mở dialog
    confirm: (options: ConfirmDialogOptions) => Promise<boolean | string>

    // Internal actions
    close: () => void
    handleConfirm: (inputValue?: string) => void
    handleCancel: () => void
}

// Giá trị mặc định nếu người dùng không truyền
const DEFAULT_OPTIONS: ConfirmDialogOptions = {
    message: "Bạn có chắc chắn muốn thực hiện hành động này?",
    title: "Xác nhận",
    confirmText: STRINGS.COMMON.CONFIRM, // "Xác nhận"
    cancelText: STRINGS.COMMON.CANCEL, // "Hủy"
    variant: "danger",
    withInput: false,
    isInputRequired: true
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

    handleConfirm: (inputValue?: string) => {
        const { resolveRef, options } = get()
        if (resolveRef) resolveRef(options.withInput ? inputValue || "" : true)
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
export const confirmDialog = async (
    options: Omit<ConfirmDialogOptions, "withInput" | "inputPlaceholder">
): Promise<boolean> => {
    const res = await useConfirmStore.getState().confirm({ ...options, withInput: false })
    return res as boolean
}

export const promptDialog = async (
    options: Omit<ConfirmDialogOptions, "withInput">
): Promise<string | false> => {
    const res = await useConfirmStore.getState().confirm({ ...options, withInput: true })
    return res as string | false
}
