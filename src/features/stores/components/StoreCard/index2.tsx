import React from "react"
import { StoreDetailDto } from "../../models/schema/response"
import { Card, CardBody, CardFooter, Image } from "@heroui/react"
import { motion } from "framer-motion"
import { MapPin, Phone, Clock } from "lucide-react"
import { formatPhoneNumber } from "@/utils/helpers/stringHelper"

type StoreCardProps = {
    store: StoreDetailDto
    distance?: number | null
    onClick: () => void
}

export default function StoreCardsss({ store, distance, onClick }: StoreCardProps) {
    return (
        <motion.div
            layout
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            whileHover={{ y: -8 }}
            transition={{ duration: 0.3 }}
            className="h-full"
        >
            <Card
                isPressable
                onPress={onClick}
                className="w-full h-full bg-linear-to-br from-[#5D4037] to-[#3E2723] border border-[#8D6E63]/30 rounded-4xl overflow-visible shadow-2xl group"
            >
                <CardBody className="p-0 overflow-visible z-10">
                    <div className="relative h-48 w-full -mt-4 px-4">
                        {/* Image Container with Glow */}
                        <div className="absolute inset-0 bg-[#FFB74D]/10 blur-2xl rounded-full scale-90 group-hover:scale-105 transition-transform duration-500" />

                        <Image
                            src={store.imageUrl}
                            alt={store.name}
                            className="w-full h-full object-cover rounded-2xl shadow-xl transform group-hover:scale-105 transition-transform duration-500"
                            classNames={{
                                wrapper: "w-full h-full !max-w-none",
                                img: "w-full h-full object-cover rounded-2xl aspect-video"
                            }}
                        />

                        {/* Status Badge */}
                        <div
                            className={`absolute top-2 right-6 px-3 py-1 rounded-full text-xs font-bold shadow-lg backdrop-blur-md border ${
                                store.isOpenNow
                                    ? "bg-green-500/20 border-green-400 text-green-300"
                                    : "bg-red-500/20 border-red-400 text-red-300"
                            }`}
                        >
                            {store.isOpenNow ? "ĐANG MỞ CỬA" : "ĐÃ ĐÓNG CỬA"}
                        </div>

                        {/* Distance Badge */}
                        {distance !== null && distance !== undefined && (
                            <div className="absolute bottom-2 right-6 px-3 py-1 rounded-full text-xs font-bold bg-[#FFB74D] text-[#3E2723] shadow-lg flex items-center gap-1">
                                <MapPin size={12} />
                                {distance.toFixed(1)} km
                            </div>
                        )}
                    </div>
                </CardBody>

                <CardFooter className="flex flex-col items-start px-6 pb-6 pt-4 gap-3 z-20">
                    <div className="w-full">
                        <h3 className="text-xl font-bold text-white mb-1 line-clamp-1 group-hover:text-[#FFB74D] transition-colors">
                            {store.name}
                        </h3>
                        <div className="mx-auto w-30 h-1 bg-[#FFB74D] rounded-full mt-2 mb-3" />
                    </div>

                    <div className="space-y-2 w-full text-sm text-[#D7CCC8]">
                        <div className="flex items-start gap-2">
                            <MapPin size={16} className="text-[#FFB74D] mt-0.5 shrink-0" />
                            <span className="line-clamp-2">{store.address}</span>
                        </div>

                        <div className="flex items-center gap-2">
                            <Phone size={16} className="text-[#FFB74D] shrink-0" />
                            <span>{formatPhoneNumber(store.phone)}</span>
                        </div>

                        <div className="flex items-center gap-2">
                            <Clock size={16} className="text-[#FFB74D] shrink-0" />
                            <span>Mở cửa 24/7</span>
                        </div>
                    </div>

                    <div className="w-full mt-2 bg-white/10 group-hover:bg-[#FFB74D] text-[#FFB74D] group-hover:text-[#3E2723] font-bold rounded-xl transition-all border border-[#FFB74D]/30 py-2.5 px-4 text-center cursor-pointer">
                        Xem Chi Tiết
                    </div>
                </CardFooter>
            </Card>
        </motion.div>
    )
}
