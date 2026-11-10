"use client"

import { useRouter } from "next/navigation"
import { Button } from "@heroui/react"
import { FormikProvider, useFormik } from "formik"
import { Send, ArrowLeft } from "lucide-react"

import { useSubmitApplication } from "@/features/franchise/hooks/useFranchise"
import { SubmitApplicationRequest } from "@/features/franchise/models/request"
import { SubmitFranchiseSchema } from "./schema"

import { useGetMe } from "@/features/profile/hooks"
import {
    AttachmentCard,
    FranchisePolicyCard,
    PersonalInfoCard,
    StoreInfoCard
} from "@/features/franchise/components"

export default function FranchiseRegisterView() {
    const router = useRouter()

    const { data: user, isLoading: isUserLoading } = useGetMe()
    const { mutate: submitForm, isPending: isSubmitting } = useSubmitApplication({
        onSuccess: () => {
            router.push("/profile/franchise/applications")
        }
    })

    const formik = useFormik<SubmitApplicationRequest>({
        enableReinitialize: true,
        initialValues: {
            fullName: user ? `${user.lastName || ""} ${user.firstName || ""}`.trim() : "",
            email: user?.email || "",
            phone: user?.phone || "",
            identityNumber: "",
            provinceId: "",
            districtId: "",
            wardId: "",
            storeName: "",
            storeAddress: "",
            storeSizeSqm: undefined,
            storeFront: undefined,
            landCerts: []
        },
        validationSchema: SubmitFranchiseSchema,
        onSubmit: (values) => {
            const formData = new FormData()

            formData.append("fullName", values.fullName)
            formData.append("email", values.email)
            formData.append("phone", values.phone)
            formData.append("identityNumber", values.identityNumber)
            formData.append("provinceId", values.provinceId)
            formData.append("districtId", values.districtId)
            formData.append("wardId", values.wardId)
            formData.append("storeName", values.storeName)
            formData.append("storeAddress", values.storeAddress)

            if (values.storeSizeSqm) {
                formData.append("storeSizeSqm", values.storeSizeSqm.toString())
            }

            if (values.storeFront) formData.append("storeFront", values.storeFront)

            if (values.landCerts && values.landCerts.length > 0) {
                values.landCerts.forEach((file) => {
                    formData.append("landCerts", file)
                })
            }

            submitForm(formData as any)
        }
    })

    return (
        <FormikProvider value={formik}>
            <div className="p-4 md:p-8 space-y-6 max-w-7xl mx-auto min-h-fit h-fit bg-gray-50/50">
                {/* HEADER */}
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                        <Button isIconOnly variant="light" onPress={() => router.push("/")}>
                            <ArrowLeft size={20} />
                        </Button>
                        <div>
                            <h1 className="text-2xl font-black text-default-900">
                                Đơn Đăng Ký Nhượng Quyền
                            </h1>
                            <p className="text-sm text-default-500">
                                Vui lòng điền đầy đủ thông tin để thẩm định.
                            </p>
                        </div>
                    </div>
                    <Button
                        color="primary"
                        className="font-bold shadow-md px-8"
                        startContent={<Send size={18} />}
                        isLoading={isSubmitting}
                        onPress={() => formik.handleSubmit()}
                    >
                        GỬI HỒ SƠ THẨM ĐỊNH
                    </Button>
                </div>

                {/* BODY LẮP RÁP */}
                <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                    <div className="lg:col-span-7 space-y-6">
                        <PersonalInfoCard isUserLoading={isUserLoading} />
                        <StoreInfoCard />
                    </div>
                    <div className="lg:col-span-5 space-y-6">
                        <FranchisePolicyCard />
                    </div>
                </div>
                <AttachmentCard />
            </div>
        </FormikProvider>
    )
}
