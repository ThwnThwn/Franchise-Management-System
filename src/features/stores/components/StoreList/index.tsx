import React from "react"
import { StoreDetailDto } from "../../models/schema/response"
import StoreCard from "../StoreCard"
import { calculateDistance } from "@/utils/helpers/locationHelper"

type StoreListProps = {
    stores: StoreDetailDto[]
    loading: boolean
    onStoreClick: (store: StoreDetailDto) => void
    userLocation?: { latitude: number; longitude: number } | null
}

export default function StoreList({ stores, loading, onStoreClick, userLocation }: StoreListProps) {
    if (loading) {
        return (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {[...Array(6)].map((_, index) => (
                    <div key={index} className="bg-gray-200 rounded-lg h-80 animate-pulse" />
                ))}
            </div>
        )
    }

    if (stores.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-16 min-h-[60vh]">
                <svg
                    className="w-24 h-24 text-gray-300 mb-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                >
                    <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={1.5}
                        d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                    />
                </svg>
                <p className="text-gray-500 text-lg">Không tìm thấy cửa hàng nào</p>
            </div>
        )
    }

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {stores.map((store) => {
                const distance =
                    userLocation && store.latitude && store.longitude
                        ? calculateDistance(
                              userLocation.latitude,
                              userLocation.longitude,
                              store.latitude,
                              store.longitude
                          )
                        : null

                return (
                    <StoreCard
                        key={store.id}
                        store={store}
                        distance={distance}
                        onClick={() => onStoreClick(store)}
                    />
                )
            })}
        </div>
    )
}
