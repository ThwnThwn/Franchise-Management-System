import { ORDER_TEXT, PaymentMethodLabels } from "@/features/orders/constants"
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
                    Phương thức thanh toán
                </p>
                <p className="font-bold text-default-900">
                    {order.paymentMethod !== undefined
                        ? PaymentMethodLabels[order.paymentMethod]
                        : "-"}
                </p>
            </div>
            {order.notes && (
                <div className="space-y-1 min-w-full md:min-w-50">
                    <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                        {ORDER_TEXT.labels.NOTES}
                    </p>
                    <p className="italic text-sm text-default-700 p-2 bg-warning-50 rounded-lg border border-warning-100">
                        {order.notes}
                    </p>
                </div>
            )}
        </div>
    )
}
