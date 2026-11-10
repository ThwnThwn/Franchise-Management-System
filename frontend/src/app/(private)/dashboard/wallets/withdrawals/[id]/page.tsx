import WithdrawalDetailPageView from "@/features/wallets/pages/WithdrawalDetailPageView"

export default async function WithdrawalDetailPage({
    params
}: {
    params: Promise<{ id: string }>
}) {
    const { id } = await params
    return <WithdrawalDetailPageView id={id} />
}
