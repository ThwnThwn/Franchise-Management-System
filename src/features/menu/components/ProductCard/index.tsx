"use client"
import { CardStyled, ImageStyled } from "@/components"
import { CardBody, CardFooter } from "@heroui/react"
import React from "react"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { MENU_TYPE } from "../../constants"
import { Product, ProductSize } from "../../models/products/respone"
import { motion } from "framer-motion"

export function ProductCard({ product, onClick }: { product: Product; onClick?: () => void }) {
    return (
        <motion.div
            key={product.id}
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
            onClick={onClick}
            // onKeyDown={(e) => {
            //     if (e.key === "Enter" || e.key === " ") {
            //         e.preventDefault()
            //         handleOpenProduct(product)
            //     }
            // }}
            className="cursor-pointer"
        >
            <CardStyled className="hover:shadow-xl interactive-scale w-xs" shadow="sm">
                <CardBody className="overflow-visible ">
                    <ImageStyled
                        alt={product.name}
                        className="object-cover h-47 w-72 shadow-lg"
                        src={product.imageUrl}
                    />
                    {product.isActive === false && (
                        <span className="absolute top-3 right-3 bg-red-500 text-white px-3 py-1 text-xs rounded z-10">
                            {"fleet.status_unavailable"}
                        </span>
                    )}
                </CardBody>
                <CardFooter className="flex flex-col text-small justify-between">
                    <div className="flex justify-between items-center text-xl mb-2">
                        <b>{product.name}</b>
                    </div>
                    {/* </div> */}
                    <hr className=" text-gray-300 border-1 w-full m-1" />

                    <div className=" flex items-center justify-center my-2">
                        <span className="text-xl font-bold text-primary whitespace-nowrap">
                            {formatCurrencyWithSymbol(
                                product.sizes.find(
                                    (s: ProductSize) => s.name === MENU_TYPE.MIN_SIZE
                                )?.basePrice ?? 0
                            )}
                        </span>
                    </div>
                </CardFooter>
            </CardStyled>
        </motion.div>
    )
}
