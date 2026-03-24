"use client"

import { Card, CardBody } from "@heroui/react"
import { MapPin } from "lucide-react"
import { useFormikContext } from "formik"
import { InputStyled } from "@/components"
import { useFormikHelpers } from "@/hooks"
import { SubmitApplicationRequest } from "@/features/franchise/models/request"

export const StoreInfoCard = () => {
    const formik = useFormikContext<SubmitApplicationRequest>()
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)
    const { values, handleChange, handleBlur } = formik

    return (
        <Card shadow="sm" className="border border-default-200">
            <CardBody className="p-6 space-y-6 bg-white">
                <h3 className="text-sm font-black uppercase text-secondary-600 tracking-wider mb-2 flex items-center gap-2">
                    <MapPin size={18} />
                    2. Thông tin Quán dự kiến
                </h3>

                <div className="grid grid-cols-2 gap-4">
                    <InputStyled
                        label="Tên quán dự kiến"
                        name="storeName"
                        placeholder="VD: The Coffee Shop - Chi nhánh Quận 1"
                        value={values.storeName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        isInvalid={isInvalid("storeName")}
                        errorMessage={getErrorMessage("storeName")}
                        className="col-span-2"
                        classNames={{ label: "font-bold text-default-700" }}
                    />

                    {/* TODO: Thay bằng component Select Tỉnh/Huyện/Xã */}
                    <InputStyled
                        label="Mã Tỉnh"
                        name="provinceId"
                        value={values.provinceId}
                        onChange={handleChange}
                        isInvalid={isInvalid("provinceId")}
                        errorMessage={getErrorMessage("provinceId")}
                    />
                    <InputStyled
                        label="Mã Quận"
                        name="districtId"
                        value={values.districtId}
                        onChange={handleChange}
                        isInvalid={isInvalid("districtId")}
                        errorMessage={getErrorMessage("districtId")}
                    />
                    <InputStyled
                        label="Mã Phường"
                        name="wardId"
                        value={values.wardId}
                        onChange={handleChange}
                        isInvalid={isInvalid("wardId")}
                        errorMessage={getErrorMessage("wardId")}
                    />
                    <InputStyled
                        label="Diện tích mặt bằng (m2)"
                        name="storeSizeSqm"
                        type="number"
                        placeholder="VD: 50"
                        value={values.storeSizeSqm?.toString() || ""}
                        onChange={handleChange}
                        isInvalid={isInvalid("storeSizeSqm")}
                        errorMessage={getErrorMessage("storeSizeSqm")}
                    />
                    <InputStyled
                        label="Địa chỉ chi tiết"
                        name="storeAddress"
                        value={values.storeAddress}
                        onChange={handleChange}
                        isInvalid={isInvalid("storeAddress")}
                        errorMessage={getErrorMessage("storeAddress")}
                        className="col-span-2"
                    />
                </div>
            </CardBody>
        </Card>
    )
}
