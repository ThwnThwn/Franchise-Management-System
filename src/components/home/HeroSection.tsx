"use client"

import React from "react"
import Link from "next/link"
import Image from "next/image"
import { motion } from "framer-motion"

export default function HeroSection() {
    return (
        <section className="relative h-[85vh] w-full overflow-hidden">
            {/* Background Image with Overlay */}
            <div className="absolute inset-0 z-0">
                <div className="absolute inset-0 bg-black/40 z-10" />
                <Image
                    src="https://images.unsplash.com/photo-1497935586351-b67a49e012bf?q=80&w=2073&auto=format&fit=crop"
                    alt="Coffee Shop Ambience"
                    fill
                    className="object-cover"
                    priority
                />
            </div>

            {/* Content Container */}
            <div className="relative z-20 h-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex items-center">
                <div className="max-w-3xl text-white">
                    <motion.div
                        initial={{ opacity: 0, y: 30 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.8 }}
                    >
                        <h1 className="text-5xl md:text-7xl font-bold mb-6 leading-tight">
                            Hãy thử <br />
                            <span className="text-yellow-400">Cà Hong Bạn</span>
                        </h1>
                        <p className="text-xl md:text-2xl mb-8 text-gray-200 font-light leading-relaxed">
                            Khơi nguồn sáng tạo với từng giọt cà phê đậm đà.
                            Không gian hiện đại, trải nghiệm đẳng cấp ngay tại thành phố của bạn.
                        </p>
                        <div className="flex flex-wrap gap-4">
                            <Link href="/menu">
                                <button className="px-8 py-4 bg-yellow-500 hover:bg-yellow-400 text-black font-bold rounded-full transition-all transform hover:scale-105 shadow-lg">
                                    Thực Đơn
                                </button>
                            </Link>
                            <Link href="/stores">
                                <button className="px-8 py-4 bg-transparent border-2 border-white hover:bg-white/10 text-white font-bold rounded-full transition-all">
                                    Tìm Cửa Hàng
                                </button>
                            </Link>
                        </div>
                    </motion.div>
                </div>
            </div>

            {/* Scroll Indicator */}
            <motion.div
                className="absolute bottom-10 left-1/2 transform -translate-x-1/2 z-20 text-white"
                animate={{ y: [0, 10, 0] }}
                transition={{ duration: 1.5, repeat: Infinity }}
            >
                <div className="w-8 h-12 border-2 border-white rounded-full flex justify-center pt-2">
                    <div className="w-1 h-3 bg-white rounded-full" />
                </div>
            </motion.div>
        </section>
    )
}
