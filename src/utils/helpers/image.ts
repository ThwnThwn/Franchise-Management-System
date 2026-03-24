import { Area } from "react-easy-crop"

const createImage = (url: string): Promise<HTMLImageElement> =>
    new Promise((resolve, reject) => {
        const image = new Image()
        image.addEventListener("load", () => resolve(image))
        image.addEventListener("error", (error) => reject(error))
        image.setAttribute("crossOrigin", "anonymous")
        image.src = url
    })

export async function getCroppedImage(imageSrc: string, crop: Area): Promise<Blob> {
    const image = await createImage(imageSrc)
    const canvas = document.createElement("canvas")
    const ctx = canvas.getContext("2d")!

    canvas.width = crop.width
    canvas.height = crop.height

    ctx.drawImage(
        image, //
        crop.x,
        crop.y,
        crop.width,
        crop.height,
        0,
        0,
        crop.width,
        crop.height
    )

    return new Promise((resolve) => {
        canvas.toBlob((blob) => {
            resolve(blob!)
        }, "image/jpeg")
    })
}

export const resizeImage = (file: File, maxWidth: number = 1200): Promise<Blob> => {
    return new Promise((resolve, reject) => {
        const reader = new FileReader()
        reader.readAsDataURL(file)
        reader.onload = (event) => {
            const img = new Image()
            img.src = event.target?.result as string
            img.onload = () => {
                const canvas = document.createElement("canvas")
                let width = img.width
                let height = img.height

                if (width > maxWidth) {
                    height = (maxWidth / width) * height
                    width = maxWidth
                }

                canvas.width = width
                canvas.height = height
                const ctx = canvas.getContext("2d")
                ctx?.drawImage(img, 0, 0, width, height)

                canvas.toBlob(
                    (blob) => {
                        if (blob) resolve(blob)
                        else reject(new Error("Canvas to Blob failed"))
                    },
                    "image/jpeg",
                    0.8
                )
            }
        }
        reader.onerror = (err) => reject(err)
    })
}
