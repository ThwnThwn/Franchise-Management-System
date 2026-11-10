export type CreateSupplierRequest = {
    code: string
    name: string
    contactPerson?: string
    phone?: string
    email?: string
    address?: string
    taxCode?: string
}

export type UpdateSupplierRequest = {
    id: string
    code: string
    name?: string
    contactPerson?: string
    phone?: string
    email?: string
    address?: string
    taxCode?: string
    isActive?: boolean
}

export type GetSuppliersParams = {
    code?: string
    isActive?: boolean
    page?: number
    pageSize?: number
    name?: string
}
