"use client"

import React, { useEffect, useState } from "react"
import { motion, AnimatePresence } from "framer-motion"
import { ArrowBigUp } from "lucide-react"

export function ScrollToTopButton() {
    const [visible, setVisible] = useState(false)
    const [launch, setLaunch] = useState(false)
    const [flyHeight, setFlyHeight] = useState(0)

    useEffect(() => {
        const toggleVisibility = () => setVisible(window.scrollY > 300)
        window.addEventListener("scroll", toggleVisibility)
        return () => window.removeEventListener("scroll", toggleVisibility)
    }, [])

    const scrollToTop = () => {
        const distance = window.scrollY

        const height = Math.min(800, distance * 0.6)
        setFlyHeight(height)
        setLaunch(true)
        setTimeout(() => setLaunch(false), 900)
        window.scrollTo({ top: 0, behavior: "smooth" })
    }

    return (
        <AnimatePresence>
            {visible && (
                <motion.button
                    key="scroll-top"
                    onClick={scrollToTop}
                    initial={{ opacity: 0, y: 40, scale: 0.9 }}
                    animate={{ opacity: 1, y: 0, scale: 1 }}
                    exit={{ opacity: 0, y: 40, scale: 0.9 }}
                    transition={{ duration: 0.3, ease: "easeOut" }}
                    whileHover={{ scale: 1.1 }}
                    whileTap={{ scale: 0.95 }}
                    className="bg-primary text-white fixed bottom-6 right-6 z-50 p-4 rounded-full shadow-lg
                     flex items-center justify-center overflow-hidden"
                >
                    {/* Mũi tên có animation bay lên cao */}
                    <motion.div
                        key={launch ? "flying" : "idle"}
                        animate={
                            launch
                                ? {
                                    y: [0, -flyHeight, -flyHeight - 100],
                                    opacity: [1, 0.9, 0],
                                    scale: [1, 1.2, 0.8],
                                    rotate: [-2, 0, 2]
                                }
                                : { y: 0, opacity: 1, scale: 1, rotate: 0 }
                        }
                        transition={{ duration: 0.9, ease: "easeInOut" }}
                        className="flex items-center justify-center"
                    >
                        <ArrowBigUp size={30} />
                    </motion.div>
                </motion.button>
            )}
        </AnimatePresence>
    )
}
