import { SpinnerStyled } from "@/components"
import { calculateDistance } from "@/utils/helpers/locationHelper"
import { useMemo } from "react"
import { StoreDetailDto } from "../../models/schema/response"
import StoreCard from "../StoreCard"

type StoreListProps = {
    stores: StoreDetailDto[]
    loading: boolean
    onStoreClick: (store: StoreDetailDto) => void
    userLocation?: { latitude: number; longitude: number } | null
    emptyMessage?: string
}

export default function StoreList({
    stores,
    loading,
    onStoreClick,
    userLocation,
    emptyMessage = "Không tìm thấy cửa hàng nào"
}: StoreListProps) {
    const sortedStores = useMemo(() => {
        // Nếu không có userLocation, trả về list gốc hoặc list mặc định
        if (!userLocation)
            return [...stores].map((store) => {
                const distance = 0
                return { ...store, calculatedDistance: distance }
            })

        return [...stores]
            .map((store) => {
                const distance =
                    store.latitude && store.longitude
                        ? calculateDistance(
                              userLocation.latitude,
                              userLocation.longitude,
                              store.latitude,
                              store.longitude
                          )
                        : Infinity // Nếu không có tọa độ thì cho ra "vô cực" để bay xuống cuối list

                return { ...store, calculatedDistance: distance }
            })
            .sort((a, b) => a.calculatedDistance - b.calculatedDistance)
    }, [stores, userLocation])

    if (loading) {
        return <SpinnerStyled />
    }

    if (stores.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 min-h-[50vh] text-[#BCAAA4]">
                <div className="w-24 h-24 bg-[#5D4037]/30 rounded-full flex items-center justify-center mb-6">
                    <svg
                        className="w-12 h-12 opacity-50"
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
                </div>
                <p className="text-xl font-medium">{emptyMessage}</p>
            </div>
        )
    }

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {sortedStores.map((store) => (
                <StoreCard
                    key={store.id}
                    store={store}
                    // calculatedDistance đã có sẵn trong object sau khi map ở trên
                    distance={
                        store.calculatedDistance === Infinity ? null : store.calculatedDistance
                    }
                    onClick={() => onStoreClick(store)}
                />
            ))}
        </div>
    )
}
