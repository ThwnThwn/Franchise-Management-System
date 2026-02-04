"use client"

import React from "react"
import Link from "next/link"
import { motion } from "framer-motion"
import { Button, Image } from "@heroui/react"

export default function HeroSection() {
    return (
        <section className="relative w-full min-h-[85vh] bg-gradient-to-r from-[#5D4037] to-[#3E2723] overflow-hidden flex items-center">
            {/* Background Pattern/Overlay */}
            <div className="absolute inset-0 z-0 opacity-10 pointer-events-none">
                <motion.div
                    animate={{
                        scale: [1, 1.2, 1],
                        rotate: [0, 90, 0],
                    }}
                    transition={{ duration: 20, repeat: Infinity, ease: "linear" }}
                    className="absolute top-0 right-0 w-[800px] h-[800px] bg-white rounded-full blur-[100px] -translate-y-1/2 translate-x-1/2"
                />
                <motion.div
                    animate={{
                        scale: [1, 1.1, 1],
                        x: [0, 50, 0],
                    }}
                    transition={{ duration: 15, repeat: Infinity, ease: "easeInOut" }}
                    className="absolute bottom-0 left-0 w-[600px] h-[600px] bg-black rounded-full blur-[80px] translate-y-1/2 -translate-x-1/2"
                />
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 w-full z-10 relative">
                <div className="flex flex-col md:flex-row items-center justify-between gap-12">

                    {/* Left: Floating Coffee Cup Image */}
                    <motion.div
                        className="w-full md:w-1/2 flex justify-center md:justify-start"
                        initial={{ opacity: 0, x: -100, rotate: -10 }}
                        animate={{ opacity: 1, x: 0, rotate: 0 }}
                        transition={{ duration: 1, type: "spring", bounce: 0.4 }}
                    >
                        <div className="relative w-[500px] h-[500px] md:w-[650px] md:h-[650px]">
                            {/* Glow Effect */}
                            <div className="absolute inset-0 bg-[#FFB74D]/20 blur-3xl rounded-full scale-75 animate-pulse" />

                            <motion.div
                                animate={{ y: [-15, 15, -15] }}
                                transition={{ repeat: Infinity, duration: 6, ease: "easeInOut" }}
                                className="rotate-[8deg]"
                            >
                                <Image
                                    src="/images/coffe_home.png"
                                    alt="Premium Coffee Cup"
                                    className="w-full h-full object-contain drop-shadow-2xl"
                                    removeWrapper
                                />
                            </motion.div>
                        </div>
                    </motion.div>

                    {/* Right: Content */}
                    <motion.div
                        className="w-full md:w-1/2 text-center md:text-right"
                        initial="hidden"
                        animate="visible"
                        variants={{
                            hidden: { opacity: 0, x: 50 },
                            visible: {
                                opacity: 1,
                                x: 0,
                                transition: {
                                    staggerChildren: 0.2,
                                    delayChildren: 0.3
                                }
                            }
                        }}
                    >
                        <motion.h2
                            variants={{ hidden: { opacity: 0, y: 20 }, visible: { opacity: 1, y: 0 } }}
                            className="text-[#A1887F] font-bold tracking-[0.2em] text-sm md:text-base mb-4 uppercase"
                        >
                            Premium Quality
                        </motion.h2>

                        <motion.h1
                            variants={{ hidden: { opacity: 0, scale: 0.9 }, visible: { opacity: 1, scale: 1 } }}
                            className="text-5xl md:text-7xl lg:text-8xl font-black text-white italic leading-[0.9] mb-6 drop-shadow-lg"
                        >
                            CÀ <br />
                            <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#FFB74D] to-[#FFCC80]">
                                HONG BẠN
                            </span>
                        </motion.h1>

                        <motion.p
                            variants={{ hidden: { opacity: 0, x: 20 }, visible: { opacity: 1, x: 0 } }}
                            className="text-[#D7CCC8] text-lg md:text-xl mb-8 max-w-lg ml-auto leading-relaxed font-light"
                        >
                            Hương vị đánh thức mọi giác quan. Một sự pha trộn hoàn hảo của hạt Robusta mạnh mẽ và Arabica thanh tao.
                        </motion.p>

                        <motion.div
                            variants={{ hidden: { opacity: 0, y: 20 }, visible: { opacity: 1, y: 0 } }}
                            className="flex flex-col md:flex-row justify-end items-center gap-6"
                        >
                            <h3 className="text-4xl font-bold text-[#FFB74D]">35.000đ</h3>
                            <Link href="/menu">
                                <Button
                                    size="lg"
                                    radius="full"
                                    className="bg-[#FFB74D] text-[#3E2723] font-bold px-10 shadow-xl hover:scale-105 transition-transform text-lg"
                                >
                                    Mua ngay
                                </Button>
                            </Link>
                        </motion.div>
                    </motion.div>
                </div>
            </div>

            {/* Scroll Indicator */}
            <motion.div
                className="absolute bottom-8 left-1/2 -translate-x-1/2 text-white/50"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1, y: [0, 8, 0] }}
                transition={{
                    opacity: { delay: 1.5, duration: 1 },
                    y: { repeat: Infinity, duration: 1.5 }
                }}
            >
                <div className="w-6 h-10 border-2 border-white/30 rounded-full flex justify-center pt-2">
                    <div className="w-1 h-2 bg-white/50 rounded-full" />
                </div>
            </motion.div>
        </section>
    )
}
