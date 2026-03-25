"use client"

import { useRouter } from "next/navigation"
import { Button, Card, CardBody } from "@heroui/react"
import { ArrowRight, Coffee, TrendingUp, ShieldCheck, Headset } from "lucide-react"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

export default function FranchiseSection({ className }: { className?: string }) {
    const router = useRouter()

    // 1. Lấy state đăng nhập và hàm mở Modal Login
    const isLogined = useTokenStore((s) => !!s.accessToken)
    const roleSlug = useTokenStore((s) => s.roleSlug) || undefined

    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()

    // 2. Xử lý logic khi bấm nút "Đăng ký ngay"
    const handleRegisterClick = () => {
        if (!isLogined) {
            onOpenLogin() // Ép đăng nhập cái đã, Modal Login sẽ bay ra
            return
        }
        // Nếu có token rồi thì phi thẳng vào trang điền form
        router.push("/franchise/register")
    }

    const benefits = [
        {
            icon: <TrendingUp className="text-success" size={24} />,
            title: "Lợi nhuận hấp dẫn",
            desc: "Thời gian hoàn vốn nhanh chỉ từ 12-18 tháng với tỷ suất lợi nhuận cao."
        },
        {
            icon: <ShieldCheck className="text-primary" size={24} />,
            title: "Rủi ro thấp",
            desc: "Mô hình đã được chứng minh thành công, thương hiệu uy tín."
        },
        {
            icon: <Headset className="text-warning" size={24} />,
            title: "Hỗ trợ toàn diện A-Z",
            desc: "Setup quán, đào tạo nhân sự, cung cấp nguyên liệu và marketing."
        }
    ]

    if (isLogined && roleSlug !== RoleSlug.Customer) {
        return null
    }

    return (
        <section
            className={`py-16 md:py-24 bg-linear-to-br from-default-50 to-default-100 ${className || ""}`}
        >
            <div className="container mx-auto px-4 md:px-8 max-w-7xl">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
                    {/* CỘT TRÁI: TEXT & NÚT BẤM */}
                    <div className="space-y-8">
                        <div className="space-y-4">
                            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary-600 font-bold text-sm mb-2">
                                <Coffee size={16} />
                                <span>Cơ Hội Vàng 2026</span>
                            </div>
                            <h2 className="text-4xl md:text-5xl font-black text-default-900 tracking-tight leading-tight">
                                Trở Thành Đối Tác <br />
                                <span className="text-primary">Nhượng Quyền</span> Của Chúng Tôi
                            </h2>
                            <p className="text-lg text-default-600 max-w-lg">
                                Bạn có mặt bằng đẹp và đam mê kinh doanh F&B? Hãy để chúng tôi đồng
                                hành cùng bạn xây dựng một cửa hàng cà phê siêu lợi nhuận với quy
                                trình chuyên nghiệp nhất.
                            </p>
                        </div>

                        <div className="flex flex-col sm:flex-row gap-4">
                            <Button
                                color="primary"
                                size="lg"
                                className="font-bold px-8 shadow-lg shadow-primary/30"
                                endContent={<ArrowRight size={18} />}
                                onPress={handleRegisterClick}
                            >
                                ĐĂNG KÝ NGAY
                            </Button>
                            {/* <Button
                                variant="flat"
                                color="default"
                                size="lg"
                                className="font-semibold px-8"
                                onPress={() => router.push("/franchise")} // Link tới trang Landing page giới thiệu chi tiết (nếu có)
                            >
                                Tìm Hiểu Thêm
                            </Button> */}
                        </div>
                    </div>

                    {/* CỘT PHẢI: GRID LỢI ÍCH */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        {benefits.map((benefit, idx) => (
                            <Card
                                key={idx}
                                shadow="sm"
                                className={`border border-default-200 hover:-translate-y-1 hover:shadow-md transition-all duration-300 bg-white ${idx === 2 ? "sm:col-span-2" : ""}`}
                            >
                                <CardBody className="p-6 flex flex-col gap-3">
                                    <div className="w-12 h-12 rounded-xl bg-default-100 flex items-center justify-center">
                                        {benefit.icon}
                                    </div>
                                    <h4 className="text-lg font-bold text-default-900">
                                        {benefit.title}
                                    </h4>
                                    <p className="text-sm text-default-500">{benefit.desc}</p>
                                </CardBody>
                            </Card>
                        ))}
                    </div>
                </div>
            </div>
        </section>
    )
}
