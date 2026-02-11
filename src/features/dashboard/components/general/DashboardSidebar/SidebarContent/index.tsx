"use client"

import React from "react"
import { Card } from "@heroui/react"
import { motion } from "framer-motion"
import { X } from "lucide-react"
import Link from "next/link"
import { LogoStyled } from "@/components"
import { RoleSlugLabels } from "@/constants/labels"
import { MenuItem } from "../menuItems"
import { RoleSlug } from "@/constants/enums"

interface SidebarContentProps {
    isMobile: boolean
    items: MenuItem[]
    pathname: string
    roleSlug: RoleSlug
    onClose: () => void
}

export const SidebarContent = ({
    isMobile,
    items,
    pathname,
    roleSlug,
    onClose
}: SidebarContentProps) => {
    return (
        <div className="flex flex-col h-full relative">
            <div className="p-6 flex-1 flex flex-col min-h-0">
                {/* Logo Section */}
                <div className="flex items-center justify-between mb-4 pt-2">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                            <LogoStyled className="w-10 h-10" />
                        </div>
                        <div>
                            <h1 className="text-xl font-bold text-primary">Coffee Shop</h1>
                            <p className="text-xs text-foreground/60">Management</p>
                        </div>
                    </div>
                    {isMobile && (
                        <button
                            onClick={onClose}
                            className="p-1 text-primary hover:bg-primary-50 rounded-lg transition-colors"
                        >
                            <X size={24} />
                        </button>
                    )}
                </div>

                {/* Navigation Menu */}
                <nav className="flex-1 space-y-2 overflow-y-auto custom-scrollbar pr-2">
                    {items.map((item, index) => {
                        const isActive =
                            item.href === "/dashboard"
                                ? pathname === item.href
                                : pathname.startsWith(item.href)

                        return (
                            <Link key={item.href} href={item.href} onClick={onClose}>
                                <motion.div
                                    initial={isMobile ? { opacity: 0, x: -20 } : false}
                                    animate={isMobile ? { opacity: 1, x: 0 } : false}
                                    transition={{ duration: 0.3, delay: index * 0.05 }}
                                    className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-300 group ${
                                        isActive
                                            ? "bg-linear-to-r from-primary to-primary/90 text-white shadow-lg"
                                            : "text-foreground/70 hover:bg-primary-50 hover:text-primary"
                                    }`}
                                >
                                    <span className={isActive ? "" : "group-hover:scale-110"}>
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
                    })}
                </nav>
            </div>

            {/* User Profile Sticky Bottom */}
            <div className="mt-auto sticky bottom-0 p-6 bg-background/80 backdrop-blur-md border-t border-divider">
                <Card className="p-4 bg-linear-to-br from-primary-50 to-primary-100/50 border-none shadow-none">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center text-white font-bold uppercase shadow-sm">
                            {roleSlug?.charAt(0) || "U"}
                        </div>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold text-foreground truncate capitalize">
                                {roleSlug || "User"}
                            </p>
                            <p className="text-xs text-foreground/60">{RoleSlugLabels[roleSlug]}</p>
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    )
}
