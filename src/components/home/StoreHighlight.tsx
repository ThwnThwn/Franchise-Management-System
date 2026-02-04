"use client"

import React from "react"
import Link from "next/link"
import Image from "next/image"
import { motion } from "framer-motion"

export default function StoreHighlight() {
    return (
        <section className="py-20 bg-gradient-to-r from-[#5D4037] to-[#3E2723] relative overflow-hidden">
            {/* Background Pattern/Overlay */}
            <div className="absolute inset-0 z-0 opacity-10">
                <div className="absolute top-0 left-0 w-[800px] h-[800px] bg-white rounded-full blur-[100px] -translate-y-1/2 -translate-x-1/2" />
                <div className="absolute bottom-0 right-0 w-[600px] h-[600px] bg-black rounded-full blur-[80px] translate-y-1/2 translate-x-1/2" />
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">
                    <motion.div
                        initial={{ opacity: 0, x: -50 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.8 }}
                    >
                        <h2 className="text-4xl md:text-5xl font-bold text-white mb-6">
                            Không Gian <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#FFB74D] to-[#FFCC80]">Đẳng Cấp</span>
                        </h2>
                        <p className="text-[#D7CCC8] text-lg mb-8 leading-relaxed">
                            Mỗi cửa hàng Cà Hong Bạn đều được thiết kế với không gian ấm cúng,
                            hiện đại và đầy cảm hứng. Là nơi lý tưởng để bạn làm việc,
                            gặp gỡ bạn bè hay đơn giản là tận hưởng một tách cà phê ngon.
                        </p>

                        <div className="space-y-4 mb-8">
                            <div className="flex items-center text-white">
                                <div className="w-12 h-12 bg-[#FFB74D]/20 backdrop-blur-sm rounded-full flex items-center justify-center mr-4 text-[#FFB74D] border border-[#FFB74D]/30">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                                    </svg>
                                </div>
                                <span className="text-lg">Tìm cửa hàng gần nhất với GPS</span>
                            </div>
                            <div className="flex items-center text-white">
                                <div className="w-12 h-12 bg-[#FFB74D]/20 backdrop-blur-sm rounded-full flex items-center justify-center mr-4 text-[#FFB74D] border border-[#FFB74D]/30">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                </div>
                                <span className="text-lg">Mở cửa 24/7 tại các chi nhánh trung tâm</span>
                            </div>
                        </div>

                        <Link href="/stores">
                            <button className="px-8 py-4 bg-[#FFB74D] hover:bg-[#FFCC80] text-[#3E2723] font-bold rounded-full transition-all shadow-xl hover:scale-105 hover:shadow-[#FFB74D]/50">
                                Tìm Cửa Hàng Ngay
                            </button>
                        </Link>
                    </motion.div>

                    <motion.div
                        initial={{ opacity: 0, scale: 0.9 }}
                        whileInView={{ opacity: 1, scale: 1 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.8 }}
                        className="relative h-[500px] w-full rounded-3xl overflow-hidden shadow-2xl border-4 border-[#8D6E63]/30"
                    >
                        <Image
                            src="https://images.unsplash.com/photo-1554118811-1e0d58224f24?q=80&w=1000&auto=format&fit=crop"
                            alt="Store Interior"
                            fill
                            className="object-cover"
                        />

                        {/* Overlay gradient */}
                        <div className="absolute inset-0 bg-gradient-to-t from-[#3E2723]/60 via-transparent to-transparent" />

                        {/* Interactive Float Card */}
                        <motion.div
                            className="absolute bottom-6 left-6 right-6 bg-white/95 backdrop-blur-md p-6 rounded-2xl shadow-xl border border-[#FFB74D]/20"
                            initial={{ y: 50, opacity: 0 }}
                            whileInView={{ y: 0, opacity: 1 }}
                            transition={{ delay: 0.3, duration: 0.6 }}
                        >
                            <div className="flex justify-between items-center">
                                <div>
                                    <h4 className="font-bold text-[#3E2723] text-lg">Chi Nhánh Mới Nhất</h4>
                                    <p className="text-sm text-[#8D6E63]">
                                        Coffee Central Q1</p>
                                </div>
                                <Link href="/stores">
                                    <div className="w-12 h-12 bg-[#FFB74D] rounded-full flex items-center justify-center text-white hover:bg-[#FFCC80] cursor-pointer transition-all hover:scale-110 shadow-lg">
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                                        </svg>
                                    </div>
                                </Link>
                            </div>
                        </motion.div>
                    </motion.div>
                </div>
            </div>
        </section>
    )
}
