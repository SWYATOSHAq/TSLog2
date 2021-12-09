using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.Common;
using TSLog.LegalEntities.BO;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly ILinksClientsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;
        private readonly bool DebugMode = true;

        public ClientsController(IConfiguration configuration, ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksClientsStorage)storage;
            _xmlConverter = xmlConverter;
            DebugMode = configuration.GetValue<bool>("DebugMode");
        }

        [HttpPost]
        public async Task<string> PostClients()
        {
            var user = (LegalEntity)HttpContext.Items["User"];


            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var clientsListRequest = _xmlConverter.ClientsFromXml(reader);
            if (clientsListRequest.DataSource == user)
            {
                var clientsConverter = new EINWMSClientsConverter();
                var transactionId = _storage.GenerateTransactionId();
                var fileContent = clientsConverter.Convert(transactionId, clientsListRequest);
                UtilsIO.WriteFileToWMSINFolder(fileContent, transactionId);

                if (!DebugMode)
                    return "<status code=\"0\">Справочник клиентов отправлен на загрузку</status>";
                else
                    return "" + fileContent;
            }
            else
                return "<status code=\"1\">ошибка входных данных: неверный dataSource</status>";
        }

        [HttpGet("{clientId}")]
        public LegalEntity GetClientById(string clientId)
        {
            return _storage.GetLegalEntity(clientId);
        }
    }
}