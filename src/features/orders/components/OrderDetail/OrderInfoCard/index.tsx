import {
    ORDER_TEXT,
    OrderType,
    OrderTypeLabel,
    PaymentMethodLabels
} from "@/features/orders/constants"
import { Order } from "@/features/orders/models/response"

export const OrderInfoCard = ({ order }: { order: Order }) => {
    return (
        <div className="flex flex-wrap justify-between gap-6 p-5 rounded-2xl border border-default-200 bg-white shadow-sm">
            <div className="space-y-1">
                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                    {ORDER_TEXT.labels.CUSTOMER}
                </p>
                <p className="font-bold text-default-900">
                    {order.customerId ? ORDER_TEXT.labels.MEMBER : ORDER_TEXT.labels.GUEST}
                </p>
            </div>
            <div className="space-y-1">
                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                    Thanh toán
                </p>
                <p className="font-bold text-default-900">
                    {order.paymentMethod !== undefined
                        ? PaymentMethodLabels[order.paymentMethod]
                        : "-"}
                </p>
            </div>

            <div className="space-y-1">
                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                    Hình thức nhận
                </p>
                <div className="flex items-center gap-2">
                    <p className="font-bold text-primary">{OrderTypeLabel[order.type]}</p>
                    {order.type === OrderType.Delivery && order.distance !== undefined && (
                        <span className="text-xs text-default-500 bg-default-100 px-2 py-0.5 rounded-full">
                            {order.distance} km
                        </span>
                    )}
                </div>
            </div>

            <div className="space-y-1">
                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                    Thời gian
                </p>
                <p className="font-bold text-default-900">
                    {order.scheduledTime ? (
                        <span className="text-warning-600 bg-warning-50 px-2 py-0.5 rounded-full">
                            Hẹn lúc: {order.scheduledTime}
                        </span>
                    ) : (
                        "Nhận ngay"
                    )}
                </p>
            </div>

            {(order.address || order.notes) && (
                <div className="flex flex-col gap-4 pt-4 border-t border-default-100">
                    {order.address && (
                        <div className="space-y-1 w-full">
                            <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                                Địa chỉ giao hàng
                            </p>
                            <p className="text-sm font-medium text-default-800">{order.address}</p>
                        </div>
                    )}

                    {/* Ghi chú */}
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
        </div>
    )
}
