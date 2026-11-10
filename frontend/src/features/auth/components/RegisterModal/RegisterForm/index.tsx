"use client"
import { cn } from "@heroui/react"
import React, { useState } from "react"
import { RowStep } from "./RowStep"
import { AnimatePresence, motion } from "framer-motion"
import { Register } from "./RegisterInfo"
import { RegisterOTP } from "./RegisterOTP"
import { CHB, SLOGAN } from "@/constants/constants"

export function RegisterForm({ onSuccess }: { onSuccess?: () => void }) {
    const [step, setStep] = useState(0)
    const [direction, setDirection] = useState(1)
    const [email, setEmail] = useState("")

    const variants = {
        enter: (direction: number) => ({
            x: direction > 0 ? 50 : -50, // Giảm khoảng cách trượt để mượt hơn trong container nhỏ
            opacity: 0
        }),
        center: { x: 0, opacity: 1 },
        exit: (direction: number) => ({
            x: direction > 0 ? -50 : 50,
            opacity: 0
        })
    }

    const handleNextStep = () => {
        setDirection(1)
        setStep((prev) => prev + 1)
    }

    const handlePrevStep = () => {
        setDirection(-1)
        setStep((prev) => prev - 1)
    }

    return (
        /* Container chính bao gồm cả ảnh và form */
        <div className="flex flex-col md:flex-row bg-white w-full lg:w-6xl max-w-[95vw] shadow-2xl rounded-3xl overflow-hidden min-h-209.5">
            {/* PHẦN 1: Ảnh hoặc Gradient bên trái (Ẩn trên mobile nếu cần) */}
            <div className="relative hidden md:flex md:w-5/12 bg-[#2D2424] overflow-hidden items-center justify-center p-12">
                {/* Lớp nền Gradient màu cafe loan dần */}
                <div className="absolute inset-0 bg-linear-to-br from-[#4E342E] via-[#2D2424] to-[#1A120B] opacity-90" />

                {/* Hình ảnh decor (Thay URL ảnh cafe của bạn vào đây) */}
                <div
                    className="absolute inset-0 opacity-40 bg-cover bg-center mix-blend-overlay"
                    style={{
                        backgroundImage:
                            "url('https://aeroco.vn/wp-content/uploads/2023/09/an-tuong-ca-phe-viet-nam-trong-mat-ban-be-quoc-te-1024x730.jpeg')"
                    }}
                />

                <div className="relative z-10 text-center">
                    <motion.h2
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="text-4xl font-bold text-[#D7CCC8] mb-4"
                    >
                        {CHB}
                    </motion.h2>
                    <p className="text-[#A1887F] text-lg">{SLOGAN}</p>
                </div>
            </div>

            {/* PHẦN 2: Nội dung Form bên phải */}
            <div className="flex-1 flex flex-col p-6 sm:p-12 justify-center bg-white">
                <div className="w-full max-w-112.5 mx-auto">
                    <div className="mb-8 hidden md:block">
                        <RowStep n={step} />
                    </div>

                    <div
                        className={cn(
                            "relative transition-all duration-500 ease-in-out",
                            step === 0 ? "min-h-137.5" : "min-h-100"
                        )}
                    >
                        <AnimatePresence mode="wait" custom={direction}>
                            {step === 0 && (
                                <motion.div
                                    key="email"
                                    custom={direction}
                                    variants={variants}
                                    initial="enter"
                                    animate="center"
                                    exit="exit"
                                    transition={{ duration: 0.4, ease: "easeInOut" }}
                                    className="w-full"
                                >
                                    <Register
                                        email={email}
                                        setEmail={setEmail}
                                        onSuccess={handleNextStep}
                                    />
                                </motion.div>
                            )}
                            {step === 1 && (
                                <motion.div
                                    key="otp"
                                    custom={direction}
                                    variants={variants}
                                    initial="enter"
                                    animate="center"
                                    exit="exit"
                                    transition={{ duration: 0.4, ease: "easeInOut" }}
                                    className="w-full"
                                >
                                    <RegisterOTP
                                        email={email}
                                        onBack={handlePrevStep}
                                        onSuccess={onSuccess}
                                    />
                                </motion.div>
                            )}
                        </AnimatePresence>
                    </div>
                </div>
            </div>
        </div>
    )
}
