"use client"

import React from "react"
// import MenuPage from "@/features/menu/pages/MenuPage"
import MenuPage from "@/features/menu/pages/MenuPage"
import OrdersPage from "@/features/orders/pages/OrdersView"

export default function Home() {
    // handle when log

    return (
        // <div className="p-10 gap-4 flex flex-col">
        //     {/* 1. Nút bấm */}
        //     <div className="flex gap-4">
        //         <div className="flex gap-4 p-10 bg-background">
        //             <ButtonStyled color="primary" variant="ghost">
        //                 Đặt món ngay
        //             </ButtonStyled>
        //             <ButtonStyled color="secondary">Thêm Topping</ButtonStyled>
        //             <ButtonStyled color="default">Mặc định</ButtonStyled>

        //             {/* Hai nút này sẽ đổi màu mới */}
        //             <ButtonStyled color="success">Thành công (Success)</ButtonStyled>
        //             <ButtonStyled color="warning">Cảnh báo (Warning)</ButtonStyled>
        //             <ButtonStyled color="danger">Xóa bỏ (Danger)</ButtonStyled>
        //         </div>
        //     </div>

        //     {/* 2. Card sản phẩm */}
        //     {/* Tự động ăn màu nền trắng (content1) và chữ nâu (foreground) */}
        //     <Card className="max-w-100">
        //         <CardBody>
        //             <h2 className="text-xl font-bold">Cà phê Sữa Đá</h2>
        //             <p className="text-default-500">Đậm đà hương vị Việt Nam</p>

        //             <div className="mt-4">
        //                 {/* Input focus sẽ ra viền màu Nâu */}
        //                 <Input label="Ghi chú" placeholder="Ít đường, nhiều đá..." />
        //             </div>
        //         </CardBody>
        //     </Card>
        //     <NavbarStyled>
        //         <NavbarContent>
        //             <NavbarItem className="flex items-center z-9999">
        //                 {isLoggedIn ? (
        //                     // <ProfileDropdown onOpen={() => setIsOpen(false)} />
        //                     "haha"
        //                 ) : (
        //                     <ButtonStyled
        //                         onPress={() => {
        //                             onOpenLogin()
        //                             // setIsOpen(false)
        //                         }}
        //                         color="primary"
        //                         variant="solid"
        //                         className="rounded-3xl opacity-97"
        //                     >
        //                         {"Login"}
        //                     </ButtonStyled>
        //                 )}
        //             </NavbarItem>
        //         </NavbarContent>
        //     </NavbarStyled>
        // </div>
        <>
            <MenuPage />
            <OrdersPage />
        </>
    )
}
