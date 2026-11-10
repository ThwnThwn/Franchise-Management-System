/**
 * Dashboard Mock Data Generator
 * Provides sample data for dashboard demonstration
 */

import type {
    DashboardData,
    DashboardStats,
    Order,
    OrderStatus,
    Product,
    SalesData
} from "@/types/dashboard"

const randomBetween = (min: number, max: number): number => {
    return Math.floor(Math.random() * (max - min + 1)) + min
}

export const formatCurrency = (amount: number): string => {
    return amount.toLocaleString("vi-VN") + "₫"
}

export const formatDate = (dateString: string): string => {
    const date = new Date(dateString)
    const day = String(date.getDate()).padStart(2, "0")
    const month = String(date.getMonth() + 1).padStart(2, "0")
    const year = date.getFullYear()
    const hours = String(date.getHours()).padStart(2, "0")
    const minutes = String(date.getMinutes()).padStart(2, "0")
    return `${day}/${month}/${year} ${hours}:${minutes}`
}

export const generateDashboardStats = (): DashboardStats => {
    return {
        totalRevenue: 125480000, // 125.48M VND
        totalOrders: 1247,
        totalCustomers: 856,
        activeProducts: 42,
        revenueChange: 12.5,
        ordersChange: 8.3,
        customersChange: 15.2,
        productsChange: 5.0
    }
}

export const generateSalesData = (days: number = 30): SalesData[] => {
    const data: SalesData[] = []
    const today = new Date()

    for (let i = days - 1; i >= 0; i--) {
        const date = new Date(today)
        date.setDate(date.getDate() - i)
        const isWeekend = date.getDay() === 0 || date.getDay() === 6
        const baseOrders = isWeekend ? 50 : 35
        const baseRevenue = isWeekend ? 5000000 : 3500000

        data.push({
            date: date.toISOString().split("T")[0],
            orders: randomBetween(baseOrders - 10, baseOrders + 15),
            revenue: randomBetween(baseRevenue - 1000000, baseRevenue + 2000000)
        })
    }

    return data
}

const COFFEE_PRODUCTS = [
    "Cà phê Espresso",
    "Cà phê Latte",
    "Cà phê Cappuccino",
    "Cà phê Americano",
    "Cà phê Mocha",
    "Cà phê Macchiato",
    "Cà phê Sữa Đá",
    "Cà phê Đen Đá",
    "Bạc Xỉu",
    "Cà phê Cold Brew",
    "Cà phê Caramel",
    "Cà phê Hazelnut"
]

const CUSTOMER_NAMES = [
    "Nguyễn Văn A",
    "Trần Thị B",
    "Lê Hoàng C",
    "Phạm Thị D",
    "Hoàng Văn E",
    "Vũ Thị F",
    "Đỗ Văn G",
    "Bùi Thị H",
    "Ngô Văn H",
    "Đinh Thị I",
    "Mai Văn J",
    "Trương Thị K"
]

const generateOrderStatus = (): OrderStatus => {
    const rand = Math.random()
    if (rand < 0.7) return "completed"
    if (rand < 0.9) return "pending"
    return "cancelled"
}

export const generateRecentOrders = (count: number = 15): Order[] => {
    const orders: Order[] = []
    const now = new Date()

    for (let i = 0; i < count; i++) {
        const orderDate = new Date(now)
        orderDate.setHours(orderDate.getHours() - randomBetween(1, 48))

        const numProducts = randomBetween(1, 3)
        const products: string[] = []

        for (let j = 0; j < numProducts; j++) {
            const product = COFFEE_PRODUCTS[randomBetween(0, COFFEE_PRODUCTS.length - 1)]
            if (!products.includes(product)) {
                products.push(product)
            }
        }

        const quantity = randomBetween(1, 5)
        const pricePerItem = randomBetween(35000, 95000)

        orders.push({
            id: `ORD-${String(1000 + i).padStart(4, "0")}`,
            customerName: CUSTOMER_NAMES[randomBetween(0, CUSTOMER_NAMES.length - 1)],
            products,
            quantity,
            totalAmount: quantity * pricePerItem,
            status: generateOrderStatus(),
            createdAt: orderDate.toISOString()
        })
    }

    return orders.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
}

export const generateTopProducts = (count: number = 8): Product[] => {
    const products: Product[] = []
    const usedProducts: string[] = []

    for (let i = 0; i < count; i++) {
        let productName: string
        do {
            productName = COFFEE_PRODUCTS[randomBetween(0, COFFEE_PRODUCTS.length - 1)]
        } while (usedProducts.includes(productName))

        usedProducts.push(productName)

        const salesCount = randomBetween(50, 250)
        const avgPrice = randomBetween(45000, 85000)

        products.push({
            id: `PROD-${String(100 + i).padStart(3, "0")}`,
            name: productName,
            category: "Cà phê",
            salesCount,
            revenue: salesCount * avgPrice,
            imageUrl: undefined
        })
    }

    return products.sort((a, b) => b.revenue - a.revenue)
}

export const generateDashboardData = (): DashboardData => {
    return {
        stats: generateDashboardStats(),
        salesData: generateSalesData(30),
        recentOrders: generateRecentOrders(15),
        topProducts: generateTopProducts(8)
    }
}

let cachedDashboardData: DashboardData | null = null
// lấy mockdata
export const getMockDashboardData = (): DashboardData => {
    if (!cachedDashboardData) {
        cachedDashboardData = generateDashboardData()
    }
    return cachedDashboardData
}

// refest lại mockdata
export const refreshMockData = (): DashboardData => {
    cachedDashboardData = generateDashboardData()
    return cachedDashboardData
}
