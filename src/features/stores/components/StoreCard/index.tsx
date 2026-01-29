import { StoreDetailDto } from "../../models/schema/response"
import Image from "next/image"

type StoreCardProps = {
    store: StoreDetailDto
    distance?: number | null
    onClick: () => void
}

export default function StoreCard({ store, distance, onClick }: StoreCardProps) {
    return (
        <div
            onClick={onClick}
            className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow cursor-pointer overflow-hidden"
        >
            <div className="relative h-48 w-full">
                <Image
                    src={store.imageUrl || "/placeholder-store.jpg"}
                    alt={store.name}
                    fill
                    className="object-cover"
                />
                {store.isOpenNow && (
                    <div className="absolute top-2 right-2 bg-green-500 text-white px-3 py-1 rounded-full text-sm font-semibold">
                        Đang mở cửa
                    </div>
                )}
                {!store.isOpenNow && (
                    <div className="absolute top-2 right-2 bg-gray-500 text-white px-3 py-1 rounded-full text-sm font-semibold">
                        Đã đóng cửa
                    </div>
                )}
            </div>

            <div className="p-4">
                <h3 className="text-lg font-bold text-gray-900 mb-2">{store.name}</h3>
                <p className="text-sm text-gray-600 mb-2 line-clamp-2">{store.address}</p>
                <p className="text-sm text-gray-500 mb-2">{store.phone}</p>

                {distance !== null && distance !== undefined && (
                    <div className="flex items-center text-sm text-yellow-700 font-medium">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            className="h-4 w-4 mr-1"
                            fill="none"
                            viewBox="0 0 24 24"
                            stroke="currentColor"
                        >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                            />
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                            />
                        </svg>
                        <span>{distance.toFixed(1)} km</span>
                    </div>
                )}
            </div>
        </div>
    )
}
