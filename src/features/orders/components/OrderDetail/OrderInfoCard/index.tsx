import {
    ORDER_TEXT,
    OrderType,
    OrderTypeLabel,
    PaymentMethodLabels
} from "@/features/orders/constants"
import { OrderHeader } from "@/features/orders/models/response"
import { OrderDeliverySection } from "../OrderDeliverySection"

export const OrderInfoCard = ({ order }: { order: OrderHeader }) => {
    return (
        <div className="flex flex-col gap-5 px-5 py-3 rounded-2xl border border-default-200 bg-white shadow-sm">
            {/* Hàng 1: 4 Cột thông tin cơ bản */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                        {ORDER_TEXT.labels.CUSTOMER}
                    </p>
                    <p className="text-base font-bold text-default-900">
                        {order.customerId ? ORDER_TEXT.labels.MEMBER : ORDER_TEXT.labels.GUEST}
                    </p>
                </div>

                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                        Thanh toán
                    </p>
                    <p className="text-base font-bold text-default-900">
                        {order.paymentMethod !== undefined
                            ? PaymentMethodLabels[order.paymentMethod]
                            : "-"}
                    </p>
                </div>

                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                        Hình thức nhận
                    </p>
                    <p className="text-base font-bold text-default-900">
                        {OrderTypeLabel[order.type]}
                    </p>
                </div>

                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                        Thời gian
                    </p>
                    <div className="text-base font-bold text-default-900">
                        {order.scheduledTime ? (
                            <span className="text-warning-600 bg-warning-50 px-2 py-0.5 rounded-full text-sm">
                                Hẹn lúc: {order.scheduledTime}
                            </span>
                        ) : (
                            "Nhận ngay"
                        )}
                    </div>
                </div>
            </div>

            {/* Hàng 2: Địa chỉ & Ghi chú (Nếu có) */}
            {(order.address || order.notes) && (
                <div className="flex flex-col gap-4 pt-3 border-t border-default-100">
                    {order.address && (
                        <div className="space-y-1 w-full">
                            <div className="flex items-center gap-2">
                                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                                    Địa chỉ giao hàng
                                </p>
                                {order.type === OrderType.Delivery &&
                                    order.distance !== undefined && (
                                        <span className="text-[10px] font-bold text-default-700 bg-default-100 px-2 py-0.5 rounded-full uppercase tracking-widest">
                                            {order.distance} km
                                        </span>
                                    )}
                            </div>
                            <p className="text-sm font-medium text-default-800">{order.address}</p>
                        </div>
                    )}

                    {order.notes && (
                        <div className="space-y-1 w-full">
                            <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                                {ORDER_TEXT.labels.NOTES}
                            </p>
                            <p className="italic text-sm text-default-700 p-2.5 bg-warning-50 rounded-lg border border-warning-100">
                                {order.notes}
                            </p>
                        </div>
                    )}
                </div>
            )}

            {/* Hàng 3: Khối Điều Phối Giao Hàng (Nằm gọn bên trong Card) */}
            <OrderDeliverySection order={order} />
        </div>
    )
}
