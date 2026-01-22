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
