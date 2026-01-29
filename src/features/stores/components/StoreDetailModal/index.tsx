import { StoreDetailDto } from "../../models/schema/response"
import Image from "next/image"
import { motion, AnimatePresence } from "framer-motion"
import { useEffect } from "react"

type StoreDetailModalProps = {
    store: StoreDetailDto | null
    isOpen: boolean
    onClose: () => void
}

export default function StoreDetailModal({
    store,
    isOpen,
    onClose
}: StoreDetailModalProps) {
    // Prevent body scroll when modal is open and handle scrollbar shift
    useEffect(() => {
        if (isOpen) {
            const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth
            document.body.style.paddingRight = `${scrollbarWidth}px`
            document.body.style.overflow = "hidden"

            // Fix header jump
            const navbar = document.getElementById("main-navbar")
            if (navbar) {
                navbar.style.paddingRight = `${scrollbarWidth}px`
            }
        } else {
            document.body.style.overflow = "unset"
            document.body.style.paddingRight = "0px"

            const navbar = document.getElementById("main-navbar")
            if (navbar) {
                navbar.style.paddingRight = "0px"
            }
        }
        return () => {
            document.body.style.overflow = "unset"
            document.body.style.paddingRight = "0px"
            const navbar = document.getElementById("main-navbar")
            if (navbar) {
                navbar.style.paddingRight = "0px"
            }
        }
    }, [isOpen])

    return (
        <AnimatePresence>
            {isOpen && store && (
                <>
                    {/* Backdrop */}
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.3 }}
                        className="fixed inset-0 z-50 bg-black/50"
                        onClick={onClose}
                    />

                    {/* Side Drawer */}
                    <motion.div
                        initial={{ x: "-100%" }}
                        animate={{ x: 0 }}
                        exit={{ x: "-100%" }}
                        transition={{ type: "tween", duration: 0.3, ease: "easeInOut" }}
                        className="fixed inset-y-0 left-0 z-50 w-full max-w-md bg-white shadow-2xl overflow-y-auto"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="relative h-64 w-full">
                            <Image
                                src={store.imageUrl || "/placeholder-store.jpg"}
                                alt={store.name}
                                fill
                                className="object-cover"
                            />
                            <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />

                            <button
                                onClick={onClose}
                                className="absolute top-4 right-4 bg-black/20 hover:bg-black/40 text-white rounded-full p-2 backdrop-blur-md transition-all"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    className="h-6 w-6"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke="currentColor"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M6 18L18 6M6 6l12 12"
                                    />
                                </svg>
                            </button>

                            <div className="absolute bottom-4 left-4 right-4 text-white">
                                <h2 className="text-2xl font-bold mb-2 shadow-sm">{store.name}</h2>
                                {store.isOpenNow ? (
                                    <span className="bg-green-500/90 text-white px-3 py-1 rounded-full text-sm font-semibold backdrop-blur-md">
                                        Đang mở cửa
                                    </span>
                                ) : (
                                    <span className="bg-gray-500/90 text-white px-3 py-1 rounded-full text-sm font-semibold backdrop-blur-md">
                                        Đã đóng cửa
                                    </span>
                                )}
                            </div>
                        </div>

                        <div className="p-6 space-y-8">
                            <div className="space-y-6">
                                <h3 className="text-lg font-bold text-gray-900 border-b pb-2">Thông Tin</h3>

                                <div className="flex items-start group">
                                    <div className="w-10 h-10 rounded-full bg-amber-50 flex items-center justify-center mr-4 flex-shrink-0 group-hover:bg-amber-100 transition-colors">
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-amber-900" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                                        </svg>
                                    </div>
                                    <div>
                                        <p className="text-sm font-medium text-gray-500 mb-1">Địa chỉ</p>
                                        <p className="text-gray-900 leading-relaxed">{store.address}</p>
                                    </div>
                                </div>

                                <div className="flex items-start group">
                                    <div className="w-10 h-10 rounded-full bg-amber-50 flex items-center justify-center mr-4 flex-shrink-0 group-hover:bg-amber-100 transition-colors">
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-amber-900" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                                        </svg>
                                    </div>
                                    <div>
                                        <p className="text-sm font-medium text-gray-500 mb-1">Số điện thoại</p>
                                        <p className="text-gray-900 font-medium">{store.phone}</p>
                                    </div>
                                </div>

                                {store.latitude && store.longitude && (
                                    <div className="flex items-start group">
                                        <div className="w-10 h-10 rounded-full bg-amber-50 flex items-center justify-center mr-4 flex-shrink-0 group-hover:bg-amber-100 transition-colors">
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-amber-900" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
                                            </svg>
                                        </div>
                                        <div>
                                            <p className="text-sm font-medium text-gray-500 mb-1">Tọa độ GPS</p>
                                            <p className="text-gray-900 font-mono text-sm bg-gray-50 px-2 py-1 rounded inline-block">
                                                {store.latitude.toFixed(6)}, {store.longitude.toFixed(6)}
                                            </p>
                                        </div>
                                    </div>
                                )}
                            </div>

                            <button
                                onClick={onClose}
                                className="w-full py-4  bg-[#5D4037] text-white rounded-xl hover:bg-amber-800 transition-all font-bold shadow-lg transform active:scale-95"
                            >
                                Đóng
                            </button>
                        </div>
                    </motion.div>
                </>
            )}
        </AnimatePresence>
    )
}
