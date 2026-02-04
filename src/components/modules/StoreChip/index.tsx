"use client"

import { Chip, ChipProps, cn } from "@heroui/react"
import { MapPin } from "lucide-react"
import React from "react"

export interface StoreChipProps extends ChipProps {
    storeName?: string
}

export function StoreChip(props: StoreChipProps) {
    return (
        <Chip
            className={cn("hover:cursor-pointer font-semibold", props.className)}
            onClick={props.onClick}
            startContent={props.startContent ? props.startContent : <MapPin size={16} />}
        >
            {props.storeName || "-"}
        </Chip>
    )
}
