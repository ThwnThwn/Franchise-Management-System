"use client"

import React from "react"
import { mergeClasses, Select, SelectItem } from "@heroui/react"
import type { SelectProps } from "@heroui/react"
import { Funnel } from "lucide-react"

type SlotOverrides<T> = NonNullable<SelectProps<T extends object ? T : object>["classNames"]>

const defaultClassnames: SlotOverrides<any> = {
    base: "",
    trigger:
        "group h-12 px-5 border border-slate-200 bg-white text-slate-800 transition-colors data-[hover=true]:border-slate-300 data-[focus=true]:border-primary data-[focus-visible=true]:border-primary data-[open=true]:border-primary",
    listboxWrapper: "",
    popoverContent: "",
    label: "",
    //labelWrapper: "",
    //topContent: "",
    //bottomContent: "",
    value: "text-slate-800",
    helperWrapper: "",
    errorMessage: "",
    description: "",
    selectorIcon:
        "text-slate-500 transition-colors group-data-[focus=true]:text-primary group-data-[open=true]:text-primary",
    innerWrapper: "",
    //startContent: "",
    endContent: ""
}

export type FilterTypeStyleProps<T extends object = object> = Omit<
    SelectProps<T>,
    "variant" | "size" | "radius"
> & {
    classNames?: SlotOverrides<T>
    showIcon?: boolean
}

export function FilterTypeStyle<T extends object = object>({
    classNames,
    showIcon = true,
    startContent,
    ...props
}: FilterTypeStyleProps<T>) {
    const mergedClassNames = mergeClasses(defaultClassnames, classNames)
    const defaultIcon = (
        <Funnel
            className="h-5 w-5 text-slate-400 transition-colors group-data-[focus=true]:text-primary group-data-[open=true]:text-primary"
            aria-hidden
            data-slot="filter-icon"
        />
    )

    return (
        <Select
            {...props}
            variant="bordered"
            size="md"
            radius="lg"
            startContent={startContent ?? (showIcon ? defaultIcon : undefined)}
            classNames={mergedClassNames}
        />
    )
}
export const FilterTypeOption = SelectItem
