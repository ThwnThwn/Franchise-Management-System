import { User as HeroUser } from "@heroui/react"
import { useUserHelper } from "@/hooks"

export const UserCell = ({ user, userId }: { user: any; userId: string }) => {
    const { toFullName } = useUserHelper()

    if (!user)
        return (
            <span className="text-xs text-default-400 font-mono italic">
                ID: {userId.substring(0, 8)}...
            </span>
        )

    return (
        <HeroUser
            name={toFullName(user)}
            description={user.email}
            avatarProps={{ src: user.avatarUrl, size: "sm" }}
        />
    )
}
