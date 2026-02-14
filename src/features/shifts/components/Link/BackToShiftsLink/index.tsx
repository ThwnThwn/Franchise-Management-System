import React from "react"
import NextLink from "next/link"
import { Link } from "@heroui/react"
import { ChevronLeft } from "lucide-react"

export const BackToShiftsLink = () => {
    return (
        <Link as={NextLink} href="/dashboard/shifts" className="gap-1.5 font-bold h-fit">
            <ChevronLeft size={16} strokeWidth={2.5} />
            Quay lại Lịch làm việc
        </Link>
    )
}
