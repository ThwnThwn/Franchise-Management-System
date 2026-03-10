"use client"

import React, { useMemo, useState } from "react"
import {
    BarChart,
    Bar,
    LineChart,
    Line,
    PieChart,
    Pie,
    Cell,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip as RechartsTooltip,
    ResponsiveContainer
} from "recharts"
import { Card, CardBody, CardHeader, Chip, Select, SelectItem, Spinner } from "@heroui/react"
import {
    BarChart2,
    ShoppingCart,
    CheckCircle,
    XCircle,
    DollarSign,
    Download,
    RefreshCw,
    CalendarDays,
    FileDown
} from "lucide-react"
import dayjs from "dayjs"
import {
    useGetRevenueReport,
    useGetOrderVolume,
    useExportRevenueReport,
    useExportOrderVolume
} from "../../hooks/useOrder"
import { OrderStatus, OrderStatusLabels } from "../../constants"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ButtonStyled } from "@/components"
import { GetRevenueReportQuery, GetOrderVolumeQuery } from "../../models/request"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

// ── Helpers ────────────────────────────────────────────────────────────────────

const formatCurrency = (v: number) =>
    new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(v)

const formatDateLabel = (d: string) => dayjs(d).format("DD/MM")

// ── Preset date ranges ──────────────────────────────────────────────────────────

type RangePreset = "7d" | "14d" | "30d" | "90d" | "custom"

const PRESETS: { key: RangePreset; label: string; days: number }[] = [
    { key: "7d", label: "7 ngày", days: 7 },
    { key: "14d", label: "14 ngày", days: 14 },
    { key: "30d", label: "30 ngày", days: 30 },
    { key: "90d", label: "90 ngày", days: 90 }
]

// ── Status filter options ────────────────────────────────────────────────────────

const ALL_STATUSES = Object.values(OrderStatus).filter(
    (v): v is OrderStatus => typeof v === "number"
)

// ── Chart palette ──────────────────────────────────────────────────────────────

const STATUS_COLOR_MAP: Record<OrderStatus, string> = {
    [OrderStatus.PaymentPending]: "#f59e0b",
    [OrderStatus.ConfirmationPending]: "#3b82f6",
    [OrderStatus.Confirmed]: "#6366f1",
    [OrderStatus.Preparing]: "#8b5cf6",
    [OrderStatus.ReadyForPickup]: "#06b6d4",
    [OrderStatus.Delivering]: "#0ea5e9",
    [OrderStatus.Completed]: "#10b981",
    [OrderStatus.Cancelled]: "#ef4444"
}

const BAR_COLORS = [
    "#6366f1",
    "#8b5cf6",
    "#06b6d4",
    "#10b981",
    "#f59e0b",
    "#ef4444",
    "#3b82f6",
    "#ec4899",
    "#14b8a6",
    "#f97316"
]

// ── KPI Card ───────────────────────────────────────────────────────────────────

function KpiCard({
    icon,
    label,
    value,
    sub,
    color
}: {
    icon: React.ReactNode
    label: string
    value: string | number
    sub?: string
    color: string
}) {
    return (
        <Card className="bg-white border border-slate-100 shadow-sm">
            <CardBody className="p-5">
                <div className="flex items-start justify-between">
                    <div>
                        <p className="text-sm text-slate-500 font-medium">{label}</p>
                        <p className="text-2xl font-bold text-slate-900 mt-1">{value}</p>
                        {sub && <p className="text-xs text-slate-400 mt-0.5">{sub}</p>}
                    </div>
                    <div
                        className="w-11 h-11 rounded-xl flex items-center justify-center shrink-0"
                        style={{ background: color + "20" }}
                    >
                        <span style={{ color }}>{icon}</span>
                    </div>
                </div>
            </CardBody>
        </Card>
    )
}

// ── Empty chart state ──────────────────────────────────────────────────────────

function EmptyChart({ label = "Không có dữ liệu trong kỳ này" }: { label?: string }) {
    return (
        <div className="h-56 flex flex-col items-center justify-center text-slate-400 gap-2">
            <BarChart2 size={32} strokeWidth={1.5} />
            <p className="text-sm">{label}</p>
        </div>
    )
}

function ChartLoader() {
    return (
        <div className="h-56 flex items-center justify-center">
            <Spinner size="lg" />
        </div>
    )
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function OrderAnalyticsPage() {
    const storeIdFromToken = useTokenStore((s) => s.storeId)
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isAdmin = roleSlug === RoleSlug.Admin

    // ── Filters ───────────────────────────────────────────────────────────────
    const [preset, setPreset] = useState<RangePreset>("30d")
    const [customFrom, setCustomFrom] = useState("")
    const [customTo, setCustomTo] = useState("")
    const [statusFilter, setStatusFilter] = useState<string>("all")

    const { revenueFromDate, revenueToDate, volumeFromDate, volumeToDate } = useMemo(() => {
        if (preset === "custom") {
            return {
                revenueFromDate: customFrom || undefined,
                revenueToDate: customTo || undefined,
                volumeFromDate: customFrom || undefined,
                volumeToDate: customTo || undefined
            }
        }
        const days = PRESETS.find((p) => p.key === preset)?.days ?? 30
        const from = dayjs().subtract(days, "day")
        const to = dayjs()
        return {
            revenueFromDate: from.format("YYYY-MM-DD"),
            revenueToDate: to.format("YYYY-MM-DD"),
            volumeFromDate: from.format("YYYY-MM-DD"),
            volumeToDate: to.format("YYYY-MM-DD")
        }
    }, [preset, customFrom, customTo])

    // storeId boundary — Admin sees all, Manager/Staff sees their own store
    const storeId = !isAdmin ? (storeIdFromToken ?? undefined) : undefined

    const revenueQuery: GetRevenueReportQuery = useMemo(
        () => ({ fromDate: revenueFromDate, toDate: revenueToDate, storeId }),
        [revenueFromDate, revenueToDate, storeId]
    )

    const volumeQuery: GetOrderVolumeQuery = useMemo(
        () => ({
            fromDate: volumeFromDate,
            toDate: volumeToDate,
            storeId,
            status: statusFilter !== "all" ? (Number(statusFilter) as OrderStatus) : undefined
        }),
        [volumeFromDate, volumeToDate, storeId, statusFilter]
    )

    // ── Data fetching ─────────────────────────────────────────────────────────
    const {
        data: revenueData,
        isLoading: revenueLoading,
        isRefetching: revenueRefetching,
        refetch: refetchRevenue
    } = useGetRevenueReport(revenueQuery)

    const {
        data: volumeData,
        isLoading: volumeLoading,
        isRefetching: volumeRefetching,
        refetch: refetchVolume
    } = useGetOrderVolume(volumeQuery)

    const { mutate: exportRevenue, isPending: exportingRevenue } = useExportRevenueReport()
    const { mutate: exportVolume, isPending: exportingVolume } = useExportOrderVolume()

    const loading = revenueLoading || revenueRefetching || volumeLoading || volumeRefetching

    // ── Derived data ──────────────────────────────────────────────────────────

    // KPI stats from volume API
    const kpi = useMemo(() => {
        const days = volumeData?.orders ?? []
        const totalOrders = days.reduce((s, d) => s + d.totalOrders, 0)
        const completed = days.reduce((s, d) => s + d.completedOrder, 0)
        const cancelled = days.reduce((s, d) => s + d.canceledOrder, 0)
        const revenue = revenueData?.overallTotalRevenue ?? 0
        return { totalOrders, completed, cancelled, revenue }
    }, [volumeData, revenueData])

    // Orders per day chart (Line)
    const ordersPerDay = useMemo(() => {
        return (volumeData?.orders ?? [])
            .slice()
            .sort((a, b) => a.date.localeCompare(b.date))
            .map((d) => ({
                label: formatDateLabel(d.date),
                total: d.totalOrders,
                completed: d.completedOrder,
                cancelled: d.canceledOrder
            }))
    }, [volumeData])

    // Revenue per product (Bar chart)
    const topProducts = useMemo(() => {
        return (revenueData?.items ?? [])
            .slice()
            .sort((a, b) => b.totalRevenue - a.totalRevenue)
            .slice(0, 10)
            .map((item, i) => ({
                name:
                    item.productName.length > 22
                        ? item.productName.slice(0, 20) + "…"
                        : item.productName,
                fullName: item.productName,
                qty: item.quantitySold,
                revenue: item.totalRevenue,
                color: BAR_COLORS[i % BAR_COLORS.length]
            }))
    }, [revenueData])

    // Status distribution from volume statusCounts (Pie chart)
    const statusDistribution = useMemo(() => {
        const agg: Record<string, number> = {}
        for (const day of volumeData?.orders ?? []) {
            for (const [k, v] of Object.entries(day.statusCounts ?? {})) {
                agg[k] = (agg[k] ?? 0) + v
            }
        }
        return Object.entries(agg)
            .sort(([a], [b]) => Number(a) - Number(b))
            .map(([k, count]) => {
                const status = Number(k) as OrderStatus
                return {
                    name: OrderStatusLabels[status] ?? k,
                    value: count,
                    color: STATUS_COLOR_MAP[status] ?? "#94a3b8"
                }
            })
    }, [volumeData])

    // ── Render helpers ────────────────────────────────────────────────────────

    const handleRefresh = () => {
        refetchRevenue()
        refetchVolume()
    }

    const presetLabel = PRESETS.find((p) => p.key === preset)?.label ?? "khoảng đã chọn"

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            {/* Header */}
            <DashboardHeader
                icon={<BarChart2 className="w-6 h-6 text-white" />}
                title="Phân tích đơn hàng"
                description="Theo dõi tổng quan đơn hàng, doanh thu và hiệu suất hoạt động"
            />

            {/* ── TOOLBAR ──────────────────────────────────────────────────────── */}
            <div className="flex flex-col lg:flex-row gap-3 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                {/* Preset buttons */}
                <div className="flex flex-wrap gap-2 flex-1">
                    <span className="flex items-center gap-1.5 text-sm font-medium text-slate-600 mr-1 self-center">
                        <CalendarDays size={15} />
                        Khoảng thời gian:
                    </span>
                    {PRESETS.map((p) => (
                        <button
                            key={p.key}
                            onClick={() => setPreset(p.key)}
                            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                                preset === p.key
                                    ? "bg-primary text-white shadow-sm"
                                    : "bg-slate-100 hover:bg-slate-200 text-slate-600"
                            }`}
                        >
                            {p.label}
                        </button>
                    ))}
                    <button
                        onClick={() => setPreset("custom")}
                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                            preset === "custom"
                                ? "bg-primary text-white shadow-sm"
                                : "bg-slate-100 hover:bg-slate-200 text-slate-600"
                        }`}
                    >
                        Tùy chỉnh
                    </button>

                    {preset === "custom" && (
                        <div className="flex items-center gap-2 ml-2">
                            <input
                                type="date"
                                value={customFrom}
                                onChange={(e) => setCustomFrom(e.target.value)}
                                className="text-sm border border-slate-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-2 focus:ring-primary/30"
                            />
                            <span className="text-slate-400 text-sm">—</span>
                            <input
                                type="date"
                                value={customTo}
                                onChange={(e) => setCustomTo(e.target.value)}
                                className="text-sm border border-slate-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-2 focus:ring-primary/30"
                            />
                        </div>
                    )}
                </div>

                {/* Status filter + actions */}
                <div className="flex flex-wrap items-center gap-2">
                    <Select
                        label="Trạng thái"
                        size="sm"
                        className="w-44"
                        selectedKeys={[statusFilter]}
                        onSelectionChange={(keys) => setStatusFilter([...keys][0] as string)}
                        variant="bordered"
                    >
                        <>
                            <SelectItem key="all">Tất cả</SelectItem>
                            {ALL_STATUSES.map((s) => (
                                <SelectItem key={String(s)}>{OrderStatusLabels[s]}</SelectItem>
                            ))}
                        </>
                    </Select>

                    <ButtonStyled
                        variant="bordered"
                        size="sm"
                        startContent={<RefreshCw size={15} />}
                        isLoading={loading}
                        onPress={handleRefresh}
                    >
                        Làm mới
                    </ButtonStyled>

                    {/* Export dropdown */}
                    <ButtonStyled
                        color="success"
                        size="sm"
                        startContent={<Download size={15} />}
                        isLoading={exportingVolume}
                        onPress={() => exportVolume(volumeQuery)}
                    >
                        Xuất đơn (CSV)
                    </ButtonStyled>

                    <ButtonStyled
                        variant="bordered"
                        size="sm"
                        startContent={<FileDown size={15} />}
                        isLoading={exportingRevenue}
                        onPress={() => exportRevenue(revenueQuery)}
                    >
                        Xuất doanh thu (CSV)
                    </ButtonStyled>
                </div>
            </div>

            {/* ── KPI CARDS ──────────────────────────────────────────────────────── */}
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <KpiCard
                    icon={<ShoppingCart size={22} />}
                    label="Tổng đơn hàng"
                    value={volumeLoading ? "—" : kpi.totalOrders.toLocaleString()}
                    sub={`Trong ${preset !== "custom" ? presetLabel : "khoảng đã chọn"}`}
                    color="#6366f1"
                />
                <KpiCard
                    icon={<DollarSign size={22} />}
                    label="Doanh thu"
                    value={revenueLoading ? "—" : formatCurrency(kpi.revenue)}
                    sub="Từ API phân tích doanh thu"
                    color="#10b981"
                />
                <KpiCard
                    icon={<CheckCircle size={22} />}
                    label="Đơn hoàn thành"
                    value={volumeLoading ? "—" : kpi.completed.toLocaleString()}
                    sub={
                        kpi.totalOrders > 0
                            ? `${((kpi.completed / kpi.totalOrders) * 100).toFixed(1)}% tổng đơn`
                            : undefined
                    }
                    color="#10b981"
                />
                <KpiCard
                    icon={<XCircle size={22} />}
                    label="Đơn đã huỷ"
                    value={volumeLoading ? "—" : kpi.cancelled.toLocaleString()}
                    sub={
                        kpi.totalOrders > 0
                            ? `${((kpi.cancelled / kpi.totalOrders) * 100).toFixed(1)}% tổng đơn`
                            : undefined
                    }
                    color="#ef4444"
                />
            </div>

            {/* ── CHARTS ROW 1: Orders per Day + Status Distribution ─────────────── */}
            <div className="grid grid-cols-1 xl:grid-cols-3 gap-4 mb-4">
                {/* Line chart – Total / Completed / Cancelled per day */}
                <Card className="xl:col-span-2 bg-white border border-slate-100 shadow-sm">
                    <CardHeader className="px-6 pt-5 pb-2">
                        <div>
                            <h3 className="text-base font-semibold text-slate-900">
                                Số đơn hàng theo ngày
                            </h3>
                            <p className="text-xs text-slate-400 mt-0.5">
                                Tổng đơn, đơn hoàn thành, và đơn huỷ mỗi ngày
                            </p>
                        </div>
                    </CardHeader>
                    <CardBody className="px-4 pb-5 pt-0">
                        {volumeLoading ? (
                            <ChartLoader />
                        ) : ordersPerDay.length === 0 ? (
                            <EmptyChart />
                        ) : (
                            <ResponsiveContainer width="100%" height={260}>
                                <LineChart
                                    data={ordersPerDay}
                                    margin={{ top: 8, right: 16, left: -10, bottom: 0 }}
                                >
                                    <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                                    <XAxis
                                        dataKey="label"
                                        tick={{ fontSize: 11, fill: "#94a3b8" }}
                                        axisLine={false}
                                        tickLine={false}
                                        interval="preserveStartEnd"
                                    />
                                    <YAxis
                                        tick={{ fontSize: 11, fill: "#94a3b8" }}
                                        axisLine={false}
                                        tickLine={false}
                                        allowDecimals={false}
                                    />
                                    <RechartsTooltip
                                        contentStyle={{
                                            borderRadius: "10px",
                                            border: "1px solid #e2e8f0",
                                            fontSize: 12
                                        }}
                                        formatter={(v, name) => {
                                            const labels: Record<string, string> = {
                                                total: "Tổng đơn",
                                                completed: "Hoàn thành",
                                                cancelled: "Huỷ"
                                            }
                                            return [
                                                `${v ?? 0} đơn`,
                                                labels[String(name)] ?? String(name)
                                            ]
                                        }}
                                        labelFormatter={(l) => `Ngày ${l}`}
                                    />
                                    <Line
                                        type="monotone"
                                        dataKey="total"
                                        stroke="#6366f1"
                                        strokeWidth={2.5}
                                        dot={{ r: 3, fill: "#6366f1", strokeWidth: 0 }}
                                        activeDot={{ r: 5 }}
                                    />
                                    <Line
                                        type="monotone"
                                        dataKey="completed"
                                        stroke="#10b981"
                                        strokeWidth={2}
                                        dot={false}
                                    />
                                    <Line
                                        type="monotone"
                                        dataKey="cancelled"
                                        stroke="#ef4444"
                                        strokeWidth={2}
                                        dot={false}
                                        strokeDasharray="4 2"
                                    />
                                </LineChart>
                            </ResponsiveContainer>
                        )}
                        {/* Legend */}
                        {!volumeLoading && ordersPerDay.length > 0 && (
                            <div className="flex justify-center gap-5 mt-1">
                                {[
                                    { color: "#6366f1", label: "Tổng đơn" },
                                    { color: "#10b981", label: "Hoàn thành" },
                                    { color: "#ef4444", label: "Huỷ" }
                                ].map((l) => (
                                    <div
                                        key={l.label}
                                        className="flex items-center gap-1.5 text-xs text-slate-500"
                                    >
                                        <span
                                            className="w-3 h-0.5 rounded-full"
                                            style={{ background: l.color }}
                                        />
                                        {l.label}
                                    </div>
                                ))}
                            </div>
                        )}
                    </CardBody>
                </Card>

                {/* Pie chart – Status distribution */}
                <Card className="bg-white border border-slate-100 shadow-sm">
                    <CardHeader className="px-6 pt-5 pb-2">
                        <div>
                            <h3 className="text-base font-semibold text-slate-900">
                                Phân phối trạng thái
                            </h3>
                            <p className="text-xs text-slate-400 mt-0.5">
                                Tỉ lệ từng trạng thái đơn
                            </p>
                        </div>
                    </CardHeader>
                    <CardBody className="px-4 pb-5 pt-0">
                        {volumeLoading ? (
                            <ChartLoader />
                        ) : statusDistribution.length === 0 ? (
                            <EmptyChart />
                        ) : (
                            <>
                                <ResponsiveContainer width="100%" height={175}>
                                    <PieChart>
                                        <Pie
                                            data={statusDistribution}
                                            cx="50%"
                                            cy="50%"
                                            innerRadius={48}
                                            outerRadius={78}
                                            paddingAngle={2}
                                            dataKey="value"
                                        >
                                            {statusDistribution.map((entry, i) => (
                                                <Cell key={i} fill={entry.color} />
                                            ))}
                                        </Pie>
                                        <RechartsTooltip
                                            contentStyle={{
                                                borderRadius: "10px",
                                                border: "1px solid #e2e8f0",
                                                fontSize: 12
                                            }}
                                            formatter={(v, name) => [`${v ?? 0} đơn`, name]}
                                        />
                                    </PieChart>
                                </ResponsiveContainer>
                                <div className="grid grid-cols-2 gap-x-2 gap-y-1 mt-1">
                                    {statusDistribution.map((s, i) => (
                                        <div key={i} className="flex items-center gap-1.5 truncate">
                                            <span
                                                className="w-2.5 h-2.5 rounded-full shrink-0"
                                                style={{ background: s.color }}
                                            />
                                            <span className="text-xs text-slate-600 truncate">
                                                {s.name}
                                            </span>
                                            <span className="text-xs font-semibold text-slate-900 ml-auto shrink-0">
                                                {s.value}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            </>
                        )}
                    </CardBody>
                </Card>
            </div>

            {/* ── CHARTS ROW 2: Top Products by Revenue + Quantity ──────────────── */}
            <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 mb-4">
                {/* Revenue per product bar */}
                <Card className="bg-white border border-slate-100 shadow-sm">
                    <CardHeader className="px-6 pt-5 pb-2">
                        <div>
                            <h3 className="text-base font-semibold text-slate-900">
                                Top doanh thu theo sản phẩm
                            </h3>
                            <p className="text-xs text-slate-400 mt-0.5">
                                10 sản phẩm có doanh thu cao nhất
                            </p>
                        </div>
                    </CardHeader>
                    <CardBody className="px-4 pb-5 pt-0">
                        {revenueLoading ? (
                            <ChartLoader />
                        ) : topProducts.length === 0 ? (
                            <EmptyChart />
                        ) : (
                            <ResponsiveContainer width="100%" height={230}>
                                <BarChart
                                    data={topProducts}
                                    layout="vertical"
                                    margin={{ top: 4, right: 60, left: 8, bottom: 0 }}
                                >
                                    <CartesianGrid
                                        strokeDasharray="3 3"
                                        stroke="#f1f5f9"
                                        horizontal={false}
                                    />
                                    <XAxis
                                        type="number"
                                        tick={{ fontSize: 10, fill: "#94a3b8" }}
                                        axisLine={false}
                                        tickLine={false}
                                        tickFormatter={(v) =>
                                            v >= 1_000_000
                                                ? `${(v / 1_000_000).toFixed(1)}M`
                                                : v >= 1000
                                                  ? `${(v / 1000).toFixed(0)}K`
                                                  : String(v)
                                        }
                                    />
                                    <YAxis
                                        type="category"
                                        dataKey="name"
                                        width={120}
                                        tick={{ fontSize: 11, fill: "#64748b" }}
                                        axisLine={false}
                                        tickLine={false}
                                    />
                                    <RechartsTooltip
                                        contentStyle={{
                                            borderRadius: "10px",
                                            border: "1px solid #e2e8f0",
                                            fontSize: 12
                                        }}
                                        formatter={(v, _name, item) => [
                                            formatCurrency(Number(v ?? 0)),
                                            (item?.payload as { fullName?: string })?.fullName ?? ""
                                        ]}
                                    />
                                    <Bar dataKey="revenue" radius={[0, 4, 4, 0]} maxBarSize={18}>
                                        {topProducts.map((entry, i) => (
                                            <Cell key={i} fill={entry.color} />
                                        ))}
                                    </Bar>
                                </BarChart>
                            </ResponsiveContainer>
                        )}
                    </CardBody>
                </Card>

                {/* Quantity per product bar */}
                <Card className="bg-white border border-slate-100 shadow-sm">
                    <CardHeader className="px-6 pt-5 pb-2">
                        <div>
                            <h3 className="text-base font-semibold text-slate-900">
                                Top sản phẩm bán chạy
                            </h3>
                            <p className="text-xs text-slate-400 mt-0.5">Theo số lượng đã bán</p>
                        </div>
                    </CardHeader>
                    <CardBody className="px-4 pb-5 pt-0">
                        {revenueLoading ? (
                            <ChartLoader />
                        ) : topProducts.length === 0 ? (
                            <EmptyChart />
                        ) : (
                            <ResponsiveContainer width="100%" height={230}>
                                <BarChart
                                    data={topProducts}
                                    layout="vertical"
                                    margin={{ top: 4, right: 40, left: 8, bottom: 0 }}
                                >
                                    <CartesianGrid
                                        strokeDasharray="3 3"
                                        stroke="#f1f5f9"
                                        horizontal={false}
                                    />
                                    <XAxis
                                        type="number"
                                        tick={{ fontSize: 11, fill: "#94a3b8" }}
                                        axisLine={false}
                                        tickLine={false}
                                        allowDecimals={false}
                                    />
                                    <YAxis
                                        type="category"
                                        dataKey="name"
                                        width={120}
                                        tick={{ fontSize: 11, fill: "#64748b" }}
                                        axisLine={false}
                                        tickLine={false}
                                    />
                                    <RechartsTooltip
                                        contentStyle={{
                                            borderRadius: "10px",
                                            border: "1px solid #e2e8f0",
                                            fontSize: 12
                                        }}
                                        formatter={(v, _name, item) => [
                                            `${v ?? 0} phần`,
                                            (item?.payload as { fullName?: string })?.fullName ?? ""
                                        ]}
                                    />
                                    <Bar
                                        dataKey="qty"
                                        fill="#6366f1"
                                        radius={[0, 4, 4, 0]}
                                        maxBarSize={18}
                                    />
                                </BarChart>
                            </ResponsiveContainer>
                        )}
                    </CardBody>
                </Card>
            </div>

            {/* ── REVENUE TABLE ─────────────────────────────────────────────────── */}
            <Card className="bg-white border border-slate-100 shadow-sm">
                <CardHeader className="px-6 pt-5 pb-3">
                    <div>
                        <h3 className="text-base font-semibold text-slate-900">
                            Chi tiết doanh thu theo sản phẩm
                        </h3>
                        <p className="text-xs text-slate-400 mt-0.5">
                            Tổng doanh thu:{" "}
                            <span className="font-semibold text-emerald-600">
                                {revenueLoading ? "…" : formatCurrency(kpi.revenue)}
                            </span>
                        </p>
                    </div>
                </CardHeader>
                <CardBody className="px-6 pb-5 pt-0">
                    {revenueLoading ? (
                        <div className="flex justify-center py-8">
                            <Spinner size="lg" />
                        </div>
                    ) : topProducts.length === 0 ? (
                        <p className="text-center text-slate-400 py-8 text-sm">Không có dữ liệu</p>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="w-full text-sm">
                                <thead>
                                    <tr className="border-b border-slate-100">
                                        <th className="text-left py-2.5 text-xs font-semibold text-slate-500 uppercase">
                                            Sản phẩm
                                        </th>
                                        <th className="text-right py-2.5 text-xs font-semibold text-slate-500 uppercase">
                                            SL bán
                                        </th>
                                        <th className="text-right py-2.5 text-xs font-semibold text-slate-500 uppercase">
                                            Đơn giá
                                        </th>
                                        <th className="text-right py-2.5 text-xs font-semibold text-slate-500 uppercase">
                                            Doanh thu
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {(revenueData?.items ?? []).map((item, i) => (
                                        <tr
                                            key={i}
                                            className="border-b border-slate-50 hover:bg-slate-50/50 transition-colors"
                                        >
                                            <td className="py-3">
                                                <div>
                                                    <p className="font-medium text-slate-800">
                                                        {item.productName}
                                                    </p>
                                                    <p className="text-xs text-slate-400">
                                                        {item.sku}
                                                    </p>
                                                </div>
                                            </td>
                                            <td className="py-3 text-right font-semibold text-slate-900">
                                                {item.quantitySold.toLocaleString()}
                                            </td>
                                            <td className="py-3 text-right text-slate-600">
                                                {formatCurrency(item.unitPrice)}
                                            </td>
                                            <td className="py-3 text-right">
                                                <Chip
                                                    size="sm"
                                                    className="font-semibold bg-emerald-50 text-emerald-700"
                                                >
                                                    {formatCurrency(item.totalRevenue)}
                                                </Chip>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </CardBody>
            </Card>
        </div>
    )
}
