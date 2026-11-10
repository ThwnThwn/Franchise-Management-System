"use client"

import React from "react"
import { Chip } from "@heroui/react"
import { RecipeViewRes } from "@/features/products/models/recipes/response"
import { Info } from "lucide-react"

interface Props {
    variantId: string
    recipes: RecipeViewRes[]
}

export const RecipeIngredientList = ({ variantId, recipes }: Props) => {
    const ingredients = recipes.filter((r) => r.variantId === variantId)
    const hasIngredients = ingredients.length > 0

    return (
        <div className="w-60 grid grid-cols-1 gap-2 bg-default-50 p-3 rounded-xl border border-dashed border-default-200">
            {hasIngredients ? (
                // --- TRƯỜNG HỢP CÓ DỮ LIỆU ---
                ingredients.map((ing, index) => (
                    <div
                        key={index}
                        className="flex gap-2 justify-between items-center text-xs animate-in fade-in duration-300"
                    >
                        <span className="text-slate-600 font-medium">• {ing.name}</span>
                        <Chip
                            size="sm"
                            variant="bordered"
                            color="primary"
                            className="h-5 font-bold tabular-nums"
                        >
                            {ing.quantity} {ing.baseUnit}
                        </Chip>
                    </div>
                ))
            ) : (
                // --- TRƯỜNG HỢP TRỐNG (EMPTY CONTENT) ---
                <div className="flex items-center gap-2 text-default-400 py-1">
                    <Info size={14} className="shrink-0" />
                    <span className="text-[10px] font-medium italic">
                        Món này chưa cập nhật nguyên liệu chi tiết.
                    </span>
                </div>
            )}
        </div>
    )
}
