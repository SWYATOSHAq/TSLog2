namespace TSLog.Reports.BO
{
    public interface IReportsStorage: IReportsOracleStorage, IReportsPgSqlStorage
    { }
    public interface IReportsOracleStorage
    {
        public dynamic[] GetFillingReportData();
    }
    public interface IReportsPgSqlStorage
    { }
}