"use client"
import * as React from "react"
import { Textarea, type TextAreaProps, cn } from "@heroui/react"

export const TextareaStyled = React.forwardRef<React.ElementRef<typeof Textarea>, TextAreaProps>(
    ({ className, classNames, variant = "bordered", color = "default", ...rest }, ref) => (
        <Textarea
            ref={ref}
            variant={variant}
            color={color}
            {...rest}
            className={cn("w-full", className)}
            classNames={{
                ...classNames,
                inputWrapper: cn(
                    "data-[hover=true]:border-primary data-[focus=true]:border-primary",
                    classNames?.inputWrapper
                ),
                label: cn("text-gray-700", classNames?.label),
                input: cn("text-gray-900", classNames?.input)
            }}
        />
    )
)

TextareaStyled.displayName = "TextareaStyled"
