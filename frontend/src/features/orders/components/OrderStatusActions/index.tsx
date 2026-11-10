"use client"

import {
    ORDER_STATUS_TRANSITION_POLICY,
    OrderStatus,
    OrderStatusColors,
    OrderStatusLabels
} from "@/features/orders/constants"
import { Button } from "@heroui/react"
import { ArrowRight } from "lucide-react"
import { isManualActionRequired } from "@/features/orders/utils"

interface Props {
    currentStatus: OrderStatus
    onUpdate: (next: OrderStatus) => void
    isLoading?: boolean
}

export const OrderStatusActions = ({ currentStatus, onUpdate, isLoading = false }: Props) => {
    const canManual = isManualActionRequired(currentStatus)
    const nextStatuses = ORDER_STATUS_TRANSITION_POLICY[currentStatus]

    if (!canManual || nextStatuses.length === 0) return null

    return (
        <div className="flex flex-col gap-3 p-4 bg-default-50 rounded-2xl border border-default-100">
            <p className="text-[10px] font-black uppercase text-default-400 tracking-widest ml-1">
                Hành động tiếp theo
            </p>

            <div className="flex flex-wrap justify-center gap-3">
                {nextStatuses.map((status, index) => {
                    const isPrimary = index === 0 && status !== OrderStatus.Cancelled

                    return (
                        <Button
                            key={status}
                            size="md"
                            variant={isPrimary ? "shadow" : "flat"}
                            color={OrderStatusColors[status]}
                            isLoading={isLoading}
                            onPress={() => onUpdate(status)}
                            className="font-black px-6"
                            startContent={
                                isPrimary && (
                                    <div className="relative flex h-2 w-2 mr-1">
                                        {/* Hiệu ứng vòng tròn tỏa ra (Ping) */}
                                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-current opacity-75"></span>
                                        {/* Dấu chấm đứng yên */}
                                        <span className="relative inline-flex rounded-full h-2 w-2 bg-current"></span>
                                    </div>
                                )
                            }
                            endContent={isPrimary && <ArrowRight size={16} />}
                        >
                            <span className="uppercase tracking-tighter">
                                {OrderStatusLabels[status]}
                            </span>
                        </Button>
                    )
                })}
            </div>
        </div>
    )
}
