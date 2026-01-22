import React from "react"
import { ButtonStyled } from "@/components"
import { ButtonProps, cn } from "@heroui/react"

export function ButtonIconStyled(props: ButtonProps) {
    return <ButtonStyled {...props} className={cn("p-3 min-w-fit", props.className)} />
}
