import React from "react"
import { Link } from "@heroui/react"
import { ChevronRight } from "lucide-react"
import NextLink from "next/link"

export const GoToTimeLogLink = () => {
    return (
        <Link as={NextLink} href="/dashboard/shifts/time-logs" className="gap-1.5 font-bold h-fit">
            Xem chấm công
            <ChevronRight size={16} strokeWidth={2.5} />
        </Link>
    )
}
