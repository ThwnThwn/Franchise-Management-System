"use client"
import { Input, InputProps, cn } from "@heroui/react"
import React from "react"

interface InputStyledPropes extends InputProps {
    isIncludeTax?: boolean
}

export function InputStyled(props: InputStyledPropes) {
    // const mergedClassNames = mergeClassNames(props.classNames)
    return (
        <Input
            variant="bordered"
            endContent={
                props.isIncludeTax ? (
                    <div className="pointer-events-none flex items-center">
                        <span className="text-default-400 text-3xl">*</span>
                    </div>
                ) : (
                    <></>
                )
            }
            {...props}
            className={cn(props.className)}
            // classNames={mergedClassNames}
            classNames={{
                ...props.classNames,
                inputWrapper: cn(
                    "data-[hover=true]:border-primary data-[focus=true]:border-primary",
                    props.classNames?.inputWrapper
                ),
                label: cn("text-gray-700", props.classNames?.label),
                input: cn("text-gray-900", props.classNames?.input)
            }}
        />
    )
}
