using AngleSharp.Css;
using Microsoft.Extensions.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Links.JITLimport.Helpers;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.Partners.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport.Services;

internal class ExcelParsingService : IExcelParsingService
{
    private readonly IYardDbConnectService _yardDbConnectService;
    public ExcelParsingService(IYardDbConnectService yardDbConnectService)
    {
        _yardDbConnectService = yardDbConnectService;
    }
    public void ParseExcelFiles(string directoryPath)
    {
        var excelFiles = Directory.GetFiles(directoryPath, "*.xls");

        foreach (var filePath in excelFiles)
        {
            string startDate = String.Empty;
            string startTime = String.Empty;
            string vehicle = String.Empty;
            string driverName = String.Empty;
            string partnerManagerName = String.Empty;
            string user = "auto_jitloper";

            string fileName = Path.GetFileName(filePath);
            string[] parts = fileName.Split('_');
            string operationTypePart = parts[0];
            string operationTypeCode = String.Empty;
            string vehicleType = String.Empty;
            dynamic[] actions;
            //= _yardDbConnectService.GetYardWarhouseAction();
            if (operationTypePart == "ЗнО")
            {
                operationTypeCode = "O_BOX_MIX";
                vehicleType = "1.5_TONS";
                actions = new dynamic[]
                {
                    new {
                        operationId = -1,
                        kind = "O",
                        id = "00420000MASTER NORMAL"
                    }
                };
            }
            else
            {
                operationTypeCode = "I_BOX_MIX";
                vehicleType = "20_TONS";
                actions = new dynamic[]
                {
                    new {
                        operationId = -1,
                        kind = "I",
                        id = "00420000MASTER NORMAL"
                    }
                };
            }


            string stringActionKind = actions[0].kind;
            string stringActionId = actions[0].id;
            WarehouseAction warehouseAction = _yardDbConnectService.GetYardWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);


            var operationType = _yardDbConnectService.GetYardOperationType(operationTypeCode);

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var hssfWorkbook = new HSSFWorkbook(fileStream);
            var sheet = hssfWorkbook.GetSheetAt(0); // Assuming the data is in the first sheet.
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;

            for (int row = 0; row <= sheet.LastRowNum; row++) // Skip header row by starting at 1
            {
                var parsedRow = sheet.GetRow(row);
                if (parsedRow == null) continue; // skip empty rows
                if (parsedRow.Cells.All(d => d.CellType == CellType.Blank)) continue;

                for (int j = parsedRow.FirstCellNum; j < cellCount; j++)
                {
                    if (parsedRow.GetCell(j) != null)
                    {
                        if (!string.IsNullOrEmpty(parsedRow.GetCell(j).ToString()) && !string.IsNullOrWhiteSpace(parsedRow.GetCell(j).ToString()))
                        {
                            var temp = parsedRow.GetCell(j).ToString().Trim();
                            switch (temp)
                            {
                                case "Дата отгрузки:":
                                    startDate = parsedRow.GetCell(5).ToString();
                                    break;
                                case "Время отгрузки:":
                                    startTime = parsedRow.GetCell(12).ToString();
                                    break;
                                case "Авто:":
                                    vehicle = parsedRow.GetCell(1).ToString();
                                    break;
                                case "Водитель:":
                                    driverName = parsedRow.GetCell(3).ToString();
                                    break;
                                case "Заявку создал:":
                                    partnerManagerName = parsedRow.GetCell(4).ToString();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            DateTime date = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
            DateTime time = DateTime.ParseExact(startTime, "HH:mm", null);
            DateTime planWorkFinishDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
            var vehicleKind = _yardDbConnectService.GetYardVehicleKind(vehicleType);
            var planWorkStartDate = planWorkFinishDate.Subtract(new TimeSpan(0, 0, YardUtils._GetFillingPlanOperationDuration(operationType, vehicleKind, actions)));
            var vehicleNumber = vehicle;
            var vehicleBrand = "Смотри поле 'Номер авто'";
            var vehicleModel = "Смотри поле 'Номер авто'";
            var trailerNumber = "Смотри поле 'Номер авто'";
            var driverPhoneNumber = String.Empty;
            var state = _yardDbConnectService.GetYardOperationState();
            var priority = Operation.DefaultPriority;
            var partner = _yardDbConnectService.GetYardPartner();
            var manager = _yardDbConnectService.GetYardPartnerManagers(partner);
            var partnerManagerPhoneNumber = manager.PhoneNumber;
            var partnerManagerEmail = manager.Email;

            var operation = new Operation(-1, null, operationType, planWorkStartDate, planWorkFinishDate,
                vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
                driverName, driverPhoneNumber,
                state,
                priority,
                partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                manager);
            operation.Note = String.Empty;
            operation.Parking = false;

            _yardDbConnectService.CreateYardOperation(operation, user, warehouseAction);
        }
    }
}
