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
            themes: {
                "coffee-theme": {
                    extend: "light",
                    colors: {
                        // --- CÁC MÀU CƠ BẢN (Như cũ) ---
                        background: "#FAF8F5", // Be sáng
                        foreground: "#3E2723", // Nâu đen (Quan trọng!)

                        primary: {
                            DEFAULT: "#5D4037", // Nâu Mocha
                            foreground: "#FFFFFF"
                        },
                        secondary: {
                            DEFAULT: "#8D6E63", // Nâu Latte
                            foreground: "#FFFFFF"
                        },
                        default: {
                            DEFAULT: "#D7CCC8", // Be xám
                            foreground: "#3E2723"
                        },

                        // --- CẬP NHẬT CÁC MÀU TRẠNG THÁI ---

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
