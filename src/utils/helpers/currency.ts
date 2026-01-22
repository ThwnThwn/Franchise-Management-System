export const formatCurrency = (n: number) => {
    if (n < 0) n *= -1
    return new Intl.NumberFormat("vi-VN").format(n).replace(/\./g, ",")
}

export const formatCurrencyWithSymbol = (n: number) => {
    if (n < 0) n *= -1
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND"
    })
        .format(n)
        .replace(/\./g, ",")
}
