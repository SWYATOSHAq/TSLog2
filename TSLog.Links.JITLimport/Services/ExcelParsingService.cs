using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.JITLimport.Services;

internal class ExcelParsingService
{
    public void ParseExcelFiles(string directoryPath)
    {
        var excelFiles = Directory.GetFiles(directoryPath, "*.xls");
        string date = String.Empty;
        string startTime = String.Empty;
        string vehicle = String.Empty;
        string driverName = String.Empty;
        string partnerManagerName = String.Empty;

        foreach (var filePath in excelFiles)
        {
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
                                    date = parsedRow.GetCell(5).ToString();
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
        }
    }
}
