"use client"

import React, { useState, useCallback } from "react"
import Cropper, { Area, MediaSize, Point } from "react-easy-crop"
import { SliderStyled } from "@/components/styled/SliderStyled"
import { cn } from "@heroui/react"

type ImageCropperProps = {
    imgSrc: string
    aspect?: number
    cropShape?: "rect" | "round"
    cropSize: { width: number; height: number }
    onCropComplete: (area: Area) => void
    className?: string
}

export function ImageCropper({
    imgSrc,
    aspect = 1,
    cropShape = "rect",
    cropSize,
    onCropComplete,
    className
}: ImageCropperProps) {
    const [crop, setCrop] = useState<Point>({ x: 0, y: 0 })
    const [zoom, setZoom] = useState(1)
    const [minZoom, setMinZoom] = useState(1)
    const maxZoom = 10

    const handleMediaLoaded = useCallback(
        (mediaSize: MediaSize) => {
            const { width, height } = mediaSize
            const containerW = cropSize.width || 300
            const containerH = cropSize.height || 300

            const ratioX = containerW / width
            const ratioY = containerH / height
            const newMinZoom = Math.max(ratioX, ratioY, 1)

            setMinZoom(newMinZoom)
            setZoom(newMinZoom)
        },
        [cropSize]
    )

    return (
        <div
            style={{ width: cropSize.width, height: cropSize.height }}
            className={cn("relative bg-gray-900 rounded-xl mx-auto max-w-full", className)}
        >
            <Cropper
                key={imgSrc}
                image={imgSrc}
                crop={crop}
                zoom={zoom}
                aspect={aspect}
                cropSize={cropSize}
                cropShape={cropShape}
                onCropChange={setCrop}
                onZoomChange={(z) => setZoom(Math.max(z, minZoom))}
                onCropComplete={(_, areaPixels) => onCropComplete(areaPixels)}
                onMediaLoaded={handleMediaLoaded}
                restrictPosition={true}
                minZoom={minZoom}
                maxZoom={maxZoom}
                objectFit="contain"
            />

            {/* 3. Slider: Ẩn trên mobile (hidden), Hiện trên Desktop (sm:flex) */}
            <div className={cn("absolute top-0 -right-8 h-full items-center", "hidden sm:flex")}>
                <SliderStyled
                    minValue={minZoom}
                    maxValue={maxZoom}
                    step={0.1}
                    value={zoom}
                    orientation="vertical"
                    onChange={(value) => setZoom(value as number)}
                    className="h-[70%] radian"
                />
            </div>
        </div>
    )
}
