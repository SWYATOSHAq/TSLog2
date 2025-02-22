using iText.Barcodes;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using TSLog.Incomes.BO;
using TSLog.Outgoes.BO;
using HorizontalAlignment = iText.Layout.Properties.HorizontalAlignment;
using iTextDocument = iText.Layout.Document;

namespace TSLog.Links.Connector;

class WmsStorage(IStorage dbStorage) : IWmsStorage
{
    private readonly IStorage _dbStorage = dbStorage;

    private const string Tsl =
        "ООО \"ТЕХСКЛАДЛОГИСТИК\" " +
        "143006, Московская область, Одинцовский район, город Одинцово, ул. Транспортная, стр. 6А/6, комната 51, этаж 3, " +
        "ИНН 5032315482, КПП 503201001, р/с 40702810001850001897 в АО \"Альфа-Банк\", БИК 044525593";

    private const string FontPath = "C:\\Windows\\Fonts\\Verdana.ttf";
    private const int DefaultFontSize = 8;

    private const string DefaultIncomesPath = "C:\\DiaDoc\\Incomes";
    private const string DefaultOutgoesPath = "C:\\DiaDoc\\Outgoes";
    public string IncomesPath { private get; set; } = DefaultIncomesPath;
    public string OutgoesPath { private get; set; } = DefaultOutgoesPath;

    public string GetFile(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Kind == "O")
        {
            var outgo = _dbStorage.GetOutgo(document.Id);
            return GetOutgoPdf(outgo);
        }
        else if (document.Kind == "I")
        {
            var income = _dbStorage.GetIncome(document.Id);
            return GetIncomePdf(income);
        }
        throw new Exception($"Unknown document kind {document.Kind}.");
    }

    public string GetIncomePdf(Income income)
    {
        ArgumentNullException.ThrowIfNull(income);

        // Must have write permissions to the path folder.
        if (!Directory.Exists(OutgoesPath))
            throw new DirectoryNotFoundException(OutgoesPath);
        var fileName = $"{income.Date:dd.MM.yyyy} {income.PartnerNumber}.pdf";
        fileName = System.IO.Path.Combine(OutgoesPath, fileName);

        var pdfWriter = new PdfWriter(fileName);
        var pdfDocument = new PdfDocument(pdfWriter);
        var document = new iTextDocument(pdfDocument, PageSize.A4, false);

        var defaultFont = PdfFontFactory.CreateFont(FontPath, "Cp1251");

        var pagesCount = pdfDocument.GetNumberOfPages();
        for (int i = 1; i <= pagesCount; i++)
        {
            // Write aligned text to the specified by parameters point.
            document.ShowTextAligned(new Paragraph($"Страница {i} из {pagesCount}").SetFont(defaultFont).SetFontSize(8),
                559, 10, i, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);
        }

        document.Close();
        return fileName;
    }
    public string GetOutgoPdf(Outgo outgo)
    {
        ArgumentNullException.ThrowIfNull(outgo);

        static void AddTableColumnHeaderCell(Table table, string text, int rowspan = 1, int colspan = 1)
        {
            var cell = new Cell(rowspan, colspan)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(text).SetFontSize(8));
            table.AddCell(cell);
        }
        static void AddTableCell(Table table, string text)
        {
            var cell = new Cell(1, 1)
                .SetFontSize(6)
                .Add(new Paragraph(text));
            table.AddCell(cell);
        }

        // Must have write permissions to the path folder.
        if (!Directory.Exists(OutgoesPath))
            throw new DirectoryNotFoundException(OutgoesPath);
        var fileName = $"{outgo.Date:dd.MM.yyyy} {outgo.PartnerNumber}.pdf";
        fileName = System.IO.Path.Combine(OutgoesPath, fileName);

        var pdfWriter = new PdfWriter(fileName);
        var pdfDocument = new PdfDocument(pdfWriter);
        var document = new iTextDocument(pdfDocument, PageSize.A4, false);

        var defaultFont = PdfFontFactory.CreateFont(FontPath, "Cp1251");

        var table = new Table(2).SetFont(defaultFont);
        var cell = new Cell(1, 1).SetWidth(new UnitValue(2, 75)).SetBorder(Border.NO_BORDER);
        

        var paragraph = new Paragraph(Tsl)
            .SetFont(defaultFont)
            .SetFontSize(DefaultFontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetMarginTop(0)
            .SetWidth(new UnitValue(2, 100))
            .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));
        cell.Add(paragraph);
        paragraph = new Paragraph(Tsl)
            .SetFont(defaultFont)
            .SetFontSize(DefaultFontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetMarginTop(10)
            .SetWidth(new UnitValue(2, 100))
            .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));
        cell.Add(paragraph);

        paragraph = new Paragraph("(структурное подразделение)")
            .SetFont(defaultFont)
            .SetFontSize(6)
            .SetWidth(new UnitValue(2, 100))
            .SetTextAlignment(TextAlignment.CENTER);
        cell.Add(paragraph);

        var client = "ООО «Торговый дом «Аскания»," +
            "357210, Минеральные Воды, 3-я Промышленная ул, дом №6,кор.2, Ставропольский край, Минераловодский р-н, " +
            "р/с 40702810561090000746 в банке ФЦБ ВТБ (ПАО), ИНН 2630045364, КПП 263001001, БИК 044525411, к/с 30101810145250000411";
        paragraph = new Paragraph(client)
            .SetFont(defaultFont)
            .SetFontSize(DefaultFontSize)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetMarginTop(10)
            .SetWidth(new UnitValue(2, 100))
            .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));
        cell.Add(paragraph);

        paragraph = new Paragraph("(поклажедатель - наименование, адрес, телефон, ФИО)")
            .SetFont(defaultFont)
            .SetFontSize(6)
            .SetWidth(new UnitValue(2, 100))
            .SetTextAlignment(TextAlignment.CENTER);
        cell.Add(paragraph);

        table.AddCell(cell);
        
        // "Табличка" с информацией о форме документа.
        var formTable = new Table(2).SetFont(defaultFont).SetFontSize(6);
        var formTableCell = new Cell(1, 2).SetHeight(new UnitValue(2, 20))
            .Add(new Paragraph("Унифицированная форма № МХ-3\n" +
                "Утверждена постановленем Госкомстата\n" +
                "России от 09.08.99 №66"))
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        formTable.AddCell(formTableCell);

        formTableCell = new Cell().SetBorder(Border.NO_BORDER).SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("Код")).SetMinHeight(12).SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("Форма по ОКУД")).SetBorder(Border.NO_BORDER).SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("0335003")).SetMinHeight(12).SetTextAlignment(TextAlignment.CENTER); ;
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("по ОКПО")).SetBorder(Border.NO_BORDER).SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("43190726")).SetMinHeight(12).SetTextAlignment(TextAlignment.CENTER); ;
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetBorder(Border.NO_BORDER).SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("Вид деятельности по ОКДП")).SetBorder(Border.NO_BORDER)
            .SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("по ОКПО")).SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetMinHeight(new UnitValue(2, 20)).SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetBorder(Border.NO_BORDER).SetMinHeight(12).SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetMinHeight(12);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("номер")).SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("ХР2_01/04-2016")).SetMinHeight(12).SetTextAlignment(TextAlignment.CENTER);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("дата")).SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("01.04.2016 11:25:00")).SetMinHeight(12).SetTextAlignment(TextAlignment.CENTER); ;
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().Add(new Paragraph("Вид операции")).SetBorder(Border.NO_BORDER).SetMinHeight(12).SetTextAlignment(TextAlignment.RIGHT);
        formTable.AddCell(formTableCell);
        formTableCell = new Cell().SetMinHeight(12);
        formTable.AddCell(formTableCell);


        cell = new Cell(1, 1).SetWidth(new UnitValue(2, 25)).SetBorder(Border.NO_BORDER).Add(formTable);
        table.AddCell(cell);

        document.Add(table);

        table = new Table(2).SetFont(defaultFont).SetFontSize(8)
            .SetWidth(new UnitValue(2, 50))
            .SetHorizontalAlignment(HorizontalAlignment.CENTER)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(10)
            .AddCell("Номер документа")
            .AddCell("Дата составления")
            .AddCell(outgo.Number)
            .AddCell(outgo.Date.ToString("dd.MM.yyyy"));
        document.Add(table);

        var title = new Paragraph("АКТ О ВОЗВРАТЕ ТОВАРНО-МАТЕРИАЛЬНЫХ ЦЕННОСТЕЙ, СДАННЫХ НА ХРАНЕНИЕ")
            .SetFont(defaultFont)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(10);
        document.Add(title);

        var tempString = "Акт составлен в том, что поклажедатель принял от хранителя следующие товарно-материальные ценности:";
        paragraph = new Paragraph(tempString)
            .SetFont(defaultFont)
            .SetFontSize(8)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetMarginTop(10);
        document.Add(paragraph);

        table = new Table(7).SetFont(defaultFont);
        AddTableColumnHeaderCell(table, "№ по порядку", 2, 1);
        AddTableColumnHeaderCell(table, "Товарно-материальные ценности", 1, 2);
        AddTableColumnHeaderCell(table, "Единица измерения", 2, 1);
        AddTableColumnHeaderCell(table, "Количество (масса)", 2, 1);
        AddTableColumnHeaderCell(table, "цена, руб. коп.", 2, 1);
        AddTableColumnHeaderCell(table, "стоимость, руб. коп.", 2, 1);
        AddTableColumnHeaderCell(table, "наименование товара");
        AddTableColumnHeaderCell(table, "код");
        
        int index = 0;
        var totalFactQuantity = 0.0;
        var totalSum = 0.0;
        foreach (var line in outgo.Lines)
        {
            index++;
            AddTableCell(table, index.ToString());
            AddTableCell(table, line.Product.Name);
            AddTableCell(table, line.Product.Code);

            // TODO: Прописать short_name.
            // AddTableCell(table, line.QuantityMeasureUnit.Code);
            AddTableCell(table, line.QuantityMeasureUnit.Code);

            AddTableCell(table, line.FactQuantity.ToString());
            totalFactQuantity += line.FactQuantity;

            AddTableCell(table, line.Price.ToString());

            AddTableCell(table, line.Sum.ToString());
            totalSum += line.Sum;
        }

        cell = new Cell(1, 4)
            .SetFontSize(6)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph("Итого"));
        table.AddCell(cell);
        AddTableCell(table, totalFactQuantity.ToString());
        AddTableCell(table, "X");
        AddTableCell(table, totalSum.ToString());
        cell = new Cell(1, 4)
            .SetFontSize(6)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph("Всего по акту"));
        table.AddCell(cell);
        AddTableCell(table, totalFactQuantity.ToString());
        AddTableCell(table, "X");
        AddTableCell(table, totalSum.ToString());


        document.Add(table);

        paragraph = new Paragraph().SetWidth(new UnitValue(2, 80));
        var text = new Text("В соответствии с договором о хранении за время хранения")
            .SetFont(defaultFont)
            .SetFontSize(8);
        paragraph.Add(text);

        text = new Text("    ")
            .SetFont(defaultFont)
            .SetFontSize(8);
        paragraph.Add(text);
        text = new Text("                    ")
            .SetFont(defaultFont)
            .SetFontSize(8)
            .SetUnderline();
        paragraph.Add(text);
        text = new Text("    ")
            .SetFont(defaultFont)
            .SetFontSize(8);
        paragraph.Add(text);

        text = new Text("дней")
            .SetFont(defaultFont)
            .SetFontSize(8);
        paragraph.Add(text);
        
        paragraph.SetMarginBottom(0);
        document.Add(paragraph);

        paragraph = new Paragraph("(срок хранения)")
            .SetWidth(new UnitValue(2, 60))
            .SetFont(defaultFont)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetVerticalAlignment(VerticalAlignment.TOP)
            .SetMargin(0)
            .SetFontSize(6);
        document.Add(paragraph);

        paragraph = new Paragraph("Вышеперечисленных товарно-материальных ценностей " +
            "организацией-хранителем выполнены, а поклажедателем" +
            "приняты следующие работы и услуги:")
            .SetFont(defaultFont)
            .SetFontSize(8);
        document.Add(paragraph);

        table = new Table(6)
            .SetFont(defaultFont)
            .SetWidth(new UnitValue(2, 100));
        AddTableColumnHeaderCell(table, "№ по порядку");
        AddTableColumnHeaderCell(table, "Наименование услуги");
        AddTableColumnHeaderCell(table, "Единица измерения");
        AddTableColumnHeaderCell(table, "Количество (масса)");
        AddTableColumnHeaderCell(table, "цена, руб. коп. (без НДС)");
        AddTableColumnHeaderCell(table, "стоимость, руб. коп. (без НДС)");
        document.Add(table);


        var barcode = new BarcodeEAN(pdfDocument);
        barcode.SetCodeType(BarcodeEAN.EAN8);
        barcode.SetCode("12345678");

        // Create barcode object to put it to the cell as image.
        PdfFormXObject barcodeObject = barcode.CreateFormXObject(null, null, pdfDocument);
        paragraph = new Paragraph()
            .Add(new iText.Layout.Element.Image(barcodeObject))
            .SetPaddingTop(10)
            .SetPaddingRight(10)
            .SetPaddingBottom(10)
            .SetPaddingLeft(10);
        document.Add(paragraph);

        paragraph = new Paragraph("Товарно-материальные ценности на хранение")
            .SetFont(defaultFont)
            .SetFontSize(8);
        document.Add(paragraph);

        void CreateSign(iTextDocument document, string str)
        {
            var paragraph = new Paragraph().SetWidth(new UnitValue(2, 100));
            var text = new Text(str).SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);

            var space = new string(' ', 54);
            var space2 = "    ";
            text = new Text(space2).SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);
            text = new Text(space).SetUnderline().SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);

            text = new Text(space2).SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);
            text = new Text(space).SetUnderline().SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);

            text = new Text(space2).SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);
            text = new Text(space).SetUnderline().SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);

            text = new Text(".").SetFont(defaultFont).SetFontSize(8);
            paragraph.Add(text);
            document.Add(paragraph);

            paragraph = new Paragraph().SetWidth(new UnitValue(2, 80));
            text = new Text("(должность)").SetRelativePosition(100, 0, 0, 0).SetFont(defaultFont).SetFontSize(6);
            paragraph.Add(text);
            text = new Text("(подпись)").SetRelativePosition(220, 0, 0, 0).SetFont(defaultFont).SetFontSize(6);
            paragraph.Add(text);
            text = new Text("(расшифровка)").SetRelativePosition(340, 0, 0, 0).SetFont(defaultFont).SetFontSize(6);
            paragraph.Add(text);
            document.Add(paragraph);
        }

        CreateSign(document, "Сдал");
        CreateSign(document, "Принял");

        var pagesCount = pdfDocument.GetNumberOfPages();
        for (int i = 1; i <= pagesCount; i++)
        {
            // Write aligned text to the specified by parameters point.
            document.ShowTextAligned(new Paragraph($"Страница {i} из {pagesCount}").SetFont(defaultFont).SetFontSize(8),
                559, 10, i, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);
        }

        document.Close();
        return fileName;
    }
}
