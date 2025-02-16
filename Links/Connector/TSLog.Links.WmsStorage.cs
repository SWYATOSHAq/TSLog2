using iText.Barcodes;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Layout.Element;
using iText.Layout.Properties;
using TSLog.Incomes.BO;
using TSLog.Outgoes.BO;

namespace TSLog.Links.Connector;

class WmsStorage(IStorage dbStorage) : IWmsStorage
{
    private readonly IStorage _dbStorage = dbStorage;


    private static void AddTableCell(Table table, string text)
    {
        var cell = new Cell(1, 1);
        cell.SetFontSize(6);
        var p = new Paragraph(text);
        cell.Add(p);
        table.AddCell(cell);
    }
    public string GetFile(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        /*
        if (document.Kind == "O")
        {
            //var header = _dbStorage.GetOutgoHeader(document.Id);
            //var lines = _dbStorage.GetOutgoLines(document.Id);
        }
        else if (document.Kind == "I")
        { }
        */

        var outgo = _dbStorage.GetOutgo(document.Id);

        // Must have write permissions to the path folder
        var fileName = "C:\\m.burov\\demo.pdf";
        PdfWriter writer = new PdfWriter(fileName);
        PdfDocument pdf = new PdfDocument(writer);
        iText.Layout.Document doc = new iText.Layout.Document(pdf);


        var tsl = 
            "ООО \"ТЕХСКЛАДЛОГИСТИК\" 143006, Московская область, Одинцовский район, город Одинцово, ул. " +
            "Транспортная, стр. 6А/6, комната 51, этаж 3, ИНН 5032315482, КПП 503201001, р/с 40702810001850001897 в " +
            "АО \"Альфа-Банк\", БИК 044525593";

        string FONT = "C:\\Windows\\Fonts\\Verdana.ttf";
        PdfFont f1 = PdfFontFactory.CreateFont(FONT, "Cp1251");


        iText.Layout.Element.Paragraph text = new Paragraph(tsl)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(8)
            .SetFont(f1);
        iText.Layout.Element.Div div = new Div();
        UnitValue width = new UnitValue(2, 75);
        div.SetWidth(width);
        div.SetFont(f1);
        div.Add(text);
        div.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
        div.SetBorderBottom(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 0.5f));
        doc.Add(div);
        div.SetMarginTop(10);
        doc.Add(div);

        var client = "ООО «Торговый дом «Аскания», 357210, Минеральные Воды, 3-я Промышленная ул, дом №6,кор.2, " +
            "Ставропольский край, Минераловодский р-н, " +
            "р/с 40702810561090000746 в банке ФЦБ ВТБ (ПАО), ИНН 2630045364, КПП 263001001, " +
            "БИК 044525411, к/с 30101810145250000411";
        text = new Paragraph(client)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(8)
            .SetFont(f1);
        div = new Div();
        width = new UnitValue(2, 75);
        div.SetWidth(width);
        div.SetFont(f1);
        div.Add(text);
        div.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
        div.SetBorderBottom(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 0.5f));
        div.SetMarginTop(10);
        doc.Add(div);

        iText.Layout.Element.Table table = new Table(2).SetFont(f1).SetFontSize(8);
        table.SetWidth(new UnitValue(2, 70));
        table.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
        table.SetTextAlignment(TextAlignment.CENTER);
        table.AddCell("Номер документа");
        table.AddCell("Дата составления");
        table.AddCell(outgo.Number);
        table.AddCell(outgo.Date.ToString());
        table.SetMarginTop(10);
        doc.Add(table);

        Paragraph title = new Paragraph("АКТ О ВОЗВРАТЕ ТОВАРНО-МАТЕРИАЛЬНЫХ ЦЕННОСТЕЙ, СДАННЫХ НА ХРАНЕНИЕ")
            .SetFont(f1)
            .SetTextAlignment(TextAlignment.CENTER);
        title.SetMarginTop(10);
        doc.Add(title);


        var str = "Акт составлен в том, что поклажедатель принял от хранителя следующие товарно-материальные ценности:";
        Paragraph textBlock = new Paragraph(str)
            .SetFont(f1)
            .SetTextAlignment(TextAlignment.CENTER);
        textBlock.SetMarginTop(10);
        doc.Add(textBlock);

        table = new Table(7).SetFont(f1);

        Cell cell = new Cell(2, 1).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        cell.SetFont(f1);
        cell.Add(new Paragraph("№ по порядку").SetFontSize(8));
        table.AddCell(cell);

        cell = new Cell(1, 2).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        cell.Add(new Paragraph("Товарно-материальные ценности").SetFontSize(8));
        table.AddCell(cell);

        cell = new Cell(2, 1).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        cell.Add(new Paragraph("Единица измерения").SetFontSize(8));
        table.AddCell(cell);

        cell = new Cell(2, 1).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        cell.Add(new Paragraph("Количество (масса)").SetFontSize(8));
        table.AddCell(cell);

        cell = new Cell(2, 1).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        cell.Add(new Paragraph("цена, руб. коп.").SetFontSize(8));
        table.AddCell(cell);

        cell = new Cell(2, 1).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        cell.Add(new Paragraph("стоимость, руб. коп.").SetFontSize(8));
        table.AddCell(cell);

        table.AddCell("наименование товара");
        table.AddCell("код");

        int index = 0;
        foreach (var line in outgo.Lines)
        {
            index++;
            AddTableCell(table, index.ToString());
            AddTableCell(table, line.Product.Name);
            AddTableCell(table, line.Product.Code);
            AddTableCell(table, line.QuantityMeasureUnit.ShortName);
            AddTableCell(table, line.FactQuantity.ToString());
            AddTableCell(table, line.Price.ToString());
            AddTableCell(table, line.Sum.ToString());
        }

        doc.Add(table);

        iText.Layout.Element.GridContainer gc = new GridContainer();
        iText.Layout.Element.List l = new List();

        Paragraph par = new Paragraph();
        div.SetBaseDirection(BaseDirection.LEFT_TO_RIGHT);

        iText.Layout.Element.Text t = new Text("В соответствии с договором о хранении за время хранения")
            .SetFont(f1)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(10);
        par.Add(t);

        t = new Text("(срок хранения)")
            .SetFont(f1)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(6);
        par.Add(t);

        t = new Text("дней")
            .SetFont(f1)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(10);
        par.Add(t);

        doc.Add(par);


        par = new Paragraph("Вышеперечисленных товарно-материальных ценностей организацией-хранителем выполнены, а поклажедателем" +
            "приняты следующие работы и услуги:").SetFont(f1);
        doc.Add(par);

        table = new Table(6).SetFont(f1).SetWidth(new UnitValue(2, 100));
        AddTableCell(table, "№ по порядку");
        AddTableCell(table, "Наименование услуги");
        AddTableCell(table, "Единица измерения");
        AddTableCell(table, "Количество (масса)");
        AddTableCell(table, "цена, руб. коп. (без НДС)");
        AddTableCell(table, "стоимость, руб. коп. (без НДС)");
        doc.Add(table);


        BarcodeEAN barcode = new BarcodeEAN(pdf);
        barcode.SetCodeType(BarcodeEAN.EAN8);
        barcode.SetCode("12345678");

        // Create barcode object to put it to the cell as image
        PdfFormXObject barcodeObject = barcode.CreateFormXObject(null, null, pdf);
        Paragraph bc = new Paragraph().Add(new iText.Layout.Element.Image(barcodeObject));
        bc.SetPaddingTop(10);
        bc.SetPaddingRight(10);
        bc.SetPaddingBottom(10);
        bc.SetPaddingLeft(10);
        doc.Add(bc);

        par = new Paragraph("Товарно - материальные ценности на хранение").SetFont(f1);
        doc.Add(par);

        par = new Paragraph("Сдал").SetFont(f1);
        doc.Add(par);

        par = new Paragraph("Принял").SetFont(f1);
        doc.Add(par);

        doc.Close();
        return fileName;
    }

    public string GetIncomePdf(Income income)
    {
        return string.Empty;
    }
    public string GetOutgoPdf(Outgo outgo)
    {
        return string.Empty;
    }
}
