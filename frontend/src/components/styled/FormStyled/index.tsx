"use client"
import React from "react"
import { cn, Form, FormProps } from "@heroui/react"

export function FormStyled(props: FormProps) {
    return <Form {...props} className={cn("space-y-4", props.className)} />
}
