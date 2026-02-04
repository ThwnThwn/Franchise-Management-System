import { CardStyled, ImageStyled } from "@/components"
import React from "react"
import { StoreDetailDto } from "../../models/schema/response"
import { motion } from "framer-motion"
import { formatPhoneNumber } from "@/utils/helpers/stringHelper"
import { CardBody, CardFooter } from "@heroui/react"
import { MapPin, Phone } from "lucide-react"

type StoreCardProps = {
    store: StoreDetailDto
    distance?: number | null
    onClick?: () => void
}

export default function StoreCard({ store, distance, onClick }: StoreCardProps) {
    return (
        <motion.div
            key={store.id}
            variants={{
                hidden: { opacity: 0, y: 20 },
                visible: { opacity: 1, y: 0 }
            }}
            whileHover={{
                scale: 1.03,
                transition: { duration: 0.2 }
            }}
            role="button"
            tabIndex={0}
            onClick={() => onClick?.()}
            className="cursor-pointer h-full relative overflow-visible"
        >
            <CardStyled
                // onClick={onClick}
                className="w-fit overflow-visible py-1 px-2 rounded-3xl"
                // className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow cursor-pointer overflow-hidden"
            >
                <CardBody className="overflow-visible mb-0 pb-0">
                    <div className="h-40 w-50 overflow-visible">
                        <ImageStyled
                            src={store.imageUrl}
                            alt={store.name}
                            className="h-40 w-50 mx-auto rounded-4xl"
                        />
                        <div
                            className={`absolute -top-4 right-2 px-3 py-1 rounded-full text-xs font-bold shadow-lg bg-white border ${
                                store.isOpenNow
                                    ? "border-success-400 text-success-300"
                                    : "border-danger-400 text-danger-300"
                            }`}
                        >
                            {store.isOpenNow ? "ĐANG MỞ CỬA" : "ĐÃ ĐÓNG CỬA"}
                        </div>
                        {distance !== null && distance !== undefined && (
                            <div className="absolute -bottom-2 right-6 px-3 py-1 rounded-full text-xs font-bold bg-[#FFB74D] text-[#3E2723] shadow-lg flex items-center gap-1">
                                <MapPin size={12} />
                                {distance.toFixed(1)} km
                            </div>
                        )}
                        {/* {store.isOpenNow && (
                            <div className="absolute top-2 right-2 bg-green-500 text-white px-3 py-1 rounded-full text-sm font-semibold">
                                Đang mở cửa
                            </div>
                        )}
                        {!store.isOpenNow && (
                            <div className="absolute top-2 right-2 bg-gray-500 text-white px-3 py-1 rounded-full text-sm font-semibold">
                                Đã đóng cửa
                            </div>
                        )} */}
                    </div>
                </CardBody>

                <CardFooter>
                    <div className="w-50">
                        <div className="w-full text-center">
                            <h3 className="text-xl font-bold mb-1 line-clamp-1 group-hover:text-[#FFB74D] transition-colors">
                                {store.name}
                            </h3>
                            <div className="mx-auto w-30 h-1 bg-[#FFB74D] rounded-full mt-2 mb-3" />
                        </div>
                        <div className="space-y-2 w-full text-sm">
                            <div className="flex items-start gap-2">
                                <MapPin size={16} className="mt-0.5 shrink-0" />
                                <span className="line-clamp-2">{store.address}</span>
                            </div>

                            <div className="flex items-center gap-2">
                                <Phone size={16} className="shrink-0" />
                                <span>{formatPhoneNumber(store.phone)}</span>
                            </div>
                            {/* 
                            <div className="flex items-center gap-2">
                                <Clock size={16} className="text-[#FFB74D] shrink-0" />
                                <span>Mở cửa 24/7</span>
                            </div> */}
                        </div>
                    </div>
                </CardFooter>
            </CardStyled>
        </motion.div>
    )
}
