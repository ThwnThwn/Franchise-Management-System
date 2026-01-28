"use client";
import type { TopProductsProps } from "@/types/dashboard";
import { Card, CardBody, CardHeader, Progress } from "@heroui/react";
import { motion } from "framer-motion";
import { Coffee } from "lucide-react";
import { formatCurrency } from "@/utils/dashboardMockData";

export const TopProducts = ({ products, maxItems = 8 }: TopProductsProps) => {
    const displayProducts = products.slice(0, maxItems);
    const maxRevenue = Math.max(...displayProducts.map((p) => p.revenue));

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.4 }}
        >
            <Card className="bg-gradient-to-br from-white/80 to-white/60 backdrop-blur-lg border border-primary-50 shadow-lg">
                <CardHeader className="px-6 pt-6 pb-4">
                    <div>
                        <h3 className="text-xl font-bold text-foreground">Sản phẩm bán chạy</h3>
                        <p className="text-sm text-foreground/60 mt-1">
                            Top {displayProducts.length} sản phẩm có doanh thu cao nhất
                        </p>
                    </div>
                </CardHeader>

                <CardBody className="px-6 pb-6">
                    <div className="space-y-4">
                        {displayProducts.map((product, index) => {
                            const percentage = (product.revenue / maxRevenue) * 100;

                            return (
                                <motion.div
                                    key={product.id}
                                    initial={{ opacity: 0, x: -20 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    transition={{ duration: 0.3, delay: index * 0.08 }}
                                    className="group"
                                >
                                    <div className="flex items-start gap-4 p-3 rounded-lg hover:bg-primary-50/30 transition-all duration-300">
                                        {/* Product Icon */}
                                        <div className="flex-shrink-0 w-12 h-12 rounded-lg bg-gradient-to-br from-primary-100 to-primary-200 flex items-center justify-center group-hover:scale-110 transition-transform">
                                            <Coffee className="w-6 h-6 text-primary" />
                                        </div>

                                        {/* Product Info */}
                                        <div className="flex-1 min-w-0">
                                            <div className="flex items-start justify-between gap-2 mb-2">
                                                <div className="flex-1 min-w-0">
                                                    <h4 className="text-sm font-semibold text-foreground truncate">
                                                        {product.name}
                                                    </h4>
                                                    <p className="text-xs text-foreground/60">
                                                        {product.salesCount} đã bán
                                                    </p>
                                                </div>
                                                <div className="text-right flex-shrink-0">
                                                    <p className="text-sm font-bold text-primary">
                                                        {formatCurrency(product.revenue)}
                                                    </p>
                                                </div>
                                            </div>

                                            {/* Progress Bar */}
                                            <Progress
                                                value={percentage}
                                                size="sm"
                                                color="primary"
                                                className="max-w-full"
                                                classNames={{
                                                    indicator: "bg-gradient-to-r from-primary to-secondary",
                                                }}
                                            />
                                        </div>

                                        {/* Rank Badge */}
                                        <div className="flex-shrink-0 w-7 h-7 rounded-full bg-primary text-white flex items-center justify-center text-xs font-bold">
                                            {index + 1}
                                        </div>
                                    </div>
                                </motion.div>
                            );
                        })}
                    </div>
                </CardBody>
            </Card>
        </motion.div>
    );
};
