/**
 * Staff Management Page
 * Placeholder for employee management
 */

"use client";

import { Card, CardBody, CardHeader } from "@heroui/react";
import { Users } from "lucide-react";

export default function StaffManagementPage() {
    return (
        <div className="min-h-screen p-4 md:p-6 lg:p-8">
            <div className="max-w-7xl mx-auto">
                {/* Page Header */}
                <div className="mb-8">
                    <div className="flex items-center gap-3 mb-2">
                        <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center">
                            <Users className="w-6 h-6 text-white" />
                        </div>
                        <div>
                            <h1 className="text-3xl lg:text-4xl font-bold text-foreground">
                                Quản lý nhân viên
                            </h1>
                            <p className="text-foreground/60 mt-1">
                                Quản lý thông tin nhân viên và ca làm việc
                            </p>
                        </div>
                    </div>
                </div>

                {/* Placeholder Content */}
                <Card className="bg-gradient-to-br from-white/80 to-white/60 backdrop-blur-lg border border-primary-100 shadow-lg">
                    <CardHeader className="flex flex-col items-start px-6 pt-6 pb-4">
                        <h3 className="text-xl font-bold text-foreground">
                            Danh sách nhân viên
                        </h3>
                        <p className="text-sm text-foreground/60 mt-1">
                            Trang này đang được phát triển
                        </p>
                    </CardHeader>
                    <CardBody className="px-6 pb-6">
                        <div className="flex items-center justify-center py-20">
                            <div className="text-center">
                                <Users className="w-16 h-16 text-primary/30 mx-auto mb-4" />
                                <p className="text-foreground/60">
                                    Tính năng quản lý nhân viên sẽ được bổ sung sau
                                </p>
                            </div>
                        </div>
                    </CardBody>
                </Card>
            </div>
        </div>
    );
}
