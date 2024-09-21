using System;
using System.Collections.Generic;
using System.Data.OracleClient;

namespace TSLog2.BossControl.Synchronizer
{
    internal class Reader
    {
        private readonly OracleConnection _connection = new OracleConnection();
        public void Connect()
        {
            OracleConnectionStringBuilder sb = new OracleConnectionStringBuilder();
            sb.DataSource = "ECCOPDB";
            sb.UserID = "Ecco2_s1_pdba";
            sb.Password = "e";
            _connection.ConnectionString = sb.ConnectionString;
            _connection.Open();
        }

        public SiteWorker[] GetData()
        {
            var data = new List<SiteWorker>(1024 * 1024 * 2);
            using (var command = _connection.CreateCommand())
            {
                //command.CommandText = "select * from ecco.STF_SITE_WORKER sw order by sw.id";
                command.CommandText = 
                    "select * from (select * from ecco.STF_SITE_WORKER sw order by sw.id desc) where rownum < 1024";
                using (var reader = command.ExecuteReader())
                {
                    int complete = 0;
                    int buffSize = 0;
                    int maxBuffSize = 4 * 1024;
                    while (reader.Read()) 
                    {
                        var siteWorker = new SiteWorker
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            DateTime = Convert.ToDateTime(reader["date_time"]),
                            Kind = Convert.ToString(reader["kind"]),
                            WorkerId = Convert.ToInt32(reader["worker_id"]),
                            SiteId = Convert.ToInt32(reader["site_id"]),
                            PartnerId = Convert.ToString(reader["partner_id"]),
                            Note = Convert.ToString(reader["note"])
                        };
                        data.Add(siteWorker);
                        buffSize++;
                        complete++;
                        if (buffSize >= maxBuffSize) 
                        {
                            Console.Clear();
                            Console.WriteLine(string.Format("Read {0}", complete));
                            buffSize -= maxBuffSize;
                        }
                    }
                    reader.Close();
                }
            }
            return data.ToArray();
        }
        public void Disconnect()
        {
            _connection.Close();
        }
    }
}
