import { useEffect, useState } from "react"
import { storeApi } from "../services"
import { StoreDetailDto } from "../models/schema/response"

export const useStoreDetail = (storeId: string | null) => {
    const [data, setData] = useState<StoreDetailDto | null>(null)
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!storeId) {
            setData(null)
            return
        }

        const fetchStoreDetail = async () => {
            setLoading(true)
            setError(null)
            try {
                const result = await storeApi.getDetail(storeId)
                setData(result)
            } catch (err) {
                setError(err instanceof Error ? err.message : "Failed to fetch store detail")
            } finally {
                setLoading(false)
            }
        }

        fetchStoreDetail()
    }, [storeId])

    return { data, loading, error }
}
