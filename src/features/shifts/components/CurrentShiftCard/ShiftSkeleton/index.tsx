"use client"

import React from "react"
import { Spinner, Card, CardBody } from "@heroui/react"

/**
 * SHIFT SKELETON
 * Hiển thị khi đang loading thông tin ca làm từ .NET
 */
export const ShiftSkeleton = () => (
    <Card className="w-full bg-default-50 border-none shadow-none py-10">
        <CardBody className="flex flex-col items-center justify-center gap-3">
            <Spinner label="Đang tải thông tin ca làm..." color="primary" labelColor="primary" />
        </CardBody>
    </Card>
)
