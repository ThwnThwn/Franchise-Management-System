"use client"

import React from "react"
import { AlertModal } from "@/components/modals"
import { useConfirmStore } from "@/hooks"

export const GlobalConfirmDialog = () => {
    const { isOpen, options, handleConfirm, handleCancel } = useConfirmStore()

    return (
        <AlertModal
            isOpen={isOpen}
            onClose={handleCancel}
            onConfirm={handleConfirm}
            header={options.title}
            body={options.message}
            variant={options.variant}
            confirmText={options.confirmText}
            cancelText={options.cancelText}
        />
    )
}
