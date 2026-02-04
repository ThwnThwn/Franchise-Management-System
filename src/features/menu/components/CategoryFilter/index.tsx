"use client"
import React from "react"
import { ScrollShadow, Button } from "@heroui/react"
import { Coffee, IceCream, Utensils, CupSoda, Sandwich, Cake, Pizza, GlassWater } from "lucide-react"

type Category = {
    id: string
    name: string
}

type CategoryFilterProps = {
    categories: Category[]
    selectedId: string | null
    onSelect: (id: string | null) => void
}

export function CategoryFilter({ categories, selectedId, onSelect }: CategoryFilterProps) {
    // Map icons based on category name (simple heuristic)
    const getIcon = (name: string) => {
        const lower = name.toLowerCase()
        const iconProps = { size: 24 }

        if (lower.includes("cà phê") || lower.includes("coffee") || lower.includes("cafe")) return <Coffee {...iconProps} />
        if (lower.includes("trà") || lower.includes("tea") || lower.includes("matcha")) return <CupSoda {...iconProps} />
        if (lower.includes("kem") || lower.includes("ice cream") || lower.includes("ice")) return <IceCream {...iconProps} />
        if (lower.includes("bánh") || lower.includes("bakery") || lower.includes("cake") || lower.includes("dessert")) return <Cake {...iconProps} />
        if (lower.includes("pizza")) return <Pizza {...iconProps} />
        if (lower.includes("nước") || lower.includes("juice") || lower.includes("water") || lower.includes("sinh tố")) return <GlassWater {...iconProps} />
        if (lower.includes("thức ăn") || lower.includes("food") || lower.includes("món")) return <Utensils {...iconProps} />

        return <Utensils {...iconProps} />
    }

    return (
        <div className="w-full">
            <ScrollShadow orientation="horizontal" className="w-full pb-4" hideScrollBar>
                <div className="flex gap-4 items-start px-2">
                    {/* All Option */}
                    <div className="flex flex-col items-center gap-2 min-w-[70px]">
                        <Button
                            isIconOnly
                            radius="full"
                            variant={selectedId === null ? "solid" : "flat"}
                            color={selectedId === null ? "primary" : "default"}
                            className={`w-14 h-14 transition-transform active:scale-95`}
                            onPress={() => onSelect(null)}
                        >
                            <Utensils size={24} />
                        </Button>
                        <span
                            className={`text-xs font-bold uppercase transition-colors ${selectedId === null ? "text-primary" : "text-gray-400"
                                }`}
                        >
                            Tất cả
                        </span>
                    </div>

                    {categories.map((cat) => {
                        const isSelected = selectedId === cat.id
                        return (
                            <div key={cat.id} className="flex flex-col items-center gap-2 min-w-[70px]">
                                <Button
                                    isIconOnly
                                    radius="full"
                                    variant={isSelected ? "solid" : "flat"}
                                    color={isSelected ? "primary" : "default"}
                                    className={`w-14 h-14 transition-transform active:scale-95`}
                                    onPress={() => onSelect(cat.id)}
                                >
                                    {getIcon(cat.name)}
                                </Button>
                                <span
                                    className={`text-xs font-bold uppercase whitespace-nowrap transition-colors ${isSelected ? "text-primary" : "text-gray-400"
                                        }`}
                                >
                                    {cat.name}
                                </span>
                            </div>
                        )
                    })}
                </div>
            </ScrollShadow>
        </div>
    )
}
