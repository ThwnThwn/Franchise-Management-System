"use client"

import React from "react"
import { createPortal } from "react-dom"
import { motion, AnimatePresence } from "framer-motion"
import { RotateCcw, Save } from "lucide-react"
import { ButtonIconStyled } from "@/components"

type Props = {
    hasChanges: boolean
    isLoading: boolean
    onSave: () => void
    onReset: () => void
    title?: string
    message?: string
}

export function UnsavedChangesNotifier({
    hasChanges,
    isLoading,
    onSave,
    onReset,
    title = "Cẩn thận!",
    message = "Bạn có thay đổi chưa lưu."
}: Props) {
    if (typeof document === "undefined") return null

    return createPortal(
        <AnimatePresence>
            {hasChanges && (
                <motion.div
                    initial={{ y: 100, x: "-50%", opacity: 0 }}
                    animate={{ y: 0, x: "-50%", opacity: 1 }}
                    exit={{ y: 100, x: "-50%", opacity: 0 }}
                    transition={{ type: "spring", stiffness: 300, damping: 30 }}
                    className="fixed bottom-8 left-1/2 z-51 bg-slate-900 border border-slate-700 text-white px-6 py-4 rounded-2xl shadow-2xl flex items-center gap-6 min-w-[320px] max-w-[90vw]"
                >
                    <div className="flex flex-col">
                        <span className="text-sm font-bold text-warning">{title}</span>
                        <span className="text-xs text-slate-400 font-medium">{message}</span>
                    </div>

                    <div className="ml-auto flex items-center gap-3">
                        {!isLoading && (
                            <ButtonIconStyled
                                size="sm"
                                variant="light"
                                className="text-white hover:bg-white/10"
                                onPress={onReset}
                                title="Khôi phục"
                            >
                                <RotateCcw size={18} />
                            </ButtonIconStyled>
                        )}

                        <ButtonIconStyled
                            size="sm"
                            color="warning"
                            onPress={onSave}
                            isLoading={isLoading}
                            title="Lưu thay đổi"
                        >
                            <Save size={18} />
                        </ButtonIconStyled>
                    </div>
                </motion.div>
            )}
        </AnimatePresence>,
        document.body
    )
}
