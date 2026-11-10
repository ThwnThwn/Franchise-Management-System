"use client"

import React, { useState } from "react"
import { Tag, Search, Plus, Pencil, Trash2 } from "lucide-react"
import {
    Modal,
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Input,
    Textarea,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Chip,
    Tooltip,
    Spinner
} from "@heroui/react"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { InputStyled, ButtonIconStyled, TableStyled, AlertModal } from "@/components"
import {
    useGetAllCategories,
    useCreateCategory,
    useUpdateCategory,
    useDeleteCategory
} from "../hooks/useCategory"
import { Category } from "../models/categories/response"

// ── Types ─────────────────────────────────────────────────
type CategoryFormState = { name: string; description: string }
const EMPTY_FORM: CategoryFormState = { name: "", description: "" }

// ── Main Page ─────────────────────────────────────────────
export default function CategoriesPage() {
    const { data: categories = [], isLoading } = useGetAllCategories()

    // Search
    const [search, setSearch] = useState("")
    const filtered = categories.filter((c) =>
        c.name.toLowerCase().includes(search.toLowerCase())
    )

    // ── Create Modal ──────────────────────────────────────
    const [isCreateOpen, setIsCreateOpen] = useState(false)
    const [createForm, setCreateForm] = useState<CategoryFormState>(EMPTY_FORM)
    const [createErrors, setCreateErrors] = useState<Partial<CategoryFormState>>({})

    const createMutation = useCreateCategory({
        onSuccess: () => {
            setIsCreateOpen(false)
            setCreateForm(EMPTY_FORM)
        }
    })

    const validateCreate = () => {
        const errs: Partial<CategoryFormState> = {}
        if (!createForm.name.trim()) errs.name = "Tên danh mục không được để trống"
        setCreateErrors(errs)
        return Object.keys(errs).length === 0
    }

    // ── Edit Modal ────────────────────────────────────────
    const [editingCategory, setEditingCategory] = useState<Category | null>(null)
    const [editForm, setEditForm] = useState<CategoryFormState>(EMPTY_FORM)
    const [editErrors, setEditErrors] = useState<Partial<CategoryFormState>>({})

    const updateMutation = useUpdateCategory({
        onSuccess: () => {
            setEditingCategory(null)
        }
    })

    const openEdit = (cat: Category) => {
        setEditingCategory(cat)
        setEditForm({ name: cat.name, description: cat.description ?? "" })
        setEditErrors({})
    }

    const validateEdit = () => {
        const errs: Partial<CategoryFormState> = {}
        if (!editForm.name.trim()) errs.name = "Tên danh mục không được để trống"
        setEditErrors(errs)
        return Object.keys(errs).length === 0
    }

    // ── Delete Modal ──────────────────────────────────────
    const [deletingCategory, setDeletingCategory] = useState<Category | null>(null)

    const deleteMutation = useDeleteCategory({
        onSuccess: () => setDeletingCategory(null)
    })

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            <DashboardHeader
                icon={<Tag className="w-6 h-6 text-white" />}
                title="Quản lý danh mục"
                description="Tổ chức sản phẩm theo nhóm danh mục"
            />

            {/* ── TOOLBAR ── */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <InputStyled
                    placeholder="Tìm kiếm danh mục..."
                    startContent={<Search size={18} className="text-slate-400" />}
                    className="w-full sm:max-w-sm"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                />
                <Button
                    color="primary"
                    className="btn-gradient font-semibold"
                    startContent={<Plus size={18} />}
                    onPress={() => {
                        setCreateForm(EMPTY_FORM)
                        setCreateErrors({})
                        setIsCreateOpen(true)
                    }}
                >
                    Thêm danh mục
                </Button>
            </div>

            {/* ── COUNT ── */}
            <div className="mb-4 text-sm text-slate-600">
                Tổng:{" "}
                <span className="font-semibold text-slate-900">{filtered.length}</span> danh mục
            </div>

            {/* ── TABLE ── */}
            <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
                <TableStyled removeWrapper aria-label="Danh sách danh mục">
                    <TableHeader>
                        <TableColumn width="40%">TÊN DANH MỤC</TableColumn>
                        <TableColumn width="45%">MÔ TẢ</TableColumn>
                        <TableColumn width="15%" align="center">
                            THAO TÁC
                        </TableColumn>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            Array.from({ length: 5 }).map((_, i) => (
                                <TableRow key={i}>
                                    <TableCell colSpan={3} className="text-center py-6">
                                        <Spinner size="sm" />
                                    </TableCell>
                                </TableRow>
                            ))
                        ) : filtered.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={3} className="text-center py-12 text-slate-400">
                                    {search
                                        ? `Không tìm thấy danh mục "${search}"`
                                        : "Chưa có danh mục nào"}
                                </TableCell>
                            </TableRow>
                        ) : (
                            filtered.map((cat) => (
                                <TableRow
                                    key={cat.id}
                                    className="border-b border-slate-50 last:border-0 hover:bg-slate-50/50 transition-colors"
                                >
                                    {/* Name */}
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            <div className="w-8 h-8 rounded-lg bg-primary-50 flex items-center justify-center shrink-0">
                                                <Tag size={14} className="text-primary-500" />
                                            </div>
                                            <div>
                                                <div className="font-semibold text-slate-900">
                                                    {cat.name}
                                                </div>
                                                <div className="text-xs text-slate-400 font-mono">
                                                    {cat.id.slice(0, 8)}...
                                                </div>
                                            </div>
                                        </div>
                                    </TableCell>

                                    {/* Description */}
                                    <TableCell className="text-sm text-slate-600">
                                        {cat.description || (
                                            <span className="text-slate-300 italic">
                                                Không có mô tả
                                            </span>
                                        )}
                                    </TableCell>

                                    {/* Actions */}
                                    <TableCell>
                                        <div className="flex items-center justify-center gap-1">
                                            <Tooltip content="Chỉnh sửa" color="primary">
                                                <ButtonIconStyled
                                                    variant="light"
                                                    color="primary"
                                                    className="opacity-70 hover:opacity-100"
                                                    onPress={() => openEdit(cat)}
                                                >
                                                    <Pencil size={16} />
                                                </ButtonIconStyled>
                                            </Tooltip>
                                            <Tooltip content="Xóa" color="danger">
                                                <ButtonIconStyled
                                                    variant="light"
                                                    color="danger"
                                                    className="opacity-70 hover:opacity-100"
                                                    onPress={() => setDeletingCategory(cat)}
                                                >
                                                    <Trash2 size={16} />
                                                </ButtonIconStyled>
                                            </Tooltip>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </TableStyled>
            </div>

            {/* ── CREATE MODAL ── */}
            <Modal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} size="md">
                <ModalContent>
                    <ModalHeader className="text-slate-900">Thêm danh mục mới</ModalHeader>
                    <ModalBody className="gap-4">
                        <Input
                            label="Tên danh mục"
                            placeholder="Ví dụ: Cà phê, Trà trái cây..."
                            isRequired
                            value={createForm.name}
                            onValueChange={(v) => setCreateForm((p) => ({ ...p, name: v }))}
                            isInvalid={!!createErrors.name}
                            errorMessage={createErrors.name}
                            variant="bordered"
                        />
                        <Textarea
                            label="Mô tả"
                            placeholder="Mô tả ngắn về danh mục này..."
                            value={createForm.description}
                            onValueChange={(v) =>
                                setCreateForm((p) => ({ ...p, description: v }))
                            }
                            variant="bordered"
                            minRows={3}
                        />
                    </ModalBody>
                    <ModalFooter>
                        <Button variant="light" onPress={() => setIsCreateOpen(false)}>
                            Hủy
                        </Button>
                        <Button
                            color="primary"
                            className="btn-gradient"
                            isLoading={createMutation.isPending}
                            onPress={() => {
                                if (!validateCreate()) return
                                createMutation.mutate({
                                    name: createForm.name.trim(),
                                    description: createForm.description.trim() || undefined
                                })
                            }}
                        >
                            Tạo danh mục
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>

            {/* ── EDIT MODAL ── */}
            <Modal
                isOpen={!!editingCategory}
                onClose={() => setEditingCategory(null)}
                size="md"
            >
                <ModalContent>
                    <ModalHeader className="text-slate-900">Chỉnh sửa danh mục</ModalHeader>
                    <ModalBody className="gap-4">
                        <Input
                            label="Tên danh mục"
                            isRequired
                            value={editForm.name}
                            onValueChange={(v) => setEditForm((p) => ({ ...p, name: v }))}
                            isInvalid={!!editErrors.name}
                            errorMessage={editErrors.name}
                            variant="bordered"
                        />
                        <Textarea
                            label="Mô tả"
                            value={editForm.description}
                            onValueChange={(v) => setEditForm((p) => ({ ...p, description: v }))}
                            variant="bordered"
                            minRows={3}
                        />
                    </ModalBody>
                    <ModalFooter>
                        <Button variant="light" onPress={() => setEditingCategory(null)}>
                            Hủy
                        </Button>
                        <Button
                            color="primary"
                            className="btn-gradient"
                            isLoading={updateMutation.isPending}
                            onPress={() => {
                                if (!validateEdit() || !editingCategory) return
                                updateMutation.mutate({
                                    id: editingCategory.id,
                                    req: {
                                        name: editForm.name.trim(),
                                        description: editForm.description.trim() || undefined
                                    }
                                })
                            }}
                        >
                            Lưu thay đổi
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>

            {/* ── DELETE CONFIRM ── */}
            <AlertModal
                isOpen={!!deletingCategory}
                onClose={() => setDeletingCategory(null)}
                onConfirm={() => {
                    if (deletingCategory) deleteMutation.mutate(deletingCategory.id)
                }}
                header="Xác nhận xóa danh mục"
                variant="danger"
                confirmText="Xóa"
                isLoading={deleteMutation.isPending}
                body={
                    <div className="text-slate-600">
                        Bạn có chắc muốn xóa danh mục{" "}
                        <span className="font-bold text-slate-900">
                            {deletingCategory?.name}
                        </span>
                        ?
                        <div className="mt-2 text-xs text-slate-400 bg-slate-50 p-2 rounded">
                            ⚠️ Các sản phẩm thuộc danh mục này vẫn còn trong hệ thống.
                        </div>
                    </div>
                }
            />
        </div>
    )
}
