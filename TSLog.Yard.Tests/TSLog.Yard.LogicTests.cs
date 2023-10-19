using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using TSLog.Authorization;
using TSLog.Yard.BO;
using TSLog.Yard.Logic;
using Microsoft.Extensions.Logging.Abstractions;

namespace TSLog.Yard.Tests
{
    [TestClass]
    public class LogicTests : IDisposable
    {
        private readonly string _oracleUserName;
        private readonly string _oraclePassword;
        private readonly string _pgsqlAdminUserName;
        private readonly string _pgsqlUserName;
        private readonly string _pgsqlPassword;

        private readonly Processor _processor;
        private readonly IConfiguration _configuration;
        private readonly IYardStorage _storage;

        public LogicTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            _configuration = builder.Build();

            var loggerFactory = new LoggerFactory();
            var oracleSection = _configuration.GetSection("OracleConnectionDebugSettings");
            var oracleTns = GetStringValue(oracleSection, "DataSource");
            _oracleUserName = GetStringValue(oracleSection, "UserName");
            _oraclePassword = GetStringValue(oracleSection, "Password");
            var oracleCommonSchema = GetStringValue(oracleSection, "CommonSchema");
            var oracleStaffSchema = GetStringValue(oracleSection, "StaffSchema");
            var oracleLegalEntitiesSchema = GetStringValue(oracleSection, "LegalEntitiesSchema");
            var oraclePartnersSchema = GetStringValue(oracleSection, "PartnersSchema");

            var pgsqlSection = _configuration.GetSection("PostgreSqlConnectionDebugSettings");
            var pgsqlHost = GetStringValue(pgsqlSection, "Host");
            _pgsqlAdminUserName = GetStringValue(pgsqlSection, "AdminUserName");
            _pgsqlUserName = GetStringValue(pgsqlSection, "UserName");
            _pgsqlPassword = GetStringValue(pgsqlSection, "Password");
            var pgsqlDatabase = GetStringValue(pgsqlSection, "Database");
            var pgsqlYardSchema = GetStringValue(pgsqlSection, "YardSchema");

            _storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase, loggerFactory);            
            _storage.SetOracleCommonSchema(oracleCommonSchema);
            _storage.SetOracleStaffSchema(oracleStaffSchema);
            _storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
            _storage.SetOraclePartnersSchema(oraclePartnersSchema);
            _storage.SetPgsqlYardSchema(pgsqlYardSchema);

            var rightsChecker = new RightsChecker();
            var programFileName = Assembly.GetExecutingAssembly().Location;
            var programDirectory = Path.GetDirectoryName(programFileName) ?? throw new Exception("Rights file not found!");
            var rightsFileName = Path.Combine(programDirectory, "rights.xml");
            var userPartnersFileName = Path.Combine(programDirectory, "userPartners.xml");
            rightsChecker.LoadRights(rightsFileName);
            rightsChecker.LoadUserPartners(userPartnersFileName);
            _processor = new Processor(_configuration, _storage, rightsChecker, NullLogger<Processor>.Instance);
        }

        private static string GetStringValue(IConfigurationSection section, string name)
        {
            return section.GetValue<string>(name) ?? string.Empty;
        }
        private void Connect(bool isAdmin = false)
        {
            _storage.ConnectOracle(_oracleUserName, _oraclePassword);
            _storage.ConnectPgSql(isAdmin ? _pgsqlAdminUserName : _pgsqlUserName, _pgsqlPassword);
        }

        private void Disconnect()
        {
            _storage.DisconnectOracle();
            _storage.DisconnectPgsql();
        }

        [TestInitialize]
        public void Setup()
        {
            Connect(true);
            _storage.TruncateDataTables();
            Disconnect();
            Connect();
        }
        [TestCleanup]
        public void TearDown()
        {
            Disconnect();
        }

        [TestMethod]
        public void GetPositionWorkers()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "testyarddisp"),
                new Claim(ClaimTypes.NameIdentifier, "testyarddisp"),
                new Claim(ClaimTypes.Role, "YardDispatchers"),
                new Claim("name", "testyarddisp"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            var objects = _processor.GetPositionWorkers(string.Empty, "EXT_LOADER", user,
                out int statusCode, out string statusMessage);
            Assert.IsTrue(objects.Length > 0);
            Assert.IsTrue(statusCode == (int)HttpStatusCode.OK);
            Assert.IsTrue(statusMessage == string.Empty);
        }

        public void Dispose() => _storage.Dispose();
    }
}
