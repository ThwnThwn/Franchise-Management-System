import { CampaignDetailView } from "@/features/campaigns/pages/CampaignDetailView"
import { campaignApi } from "@/features/campaigns/services/campaignApi"
import { notFound } from "next/navigation"

export const dynamic = "force-dynamic"

interface PageProps {
    params: Promise<{ id: string }>
}

export default async function CampaignDetailPage({ params }: PageProps) {
    const { id } = await params
    const isNew = id === "new"

    let initialData = undefined

    if (!isNew) {
        try {
            initialData = await campaignApi.getById({ id })

            if (!initialData) {
                notFound()
            }
        } catch {
            notFound()
        }
    }

    return <CampaignDetailView campaignId={id} initialData={initialData} />
}
