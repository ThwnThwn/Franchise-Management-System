"use client"

import { useCallback } from "react"

export const usePrintInvoice = () => {
    const handlePrint = useCallback((elementId: string) => {
        const printContent = document.getElementById(elementId)
        if (!printContent) return

        // 1. Mở popup
        const printWindow = window.open("", "_blank", "width=1050,height=900")
        if (!printWindow) return

        // 2. Lấy Styles
        const styles = Array.from(document.querySelectorAll("style, link[rel='stylesheet']"))
            .map((style) => style.outerHTML)
            .join("")

        printWindow.document.write(`
            <html>
                <head>
                    <title>In hóa đơn - CHB Coffee</title>
                    ${styles}
                    <style>
                        /* ĐÈ BỆT MỌI CSS CŨ ĐANG LÀM ẨN NỘI DUNG */
                        @media print {
                            @page { margin: 0; size: auto; }
                            body { 
                                visibility: visible !important; 
                                background: white !important; 
                            }
                            #${elementId}, #${elementId} * {
                                visibility: visible !important;
                                display: block !important;
                            }
                        }
                        
                        /* Layout cho màn hình xem trước */
                        body { 
                            margin: 0; 
                            padding: 2cm; 
                            display: flex; 
                            justify-content: center; 
                            background: #f4f4f5; 
                        }
                        .print-container {
                            background: white;
                            width: 100%;
                            max-width: 800px;
                        }
                        /* Ẩn các icon/nút nếu lỡ bị copy vào */
                        button, .no-print { display: none !important; }
                    </style>
                </head>
                <body>
                    <div class="print-container">
                        ${printContent.innerHTML}
                    </div>
                </body>
            </html>
        `)

        printWindow.document.close()

        // 3. Logic kích hoạt lệnh In
        // Sử dụng một Promise để đợi tất cả ảnh (Logo/Sản phẩm) load xong
        const images = printWindow.document.images
        const imagePromises = Array.from(images).map((img) => {
            if (img.complete) return Promise.resolve()
            return new Promise((resolve) => {
                img.onload = resolve
                img.onerror = resolve
            })
        })

        Promise.all(imagePromises).then(() => {
            setTimeout(() => {
                printWindow.focus()
                printWindow.print()
                // printWindow.close() // Giữ lại để debug nếu cần
            }, 500)
        })
    }, [])

    return { handlePrint }
}
