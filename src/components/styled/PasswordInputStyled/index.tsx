"use client"

import { useState } from "react"
import { Eye, EyeOff } from "lucide-react"
import { ComponentProps } from "react"
import { InputStyled } from "../InputStyled"

type InputStyledProps = ComponentProps<typeof InputStyled>

export const PasswordInputStyled = (props: InputStyledProps) => {
    const [isVisible, setIsVisible] = useState(false)
    const toggleVisibility = () => setIsVisible(!isVisible)

    return (
        <InputStyled
            {...props}
            type={isVisible ? "text" : "password"}
            endContent={
                <button
                    className="focus:outline-none p-1 hover:bg-default-100 rounded-full transition-colors"
                    type="button"
                    onClick={toggleVisibility}
                    aria-label="toggle password visibility"
                >
                    {isVisible ? (
                        <EyeOff className="text-default-400" size={20} />
                    ) : (
                        <Eye className="text-default-400" size={20} />
                    )}
                </button>
            }
        />
    )
}
