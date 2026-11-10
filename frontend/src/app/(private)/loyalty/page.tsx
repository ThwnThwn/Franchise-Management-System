import LoyaltyPageView from "@/features/profile/pages/LoyaltyPageView";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Cà Phê Của Bạn - Đặc quyền thành viên",
    description: "Tích điểm đổi quà và tận hưởng các đặc quyền hạng thẻ hấp dẫn."
}

export default function LoyaltyPage() {
    return <LoyaltyPageView />
}
