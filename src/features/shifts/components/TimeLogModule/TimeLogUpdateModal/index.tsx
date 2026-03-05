"use client"

import { ModalContent, ModalHeader, ModalBody, ModalFooter, Button } from "@heroui/react"
import { Formik } from "formik"
import { Pencil } from "lucide-react"

import { ModalStyled, ButtonStyled } from "@/components"

import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogStatus } from "@/features/shifts/constants"
import { updateTimeLogSchema } from "./schema"
import { useUpdateTimeLog } from "@/features/shifts/hooks"
import { TimeLogUpdateBody } from "./TimeLogUpdateBody"
import { TimeLogViewBody } from "./TimeLogViewBody"

interface Props {
    isOpen: boolean
    onClose: () => void
    data: TimeLogResponse | null // Record đang được chọn để edit
    isReadOnly: boolean
}

export const TimeLogUpdateModal = ({ isOpen, onClose, data, isReadOnly }: Props) => {
    const updateMutation = useUpdateTimeLog({ onSuccess: onClose })

    if (!data) return null

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="2xl">
            <Formik
                initialValues={{
                    status: data.status ?? TimeLogStatus.Processing,
                    managerNote: data.managerNote ?? ""
                }}
                enableReinitialize
                validationSchema={updateTimeLogSchema}
                onSubmit={(values) => {
                    if (!data || isReadOnly) return
                    updateMutation.mutate({
                        id: data.id,
                        req: {
                            status: values.status,
                            managerNote: values.managerNote || undefined
                        }
                    })
                }}
            >
                {({ handleSubmit, dirty, isSubmitting }) => (
                    <ModalContent>
                        <ModalHeader className="flex gap-2 items-center border-b pb-4">
                            <Pencil className="text-primary" size={24} />
                            <div className="flex flex-col">
                                <span className="text-lg">Cập Nhật Chấm Công</span>
                                <span className="text-tiny font-normal text-default-400">
                                    Điều chỉnh trạng thái và ghi chú cho bản ghi #
                                    {data.id.substring(0, 8)}
                                </span>
                            </div>
                        </ModalHeader>

                        <ModalBody className="py-6 overflow-y-auto max-h-[70vh]">
                            {isReadOnly ? (
                                <TimeLogViewBody data={data} />
                            ) : (
                                <TimeLogUpdateBody data={data} />
                            )}
                        </ModalBody>

                        <ModalFooter className="border-t bg-default-50/50 rounded-b-3xl">
                            <Button
                                variant="light"
                                onPress={onClose}
                                isDisabled={updateMutation.isPending}
                            >
                                Hủy
                            </Button>
                            {!isReadOnly && (
                                <ButtonStyled
                                    color="primary"
                                    isLoading={isSubmitting}
                                    isDisabled={!dirty}
                                    onPress={() => handleSubmit()}
                                    className="px-8"
                                >
                                    Lưu thay đổi
                                </ButtonStyled>
                            )}
                        </ModalFooter>
                    </ModalContent>
                )}
            </Formik>
        </ModalStyled>
    )
}
