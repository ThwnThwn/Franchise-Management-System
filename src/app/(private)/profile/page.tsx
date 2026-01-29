"use client"

import {
    ButtonIconStyled,
    ButtonStyled,
    DatePickerStyled,
    InputStyled
} from "@/components"
import { Gender } from "@/constants/enums"
import { GenderLabels } from "@/constants/labels"
import { NAME_REGEX, PHONE_REGEX } from "@/constants/regexs"
import { AUTH_ERRORS } from "@/features"
import { useGetMe, useUpdateMe } from "@/features/profile/hooks/useProfile"
import { UserUpdateReq } from "@/features/profile/models/request"
import { useDay } from "@/hooks"
import { useUserHelper } from "@/hooks/reuseable/useUserHelper"
import { EnumPicker } from "@/components/modules/EnumPicker"
import { useFormik } from "formik"
import { Camera, Coffee, Mail, MapPin, Phone, User, CalendarDays, Pencil, Save, X } from "lucide-react"
import React, { useCallback, useState } from "react"
import * as Yup from "yup"
import { Avatar } from "@heroui/react"
import { motion } from "framer-motion"
import { parseDate } from "@internationalized/date"

export default function ProfilePage() {
    const [editable, setEditable] = useState(false)
    const updateMeMutation = useUpdateMe({ onSuccess: undefined })
    const { toFullName } = useUserHelper()
    const { data: user } = useGetMe()
    const { toDate, formatDateTime } = useDay({ defaultFormat: "YYYY-MM-DD" })

    const handleUpdateMe = useCallback(
        async (values: UserUpdateReq) => {
            await updateMeMutation.mutateAsync(values)
            setEditable(false)
        },
        [updateMeMutation]
    )

    const updateMeFormik = useFormik({
        enableReinitialize: true,
        initialValues: {
            firstName: user?.firstName || "",
            lastName: user?.lastName || "",
            phone: user?.phone || "",
            gender: user?.gender || Gender.Male,
            dateOfBirth: user?.dateOfBirth || "",
            address: user?.address || ""
        },
        validationSchema: Yup.object({
            firstName: Yup.string()
                .required(AUTH_ERRORS.AUTH.FIRST_NAME_REQUIRED)
                .matches(NAME_REGEX, "Tên không hợp lệ"),
            lastName: Yup.string().required("Vui lòng nhập họ"),
            phone: Yup.string()
                .required("Vui lòng nhập số điện thoại")
                .matches(PHONE_REGEX, "Số điện thoại không hợp lệ"),
            gender: Yup.number().required("Vui lòng chọn giới tính"),
            dateOfBirth: Yup.string().required("Vui lòng chọn ngày sinh"),
            address: Yup.string().max(200, "Địa chỉ quá dài").nullable()
        }),
        onSubmit: async (values) => {
            await handleUpdateMe(values)
        }
    })

    if (!user) return null

    return (
        <div className="w-full min-h-[calc(100vh-80px)] bg-[#fdfbf7] p-4 md:p-8">
            <div className="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-8">
                {/* Left Column: Profile Card & Quick Stats */}
                <motion.div
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ duration: 0.5 }}
                    className="lg:col-span-4 space-y-6"
                >
                    {/* User Card */}
                    <div className="bg-white rounded-3xl shadow-xl overflow-hidden border border-amber-100">
                        <div className="h-32 bg-[url('https://images.unsplash.com/photo-1447933601403-0c6688de566e?q=80&w=1000&auto=format&fit=crop')] bg-cover bg-center relative">
                            <div className="absolute inset-0 bg-gradient-to-b from-black/30 to-black/60"></div>
                        </div>
                        <div className="relative px-6 pb-6 mt-[-50px] flex flex-col items-center">
                            <div className="relative">
                                <Avatar
                                    src="https://i.pravatar.cc/150?u=a042581f4e29026704d"
                                    className="w-28 h-28 border-4 border-white shadow-2xl"
                                />
                                <div className="absolute bottom-1 right-1 bg-amber-500 p-1.5 rounded-full border-2 border-white cursor-pointer hover:bg-amber-600 transition-colors">
                                    <Camera size={14} className="text-white" />
                                </div>
                            </div>

                            <h2 className="mt-4 text-2xl font-bold text-gray-800">
                                {toFullName({ firstName: user.firstName, lastName: user.lastName })}
                            </h2>
                            <p className="text-amber-600 font-medium">@{user.userName}</p>

                            <div className="mt-4 flex gap-2">
                                <span className="px-3 py-1 bg-amber-100 text-amber-800 rounded-full text-xs font-semibold uppercase tracking-wider">
                                    {user.role?.name || "Khách hàng thân thiết"}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Quick Contact Info (Read-only view) */}
                    <div className="bg-white rounded-3xl p-6 shadow-lg border border-amber-50">
                        <h3 className="text-lg font-bold text-gray-800 mb-4 flex items-center gap-2">
                            <Coffee size={20} className="text-amber-600" />
                            Thông tin liên hệ
                        </h3>
                        <div className="space-y-4">
                            <div className="flex items-center gap-3 p-3 bg-amber-50/50 rounded-xl hover:bg-amber-50 transition-colors">
                                <div className="p-2 bg-white rounded-lg shadow-sm">
                                    <Mail size={18} className="text-amber-600" />
                                </div>
                                <div className="overflow-hidden">
                                    <p className="text-xs text-gray-500">Email</p>
                                    <p className="text-sm font-medium text-gray-800 truncate">{user.email}</p>
                                </div>
                            </div>
                            <div className="flex items-center gap-3 p-3 bg-amber-50/50 rounded-xl hover:bg-amber-50 transition-colors">
                                <div className="p-2 bg-white rounded-lg shadow-sm">
                                    <Phone size={18} className="text-amber-600" />
                                </div>
                                <div>
                                    <p className="text-xs text-gray-500">Điện thoại</p>
                                    <p className="text-sm font-medium text-gray-800">{user.phone || "Chưa cập nhật"}</p>
                                </div>
                            </div>
                            <div className="flex items-center gap-3 p-3 bg-amber-50/50 rounded-xl hover:bg-amber-50 transition-colors">
                                <div className="p-2 bg-white rounded-lg shadow-sm">
                                    <MapPin size={18} className="text-amber-600" />
                                </div>
                                <div>
                                    <p className="text-xs text-gray-500">Địa chỉ</p>
                                    <p className="text-sm font-medium text-gray-800 truncate max-w-[200px]">{user.address || "Chưa cập nhật"}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </motion.div>

                {/* Right Column: Detailed Edit Form */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5, delay: 0.1 }}
                    className="lg:col-span-8"
                >
                    <div className="bg-white rounded-3xl shadow-xl border border-amber-100 p-6 md:p-8">
                        <div className="flex justify-between items-center mb-8 border-b border-gray-100 pb-4">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-800">Hồ Sơ Cá Nhân</h1>
                                <p className="text-gray-500 mt-1">Quản lý thông tin đầy đủ của bạn</p>
                            </div>
                            {!editable ? (
                                <ButtonStyled
                                    onPress={() => setEditable(true)}
                                    className="bg-amber-900 text-white shadow-lg hover:bg-amber-800"
                                    startContent={<Pencil size={18} />}
                                >
                                    Chỉnh sửa
                                </ButtonStyled>
                            ) : (
                                <div className="flex gap-2">
                                    <ButtonStyled
                                        variant="light"
                                        color="danger"
                                        onPress={() => {
                                            setEditable(false)
                                            updateMeFormik.resetForm()
                                        }}
                                        startContent={<X size={18} />}
                                    >
                                        Hủy
                                    </ButtonStyled>
                                    <ButtonStyled
                                        onPress={updateMeFormik.submitForm}
                                        isLoading={updateMeFormik.isSubmitting}
                                        className="bg-amber-600 text-white shadow-lg hover:bg-amber-700"
                                        startContent={<Save size={18} />}
                                    >
                                        Lưu thay đổi
                                    </ButtonStyled>
                                </div>
                            )}
                        </div>

                        <form className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Read-Only Fields */}
                            <div className="md:col-span-1">
                                <InputStyled
                                    isReadOnly
                                    label="Tên đăng nhập"
                                    value={user.userName}
                                    variant="faded"
                                    startContent={<User className="text-gray-400" size={18} />}
                                    className="opacity-70 pointer-events-none"
                                />
                            </div>
                            <div className="md:col-span-1">
                                <InputStyled
                                    isReadOnly
                                    label="Email"
                                    value={user.email}
                                    variant="faded"
                                    startContent={<Mail className="text-gray-400" size={18} />}
                                    className="opacity-70 pointer-events-none"
                                />
                            </div>

                            <div className="col-span-2 my-2 border-t border-dashed border-gray-200" />

                            {/* Editable Fields */}
                            <InputStyled
                                isReadOnly={!editable}
                                label="Họ & Tên đệm"
                                placeholder="Nhập họ..."
                                variant={editable ? "bordered" : "flat"}
                                value={updateMeFormik.values.lastName}
                                onValueChange={(val) => updateMeFormik.setFieldValue("lastName", val)}
                                isInvalid={!!(updateMeFormik.touched.lastName && updateMeFormik.errors.lastName)}
                                errorMessage={updateMeFormik.errors.lastName}
                            />
                            <InputStyled
                                isReadOnly={!editable}
                                label="Tên"
                                placeholder="Nhập tên..."
                                variant={editable ? "bordered" : "flat"}
                                value={updateMeFormik.values.firstName}
                                onValueChange={(val) => updateMeFormik.setFieldValue("firstName", val)}
                                isInvalid={!!(updateMeFormik.touched.firstName && updateMeFormik.errors.firstName)}
                                errorMessage={updateMeFormik.errors.firstName}
                            />

                            <InputStyled
                                isReadOnly={!editable}
                                label="Số điện thoại"
                                placeholder="Nhập số điện thoại..."
                                variant={editable ? "bordered" : "flat"}
                                value={updateMeFormik.values.phone}
                                onValueChange={(val) => updateMeFormik.setFieldValue("phone", val)}
                                isInvalid={!!(updateMeFormik.touched.phone && updateMeFormik.errors.phone)}
                                errorMessage={updateMeFormik.errors.phone}
                                startContent={<Phone className="text-gray-400" size={18} />}
                            />

                            <EnumPicker
                                isReadOnly={!editable}
                                label="Giới tính"
                                labels={GenderLabels}
                                value={updateMeFormik.values.gender}
                                onChange={(val) => updateMeFormik.setFieldValue("gender", val)}
                                isClearable={false}
                            />

                            <div className="md:col-span-1">
                                <DatePickerStyled
                                    isReadOnly={!editable}
                                    label="Ngày sinh"
                                    value={
                                        updateMeFormik.values.dateOfBirth
                                            ? parseDate(updateMeFormik.values.dateOfBirth.split("T")[0])
                                            : undefined
                                    }
                                    onChange={(date) => {
                                        if (date) {
                                            updateMeFormik.setFieldValue("dateOfBirth", date.toString())
                                        }
                                    }}
                                    isInvalid={!!(updateMeFormik.touched.dateOfBirth && updateMeFormik.errors.dateOfBirth)}
                                    errorMessage={updateMeFormik.errors.dateOfBirth as string}
                                />
                            </div>

                            <div className="md:col-span-2">
                                <InputStyled
                                    isReadOnly={!editable}
                                    label="Địa chỉ"
                                    placeholder="Nhập địa chỉ của bạn..."
                                    variant={editable ? "bordered" : "flat"}
                                    value={updateMeFormik.values.address}
                                    onValueChange={(val) => updateMeFormik.setFieldValue("address", val)}
                                    isInvalid={!!(updateMeFormik.touched.address && updateMeFormik.errors.address)}
                                    errorMessage={updateMeFormik.errors.address}
                                    startContent={<MapPin className="text-gray-400" size={18} />}
                                />
                            </div>
                        </form>
                    </div>

                    {/* Fun Stats or Banner */}
                    <div className="mt-8 bg-gradient-to-r from-amber-700 to-amber-900 rounded-3xl p-8 text-white shadow-xl flex items-center justify-between relative overflow-hidden">
                        <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl transform translate-x-1/2 -translate-y-1/2"></div>
                        <div className="relative z-10">
                            <h3 className="text-2xl font-bold mb-2">Thành Viên Thân Thiết</h3>
                            <p className="text-amber-100 max-w-md">Cập nhật đầy đủ thông tin để nhận thêm ưu đãi đặc biệt từ Cà phê Của Bạn.</p>
                        </div>
                        <div className="hidden md:flex items-center justify-center w-16 h-16 bg-white/20 backdrop-blur-md rounded-2xl border border-white/30">
                            <Coffee size={32} className="text-amber-100" />
                        </div>
                    </div>
                </motion.div>
            </div>
        </div>
    )
}
