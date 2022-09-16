using TSLog.Common;
using TSLog.Reports.BO;

namespace TSLog.Reports.Data
{
    /// <summary>
    /// Хранилище данных для системы отчётности
    /// </summary>
    public class ReportsStorage: IReportsStorage
    {
        private const int DefaultMaxRowsCount = 16;
        /// <summary>
        /// Максимальное количество строк для вывода
        /// </summary>
        public int MaxRowsCount
        {
            set
            {
                _oracleStorage.MaxRowsCount = value > 0 ? value : DefaultMaxRowsCount;
            }
        }
        private readonly ReportsOracleStorage _oracleStorage;
        public ReportsStorage(string oracleDataSource)
        {
            _oracleStorage = new ReportsOracleStorage(oracleDataSource);            
        }
        public void ConnectOracle(string userName, string password)
        {
            _oracleStorage.Connect(userName, Utils.StringToSecureString(password));
        }
        public void SetOracleCommonSchema(string commonSchema)
        {
            _oracleStorage.CommonSchema = commonSchema;
        }
        public void SetOracleWmsSchema(string wmsSchema)
        {
            _oracleStorage.WmsSchema = wmsSchema;
        }
        public void SetOracleWmsExtensionsSchema(string wmsExtensionsSchema)
        {
            _oracleStorage.WmsExtensionsSchema = wmsExtensionsSchema;
        }
        public void SetOracleStaffSchema(string staffShcema)
        {
            _oracleStorage.StaffSchema = staffShcema;
        }
        public void SetOracleLegalEntitiesSchema(string legalEntitiesSchema)
        {
            _oracleStorage.LegalEntitiesSchema = legalEntitiesSchema;
        }
        public void SetOraclePartnersSchema(string partnersSchema)
        {
            _oracleStorage.PartnersSchema = partnersSchema;
        }
        public void DisconnectOracle()
        {
            _oracleStorage.Disconnect();
        }

        public dynamic[] GetFillingReportData1()
        {
            return _oracleStorage.GetFillingReportData1();
        }
        public dynamic[] GetFillingReportData2(DateTime fromDate, DateTime toDate)
        {
            return _oracleStorage.GetFillingReportData2(fromDate, toDate);
        }
    }
}