import ContractDetailView from "@/features/franchise/pages/ContractDetailView"

export default async function ContractDetailAdminPage({
    params
}: {
    params: Promise<{ id: string }>
}) {
    const { id } = await params
    return <ContractDetailView id={id} />
}
