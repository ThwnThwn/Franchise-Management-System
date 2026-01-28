"use client";

import type { RecentOrdersProps } from "@/types/dashboard";
import { Card, CardBody, CardHeader, Chip } from "@heroui/react";
import { motion } from "framer-motion";
import { formatCurrency, formatDate } from "@/utils/dashboardMockData";
import type { OrderStatus } from "@/types/dashboard";

const getStatusConfig = (status: OrderStatus) => {
    switch (status) {
        case "completed":
            return { color: "success" as const, label: "Hoàn thành" };
        case "pending":
            return { color: "warning" as const, label: "Đang xử lý" };
        case "cancelled":
            return { color: "danger" as const, label: "Đã hủy" };
    }
};

export const RecentOrders = ({ orders, maxItems = 10 }: RecentOrdersProps) => {
    const displayOrders = orders.slice(0, maxItems);

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.3 }}
        >
            <Card className="bg-gradient-to-br from-white/80 to-white/60 backdrop-blur-lg border border-primary-50 shadow-lg">
                <CardHeader className="px-6 pt-6 pb-4">
                    <div>
                        <h3 className="text-xl font-bold text-foreground">Đơn hàng gần đây</h3>
                        <p className="text-sm text-foreground/60 mt-1">
                            {orders.length} đơn hàng mới nhất
                        </p>
                    </div>
                </CardHeader>

                <CardBody className="px-6 pb-6">
                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead>
                                <tr className="border-b border-primary-100">
                                    <th className="text-left py-3 px-2 text-xs font-semibold text-foreground/70 uppercase">
                                        Mã đơn
                                    </th>
                                    <th className="text-left py-3 px-2 text-xs font-semibold text-foreground/70 uppercase">
                                        Khách hàng
                                    </th>
                                    <th className="text-left py-3 px-2 text-xs font-semibold text-foreground/70 uppercase hidden md:table-cell">
                                        Sản phẩm
                                    </th>
                                    <th className="text-right py-3 px-2 text-xs font-semibold text-foreground/70 uppercase">
                                        Số lượng
                                    </th>
                                    <th className="text-right py-3 px-2 text-xs font-semibold text-foreground/70 uppercase">
                                        Tổng tiền
                                    </th>
                                    <th className="text-center py-3 px-2 text-xs font-semibold text-foreground/70 uppercase">
                                        Trạng thái
                                    </th>
                                    <th className="text-left py-3 px-2 text-xs font-semibold text-foreground/70 uppercase hidden lg:table-cell">
                                        Thời gian
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                {displayOrders.map((order, index) => {
                                    const statusConfig = getStatusConfig(order.status);
                                    return (
                                        <motion.tr
                                            key={order.id}
                                            initial={{ opacity: 0, x: -20 }}
                                            animate={{ opacity: 1, x: 0 }}
                                            transition={{ duration: 0.3, delay: index * 0.05 }}
                                            className="border-b border-primary-50 hover:bg-primary-50/30 transition-colors"
                                        >
                                            <td className="py-3 px-2">
                                                <span className="text-sm font-semibold text-primary">
                                                    {order.id}
                                                </span>
                                            </td>
                                            <td className="py-3 px-2">
                                                <span className="text-sm text-foreground">
                                                    {order.customerName}
                                                </span>
                                            </td>
                                            <td className="py-3 px-2 hidden md:table-cell">
                                                <div className="text-xs text-foreground/70 max-w-[200px]">
                                                    {order.products.join(", ")}
                                                </div>
                                            </td>
                                            <td className="py-3 px-2 text-right">
                                                <span className="text-sm font-medium text-foreground">
                                                    {order.quantity}
                                                </span>
                                            </td>
                                            <td className="py-3 px-2 text-right">
                                                <span className="text-sm font-semibold text-foreground">
                                                    {formatCurrency(order.totalAmount)}
                                                </span>
                                            </td>
                                            <td className="py-3 px-2 text-center">
                                                <Chip
                                                    size="sm"
                                                    color={statusConfig.color}
                                                    variant="flat"
                                                    className="text-xs font-medium"
                                                >
                                                    {statusConfig.label}
                                                </Chip>
                                            </td>
                                            <td className="py-3 px-2 hidden lg:table-cell">
                                                <span className="text-xs text-foreground/60">
                                                    {formatDate(order.createdAt)}
                                                </span>
                                            </td>
                                        </motion.tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </CardBody>
            </Card>
        </motion.div>
    );
};
