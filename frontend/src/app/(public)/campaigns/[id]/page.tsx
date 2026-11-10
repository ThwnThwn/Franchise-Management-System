import { CampaignPublicDetailView } from "@/features/campaigns/pages/CampaignPublicDetailView"
import { campaignApi } from "@/features/campaigns/services"
import { notFound } from "next/navigation"

interface PageProps {
    params: Promise<{ id: string }>
}

export default async function CampaignPublicDetailPage({ params }: PageProps) {
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

    if (!initialData) return null

    return <CampaignPublicDetailView campaign={initialData} />
}
