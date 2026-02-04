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

export interface RolePermissionGroupRes {
    resource: string
    permissions: RolePermissionRes[]
}

export interface RolePermissionRes {
    id: string
    slug: string
    name: string
    description: string
    isActived: boolean
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
