using Oracle.ManagedDataAccess.Client;
using System.Text;

namespace TSLog.Links.KotonExport;
public class Worker
{
    private const string DataSource = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.255.106)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = eccopdb)))";
    public const string UserId = "ecco";
    public const string Password = "e";

    public string Sql =
        "   select auf.aufwwsbeleg partner_order_id, \r\n" +
        "          art.artean product_id, \r\n" +
        "          aup.aupmgist fact_quantity, \r\n" +
        "          auf.dzupd close_date  \r\n" +
        "     from ecco2_s1_pdba.auf_t auf\r\n" +
        "     join ecco2_s1_pdba.aup_t aup on auf.aufid = aup.aufid\r\n" +
        "left join ecco2_s1_pdba.art_t art on art.artid = aup.artid\r\n" +
        "    where auf.auanam in ('OZON', 'NORMAL')    \r\n" +
        "      and substr(auf.aufwwsbeleg, 1, 3) <> 'HM-'\r\n" +
        "      and substr(auf.aufwwsbeleg, 1, 3) <> 'RT0'\r\n" +
        "      and substr(auf.aufwwsbeleg, 1, 3) <> 'AE_'\r\n" +
        "      and auf.mndnr = '0002'\r\n" +
       // "      and rownum < 100\r\n" +
        " order by partner_order_id";
    public const string FilePath = "C:\\data.csv";

    public const int MaxBlockSize = 256;

    public void Export()
    {
        using var connection = new OracleConnection();
        var stringBuilder = new OracleConnectionStringBuilder
        {
            DataSource = DataSource,
            UserID = UserId,
            Password = Password
        };
        connection.ConnectionString = stringBuilder.ConnectionString;
        connection.Open();

        using (var writer = new StreamWriter(FilePath, false, Encoding.UTF8))
        {
            var line = $"\"Id заказа\";\"Id товара\";\"Количество\";\"Дата закрытия\"";
            writer.WriteLine(line);

            var command = connection.CreateCommand();
            command.CommandText = Sql;
            using (var reader = command.ExecuteReader())
            {
                var blockSize = 0;
                var complete = 0;
                while (reader.Read())
                {
                    var partnerOrderId = reader["partner_order_id"] != 
                        DBNull.Value ? Convert.ToString(reader["partner_order_id"]) : string.Empty;
                    var productId = reader["product_id"] != 
                        DBNull.Value ? Convert.ToString(reader["product_id"]) : string.Empty;
                    var factQuantity = reader["fact_quantity"] != 
                        DBNull.Value ? Convert.ToInt64(reader["fact_quantity"]) : 0;
                    var closeDate = reader["close_date"] !=
                        DBNull.Value ? Convert.ToDateTime(reader["close_date"]) : DateTime.MinValue;
                    var stringDate = closeDate != 
                        DateTime.MinValue ? closeDate.ToString("dd.MM.yyyy") : string.Empty;

                    line = $"\"{partnerOrderId}\";\"{productId}\";\"{factQuantity}\";\"{stringDate}\"";
                    writer.WriteLine(line);
                    blockSize++;
                    complete++;

                    if (blockSize >= MaxBlockSize)
                    {
                        Console.WriteLine($"Выгружено {complete}");
                        blockSize -= MaxBlockSize;
                    }
                }
                reader.Close();
            }
            writer.Close();
        }
        connection.Close();
    }
}
