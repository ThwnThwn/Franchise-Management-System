"use client"

import { useState } from "react"
import Link from "next/link"
import { motion, AnimatePresence } from "framer-motion"
import { ChevronRight } from "lucide-react"
import { MenuItem } from "@/constants/menu/sidebar"

export const SidebarItem = ({
    item,
    activeHref, // Nhận activeHref từ cha
    isMobile,
    onClose
}: {
    item: MenuItem
    activeHref: string
    isMobile: boolean
    onClose: () => void
}) => {
    const hasSubItems = !!item.subItems && item.subItems.length > 0

    // Logic kiểm tra active giờ đây cực kỳ đơn giản
    // Cha có active không? (Cha active nếu bản thân nó active hoặc 1 trong các con của nó active)
    const isActive = hasSubItems
        ? item.subItems!.some((sub) => sub.href === activeHref) || item.href === activeHref
        : item.href === activeHref

    const [isOpen, setIsOpen] = useState(isActive)
    const [prevIsActive, setPrevIsActive] = useState(isActive)

    if (isActive !== prevIsActive) {
        setPrevIsActive(isActive)
        if (isActive) {
            setIsOpen(true)
        }
    }

    const activeClasses = "bg-linear-to-r from-primary to-primary/90 text-white shadow-lg font-bold"
    const inactiveClasses = "text-foreground/70 hover:bg-primary-50 hover:text-primary"

    if (!hasSubItems) {
        if (!item.href) return null

        return (
            <Link href={item.href} onClick={onClose} className="block">
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

    return (
        <div className="flex flex-col">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className={`flex items-center justify-between px-4 py-3 rounded-xl transition-all duration-300 group ${
                    isActive && !isOpen ? "bg-primary-50 text-primary font-bold" : inactiveClasses
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
                <motion.div animate={{ rotate: isOpen ? 90 : 0 }} transition={{ duration: 0.2 }}>
                    <ChevronRight size={16} className="opacity-70" />
                </motion.div>
            </button>

            <AnimatePresence initial={false}>
                {isOpen && (
                    <motion.div
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: "auto", opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        transition={{ duration: 0.2, ease: "easeInOut" }}
                        className="overflow-hidden"
                    >
                        <div className="flex flex-col gap-1 mt-1 mb-2 ml-[1.6rem] border-l-2 border-primary pl-2 pr-0">
                            {item.subItems!.map((sub, index) => {
                                if (!sub.href) return null
                                const isSubActive = sub.href === activeHref

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
