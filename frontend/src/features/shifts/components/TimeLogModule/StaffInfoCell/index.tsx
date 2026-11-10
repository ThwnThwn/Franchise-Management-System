import React from "react"
import { User } from "@heroui/react"
import { useUserHelper } from "@/hooks"

interface Props {
    staff: {
        firstName?: string
        lastName?: string
        avatarUrl?: string
        email?: string
    }
}

export const StaffInfoCell = ({ staff }: Props) => {
    const { toFullName } = useUserHelper()
    return (
        <User
            name={toFullName({
                firstName: staff.firstName,
                lastName: staff.lastName
            })}
            description={staff.email}
            avatarProps={{
                src: staff.avatarUrl,
                size: "sm",
                isBordered: true
            }}
        />
    )
}
