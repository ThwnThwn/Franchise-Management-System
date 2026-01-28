/**
 * Sidebar Component
 * Navigation sidebar for dashboard
 */

"use client";

import { useState } from "react";
import { Card } from "@heroui/react";
import { motion, AnimatePresence } from "framer-motion";
import {
    LayoutDashboard,
    Coffee,
    Users,
    Package,
    Settings,
    Menu,
    X,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

interface MenuItem {
    icon: React.ReactNode;
    label: string;
    href: string;
}

const menuItems: MenuItem[] = [
    {
        icon: <LayoutDashboard className="w-5 h-5" />,
        label: "Tổng quan",
        href: "/dashboard",
    },
    {
        icon: <Coffee className="w-5 h-5" />,
        label: "Quản lý cà phê",
        href: "/dashboard/coffee",
    },
    {
        icon: <Users className="w-5 h-5" />,
        label: "Quản lý nhân viên",
        href: "/dashboard/staff",
    },
    {
        icon: <Package className="w-5 h-5" />,
        label: "Quản lý kho",
        href: "/dashboard/inventory",
    },
    {
        icon: <Settings className="w-5 h-5" />,
        label: "Cài đặt",
        href: "/dashboard/settings",
    },
];

export const Sidebar = () => {
    const [isOpen, setIsOpen] = useState(false);
    const pathname = usePathname();

    return (
        <>
            {/* Mobile Menu Button */}
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="fixed top-4 left-4 z-50 lg:hidden p-2 rounded-lg bg-white/90 backdrop-blur-md border border-primary-100 shadow-lg"
            >
                {isOpen ? (
                    <X className="w-6 h-6 text-primary" />
                ) : (
                    <Menu className="w-6 h-6 text-primary" />
                )}
            </button>

            {/* Overlay for mobile */}
            <AnimatePresence>
                {isOpen && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        onClick={() => setIsOpen(false)}
                        className="fixed inset-0 bg-black/50 z-40 lg:hidden"
                    />
                )}
            </AnimatePresence>

            {/* Sidebar */}
            <motion.aside
                initial={false}
                animate={{
                    x: isOpen ? 0 : "-100%",
                }}
                transition={{ duration: 0.3, ease: "easeInOut" }}
                className="fixed top-0 left-0 h-screen w-64 bg-gradient-to-b from-white/95 to-white/90 backdrop-blur-xl border-r border-primary-100 shadow-2xl z-40 lg:translate-x-0 lg:static"
            >
                <div className="flex flex-col h-full p-6">
                    {/* Logo */}
                    <div className="flex items-center gap-3 mb-8 pt-2">
                        <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center">
                            <Coffee className="w-6 h-6 text-white" />
                        </div>
                        <div>
                            <h1 className="text-xl font-bold text-primary">Coffee Shop</h1>
                            <p className="text-xs text-foreground/60">Management</p>
                        </div>
                    </div>

                    {/* Navigation Menu */}
                    <nav className="flex-1 space-y-2">
                        {menuItems.map((item, index) => {
                            const isActive = pathname === item.href;
                            return (
                                <Link
                                    key={item.href}
                                    href={item.href}
                                    onClick={() => setIsOpen(false)}
                                >
                                    <motion.div
                                        initial={{ opacity: 0, x: -20 }}
                                        animate={{ opacity: 1, x: 0 }}
                                        transition={{ duration: 0.3, delay: index * 0.05 }}
                                        className={`
                      flex items-center gap-3 px-4 py-3 rounded-xl
                      transition-all duration-300 group
                      ${isActive
                                                ? "bg-gradient-to-r from-primary to-primary/90 text-white shadow-lg"
                                                : "text-foreground/70 hover:bg-primary-50 hover:text-primary"
                                            }
                    `}
                                    >
                                        <span
                                            className={`
                      transition-transform duration-300
                      ${isActive ? "" : "group-hover:scale-110"}
                    `}
                                        >
                                            {item.icon}
                                        </span>
                                        <span className="font-medium text-sm">{item.label}</span>
                                        {isActive && (
                                            <motion.div
                                                layoutId="activeIndicator"
                                                className="ml-auto w-2 h-2 rounded-full bg-white"
                                            />
                                        )}
                                    </motion.div>
                                </Link>
                            );
                        })}
                    </nav>

                    {/* User Profile */}
                    <Card className="mt-auto p-4 bg-gradient-to-br from-primary-50 to-primary-100/50 border-none">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary to-secondary flex items-center justify-center text-white font-bold">
                                A
                            </div>
                            <div className="flex-1 min-w-0">
                                <p className="text-sm font-semibold text-foreground truncate">
                                    Admin
                                </p>
                                <p className="text-xs text-foreground/60">Quản trị viên</p>
                            </div>
                        </div>
                    </Card>
                </div>
            </motion.aside>
        </>
    );
};
