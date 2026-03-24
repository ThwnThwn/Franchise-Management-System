import ContractDetailAdminView from "@/features/franchise/pages/ContractDetailAdminView"

export default async function ContractDetailAdminPage({
    params
}: {
    params: Promise<{ id: string }>
}) {
    const { id } = await params
    return <ContractDetailAdminView id={id} />
}
