"use client"

import React from "react"
import { Slider, SliderProps } from "@heroui/react"

export function SliderStyled(props: SliderProps) {
    return <Slider color="primary" size="md" {...props} />
}
