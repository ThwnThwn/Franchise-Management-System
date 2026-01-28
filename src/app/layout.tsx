import type { Metadata } from "next"
import { Geist, Geist_Mono } from "next/font/google"
import "./globals.css"
import React from "react"
import { AppProviders } from "@/providers"
import { ClientHydration, Navbar, Modals } from "@/components"
import Footer from "@/components/shared/Footer"

const geistSans = Geist({
    variable: "--font-geist-sans",
    subsets: ["latin"]
})

const geistMono = Geist_Mono({
    variable: "--font-geist-mono",
    subsets: ["latin"]
})

export const metadata: Metadata = {
    title: "Cà Hong Bạn",
    description: "Cà hong bạn"
}

export default function RootLayout({
    children
}: Readonly<{
    children: React.ReactNode
}>) {
    return (
        <html lang="en" className={`${geistSans.variable} ${geistMono.variable} min-w-xs`}>
            <body
                className="coffee-theme text-foreground bg-background florage-background antialiased 
                min-h-screen min-w-xs max-w-screen"
            >
                <ClientHydration>
                    <AppProviders>
                        <div className="min-h-screen flex flex-col items-center pt-25">
                            <Navbar />
                            {children}
                            <Footer />
                            <Modals />
                        </div>
                    </AppProviders>
                </ClientHydration>
            </body>
        </html>
    )
}
