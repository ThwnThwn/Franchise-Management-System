"use client"

import { useState } from "react"
import Link from "next/link"
import { motion, AnimatePresence } from "framer-motion"
import { ChevronRight } from "lucide-react"
import { MenuItem } from "@/constants/menu/sidebar"

export const SidebarItem = ({
    item,
    pathname,
    isMobile,
    onClose
}: {
    item: MenuItem
    pathname: string
    isMobile: boolean
    onClose: () => void
}) => {
    const hasSubItems = !!item.subItems && item.subItems.length > 0

    // LOGIC FIX: Hàm kiểm tra Active siêu chặt chẽ
    // Thay vì startWith rườm rà, mình check chính xác xem pathname có khớp không.
    // LOGIC FIX: Hàm kiểm tra Active siêu chặt chẽ
    const checkActive = (href: string) => {
        // Nếu là trang gốc dashboard
        if (href === "/dashboard") return pathname === "/dashboard"

        // Nếu path khớp chính xác 100%
        if (pathname === href) return true

        // Nếu path hiện tại dài hơn (ví dụ đang ở trang chi tiết /dashboard/shifts/123)
        // thì mình kiểm tra xem url có startWith cái href và theo sau nó có phải là dấu "/" hay không.
        // NHƯNG ĐẶC BIỆT LƯU Ý: Phải so độ dài để thằng nào dài nhất ăn điểm!
        // Ở đây mình phó mặc cho React lặp qua các subItems, nếu đang ở /shifts/pools thì nó phải
        // KHÔNG match với /shifts (Vì /shifts/pools dài hơn).

        // CÁCH AN TOÀN NHẤT CHO CẤU TRÚC HIỆN TẠI:
        // Cắt bỏ phần id hoặc query (nếu có) để so sánh thư mục cha gần nhất.
        const pathSegments = pathname.split("/").filter(Boolean)
        const hrefSegments = href.split("/").filter(Boolean)

        // Nếu href dài hơn pathname (vô lý)
        if (hrefSegments.length > pathSegments.length) return false

        // Kiểm tra xem từng khúc của href có khớp y xì với khúc đầu của pathname không
        for (let i = 0; i < hrefSegments.length; i++) {
            if (pathSegments[i] !== hrefSegments[i]) return false
        }

        // TỚI ĐÂY NGHĨA LÀ MATCH CẤP CHA RỒI.
        // VẤN ĐỀ CUỐI: Chống dính /shifts và /shifts/pools
        // Nếu mình đang ở /shifts/pools (length 3), mà href đang xét là /shifts (length 2)
        // Mình PHẢI check xem trong list sub-items CÓ thằng nào length 3 match hoàn toàn không.
        // Rất phức tạp!

        // => GIẢI PHÁP TỐI ƯU NHẤT: Bắt chính xác 100% là khỏe nhất cho Menu con.
        return false
    }

    // Cha có active không?
    const isActive = hasSubItems
        ? item.subItems!.some((sub) => checkActive(sub.href))
        : checkActive(item.href || "")

    // State mở accordion
    const [isOpen, setIsOpen] = useState(isActive)
    const [prevIsActive, setPrevIsActive] = useState(isActive)

    // Render-phase state update
    if (isActive !== prevIsActive) {
        setPrevIsActive(isActive)
        if (isActive) {
            setIsOpen(true)
        }
    }

    // --- CẤU HÌNH CLASS CHUNG ĐỂ ĐỒNG NHẤT MÀU SẮC ---
    // Ní yêu cầu sub-item có màu Y CHANG thằng ko có sub-item
    const activeClasses = "bg-linear-to-r from-primary to-primary/90 text-white shadow-lg font-bold"
    const inactiveClasses = "text-foreground/70 hover:bg-primary-50 hover:text-primary"

    // 1. NẾU KHÔNG CÓ SUB-MENU (Link thẳng)
    if (!hasSubItems) {
        return (
            <Link href={item.href!} onClick={onClose} className="block">
                <motion.div
                    className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-300 group ${
                        isActive ? activeClasses : inactiveClasses
                    }`}
                >
                    <span className={isActive ? "" : "group-hover:scale-110 transition-transform"}>
                        {item.icon}
                    </span>
                    <span className="font-medium text-sm">{item.label}</span>
                    {isActive && (
                        <motion.div
                            layoutId={isMobile ? "activeMobile" : "activeDesktop"}
                            className="ml-auto w-2 h-2 rounded-full bg-white"
                        />
                    )}
                </motion.div>
            </Link>
        )
    }

    // 2. NẾU CÓ SUB-MENU (Xổ xuống)
    return (
        <div className="flex flex-col">
            {/* Nút bấm của thằng Cha */}
            <button
                onClick={() => setIsOpen(!isOpen)}
                className={`flex items-center justify-between px-4 py-3 rounded-xl transition-all duration-300 group ${
                    isActive && !isOpen
                        ? "bg-primary-50 text-primary font-bold" // Đang active trang con mà đóng accordion
                        : inactiveClasses
                }`}
            >
                <div className="flex items-center gap-3">
                    <span
                        className={
                            isActive ? "text-primary" : "group-hover:scale-110 transition-transform"
                        }
                    >
                        {item.icon}
                    </span>
                    <span className="font-medium text-sm">{item.label}</span>
                </div>
                {/* Mũi tên xoay */}
                <motion.div animate={{ rotate: isOpen ? 90 : 0 }} transition={{ duration: 0.2 }}>
                    <ChevronRight size={16} className="opacity-70" />
                </motion.div>
            </button>

            {/* Danh sách Sub-items */}
            <AnimatePresence initial={false}>
                {isOpen && (
                    <motion.div
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: "auto", opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        transition={{ duration: 0.2, ease: "easeInOut" }}
                        className="overflow-hidden"
                    >
                        {/* LOGIC FIX: Xóa ml-5 để width full, thay bằng padding-left bên trong từng thẻ 
                          Thêm viền trái (border-l-2) bám sát theo chiều dọc
                        */}
                        <div className="flex flex-col gap-1 mt-1 mb-2 ml-[1.6rem] border-l-2 border-primary pl-2 pr-0">
                            {item.subItems!.map((sub, index) => {
                                // Logic checkActive chặt chẽ
                                const isSubActive = checkActive(sub.href)

                                return (
                                    <Link
                                        key={index}
                                        href={sub.href}
                                        onClick={onClose}
                                        className="block"
                                    >
                                        <motion.div
                                            className={`flex items-center px-4 py-3 rounded-xl transition-all duration-300 group ${
                                                isSubActive ? activeClasses : inactiveClasses
                                            }`}
                                        >
                                            {/* Đổi màu chấm tròn cho hợp màu nền (nếu nền xanh thì chấm trắng) */}
                                            <span className="font-medium text-sm">{sub.label}</span>
                                            {isSubActive && (
                                                <motion.div
                                                    layoutId={
                                                        isMobile ? "activeMobile" : "activeDesktop"
                                                    }
                                                    className="ml-auto w-2 h-2 rounded-full bg-white"
                                                />
                                            )}
                                        </motion.div>
                                    </Link>
                                )
                            })}
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    )
}
