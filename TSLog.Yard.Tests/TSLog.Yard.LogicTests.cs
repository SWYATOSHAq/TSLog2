using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Security.Claims;
using TSLog.Authorization;
using TSLog.Yard.BO;
using TSLog.Yard.Logic;

namespace TSLog.Yard.Tests
{
    [TestClass]
    public class LogicTests
    {
        private Processor _processor;
        private IConfiguration _configuration;
        private IYardStorage _storage;

        private static string GetStringValue(IConfigurationSection section, string name)
        {
            return section.GetValue<string>(name) ?? string.Empty;
        }
        private void Connect(bool isAdmin = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            _configuration = builder.Build();

            var oracleSection = _configuration.GetSection("OracleConnectionDebugSettings");
            var oracleTns = GetStringValue(oracleSection, "DataSource");
            var oracleUserName = GetStringValue(oracleSection, "UserName");
            var oraclePassword = GetStringValue(oracleSection, "Password");
            var oracleCommonSchema = GetStringValue(oracleSection, "CommonSchema");
            var oracleStaffSchema = GetStringValue(oracleSection, "StaffSchema");
            var oracleLegalEntitiesSchema = GetStringValue(oracleSection, "LegalEntitiesSchema");
            var oraclePartnersSchema = GetStringValue(oracleSection, "PartnersSchema");

            var pgsqlSection = _configuration.GetSection("PostgreSqlConnectionDebugSettings");
            var pgsqlHost = GetStringValue(pgsqlSection, "Host");
            var pgsqlAdminUserName = GetStringValue(pgsqlSection, "AdminUserName");
            var pgsqlUserName = GetStringValue(pgsqlSection, "UserName");
            var pgsqlPassword = GetStringValue(pgsqlSection, "Password");
            var pgsqlDatabase = GetStringValue(pgsqlSection, "Database");
            var pgsqlYardSchema = GetStringValue(pgsqlSection, "YardSchema");

            _storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase);
            _storage.ConnectOracle(oracleUserName, oraclePassword);
            _storage.SetOracleCommonSchema(oracleCommonSchema);
            _storage.SetOracleStaffSchema(oracleStaffSchema);
            _storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
            _storage.SetOraclePartnersSchema(oraclePartnersSchema);

            _storage.ConnectPgSql(isAdmin ? pgsqlAdminUserName : pgsqlUserName, pgsqlPassword);
            _storage.SetPgsqlYardSchema(pgsqlYardSchema);
        }

        private void Disconnect()
        {
            _storage.DisconnectOracle();
            _storage.DisconnectPgsql();
            _storage.Dispose();
        }

        [TestInitialize]
        public void Setup()
        {
            Connect(true);
            _storage.TruncateDataTables();
            Disconnect();
            Connect();
            var rightsChecker = new RightsChecker();
            var programFileName = Assembly.GetExecutingAssembly().Location;
            var programDirectory = Path.GetDirectoryName(programFileName) ?? throw new Exception("Rights file not found");
            var rightsFileName = Path.Combine(programDirectory, "rights.xml");
            var userPartnersFileName = Path.Combine(programDirectory, "userPartners.xml");
            rightsChecker.LoadRights(rightsFileName);
            rightsChecker.LoadUserPartners(userPartnersFileName);
            _processor = new Processor(_configuration, _storage, rightsChecker);

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
            Assert.IsTrue(statusCode == 200);
            Assert.IsTrue(statusMessage == string.Empty);
        }
    }
}
