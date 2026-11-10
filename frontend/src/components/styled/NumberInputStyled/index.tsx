"use client"

import { NumberInput, NumberInputProps, cn } from "@heroui/react"

export function NumberInputStyled(props: NumberInputProps) {
    return (
        <NumberInput
            variant="bordered"
            {...(props as any)}
            className={cn(props.className)}
            classNames={{
                ...props.classNames,
                inputWrapper: cn(
                    "data-[hover=true]:border-primary data-[focus=true]:border-primary",
                    props.classNames?.inputWrapper
                ),
                label: cn("text-gray-700", props.classNames?.label),
                input: cn("text-gray-900", props.classNames?.input)
            }}
            hideStepper={true}
        />
    )
}
