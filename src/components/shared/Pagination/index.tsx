type PaginationProps = {
    currentPage: number
    totalPages: number
    onPageChange: (page: number) => void
}

export default function Pagination({
    currentPage,
    totalPages,
    onPageChange
}: PaginationProps) {
    const renderPageNumbers = () => {
        const pages = []
        const maxPagesToShow = 5
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2))
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1)

        if (endPage - startPage < maxPagesToShow - 1) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1)
        }

        for (let i = startPage; i <= endPage; i++) {
            pages.push(
                <button
                    key={i}
                    onClick={() => onPageChange(i)}
                    className={`px-4 py-2 mx-1 rounded-xl transition-all ${i === currentPage
                        ? "bg-[#FFB74D] text-[#3E2723] font-bold shadow-lg"
                        : "bg-white/10 text-[#D7CCC8] hover:bg-white/20 border border-white/10"
                        }`}
                >
                    {i}
                </button>
            )
        }

        return pages
    }

    if (totalPages <= 1) return null

    return (
        <div className="flex items-center justify-center mt-8 space-x-2">
            <button
                onClick={() => onPageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className={`px-4 py-2 rounded-xl transition-all ${currentPage === 1
                    ? "bg-white/5 text-white/20 cursor-not-allowed"
                    : "bg-white/10 text-[#D7CCC8] hover:bg-white/20 border border-white/10"
                    }`}
            >
                Trước
            </button>

            {renderPageNumbers()}

            <button
                onClick={() => onPageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                className={`px-4 py-2 rounded-xl transition-all ${currentPage === totalPages
                    ? "bg-white/5 text-white/20 cursor-not-allowed"
                    : "bg-white/10 text-[#D7CCC8] hover:bg-white/20 border border-white/10"
                    }`}
            >
                Sau
            </button>
        </div>
    )
}
