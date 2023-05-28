using Microsoft.Extensions.Configuration;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Tests
{
    public class StorageTests
    {
        private IYardStorage _storage;

        [SetUp]
        public void Setup()
        {
            Connect(true);
            _storage.TruncateDataTables();
            Disconnect();
            Connect();
        }
        [TearDown]
        public void TearDown()
        {
            Disconnect();
        }

        private static string GetStringValue(IConfigurationSection section, string name)
        {
            return section.GetValue<string>(name) ?? string.Empty;
        }

        private void Connect(bool isAdmin = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            var configuration = builder.Build();

            var oracleSection = configuration.GetSection("OracleConnectionDebugSettings");
            var oracleTns = GetStringValue(oracleSection, "DataSource");
            var oracleUserName = GetStringValue(oracleSection, "UserName");
            var oraclePassword = GetStringValue(oracleSection, "Password");
            var oracleCommonSchema = GetStringValue(oracleSection, "CommonSchema");
            var oracleStaffSchema = GetStringValue(oracleSection, "StaffSchema");
            var oracleLegalEntitiesSchema = GetStringValue(oracleSection, "LegalEntitiesSchema");
            var oraclePartnersSchema = GetStringValue(oracleSection, "PartnersSchema");

            var pgsqlSection = configuration.GetSection("PostgreSqlConnectionDebugSettings");
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

        [Test]
        public void OperationFiltering()
        {
            // Фильтрация операций двора (ПРР) в БД по различным параметрам.
            CreateSomeOperation();

            var filterFromDate = DateTime.Now.AddDays(+1);
            var filterToDate = DateTime.Now.AddDays(+8);
            var filteredOperations = _storage.GetOperations(string.Empty, filterFromDate, filterToDate, null, null, null, null, null,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, Array.Empty<Employee>());
            Assert.That(filteredOperations, Is.Not.Empty);
        }
        private Entity GetOperationStateById(short id)
        {
            var operationState = _storage.GetOperationState(id);
            Assert.That(operationState.Id, Is.EqualTo(id));
            return operationState;
        }
        private Entity GetOperationStateByCode(string code)
        {
            Assert.That(!string.IsNullOrWhiteSpace(code), Is.True);
            var operationState = _storage.GetOperationState(code);
            Assert.That(operationState.Code, Is.EqualTo(code));
            return operationState;
        }
        private BO.OperationType GetOperationTypeById(short id)
        {
            var operationType = _storage.GetOperationType(id);
            Assert.That(operationType.Id, Is.EqualTo(id));
            return operationType;
        }
        private BO.OperationType GetOperationTypeByCode(string code)
        {
            Assert.That(!string.IsNullOrWhiteSpace(code), Is.True);
            var operationType = _storage.GetOperationType(code);
            Assert.That(operationType.Code, Is.EqualTo(code));
            return operationType;
        }
        private VehicleKind GetVehicleKindById(short id)
        {
            var vehicleKind = _storage.GetVehicleKind(id);
            Assert.That(vehicleKind.Id, Is.EqualTo(id));
            return vehicleKind;
        }
        private VehicleKind GetVehicleKindByCode(string code)
        {
            Assert.That(!string.IsNullOrWhiteSpace(code), Is.True);
            var vehicleKind = _storage.GetVehicleKind(code);
            Assert.That(vehicleKind.Code, Is.EqualTo(code));
            return vehicleKind;
        }
        private Gate GetGateById(short id)
        {
            var gate = _storage.GetGate(id);
            Assert.That(gate.Id, Is.EqualTo(id));
            return gate;
        }                
        private Gate GetGateByCode(string code)
        {
            var gate = _storage.GetGate(code);
            Assert.That(gate.Code, Is.EqualTo(code) );
            return gate;
        }
        private Employee GetEmployee(int id)
        {
            var employee = _storage.GetEmployee(id);
            Assert.That(employee, Is.Not.EqualTo(null));
            return employee;
        }
        private short GetDuration(BO.OperationType operationType, VehicleKind vehicleKind)
        {
            var duration = _storage.GetDuration(operationType, vehicleKind, out _, out _);
            Assert.That(duration, Is.GreaterThan(0));
            return duration;
        }
        private Partner GetPartnerById(string id)
        {
            Assert.That(!string.IsNullOrWhiteSpace(id), Is.True);
            var partner = _storage.GetPartner(id);
            Assert.That(partner.Code, Is.EqualTo(id));
            return partner;
        }
        private WarehouseAction GetWarehouseAction(ActionKind kind, Partner partner, string number)
        {
            var action = _storage.GetWarehouseAction(kind, partner, number);
            Assert.That(action, Is.Not.EqualTo(null));
            return action;
        }
        [Test]
        public void GetOperationState()
        {
            // Загрузка состояния операций по id и по коду.
            var operationState1 = GetOperationStateById(1001);
            var operationState2 = GetOperationStateByCode(operationState1.Code);
            Assert.That(operationState1, Is.EqualTo(operationState2));
        }
        [Test]
        public void GetOperationType()
        {
            // Загрузка типа операций по id и по коду.
            var operationType1 = GetOperationTypeById(1001);
            var operationType2 = GetOperationTypeByCode(operationType1.Code);
            Assert.That(operationType1, Is.EqualTo(operationType2));
        }
        [Test]
        public void GetVehicleKind()
        {
            // Загрузка типа тип машины по id и по коду.
            var vehicleKind1 = GetVehicleKindById(1001);
            var vehicleKind2 = GetVehicleKindByCode(vehicleKind1.Code);
            Assert.That(vehicleKind1, Is.EqualTo(vehicleKind2));
        }
        [Test]
        public void GetGate()
        {
            // Загрузка ворота по id и по коду.
            var gate1 = GetGateById(1001);
            var gate2 = GetGateByCode(gate1.Code);
            Assert.That(gate1, Is.EqualTo(gate2));
        }
        [Test]
        public void GetEmployee()
        {
            // Загрузка сотрудника по id.
            GetEmployee(100004);
        }
        [Test]
        public void GetDuration()
        {
            var operationType = GetOperationTypeByCode("O_BOX_MIX");
            var vehicleKind = GetVehicleKindByCode("20_TONS");
            GetDuration(operationType, vehicleKind);
        }
        [Test]
        public void GetIntervals()
        {
            // Проверка выгрузки свободных интервалов.
            var operationType = GetOperationTypeById(1001);
            var vehicleKind = GetVehicleKindById(1001);
            var gate = GetGateById(1001);

            var intervals = _storage.GetAvalibleIntervals(DateTime.Now.AddDays(-20), DateTime.Now.AddDays(+20), 
                operationType, vehicleKind);
            Assert.That(intervals, Is.Not.Empty);

            intervals = _storage.GetGateIntervals(gate, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
            Assert.That(intervals, Is.Not.Empty);

            intervals = _storage.GetAllIntervals(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
            Assert.That(intervals, Is.Not.Empty);

            var union = Interval.Union(intervals);
            Assert.That(union, Has.Length.EqualTo(1));
        }
        [Test]
        public void GetPartner()
        {
            // Загрузка клиента по id.
            GetPartnerById("0003");
        }
        [Test]
        public void GetPartnerWithManagers()
        {
            // Загрузка партнера по mndnr и информации по партнеру.
            var partner = GetPartnerById("0003");
            var managers = _storage.GetPartnerManagers(partner);
            Assert.That(managers, Is.Not.Empty);
        }
        [Test]        
        public void GetOperations()
        {
            // Проверка выгрузки операций.
            CreateSomeOperation();

            var operations = _storage.GetOperations(DateTime.Now.AddDays(+1), DateTime.Now.AddDays(+8), null);
            Assert.That(operations, Is.Not.Empty);
        }
        private Operation CreateSomeOperation()
        {
            var state = GetOperationStateByCode("CREATED");
            var type = GetOperationTypeByCode("O_BOX_MIX");
            var vehicleKind = GetVehicleKindByCode("20_TONS");
            var manager = GetEmployee(100004);
            var duration = GetDuration(type, vehicleKind);
            var partner = GetPartnerById("0003");
            var action = GetWarehouseAction(ActionKind.Outgo, partner, "ВБ00-008867");

            var startDate = DateTime.Now.AddDays(3);
            var finishDate = startDate;            
            finishDate = finishDate.Add(new TimeSpan(0, 0, duration));            

            var operation = new Operation(-1, null, type, startDate, finishDate,
                vehicleKind, "some brand", "some model", "ABCDEF", "trailer number",
                "Ivan Ivanov", "123-456-789",
                state, 1,
                "Ivanov", "Ivan", "Ivanovich",
                manager);

            operation.AddAction(action);
            var actions = operation.GetActions();
            Assert.That(actions, Has.Length.EqualTo(1));
            var partners = operation.GetPartners();
            Assert.That(partners, Has.Length.EqualTo(1));

            var user = "m.burov";
            operation = _storage.AddOperation(operation, user);
            Assert.That(operation.Id, Is.GreaterThan(0));

            foreach (var warehouseAction in operation.GetActions())
                _storage.AddAction(operation, warehouseAction);

            var addedOpereation = _storage.GetOperation(operation.Id);
            Assert.That(operation.ActionsCount, Is.EqualTo(addedOpereation.ActionsCount));

            foreach (var warehouseAction in operation.GetActions())
            {
                var addedWarehouseAction = addedOpereation.GetAction(warehouseAction.Kind, warehouseAction.Id);
                Assert.That(addedWarehouseAction != null, Is.True);
                Assert.That(addedWarehouseAction.Kind, Is.EqualTo(warehouseAction.Kind));
                Assert.That(addedWarehouseAction.Id, Is.EqualTo(warehouseAction.Id));
                Assert.That(addedWarehouseAction.Partner, Is.EqualTo(warehouseAction.Partner));
                Assert.That(addedWarehouseAction.PartnerActionId, Is.EqualTo(warehouseAction.PartnerActionId));
            }

            return operation;
        }
        [Test]
        public void CreateOperation()
        {
            // Сборка тестовой операции.
            CreateSomeOperation();
        }
        [Test]
        public void ChangeOperationsStates()
        {
            // Сборка тестовой операции и проход по состояниям.
            var operation = CreateSomeOperation();

            var user = "m.burov";
            _storage.AddOperation(operation, user);
            operation = _storage.AddOperation(operation, user);
            foreach (var warehouseAction in operation.GetActions())
                _storage.AddAction(operation, warehouseAction);
            Assert.That(operation.Id, Is.GreaterThan(0));

            _storage.SetOperationGate(operation, null, user);
            operation = _storage.GetOperation(operation.Id);
            Assert.That(operation.Gate, Is.Not.EqualTo(null));
            Assert.That(operation.State.Code, Is.EqualTo("CONFIRMED"));

            _storage.StartOperation(operation, user);
            operation = _storage.GetOperation(operation.Id);
            Assert.That(operation.State.Code, Is.EqualTo("STARTED"));
            Assert.That(operation.StartDate, Is.Not.EqualTo(DateTime.MinValue));

            _storage.StartOperationWork(operation, user);
            operation = _storage.GetOperation(operation.Id);
            Assert.That(operation.State.Code, Is.EqualTo("WORK_STARTED"));
            Assert.That(operation.FactWorkStartDate, Is.Not.EqualTo(DateTime.MinValue));

            _storage.FinishOperationWork(operation, user);
            operation = _storage.GetOperation(operation.Id);
            Assert.That(operation.State.Code, Is.EqualTo("WORK_FINISHED"));
            Assert.That(operation.FactWorkFinishDate, Is.Not.EqualTo(DateTime.MinValue));

            _storage.FinishOperation(operation, user);
            operation = _storage.GetOperation(operation.Id);
            Assert.That(operation.State.Code, Is.EqualTo("FINISHED"));
            Assert.That(operation.FinishDate, Is.Not.EqualTo(DateTime.MinValue));
        }
        [Test]
        public void CancelAndDeleteOperation()
        {
            // Сборка тестовой операции и её отмена и удаление.
            var operation = CreateSomeOperation();

            var user = "m.burov";
            _storage.AddOperation(operation, user);
            operation = _storage.AddOperation(operation, user);
            foreach (var warehouseAction in operation.GetActions())
                _storage.AddAction(operation, warehouseAction);
            Assert.That(operation.Id, Is.GreaterThan(0));

            _storage.CancelOperation(operation, user);
            operation = _storage.GetOperation(operation.Id);
            Assert.That(operation.State.Code, Is.EqualTo("CANCELED"));

            _storage.DeleteOperation(operation, user);
        }
        /*
        [Test]
        public void Test1()
        {
            var testConfiguration = new Dictionary<string, string> {
                {"UseAuthentication", "true"},
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfiguration)
                .Build();

            var storageMock = new Mock<IYardStorage>();
            var rightsCheckerMock = new Mock<IRightsChecker>();
            rightsCheckerMock.Setup(rc => rc.CheckRight("USE_YARD", It.IsAny<ClaimsPrincipal>())).Returns(true);
            rightsCheckerMock.Setup(rc => rc.CheckRight("CREATE_OPERATION_BY_PARTNER", It.IsAny<ClaimsPrincipal>())).Returns(true);

            var controller = new YardController(configuration, storageMock.Object, rightsCheckerMock.Object);


            // Act
            var result = controller.CreateOperationByPartner().GetAwaiter().GetResult();

            // Assert
            Assert.That(result, Does.Not.StartWith("Wrong start date"));
        }
        */
    }
}
