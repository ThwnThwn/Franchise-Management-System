import React from "react"

export interface DashboardStats {
    totalRevenue: number
    totalOrders: number
    totalCustomers: number
    activeProducts: number
    revenueChange: number
    ordersChange: number
    customersChange: number
    productsChange: number
}

export interface SalesData {
    date: string
    revenue: number
    orders: number
}
// order status ở đây
export type OrderStatus = "pending" | "completed" | "cancelled"
export interface Order {
    id: string
    customerName: string
    products: string[]
    quantity: number
    totalAmount: number
    status: OrderStatus
    createdAt: string
}
export interface Product {
    id: string
    name: string
    category: string
    salesCount: number
    revenue: number
    imageUrl?: string
}

export interface StatCardProps {
    title: string
    value: string | number
    change: number // percentage
    icon: React.ReactNode
    trend: "up" | "down"
}

/**
 * Props for SalesChart component
 */
export interface SalesChartProps {
    data: SalesData[]
    period?: "7d" | "30d" | "90d"
    onPeriodChange?: (period: "7d" | "30d" | "90d") => void
}

/**
 * Props for RecentOrders component
 */
export interface RecentOrdersProps {
    orders: Order[]
    maxItems?: number
}

/**
 * Props for TopProducts component
 */
export interface TopProductsProps {
    products: Product[]
    maxItems?: number
}

/**
 * Complete dashboard data structure
 */
export interface DashboardData {
    stats: DashboardStats
    salesData: SalesData[]
    recentOrders: Order[]
    topProducts: Product[]
}
