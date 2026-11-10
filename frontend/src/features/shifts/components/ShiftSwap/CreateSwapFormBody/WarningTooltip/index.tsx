"use client"

import { Chip, Tooltip } from "@heroui/react"
import { AlertTriangle } from "lucide-react"

// BỔ SUNG: Component nội bộ để tái sử dụng Tooltip Cảnh Báo
export const WarningTooltip = ({
    errors,
    placement = "top"
}: {
    errors: string[]
    placement?: "top" | "left"
}) => (
    <Tooltip
        content={
            <div className="p-1 max-w-50">
                <div className="font-bold mb-1 text-tiny">Lưu ý hệ thống:</div>
                {errors.map((err, i) => (
                    <div key={i} className="text-tiny text-white/90">
                        • {err}
                    </div>
                ))}
            </div>
        }
        color="warning"
        placement={placement}
    >
        <div className="cursor-help">
            <Chip
                size="sm"
                color="warning"
                variant="flat"
                startContent={<AlertTriangle size={12} />}
            >
                Cảnh báo
            </Chip>
        </div>
    </Tooltip>
)
