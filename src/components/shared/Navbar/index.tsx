"use client"
import React, { useEffect, useMemo, useState } from "react"
import "./index.css"
import { NavbarBrand, NavbarContent, NavbarItem } from "@heroui/react"
import Link from "next/link"
import { ButtonStyled, NavbarStyled, LogoStyled, ButtonIconStyled } from "@/components/"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { ProfileDropdown } from "./ProfileDropdown"
import { usePathname } from "next/navigation"
import clsx from "clsx"
import { X, Menu } from "lucide-react"
import { motion } from "framer-motion"
import { createPortal } from "react-dom"
import { STRINGS } from "@/constants/strings"
import { CHB, CHB_SHORT } from "@/constants/constants"

type MenuKey = "home" | "stores" | "menu" | "about" | "contact" | undefined
type MenuItem = { key: Exclude<MenuKey, undefined>; label: string }

export function Navbar() {
    const pathname = usePathname()
    // handle when login
    const isLoggedIn = useTokenStore((s) => !!s.accessToken)
    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()

    // handle navbar
    type NavbarState = "default" | "top" | "middle"
    const [scrollState, setScroledState] = useState<NavbarState>("default")
    const [isHiddenNavbar, setIsHiddenNavbar] = useState(false)
    const [lastScrollY, setLastScrollY] = useState(0)
    const [isOpen, setIsOpen] = useState(false) // mobile drawer state
    // set active key
    const { menuKey } = useMemo(() => {
        const segment = pathname.split("/").filter(Boolean)[0]

        const mapSegmentToMenuKey = (segment: string | undefined): MenuKey => {
            switch (segment) {
                case "":
                case undefined:
                    return "home"
                case "stores":
                    return "stores"
                case "menu":
                    return "menu"
                default:
                    return undefined
            }
        }

        return {
            firstSegment: segment,
            menuKey: mapSegmentToMenuKey(segment)
        }
    }, [pathname])

    const [activeMenuKey, setActiveMenuKey] = useState<MenuKey>(menuKey)
    const menus = useMemo(
        () => [
            { key: "home", label: STRINGS.NAVBAR.HOME },
            { key: "menu", label: "Thực đơn" },
            { key: "stores", label: "Cửa hàng" }
        ] satisfies MenuItem[],
        []
    )
    useEffect(() => {
        setActiveMenuKey(menuKey)
    }, [menuKey])

    const isCollapse = scrollState === "top" || scrollState === "middle"

    // handle navbar animation
    const baseClasses = clsx(
        "bg-transparent transition-all duration-400 ease-in-out fixed left-0 w-full z-50 mx-auto",
        "max-w-6xl mt-3 rounded-3xl justify-between",
        "data-[visible=false]:mt-0",
        {
            "-translate-y-full opacity-0": isHiddenNavbar,
            "translate-y-0 opacity-100": !isHiddenNavbar,
            "text-white rounded-3xl bg-[#080808]/60 backdrop-blur-md mx-auto max-w-3xl scale-95":
                isCollapse,
            "backdrop-blur-none": !isCollapse,
            "text-white": menuKey === "about"
        }
    )

    const itemClasses = [
        "flex",
        "relative",
        "h-full",
        "data-[active=true]:after:content-['']",
        "data-[active=true]:after:absolute",
        "data-[active=true]:after:bottom-0",
        "data-[active=true]:after:left-0",
        "data-[active=true]:after:right-0",
        "data-[active=true]:after:h-[5px]",
        "data-[active=true]:after:w-full",
        "data-[active=true]:after:rounded-[3px]",
        "data-[active=true]:after:bg-warning"
    ]

    // useEffect
    // handle navbar scroll
    useEffect(() => {
        const handleScroll = () => {
            const y = window.scrollY
            let nextScrollState: NavbarState
            // let nextHidden: boolean

            if (y >= 0 && y < 10) {
                nextScrollState = "default"
                // nextHidden = false
            } else if (y >= 10 && y < 400) {
                nextScrollState = "top"
                // nextHidden = false
            } else {
                nextScrollState = "middle"
                // nextHidden = true
            }

            setScroledState((prev) => (prev === nextScrollState ? prev : nextScrollState))
            // setIsHiddenNavbar((prev) =>
            //     prev === nextHidden ? prev : nextHidden
            // )

            if (y < 400) {
                setIsHiddenNavbar(false)
            } else {
                if (y > lastScrollY) {
                    // scroll xuống
                    setIsHiddenNavbar(true)
                } else {
                    // scroll lên
                    setIsHiddenNavbar(false)
                }
            }
            setLastScrollY(y)
        }

        window.addEventListener("scroll", handleScroll, { passive: true })

        return () => window.removeEventListener("scroll", handleScroll)
    }, [lastScrollY])
    // const [open, setOpen] = useState(false)

    return (
        <NavbarStyled
            id="main-navbar"
            data-visible={!isHiddenNavbar}
            classNames={{
                base: [baseClasses],
                item: [itemClasses]
            }}
        >
            {/* start content */}
            <NavbarBrand>
                <Link
                    href={"/"}
                    className="flex items-center gap-2 z-9999"
                    onClick={() => setIsOpen(false)}
                >
                    <LogoStyled className="w-12 h-12" />
                    <p className="font-bold text-inherit">{isCollapse ? CHB_SHORT : CHB}</p>
                </Link>
            </NavbarBrand>
            {/* middle content */}
            <NavbarContent className="hidden sm:flex gap-4 justify-center">
                {menus.map((menu) => (
                    <NavbarItem
                        id={menu.key}
                        key={menu.key}
                        isActive={activeMenuKey == menu.key}
                        className={`""text-center px-3 w-fit"
                            ${isCollapse ? "text-white" : "text-inherit"}`}
                    >
                        <Link
                            href={menu.key === "home" ? "/" : "/" + menu.key}
                            onClick={() => setActiveMenuKey(menu.key)}
                            className="h-full flex items-center"
                        >
                            {menu.label}
                        </Link>
                    </NavbarItem>
                ))}
            </NavbarContent>
            {/* end content */}
            <NavbarContent justify="end">
                <div className="relative sm:hidden">
                    {/* Nút mở menu */}
                    <ButtonIconStyled onPress={() => setIsOpen((prev) => !prev)} className="z-9999">
                        {isOpen ? <X size={24} /> : <Menu size={24} />}
                    </ButtonIconStyled>

                    {/* Backdrop */}
                    {isOpen &&
                        createPortal(
                            <div
                                className="fixed inset-0 bg-black/40 z-40 backdrop-blur-sm"
                                onClick={() => setIsOpen(false)}
                            />,
                            document.body
                        )}

                    {/* Menu trượt xuống */}
                    <motion.div
                        initial={{ y: "-100%", opacity: 0 }}
                        animate={isOpen ? { y: 0, opacity: 1 } : { y: "-100%", opacity: 0 }}
                        transition={{ type: "spring", stiffness: 260, damping: 25 }}
                        className="fixed top-19 left-0 w-full bg-white shadow-lg flex flex-col items-center z-999 pb-3"
                    >
                        {menus.map((menu) => {
                            const isActive =
                                pathname === (menu.key === "home" ? "/" : `/${menu.key}`)
                            return (
                                <button
                                    key={menu.key}
                                    className={`text-lg block w-full py-2 text-center font-medium ${isActive
                                        ? "text-white font-semibold bg-primary"
                                        : "text-black hover:text-white hover:bg-primary"
                                        }`}
                                >
                                    <Link
                                        href={menu.key === "home" ? "/" : "/" + menu.key}
                                        onClick={() => setIsOpen(false)}
                                    >
                                        {menu.label}
                                    </Link>
                                </button>
                            )
                        })}
                    </motion.div>
                </div>

                {/* nút giỏ hàng */}
                {/* <NavbarItem className="flex items-center z-9999">
                    <CartPopover />
                </NavbarItem> */}

                <NavbarItem className="flex items-center z-9999">
                    {isLoggedIn ? (
                        <ProfileDropdown onOpen={() => setIsOpen(false)} />
                    ) : (
                        <ButtonStyled
                            onPress={() => {
                                onOpenLogin()
                                setIsOpen(false)
                            }}
                            color="primary"
                            variant="solid"
                            className="rounded-3xl opacity-97"
                        >
                            {STRINGS.NAVBAR.LOGIN}
                        </ButtonStyled>
                    )}
                </NavbarItem>
            </NavbarContent>
        </NavbarStyled>
    )
}
