"use client"

import { ButtonStyled, ModalStyled } from "@/components/styled"
import { PasswordInputStyled } from "@/components/styled"
import { useChangePassword } from "@/features/auth/hooks"
import { useFormikHelpers } from "@/hooks"
import { ModalBody, ModalContent, ModalFooter, ModalHeader, Switch } from "@heroui/react"
import { useFormik } from "formik"
import { Lock, ShieldCheck } from "lucide-react"
import { ChangePasswordSchema } from "./schema"

interface Props {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
}

export const ChangePasswordModal = ({ isOpen, onOpenChange }: Props) => {
    const { changeWithConfirm, isPending } = useChangePassword({
        onSuccess: () => {
            onOpenChange(false)
            formik.resetForm()
        }
    })

    const formik = useFormik({
        initialValues: {
            oldPassword: "",
            newPassword: "",
            confirmNewPassword: "",
            forgetAll: false
        },
        validationSchema: ChangePasswordSchema,
        onSubmit: (values) => {
            changeWithConfirm(values)
        }
    })

    const { getErrorMessage, isInvalid } = useFormikHelpers(formik)

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} className="max-w-md">
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1 text-2xl font-black uppercase tracking-tighter">
                            Đổi mật khẩu
                        </ModalHeader>
                        <ModalBody>
                            <form
                                id="change-password-form"
                                onSubmit={formik.handleSubmit}
                                className="space-y-4"
                            >
                                {/* Mật khẩu cũ */}
                                <PasswordInputStyled
                                    label="Mật khẩu hiện tại"
                                    placeholder="Nhập mật khẩu đang dùng"
                                    variant="bordered"
                                    startContent={<Lock size={18} className="text-default-400" />}
                                    name="oldPassword"
                                    value={formik.values.oldPassword}
                                    onChange={formik.handleChange}
                                    onBlur={formik.handleBlur}
                                    isInvalid={isInvalid("oldPassword")}
                                    errorMessage={getErrorMessage("oldPassword")}
                                />

                                <PasswordInputStyled
                                    label="Mật khẩu mới"
                                    placeholder="Nhập mật khẩu mới"
                                    variant="bordered"
                                    startContent={
                                        <ShieldCheck size={18} className="text-default-400" />
                                    }
                                    name="newPassword"
                                    value={formik.values.newPassword}
                                    onChange={formik.handleChange}
                                    onBlur={formik.handleBlur}
                                    isInvalid={isInvalid("newPassword")}
                                    errorMessage={getErrorMessage("newPassword")}
                                />

                                <PasswordInputStyled
                                    label="Xác nhận mật khẩu mới"
                                    placeholder="Nhập lại mật khẩu mới"
                                    variant="bordered"
                                    name="confirmNewPassword"
                                    value={formik.values.confirmNewPassword}
                                    onChange={formik.handleChange}
                                    onBlur={formik.handleBlur}
                                    isInvalid={isInvalid("confirmNewPassword")}
                                    errorMessage={getErrorMessage("confirmNewPassword")}
                                />
                                {/* Switch Forget All */}
                                <div className="flex items-center justify-between p-3 bg-default-50 rounded-2xl border border-default-100">
                                    <div className="flex flex-col gap-0.5">
                                        <span className="text-sm font-bold">
                                            Đăng xuất tất cả thiết bị?
                                        </span>
                                        <span className="text-tiny text-default-400">
                                            An toàn hơn nếu bạn nghi ngờ bị lộ mật khẩu
                                        </span>
                                    </div>
                                    <Switch
                                        color="danger"
                                        name="forgetAll"
                                        isSelected={formik.values.forgetAll}
                                        onValueChange={(value) =>
                                            formik.setFieldValue("forgetAll", value)
                                        }
                                    />
                                </div>
                            </form>
                        </ModalBody>
                        <ModalFooter>
                            <ButtonStyled variant="light" onPress={onClose} className="font-bold">
                                Hủy
                            </ButtonStyled>
                            <ButtonStyled
                                color="primary"
                                type="submit"
                                form="change-password-form"
                                isLoading={isPending}
                                className="font-black px-8"
                            >
                                Cập nhật ngay
                            </ButtonStyled>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
