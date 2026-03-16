export type SupplierItem = {
    id: string
    code: string
    name: string
    contactPerson: string
    email: string
    address: string
    taxCode: string
    phone: string
    isActive: boolean
}

export type SupplierDetail = {
    id: string
    code: string
    name: string
    contactPerson: string
    phone: string
    email: string
    address: string
    taxCode: string
    isActive: boolean
    createdAt: string
    updatedAt: string
}

export type GetSuppliersResult = {
    items: SupplierItem[]
    totalCount: number
}
