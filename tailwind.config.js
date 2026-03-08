import { heroui } from "@heroui/react"

/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
        "./node_modules/@heroui/theme/dist/**/*.{js,ts,jsx,tsx}"
    ],
    theme: {
        extend: {}
    },
    darkMode: "class",
    plugins: [
        heroui({
            defaultTheme: "coffee-theme",
            themes: {
                "coffee-theme": {
                    extend: "light",
                    colors: {
                        background: "#FAF8F5", // Be sáng
                        foreground: "#3E2723", // Nâu đen
                        primary: {
                            50: "#ebdcd8", // Đậm hơn rõ rệt
                            100: "#dfc7bf",
                            200: "#c9a69b",
                            300: "#b08477",
                            400: "#97685a",
                            500: "#7d4e41",
                            600: "#683f33",
                            700: "#5D4037", // DEFAULT
                            800: "#4d342c",
                            900: "#402a24",
                            DEFAULT: "#5D4037",
                            foreground: "#FFFFFF"
                        },
                        secondary: {
                            50: "#eee7e4",
                            100: "#dcd0cb",
                            200: "#cbb6af",
                            300: "#b59a91",
                            400: "#a18276",
                            500: "#8D6E63", // DEFAULT
                            600: "#7a5d53",
                            700: "#634940",
                            800: "#4f3a32",
                            900: "#3d2b24",
                            DEFAULT: "#8D6E63",
                            foreground: "#FFFFFF"
                        },
                        default: {
                            DEFAULT: "#D7CCC8", // Be xám
                            foreground: "#3E2723"
                        },

                        // 1. Success (Thành công): Xanh Matcha trầm
                        success: {
                            DEFAULT: "#4E7C59",
                            foreground: "#FFFFFF"
                        },

                        // 2. Warning (Cảnh báo): Vàng nghệ / Cam đất
                        warning: {
                            DEFAULT: "#B45309", // Màu cam nâu (Amber 700) nhìn rất ấm
                            foreground: "#FFFFFF" // Chữ trắng cho nổi
                        },

                        // 3. Danger (Nguy hiểm/Lỗi): Đỏ gạch trầm
                        danger: {
                            DEFAULT: "#991B1B", // Đỏ nâu đậm (Red 800)
                            foreground: "#FFFFFF" // Chữ trắng
                        },

                        // --- CÁC MÀU KHÁC ---
                        focus: "#5D4037", // Viền khi focus
                        content1: "#FFFFFF", // Nền thẻ
                        content2: "#EFEBE9" // Nền phụ
                    }
                }
            }
        })
    ]
}
