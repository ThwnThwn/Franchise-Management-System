"use client"

import { Chip } from "@heroui/react"
import { LogoStyled } from "@/components"
import {
    ORDER_TEXT,
    OrderStatusColors,
    OrderStatusLabels,
    OrderStatusNote
} from "@/features/orders/constants"
import { useDay } from "@/hooks"
import { CHB, DATE_TIME_VIEW_FORMAT } from "@/constants/constants"
import { Order } from "@/features/orders/models/response"

export function OrderDetailHeader({ order }: { order: Order }) {
    const { formatDateTime } = useDay({ defaultFormat: DATE_TIME_VIEW_FORMAT })

    return (
        <>
            <div className="hidden print:flex flex-col items-center text-center">
                <LogoStyled className="w-12 h-12" />
            </div>

            <div className="flex flex-col gap-1 w-full mb-0">
                <div className="space-y-4">
                    <center>
                        <h2 className="mb-0 text-xl font-black uppercase">
                            {order.store?.name ?? `${CHB} COFFEE SHOP`}
                        </h2>
                        <p className="text-default-400">{order.store?.address ?? ""}</p>
                        <div className="border-b border-black w-full my-4" />
                    </center>
                    <div className="flex justify-between items-start gap-3">
                        <div className="space-y-1">
                            <h1 className="text-3xl md:text-4xl font-black tracking-tighter text-slate-900 leading-none">
                                {ORDER_TEXT.labels.ORDER_DETAIL_TITLE}
                            </h1>
                        </div>

                        {/* <div className="flex flex-col items-end gap-2"> */}
                        <div className="flex items-center gap-1">
                            {/* <p className="hidden print:block text-[10px] font-mono text-default-400 tracking-widest uppercase font-bold">
                                        Trạng thái:
                                    </p> */}

                            <Chip
                                color={OrderStatusColors[order.status]}
                                variant="flat"
                                size="lg"
                                className="font-black border-none px-4"
                            >
                                {OrderStatusLabels[order.status]}
                            </Chip>
                        </div>
                        {/* </div> */}
                    </div>
                    {/* Mã đơn hàng nằm gọn dưới tiêu đề */}
                    <p className="text-sm mb-1 font-mono text-default-400 tracking-widest uppercase">
                        Mã đơn: <span className="text-slate-900 font-bold">#{order.code}</span>
                    </p>
                    {/* Thông tin thời gian */}
                    <p className="text-[10px] font-mono text-default-400 tracking-[0.2em] uppercase">
                        {ORDER_TEXT.labels.ORDER_DATE}:{" "}
                        <span className="text-slate-900 font-bold ml-1">
                            {formatDateTime({ date: order.createdAt })}
                        </span>
                    </p>
                    {/* Dòng Note cụ thể cho từng Status - Điểm nhấn mới */}
                    <div className="bg-default-50 p-4 rounded-2xl border-l-4 border-default-200 print:bg-white print:p-0 print:border-none">
                        <p className="text-sm text-slate-600 font-medium italic">
                            <span className="font-bold not-italic">Ghi chú: </span>
                            {OrderStatusNote[order.status]}
                        </p>
                    </div>
                </div>
            </div>
        </>
    )
}
