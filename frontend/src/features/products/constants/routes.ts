export const PRODUCTS_ROUTES = {
    LIST: "/dashboard/products",
    CREATE: "/dashboard/products/create",
    DETAIL: (id: string) => `/dashboard/products/${id}`, // Dùng hàm cho params động
    EDIT: (id: string) => `/dashboard/products/${id}` // Update chung trang với Detail
}
