"use client"

import React from "react"
import { StatCard, SalesChart, RecentOrders, TopProducts } from "@/features/dashboard"
import { getMockDashboardData, formatCurrency } from "@/utils/dashboardMockData"
import { DollarSign, ShoppingCart, Users, Package, Calendar } from "lucide-react"
import { Button } from "@heroui/react"

export default function DashboardPage() {
    // lấy mockdata test
    const dashboardData = getMockDashboardData()
    const { stats, salesData, recentOrders, topProducts } = dashboardData
    // return null
    return (
        <div className="min-h-screen p-4">
            {/* Page Header */}
            <div className="max-w-7xl mx-auto mb-8">
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                    <div>
                        <h1 className="text-3xl lg:text-4xl font-bold text-foreground mb-2">
                            Dashboard Quản Lý Cà Phê
                        </h1>
                        <p className="text-foreground/60 flex items-center gap-2">
                            <Calendar className="w-4 h-4" />
                            <span>
                                {new Date().toLocaleDateString("vi-VN", {
                                    weekday: "long",
                                    year: "numeric",
                                    month: "long",
                                    day: "numeric"
                                })}
                            </span>
                        </p>
                    </div>

                    <Button color="primary" variant="solid" size="md" className="font-semibold">
                        Xuất báo cáo
                    </Button>
                </div>
            </div>

            <div className="max-w-7xl mx-auto space-y-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
                    <StatCard
                        title="Tổng doanh thu"
                        value={formatCurrency(stats.totalRevenue)}
                        change={stats.revenueChange}
                        icon={<DollarSign className="w-6 h-6" />}
                        trend="up"
                    />
                    <StatCard
                        title="Tổng đơn hàng"
                        value={stats.totalOrders}
                        change={stats.ordersChange}
                        icon={<ShoppingCart className="w-6 h-6" />}
                        trend="up"
                    />
                    <StatCard
                        title="Khách hàng"
                        value={stats.totalCustomers}
                        change={stats.customersChange}
                        icon={<Users className="w-6 h-6" />}
                        trend="up"
                    />
                    <StatCard
                        title="Sản phẩm"
                        value={stats.activeProducts}
                        change={stats.productsChange}
                        icon={<Package className="w-6 h-6" />}
                        trend="up"
                    />
                </div>
                <SalesChart data={salesData} />
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <RecentOrders orders={recentOrders} maxItems={8} />
                    <TopProducts products={topProducts} maxItems={8} />
                </div>
            </div>
        </div>
    )
}
