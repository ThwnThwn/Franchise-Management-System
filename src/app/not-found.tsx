"use client"

import React from "react"
import Link from "next/link"
import { ButtonStyled } from "@/components"

export default function NotFound() {
    return (
        <div className="-my-25 flex flex-col items-center justify-center min-w-full min-h-screen bg-background text-foreground gap-4">
            <h2 className="text-4xl font-bold text-primary">Not Found</h2>
            <p className="text-default-500">Xin lỗi, trang bạn tìm kiếm không tồn tại.</p>

            <ButtonStyled as={Link} href="/" color="primary" variant="solid">
                Về trang chủ
            </ButtonStyled>
        </div>
    )
}
