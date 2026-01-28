"use client";
import type { SalesChartProps } from "@/types/dashboard";
import { Card, CardBody, CardHeader, Button } from "@heroui/react";
import { motion } from "framer-motion";
import { useState } from "react";
import {
    AreaChart,
    Area,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
} from "recharts";
import { formatCurrency } from "@/utils/dashboardMockData";

type PeriodType = "7d" | "30d" | "90d";

export const SalesChart = ({ data, period = "30d" }: SalesChartProps) => {
    const [selectedPeriod, setSelectedPeriod] = useState<PeriodType>(period);

    // lọc ra dữ liệu theo thời gian
    const getFilteredData = () => {
        const days = selectedPeriod === "7d" ? 7 : selectedPeriod === "30d" ? 30 : 90;
        return data.slice(-days);
    };

    const filteredData = getFilteredData();

    // Helper function to format date
    const formatChartDate = (dateString: string) => {
        const date = new Date(dateString);
        const day = date.getDate();
        const month = date.getMonth() + 1;
        return `${day}/${month}`;
    };

    // tooltip
    const CustomTooltip = ({ active, payload }: any) => {
        if (active && payload && payload.length) {
            return (
                <div className="bg-white/95 backdrop-blur-md border border-primary-100 rounded-lg p-3 shadow-lg">
                    <p className="text-sm font-semibold text-foreground mb-2">
                        {formatChartDate(payload[0].payload.date)}
                    </p>
                    <p className="text-xs text-success font-medium">
                        Doanh thu: {formatCurrency(payload[0].value)}
                    </p>
                    <p className="text-xs text-primary font-medium">
                        Đơn hàng: {payload[1].value}
                    </p>
                </div>
            );
        }
        return null;
    };

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.2 }}
        >
            <Card className="bg-gradient-to-br from-white/80 to-white/60 backdrop-blur-lg border border-primary-50 shadow-lg">
                <CardHeader className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 px-6 pt-6 pb-4">
                    <div>
                        <h3 className="text-xl font-bold text-foreground">Xu hướng bán hàng</h3>
                        <p className="text-sm text-foreground/60 mt-1">
                            Doanh thu và số lượng đơn hàng theo thời gian
                        </p>
                    </div>

                    <div className="flex gap-2">
                        {(["7d", "30d", "90d"] as PeriodType[]).map((p) => (
                            <Button
                                key={p}
                                size="sm"
                                variant={selectedPeriod === p ? "solid" : "flat"}
                                color={selectedPeriod === p ? "primary" : "default"}
                                onPress={() => setSelectedPeriod(p)}
                                className="min-w-[60px]"
                            >
                                {p === "7d" ? "7 ngày" : p === "30d" ? "30 ngày" : "90 ngày"}
                            </Button>
                        ))}
                    </div>
                </CardHeader>

                <CardBody className="px-6 pb-6">
                    <ResponsiveContainer width="100%" height={300}>
                        <AreaChart data={filteredData}>
                            <defs>
                                <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="5%" stopColor="#5D4037" stopOpacity={0.3} />
                                    <stop offset="95%" stopColor="#5D4037" stopOpacity={0} />
                                </linearGradient>
                                <linearGradient id="colorOrders" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="5%" stopColor="#8D6E63" stopOpacity={0.3} />
                                    <stop offset="95%" stopColor="#8D6E63" stopOpacity={0} />
                                </linearGradient>
                            </defs>
                            <CartesianGrid strokeDasharray="3 3" stroke="#D7CCC8" opacity={0.3} />
                            <XAxis
                                dataKey="date"
                                tickFormatter={(value) => formatChartDate(value)}
                                stroke="#8D6E63"
                                style={{ fontSize: "12px" }}
                            />
                            <YAxis
                                yAxisId="left"
                                tickFormatter={(value) => `${(value / 1000000).toFixed(0)}M`}
                                stroke="#5D4037"
                                style={{ fontSize: "12px" }}
                            />
                            <YAxis
                                yAxisId="right"
                                orientation="right"
                                stroke="#8D6E63"
                                style={{ fontSize: "12px" }}
                            />
                            <Tooltip content={<CustomTooltip />} />
                            <Area
                                yAxisId="left"
                                type="monotone"
                                dataKey="revenue"
                                stroke="#5D4037"
                                strokeWidth={2}
                                fill="url(#colorRevenue)"
                            />
                            <Area
                                yAxisId="right"
                                type="monotone"
                                dataKey="orders"
                                stroke="#8D6E63"
                                strokeWidth={2}
                                fill="url(#colorOrders)"
                            />
                        </AreaChart>
                    </ResponsiveContainer>

                    <div className="flex justify-center gap-6 mt-4">
                        <div className="flex items-center gap-2">
                            <div className="w-3 h-3 rounded-full bg-primary"></div>
                            <span className="text-xs text-foreground/70">Doanh thu</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <div className="w-3 h-3 rounded-full bg-secondary"></div>
                            <span className="text-xs text-foreground/70">Đơn hàng</span>
                        </div>
                    </div>
                </CardBody>
            </Card>
        </motion.div>
    );
};
