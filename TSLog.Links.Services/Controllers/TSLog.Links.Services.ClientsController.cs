using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.BO;
using TSLog.Common;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ILinksClientsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

        public ClientsController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksClientsStorage)storage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public async Task<string> PostClients()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var clientsList = _xmlConverter.ClientsFromXml(reader);
            var converterClients = new EINWMSClientsConverter();

            var transactionNumber = _storage.GenerateTransactionId();
            var fileContent = converterClients.Convert(transactionNumber, clientsList);

            UtilsIO.WriteFileToWMSINFolder(fileContent, transactionNumber);

            return "" + fileContent; // todo retrun smth userfull
        }

        [HttpGet("{clientId}")]
        public LegalEntity GetClientById(string clientId)
        {
            return _storage.GetLegalEntity(clientId);
        }
    }
}