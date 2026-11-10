import ApplicationDetailView from "@/features/franchise/pages/ApplicationDetailView"

export default async function ApplicationDetailAdminPage({
    params
}: {
    params: Promise<{ id: string }>
}) {
    const { id } = await params
    return <ApplicationDetailView id={id} />
}
