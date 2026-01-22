"use client"

import { Checkbox, CheckboxProps } from "@heroui/react"
import React from "react"

export interface CheckboxStyledProps extends CheckboxProps {}

export function CheckboxStyled(props: CheckboxStyledProps) {
    return <Checkbox {...props} />
}
