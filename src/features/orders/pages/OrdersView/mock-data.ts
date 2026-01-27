import { Order } from "@/features/orders/models/response"
import { OrderStatus, PaymentMethod } from "../../constants"
// Hoặc đường dẫn tới file types của bạn

export const mockOrders: Order[] = [
    // 1. Đơn hàng thành công (Paid - VNPay - Nhiều món)
    {
        id: "uuid-1",
        code: "ORD-2401-001",
        subtotal: 110000,
        tax: 8800, // 8% VAT
        totalAmount: 118800,
        paidAmount: 118800,
        paymentMethod: PaymentMethod.VNPay, // Enum 1
        status: OrderStatus.Paid, // Enum 1
        paidAt: "2026-01-27T08:30:00Z",
        notes: "Ít đường, nhiều đá nha shop",
        createdAt: "2026-01-27T08:15:00Z",
        updatedAt: "2026-01-27T08:30:00Z",
        customerId: "cust-uuid-1",
        items: [
            {
                id: "item-1",
                productId: "prod-1",
                variantId: "var-1",
                quantity: 2,
                unitPrice: 35000,
                productName: "Cà Phê Muối Huế",
                sku: "CFM-M",
                imageUrl:
                    "https://images.unsplash.com/photo-1600093463592-8e36ae95ef56?q=80&w=200&auto=format&fit=crop"
            },
            {
                id: "item-2",
                productId: "prod-2",
                variantId: "var-2",
                quantity: 1,
                unitPrice: 40000,
                productName: "Croissant Trứng Muối",
                sku: "CR-TM",
                imageUrl:
                    "https://images.unsplash.com/photo-1555507036-ab1f40388085?q=80&w=200&auto=format&fit=crop"
            }
        ]
    },

    // 2. Đơn hàng mới đặt (Pending - Tiền mặt - Chưa thanh toán)
    {
        id: "uuid-2",
        code: "ORD-2401-002",
        subtotal: 45000,
        tax: 3600,
        totalAmount: 48600,
        paidAmount: 0, // Chưa trả đồng nào
        paymentMethod: PaymentMethod.Cash, // Enum 0
        status: OrderStatus.Pending, // Enum 0
        paidAt: undefined, // Chưa thanh toán
        notes: undefined,
        createdAt: "2026-01-27T10:00:00Z",
        updatedAt: "2026-01-27T10:00:00Z",
        customerId: "cust-uuid-2",
        items: [
            {
                id: "item-3",
                productId: "prod-3",
                variantId: "var-3",
                quantity: 1,
                unitPrice: 45000,
                productName: "Trà Đào Cam Sả (Size L)",
                sku: "TDCS-L",
                imageUrl:
                    "https://images.unsplash.com/photo-1595981267035-7b04ca84a82d?q=80&w=200&auto=format&fit=crop"
            }
        ]
    },

    // 3. Đơn hàng đã hủy (Cancelled - Khách vãng lai - Không có ảnh SP)
    {
        id: "uuid-3",
        code: "ORD-2401-003",
        subtotal: 300000,
        tax: 24000,
        totalAmount: 324000,
        paidAmount: 0,
        paymentMethod: undefined, // Hủy trước khi chọn thanh toán
        status: OrderStatus.Cancelled, // Enum 2
        paidAt: undefined,
        notes: "Giao gấp trong 15p",
        createdAt: "2026-01-26T15:00:00Z",
        updatedAt: "2026-01-26T15:30:00Z",
        customerId: undefined, // Khách vãng lai (Guest)
        items: [
            {
                id: "item-4",
                productId: "prod-4",
                variantId: "var-4",
                quantity: 10,
                unitPrice: 30000,
                productName: "Cà Phê Đen Đá (Mang đi)",
                sku: "CFD-S",
                imageUrl: undefined // Test trường hợp không có ảnh (hiện No Img)
            }
        ]
    },

    // 4. Đơn hàng lớn (Paid - Chuyển khoản/VNPay)
    {
        id: "uuid-4",
        code: "ORD-2401-004",
        subtotal: 1250000,
        tax: 100000,
        totalAmount: 1350000,
        paidAmount: 1350000,
        paymentMethod: PaymentMethod.VNPay,
        status: OrderStatus.Paid,
        paidAt: "2026-01-25T09:00:00Z",
        notes: "Xuất hóa đơn công ty ABC",
        createdAt: "2026-01-25T08:45:00Z",
        updatedAt: "2026-01-25T09:00:00Z",
        customerId: "cust-uuid-1", // Khách hàng quay lại mua tiếp
        items: [
            {
                id: "item-5",
                productId: "prod-5",
                variantId: "var-5",
                quantity: 5,
                unitPrice: 55000,
                productName: "Hộp quà Cà Phê Hạt (Robusta)",
                sku: "GIFT-ROB",
                imageUrl:
                    "https://images.unsplash.com/photo-1611854779393-1b2da9d400fe?q=80&w=200&auto=format&fit=crop"
            },
            {
                id: "item-6",
                productId: "prod-6",
                variantId: "var-6",
                quantity: 5,
                unitPrice: 195000,
                productName: "Set Bánh TeaBreak",
                sku: "TB-SET",
                imageUrl:
                    "https://images.unsplash.com/photo-1509365465985-25d11c17e812?q=80&w=200&auto=format&fit=crop"
            }
        ]
    }
]
