"use client"

import React from "react"
import HeroSection from "@/components/home/HeroSection"
import FeaturedSection from "@/components/home/FeaturedSection"
import StoreHighlight from "@/components/home/StoreHighlight"


export default function Home() {
    return (
        <div className="flex flex-col w-full">
            <HeroSection />
            <FeaturedSection />
            <StoreHighlight />
        </div>
    )
}
