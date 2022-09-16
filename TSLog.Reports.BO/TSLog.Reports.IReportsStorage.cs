namespace TSLog.Reports.BO
{
    public interface IReportsStorage: IReportsOracleStorage, IReportsPgSqlStorage
    { }
    public interface IReportsOracleStorage
    {
        public dynamic[] GetFillingReportData1();
        public dynamic[] GetFillingReportData2(DateTime fromDate, DateTime toDate);
    }
    public interface IReportsPgSqlStorage
    { }
}