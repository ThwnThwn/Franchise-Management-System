// import { RecipeViewRes } from "@/features/products/models/recipes/response"

// /**
//  * Hàm sinh dữ liệu mẫu để test UI
//  * @param variantIds Danh sách các ID cần tạo công thức
//  */
// export const generateMockRecipes = (variantIds: string[]): RecipeViewRes[] => {
//     const ingredientPool = [
//         { name: "Cà phê hạt Blend", unit: "g" },
//         { name: "Sữa tươi không đường", unit: "ml" },
//         { name: "Sữa đặc Larose", unit: "ml" },
//         { name: "Syrup Vanilla", unit: "ml" },
//         { name: "Bột Matcha Nhật", unit: "g" },
//         { name: "Trà đen Lộc Phát", unit: "ml" },
//         { name: "Đường nước", unit: "ml" },
//         { name: "Trân châu đen", unit: "g" }
//     ]

//     const recipes: RecipeViewRes[] = []

//     variantIds.forEach((vId, index) => {
//         // Case 1: Món không có công thức (để test Empty Content)
//         if (index === 0) return

//         // Case 2: Món có 1 nguyên liệu
//         if (index === 1) {
//             recipes.push({
//                 variantId: vId,
//                 name: "Đá viên",
//                 quantity: 150,
//                 baseUnit: "g"
//             })
//             return
//         }

//         // Case 3: Món có nhiều nguyên liệu (Random từ pool)
//         const randomCount = Math.floor(Math.random() * 3) + 2 // Sinh ra 2-4 nguyên liệu
//         for (let i = 0; i < randomCount; i++) {
//             const ingredient = ingredientPool[Math.floor(Math.random() * ingredientPool.length)]
//             recipes.push({
//                 variantId: vId,
//                 name: ingredient.name,
//                 quantity: Math.floor(Math.random() * 100) + 10,
//                 baseUnit: ingredient.unit
//             })
//         }
//     })

//     return recipes
// }
