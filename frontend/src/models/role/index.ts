import { RoleSlug } from "@/constants/enums"

export interface RoleUserResponse {
    id: string
    name: string
    slug: RoleSlug
}
