import ApplicationDetailAdminView from "@/features/franchise/pages/ApplicationDetailAdminView"

export default async function ApplicationDetailAdminPage({
    params
}: {
    params: Promise<{ id: string }>
}) {
    const { id } = await params
    return <ApplicationDetailAdminView id={id} />
}
