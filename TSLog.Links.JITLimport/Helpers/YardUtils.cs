using Ganss.Xss;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Links.JITLimport.Services;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport.Helpers;

public static class YardUtils
{
    private static IServiceProvider _serviceProvider;

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    //public int _GetFillingPlanOperationDuration(OperationType operationType, VehicleKind vehicleKind, dynamic[] actions)
    //{
    //    var yardDbConnectService = _serviceProvider.GetService<YardDbConnectService>();
    //    var standardTotalDuration = _yardDbConnectService.GetYardOperationDuration(operationType, vehicleKind, out int standardConstantDuration, out int standardVariableDuration);
    //    var totalTime = 0.0;
    //    foreach (var action in actions)
    //    {
    //        /*
    //            Автор - Буров Михаил.
    //            Определяем ПЛАНОВОЕ время операции двора в зависимости от состава заказов/приходов в операции двора.
    //            Смотрим все товарные позиции всех заказов/приходов в операции двора.
    //            Для каждой позиции смотрим сначала затарку, если затарка есть считаем количество паллет и коробок по затарке (от количества товаров),
    //            есть фиксированное время загрузки/выгрузки паллеты/коробки - из этого и складывается плановое время работ с конкретной товарной позицией.
    //            Если затарки нет, то пробуем определить плановое время по объёму или весу товара в товарной позицией.
    //            Сначала смотрим вес, потом объём.
    //            Смотри какую часть объёма (веса) занимает конкретная товарная позиция от ВСЕГО объёма (веса), 
    //            умножаем плановое максимальное время (при условии загрузки/выгрузки машины ЦЕЛИКОМ) на соответствующий коэффициент.
    //            Если не удалось рассчитать плановое время ни по затарке, ни по весу ни по объёму - считаем время по СОБРАННЫМ ТЕ заказа (ТОЛЬКО для отгрузок).
    //        */
    //        var kind = action.kind == "O" ? ActionKind.Outgo : ActionKind.Income;
    //        var palletTime = action.kind == "O" ? PalletLoadTime : PalletUnloadTime;
    //        var boxTime = action.kind == "O" ? BoxLoadTime : BoxUnloadTime;
    //        var pieceTime = action.kind == "O" ? PieceLoadTime : PieceUnloadTime;
    //        string id = action.id;
    //        var lines = _storage.GetWarehouseActionLines(kind, id);
    //        var actionTime = 0.0;
    //        foreach (var line in lines)
    //        {
    //            // Смотрим по затарке
    //            var lineTime = 0.0;
    //            if (line.PlanPalletsCount > 0.0 || line.PlanBoxesCount > 0.0)
    //                lineTime += line.PlanPalletsCount * palletTime + line.PlanBoxesCount * boxTime;
    //            else
    //            {
    //                // Смотрим по весу
    //                if (line.PlanGrossWeight > 0.0)
    //                    // Килограммы переводим в милиграммы
    //                    lineTime += line.PlanGrossWeight / (vehicleKind.Capacity * 1000 * 1000) * standardTotalDuration;
    //                // Смотрим по объёму
    //                else if (line.PlanVolume > 0.0)
    //                    lineTime += line.PlanVolume / (vehicleKind.CargoVolume * 1000 * 1000 * 1000) * standardTotalDuration;

    //                // Новый блок по задаче #2416 Двор. Новая функция. План факт.
    //                /*
    //                  Идея в том, что мы определяем количество "штук" в товарной позиции (SET = ST, 5 кг = 1 шт., 50 м = 1 шт).
    //                  Считаем "альтернативное" время загрузки/выгрузки товарной позиции как количество "штук", умноженное на время загрузки/выгрузке 1 штуки.
    //                  Если "альтернативное" время загрузки/выгрузки позиции более времени, рассчитанного по алгоритму, то берём "альтернативное" время.
    //                */
    //                var eachesCount = 0.0;
    //                if (line.QuantityMeasureUnit.Code == "ST" || line.QuantityMeasureUnit.Code == "SET")
    //                    eachesCount = line.PlanQuantity;
    //                else if (line.QuantityMeasureUnit.Code == "KG")
    //                    eachesCount = line.PlanQuantity / 5.0;
    //                else if (line.QuantityMeasureUnit.Code == "G")
    //                    eachesCount = line.PlanQuantity / 5.0 / 1000.0;
    //                else if (line.QuantityMeasureUnit.Code == "M")
    //                    eachesCount = line.PlanQuantity / 50.0;
    //                else if (line.QuantityMeasureUnit.Code == "CM")
    //                    eachesCount = line.PlanQuantity / 50.0 / 100.0;

    //                var altLineTime = eachesCount * boxTime;
    //                if (lineTime < altLineTime)
    //                    lineTime = altLineTime;
    //            }
    //            actionTime += lineTime;
    //        }
    //        if (actionTime <= 0.0 && action.kind == "O")
    //        {
    //            // Смотри по собранным заказам (ТОЛЬКО отгрузки)
    //            _storage.GetAssembliedOutgo(id, out double palletsCount, out double eachesCount);
    //            actionTime = palletsCount * palletTime + eachesCount * pieceTime;
    //        }
    //        totalTime += actionTime;
    //    }
    //    totalTime = Math.Round(totalTime);

    //    if (totalTime <= 0.0)
    //        totalTime = standardTotalDuration;
    //    //  2022.09.12 добавил учёт постоянного (не зависящего от состава заказов) времени операции по указанию Алексея Охотникова 
    //    else
    //        totalTime += ConstantOperationDuration;
    //    // 2022.06.30 убрал учёт постоянных микроопераций по указанию Алексея Охотникова
    //    //else
    //    //    totalTime += standardConstantDuration;

    //    // Если продолжительность операции двора больше чем "стандартное" (при загрузке/выгрузке полной машины) то берём "стандартное".
    //    if (totalTime > standardTotalDuration)
    //        totalTime = standardTotalDuration;

    //    return (int)totalTime;
    //}
}
