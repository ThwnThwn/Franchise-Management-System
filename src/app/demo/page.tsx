"use client"

import { MultiImageUploaderModal } from "@/components/shared/ImageUploaderCore/MultiImageUploaderModal"
import { useDisclosure } from "@heroui/react"
import { useMutation } from "@tanstack/react-query"
import React from "react"

// Hàm giả lập API
const mockUploadImages = async (formData: FormData) => {
    console.group("🚀 START MOCK UPLOAD")

    // 1. Kiểm tra xem FormData có gì bên trong
    // Lưu ý: console.log(formData) trực tiếp sẽ thấy rỗng, phải loop qua entries
    const files = formData.getAll("images") // Hoặc key mà bạn đã append
    console.log(`📦 Nhận được ${files.length} file(s):`)

    files.forEach((file: any, index) => {
        console.log(`#${index + 1}:`, {
            name: file.name,
            size: (file.size / 1024).toFixed(2) + " KB",
            type: file.type
        })
    })

    // 2. Giả lập độ trễ mạng (2 giây) để test Spinner loading
    console.log("⏳ Đang giả lập upload...")
    await new Promise((resolve) => setTimeout(resolve, 2000))

    console.log("✅ Upload thành công!")
    console.groupEnd()

    // 3. Trả về kết quả giả (để React Query tưởng là xong)
    return { success: true, message: "Fake upload done" }
}

export default function Home() {
    const { onOpenChange, onClose } = useDisclosure()

    const mockMutation = useMutation({
        mutationFn: mockUploadImages, // <--- Gắn hàm mock vào đây
        onSuccess: () => {
            alert("Test: Upload thành công rồi nè!")
        }
    })

    return (
        <div>
            {/* ... */}

            <MultiImageUploaderModal
                isOpen={true}
                onOpenChange={onOpenChange}
                onClose={onClose}
                // Truyền hàm mock vào
                uploadFn={mockMutation.mutateAsync}
                isUploadPending={mockMutation.isPending}
                // Test các config
                label="Test Upload Nhiều Ảnh"
                maxFiles={15} // Thử giới hạn 5 ảnh
                minFiles={2} // Thử bắt buộc chọn 2 ảnh
                cropSize={{
                    width: 400,
                    height: 300
                }}
            />
        </div>
    )
}
