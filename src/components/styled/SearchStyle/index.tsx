"use client"

import React from "react"
import { Input, mergeClasses } from "@heroui/react"
import type { InputProps } from "@heroui/react"
import { Search } from "lucide-react"

type SlotOverrides = NonNullable<InputProps["classNames"]>

const defaultClassnames: SlotOverrides = {
    base: "",
    label: "",
    input: "text-sm",
    innerWrapper: "",
    mainWrapper: "",
    helperWrapper: "",
    inputWrapper:
        "h-12 px-5 border border-slate-200 bg-white text-slate-800 transition-colors data-[hover=true]:border-slate-300 data-[focus=true]:border-primary data-[focus-visible=true]:border-primary",
    clearButton: "",
    description: "",
    errorMessage: ""
}

export type SearchStyleProps = Omit<InputProps, "variant" | "size" | "radius"> & {
    classNames?: SlotOverrides
    showIcon?: boolean
}

export function SearchStyle({
    classNames,
    showIcon = true,
    startContent,
    ...props
}: SearchStyleProps) {
    const mergedClassNames = mergeClasses(defaultClassnames, classNames)
    const defaultIcon = (
        <Search
            className="h-5 w-5 text-slate-400 transition-colors group-data-[focus=true]:text-primary group-data-[focus-visible=true]:text-primary"
            aria-hidden
            data-slot="search-icon"
        />
    )
    return (
        <Input
            {...(props as any)}
            variant="bordered"
            size="md"
            radius="lg"
            startContent={startContent ?? (showIcon ? defaultIcon : undefined)}
            classNames={mergedClassNames}
        />
    )
}
