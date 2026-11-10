"use client"

import { Card, CardBody } from "@heroui/react"
import { Store } from "lucide-react"
import { useFormikContext } from "formik"
import { InputStyled } from "@/components"
import { useFormikHelpers } from "@/hooks"
import { SubmitApplicationRequest } from "@/features/franchise/models/request"

export const PersonalInfoCard = ({ isUserLoading }: { isUserLoading: boolean }) => {
    const formik = useFormikContext<SubmitApplicationRequest>()
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)
    const { values, handleChange, handleBlur } = formik

    return (
        <Card shadow="sm" className="border border-default-200">
            <CardBody className="p-6 space-y-6 bg-white">
                <h3 className="text-sm font-black uppercase text-primary-700 tracking-wider mb-2 flex items-center gap-2">
                    <Store size={18} />
                    1. Thông tin Chủ hộ kinh doanh
                </h3>

                <div className="grid grid-cols-2 gap-4">
                    <InputStyled
                        label="Họ và tên"
                        name="fullName"
                        placeholder="Nhập họ và tên..."
                        value={values.fullName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        isInvalid={isInvalid("fullName")}
                        errorMessage={getErrorMessage("fullName")}
                        isDisabled={isUserLoading}
                        classNames={{ label: "font-bold text-default-700" }}
                    />
                    <InputStyled
                        label="Số CMND/CCCD"
                        name="identityNumber"
                        placeholder="Nhập số CMND/CCCD..."
                        value={values.identityNumber}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        isInvalid={isInvalid("identityNumber")}
                        errorMessage={getErrorMessage("identityNumber")}
                        classNames={{ label: "font-bold text-default-700" }}
                    />
                    <InputStyled
                        label="Email liên hệ"
                        name="email"
                        type="email"
                        value={values.email}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        isInvalid={isInvalid("email")}
                        errorMessage={getErrorMessage("email")}
                        isDisabled={isUserLoading}
                        classNames={{ label: "font-bold text-default-700" }}
                    />
                    <InputStyled
                        label="Số điện thoại"
                        name="phone"
                        value={values.phone}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        isInvalid={isInvalid("phone")}
                        errorMessage={getErrorMessage("phone")}
                        isDisabled={isUserLoading}
                        classNames={{ label: "font-bold text-default-700" }}
                    />
                </div>
            </CardBody>
        </Card>
    )
}
