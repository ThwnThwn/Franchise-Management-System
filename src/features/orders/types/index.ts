import { OrderItem } from "@/features/orders/models/response"

export interface OrderItemDisplay extends OrderItem {
    displayQuantity: number
}
