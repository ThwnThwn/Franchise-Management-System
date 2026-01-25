import React from "react"
import { motion, AnimatePresence } from "framer-motion"
// import { useGetAllProducts } from "../hooks"

import { CardProductStyled } from "../components/productCard/indes"
import { MOCK_PRODUCTS_DATA } from "@/utils/mockData"
import Link from "next/link"

export default function MenuPage() {
    // const { data: products, isLoading } = useGetAllProducts({})
    const products = MOCK_PRODUCTS_DATA
    // const [isSearching, setIsSearching] = useState(false)
    const isEmpty = !products || products.Products.length === 0
    // if (isLoading) return <SpinnerStyled />

    return (
        <div className="min-h-[80vh] max-w-screen-xl mx-auto p-6 text-center">
            {/* <motion.div
                initial={{ opacity: 0, y: -20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, ease: "easeOut" }}
            >
                <FilterVehicleRental setIsSearching={setIsSearching} isStaff={isStaff} />
            </motion.div> */}

            <div className="mt-10">
                <AnimatePresence mode="wait">
                    {
                        // isSearching ? (
                        //     <motion.div
                        //         key="loading"
                        //         initial={{ opacity: 0 }}
                        //         animate={{ opacity: 1 }}
                        //         exit={{ opacity: 0 }}
                        //         transition={{ duration: 0.3 }}
                        //         className="flex justify-center items-center mt-6"
                        //     >
                        //         <Spinner size="lg" color="primary" />
                        //     </motion.div>
                        // ) :
                        isEmpty ? (
                            <motion.div
                                key="empty"
                                initial={{ opacity: 0, scale: 0.95 }}
                                animate={{ opacity: 1, scale: 1 }}
                                exit={{ opacity: 0 }}
                                transition={{ duration: 0.4 }}
                                className="text-gray-500 mt-10 text-base"
                            >
                                {"vehicle_filter.no_suitable_vehicle_found"}
                            </motion.div>
                        ) : (
                            <motion.div
                                key="grid"
                                initial="hidden"
                                animate="visible"
                                variants={{
                                    hidden: { opacity: 0, y: 20 },
                                    visible: {
                                        opacity: 1,
                                        y: 0,
                                        transition: {
                                            staggerChildren: 0.05,
                                            duration: 0.5
                                        }
                                    }
                                }}
                                className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8"
                            >
                                {products.Products.map((product) => (
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
                                    >
                                        <Link href={`https://www.facebook.com/`}>
                                            <CardProductStyled getProductListItemDto={product} />
                                        </Link>
                                    </motion.div>
                                ))}
                            </motion.div>
                        )
                    }
                </AnimatePresence>
            </div>
        </div>
    )
}
