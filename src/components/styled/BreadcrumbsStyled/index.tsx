"use client"
import { BreadcrumbItem, Breadcrumbs } from "@heroui/react"
import Link from "next/link"
import React from "react"

type Crumb = {
    label: string
    href: string
}

type BreadCrumbsStyledProps = {
    items: Crumb[]
}

export function BreadCrumbsStyled({ items }: BreadCrumbsStyledProps) {
    return (
        <Breadcrumbs>
            {items.map((item, idx) => (
                <BreadcrumbItem key={idx}>
                    <Link href={item.href}>{item.label}</Link>
                </BreadcrumbItem>
            ))}
        </Breadcrumbs>
    )
}
