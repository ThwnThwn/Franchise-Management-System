export interface RoleUserResponse {
    id: string
    name: string
}

export interface RoleResponse {
    id: string
    name: string
    slug: string
    description: string
    createdAt?: string
    updatedAt?: string
}

export interface PermissionGroup {
    resource: string
    permissions: PermissionDetail[]
}

export interface PermissionDetail {
    id: string
    slug: string
    name: string
    description: string
}
