export interface CreateRoleRequest {
    name: string
    slug: string
    description: string
}

export interface UpdateRoleRequest {
    name?: string
    description?: string
}

export interface UpdateRolePermissionsRequest {
    permissionsToGrant: string[]
    permissionsToRevoke: string[]
}

export interface UpdatePermissionInfoRequest {
    name?: string
    description?: string
}
