// import React, { useState, useEffect } from "react"
// import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Input, Checkbox, Avatar, Spinner, addToast } from "@heroui/react"
// import { Search } from "lucide-react"
// import { useGetAllUsers } from "@/features/users/hooks/useUser"
// import { Voucher } from "@/features/orders/models/voucher"

// interface AssignVoucherModalProps {
//     isOpen: boolean
//     onOpenChange: (open: boolean) => void
//     voucher: Voucher | null
//     onSuccess: () => void
// }

// export const AssignVoucherModal = ({ isOpen, onOpenChange, voucher, onSuccess }: AssignVoucherModalProps) => {
//     const [searchQuery, setSearchQuery] = useState("")
//     const [debouncedQuery, setDebouncedQuery] = useState("")
//     const [selectedUserIds, setSelectedUserIds] = useState<string[]>([])

//     // Debounce search
//     useEffect(() => {
//         const timer = setTimeout(() => {
//             setDebouncedQuery(searchQuery)
//         }, 500)
//         return () => clearTimeout(timer)
//     }, [searchQuery])

//     // Reset state when modal opens
//     useEffect(() => {
//         if (isOpen) {
//             setSearchQuery("")
//             setDebouncedQuery("")
//             setSelectedUserIds([])
//         }
//     }, [isOpen])

//     const { data: usersData, isLoading: isLoadingUsers } = useGetAllUsers({
//         params: { keyword: debouncedQuery },
//         pagination: { page: 1, pageSize: 20 },
//         enabled: isOpen
//     })

//     // const { mutate: assignVoucher, isPending: isAssigning } = useAssignVoucher()

//     const handleAssign = () => {
//         if (!voucher || selectedUserIds.length === 0) return

//         assignVoucher({ id: voucher.id, userIds: selectedUserIds }, {
//             onSuccess: () => {
//                 onSuccess()
//                 addToast({
//                     title: "Thành công",
//                     description: `Đã gán voucher cho ${selectedUserIds.length} người dùng`,
//                     color: "success"
//                 })
//                 onOpenChange(false)
//             },
//             onError: () => {
//                 addToast({
//                     title: "Lỗi",
//                     description: "Gán voucher thất bại! Có thể voucher đã hết số lượng.",
//                     color: "danger"
//                 })
//             }
//         })
//     }

//     const toggleUser = (userId: string) => {
//         setSelectedUserIds(prev =>
//             prev.includes(userId)
//                 ? prev.filter(id => id !== userId)
//                 : [...prev, userId]
//         )
//     }

//     const users = usersData?.items || []

//     return (
//         <Modal isOpen={isOpen} onOpenChange={onOpenChange} size="2xl" scrollBehavior="inside">
//             <ModalContent>
//                 {(onClose) => (
//                     <>
//                         <ModalHeader className="flex flex-col gap-1">
//                             <h3>Gán Voucher cho Người dùng</h3>
//                             {voucher && (
//                                 <p className="text-sm font-normal text-default-500">
//                                     Mã: <span className="font-semibold text-primary">{voucher.code}</span>
//                                 </p>
//                             )}
//                         </ModalHeader>
//                         <ModalBody>
//                             <Input
//                                 placeholder="Tìm kiếm tài khoản..."
//                                 startContent={<Search size={18} className="text-default-400" />}
//                                 value={searchQuery}
//                                 onValueChange={setSearchQuery}
//                                 className="mb-4"
//                             />

//                             <div className="flex justify-between items-center mb-2 px-1">
//                                 <span className="text-sm font-medium">Danh sách người dùng</span>
//                                 <span className="text-sm text-default-500">Đã chọn: {selectedUserIds.length}</span>
//                             </div>

//                             <div className="border border-default-200 rounded-lg p-2 max-h-[300px] overflow-y-auto space-y-1">
//                                 {isLoadingUsers ? (
//                                     <div className="flex justify-center py-8">
//                                         <Spinner />
//                                     </div>
//                                 ) : users.length > 0 ? (
//                                     users.map(user => (
//                                         <div
//                                             key={user.id}
//                                             className={`flex items-center justify-between p-2 rounded-lg cursor-pointer transition-colors ${selectedUserIds.includes(user.id) ? 'bg-primary-50' : 'hover:bg-default-100'}`}
//                                             onClick={() => toggleUser(user.id)}
//                                         >
//                                             <div className="flex items-center gap-3">
//                                                 <Avatar src={user.avatarUrl || undefined} name={user.username?.[0]?.toUpperCase() || 'U'} size="sm" />
//                                                 <div>
//                                                     <p className="font-medium text-sm">{user.username || 'Khách'}</p>
//                                                     <p className="text-xs text-default-500">{user.email}</p>
//                                                 </div>
//                                             </div>
//                                             <Checkbox
//                                                 isSelected={selectedUserIds.includes(user.id)}
//                                                 onValueChange={() => toggleUser(user.id)}
//                                             />
//                                         </div>
//                                     ))
//                                 ) : (
//                                     <div className="text-center py-8 text-default-500">
//                                         Không tìm thấy người dùng nào.
//                                     </div>
//                                 )}
//                             </div>
//                         </ModalBody>
//                         <ModalFooter>
//                             <Button color="danger" variant="light" onPress={onClose}>
//                                 Hủy
//                             </Button>
//                             <Button
//                                 color="primary"
//                                 onPress={handleAssign}
//                                 isLoading={isAssigning}
//                                 isDisabled={selectedUserIds.length === 0}
//                             >
//                                 Gán Voucher
//                             </Button>
//                         </ModalFooter>
//                     </>
//                 )}
//             </ModalContent>
//         </Modal>
//     )
// }
