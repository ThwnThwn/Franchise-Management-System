"use client"

import { ShiftRuleSettingsResponse } from "@/features/shifts/models/response"

export const useGetShiftRuleSettings = (storeId?: string) => {
    const mockData: ShiftRuleSettingsResponse = {
        global: {
            id: "global-1",
            allowShuttle: false,
            allowShiftSwap: true,
            minHoursBetweenShifts: 8,
            maxHoursPerWeek: 44,
            updatedAt: new Date().toISOString()
        },
        // Giả lập mảng store
        stores: storeId
            ? []
            : [
                  {
                      id: "s-1",
                      storeId: "store-alpha",
                      allowShuttle: true,
                      allowShiftSwap: true,
                      minHoursBetweenShifts: 10,
                      maxHoursPerWeek: 40,
                      updatedAt: new Date().toISOString()
                  },
                  {
                      id: "s-2",
                      storeId: "store-beta",
                      allowShuttle: false,
                      allowShiftSwap: false,
                      minHoursBetweenShifts: 6,
                      maxHoursPerWeek: 50,
                      updatedAt: new Date().toISOString()
                  }
              ]
    }

    return {
        data: mockData,
        isLoading: false,
        isError: false
    }
}

export const useSaveShiftRule = () => {
    return {
        mutate: (req: any) => console.log("Mock Save Rule:", req),
        isPending: false
    }
}
