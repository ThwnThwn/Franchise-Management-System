import { Icon } from "@iconify/react"
import React from "react"

type ButtonToggleVisibilityProps = {
    toggleVisibility: () => void
    isVisible: boolean
}

export function ButtonToggleVisibility({
    toggleVisibility,
    isVisible
}: ButtonToggleVisibilityProps) {
    return (
        <button
            tabIndex={-1}
            aria-label="toggle password visibility"
            className="focus:outline-solid outline-transparent"
            type="button"
            onClick={toggleVisibility}
        >
            {isVisible ? (
                <Icon
                    className="text-default-400 pointer-events-none text-2xl"
                    icon="solar:eye-closed-linear"
                />
            ) : (
                <Icon
                    className="text-default-400 pointer-events-none text-2xl"
                    icon="solar:eye-bold"
                />
            )}
        </button>
    )
}
