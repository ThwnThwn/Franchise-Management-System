import { ChipProps } from "@heroui/react"
import { StockStatus } from "./enum"

export const StockStatusColors: Record<StockStatus, ChipProps["color"]> = {
    [StockStatus.Sufficient]: "success",
    [StockStatus.LowStock]: "warning",
    [StockStatus.OutOfStock]: "danger"
}
