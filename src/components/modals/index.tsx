"use client"

import { LoginModal } from "@/features"
import { RegisterModal } from "@/features/auth/components/RegisterModal"
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
            <RegisterModal />
        </div>
    )
}
