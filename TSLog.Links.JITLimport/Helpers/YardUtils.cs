using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport.Helpers;

public static class YardUtils
{
    private static IServiceProvider _serviceProvider;
    // Средее время (ПО УМОЛЧАНИЮ) выгрузки/погрузки паллеты/коробки/штуки.
    private const int PalletUnloadTime = 93;
    private const int BoxUnloadTime = 20;
    private const int PieceUnloadTime = 25;
    private const int PalletLoadTime = 93;
    private const int BoxLoadTime = 20;
    private const int PieceLoadTime = 25;

    /// <summary>
    /// "Постоянния" часть времени операции двора ПО УМОЛЧАНИЮ (не зависит от состава заказов/приходов),
    /// 8 минут 15 секунд на постановку машины,
    /// 6 минут 16 секунд на снятие машины (получено от Алексея Охотникова 2022.09.09).
    /// СЕЙЧАС НЕ ИСПОЛЬЗУЕТСЯ. ВМЕСТО ЭТОГО ПРОИСХОДИТ ОБРАЩЕНИЕ В БД.
    /// </summary>
    private const int DefaultConstantOperationDuration = (8 * 60 + 15) + (6 * 60 + 15);

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static int _GetFillingPlanOperationDuration(OperationType operationType, VehicleKind vehicleKind, dynamic[] actions)
    {
        var yardDbConnectService = _serviceProvider.GetService<IYardDbConnectService>();
        var standardTotalDuration = yardDbConnectService.GetYardOperationDuration(operationType, vehicleKind, out int standardConstantDuration, out int standardVariableDuration);
        var totalTime = 0.0;
        foreach (var action in actions)
        {
            /*
                Автор - Буров Михаил.
                Определяем ПЛАНОВОЕ время операции двора в зависимости от состава заказов/приходов в операции двора.
                Смотрим все товарные позиции всех заказов/приходов в операции двора.
                Для каждой позиции смотрим сначала затарку, если затарка есть считаем количество паллет и коробок по затарке (от количества товаров),
                есть фиксированное время загрузки/выгрузки паллеты/коробки - из этого и складывается плановое время работ с конкретной товарной позицией.
                Если затарки нет, то пробуем определить плановое время по объёму или весу товара в товарной позицией.
                Сначала смотрим вес, потом объём.
                Смотри какую часть объёма (веса) занимает конкретная товарная позиция от ВСЕГО объёма (веса), 
                умножаем плановое максимальное время (при условии загрузки/выгрузки машины ЦЕЛИКОМ) на соответствующий коэффициент.
                Если не удалось рассчитать плановое время ни по затарке, ни по весу ни по объёму - считаем время по СОБРАННЫМ ТЕ заказа (ТОЛЬКО для отгрузок).
            */
            var kind = action.kind == "O" ? ActionKind.Outgo : ActionKind.Income;
            var palletTime = action.kind == "O" ? PalletLoadTime : PalletUnloadTime;
            var boxTime = action.kind == "O" ? BoxLoadTime : BoxUnloadTime;
            var pieceTime = action.kind == "O" ? PieceLoadTime : PieceUnloadTime;
            string id = action.id;
            var lines = yardDbConnectService.GetYardWarehouseActionLines(kind, id);
            var actionTime = 0.0;
            foreach (var line in lines)
            {
                // Смотрим по затарке.
                var lineTime = 0.0;
                if (line.PlanPalletsCount > 0.0 || line.PlanBoxesCount > 0.0)
                    lineTime += line.PlanPalletsCount * palletTime + line.PlanBoxesCount * boxTime;
                else
                {
                    // Смотрим по весу.
                    if (line.PlanGrossWeight > 0.0)
                        // Килограммы переводим в милиграммы.
                        lineTime += line.PlanGrossWeight / (vehicleKind.Capacity * 1000 * 1000) * standardTotalDuration;
                    // Смотрим по объёму
                    else if (line.PlanVolume > 0.0)
                        lineTime += line.PlanVolume / (vehicleKind.CargoVolume * 1000 * 1000 * 1000) * standardTotalDuration;

                    // Новый блок по задаче #2416 Двор. Новая функция. План факт.
                    /*
                      Идея в том, что мы определяем количество "штук" в товарной позиции (SET = ST, 5 кг = 1 шт., 50 м = 1 шт).
                      Считаем "альтернативное" время загрузки/выгрузки товарной позиции как количество "штук", умноженное на время загрузки/выгрузке 1 штуки.
                      Если "альтернативное" время загрузки/выгрузки позиции более времени, рассчитанного по алгоритму, то берём "альтернативное" время.
                    */
                    var eachesCount = 0.0;
                    if (line.QuantityMeasureUnit.Code == "ST" || line.QuantityMeasureUnit.Code == "SET")
                        eachesCount = line.PlanQuantity;
                    else if (line.QuantityMeasureUnit.Code == "KG")
                        eachesCount = line.PlanQuantity / 5.0;
                    else if (line.QuantityMeasureUnit.Code == "G")
                        eachesCount = line.PlanQuantity / 5.0 / 1000.0;
                    else if (line.QuantityMeasureUnit.Code == "M")
                        eachesCount = line.PlanQuantity / 50.0;
                    else if (line.QuantityMeasureUnit.Code == "CM")
                        eachesCount = line.PlanQuantity / 50.0 / 100.0;

                    var altLineTime = eachesCount * boxTime;
                    if (lineTime < altLineTime)
                        lineTime = altLineTime;
                }
                actionTime += lineTime;
            }
            if (actionTime <= 0.0 && action.kind == "O")
            {
                // Смотри по собранным заказам (ТОЛЬКО отгрузки)
                yardDbConnectService.GetYardAssembliedOutgo(id, out double palletsCount, out double eachesCount);
                actionTime = palletsCount * palletTime + eachesCount * pieceTime;
            }
            totalTime += actionTime;
        }
        totalTime = Math.Round(totalTime);

        if (totalTime <= 0.0)
            totalTime = standardTotalDuration;
        //  2022.09.12 добавил учёт постоянного (не зависящего от состава заказов) времени операции по указанию Алексея Охотникова .
        else
            totalTime += DefaultConstantOperationDuration;

        // Если продолжительность операции двора больше чем "стандартное" (при загрузке/выгрузке полной машины) то берём "стандартное".
        if (totalTime > standardTotalDuration)
            totalTime = standardTotalDuration;

        return (int)totalTime;
    }

    public static dynamic[] WarehouseActionsToDynamic(WarehouseAction[] actions)
    {
        var result = new List<dynamic>();
        foreach (var action in actions)
            result.Add(WarehouseActionToDynamic(action));
        return [.. result];
    }

    private static dynamic WarehouseActionToDynamic(WarehouseAction action)
    {
        dynamic result = new ExpandoObject();
        result.kind = action.Kind == ActionKind.Outgo ? "O" : "I";
        result.id = action.Id;
        result.partnerActionId = action.PartnerActionId;
        result.partnerId = action.Partner.Id;
        result.partnerName = action.Partner.Name;
        result.planActionDate = action.PlanActionDate;
        result.insertDate = action.InsertDate;

        return result;
    }
}
