"use client"

import { CampaignResponse } from "@/features/campaigns/models/response"
import { Button, cn } from "@heroui/react"
import { AnimatePresence, motion } from "framer-motion"
import { ChevronLeft, ChevronRight, Dot } from "lucide-react"
import { useCallback, useState } from "react"
import { CampaignBanner } from "../CampaignBanner"
import { useRouter } from "next/navigation"

interface Props {
    campaigns: CampaignResponse[]
    autoPlayInterval?: number
}

export const CampaignCarousel = ({ campaigns }: Props) => {
    const router = useRouter()

    const [currentIndex, setCurrentIndex] = useState(0)
    const [direction, setDirection] = useState(0)

    const slideCount = campaigns.length

    const nextSlide = useCallback(() => {
        setDirection(1)
        setCurrentIndex((prev) => (prev + 1) % slideCount)
    }, [slideCount])

    const prevSlide = useCallback(() => {
        setDirection(-1)
        setCurrentIndex((prev) => (prev - 1 + slideCount) % slideCount)
    }, [slideCount])

    // useEffect(() => {
    //     if (slideCount <= 1) return
    //     const timer = setInterval(nextSlide, autoPlayInterval)
    //     return () => clearInterval(timer)
    // }, [nextSlide, autoPlayInterval, slideCount])

    if (slideCount === 0) return null

    const variants = {
        enter: (direction: number) => ({
            x: direction > 0 ? "100%" : "-100%",
            opacity: 0
        }),
        center: { x: 0, opacity: 1 },
        exit: (direction: number) => ({
            x: direction < 0 ? "100%" : "-100%",
            opacity: 0
        })
    }

    return (
        <div className="w-full overflow-hidden flex flex-col items-center">
            {/* KHUNG CHUẨN: Bóp lại đúng bằng kích thước banner (max-w-4xl) */}
            <div className="relative group w-full max-w-3xl h-60 md:h-70">
                {/* --- NỘI DUNG SLIDE --- */}
                <AnimatePresence initial={false} custom={direction} mode="wait">
                    <motion.div
                        key={currentIndex}
                        custom={direction}
                        variants={variants}
                        initial="enter"
                        animate="center"
                        exit="exit"
                        transition={{
                            x: { type: "spring", stiffness: 300, damping: 30 },
                            opacity: { duration: 0.2 }
                        }}
                        // Dùng absolute inset-0 để nó bung full cái khung 4xl
                        className="absolute inset-0 w-full h-full shadow-2xl rounded-3xl"
                    >
                        <CampaignBanner
                            campaign={campaigns[currentIndex]}
                            size="lg"
                            theme={"gold"}
                            className="h-full w-full"
                            onPress={() => router.push(`/campaigns/${campaigns[currentIndex].id}`)}
                        />
                    </motion.div>
                </AnimatePresence>

                {/* --- NÚT ĐIỀU HƯỚNG --- */}
                {slideCount > 1 && (
                    <>
                        {/* Nút Trái (Cao bằng thẻ, hiện khi hover) */}
                        <Button
                            radius="none"
                            variant="light"
                            className={cn(
                                "absolute left-0 top-0 h-full w-12 z-20 p-0 min-w-0",
                                "bg-black/10 hover:bg-black/40 backdrop-blur-[2px] text-white", // Màu tối nhẹ làm nổi icon trắng
                                "opacity-0 group-hover:opacity-100 transition-all duration-300",
                                "rounded-l-3xl border-r border-white/10" // Bo tròn góc trái khớp với banner
                            )}
                            onPress={prevSlide}
                        >
                            <ChevronLeft size={32} />
                        </Button>

                        {/* Nút Phải (Cao bằng thẻ, hiện khi hover) */}
                        <Button
                            radius="none"
                            variant="light"
                            className={cn(
                                "absolute right-0 top-0 h-full w-12 z-20 p-0 min-w-0",
                                "bg-black/10 hover:bg-black/40 backdrop-blur-[2px] text-white",
                                "opacity-0 group-hover:opacity-100 transition-all duration-300",
                                "rounded-r-3xl border-l border-white/10" // Bo tròn góc phải khớp với banner
                            )}
                            onPress={nextSlide}
                        >
                            <ChevronRight size={32} />
                        </Button>
                    </>
                )}
            </div>

            {slideCount > 1 && (
                <div className="flex justify-center mt-6 gap-2 relative z-10">
                    {campaigns.map((_, index) => (
                        <button
                            key={index}
                            onClick={() => {
                                setDirection(index > currentIndex ? 1 : -1)
                                setCurrentIndex(index)
                            }}
                            // Thêm p-2 để cái nút to ra, khách nhắm mắt bấm cũng trúng
                            className="focus:outline-none p-2 rounded-full group/dot cursor-pointer"
                        >
                            <Dot
                                size={32}
                                className={cn(
                                    "transition-all duration-300 origin-center",
                                    index === currentIndex
                                        ? "text-primary-500 scale-150" // Chỉnh lại scale-150 là đủ to rồi
                                        : "text-default-300 opacity-50 group-hover/dot:opacity-100 group-hover/dot:scale-125" // Fix lại opacity-50
                                )}
                            />
                        </button>
                    ))}
                </div>
            )}
        </div>
    )
}
