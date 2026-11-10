"use client"

import React from "react"
import type { StatCardProps } from "@/types/dashboard"
import { Card, CardBody } from "@heroui/react"
import { TrendingUp, TrendingDown } from "lucide-react"
import { motion } from "framer-motion"

export const StatCard = ({ title, value, change, icon, trend }: StatCardProps) => {
    const isPositive = trend === "up"

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
            whileHover={{ y: -4 }}
            className="h-full"
        >
            <Card className="h-full bg-gradient-to-br from-white/80 to-white/60 backdrop-blur-lg border border-primary-50 shadow-lg hover:shadow-xl transition-all duration-300">
                <CardBody className="p-6">
                    <div className="flex items-start justify-between mb-4">
                        <div className="p-3 rounded-xl bg-gradient-to-br from-primary-50 to-primary-100 text-primary">
                            {icon}
                        </div>
                        <div
                            className={`flex items-center gap-1 px-2 py-1 rounded-full text-xs font-semibold ${
                                isPositive
                                    ? "bg-success/10 text-success"
                                    : "bg-danger/10 text-danger"
                            }`}
                        >
                            {isPositive ? (
                                <TrendingUp className="w-3 h-3" />
                            ) : (
                                <TrendingDown className="w-3 h-3" />
                            )}
                            {Math.abs(change)}%
                        </div>
                    </div>

                    <div className="space-y-1">
                        <p className="text-sm text-foreground/60 font-medium">{title}</p>
                        <p className="text-2xl lg:text-3xl font-bold text-foreground">
                            {typeof value === "number" ? value.toLocaleString("vi-VN") : value}
                        </p>
                    </div>
                </CardBody>
            </Card>
        </motion.div>
    )
}
