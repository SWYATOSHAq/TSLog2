using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using TSLog.Common;
using TSLog.Common.Data;
using TSLog.Reports.BO;

namespace TSLog.Reports.Data
{
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

        public dynamic GetFillingReportData()
        {
            dynamic data = new ExpandoObject();
            using (var command = _connection.CreateCommand())
            {
                var sql = 
                    $"select fr.lvl, fr.area, round(fr.filling, 8) filling\n  " +
                    $"  from {WmsExtensionsSchema}.wmsext_filling_report1#v fr";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var area = Utils.DbValueToString(reader["area"]);
                    var value = Utils.DbValueToDouble(reader["filling"]);
                    switch (area)
                    {
                        case "HBW":
                            data.HBW = value;
                            break;
                        case "LBW":
                            data.LBW = value;
                            break;
                        case "LB1_1_2":
                            data.LB1_1_2 = value;
                            break;
                        case "P01":
                            data.P01 = value;
                            break;
                        case "VKU":
                            data.VKU = value;
                            break;
                        case "LM2":
                            data.LM2 = value;
                            break;
                        case "LB1_3_E":
                            data.LB1_3_E = value;
                            break;
                        case "P02":
                            data.P02 = value;
                            break;
                        case "LF3":
                            data.LF3 = value;
                            break;
                        case "P03":
                            data.P03 = value;
                            break;
                    }
                }
                reader.Close();
            }
            return data;
        }
    }
}