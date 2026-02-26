import { StockStatus } from "./enum"

export const StockStatusLabels: Record<StockStatus, string> = {
    [StockStatus.Sufficient]: "Còn hàng",
    [StockStatus.LowStock]: "Sắp hết hàng",
    [StockStatus.OutOfStock]: "Hết hàng"
}
