import { useEffect, useState } from "react"
import { storeApi } from "../services"
import { GetStoresParams } from "../models/schema/request"
import { GetStoresResponseDto } from "../models/schema/response"

export const useStores = (params: GetStoresParams) => {
    const [data, setData] = useState<GetStoresResponseDto | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        const fetchStores = async () => {
            setLoading(true)
            setError(null)
            try {
                const result = await storeApi.getAll(params)
                setData(result)
            } catch (err) {
                setError(err instanceof Error ? err.message : "Failed to fetch stores")
            } finally {
                setLoading(false)
            }
        }

        fetchStores()
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [params.page, params.pageSize, params.keyword])

    return { data, loading, error, refetch: () => {} }
}
