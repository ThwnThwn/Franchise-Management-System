import { LoginModal } from "@/features/auth/components"
import React from "react"

/* ========================= A ========================= */
export * from "./AlertModal"

export const Modals = () => {
    return (
        <div>
            {/* <ForgotPasswordModal />
            <LoginModal />
            <RegisterModal /> */}
            <LoginModal />
        </div>
    )
}
