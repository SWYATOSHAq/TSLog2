using System;
using System.Collections.Generic;
using System.Data.OracleClient;

namespace TSLog2.BossControl.Synchronizer
{
    internal class Writer
    {
        private readonly OracleConnection _connection = new OracleConnection();
        public void Connect()
        {
            OracleConnectionStringBuilder sb = new OracleConnectionStringBuilder();
            sb.DataSource = "TSKL_PDB";
            sb.UserID = "tskl_s1_pdba";
            sb.Password = "t";
            _connection.ConnectionString = sb.ConnectionString;
            _connection.Open();
        }
        public IDictionary<string, SiteWorker> GetData()
        {
            var data = new Dictionary<string, SiteWorker>(1024 * 1024 * 2);
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
                        if (!data.ContainsKey(siteWorker.Key))
                            data.Add(siteWorker.Key, siteWorker);
                        else
                            throw new Exception("Already exists!");
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
            return data;
        }
        public void InsertSiteWorker(SiteWorker siteWorker)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO ECCO.STF_SITE_WORKER (ID,DATE_TIME,KIND,WORKER_ID,SITE_ID,PARTNER_ID,NOTE) " + 
                    "VALUES (:id, :date_time, :kind, :worker_id, :site_id, :partner_id, :note)";

                command.Parameters.Add("id", OracleType.Number).Value = siteWorker.Id;
                command.Parameters.Add("date_time", OracleType.DateTime).Value = siteWorker.DateTime;
                command.Parameters.Add("kind", OracleType.NVarChar).Value = siteWorker.Kind;
                command.Parameters.Add("worker_id", OracleType.Number).Value = siteWorker.WorkerId;
                command.Parameters.Add("site_id", OracleType.Number).Value = siteWorker.SiteId;
                command.Parameters.Add("partner_id", OracleType.VarChar).Value = siteWorker.PartnerId;
                command.Parameters.Add("note", OracleType.NVarChar).Value = siteWorker.Note;

                command.ExecuteNonQuery();
            }
        }
        public void Disconnect()
        {
            _connection.Close();
        }
    }
}
