using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OracleClient;
using System.Diagnostics.Contracts;
using System.Linq;

namespace TSLog2.BossControl.Synchronizer
{
    class Action
    {
        public string Kind { get; set; }
        public string Id { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            
            NpgsqlConnection conn = new NpgsqlConnection();
            NpgsqlConnectionStringBuilder sb = new NpgsqlConnectionStringBuilder();
            sb.Host = "10.50.254.14";
            sb.Database = "tslog";
            sb.Username = "yarddispatcher";
            sb.Password = "sdfdsSFS38&%^(#__Sdhgsdgy_sDDG22";
            conn.ConnectionString = sb.ConnectionString;
            conn.Open();

            var actions = new List<Action>();
            using (var command = conn.CreateCommand())
            {
                command.CommandText = "select * from yard.v_operation_action oa";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) 
                    {
                        string kind = Convert.ToString(reader["action_kind"]);
                        string id = Convert.ToString(reader["action_id"]);
                        var action = new Action();
                        action.Kind = kind;
                        action.Id = id;
                        actions.Add(action);
                    }
                    reader.Close();
                }
            }

            conn.Close();

            OracleConnection oraConn = new OracleConnection();
            OracleConnectionStringBuilder sb2 = new OracleConnectionStringBuilder();
            sb2.DataSource = "TSKL_PDB";
            sb2.UserID = "tskl_s1_pdba";
            sb2.Password = "t";
            oraConn.ConnectionString = sb2.ConnectionString;
            oraConn.Open();

            var listI = new List<string>();
            var listO = new List<string>();
            foreach (var action in actions)
            {
                if (action.Kind == "I")
                {
                    using (var command = oraConn.CreateCommand())
                    {
                        command.CommandText =
                            "select count(*) from TSKL_S1_PDBA.wea_t o where o.WEAID = '" + action.Id + "'";
                        int count  = Convert.ToInt32(command.ExecuteScalar());
                        if (count <= 0)
                        {
                            listI.Add(action.Id);
                        }
                    }
                }
                else if (action.Kind == "O")
                {
                    using (var command = oraConn.CreateCommand())
                    {
                        command.CommandText =
                            "select count(*) from TSKL_S1_PDBA.AUF_T o where o.AUFID = '" + action.Id + "'";
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        if (count <= 0)
                        {
                            listO.Add(action.Id);
                        }
                    }
                }
            }

            oraConn.Close();

            /*
            Writer writer = new Writer();
            Reader reader = new Reader();
            reader.Connect();
            writer.Connect();

            var old_data = reader.GetData();
            var new_data = writer.GetData();
            foreach (var siteWorker in old_data)
            {
                if (!new_data.ContainsKey(siteWorker.Key))
                {
                    writer.InsertSiteWorker(siteWorker);
                }
            }

            reader.Disconnect();
            writer.Disconnect();
            */
        }
    }
}
