"use client"

import React, { useEffect, useState } from "react"
import Image from "next/image"
import Link from "next/link"
import { motion } from "framer-motion"
import { productApi } from "@/features/menu/services/productApi"
import { GetProductListItemDto } from "@/features/menu/models/schema/respone"

export default function FeaturedSection() {
    const [featuredItems, setFeaturedItems] = useState<GetProductListItemDto[]>([])
    const [isLoading, setIsLoading] = useState(true)

    useEffect(() => {
        const fetchProducts = async () => {
            try {
                const res = await productApi.getAll({ page: 1, pageSize: 4 })
                if (res?.products) {
                    setFeaturedItems(res.products.slice(0, 4))
                }
            } catch (error) {
                console.error("Failed to fetch featured products", error)
            } finally {
                setIsLoading(false)
            }
        }
        fetchProducts()
    }, [])

    if (isLoading) {
        return (
            <section className="py-20 bg-white">
                <div className="max-w-7xl mx-auto px-4 text-center">
                    <p>Đang tải sản phẩm nổi bật...</p>
                </div>
            </section>
        )
    }

    return (
        <section className="py-20 bg-white">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="text-center mb-16">
                    <motion.h2
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="text-4xl font-bold text-gray-900 mb-4"
                    >
                        Sản Phẩm Nổi Bật
                    </motion.h2>
                    <div className="w-24 h-1 bg-yellow-500 mx-auto rounded-full" />
                    <p className="mt-4 text-gray-600 text-lg">
                        Những thức uống được yêu thích nhất tại cửa hàng
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                    {featuredItems.map((item, index) => (
                        <motion.div
                            key={item.id}
                            initial={{ opacity: 0, y: 30 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ delay: index * 0.1 }}
                            className="group bg-white rounded-2xl shadow-sm hover:shadow-xl transition-all duration-300 border border-gray-100 overflow-hidden"
                        >
                            <div className="relative h-64 w-full overflow-hidden">
                                <Image
                                    src={item.imageUrl || "https://images.unsplash.com/photo-1554118811-1e0d58224f24?q=80&w=1000&auto=format&fit=crop"}
                                    alt={item.name}
                                    fill
                                    className="object-cover transform group-hover:scale-110 transition-transform duration-500"
                                />
                                <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-end justify-center pb-6">
                                    <Link href="/menu">
                                        <span className="px-6 py-2 bg-yellow-500 text-black font-semibold rounded-full hover:bg-yellow-400 transition-colors">
                                            Đặt ngay
                                        </span>
                                    </Link>
                                </div>
                            </div>
                            <div className="p-6 text-center">
                                <h3 className="text-xl font-bold text-gray-900 mb-2 truncate">
                                    {item.name}
                                </h3>
                                <p className="text-gray-500 text-sm line-clamp-2 mb-4">
                                    {item.description}
                                </p>
                                <div className="text-yellow-600 font-bold text-lg">
                                    {item.sizes?.[0]?.basePrice?.toLocaleString() ?? "N/A"}đ
                                </div>
                            </div>
                        </motion.div>
                    ))}
                </div>

                <div className="text-center mt-12">
                    <Link href="/menu">
                        <button className="px-8 py-3 border-2 border-gray-900 text-gray-900 font-bold rounded-lg hover:bg-gray-900 hover:text-white transition-colors">
                            Xem tất cả menu
                        </button>
                    </Link>
                </div>
            </div>
        </section>
    )
}
