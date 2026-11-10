import { create } from "zustand"

type SideBarItemStore = {
    activeMenuKey?: string
    setActiveMenuKey: (activeMenuKey?: string) => void
}

export const useSideBarItemStore = create<SideBarItemStore>((set) => ({
    activeMenuKey: "",
    setActiveMenuKey: (activeMenuKey) => set({ activeMenuKey })
}))
