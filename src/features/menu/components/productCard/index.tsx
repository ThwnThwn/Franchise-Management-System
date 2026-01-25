import { CardStyled, ImageStyled } from "@/components"
import { CardBody, CardFooter } from "@heroui/react"
import React from "react"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { TYPE } from "../../constants"
import { GetProductListItemDto, ProductSizeListItemDto } from "../../models/schema/respone"

export function CardProductStyled({
    getProductListItemDto
}: {
    getProductListItemDto: GetProductListItemDto
}) {
    return (
        <CardStyled className="hover:shadow-xl interactive-scale" shadow="sm">
            <CardBody className="overflow-visible ">
                <ImageStyled
                    alt={getProductListItemDto.name}
                    className="w-[300px] object-cover h-[188px] shadow-lg"
                    src={getProductListItemDto.imageUrl}
                    width={300}
                    height={188}
                />
                {getProductListItemDto.isActive === false && (
                    <span className="absolute top-3 right-3 bg-red-500 text-white px-3 py-1 text-xs rounded z-10">
                        {"fleet.status_unavailable"}
                    </span>
                )}
            </CardBody>
            <CardFooter className="flex flex-col text-small justify-between">
                <div className="flex justify-between items-center text-2xl mb-2">
                    <b>{getProductListItemDto.name}</b>
                </div>
                {/* </div> */}
                <hr className=" text-gray-300 border-1 w-full m-1" />

                <div className=" flex items-center justify-center mt-2 mb-2 p-2 ">
                    <span className="text-2xl font-bold text-green-600 whitespace-nowrap">
                        {formatCurrencyWithSymbol(
                            getProductListItemDto.sizes.find(
                                (s: ProductSizeListItemDto) => s.name === TYPE.MIN_SIZE
                            )?.basePrice ?? 0
                        )}{" "}
                        &nbsp;
                    </span>

                    {/* <span className="text-black">{"   " + "vehicle_model.vnd_per_day"}</span> */}
                </div>
                {/* segmentName */}
                {/* <div className="grid grid-cols-2 gap-2 mt-2 mr-0 max-w-60 w-full">
                    <div className="flex gap-2 ">
                        <Armchair className="flex w-6 h-6" />
                        <span>{vehicleModel.segment.name}</span>
                    </div>

                    <div className="flex gap-2 justify-end">
                        <BatteryChargingIcon className="h-6 w-6" />
                        <span>{vehicleModel.ecoRangeKm} Km</span>
                    </div>

                    <div className="flex gap-2">
                        <Users className="h-6 w-6" />
                        <span>
                            {vehicleModel.seatingCapacity} {t("vehicle_model.seating_capacity")}
                        </span>
                    </div>

                    <div className="flex gap-2 justify-end">
                        <Icon icon="mdi:airbag" width="20" height="20" className="text-gray-700" />
                        <span>
                            {vehicleModel.numberOfAirbags} {t("vehicle_model.airbag")}
                        </span>
                    </div>
                </div> */}
            </CardFooter>
        </CardStyled>
    )
}
