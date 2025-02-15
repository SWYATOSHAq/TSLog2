using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using TSLog.Incomes.BO;
using TSLog.Outgoes.BO;

namespace TSLog.Links.Connector;

class WmsStorage(IStorage dbStorage) : IWmsStorage
{
    private readonly IStorage _dbStorage = dbStorage;

    public string GetFile(Document document)
    {
        if (document.Kind == "O")
        {
            //var header = _dbStorage.GetOutgoHeader(document.Id);
            //var lines = _dbStorage.GetOutgoLines(document.Id);
        }
        else if (document.Kind == "I")
        { }

        // Must have write permissions to the path folder
        PdfWriter writer = new PdfWriter("C:\\m.burov\\demo.pdf");
        PdfDocument pdf = new PdfDocument(writer);
        iText.Layout.Document doc = new iText.Layout.Document(pdf);
        Paragraph header = new Paragraph("HEADER")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(20);


        doc.Add(header);
        doc.Close();

        return string.Empty;
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
