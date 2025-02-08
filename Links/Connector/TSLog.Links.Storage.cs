using Oracle.ManagedDataAccess.Client;
using TSLog.Common.Data;

namespace TSLog.Links.Connector;

public class Storage(string dataSource) : OracleStorage(dataSource), IStorage
{
    private const string DefaultLinksSchema = "common";
    public string LinksSchema { protected get; set; } = DefaultLinksSchema;

    public Document[] GetDocumentsToSend()
    {
        var sql = $"select * from {LinksSchema}.v_sending_document sd where sd.send_date is null;";
        var documents = new List<Document>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var kind = Convert.ToString(reader["kind"]) ?? string.Empty;
                var id = Convert.ToString(reader["id"]) ?? string.Empty;
                var document = new Document()
                {
                    Kind = kind,
                    Id = id
                };
                documents.Add(document);

            }
        }
        return [.. documents];
    }
    public void MarkSendDocument(Document document)
    {
        var sql = $"insert into {LinksSchema}.sending_document (kind, id) values (:kind, :id)";
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("kind", OracleDbType.NVarchar2).Value = document.Kind;
        command.Parameters.Add("id", OracleDbType.NVarchar2).Value = document.Id;
        command.ExecuteScalar();
    }
}
