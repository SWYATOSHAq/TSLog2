using System.Dynamic;
using TSLog.Common;
using TSLog.Common.Data;
using TSLog.Reports.BO;

namespace TSLog.Reports.Data
{
    /// <summary>
    /// Храниище данных для подсистемы отчётности
    /// </summary>
    public class ReportsOracleStorage : OracleStorage, IReportsOracleStorage
    {
        private const int DefaultMaxRowsCount = 16;
        public int MaxRowsCount { protected get; set; } = DefaultMaxRowsCount;

        private const string DefaultWmsSchema = "wms";
        private const string DefaultWmsExtensionsSchema = "wms_extensions";
        private const string DefaultStaffSchema = "staff";
        private const string DefaultLegalEntitiesSchema = "legal_entities";
        private const string DefaultPartnersSchema = "partners";
        /// <summary>
        /// Схема для хранения объектов WMS (основных объектов)
        /// </summary>
        public string WmsSchema { get; set; } = DefaultWmsSchema;
        /// <summary>
        /// Схема дляхранения дополнительных объектов WMS
        /// </summary>
        public string WmsExtensionsSchema { get; set; } = DefaultWmsExtensionsSchema;
        /// <summary>
        /// Схема для хранения объектов БД для учёта персонала
        /// </summary>
        public string StaffSchema { get; set; } = DefaultStaffSchema;
        /// <summary>
        /// Схема для хранения объектов БД для учёта юр. лиц
        /// </summary>
        public string LegalEntitiesSchema { get; set; } = DefaultLegalEntitiesSchema;
        /// <summary>
        /// Схема для хранения объектов БД для учёта партнёров (мандантов)
        /// </summary>
        public string PartnersSchema { get; set; } = DefaultPartnersSchema;
        public ReportsOracleStorage(string dataSource) 
            : base(dataSource)
        { }
        public void Connect(string userName, string password)
        {
            Connect(userName, Utils.StringToSecureString(password));
        }
        public void SetOracleCommonSchema(string commonSchema)
        {
            CommonSchema = commonSchema;
        }
        public void SetOracleWmsSchema(string wmsSchema)
        {
            WmsSchema = wmsSchema;
        }
        public void SetOracleWmsExtensionsSchema(string wmsExtensionsSchema)
        {
            WmsExtensionsSchema = wmsExtensionsSchema;
        }
        public void SetOracleStaffSchema(string staffShcema)
        {
            StaffSchema = staffShcema;
        }
        public void SetOracleLegalEntitiesSchema(string legalEntitiesSchema)
        {
            LegalEntitiesSchema = legalEntitiesSchema;
        }
        public void SetOraclePartnersSchema(string partnersSchema)
        {
            PartnersSchema = partnersSchema;
        }
        /// <summary>
        /// Возвращает данные для отчёта о загруженности склада
        /// </summary>
        /// <returns>Массив данных по загруженности склада (один элемент массива соответствует одной области склада)</returns>
        public dynamic[] GetFillingReportData()
        {
            var data = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                var sql = 
                    $"select fr.area, fr.place_type, fr.busy, fr.total\n" +
                    $"  from {WmsExtensionsSchema}.wmsext_filling_report1#v fr";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var area = Utils.DbValueToString(reader["area"]);
                    var placeType = Utils.DbValueToString(reader["place_type"]);
                    var busy = Utils.DbValueToDouble(reader["busy"]);
                    var total = Utils.DbValueToDouble(reader["total"]);                    

                    dynamic element = new ExpandoObject();
                    element.area = area;
                    element.placeType = placeType;
                    element.busy = busy;
                    element.total = total;
                    data.Add(element);
                }
                reader.Close();
            }
            return data.ToArray();
        }
    }
}