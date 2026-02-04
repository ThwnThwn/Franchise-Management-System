"use client"
import { Card, CardBody, CardFooter, Button, Image } from "@heroui/react"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { Product, ProductSize } from "../../models/products/respone"
import { motion } from "framer-motion"

// Simple SVG Icons for Cup Sizes
const CupIcon = ({ size, opacity = 1 }: { size: "s" | "m" | "l"; opacity?: number }) => {
    const scale = size === "s" ? 0.7 : size === "m" ? 0.85 : 1
    return (
        <svg
            width="24"
            height="24"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            className="text-white"
            style={{ transform: `scale(${scale})`, opacity }}
        >
            <path d="M17 21v-8a2 2 0 0 0-2-2H9a2 2 0 0 0-2 2v8" />
            <path d="M19 10a2 2 0 0 0-2-2h-3.4" />
        </svg>
    )
}

export function ProductCard({ getProductListItemDto }: { getProductListItemDto: Product }) {
    // Find base price (usually the smallest size or first variant)
    const basePrice =
        getProductListItemDto.sizes.length > 0
            ? Math.min(...getProductListItemDto.sizes.map((s) => s.basePrice))
            : 0

    return (
        <div className="pt-12 mb-4 w-full max-w-[280px] mx-auto group">
            <Card
                className="overflow-visible bg-gradient-to-b from-[#8D6E63] to-[#4E342E] rounded-[2.5rem] shadow-xl border-none min-h-[320px]"
                shadow="sm"
                isPressable
            >
                <CardBody className="overflow-visible p-0 items-center -mt-16 z-20">
                    <motion.div
                        whileHover={{ y: -10, scale: 1.05 }}
                        transition={{ duration: 0.3 }}
                        className="relative"
                    >
                        {/* Shadow under the cup */}
                        <div className="absolute -bottom-4 left-1/2 -translate-x-1/2 w-3/4 h-8 bg-black/30 blur-xl rounded-full" />

                        <Image
                            alt={getProductListItemDto.name}
                            className="object-contain h-56 w-56 drop-shadow-2xl z-20"
                            src={getProductListItemDto.imageUrl}
                            removeWrapper
                        />
                    </motion.div>
                </CardBody>

                <CardFooter className="flex flex-col justify-end items-start px-6 pb-6 pt-2 h-full z-10 w-full gap-2">
                    {/* Name */}
                    <div className="w-full">
                        <h3 className="text-white font-black text-2xl uppercase leading-none tracking-tight line-clamp-2 min-h-[3rem]">
                            {getProductListItemDto.name}
                        </h3>
                    </div>

                    {/* Price and Best Sales Tag */}
                    <div className="w-full flex justify-between items-center mt-2">
                        <span className="text-[#A5D6A7] font-bold text-xl">
                            {formatCurrencyWithSymbol(basePrice)}
                        </span>

                        {/* "Best Sales" Tag - Just static for visual per requirement */}
                        <span className="text-[10px] text-white/60 font-medium uppercase tracking-wider">
                            Best Sales
                        </span>
                    </div>

                    {/* Size Icons */}
                    <div className="w-full flex justify-between items-center mt-4 border-t border-white/10 pt-4">
                        <div className="flex flex-col items-center gap-1 cursor-pointer hover:bg-white/10 p-1 rounded-lg transition-colors">
                            <CupIcon size="s" opacity={0.5} />
                            <span className="text-[9px] text-white/50 font-medium">Tall</span>
                        </div>
                        <div className="flex flex-col items-center gap-1 cursor-pointer hover:bg-white/10 p-1 rounded-lg transition-colors">
                            <CupIcon size="m" opacity={0.5} />
                            <span className="text-[9px] text-white/50 font-medium">Grande</span>
                        </div>
                        <div className="flex flex-col items-center gap-1 cursor-pointer bg-white/20 p-1.5 rounded-xl border border-white/30 transition-all shadow-lg backdrop-blur-sm">
                            {/* Highlight one size as 'selected' visual */}
                            <CupIcon size="l" />
                            <span className="text-[9px] text-white font-bold">Venti</span>
                        </div>
                        <div className="flex flex-col items-center gap-1 cursor-pointer hover:bg-white/10 p-1 rounded-lg transition-colors">
                            <div className="w-5 h-5 border border-dashed border-white/50 rounded-md" />
                            <span className="text-[9px] text-white/50 font-medium">Custom</span>
                        </div>
                    </div>
                </CardFooter>
            </Card>
        </div>
    )
}
