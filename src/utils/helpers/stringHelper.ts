export function maskEmail(email: string) {
    const [username, domain] = email.split("@")
    if (username.length <= 2) {
        return username[0] + "***@" + domain
    }
    return username.slice(0, 2) + "***@" + domain
}
