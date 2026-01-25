"use client"

import { LoginModal } from "@/features"
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
