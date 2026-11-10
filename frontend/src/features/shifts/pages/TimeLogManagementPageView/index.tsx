"use client"

import { Spinner } from "@heroui/react"
import { ClipboardCheck } from "lucide-react"

// Import nội bộ module
import {
    TimeLogToolbar,
    TimeLogTable,
    TimeLogCalendar,
    TimeLogUpdateModal,
    EmptyTimeLogState
} from "@/features/shifts/components"
import { useTimeLogLogic } from "@/features/shifts/hooks"
import { useGetAllTimeLogs, useGetMyTimeLogHistory } from "@/features/shifts/hooks"

// Shared components & Dashboard
import { DashboardHeader } from "@/features/dashboard"
import { useScrollSnap } from "@/hooks"
import { PaginationStyled } from "@/components"

export default function TimeLogManagementPageView() {
    // 1. Lấy "Bộ não" điều hướng
    const {
        isManager,
        currentDate,
        viewMode,
        setViewMode,
        status,
        setStatus,
        staffName,
        setStaffName,
        pagination,
        setPagination,
        queryParams,
        handleNext,
        handlePrev,
        handleToday,
        handleJumpToDate,
        modal
    } = useTimeLogLogic()

    // 2. Gọi API tương ứng với Role
    // Manager dùng useGetAllTimeLogs, Staff dùng useGetMyTimeLogHistory
    const staffQuery = useGetMyTimeLogHistory({
        query: queryParams,
        pagination: pagination,
        enabled: !isManager
    })

    const managerQuery = useGetAllTimeLogs({
        query: queryParams,
        pagination: pagination,
        enabled: isManager
    })

    const { data, isLoading } = isManager ? managerQuery : staffQuery
    const logs = data?.items || []

    const calendarSnapRef = useScrollSnap({
        isActive: viewMode === "calendar",
        thresholdTop: -30,
        offset: 100
    })

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-3">
            {/* Header động theo Role */}
            <div className="flex flex-col md:flex-row justify-between">
                <DashboardHeader
                    icon={<ClipboardCheck className="text-white" />}
                    title={isManager ? "Quản Lý Chấm Công" : "Nhật Ký Làm Việc"}
                    description={
                        isManager
                            ? "Theo dõi và phê duyệt lịch sử ra vào ca của nhân viên trong Store"
                            : "Xem lại lịch sử chấm công và phản hồi từ quản lý"
                    }
                />
            </div>

            {/* Toolbar hợp nhất */}
            <section className="animate-in fade-in slide-in-from-top-4 duration-500">
                <TimeLogToolbar
                    isManager={isManager}
                    currentDate={currentDate}
                    viewMode={viewMode}
                    onViewChange={setViewMode}
                    status={status}
                    onStatusChange={setStatus}
                    onNext={handleNext}
                    onPrev={handlePrev}
                    onToday={handleToday}
                    onJump={handleJumpToDate}
                    staffSearch={staffName}
                    onStaffSearchChange={setStaffName}
                />
            </section>

            {/* Main Content Area */}
            <div className="min-h-100">
                {isLoading ? (
                    <div className="flex flex-col items-center justify-center py-20">
                        <Spinner size="lg" label="Đang đồng bộ dữ liệu chấm công..." />
                    </div>
                ) : logs.length > 0 ? (
                    <div className="animate-in fade-in duration-700">
                        {viewMode === "list" ? (
                            <>
                                <TimeLogTable
                                    selectedDate={currentDate}
                                    data={logs}
                                    isLoading={isLoading}
                                    isManager={isManager}
                                    onEditClick={modal.onOpen}
                                />
                                {(data?.items || []).length > 0 &&
                                    (data?.totalPages || 0) > 1 &&
                                    !isLoading && (
                                        <PaginationStyled
                                            page={pagination.page ?? 1}
                                            total={data?.totalPages ?? 10}
                                            onChange={(page: number) =>
                                                setPagination((prev) => {
                                                    return {
                                                        ...prev,
                                                        page: page
                                                    }
                                                })
                                            }
                                        />
                                    )}
                            </>
                        ) : (
                            <div
                                ref={calendarSnapRef}
                                className="bg-white p-2 rounded-3xl shadow-sm border border-divider"
                            >
                                <TimeLogCalendar
                                    logs={logs}
                                    currentDate={currentDate}
                                    onEventClick={modal.onOpen} // Click vào event trên lịch cũng mở modal
                                />
                            </div>
                        )}
                    </div>
                ) : (
                    <EmptyTimeLogState
                        message={staffName ? `Không tìm thấy nhân viên "${staffName}"` : undefined}
                    />
                )}
            </div>

            {/* Modal chỉnh sửa (Chỉ thực sự hoạt động Mutation nếu là Manager) */}
            <TimeLogUpdateModal
                isOpen={modal.isOpen}
                onClose={modal.onClose}
                data={modal.selectedLog}
                isReadOnly={!isManager}
            />
        </div>
    )
}
