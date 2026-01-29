export type BackendError = {
    title?: string
    status?: number
    detail?: string
}

export type PageResult<T> = {
    items: T[]
    pageNumber: number
    pageSize: number
    total: number
    totalPages: number
}
